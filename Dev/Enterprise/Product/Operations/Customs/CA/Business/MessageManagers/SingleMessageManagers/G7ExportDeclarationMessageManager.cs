using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class G7ExportDeclarationMessageManager : CAMessageManager
	{
		public G7ExportDeclarationMessageManager(IG7Export g7ExportDeclarationWrapper, IUserNotification notification)
			: base(g7ExportDeclarationWrapper, new G7ExportStatusCalculator(), notification)
		{
		}

		#region Overrides of SingleMessageManager

		public override string MessageFriendlyName
		{
			get { return Res.GetString("f914b421-e09b-49c7-afaa-3c78183b8dca", "G7 Export Declaration for {0}", DataWrapper.DocumentMessageNumber); }
		}

		#endregion

		#region Overrides of CAMessageManager

		public override bool CanSendWithdrawal
		{
			get { return StatusCalculator.IsLodged(DataWrapper.JobStatus); }
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			if (base.CanSendThisMessage(actionCode, out messageText))
			{
				var exportLicenseNumber = DataWrapper.ExportLicenceNumber;
				if (exportLicenseNumber.IsEmpty)
				{
					messageText = Res.GetString("c59b48a6-ad50-45fb-99ef-aaec9710c0d0", @"An Export License Number is not set. The Export License Number is entered on the Proxy Organization assigned on the Canadian Company.
(i.e. Maintain -> User Admin -> Companies -> Organization Proxy ->  Detail -> Config -> Export License Number)");
				}
				else if (!exportLicenseNumber.IsLettersAndNumbersOnlyOrEmpty || exportLicenseNumber.Length != ExportLicenseNumberLength)
				{
					messageText = Res.GetString("AD068D85-5BAC-467E-A270-E8244FDDA76B", "The Export License Number (CAX) on Organization Proxy {0} is invalid. Please configure it under Organization {0} > Details > Config > Registration Numbers / Codes.", DataWrapper.ExportLicenceProxy?.OH_Code ?? ZString.Empty);
				}
				else if (actionCode == MessageSubTypes.Withdraw && !CanSendWithdrawal)
				{
					messageText = Res.GetString("acc8d2dc-574b-48ad-adb1-9d93ea354d17", "the Export Declaration has not been reported yet.");
				}
				else
				{
					var declaration = BusinessObject as JobDeclaration;
					IsCreditCheckOKToSend(declaration, out messageText);
				}
			}
			return string.IsNullOrEmpty(messageText);
		}
		public const int ExportLicenseNumberLength = 6;

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new G7ExportMessageBuilder(DataWrapper, actionCode);
		}

		new IG7Export DataWrapper
		{
			get { return (IG7Export)base.DataWrapper; }
		}

		protected override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAG7MsgSend; }
		}

		protected override bool ShouldSetEntrySubmittedDate(MessageSubTypes actionCode)
		{
			return actionCode == MessageSubTypes.Create;
		}

		#endregion
	}
}
