using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public CDSAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
			invoiceLine = Parent.Parent;
		}

		protected override void CheckZG_MethodOfPayment()
		{
			base.CheckZG_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_MethodOfPaymentInfo);
		}

		protected override void CheckZG_CountryOfSupply()
		{
			base.CheckZG_CountryOfSupply();

			if (IsCountryOfOriginAndSupplyTheSame())
			{
				Parent?.ZG_CountryOfSupplyInfo.AddMessageError(CountryOriginAndSupplySameError);
			}

			if (IsCountryofSupplyOptional())
			{
				Parent?.ZG_CountryOfSupplyInfo.AddWarning(OriginOverrideOptionalWarning);
			}

			(invoiceLine.Validation as CDSJobComInvoiceLineValidation)?.ValidateJI_CountryOfOrigin();
		}

		bool IsCountryOfOriginAndSupplyTheSame()
		{
			var parent = Parent;
			return parent != null && !parent.ZG_CountryOfSupply.IsEmpty && !invoiceLine.JI_CountryOfOrigin.IsEmpty &&
				   parent.ZG_CountryOfSupply.Equals(invoiceLine.JI_CountryOfOrigin);
		}

		bool IsCountryofSupplyOptional()
		{
			var parent = Parent;
			return parent != null && !parent.ZG_CountryOfSupply.IsEmpty &&
				   invoiceLine.JI_PrimaryPreference.StartsWith("1", System.StringComparison.OrdinalIgnoreCase);
		}

		protected override void CheckZG_GoodsCategory()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_GoodsCategoryInfo);
			var style = (string)invoiceLine.EntryInstruction?.CEI_Style;
			if (invoiceLine.IsImport && style == ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration && invoiceLine.ZG_GoodsCategory == GoodsCategoryList.Codes.Category1)
			{
				Parent?.ZG_GoodsCategoryInfo.AddMessageError(SPIMMCategory1Error);
			}
		}

		readonly JobComInvoiceLine invoiceLine;

		const string CountryOriginAndSupplySameError = "If 5/15 Origin Override is supplied it must be different to 5/16";
		const string OriginOverrideOptionalWarning = "With preference starting 1, 5/15 Origin Override is not needed and is not sent";
		internal const string SPIMMCategory1Error = "Goods included in Category 1 are excluded from Simplified Process for Internal Market Movements (SPIMM/H8). Full declaration details are required.";
	}
}
