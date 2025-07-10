using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC035CMessageInterpreter : BaseMessageInterpreter<ICC035CDataProvider>
	{
		public override string Interpret(ICC035CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			note.Append((NoResString)"Recovery notification for NCTS departure received at " + dataProvider.NotificationDate.ToString(BE.Business.Constants.DateTimeFormats.LongTimeIncludingSecondsFormat));
			note.Append(dataProvider.NotificationText);
			note.Append((NoResString)"Amount recovered: " + dataProvider.AmountClaimed + " " + dataProvider.Currency);

			if (dataProvider.Guarantor is GuarantorXmlProvider guarantor)
			{
				note.Append((NoResString)"The guarantor for this declaration is " + guarantor.Id + " " + guarantor.Name);

				if (guarantor.Address is AddressXmlProvider address)
				{
					note.Append(address.StreetAndNumber);
					note.Append(address.Postcode);
					note.Append(address.City);
					note.Append(address.Country);
				}
			}

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
