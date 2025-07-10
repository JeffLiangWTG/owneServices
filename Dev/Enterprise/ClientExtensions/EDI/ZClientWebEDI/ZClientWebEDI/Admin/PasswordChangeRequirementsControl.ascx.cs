using System;
using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using static Enterprise.MasterFiles.Business.ContactPasswordValidator;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class PasswordChangeRequirementsControl : BaseUserControl
	{
		int minLengthValidation;

		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		void InitializeComponent()
		{
			DataSourceAssemblyName = "ZClientWebEDI";

			RegisterPasswordRequirementValidationScriptBlock();

			LowerLabel.Text = Res.GetString("68E4D8BF-E6FC-40AF-9D32-D4B965066A2F", "Contains lower-case letter");
			UpperLabel.Text = Res.GetString("16991279-51D3-4406-99FD-BB2EDB822093", "Contains upper-case letter");
			NumberLabel.Text = Res.GetString("7241F0DC-7904-411D-83B4-B1725C7C079D", "Contains number");
			SpecialCharLabel.Text = Res.GetString("CD1A551F-5323-437D-8445-F2001FFF303F", "Contains special character");
			MinLengthLabel.Text = Res.GetString("F114BD2D-28C5-48B3-85A6-8D52969E8A91", "At least {0} characters long", minLengthValidation);
			AtLeastThreeLabel.Text = Res.GetString("FF7C7E39-E071-403A-AB3C-AE4E50FC6225", "Password must contain at least three of the following: uppercase letters, lowercase letters, numbers, symbols, and non-European alphabet characters.");
		}

		#region Password Requirement Validation Script

		void RegisterPasswordRequirementValidationScriptBlock()
		{
			if (!Page.ZClientScript.IsStartupScriptRegistered(GetType(), PasswordRequirementValidationScriptBlockKey))
			{
				Page.ZClientScript.RegisterStartupScript(GetType(), PasswordRequirementValidationScriptBlockKey, PasswordRequirementValidationScriptBlock);
			}
		}

		protected virtual Dictionary<PasswordRequirements, int> GetAllPasswordRequirements => ContactPasswordValidator.GetAllPasswordRequirements;

		public string PasswordRequirementValidationScriptBlock
		{
			get
			{
				var requirements = GetAllPasswordRequirements;
				var hideValidationFieldsOnPageLoadScript = string.Empty;

				string lowerCaseValidationScript;
				string upperCaseValidationScript;
				string numberCaseValidationScript;
				string specialCharacterValidationScript;
				string checkResultScript;

				var minLengthScript = string.Empty;
				var hasMinLengthValidation = requirements.ContainsKey(PasswordRequirements.MinLength);
				if (hasMinLengthValidation)
				{
					minLengthValidation = requirements[PasswordRequirements.MinLength];
					minLengthScript = MinLengthScript(minLengthValidation);
				}

				if (requirements.ContainsKey(PasswordRequirements.ContainsAtLeastThreeOfTheFollowing))
				{
					// run script to hide other fields
					hideValidationFieldsOnPageLoadScript = @"
	function hideRequirementFields() {
		document.getElementById('AtLeastThreeSpan').style.display = ""inline"";
		document.getElementById('LowerSpan').style.display = ""none"";
		document.getElementById('UpperSpan').style.display = ""none"";
		document.getElementById('NumberSpan').style.display = ""none"";
		document.getElementById('SpecialCharSpan').style.display = ""none"";
    }
    window.onload = hideRequirementFields;";

					lowerCaseValidationScript = lowerCaseRegexScript;
					upperCaseValidationScript = upperCaseRegexScript;
					numberCaseValidationScript = numberRegexScript;
					specialCharacterValidationScript = specialCharacterRegexScript;
					checkResultScript = @"
	var passedValidations = lowerResult + upperResult + numberResult + specialResult;

	if (passedValidations >= 3) {
		document.getElementById('AtLeastThreePass').style.display = ""inline"";
		document.getElementById('AtLeastThreeFail').style.display = ""none"";
	} else {
		document.getElementById('AtLeastThreePass').style.display = ""none"";
		document.getElementById('AtLeastThreeFail').style.display = ""inline"";
	}

	var checkResult = document.getElementById('CheckResult');
	if (lengthResult && passedValidations >= 3) {
		checkResult.value = ""pass"";
	} else {
		checkResult.value = ""fail"";
	}";
				}
				else
				{
					lowerCaseValidationScript = requirements.ContainsKey(PasswordRequirements.ContainsLowerCaseLetter) ? lowerCaseScript : string.Empty;
					upperCaseValidationScript = requirements.ContainsKey(PasswordRequirements.ContainsUpperCaseLetter) ? upperCaseScript : string.Empty;
					numberCaseValidationScript = requirements.ContainsKey(PasswordRequirements.ContainsNumber) ? numberScript : string.Empty;
					specialCharacterValidationScript = requirements.ContainsKey(PasswordRequirements.ContainsSpecialCharacter) ? specialCharacterScript : string.Empty;

					checkResultScript = CheckResultScript(lowerCaseValidationScript, upperCaseValidationScript, numberCaseValidationScript, specialCharacterValidationScript, hasMinLengthValidation);
				}

				return string.Format(@"<script type=""text/javascript"">
function checkPasswordRequirements(passwordInputValue) {{
	{0}	{1}	{2}	{3}	{4}	{5}
}}
{6}
</script>", lowerCaseValidationScript,
			upperCaseValidationScript,
			numberCaseValidationScript,
			specialCharacterValidationScript,
			minLengthScript,
			checkResultScript,
			hideValidationFieldsOnPageLoadScript);
			}
		}

		public const string PasswordRequirementValidationScriptBlockKey = "PasswordRequirementValidation";

		string lowerCaseRegexScript => @"
	var lowerRegex = /[a-z]/;
	var lowerResult = lowerRegex.test(passwordInputValue);";

		string lowerCaseScript => lowerCaseRegexScript + @"
	if (lowerResult) {
		document.getElementById('LowerPass').style.display = ""inline"";
		document.getElementById('LowerFail').style.display = ""none"";
	} else {
		document.getElementById('LowerPass').style.display = ""none"";
		document.getElementById('LowerFail').style.display = ""inline"";
	}";

		string upperCaseRegexScript => @"
	var upperRegex = /[A-Z]/;
	var upperResult = upperRegex.test(passwordInputValue);";

		string upperCaseScript => upperCaseRegexScript + @"
	if (upperResult) {
		document.getElementById('UpperPass').style.display = ""inline"";
		document.getElementById('UpperFail').style.display = ""none"";
	} else {
		document.getElementById('UpperPass').style.display = ""none"";
		document.getElementById('UpperFail').style.display = ""inline"";
	}";

		string numberRegexScript => @"
	var numberRegex = /\d/;
	var numberResult = numberRegex.test(passwordInputValue);";

		string numberScript => numberRegexScript + @"
	if (numberResult) {
		document.getElementById('NumberPass').style.display = ""inline"";
		document.getElementById('NumberFail').style.display = ""none"";
	} else {
		document.getElementById('NumberPass').style.display = ""none"";
		document.getElementById('NumberFail').style.display = ""inline"";
	}";

		string specialCharacterRegexScript => @"
	var specialRegex = /[^\w]+/;
	var specialResult = specialRegex.test(passwordInputValue);";

		string specialCharacterScript => specialCharacterRegexScript + @"
	if (specialResult) {
		document.getElementById('SpecialCharPass').style.display = ""inline"";
		document.getElementById('SpecialCharFail').style.display = ""none"";
	} else {
		document.getElementById('SpecialCharPass').style.display = ""none"";
		document.getElementById('SpecialCharFail').style.display = ""inline"";
	}";

		string MinLengthScript(int minLength)
		{
			return string.Format(@"var lengthResult = passwordInputValue.length >= {0};
	if (lengthResult) {{
		document.getElementById('MinLengthPass').style.display = ""inline"";
		document.getElementById('MinLengthFail').style.display = ""none"";
	}} else {{
		document.getElementById('MinLengthPass').style.display = ""none"";
		document.getElementById('MinLengthFail').style.display = ""inline"";
	}}", minLength);
		}

		string CheckResultScript(string lowerCaseValidation, string upperCaseValidation, string numberCaseValidation, string specialCharacterValidation, bool minLengthValidation)
		{
			var checkResultList = new List<string>();
			if (!string.IsNullOrEmpty(lowerCaseValidation))
			{
				checkResultList.Add("lowerResult");
			}
			if (!string.IsNullOrEmpty(upperCaseValidation))
			{
				checkResultList.Add("upperResult");
			}
			if (!string.IsNullOrEmpty(numberCaseValidation))
			{
				checkResultList.Add("numberResult");
			}
			if (!string.IsNullOrEmpty(specialCharacterValidation))
			{
				checkResultList.Add("specialResult");
			}
			if (minLengthValidation)
			{
				checkResultList.Add("lengthResult");
			}

			return string.Format(@"var checkResult = document.getElementById('CheckResult');
	if ({0}) {{
		checkResult.value = ""pass"";
	}} else {{
		checkResult.value = ""fail"";
	}}", string.Join(" && ", checkResultList));
		}

		#endregion
	}
}
