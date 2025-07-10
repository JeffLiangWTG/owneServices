using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatLineValidation : AutoCusIntrastatLineValidation
	{
		public CusIntrastatLineValidation(AutoCusIntrastatLine parent) : base(parent)
		{
		}

		protected override void CheckCIL_Tariff()
		{
			base.CheckCIL_Tariff();
			var propertyInfo = Parent.CIL_TariffInfo;
			MandatoryValidation.CheckEntered(propertyInfo);

			if (Parent.CIL_Tariff is { IsEmpty: false, Length: < 8 })
			{
				propertyInfo.AddError(Res.GetString("6e7349e2-eb13-433b-917d-a2781c8855f6", "{0} code must be at least 8 digits", propertyInfo.HumanReadableName));
			}
		}

		protected override void CheckCIL_SupplementaryQuantityUnit()
		{
			base.CheckCIL_SupplementaryQuantityUnit();
			var propertyInfo = Parent.CIL_SupplementaryQuantityUnitInfo;
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);
		}

		protected override void CheckCIL_Region()
		{
			base.CheckCIL_Region();
			var propertyInfo = Parent.CIL_RegionInfo;
			CheckTwoLetterCodeOrEmpty(propertyInfo);
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);
		}

		protected override void CheckCIL_RN_NKCountryOfOrigin()
		{
			base.CheckCIL_RN_NKCountryOfOrigin();
			var propertyInfo = Parent.CIL_RN_NKCountryOfOriginInfo;
			CheckTwoLetterCodeOrEmpty(propertyInfo);
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);
		}

		static void CheckTwoLetterCodeOrEmpty(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.Value is ZString { IsEmpty: false, Length: not 2 })
			{
				propertyInfo.AddError(Res.GetString("afe6dc4d-994c-4b6d-8604-666ddc19a1f1", "{0} must be a 2 letter code", propertyInfo.HumanReadableName));
			}
		}
	}
}
