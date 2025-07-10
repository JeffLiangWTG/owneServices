using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public static class DEEDIMessageSharedHelpers
	{
		public static string GetDEMessageReferenceNumber(BusinessObjectFactory factory) => Env.NumberFountains.DEMessageControlNumber(GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompany.CurrentCompany.LicenceKeyIdentifier, 14).GetNextFormatted(factory);

		public static void GetInterchangeControlReferenceAndFillInPlaceHolders(this EDIMessage message)
		{
			var interchangeControlReference = Env.NumberFountains.DEInterchangeControlReference.GetNextFormatted(message.Factory);
			message.EM_MessageText = message.EM_MessageText
				.Replace(EDIMessage.SendersReferencePlaceHolderHtml, message.EM_MessageNum)
				.Replace(EDIInterchange.InterchangeNumberPlaceHolderHtml, interchangeControlReference);
		}
	}
}
