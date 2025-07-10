using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business
{
	class JobDeclarationCustomsCharges : Customs.Business.InterfaceImplementations.JobDeclarationCustomsCharges
	{
		public JobDeclarationCustomsCharges(JobDeclaration declaration)
			: base(declaration)
		{
		}

		new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		#region ICustomsCharges Members

		protected override ZBool IsCustomsChargesActiveCore
		{
			get { return declaration.IsImportIncludingB2; }
		}

		#endregion

		protected override Customs.Business.CusEntryHeader[] GetEntries()
		{
			return declaration.IsB2Adjustments || declaration.IsB3X
				? declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray()
				: new CusEntryHeader[] { declaration.B3EntryHeader };
		}

		protected override void AddAdditionalDescriptions(CustomsCharge charge)
		{
			base.AddAdditionalDescriptions(charge);
			if (charge.Description == EntryChargeTypeList.Descriptions.TotalSIMAAmount || charge.Description == EntryChargeTypeList.Descriptions.TotalNonBillableSIMAAmount)
			{
				ZDecimal sum = declaration.InvoiceLines.SelectMany(line => ((JobComInvoiceLine)line).DutiesAndTaxes.Where(duty => duty.C1_TaxType == DutyAndTaxTypes.Codes.SUR)).Sum(sur => sur.C1_Amount);
				if (sum > 0)
				{
					charge.AdditionalChargeInfos.Add(new CustomsCharge.ChargeInfo(DutyAndTaxTypes.Descriptions.SUR, sum));
				}
				sum = declaration.InvoiceLines.SelectMany(line => ((JobComInvoiceLine)line).DutiesAndTaxes.Where(duty => duty.C1_TaxType == DutyAndTaxTypes.Codes.ADD)).Sum(add => add.C1_Amount);
				if (sum > 0)
				{
					charge.AdditionalChargeInfos.Add(new CustomsCharge.ChargeInfo(DutyAndTaxTypes.Descriptions.ADD, sum));
				}
				sum = declaration.InvoiceLines.SelectMany(line => ((JobComInvoiceLine)line).DutiesAndTaxes.Where(duty => duty.C1_TaxType == DutyAndTaxTypes.Codes.CVD)).Sum(cvd => cvd.C1_Amount);
				if (sum > 0)
				{
					charge.AdditionalChargeInfos.Add(new CustomsCharge.ChargeInfo(DutyAndTaxTypes.Descriptions.CVD, sum));
				}
			}
		}
	}
}
