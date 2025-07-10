using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class B3XMessageManager : CAMessageManager
	{
		public B3XMessageManager(IB3Header b3Header, IUserNotification notification)
			: base(b3Header, new B3ImportStatusCalculator(), notification)
		{
		}

		protected override ValidateForMessageType GetValidationType()
		{
			return ValidateForMessageType.B3CUSDEC;
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			var cansend = base.CanSendThisMessage(actionCode, out messageText);
			if (cansend)
			{
				var b3XDeclaration = DataWrapper?.TopLevelBusinessObject as JobDeclaration;
				if (b3XDeclaration != null && b3XDeclaration.IsB3X && b3XDeclaration.CA_B2AcceptedDate.IsValid)
				{
					var caption = Res.GetString("07E63C67-2AB5-4680-B944-52AB2E57F568", "Send B3X Message?");
					var message = Res.GetString("8A148934-3E3B-4DE4-BA0C-EB1DC72A3BB5",
						@"This B3X has been already submitted for Review – hence no changes are allowed to the data that is printed on the B3X.
If the current changes are of the administrative nature, something that would NOT cause the reprint of the B3X to produce different results, please proceed with sending, otherwise please do not send this message.");
					cansend = notification.ShowConfirmation(message, caption);
				}
			}
			return cansend;
		}

		protected new IB3Header DataWrapper
		{
			get { return (IB3Header)base.DataWrapper; }
		}

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			var message = Res.GetString("2E02DA01-479E-48A2-8792-17E92BA54E3C", "{0} {1} has been generated.", GetActionCodeDescription(actionCodeToSend), MessageFriendlyName);
			var caption = Res.GetString("9840130A-CF42-44DF-B7D5-3C6507BEB82E", "Message generated");
			notification.ShowInformation(message, caption);
		}

		protected override string GetActionCodeDescription(MessageSubTypes actionCodeToSend)
		{
			var result = base.GetActionCodeDescription(actionCodeToSend);
			switch (actionCodeToSend)
			{
				case MessageSubTypes.Create:
					result = ZString.Format("Original");
					break;
				default:
					break;
			}
			return result;
		}

		public override string MessageFriendlyName
		{
			get { return ResString.GetMultilingualString("4B3EEF06-469B-410F-9A38-EF8E011A8B87", "X Type Entry Message for {0}", DataWrapper.TopLevelBusinessObject.HumanReadableName); }
		}

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new B3CusdecMessageBuilder<B3XMessage>(DataWrapper, actionCode);
		}

		protected override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAB3MsgSend; }
		}

		protected override void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				declaration.CA_B2SubmissionDate = ZDateTime.Now;
			}
		}

		protected override ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode) => ZString.Empty;
	}
}
