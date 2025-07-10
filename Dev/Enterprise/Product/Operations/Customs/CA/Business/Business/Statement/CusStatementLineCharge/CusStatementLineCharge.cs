using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business
{
	[DependentBusinessObject(typeof(CusStatementLine), "Charges")]
	public class CusStatementLineCharge : BaseCusStatementLineCharge, Integration.Customs.CA.ICusStatementLineCharge
	{
		public CusStatementLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public ZString ChargeTypeDescription
		{
			get
			{
				var result = ZString.Empty;
				if (!B4_ChargeType.IsEmpty)
				{
					if (((CusStatementHeader)StatementLine.StatementHeader).IsCARMDailyNotice)
					{
						result = Factory.GetCachedValue<CARMDailyNoticeChargeTypeList>().GetDescriptionFromCode(B4_ChargeType);
					}
					else
					{
						result = Factory.GetCachedValue<EntryChargeTypeList>().GetDescriptionFromCode(B4_ChargeType);
					}
				}

				return result.IsEmpty ? B4_ChargeType : result;
			}
		}

		public ZPropertyInfo ChargeTypeDescriptionInfo => GetZPropertyInfo(nameof(ChargeTypeDescription));

		#endregion
	}
}
