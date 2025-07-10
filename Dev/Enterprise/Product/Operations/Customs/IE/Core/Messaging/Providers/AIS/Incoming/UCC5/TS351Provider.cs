using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS351Provider
	{
		public TS351Provider(Ts351 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts351 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString LRN => xmlObject.Declaration.Lrn25;

		public ZDateTime ControlDate
		{
			get
			{
				new ZString(xmlObject.Declaration.ControlResult?.ControlDate).TryParseToDate(out var controlDate);
				return controlDate;
			}
		}

		public ZString ControlResultRemarks => xmlObject.Declaration.ControlResult?.Remarks;

		public ZString Remarks => xmlObject.Declaration.Remarks;
	}
}
