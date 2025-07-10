using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Messaging;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ComplianceRisk.ServiceTasks;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Integration.Forwarding;
using static Enterprise.Messaging.Integration.EDIMessageStatusList;

[assembly: HostedService(
	ComplianceRiskAssessmentServiceTask.Code,
	ComplianceRiskAssessmentServiceTask.Description,
	ComplianceRiskAssessmentServiceTask.Category,
	typeof(ComplianceRiskAssessmentServiceTask),
	IsMandatory = false,
	ActiveByDefault = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "10minutes")]

namespace Enterprise.ComplianceRisk.ServiceTasks;

public class ComplianceRiskAssessmentServiceTask : ServiceProviderImpl
{
	internal const string Category = "CPW";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	internal const string Description = "Compliance Risk Assessment Service Task";
	internal const string Code = "CRA";
	readonly BusinessObjectFactoryProvider objectFactoryProvider;
	readonly BorderWiseApiHelper borderWiseApiHelper;
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	const string ErrorKey = "Error occurred when Processing NCH Commodities in CRA service task";

	public ComplianceRiskAssessmentServiceTask()
	{
		objectFactoryProvider = new BusinessObjectFactoryProvider();
		borderWiseApiHelper = new BorderWiseApiHelper();
	}

	public override void RunTask(CancellationToken youMustReactToThisToken)
	{
		using (Environment.DisposableEnvironment.ForBranch(GlbBranch.GetFirstActiveBranch().PK.ToGuid()))
		{
			var complianceMessages = Array.Empty<ComplianceRiskAssessmentEdiMessage>();
			try
			{
				while (true)
				{
					youMustReactToThisToken.ThrowIfCancellationRequested();

					complianceMessages = GetMaterialChangeQueuedMessages();

					if (complianceMessages.Length > 0)
					{
						var complianceHostJobs = new Dictionary<ZString, (BusinessObject hostBusiness, ComplianceRiskStatus complianceRiskStatus, ComplianceCommodityDetail[] commodities)>(BatchSize);
						var complianceBWRequests = new List<ComplianceCheckRequestModel>(BatchSize);
						complianceMessages.ForEach(message => PrepareCommoditiesBWRequests(complianceHostJobs, complianceBWRequests, message));

						if (complianceBWRequests.Count > 0)
						{
							PostBWRequestWithResponseToUpdateCommoditiesRiskStatus(complianceHostJobs, borderWiseApiHelper.ComplianceCheckAsync(complianceBWRequests, youMustReactToThisToken).Result);
							RunSynchronizeComplianceRiskStatus(complianceHostJobs);
						}

						UpdateComplianceEdiMessageStatusToProcessed(complianceMessages);

						objectFactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
					}
					else
					{
						break;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is OperationCanceledException))
			{
				if (!(ex is ZSaveConcurrencyException
					|| ex is ZCannotSaveException
					|| ex is ZSaveException))
				{
					ErrorReporter.ReportOnce(ErrorKey, ex);
				}

				ServiceLogger?.Log(LogType.Error, ErrorKey, ex);

				UpdateComplianceEdiMessageStatusToError(complianceMessages);
			}
		}
	}

	int BatchSize => OrganisationsDataRegistry.Instance.ComplianceRiskAssessmentBatchSize.Value;

	/// <summary>
	/// Process compliance edi message and prepare for commodities border wise request moel
	/// </summary>
	void PrepareCommoditiesBWRequests(Dictionary<ZString, (BusinessObject hostBusiness, ComplianceRiskStatus complianceRiskStatus, ComplianceCommodityDetail[] commodities)> complianceHostJobs, List<ComplianceCheckRequestModel> complianceBWRequests, ComplianceRiskAssessmentEdiMessage complianceMessage)
	{
		var hostBusinessEntity = GetHostLinkedObject(complianceMessage);
		if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider
			&& hostBusinessEntity.IsCommodityRiskAssessableForServiceTask())
		{
			var assessmentPointPairInfo = provider.AssessmentPointPairInfo;
			if (assessmentPointPairInfo.SupportAssessmentByBorderWise)
			{
				var complianceRiskStatus = provider.Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, provider.ParentID));
				if (complianceRiskStatus != null)
				{
					var commodities = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>())
						.Where(c => c.CCD_RiskStatus == (ZString)ComplianceRiskStatusCodeList.Codes.NotChecked)
						.ToArray();
					if (commodities.Length > 0)
					{
						var request = ComplianceCheckRequestModelBuilder.GetRequestModel(hostBusinessEntity, provider.AssessmentPointPairInfo.PointPairs, commodities);
						if (ComplianceCheckRequestModelBuilder.RequestIsValid(request)
							&& !complianceHostJobs.ContainsKey(request.JobNumber))
						{
							complianceHostJobs.Add(request.JobNumber, (hostBusinessEntity, complianceRiskStatus, commodities));
							complianceBWRequests.Add(request);
							return;
						}
					}
				}
			}
		}

		complianceMessage.EM_Status = Codes.Discarded;
	}

	/// <summary>
	/// Update EDI message status to Process required commodities risk status based on border-wise compliance check response or configurable business rules.
	/// </summary>
	void UpdateComplianceEdiMessageStatusToProcessed(ComplianceRiskAssessmentEdiMessage[] complianceMessages)
	{
		complianceMessages.ForEach(message =>
		{
			if (message.EM_Status == Codes.Queued)
			{
				message.EM_Status = Codes.ProcessedOK;
			}
		});
	}

	/// <summary>
	/// Update EDI message status to Cancelled when duplicate message occurred via UXML or UI
	/// </summary>
	void UpdateDuplicateComplianceEdiMessageStatusToCancelled()
	{
		Db.Connection.ExecuteNonQuery(@"
WITH CTE AS
(
	SELECT
		EM_PK,
		ROW_NUMBER() OVER (PARTITION BY EM_LinkUniqueID ORDER BY EM_SystemCreateTimeUtc DESC) AS RowNum
	FROM
		dbo.EDIMessage WITH (INDEX(NR_RX__EM_MessageSubType_EM_Status))
	WHERE
		EM_ApplicationCode = 'CPW'
		AND EM_MessageType = 'MCH'
		AND EM_MessageSubType = 'CRA'
		AND EM_Status = 'QUE'
)
UPDATE CPWMessage
SET
	CPWMessage.EM_Status = 'CAN',
	CPWMessage.EM_SystemLastEditUser = '~BP',
	CPWMessage.EM_SystemLastEditTimeUtc = GETUTCDATE()
FROM
	dbo.EDIMessage CPWMessage
JOIN
	CTE ON CPWMessage.EM_PK = CTE.EM_PK
WHERE
	CTE.RowNum != 1;");
	}

	/// <summary>
	/// Update EDI message status to Error when the exception occurred
	/// </summary>
	void UpdateComplianceEdiMessageStatusToError(ComplianceRiskAssessmentEdiMessage[] messages)
	{
		objectFactoryProvider.CreateNewWithoutSave();
		var errorMessages = objectFactoryProvider.Current.Load<ComplianceRiskAssessmentEdiMessage>(new ZQuery(EDIMessageSchema.PK, messages.Select(u => u.PK)));
		errorMessages.ForEach(message => message.EM_Status = EDIMessageStatusList.Codes.Error);
		objectFactoryProvider.Current.Save();
	}

	/// <summary>
	/// Updates the required commodities risk status based on border-wise compliance check response or configurable business rules.
	/// </summary>
	void PostBWRequestWithResponseToUpdateCommoditiesRiskStatus(Dictionary<ZString, (BusinessObject hostBusiness, ComplianceRiskStatus complianceRiskStatus, ComplianceCommodityDetail[] commodities)> complianceJobs, ICollection<ComplianceCheckResponseModel> responses)
	{
		responses.ForEach(response =>
		{
			if (complianceJobs.TryGetValue(response.JobNumber, out var complianceJob))
			{
				var alertHelper = new RefComplianceCommodityAlertHelper(complianceJob.hostBusiness.Factory, response, complianceJob.hostBusiness is IComplianceCommodityRiskStatusProvider provider ? provider.RiskCalculateFactor : CommodityRiskCalculateFactor.All);
				complianceJob.commodities.ForEach(commodity => ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodity, response, alertHelper));
			}
		});
	}

	/// <summary>
	/// Synchronize compliance risk factor statuses to align any changes that could occur via UXML or UI.
	/// </summary>
	void RunSynchronizeComplianceRiskStatus(Dictionary<ZString, (BusinessObject hostBusinessEntity, ComplianceRiskStatus complianceRiskStatus, ComplianceCommodityDetail[] commodities)> complianceJobs)
	{
		complianceJobs.ForEach(compliance =>
		{
			var (hostBusinessEntity, complianceRiskStatus, commodities) = compliance.Value;
			ComplianceRiskStatusSynchronizer.Synchronize(new ComplianceRiskBusinessObject(hostBusinessEntity, complianceRiskStatus), shouldRunAdditionalComplianceProcess: false);
		});
	}

	ComplianceRiskAssessmentEdiMessage[] GetMaterialChangeQueuedMessages()
	{
		UpdateDuplicateComplianceEdiMessageStatusToCancelled();

		var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CPWRequestMessage)
			.AddToFilter(EDIMessageSchema.EM_MessageType, ComplianceRiskAssessmentMessagePublisher.MaterialChange)
			.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ComplianceRiskAssessment)
			.AddToFilter(EDIMessageSchema.EM_Status, Codes.Queued);
		query.TableIndexHints.Add(new TableIndexHint("NR_RX__EM_MessageSubType_EM_Status"));
		query.MaximumRows = BatchSize;
		return objectFactoryProvider.Current.Load<ComplianceRiskAssessmentEdiMessage>(query);
	}

	BusinessObject GetHostLinkedObject(ComplianceRiskAssessmentEdiMessage complianceMessage)
	{
		var hostLinkedObject = complianceMessage.EM_LinkedObject;
		if (hostLinkedObject is IViewComplianceRiskStatusProvider viewComplianceRiskStatusProvider)
		{
			var hostLinkedObjectProvider = viewComplianceRiskStatusProvider.GetProviderBusinessObject();
			if (hostLinkedObjectProvider is IQuotedBooking quotedBizO)
			{
				if (quotedBizO.ForwardingShipment != null
					&& quotedBizO.ForwardingShipment is IForwardingShipment shipment
					&& shipment.JS_IsBooking
					&& !shipment.JS_IsForwardRegistered)
				{
					return quotedBizO as BusinessObject;
				}
				return null; // Ignore Registered Booking
			}
			return hostLinkedObjectProvider as BusinessObject;
		}
		return hostLinkedObject;
	}
}
