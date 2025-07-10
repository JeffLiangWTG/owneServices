using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC531C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC531Provider
	{
		public CC531Provider(Cc531C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc531C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDate SupplementaryDeclarationLodgementStart => new ZDate(xmlObject.TimerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationStartDate);

		public ZDate SupplementaryDeclarationLodgementEnd => new ZDate(xmlObject.TimerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationExpiryDate);

		public ZString TimerExpiryInformation => xmlObject.TimerExpiryForSupplementaryDeclaration?.TimerExpiryInformation;
	}
}
