using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business;

public class EDIReleaseValidator
{
	#region Constructors

	public EDIReleaseValidator(JobDeclaration declaration)
		: this(declaration.CA_ServiceOption, declaration.CA_AssesmentOption)
	{
	}

	public EDIReleaseValidator(ZString serviceOption, ZString assessmentOption)
	{
		this.serviceOption = serviceOption;
		this.assessmentOption = assessmentOption;
	}

	#endregion

	public bool IsCombinationValid()
	{
		var isValid = ValidCombinations.Any(pair => pair[0] == serviceOption && pair[1] == assessmentOption);
		if (!isValid)
		{
			LastErrorMessage = Res.GetString("559619ac-dee4-4ad0-bc4b-45d0ca4a6405", "Invalid combination of Service Option and Assessment Option.");
		}
		return isValid;
	}

	public bool AllOptionsSpecifiedAndValid()
	{
		var isValid = new ACROSSServiceOptions().ContainsCode(serviceOption) && new AssessmentOptions().ContainsCode(assessmentOption);
		if (!isValid)
		{
			LastErrorMessage = Res.GetString("ddb129bd-f3ab-4733-92cc-4b1e56a80eee", "Both a valid Service Option and Assessment Option should be specified.");
		}
		return isValid;
	}

	public string LastErrorMessage { get; private set; }

	#region Implementation

	IEnumerable<string[]> ValidCombinations
	{
		get
		{
			return validCombinations ?? (validCombinations = new[]
			{
				new [] { ACROSSServiceOptions.Codes.ReplaceRMDwithAQ, AssessmentOptions.Codes.AppraisalQualityData },
				new [] { ACROSSServiceOptions.Codes.PARS, AssessmentOptions.Codes.AQtoFollow },
				new [] { ACROSSServiceOptions.Codes.PARS, AssessmentOptions.Codes.AppraisalQualityData },
				new [] { ACROSSServiceOptions.Codes.IID, AssessmentOptions.Codes.AppraisalQualityData },
				new [] { ACROSSServiceOptions.Codes.CSA, AssessmentOptions.Codes.AppraisalQualityData }
			});
		}
	}

	string[][] validCombinations;
	readonly ZString serviceOption;
	readonly ZString assessmentOption;

	#endregion
}
