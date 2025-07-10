using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(CusStatementLine), "Charges")]
	public class CusStatementLineCharge : BaseCusStatementLineCharge
	{
		public CusStatementLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return true;
		}

		#endregion

		[MaxLength(3)]
		[ResourceStringData("0A73BF64-D8E4-483A-B59D-DF86CCD915F9", Caption = "Fee Type")]
		[ResourceStringData("4C94B37E-6AB0-4E70-8CA4-670E9AD4C969", Caption = "Charge Type", MultipleKey = StatementHeaderTypeList.Codes.Invoice)]
		public override ZString B4_ChargeType { get => base.B4_ChargeType; set => base.B4_ChargeType = value; }

		[DecimalPlaces(0)]
		[ResourceStringData("930B540C-5AB5-4006-91FA-64295605B618", Caption = "Fee")]
		[ResourceStringData("932ADACB-B678-435A-9DBF-A7DF1C6599BF", Caption = "Tax", MultipleKey = StatementHeaderTypeList.Codes.Invoice)]
		public override ZDecimal B4_ChargeAmount { get => base.B4_ChargeAmount; set => base.B4_ChargeAmount = value; }

		[ResourceStringData("26198AAF-ED4D-4016-A74F-682F852F002E", Caption = "Fee Type Name")]
		[ResourceStringData("4E32543B-6029-468B-923B-A1E5846D2E06", Caption = "Charge Type Name", MultipleKey = StatementHeaderTypeList.Codes.Invoice)]
		public ZString ChargeTypeName
		{
			get
			{
				return Factory.GetCachedValue<ChargeTypeList>().GetDescriptionFromCode(B4_ChargeType);
			}
		}

		public ZPropertyInfo ChargeTypeNameInfo => GetZPropertyInfo(nameof(ChargeTypeName));
	}
}
