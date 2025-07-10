using System;
using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SpecialCaseTaxValidation : ZValidation
	{
		public SpecialCaseTaxValidation(SpecialCaseTax specialCaseTax) : base(specialCaseTax)
		{
			parent = specialCaseTax;
		}
		readonly SpecialCaseTax parent;

		public override Type AutoValidationType => typeof(SpecialCaseTaxValidation);

		public override void ValidateAll()
		{
			parent.LegalAct.Validation.ValidateAll();
			ValidateTaxGroup();
			ValidateTaxType();
			ValidateRateOrUnitValue();
		}

		public void ValidateTaxGroup()
		{
			ValidateCalculatedProperty(parent.TaxGroupInfo);
		}

		protected void CheckTaxGroup()
		{
			var targetInfo = parent.TaxGroupInfo;

			ListValidation.ErrorIfInvalidCode(targetInfo);
			MandatoryValidation.CheckEntered(targetInfo);
			ValidationHelper.CheckRateIsOverridenForSpecialCases(targetInfo, parent.InvoiceLine);

			if (!parent.TaxGroup.IsEmpty && parent.InvoiceLine.SpecialCaseTaxes.Where(x => x.TaxGroup == parent.TaxGroup).Count() > 1)
			{
				targetInfo.AddError(Res.GetString("39a0f8e7-2c68-4574-bb7a-91676e3e421a", "You have entered a duplicate Tax Group"));
			}
			ValidateTaxType();
		}

		public void ValidateTaxType()
		{
			ValidateCalculatedProperty(parent.TaxTypeInfo);
		}

		protected void CheckTaxType()
		{
			ListValidation.ErrorIfInvalidCode(parent.TaxTypeInfo);
			MandatoryValidation.CheckEntered(parent.TaxTypeInfo);
		}

		public void ValidateRateOrUnitValue()
		{
			ValidateCalculatedProperty(parent.RateOrUnitValueInfo);
		}

		protected void CheckRateOrUnitValue()
		{
			MandatoryValidation.CheckNotNegative(parent.RateOrUnitValueInfo);
		}
	}
}
