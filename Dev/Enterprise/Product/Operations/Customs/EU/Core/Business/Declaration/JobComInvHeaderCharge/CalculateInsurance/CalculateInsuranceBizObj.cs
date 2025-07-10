using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CalculateInsuranceBizObj : AutoCalculateInsuranceNonPersistentBizObj
	{
		public CalculateInsuranceBizObj(EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory, JobComInvoiceHeader invoice) : base(invoice.Factory)
		{
			flatRate = 0;
			var declaration = Argument.NotNull(invoice.JobDeclaration, nameof(invoice.JobDeclaration));
			this.charges = invoice.Charges;
			this.euIncoTermAndChargeFactory = euIncoTermAndChargeFactory;
			invoiceAmount = invoice.JZ_InvoiceAmount;
			currency = invoice.JZ_RX_NKInvoice_Currency;
			if (IsDutiablePercentEnabled)
			{
				DutiablePercent = SetDefaultDutiablePercent(Factory, declaration.CountryCode, declaration.DateOfValuation, declaration.JE_IATALoadPort);
			}
			var flatAmount = InsuranceRuleEngine.GetInsuranceFlatValue(declaration, invoice);
			if (flatAmount > 0)
			{
				flatRate = flatAmount;
			}
			else
			{
				InsurancePercentage = InsuranceRuleEngine.GetInsuranceUpliftPercent(declaration, invoice);
			}
		}

		static ZDecimal SetDefaultDutiablePercent(BusinessObjectFactory factory, ZString country, ZDateTime dateOfValuation, ZString iataLoadPort)
		{
			ZDecimal result = 100m;
			if (!iataLoadPort.IsEmpty)
			{
				var percentages = RefCusCodeListAttributeTypes.GetAttributeValuesFor(factory, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, dateOfValuation, iataLoadPort, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Percentage);
				if (percentages != null && percentages.Length == 1)
				{
					result = ZDecimal.ParseSafe(percentages[0], ZDecimal.Zero);
				}
			}
			return result;
		}

		protected readonly EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory;
		protected readonly IJobComInvChargeCollection<JobComInvCharge> charges;

		protected override ZDecimal GetInvoiceAmount() => invoiceAmount;
		readonly ZDecimal invoiceAmount;

		protected override ZString GetCurrency() => currency;
		readonly ZString currency;

		public override ZDecimal InsurancePercentage
		{
			get => base.InsurancePercentage;
			set
			{
				if (flatRate > 0)
				{
					flatRate = 0;
				}
				base.InsurancePercentage = value;
			}
		}

		protected override ZDecimal GetInsuranceAmount()
		{
			return flatRate > 0 ? flatRate : ZArchitecture.Core.Utilities.Round(InvoiceAmount * InsurancePercentage / 100, InsuranceAmount_Scale);
		}

		public bool Calculate()
		{
			var result = false;
			RunPreSaveValidation();
			if (!HasErrors)
			{
				result = GetCalculateResult();
			}
			return result;
		}

		protected virtual bool GetCalculateResult()
		{
			var result = false;
			if (!InsuranceAmount.IsEmpty)
			{
				euIncoTermAndChargeFactory.AddOrUpdateInsuranceCharge(charges, InsuranceAmount, Currency, DutiablePercent, IsInsuranceIncludedInLines);
				result = true;
			}
			return result;
		}

		public virtual bool IsDutiablePercentEnabled => true;

		public override ZDecimal DutiablePercent
		{
			get => IsDutiablePercentEnabled ? base.DutiablePercent : 100;
			set => base.DutiablePercent = IsDutiablePercentEnabled ? value : throw new InvalidOperationException($"{nameof(DutiablePercent)} is not enabled");
		}

		ZDecimal flatRate;
	}
}
