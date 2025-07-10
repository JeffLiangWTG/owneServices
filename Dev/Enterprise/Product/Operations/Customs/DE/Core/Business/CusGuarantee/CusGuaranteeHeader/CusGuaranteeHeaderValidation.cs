using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusGuaranteeHeaderValidation : EU.Business.CusGuaranteeHeaderValidation
	{
		public CusGuaranteeHeaderValidation(CusGuaranteeHeader parent)
			: base(parent)
		{
		}

		public new CusGuaranteeHeader Parent => (CusGuaranteeHeader)base.Parent;

		protected override void CheckCPH_SubType()
		{
			base.CheckCPH_SubType();

			if (Parent.IsTRAGuaranteeType)
			{
				var subType = Parent.CPH_SubType;
				var propertyInfo = Parent.CPH_SubTypeInfo;

				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);

				if (subType == GuaranteeSubTypeList.Codes._0 || subType == GuaranteeSubTypeList.Codes._1 || subType == GuaranteeSubTypeList.Codes._2 || subType == GuaranteeSubTypeList.Codes._4)
				{
					if (Parent.MainAccessCode.IsEmpty)
					{
						propertyInfo.AddMessageError(Res.GetString("58E5055C-A2A7-44B5-B002-F9294A3291FE", "You have not entered a Main Access Code."));
					}
				}
			}
		}

		protected override void CheckCPH_OH_PermitHolder()
		{
			base.CheckCPH_OH_PermitHolder();

			if (Parent.IsTRAGuaranteeType && Parent.PermitHolder.GetEuIdentificationNumber().IsEmpty)
			{
				Parent.CPH_OH_PermitHolderInfo.AddMessageError(Res.GetString("4FC3EA55-A293-44BC-9719-8A219C805606", "The Guarantee Holder must have a Registration Number / Code of Type 'EOR'."));
			}
		}

		protected override void CheckMainAccessCode()
		{
			base.CheckMainAccessCode();
			if (Parent.IsTRAGuaranteeType && Parent.MainAccessCode.Length != 4)
			{
				Parent.MainAccessCodeInfo.AddMessageError(Res.GetString("E73D0485-1F51-450C-83AB-8D59DEAC645F", "The Main Access Code must have 4 digits."));
			}
		}
	}
}
