using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS328Provider
	{
		public TS328Provider(Ts328 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts328 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString LRN => xmlObject.Declaration.Lrn25;

		public ZDateTime AcceptanceDate
		{
			get
			{
				new ZString(xmlObject.Declaration.AcceptanceDate).TryParseToDate(out var acceptanceDate);
				return acceptanceDate;
			}
		}

		public ZDateTime ResponseDateLimit
		{
			get
			{
				new ZString(xmlObject.Declaration.ResponseDateLimit).TryParseToDate(out var date);
				return date;
			}
		}

		public ZString Remarks => xmlObject.Declaration.Remarks;
	}
}
