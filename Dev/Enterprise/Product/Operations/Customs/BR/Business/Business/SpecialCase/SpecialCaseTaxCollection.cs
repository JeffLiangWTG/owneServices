using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class SpecialCaseTaxCollection : NonPersistentBusinessObjectCollection<SpecialCaseTax>
	{
		public SpecialCaseTaxCollection(JobComInvoiceLine parent) : base(parent.Factory)
		{
			invoiceLine = parent;
		}
		readonly JobComInvoiceLine invoiceLine;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SpecialCaseTax(invoiceLine.Taxes.AddNew(), null, null);
		}

		public override void Load()
		{
			if (!IsLoaded)
			{
				RemoveAll();

				foreach (var tax in invoiceLine.Taxes.Where(tax => tax.ShouldBeIncludedOnSpecialCases))
				{
					var quantityPerUnit = tax.JLT_MethodOfCalculation == SpecialCaseTaxTypeList.Codes.QuantityPerUnit ? FindOrAddQuantityPerUnit(tax.JLT_Type) : null;
					var legalAct = tax.JLT_Type == Constants.RateCodes.Antidumping ? FindOrAddNewLegalAct(AdditionalTaxTypeList.Codes.Antidumping) : null;
					Add(new SpecialCaseTax(tax, quantityPerUnit, legalAct));
				}
			}

			IsLoaded = true;
		}

		QuantityPerUnitInfo FindOrAddQuantityPerUnit(ZString rateCode)
		{
			return invoiceLine.QuantityPerUnitInfos.FindByRateCode(rateCode) ?? invoiceLine.QuantityPerUnitInfos.AddNew(rateCode);
		}

		LegalActInfo FindOrAddNewLegalAct(ZString subject)
		{
			return invoiceLine.LegalActInfos.FindBySubject(subject) ?? invoiceLine.LegalActInfos.AddNew(subject);
		}

		public void Rebuild()
		{
			IsLoaded = false;
			Load();
		}

		public void UpdateOrAddReductionRate(ZString rateCode)
		{
			var tax = this.Cast<SpecialCaseTax>().FirstOrDefault(x => x.TaxGroup == rateCode);
			if (tax == null)
			{
				tax = AddNew();
				tax.TaxGroup = rateCode;
			}

			tax.TaxType = SpecialCaseTaxTypeList.Codes.Reduced;
		}

		public void DeleteReductionRate(ZString rateCode)
		{
			var tax = this.Cast<SpecialCaseTax>().FirstOrDefault(x => x.TaxGroup == rateCode && x.TaxType == SpecialCaseTaxTypeList.Codes.Reduced);
			if (tax != null)
			{
				RemoveAndDelete(tax);
			}
		}

		public SpecialCaseTax FindByRateCodeAndTaxType(ZString rateCode, ZString taxType)
		{
			return this.Cast<SpecialCaseTax>().FirstOrDefault(x => x.TaxGroup == rateCode && x.TaxType == taxType);
		}
	}
}
