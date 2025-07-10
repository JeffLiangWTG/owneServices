using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDRMessageProcessor : CMRMessageResponseProcessor
	{
		public EXDRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.EXDR, "Export Declaration Response(EXDR)")
		{
		}

		#region Implementation

		protected override bool DoAdditionalProcessing()
		{
			var eXDRMessage = incomingMessage as CMREXDRMessage;
			var exportDeclarationNumber = eXDRMessage == null ? ZString.Empty : eXDRMessage.ExportDeclarationNumber;
			Logger.DebugLog("EXDR - Additional Processing - EDN: " + exportDeclarationNumber);
			Logger.DebugLog("Status Type: " + statusType);

			var linkedObject = incomingMessage.EM_LinkedObject;
			var declaration = (linkedObject as CusEntryHeader)?.Declaration ?? linkedObject as JobDeclaration;
			if (declaration == null)
			{
				Logger.LogWarning("The incoming message is not responding to a declaration. Can't continue.");
				return false;
			}

			if (!exportDeclarationNumber.IsEmpty)
			{
				if (declaration.Shipment != null)
				{
					DeleteManuallyEnteredCANNumberForShipment(declaration.Shipment);
				}

				declaration.Logs.AddNew(Events.CustomsEntryStatus, exportDeclarationNumber.ToString());
				declaration.DeclarationNumber = exportDeclarationNumber;
			}

			if (statusType.IndexOf("CLEAR") != -1)
			{
				Logger.DebugLog("EXDR: IncomingMessageValid=" + (incomingMessage != null).ToString());
				Logger.DebugLog("EXDR: OutgoingMessageValid=" + (outgoingMessage != null).ToString());
				if (outgoingMessage != null)
				{
					Logger.DebugLog("EXDR: OutgoingMessageSubType=" + outgoingMessage.EM_MessageSubType);
				}

				EXDRMessageProcessorHelper.ClearDeclaration(incomingMessage, outgoingMessage, declaration);
			}
			else if (statusType.IndexOf("ERROR") != -1)
			{
				EXDRMessageProcessorHelper.ErrorDeclaration(incomingMessage, outgoingMessage, declaration);
			}
			else if (statusType.IndexOf("REJECTED") != -1)
			{
				EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			}
			else if (statusType.IndexOf("REVOKED") != -1)
			{
				EXDRMessageProcessorHelper.RevokeDeclaration(incomingMessage, outgoingMessage, declaration);
			}
			else if (statusType.IndexOf("WITHDRAWN") != -1)
			{
				incomingMessage.EM_MessageSubType = nameof(Core.Constants.EXDRMessageSubType.WDW);
				declaration.JE_EntryStatus = CustomsEntryStatus.ClearWithdrawal.Code;
				declaration.DeclarationNumber = ZString.Empty;
				EXDRMessageProcessorHelper.UpdateEntryHeaderStatus(declaration, CustomsEntryStatus.ClearWithdrawal.Code);
			}
			else if (statusType.IsEmpty)
			{
				statusType = "NONE-GIVEN(REJECTED)";
				//no status given.  Assume a rejection
				EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			}

			return true;
		}

		void DeleteManuallyEnteredCANNumberForShipment(ForwardingShipment shipment)
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, shipment.PK);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryIsSystemGenerated, SQLComparisonOperator.Equal, ZBool.False);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Australia.CAN);

			var shipmentEntryNumbers = new CusEntryNumCollection(shipment.Factory, filter);
			shipmentEntryNumbers.Load();
			shipmentEntryNumbers.RemoveAndDeleteAll();
		}

		#endregion
	}
}
