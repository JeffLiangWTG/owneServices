using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM431;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM431Provider
	{
		public IM431Provider(Im431 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im431 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime LodgementOfSupplementaryDeclarationStartDate => (xmlObject.TimerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationStartDate).ConvertToZDateTime();

		public ZDateTime LodgementOfSupplementaryDeclarationExpiryDate => (xmlObject.TimerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationExpiryDate).ConvertToZDateTime();

		public ZString TimerExpiryInformation => xmlObject.TimerExpiryForSupplementaryDeclaration?.TimerExpiryInformation;
	}
}
