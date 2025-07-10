using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Export;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.CreditControlledDocumentApproval
{
	public class CreditControlledDocumentApprovalDataContextManager : EventDataContextManager<CreditControlledDocumentsApproval>, IDataContextManagerFromEDIMessage
	{
		public override DataContextType DataContextType => DataContextType.CreditControlledApprovalRequest;

		public override ZString DataContextKey => ParentBO.XP_RequestID;

		public override string DefaultOutputDirectory => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "need to retrieve event user from event information, XML schema is not translatable")]
		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var creditControlledDocumentApprovalRequest = businessObject as CreditControlledDocumentsApproval;
			var eventInfo = ((UniversalDataBuss.DataObjects.Universal.Event)eventDataObject);
			if (creditControlledDocumentApprovalRequest != null && eventInfo != null)
			{
				var eventType = string.Empty;
				var approvalStatus = string.Empty;
				var userCode = string.Empty;
				var rejectionReason = string.Empty;

				if (eventInfo.EventType.HasValue)
				{
					eventType = eventInfo.EventType.ToString();
				}

				var creditApprovalResponseEvent = Events.CreditApprovalResponse;

				if (!string.IsNullOrEmpty(eventType) && eventType == creditApprovalResponseEvent.Code)
				{
					approvalStatus = eventInfo.EventParameters.Type;

					if (!string.IsNullOrEmpty(approvalStatus))
					{
						if (approvalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Approved || approvalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Rejected)
						{
							var userCodeAndName = eventInfo.DataContext.ContextKeyValuePairs.FirstOrDefault(x => x.Key == "Data Source Trigger Event User");
							if (userCodeAndName != null && !userCodeAndName.Value.IsEmpty)
							{
								userCode = userCodeAndName.Value.Split('-')[0].Trim();
							}
							var eventTime = eventInfo.EventTime ?? ZDateTimeOffset.Today;

							creditControlledDocumentApprovalRequest.XP_ApprovalStatus = approvalStatus;
							creditControlledDocumentApprovalRequest.XP_GS_NKApprovingUser1 = userCode.Substring(0, userCode.Length > 3 ? 3 : userCode.Length);
							creditControlledDocumentApprovalRequest.XP_ApprovalDate = eventTime.ToZDateTime();

							var logReference = CreditControlledDocumentsApproval.AddTypeToEventReference(approvalStatus);
							logReference += CreditControlledDocumentsApproval.AppendReferenceToEventReference(creditControlledDocumentApprovalRequest.XP_RequestID);

							if (approvalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Rejected)
							{
								if (eventInfo.EventParameters.Reason.HasValue)
								{
									rejectionReason = eventInfo.EventParameters.Reason;
									logReference += CreditControlledDocumentsApproval.AppendReasonToEventReference(rejectionReason);

									creditControlledDocumentApprovalRequest.RejectionReason = rejectionReason;
								}
							}

							creditControlledDocumentApprovalRequest.Job?.Logs.AddNew(Events.CreditApprovalResponse, logReference);
						}
						else
						{
							logger.Log(LogType.Warning, "Invalid <Type> in <EventParameters>. Accepted types are 'APP' or 'REJ'");
						}
					}
					else
					{
						logger.Log(LogType.Error, "<Type> is missing in <EventParameters>");
					}
				}
			}
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new CreditControlledDocumentApprovalParentFinder(factory, this, logger);
		}
	}

	public class CreditControlledDocumentApprovalParentFinder : EventParentFinder
	{
		const string JobContextType = AccountingDataTransferConstants.DataContextTypeString.Job; // Context key is not translatable
		public CreditControlledDocumentApprovalParentFinder(BusinessObjectFactory factory, CreditControlledDocumentApprovalDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalDataBuss.DataObjects.Universal.Event xmlEvent)
		{
			CreditControlledDocumentsApproval[] results = null;
			if (xmlEvent.DataContext != null && xmlEvent.DataContext.DataTargetCollection != null && xmlEvent.DataContext.DataTargetCollection.Any())
			{
				var requestReferenceID = xmlEvent.DataContext.DataTargetCollection.FirstOrDefault(x => x.Type.HasValue && x.Type.Value == nameof(DataContextType.CreditControlledApprovalRequest));
				if (requestReferenceID != null && requestReferenceID.Key.HasValue)
				{
					GlbCompany company = null;
					var companyCode = xmlEvent.DataContext.CompanyCodeToImportInto;
					if (!companyCode.IsEmpty)
					{
						company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
					}

					if (company != null)
					{
						if (xmlEvent.ContextCollection != null)
						{
							var jobContext = xmlEvent.ContextCollection.FirstOrDefault(x => x.Type == JobContextType);
							if (jobContext != null)
							{
								if (jobContext.Value.HasValue)
								{
									var jobNumber = jobContext.Value;
									var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GenApprovalRequestSchema.XP_GB_RequestingBranch);
									branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, company.PK);

									var approvalRequestQuery = new ZDBOnlyQuery(typeof(GenApprovalRequest));
									approvalRequestQuery.AddToFilter(GenApprovalRequestSchema.XP_RequestID, requestReferenceID.Key.Value);
									approvalRequestQuery.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
									approvalRequestQuery.AddSubQuery(branchSubQuery, JoinCondition.And);

									results = factory.Load<CreditControlledDocumentsApproval>(approvalRequestQuery).Where(x => x.JobNumber == jobNumber.Value).ToArray();
								}
								else
								{
									logger.Log(LogType.Error, "Context: Value is missing for Type=Job in ContextCollection"); // Log might not be translatable
								}
							}
							else
							{
								logger.Log(LogType.Error, "Context: Type=Job is missing in ContextCollection"); // Log might not be translatable
							}
						}
						else
						{
							logger.Log(LogType.Error, "ContextCollection is missing in DataContext"); // Log might not be translatable
						}
					}
					else
					{
						logger.Log(LogType.Error, "Invalid company code."); // Log might not be translatable
					}
				}
			}
			return results;
		}
	}
}
