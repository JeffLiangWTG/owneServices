using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE460;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE460MessagePrettier : DeltaIEMessagePrettier<CC460BType>
	{
		public IE460MessagePrettier(IE460MessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC460BType messageObject)
		{
			var importOperation = messageObject.ImportOperation;
			var declarationStatus = messageObject.DeclarationStatus;
			var notificationTypeCode = importOperation?.NotificationType ?? ZString.Empty;
			var notificationTypeDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory,
					notificationTypeCode, Core.Constants.CountryCodes.France,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NotificationType, ZDateTime.Today)
				?.ZZD_Description ?? ZString.Empty;
			var result =  ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", declarationStatus?.State ?? ZString.Empty),
				("Status Date", declarationStatus?.StateDateTime ?? ZString.Empty),
				("LRN", importOperation?.LRN ?? ZString.Empty),
				("MRN", importOperation?.MRN ?? ZString.Empty),
				("Notification Date", importOperation?.NotificationDate ?? ZString.Empty),
				("Notification Type", notificationTypeDescription.IsEmpty ? ZString.Empty : (ZString)(notificationTypeCode + " - " + notificationTypeDescription)),
				("Anticipated Control Date", importOperation?.AnticipatedControlDate ?? ZString.Empty),
				("Text", importOperation?.Text ?? ZString.Empty)
			});

			if (messageObject.TypeOfControls != null)
			{
				result += ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Types Of Control"));
				foreach (var typeOfControl in messageObject.TypeOfControls)
				{
					var controlsTypeCode = typeOfControl?.Type ?? ZString.Empty;
					var controlsTypeDescription = string.IsNullOrEmpty(controlsTypeCode)
						? ZString.Empty
						: (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, controlsTypeCode, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty);
					result += ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("Sequence ", typeOfControl?.SequenceNumber ?? ZString.Empty),
						("Type", controlsTypeCode),
						("Description", controlsTypeDescription),
						("Remarks", typeOfControl?.Remarks ?? ZString.Empty)
					});
				}
			}

			if (messageObject.RequestedDocuments != null)
			{
				result += ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Requested Documents"));
				foreach (var requestedDocument in messageObject.RequestedDocuments)
				{
					result += ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("Sequence", requestedDocument?.SequenceNumber ?? ZString.Empty),
						("Type", requestedDocument?.Type ?? ZString.Empty),
						("Reference Number", requestedDocument?.ReferenceNumber ?? ZString.Empty),
						("Description", requestedDocument?.Description ?? ZString.Empty)
					});
				}
			}

			return result;
		}
	}
}
