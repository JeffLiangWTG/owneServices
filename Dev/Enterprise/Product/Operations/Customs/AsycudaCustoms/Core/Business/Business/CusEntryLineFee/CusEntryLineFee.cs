using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee, Integration.Customs.AsycudaCustoms.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.ChargeTypeList))]
		public override ZString CF_ChargeType => base.CF_ChargeType;

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation()
		{
			return new CusEntryLineFeeValidation(this);
		}

		public new CusEntryLineFeeValidation Validation
		{
			get { return (CusEntryLineFeeValidation)GetNewValidation(); }
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.CusEntryLineFee|LocalCurrencyCode", Caption = "Currency")]
		public ZString LocalCurrencyCode => EntryLine?.Declaration?.LocalCurrencyCode ?? ZString.Empty;

		public ZPropertyInfo LocalCurrencyCodeInfo => GetZPropertyInfo(nameof(LocalCurrencyCode));

		protected override bool ShouldResetDataOnMergingCore => CF_ChargeAmount.IsEmpty;
	}
}
