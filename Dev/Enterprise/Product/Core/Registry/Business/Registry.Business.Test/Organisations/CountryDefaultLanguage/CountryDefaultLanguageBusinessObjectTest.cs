using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryDefaultLanguageBusinessObject))]
	sealed class CountryDefaultLanguageBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCountry()
		{
			var collection = new CountryDefaultLanguageBusinessObjectCollection();
			var item = collection.AddNew();
			AssertEquals(ZGuid.Empty, item.CountryPk);
			AssertEquals(string.Empty, item.CountryName);

			var country = Factory.LoadTop1<IRefCountry>(new ZQuery());
			item.CountryPk = country.PK;
			AssertEquals(country.PK, item.CountryPk);
			AssertEquals(country.RN_Desc, item.CountryName);

			item.CountryPk = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, item.CountryPk);
			AssertEquals(ZString.Empty, item.CountryName);
		}

		public void TestValidateCountryPk()
		{
			var collection = new CountryDefaultLanguageBusinessObjectCollection();
			var item1 = collection.AddNew();
			item1.CountryPk = ZGuid.Invalid;
			AssertHasErrors(item1.CountryPkInfo);

			var country1 = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China));
			item1.CountryPk = country1.PK;
			AssertNoErrors(item1.CountryPkInfo);

			var item2 = collection.AddNew();
			item2.CountryPk = country1.PK;
			AssertHasErrors("Default language of 'China' has been specified.", item2.CountryPkInfo);

			var country2 = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			item2.CountryPk = country2.PK;
			AssertNoErrors(item2.CountryPkInfo);
		}

		public void TestDefaultLanguage()
		{
			var collection = new CountryDefaultLanguageBusinessObjectCollection();
			var item = collection.AddNew();
			AssertEquals(string.Empty, item.DefaultLanguage);

			item.DefaultLanguage = "ZH-CN";
			AssertEquals("ZH-CN", item.DefaultLanguage);

			item.DefaultLanguage = string.Empty;
			AssertEquals(string.Empty, item.DefaultLanguage);
		}

		public void TestValidateDefaultLanguage()
		{
			var collection = new CountryDefaultLanguageBusinessObjectCollection();
			var item1 = collection.AddNew();

			item1.DefaultLanguage = string.Empty;
			AssertHasErrors(item1.DefaultLanguageInfo);

			item1.DefaultLanguage = "XYZ";
			AssertHasErrors(item1.DefaultLanguageInfo);

			item1.DefaultLanguage = SharedConstants.Languages.English;
			AssertNoErrors(item1.DefaultLanguageInfo);
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var collection = new CountryDefaultLanguageBusinessObjectCollection(null, Factory);
			var item = collection.AddNew();
			item.CountryPk = Guid.NewGuid();
			item.DefaultLanguage = "EN";

			return item;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
