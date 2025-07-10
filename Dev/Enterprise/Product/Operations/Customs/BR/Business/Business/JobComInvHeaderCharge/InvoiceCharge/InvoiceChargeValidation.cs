using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		JobDeclaration JobDeclaration => Parent?.Parent?.JobDeclaration as JobDeclaration;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();

			if (!Parent.J7_RX_NKCurrency.IsEmpty)
			{
				CheckAllChargesHaveTheSameCurrency(Parent.J7_ChargeTypeInfo, JobDeclaration);
			}
		}

		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			CheckDistributeBy(Parent.J7_DistributeByInfo, Parent.J7_ChargeType, JobDeclaration);
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			ValidateJ7_ChargeType();
		}

		public static void CheckAllChargesHaveTheSameCurrency(ZPropertyInfo chargeTypeInfo, JobDeclaration declaration)
		{
			if (declaration != null && declaration.IsImportExcludingLicense)
			{
				var chargeType = (ZString)chargeTypeInfo.Value;

				if (ImportCustomsChargeTypeList.OverseasFreightChargeTypes.Contains(chargeType) && !declaration.AllOverseasFreightChargesHaveTheSameCurrency)
				{
					chargeTypeInfo.AddMessageError(GetNotAllChargesHaveTheSameCurrencyMessage(ImportCustomsChargeTypeList.OverseasFreightChargeTypes));
				}
				if (chargeType == CustomsChargeTypeList.Codes.OverseasInsurance && !declaration.AllOverseasInsuranceChargesHaveTheSameCurrency)
				{
					chargeTypeInfo.AddMessageError(GetNotAllChargesHaveTheSameCurrencyMessage(chargeType));
				}
			}
		}

		public static void CheckDistributeBy(ZPropertyInfo distributeByInfo, ZString chargeType, JobDeclaration declaration)
		{
			var distributeBy = (ZString)distributeByInfo.Value;

			if (declaration != null && (declaration.IsImport))
			{
				if (distributeBy != ChargeDistributeByList.Codes.NetWeight
					&& (chargeType == ImportCustomsChargeTypeList.Codes.OverseasFreightCollect
					|| chargeType == ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid
					|| chargeType == ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory))
				{
					distributeByInfo.AddMessageError(GetDistributeByMustBeMessage(ChargeDistributeByList.Codes.NetWeight));
				}
				else if (distributeBy != ChargeDistributeByList.Codes.FOB
					&& (chargeType == CustomsChargeTypeList.Codes.OverseasInsurance
					|| ImportChargesProvider.IsAdditions(chargeType)
					|| ImportChargesProvider.IsDeductions(chargeType)))
				{
					distributeByInfo.AddMessageError(GetDistributeByMustBeMessage(ChargeDistributeByList.Codes.FOB));
				}
			}
		}

		public static string GetNotAllChargesHaveTheSameCurrencyMessage(params ZString[] chargeTypes)
		{
			return Res.GetString("9A63D8CD-7E3F-4248-B51D-966AD26AC3D2", "There is at least one {0} charge with a different currency among the Invoices. The system will convert the amount to the local currency before sending it to Customs.",
				string.Join("/", chargeTypes));
		}

		public static string GetDistributeByMustBeMessage(ZString distributeType)
		{
			return Res.GetString("6F5EC610-D29A-44C1-8EC8-A90E209E0413", "Distribute Type must be {0}", distributeType);
		}
	}
}
