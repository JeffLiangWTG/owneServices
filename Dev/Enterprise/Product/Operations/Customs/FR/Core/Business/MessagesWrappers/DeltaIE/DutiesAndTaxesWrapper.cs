using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class DutiesAndTaxesWrapper : IDutiesAndTaxes
	{
		DutiesAndTaxesWrapper(CusEntryLineFee fee, string customsOffice)
		{
			Argument.NotNull(fee, nameof(fee));
			this.MethodOfPayment = fee.CF_MethodOfPayment;
			this.NationalTaxType = fee.NationalFeeTypeCode;
			this.PayableTaxAmount = (double)fee.CF_ChargeAmount;
			this.TaxBase = new List<ITax>() { TaxWrapper.New(fee) };
			this.TaxType = fee.CF_ChargeType;
			this.customsOffice = customsOffice;
		}

		readonly string customsOffice;

		DutiesAndTaxesWrapper(CusEntryHeaderCharges charge, string customsOffice)
		{
			Argument.NotNull(charge, nameof(charge));
			this.MethodOfPayment = charge.C1_MethodOfPayment;
			this.NationalTaxType = charge.C1_ChargeType;
			this.PayableTaxAmount = (double)charge.C1_ChargeAmount;
			this.TaxBase = new List<ITax>() { TaxWrapper.New(charge) };
			this.TaxType = FeeTypeCodeConverter.GetEUFeeTypeCode(charge.C1_ChargeType);
			this.customsOffice = customsOffice;
		}

		public static DutiesAndTaxesWrapper New(CusEntryLineFee fee, string customsOffice) => fee == null ? null : new DutiesAndTaxesWrapper(fee, customsOffice);

		public static DutiesAndTaxesWrapper New(CusEntryHeaderCharges charge, string customsOffice) => charge == null ? null : new DutiesAndTaxesWrapper(charge, customsOffice);

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? string.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;

		public string MethodOfPayment { get; }

		public string NationalTaxType { get; }

		public double PayableTaxAmount { get; }

		public ICollection<ITax> TaxBase { get; }

		public string TaxType { get; }
	}
}
