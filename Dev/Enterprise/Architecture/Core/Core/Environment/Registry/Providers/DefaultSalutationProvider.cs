namespace Enterprise.ZArchitecture.Environment
{
	public static class DefaultSalutationProvider
	{
		public static string GetDefaultSalutation(string language, string gender)
		{
			string salutation = string.Empty;
			if (!string.IsNullOrEmpty(language))
			{
				if (string.IsNullOrEmpty(gender))
				{
					salutation = Enterprise.Core.Constants.DefaultSalutations.DefaultSalutation.ToString(language);
				}
				else
				{
					salutation = (gender == Enterprise.Core.Constants.SalutationGenders.Man || gender == Enterprise.Core.Constants.Genders.Man) ?
						Enterprise.Core.Constants.DefaultSalutations.DearMale.ToString(language) : Enterprise.Core.Constants.DefaultSalutations.DearFemale.ToString(language);
				}
			}
			return salutation;
		}
	}
}
