using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

public class DocSADHLineTax : Enterprise.DocumentWrappers.Customs.EU.DocSADHLineTax
{
	public new static DocSADHLineTax New(IDocSADHLineTaxBoxSupporter taxSupporter, BusinessObjectFactory factory)
	{
		return taxSupporter == null ? null : new DocSADHLineTax(taxSupporter, factory);
	}

	DocSADHLineTax(IDocSADHLineTaxBoxSupporter taxSupporter, BusinessObjectFactory factory) : base(taxSupporter, factory)
	{
		this.taxSupporter = Argument.NotNull(taxSupporter, nameof(taxSupporter));
	}

	new readonly IDocSADHLineTaxBoxSupporter taxSupporter;

	public override ZString TaxType
	{
		get
		{
			var result = "";
			var nationalDeeCode = taxSupporter.NationalFeeTypeCode;
			if (nationalDeeCode.IsEmpty)
			{
				result = "    /" + G4_Type;
			}
			else
			{
				var cusMap = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.France, RefCusMapTypeList.Codes.FRDTY, nationalDeeCode, ZDateTime.Today);
				var type = cusMap.IsEmpty ? G4_Type : cusMap;
				result = nationalDeeCode + "/" + type;
			}
			return result;
		}
	}
	public override ZString TaxMethodOfPayment
	{
		get
		{
			var methodOfPayment = G4_MethodOfPayment;

			switch (G4_MethodOfPayment)
			{
				case "1":
					methodOfPayment = "C";
					break;
				case "2":
					methodOfPayment = "NC";
					break;
				case "3":
					methodOfPayment = "AI2";
					break;
				default:
					methodOfPayment = ZString.Empty;
					break;
			}
			return methodOfPayment + "/" + taxSupporter.DeclarationMethodOfPayment;
		}
	}
}
