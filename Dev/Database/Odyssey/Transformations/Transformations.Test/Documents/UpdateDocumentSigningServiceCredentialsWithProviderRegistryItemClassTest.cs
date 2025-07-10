using System;
using System.Text;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Documents;

[TestedType(typeof(UpdateDocumentSigningServiceCredentialsWithProviderRegistryItemClass))]
class UpdateDocumentSigningServiceCredentialsWithProviderRegistryItemClassTest : RegistryDataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new UpdateDocumentSigningServiceCredentialsWithProviderRegistryItemClass();
	}

	const string SdName = "SigningServiceProviderAPICredentials";

	Guid frCompany;
	Guid ptCompany;
	Guid inCompany;
	Guid itCompany;
	Guid deCompany;
	Guid inactiveCompany;
	Guid ptBranch;
	Guid notExistOwner;

	protected override void PrepareTestData()
	{
		var helper = new TestDbHelper(TestConnection);

		frCompany = helper.InsertCompany("CFR", "French Company", "EUR", "FR", true, true);
		ptCompany = helper.InsertCompany("CPT", "Portugal Company", "EUR", "PT", true, true);
		inCompany = helper.InsertCompany("CIN", "Indian Company", "USD", "IN", true, true);
		deCompany = helper.InsertCompany("CDE", "German Company", "EUR", "DE", true, true);
		itCompany = helper.InsertCompany("CIT", "Italian Company", "EUR", "IT", true, true);
		inactiveCompany = InsertInactiveCompany("CI2", "Inactive Company", "EUR", "IT", true, true);
		ptBranch = InsertBranchWithCountry("BPT", "Portugal Branch", ptCompany, "PT");
		notExistOwner = Guid.NewGuid();

		Helper.InsertStmDataRow(SdName, frCompany, "BIN", Encoding.Unicode.GetBytes(notValidXml));
		Helper.InsertStmDataRow(SdName, ptCompany, "BIN", Encoding.Unicode.GetBytes(currentXml));
		Helper.InsertStmDataRow(SdName, inCompany, "BIN", Encoding.Unicode.GetBytes(currentXml));
		Helper.InsertStmDataRow(SdName, deCompany, "BIN", Encoding.Unicode.GetBytes(currentXml));
		Helper.InsertStmDataRow(SdName, itCompany, "BIN", null);
		Helper.InsertStmDataRow(SdName, inactiveCompany, "BIN", Encoding.Unicode.GetBytes(currentXml));
		Helper.InsertStmDataRow(SdName, ptBranch, "BIN", Encoding.Unicode.GetBytes(currentXml));
		Helper.InsertStmDataRow(SdName, notExistOwner, "BIN", Encoding.Unicode.GetBytes(currentXml));

		Guid InsertInactiveCompany(string code, string name, string currency, string country, bool isReciprocal, bool isGSTRegistered)
		{
			var lastPK = Guid.NewGuid();
			helper.Insert("GlbCompany", new
			{
				GC_PK = lastPK,
				GC_Code = code,
				GC_Name = name,
				GC_RX_NKLocalCurrency = currency,
				GC_RN_NKCountryCode = country,
				GC_IsReciprocal = isReciprocal,
				GC_IsGSTRegistered = isGSTRegistered,
				GC_IsActive = false
			});
			return lastPK;
		}

		Guid InsertBranchWithCountry(string code, string name, Guid companyPk, string country)
		{
			var lastPK = Guid.NewGuid();
			helper.Insert("GlbBranch", new
			{
				GB_PK = lastPK,
				GB_Code = code,
				GB_BranchName = name,
				GB_GC = companyPk,
				GB_RN_NKCountryCode = country
			});
			return lastPK;
		}
	}

	protected override void AssertTransformationResults()
	{
		var binaryValue1 = Helper.GetStmDataValue(SdName, frCompany);
		AssertNotNull("Should have a not NULL binary value.", binaryValue1);
		AssertEquals("Registry item without SigningServiceProviderAPICredentials should not be modified.", notValidXml, Encoding.Unicode.GetString(binaryValue1));

		var binaryValue2 = Helper.GetStmDataValue(SdName, ptCompany);
		AssertNotNull("Should have a not NULL binary value.", binaryValue2);
		var ptExpectedXml = CreateXmlWithProvider("DGS");
		AssertEqualsIgnoreLineBreaks("Registry item with SigningServiceProviderAPICredentials should be updated correctly for Portugal company.", ptExpectedXml, Encoding.Unicode.GetString(binaryValue2));

		var binaryValue3 = Helper.GetStmDataValue(SdName, inCompany);
		AssertNotNull("Should have a not NULL binary value.", binaryValue3);
		var inExpectedXml = CreateXmlWithProvider("EMD");
		AssertEqualsIgnoreLineBreaks("Registry item with SigningServiceProviderAPICredentials should be updated correctly for Indian company.", inExpectedXml, Encoding.Unicode.GetString(binaryValue3));

		var binaryValue4 = Helper.GetStmDataValue(SdName, deCompany);
		AssertNull("Should have a NULL binary value.", binaryValue4);

		var binaryValue5 = Helper.GetStmDataValue(SdName, itCompany);
		AssertNull("Should have a NULL binary value.", binaryValue5);

		var binaryValue6 = Helper.GetStmDataValue(SdName, inactiveCompany);
		AssertNull("Should have a NULL binary value and no Dictionary Error raised.", binaryValue6);

		var binaryValue7 = Helper.GetStmDataValue(SdName, ptBranch);
		AssertNotNull("Should have a not NULL binary value.", binaryValue7);
		AssertEqualsIgnoreLineBreaks("Registry item with SigningServiceProviderAPICredentials should be updated correctly for Portugal branch.", ptExpectedXml, Encoding.Unicode.GetString(binaryValue7));

		var binaryValue8 = Helper.GetStmDataValue(SdName, notExistOwner);
		AssertNull("Should have a NULL binary value.", binaryValue8);
	}

	const string notValidXml = "This xml doesn't contain any node";

	const string currentXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<DocumentSigningServiceCredentialsConfiguration>
<ClientID>client</ClientID>
<AccessKey>LVhg0ApcghlbpIXfpxzd4Q==</AccessKey>
<KeyID>ZGd3Vq1CoCpJ48H3XxJxwQ==</KeyID>
</DocumentSigningServiceCredentialsConfiguration>";

	string CreateXmlWithProvider(string providerCode)
	{
		return $@"<?xml version=""1.0"" encoding=""utf-16""?>
<DocumentSigningServiceCredentialsWithProviderConfiguration>
<ProviderCode>{providerCode}</ProviderCode>
<ClientID>client</ClientID>
<AccessKey>LVhg0ApcghlbpIXfpxzd4Q==</AccessKey>
<KeyID>ZGd3Vq1CoCpJ48H3XxJxwQ==</KeyID>
</DocumentSigningServiceCredentialsWithProviderConfiguration>";
	}
}
