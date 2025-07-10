using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EuPackingSynchroniser : PackingSynchroniser
	{
		public EuPackingSynchroniser(JobDeclarationSynchroniser parentSynchroniser, BaseJobDeclaration declaration)
			: base(parentSynchroniser, declaration)
		{ }

		protected override ZString GetConvertedPackUQ(ZString freightPackType)
		{
			return Converter.GetTwoCharacterUnitType(freightPackType).MappedCode;
		}

		InvoiceLineFromOrderLineSynchroniser converter;
		InvoiceLineFromOrderLineSynchroniser Converter => converter ?? (converter = new InvoiceLineFromOrderLineSynchroniser());
	}
}
