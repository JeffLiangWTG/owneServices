using System.Collections.Generic;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class PreferenceAndStyleTestCombination
{
	PreferenceAndStyleTestCombination(string propertyDescriptionForErrorMessage, string primaryPreference, string style, string differentPropertyDescriptionForErrorMessageInCaseOfStartingWith1OrNull)
	{
		PrimaryPreference = primaryPreference;
		Style = style;
		ErrorMessage = (primaryPreferenceIsEmptyOrStartsWith1 ? differentPropertyDescriptionForErrorMessageInCaseOfStartingWith1OrNull : propertyDescriptionForErrorMessage) + $" is required when {(primaryPreferenceIsEmpty ? "no Preference is used" : "Preference is " + primaryPreference)} for Declaration Type {style}";
	}

	public string PrimaryPreference { get; }

	public string Style { get; }

	public string ErrorMessage { get; }

	public static IEnumerable<PreferenceAndStyleTestCombination> CreateCombinations(string propertyDescriptionForErrorMessage, List<string> primaryPreferenceValues, List<string> styleValues, string differentPropertyDescriptionForErrorMessageInCaseOfNull = null)
	{
		foreach (var primaryPreferenceValue in primaryPreferenceValues)
		{
			foreach (var styleValue in styleValues)
			{
				yield return new PreferenceAndStyleTestCombination(propertyDescriptionForErrorMessage, primaryPreferenceValue, styleValue, differentPropertyDescriptionForErrorMessageInCaseOfNull ?? propertyDescriptionForErrorMessage);
			}
		}
	}

	bool primaryPreferenceIsEmpty => string.IsNullOrEmpty(PrimaryPreference);
	bool primaryPreferenceIsEmptyOrStartsWith1 => string.IsNullOrEmpty(PrimaryPreference) || PrimaryPreference.StartsWith("1");
}
