using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryExportStatementSetting))]
	sealed class CountryExportStatementSettingTest : RegistryBusinessObjectTemplateTestCase<CountryExportStatementSetting>
	{
		public void TestValidateCode()
		{
			CountryExportStatementSettingCollection countrySettings = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting countrySetting = countrySettings.AddNew();
			countrySettings.RunPreSaveValidation();
			AssertHasErrorContaining(countrySetting.CountryCodeInfo, "Please enter a Country/Region Code.");

			countrySetting.CountryCode = "AU";
			AssertNoErrorContaining(countrySetting.CountryCodeInfo, "Please enter a Country/Region Code.");

			CountryExportStatementSetting countrySetting2 = countrySettings.AddNew();
			countrySetting2.CountryCode = countrySetting.CountryCode;
			countrySettings.RunPreSaveValidation();
			AssertHasErrorContaining(countrySetting2.CountryCodeInfo, "Duplicate Codes are entered.");

			countrySetting2.CountryCode = "US";
			countrySettings.RunPreSaveValidation();
			AssertEquals("HasErrors", false, countrySetting2.CountryCodeInfo.HasErrors());
		}

		public void TestHasChanges()
		{
			CountryExportStatementSetting bizObj = new CountryExportStatementSetting();
			AssertEquals("Default HasChanges", false, bizObj.HasChanges);

			ExportStatementSetting statement = bizObj.Statements.AddNew();
			statement.Code = "NDR";
			AssertEquals("HasChanges should be true as child has changes", true, bizObj.HasChanges);

			statement.HasChanges = false;
			AssertEquals("HasChanges", false, bizObj.HasChanges);

			statement.HasChanges = true;
			AssertEquals("HasChanges should be true as child has changes", true, bizObj.HasChanges);

			bizObj.HasChanges = false;
			AssertEquals("HasChanges should be false as child should have been set to false", false, bizObj.HasChanges);
		}

		#region Implementation

		protected override CountryExportStatementSetting GetBusinessObjectToClone()
		{
			CountryExportStatementSetting result = new CountryExportStatementSetting();
			result.FillWithValidTestData();
			ExportStatementSetting statement = result.Statements.AddNew();
			statement.FillWithValidTestData();

			return result;
		}

		protected override CountryExportStatementSetting GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
