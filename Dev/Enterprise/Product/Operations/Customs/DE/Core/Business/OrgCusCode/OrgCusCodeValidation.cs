using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class OrgCusCodeValidation : EU.Business.OrgCusCodeValidation
	{
		public OrgCusCodeValidation(OrgCusCode parent)
			: base(parent)
		{
		}

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();

			var codeType = Parent.OK_CodeType;
			switch (codeType)
			{
				case GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber:
					CheckSpecificTypeWithSameAddressIsRequired(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Res.GetString("2112F266-5999-419D-A20C-D4EBEFE396CB", "Code Type '{0}' requires also a record of Type '{1}' for the same Premises Address.",
						GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix));
					break;
				case GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix:
					CheckSpecificTypeIsRequiredIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Res.GetString("7E5726E1-882A-47FE-A335-7AF8BE089FCD", "Code Type '{0}' requires also a record of Type '{1}'.",
						GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
					break;
				case GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber:
					CheckSpecificTypeIsRequiredIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Res.GetString("F993F118-14AD-42FA-AD69-B11B898BA8EF", "You have not entered a Trader Excise Number ({0}).", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber));
					break;
				case GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber:
					CheckSpecificTypeWithSameAddressIsRequiredIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Res.GetString("53047D89-BDF7-4BF4-B388-0A3DE81F6206", "You have not entered a Trader ID ({0}) for Premises Address.", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID));
					break;
			}
		}

		void CheckSpecificTypeWithSameAddressIsRequiredIgnoringCountry(ZString requiredType, ZString errorMessage)
		{
			var premisesAddress = Parent.PremisesAddress;
			if (premisesAddress != null)
			{
				if (premisesAddress.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(requiredType).Length == 0)
				{
					Parent.OK_CodeTypeInfo.AddError(errorMessage);
				}
			}
		}

		void CheckSpecificTypeIsRequiredIgnoringCountry(ZString requiredType, ZString errorMessage)
		{
			var cusCodes = Parent.Header.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(requiredType);
			if (cusCodes.Length == 0)
			{
				Parent.OK_CodeTypeInfo.AddError(errorMessage);
			}
		}

		void CheckSpecificTypeWithSameAddressIsRequired(ZString requiredType, ZString errorMessage)
		{
			var premisesAddress = Parent.PremisesAddress;
			if (premisesAddress != null)
			{
				var cusCodes = premisesAddress.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(requiredType, Core.Constants.CountryCodes.Germany);
				if (cusCodes == null)
				{
					Parent.OK_CodeTypeInfo.AddError(errorMessage);
				}
			}
		}
	}
}
