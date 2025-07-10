using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	[DependentBusinessObject(typeof(CusStatementChargesDetail), "Charges")]
	public class CusStatementLineCharge : BaseCusStatementLineCharge
	{
		public CusStatementLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusStatementChargesDetail ChargesDetail => Factory.Load<CusStatementChargesDetail>(B4_B3);

		public new CusStatementLineChargeLookups Lookups => (CusStatementLineChargeLookups)base.Lookups;

		protected override Customs.Business.CusStatementLineChargeLookups GetNewLookups() => new CusStatementLineChargeLookups(this);

		[ResourceStringData("FR.CusStatementLineCharge.B4_ChargeType", Caption = "Tax Code")]
		[List(nameof(Lookups) + "." + nameof(CusStatementLineChargeLookups.ChargeTypeList))]
		public override ZString B4_ChargeType { get => base.B4_ChargeType; set => base.B4_ChargeType = value; }

		[ResourceStringData("FR.CusStatementLineCharge.B4_MethodOfPayment", Caption = "Method of Payment")]
		[List(nameof(Lookups) + "." + nameof(CusStatementLineChargeLookups.MethodOfPaymentList))]
		public override ZString B4_MethodOfPayment { get => base.B4_MethodOfPayment; set => base.B4_MethodOfPayment = value; }

		[ResourceStringData("FR.CusStatementLineCharge.MethodOfPaymentDescription", Caption = "Method of Payment Description", ShortCaption = "Method of Payment Desc.")]
		public ZString MethodOfPaymentDescription => Lookups.MethodOfPaymentList.GetMultilingualDescriptionFromCode(B4_MethodOfPayment);

		[ResourceStringData("FR.CusStatementLineCharge.B4_ChargeGroup", Caption = "Tax Code (EU)")]
		[List(nameof(Lookups) + "." + nameof(CusStatementLineChargeLookups.ChargeGroupList))]
		public override ZString B4_ChargeGroup { get => base.B4_ChargeGroup; set => base.B4_ChargeGroup = value; }

		[ResourceStringData("FR.CusStatementLineCharge.B4_ChargeAmount", Caption = "Amount")]
		public override ZDecimal B4_ChargeAmount { get => base.B4_ChargeAmount; set => base.B4_ChargeAmount = value; }
	}
}
