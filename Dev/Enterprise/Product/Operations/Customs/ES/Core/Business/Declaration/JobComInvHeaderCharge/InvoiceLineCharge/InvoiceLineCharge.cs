using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceLineCharge : EU.Business.Declaration.InvoiceLineCharge, Integration.Customs.ES.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);

		public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

		protected override bool GetIncludedInITOTReadOnly()
		{
			return !IsExport && base.GetIncludedInITOTReadOnly();
		}

		protected override bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly()
		{
			var charge = IncoTermAndChargeFactory?.GetCharge(J7_ChargeType);
			if (IsImport && charge != null && (IsExportedGoodsValueForOutwardProcessingCharge(charge) || IsInvoicedExportedGoodsValueForOutwardProcessingCharge(charge) || IsReaAidAmountIgicBaseCalculationCharge(charge)))
			{
				return true;
			}
			return base.GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly();
		}

		protected override void DefaultIsIncludedInInvoice(Common.IncoTermAndCustomsChargeFactory incoTermAndChargeFactory, ICustomsChargeCode charge)
		{
			if (charge == null || charge.Code == ChargeTypeList.Codes.StatisticalValue || IsExportedGoodsValueForOutwardProcessingCharge(charge) || IsReaAidAmountIgicBaseCalculationCharge(charge))
			{
				J7_Calc_IsIncludedInInvoiceAmount = false;
			}
			else if (IsInvoicedExportedGoodsValueForOutwardProcessingCharge(charge))
			{
				J7_Calc_IsIncludedInInvoiceAmount = true;
			}
			else if (IsExport || IsIncludedInInvoiceAmountFixed || ShouldResetDefaultIsIncludedInAmount(IncoTerm, charge))
			{
				J7_Calc_IsIncludedInInvoiceAmount = incoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(Parent.IncoTerm, charge);
			}
		}

		ZBool IsExportedGoodsValueForOutwardProcessingCharge(ICustomsChargeCode charge) => charge.Code == ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing;

		ZBool IsInvoicedExportedGoodsValueForOutwardProcessingCharge(ICustomsChargeCode charge) => charge.Code == ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing;

		ZBool IsReaAidAmountIgicBaseCalculationCharge(ICustomsChargeCode charge) => charge.Code == ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation;

		ZBool IsExport => InvoiceLine?.IsExport ?? ZBool.False;

		ZBool IsImport => InvoiceLine?.IsImport ?? ZBool.False;
	}
}
