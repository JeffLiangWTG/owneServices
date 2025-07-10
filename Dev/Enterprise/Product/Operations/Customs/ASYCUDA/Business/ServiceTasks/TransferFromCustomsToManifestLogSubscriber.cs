using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[Serializable]
	public class TransferFromCustomsToManifestLogSubscriber : LogSubscriber
	{
		public override string[] EventTypes
		{
			get { return new string[] { Events.TransferFromCustomsToManifestCode }; }
		}

		public override string Name
		{
			get { return "TMCEventLogSubscriber"; }
		}

		public override string FriendlyName
		{
			get { return "Transfer from Customs to Manifest Log Subscriber"; } // Log subscriber names should be in English only
		}

		public override string[] TableNames
		{
			get { return new string[] { JobDeclarationSchema.Constants.TableName }; }
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (IQueuedLog queuedLog in queuedLogs)
			{
				HandleLogEvent(queuedLog);
			}
		}

		void HandleLogEvent(IQueuedLog queuedLog)
		{
			var declaration = queuedLog.Factory.Load<BaseJobDeclaration>(queuedLog.SJ_ParentID);
			if (declaration != null && declaration.IsGlobalManifestIntegrationEnabled)
			{
				new DeclaratonToGlobalManifestPublisher().Publish(declaration, queuedLog.EventTimeOffset);
			}
		}
	}

	class DeclaratonToGlobalManifestPublisher : IDeclaratonToGlobalManifestPublisher
	{
		public PublishToUniversalResult Publish(BaseJobDeclaration declaration, ZDateTimeOffset eventTime)
		{
			PublishToUniversalResult result = null;
			if (!declaration.GlobalManifestReference.IsEmpty)
			{
				IExternalFetchHintSupporter externalFetchHintSupporter = declaration.Factory;
				using (externalFetchHintSupporter.SetupCreator())
				using (declaration.Branch != null ? Environment.DisposableEnvironment.ForBranch(declaration.Branch.PK.ToGuid()) : null)  // DisposableEnvironment will throw if branch PK does not load. We dont want that.
				{
					var actionInfo = new ActionInfo(new[] { new RecipientRoleDetail() { Type = RecipientRoleType.ASY, ServiceCode = ServiceCodeType.AMD } }, declaration)
					{
						ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
						TriggerEventCode = Events.TransferFromCustomsToManifestCode,
						TriggerActualDate = eventTime
					};
					var writer = GetWriter(declaration);
					var shipment = writer.GetDataObject(declaration);

					new DataContextDataObjectWriter().PopulateDataObject(actionInfo, declaration, shipment.DataContext);
					shipment.DataContext.AddDataTarget(DataContextType.AsycudaManifest, declaration.GlobalManifestReference);
					result = UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(declaration.Factory, declaration, shipment, EDIMessageSubTypeList.Codes.XmlUniversalShipment).GetResult(DataContextType.AsycudaManifest);
				}
			}
			return result;
		}

		JobDeclarationToAsycudaManifestShipmentWriter GetWriter(BaseJobDeclaration declaration)
		{
			IDataWritingManager writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
			string countryCode = declaration.CountryCode;
			JobDeclarationToAsycudaManifestShipmentWriter result = null;
			var writers = ObjectFactory.Get<Hashtable>("JobDeclarationToAsycudaManifestShipmentWriters");
			var objectHandle = (ObjectHandle)writers[countryCode];
			if (objectHandle != null)
			{
				result = (JobDeclarationToAsycudaManifestShipmentWriter)objectHandle?.GetObject(writingManager);
			}
			return result ?? new JobDeclarationToAsycudaManifestShipmentWriter(writingManager);
		}
	}
}
