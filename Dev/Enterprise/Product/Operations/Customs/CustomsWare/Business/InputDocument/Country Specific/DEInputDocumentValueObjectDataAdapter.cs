using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class DEInputDocumentValueObjectDataAdapter : EUInputDocumentValueObjectDataAdapter, ICustomsWareDEInputDocumentValueObjectDataAdapter
	{
		protected override ZString GetDeclarationType(XSD.DeclarationHeader xsdDeclarationHeader)
		{
			ZString result = "";
			if (xsdDeclarationHeader.DeclarationType.StartsWith("EX", System.StringComparison.OrdinalIgnoreCase)
				|| xsdDeclarationHeader.DeclarationType.StartsWith("DEEX", System.StringComparison.OrdinalIgnoreCase))
			{
				//EXP, Export declaration
				//EXT, exit declaration
				result = "EXP";
			}
			else if (xsdDeclarationHeader.DeclarationType.StartsWith("IM", System.StringComparison.OrdinalIgnoreCase)
				|| xsdDeclarationHeader.DeclarationType.StartsWith("DEIM", System.StringComparison.OrdinalIgnoreCase)
				|| xsdDeclarationHeader.DeclarationType.Contains("SUMA", System.StringComparison.Ordinal)
				|| xsdDeclarationHeader.DeclarationType.Contains("ZIA", System.StringComparison.Ordinal))
			{
				//IMAVUV, Import inward processing
				//IMDEGZ, Import supplementary declaration
				//IMDEZA, Import normal declaration
				//IMDVZA, Import simplified
				//IMWEZA, import warehouse normal
				//IMWREM, import warehouse removal
				//SUMA, temporary storage
				//ZIA, informal declaration
				result = "IMP";
			}

			if (result.IsEmpty)
			{
				result = base.GetDeclarationType(xsdDeclarationHeader);
			}

			return result;
		}
	}
}
