using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS304Provider
	{
		public TS304Provider(Ts304 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts304 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZDateTime AmendmentAcceptanceDate
		{
			get
			{
				new ZString(xmlObject.Declaration.AmendmentAcceptanceDate).TryParseToDate(out var acceptanceDate);
				return acceptanceDate;
			}
		}

		public ZString Remarks => xmlObject.Declaration.Remarks;
	}
}
