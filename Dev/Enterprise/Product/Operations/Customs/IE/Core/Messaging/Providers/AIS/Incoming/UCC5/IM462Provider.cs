using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM462;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM462Provider
	{
		public IM462Provider(Im462 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im462 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString CaseId => xmlObject.Declaration.CaseId;

		public ZString AmendReason => xmlObject.Declaration.AmendReason;
	}
}
