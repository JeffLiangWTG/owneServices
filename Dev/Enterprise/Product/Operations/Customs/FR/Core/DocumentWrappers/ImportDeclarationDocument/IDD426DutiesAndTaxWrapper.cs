using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD426DutiesAndTaxWrapper : IDDDutiesAndTaxWrapper<DutiesAndTaxesType>
{
	public static IDD426DutiesAndTaxWrapper New(DutiesAndTaxesType dutiesAndTaxes, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factoryToWrap)
	{
		return new IDD426DutiesAndTaxWrapper(dutiesAndTaxes, dutiesAndTaxesSummaries, factoryToWrap);
	}

	protected IDD426DutiesAndTaxWrapper(DutiesAndTaxesType dutiesAndTaxes, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factory) : base(dutiesAndTaxes, factory)
	{
		this.dutiesAndTaxes = Argument.NotNull(dutiesAndTaxes, nameof(dutiesAndTaxes));
		this.dutiesAndTaxesSummaries = dutiesAndTaxesSummaries;
	}

	readonly DutiesAndTaxesType dutiesAndTaxes;
	readonly ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;

	public override ZString NationalTaxType => dutiesAndTaxes.NationalTaxType;

	public override ZString TaxType => dutiesAndTaxes.TaxType;

	public override ZString PayableTaxAmount => dutiesAndTaxes.PayableTaxAmount.ToString();

	public override ZString Amount => dutiesAndTaxes.TaxBase?.FirstOrDefault()?.Amount.ToString() ?? ZString.Empty;

	public override ZString TaxRate => dutiesAndTaxes.TaxBase?.FirstOrDefault()?.TaxRate.ToString() ?? ZString.Empty;

	public override ZString TaxAmount => dutiesAndTaxes.TaxBase?.FirstOrDefault()?.TaxAmount.ToString() ?? ZString.Empty;

	public override ZString Status => dutiesAndTaxesSummaries?.FirstOrDefault(dutiesAndTaxes => dutiesAndTaxes.NationalTaxType.Equals(this.dutiesAndTaxes.NationalTaxType))?.TaxationStatus ?? ZString.Empty;
}
