using System;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NCTSOutboundEDIMessage : GbEDIMessage
	{
		public NCTSOutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsNCTS);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsNCTS;
		}

		protected override string GetSendersReference()
		{
			var linkedObject = EM_LinkedObject;
			var nctsHeader = (linkedObject as NctsHeader) ?? (linkedObject as NctsDepartureMovementHeader)?.Header;
			return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", nctsHeader?.BH_JobReference ?? string.Empty, EM_MessageNum);
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			if (UsesPlaceHolders)
			{
				var messageText = EM_MessageText;

				if (messageText.IndexOf(SendersReferencePlaceHolderHtml, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(SendersReferencePlaceHolderHtml, GetSendersReference());
					EM_MessageText = messageText;
				}
			}
		}
	}
}
