using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUASPEntryChargeTypeList : EntryChargeTypeList
	{
		public AUASPEntryChargeTypeList()
		{
			Add(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, CusEntryChargeTypeList.Descriptions.AQISServicePaymentAmount, true, "");
		}

		public override string DutyCode => throw new NotImplementedException();

		public override string TaxCode => throw new NotImplementedException();

		protected override IEnumerable<ZGuid> GetSpecialChargeCodePks(ZGuid companyPK)
		{
			var aspChargeCode = GetChargeCodePKForASP(companyPK.ToGuid());
			if (aspChargeCode.IsValid)
			{
				yield return aspChargeCode;
			}
		}

		public override EntryChargeTypeSetting GetChargeTypeSpecificRegistrySetting(string chargeCode)
		{
			EntryChargeTypeSetting result = null;
			if (chargeCode == CusEntryChargeTypeList.Codes.AQISServicePaymentAmount)
			{
				var aspChargeCode = GetChargeCodePKForASP(Env.CurrentCompanyPK);
				if (aspChargeCode.IsValid)
				{
					result = new EntryChargeTypeSetting
					{
						ChargeType = chargeCode,
						AC_ChargeCode = aspChargeCode
					};
				}
			}

			return result;
		}

		public static ZGuid GetChargeCodePKForASP(ZGuid companyPK)
		{
			return RatingDataRegistry.Instance.CustomsQuarantineChargeCode.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).ChargeCode;
		}
	}
}
