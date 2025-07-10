//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACountryPreferenceValidation
//
//    This class should be used for overriding validation in AutoCACountryPreferenceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CACountryPreferenceValidation : AutoCACountryPreferenceValidation
	{
		public CACountryPreferenceValidation(AutoCACountryPreference parent) : base(parent)
		{
		}

		protected override void CheckCA_CountryCode()
		{
			base.CheckCA_CountryCode();

			if (Parent.CA_CountryCode.Length != 2)
			{
				Parent.CA_CountryCodeInfo.AddError(Res.GetString("fac8eebd-ec69-49ce-8014-e453a5785e9a", "Country/Region Code should be composed of 2 characters."));
			}
			else
			{
				var query = new ZQuery();
				query.AddToFilter(CACountryPreferenceSchema.CA_CountryCode, Parent.CA_CountryCode);
				query.AddToFilter(CACountryPreferenceSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var country = Parent.Factory.LoadTop1<CACountryPreference>(query);
				if (country != null)
				{
					Parent.CA_CountryCodeInfo.AddError(Res.GetString("cc3ba87c-ba07-4522-87a8-43dff9f5d73f", "Country/Region Code is not unique."));
				}
			}
		}
	}
}
