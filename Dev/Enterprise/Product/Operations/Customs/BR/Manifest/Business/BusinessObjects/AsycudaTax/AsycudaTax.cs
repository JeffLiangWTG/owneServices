using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaTax : ASYCUDA.Business.AsycudaTax
	{
		public AsycudaTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ManifestBase.AutoAsycudaTax.Schema
		{
			public const string AET_TypeDescription = "AET_TypeDescription";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AET_MethodOfCalculation = "%";
			AET_IsCharge = ZBool.True;
		}

		public new AsycudaTaxValidation Validation => (AsycudaTaxValidation)base.Validation;
		protected override ManifestBase.AsycudaTaxValidation GetNewValidation() => new AsycudaTaxValidation(this);
		public new AsycudaTaxLookups Lookups => (AsycudaTaxLookups)base.Lookups;
		protected override ManifestBase.AsycudaTaxLookups GetNewLookups() => new AsycudaTaxLookups(this);

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(AsycudaTaxLookups.ChargeTypeList))]
		[ResourceStringData("94BAA8B2-4510-4863-87D5-8FE607B044FA", Caption = "Charge")]
		public override ZString AET_ChargeType { get => base.AET_ChargeType; set => base.AET_ChargeType = value; }

		[ResourceStringData("17C9F9FA-C8F8-4B37-9C1F-A473F70D2F6A", Caption = "Description")]
		public ZString AET_TypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(AET_ChargeType) ?? ZString.Empty;

		[ResourceStringData("76F9F94E-C107-4217-B281-F0DBF54F7C99", Caption = "Local Sell Amount")]
		[DecimalPlaces(2)]
		public override ZDecimal AET_ChargeAmount { get => base.AET_ChargeAmount; set => base.AET_ChargeAmount = value; }

		[MaxLength(3)]
		[ResourceStringData("90FE2761-1601-4713-AB08-F41C9E81CF8B", Caption = "Sell Currency")]
		public override ZString AET_RX_NKCurrency { get => base.AET_RX_NKCurrency; set => base.AET_RX_NKCurrency = value; }

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(AsycudaTaxLookups.MethodOfPaymentList))]
		[ResourceStringData("B9DA7DE2-718A-4639-A667-B419D0FCFF71", Caption = "Prepaid/Collect")]
		public override ZString AET_MethodOfPayment { get => base.AET_MethodOfPayment; set => base.AET_MethodOfPayment = value; }
	}
}
