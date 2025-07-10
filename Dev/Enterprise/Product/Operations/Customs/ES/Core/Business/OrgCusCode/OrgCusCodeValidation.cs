using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class OrgCusCodeValidation : EU.Business.OrgCusCodeValidation
	{
		public OrgCusCodeValidation(OrgCusCode parent) : base(parent)
		{
		}

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();

			var cusRegCode = Parent.OK_CustomsRegNo;
			if (!cusRegCode.IsEmpty &&
				Parent.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID
				&& Parent.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Spain)
			{
				if (ValidateControlledPremisesCodeLength(cusRegCode))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("1ED1CA08-F9D3-4600-B932-BA533026D151", "A Customs Controlled Premises Code must be 12 characters long"));
				}
				else if (ValidateControlledPremisesCodeFirstSecondChars(cusRegCode))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("3B44F6A0-291F-4648-8C92-34AEFEA8C2A8", "A Customs Controlled Premises Code in ES must start with ES"));
				}
				else if (ValidateControlledPremisesCodeThirdChar(cusRegCode))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("33A4A2F8-2326-4F35-9CFE-0D697E2FEA8C", "The third character of a Customs Controlled Premises Code must be 'X', 'I' or 'V'"));
				}
				else if (ValidateControlledPremisesCodeFourthChar(cusRegCode))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("AC6033E2-F9D7-4B06-9446-CCFDD92E8A67", "The fourth character of a Customs Controlled Premises Code must be 'A', 'B', 'C', 'D' or 'E'"));
				}
				else if (ValidateControlledPremisesCodeAlphanumeric(cusRegCode))
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("6B63A41E-696A-42C1-A1CC-F7FFD8DC0A91", "A CCP Code can only have alphanumeric characters"));
				}
			}
		}
		bool ValidateControlledPremisesCodeLength(ZString code) => code.Length != 12;
		bool ValidateControlledPremisesCodeFirstSecondChars(ZString code) => code.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Spain;
		bool ValidateControlledPremisesCodeThirdChar(ZString code)
		{
			var thirdChar = code.SubstringSafe(2, 1);
			return thirdChar != "X" && thirdChar != "I" && thirdChar != "V";
		}
		bool ValidateControlledPremisesCodeFourthChar(ZString code)
		{
			var fourthChar = code.SubstringSafe(3, 1);
			return fourthChar != "A" && fourthChar != "B" && fourthChar != "C" && fourthChar != "D" && fourthChar != "E";
		}
		bool ValidateControlledPremisesCodeAlphanumeric(ZString code) => !code.IsLettersAndNumbersOnlyOrEmpty;
	}
}
