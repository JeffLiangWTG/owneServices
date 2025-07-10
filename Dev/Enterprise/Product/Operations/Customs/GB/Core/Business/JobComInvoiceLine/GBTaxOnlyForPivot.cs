using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business
{
	public class GBTaxOnlyForPivot : CusAddInfo<Tax_CusAddInfoOnlyForPIVOT>
		, IEuTax
		, Integration.Customs.GB.IGBTaxOnlyForPivot
	{
		public GBTaxOnlyForPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new Tax_OnlyForPivot Data => (Tax_OnlyForPivot)base.Data;

		ZString IEuTax.CountryCode => ((IEuTax)Data).CountryCode;

		ICanBeImportOrExport IEuTax.ImportExportParent => ((IEuTax)Data).ImportExportParent;

		ZDecimal IEuTax.G4_CalculatedPercentage => ((IEuTax)Data).G4_CalculatedPercentage;

		ZString IEuTax.G4_MethodOfPayment { get => ((IEuTax)Data).G4_MethodOfPayment; set => ((IEuTax)Data).G4_MethodOfPayment = value; }
		ZString IEuTax.G4_RateDuty { get => ((IEuTax)Data).G4_RateDuty; set => ((IEuTax)Data).G4_RateDuty = value; }
		ZString IEuTax.G4_RateOverride { get => ((IEuTax)Data).G4_RateOverride; set => ((IEuTax)Data).G4_RateOverride = value; }
		ZString IEuTax.G4_RateSuspension { get => ((IEuTax)Data).G4_RateSuspension; set => ((IEuTax)Data).G4_RateSuspension = value; }
		ZString IEuTax.G4_Type { get => ((IEuTax)Data).G4_Type; set => ((IEuTax)Data).G4_Type = value; }
		ZString IEuTax.G4_Amount { get => ((IEuTax)Data).G4_Amount; set => ((IEuTax)Data).G4_Amount = value; }
		ZDecimal IEuTax.G4_BaseAmount { get => ((IEuTax)Data).G4_BaseAmount; set => ((IEuTax)Data).G4_BaseAmount = value; }
		ZDecimal IEuTax.G4_BaseQuantity { get => ((IEuTax)Data).G4_BaseQuantity; set => ((IEuTax)Data).G4_BaseQuantity = value; }
		ZString IEuTax.G4_BaseQuantityUQ { get => ((IEuTax)Data).G4_BaseQuantityUQ; set => ((IEuTax)Data).G4_BaseQuantityUQ = value; }

		ZPropertyInfo IEuTax.G4_MethodOfPaymentInfo => ((IEuTax)Data).G4_MethodOfPaymentInfo;

		ZPropertyInfo IEuTax.G4_RateDutyInfo => ((IEuTax)Data).G4_RateDutyInfo;

		ZPropertyInfo IEuTax.G4_RateOverrideInfo => ((IEuTax)Data).G4_RateOverrideInfo;

		ZPropertyInfo IEuTax.G4_RateSuspensionInfo => ((IEuTax)Data).G4_RateSuspensionInfo;

		ZPropertyInfo IEuTax.G4_TypeInfo => ((IEuTax)Data).G4_TypeInfo;

		ZPropertyInfo IEuTax.G4_AmountInfo => ((IEuTax)Data).G4_AmountInfo;

		ZPropertyInfo IEuTax.G4_BaseAmountInfo => ((IEuTax)Data).G4_BaseAmountInfo;

		protected override Type AddInfoType => typeof(Tax_OnlyForPivot);

		ZBool IEuTax.IsCopying => ((IEuTax)Data).IsCopying;

		#region IGBTaxOnlyForPivot Members

		Integration.Customs.GB.ITax_OnlyForPivot Integration.Customs.GB.IGBTaxOnlyForPivot.Data => Data;

		#endregion
	}
}
