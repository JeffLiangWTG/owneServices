using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC060CMessageInterpreter : BaseMessageInterpreter<ICC060CDataProvider>
	{
		public override string Interpret(ICC060CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			var notificationType = dataProvider.NotificationType;
			var type = string.Empty;
			var typeOfControlTypes = new NCTS5TypeOfControlTypes();
			note.Append((NoResString)"New Customs Status: Decision to Control Notification");
			note.Append($"Status granted on {NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(dataProvider.ControlNotificationDateAndTimeUtc)}");
			note.Append($"Type of Notification: {notificationType} {new NCTS5NotificationTypes().GetDescriptionFromCode(notificationType)}");
			note.Append("");
			foreach (var typeOfControl in dataProvider.TypeOfControls)
			{
				type = typeOfControl.Type;
				note.Append($"Type of Control {typeOfControl.SequenceNumber}: {type} {typeOfControlTypes.GetDescriptionFromCode(type)} {typeOfControl.Text}");
			}
			foreach (var requestedDocument in dataProvider.RequestedDocument)
			{
				note.Append($"Document {requestedDocument.SequenceNumber}: {requestedDocument.DocumentType} {requestedDocument.Description}");
			}
			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
