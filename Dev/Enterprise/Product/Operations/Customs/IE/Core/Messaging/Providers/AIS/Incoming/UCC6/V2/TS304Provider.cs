using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS304;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS304Provider
	{
		public TS304Provider(Ts304 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts304 xmlObject;

		public ZString Remarks => xmlObject.Declaration?.Remarks ?? ZString.Empty;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn ?? ZString.Empty;

		public ZDateTime DateOfAcceptance => (xmlObject.Declaration?.AmendmentAcceptanceDate?.DateOfAcceptance).ConvertToZDateTime();
	}
}
