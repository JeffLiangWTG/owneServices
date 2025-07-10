using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM415V;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM415VProvider
	{
		public IM415VProvider(Im415V xmlObject)
		{
			declaration = xmlObject.Declaration;
		}
		readonly DeclarationType declaration;

		public ZString AdditionalDeclarationType => declaration?.AdditionalDeclarationType12;

		public ZString LocalReferenceNumber => declaration?.Lrn25;

		public ZString MovementReferenceNumber => declaration?.Mrn;

		public ZDateTime DeclarationAcknowledgementDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.DeclarationAcknowledgementDate);
	}
}
