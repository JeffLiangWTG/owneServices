//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MessageManagers;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Environment;
	using Enterprise.Messaging.MessageBuilders;
	using Enterprise.Security;

	public class SupplementaryCargoReportMessageManager : CAResetToOriginalMessageManager
	{
		public SupplementaryCargoReportMessageManager(ISupplementaryCargoReport supplementaryCargoReport, IUserNotification notification)
			: base(supplementaryCargoReport, new SupplementaryCargoReportStatusCalculator(), notification)
		{
		}

		#region ResetToOriginal

		protected override void ResetToOriginalCore()
		{
			DataWrapper.JobStatus = ZString.Empty;
			DataWrapper.MessageStatus = ZString.Empty;
			DataWrapper.DocumentMessageNumber = ZString.Empty;
			DataWrapper.SupplementaryReferenceNumber = ZString.Empty;
			foreach (EDIMessage message in DataWrapper.Messages)
			{
				message.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Discarded;
			}
		}

		#endregion

		#region Refresh Details

		public void RefreshDetails()
		{
			var cusSCAHouse = DataWrapper.TopLevelBusinessObject as CusSCAHouse;
			if (cusSCAHouse != null && cusSCAHouse.Shipment != null)
			{
				cusSCAHouse.ShipmentSynchroniser.Synchronise(true);
			}
		}

		#endregion

		#region Overrides

		public override string MessageFriendlyName
		{
			get { return Res.GetString("fd9e2f93-5c7c-4235-b824-990de5ba42c7", "ACI Supplementary Cargo Report for {0}", DataWrapper.DocumentMessageNumber); }
		}

		#region GetNotificationsForSending

		public override MessageSendingNotificationCollection GetNotificationsForSendingAnOriginal()
		{
			var result = base.GetNotificationsForSendingAnOriginal();
			ZString errorText;
			if (!CanSendThisMessage(MessageSubTypes.Create, out errorText))
			{
				result.AddWarning(Res.GetString("8b3998ad-31b4-4271-b675-df89864ca3bf", "As {0}", errorText));
			}
			return result;
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAWithdrawal()
		{
			var result = base.GetNotificationsForSendingAWithdrawal();
			ZString errorText;
			if (!CanSendThisMessage(MessageSubTypes.Withdraw, out errorText))
			{
				result.AddWarning(Res.GetString("f8a7c7a6-fdea-463d-990b-787cca53995c", "As {0}", errorText));
			}
			return result;
		}

		#endregion

		#region CanSendThisMessage

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			_ = base.CanSendThisMessage(actionCode, out messageText);
			if (messageText.IsEmpty)
			{
				if (DataWrapper.DocumentMessageNumber.IsEmpty)
				{
					messageText = Res.GetString("1ffdc479-7e05-4728-a32c-7a06ae77b0bd", "ACI details not initialized, please close shipment, reopen shipment and open ACI tab.");
				}
				else if (new List<ISCRLine>(DataWrapper.GoodsLines).Count == 0)
				{
					messageText = Res.GetString("76a53106-992e-4fbd-b73f-569ab2384969", "there are no valid Packing Lines entered for ACI job, please open ACI tab on shipment and check the packing lines.");
				}
				else if (actionCode == MessageSubTypes.Withdraw && !CanSendWithdrawal)
				{
					messageText = Res.GetString("e94c1377-dfa2-4bb5-9a3c-bc030f8dfe32", "the ACI Supplementary Cargo has not been reported yet.");
				}
			}
			return messageText.IsEmpty;
		}

		#endregion

		#region PopulateMessage

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			var actionCode = MessageSubTypes.Undefined;
			DefineActionCodeIfUndefined(ref actionCode);
			return PopulateMessage(actionCode);
		}

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new SupplementaryCargoReportMessageBuilder(DataWrapper, actionCode);
		}

		#endregion

		new ISupplementaryCargoReport DataWrapper
		{
			get { return (ISupplementaryCargoReport)base.DataWrapper; }
		}

		protected override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ConsolCAeManifestSendWithMessageErrors; }
		}

		protected override SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint
		{
			get { return Env.Security.ConsolCAeManifestSendWithMessageErrors; }
		}

		protected override SecurityCheckpoint ResetToOriginalSecurityCheckpoint
		{
			get
			{
				return Env.Security.ConsolCAeManifestResetToOriginal;
			}
		}

		#endregion
	}
}
