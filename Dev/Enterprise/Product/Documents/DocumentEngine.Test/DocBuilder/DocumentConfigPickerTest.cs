using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class DocumentConfigPickerTest : TestCaseWithFactory
	{
		public void TestIgnoreUserCustomizedTemplatesAndConfigurations()
		{
			var client = Factory.New<OrgHeader>();
			var company = GlbCompany.CurrentCompany;

			var documentCommand = Factory.New<DocumentCommand>();
			var document = documentCommand.Documents.AddNew();

			var picker = new DocumentConfigPicker(document, true);

			AssertEquals("picker.GetDocumentConfig(client, company)", null, picker.GetDocumentConfig(client, company));

			var systemConfig = document.DocConfigs.AddNew();
			systemConfig.S3_IsSystem = ZBool.True;
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			var enterpriseConfig = document.DocConfigs.AddNew();
			enterpriseConfig.S3_IsSystem = ZBool.False;
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			var companyConfig = document.DocConfigs.AddNew();
			companyConfig.S3_IsSystem = ZBool.False;
			companyConfig.S3_GC = company.PK;
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			var clientConfig = document.DocConfigs.AddNew();
			clientConfig.S3_IsSystem = ZBool.False;
			clientConfig.S3_OH = client.PK;
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			var exactConfig = document.DocConfigs.AddNew();
			exactConfig.S3_IsSystem = ZBool.False;
			exactConfig.S3_GC = company.PK;
			exactConfig.S3_OH = client.PK;
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(exactConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(clientConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(companyConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(enterpriseConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(systemConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", null, picker.GetDocumentConfig(client, company));
		}

		public void TestGetDocumentConfig()
		{
			var client = Factory.New<OrgHeader>();
			var company = GlbCompany.CurrentCompany;

			var documentCommand = Factory.New<DocumentCommand>();
			var document = documentCommand.Documents.AddNew();

			var picker = new DocumentConfigPicker(document, false);

			AssertEquals("picker.GetDocumentConfig(client, company)", null, picker.GetDocumentConfig(client, company));

			var systemConfig = document.DocConfigs.AddNew();
			systemConfig.S3_IsSystem = ZBool.True;
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			var enterpriseConfig = document.DocConfigs.AddNew();
			enterpriseConfig.S3_IsSystem = ZBool.False;
			AssertEquals("picker.GetDocumentConfig(client, company)", enterpriseConfig, picker.GetDocumentConfig(client, company));

			var companyConfig = document.DocConfigs.AddNew();
			companyConfig.S3_IsSystem = ZBool.False;
			companyConfig.S3_GC = company.PK;
			AssertEquals("picker.GetDocumentConfig(client, company)", companyConfig, picker.GetDocumentConfig(client, company));

			var clientConfig = document.DocConfigs.AddNew();
			clientConfig.S3_IsSystem = ZBool.False;
			clientConfig.S3_OH = client.PK;
			AssertEquals("picker.GetDocumentConfig(client, company)", clientConfig, picker.GetDocumentConfig(client, company));

			var exactConfig = document.DocConfigs.AddNew();
			exactConfig.S3_IsSystem = ZBool.False;
			exactConfig.S3_GC = company.PK;
			exactConfig.S3_OH = client.PK;
			AssertEquals("picker.GetDocumentConfig(client, company)", exactConfig, picker.GetDocumentConfig(client, company));

			var templateSystemConfig = document.DocConfigs.AddNew();
			templateSystemConfig.S3_Description = "System Template";
			templateSystemConfig.S3_IsSystem = true;
			templateSystemConfig.S3_IsTemplate = true;
			AssertNotEquals("picker.GetDocumentConfig(client, company)", templateSystemConfig, picker.GetDocumentConfig(client, company));

			var templateSystemConfig2 = document.DocConfigs.AddNew();
			templateSystemConfig2.S3_Description = "System Template";
			templateSystemConfig2.S3_IsSystem = true;
			templateSystemConfig2.S3_IsTemplate = true;
			AssertNotEquals("picker.GetDocumentConfig(client, company)", templateSystemConfig2, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(exactConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", clientConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(clientConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", companyConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(companyConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", enterpriseConfig, picker.GetDocumentConfig(client, company));

			document.DocConfigs.RemoveAndDelete(enterpriseConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", systemConfig, picker.GetDocumentConfig(client, company));

			systemConfig.S3_ExcludedFromDocPack = true;
			templateSystemConfig.S3_ExcludedFromDocPack = true;
			templateSystemConfig2.S3_ExcludedFromDocPack = true;
			exactConfig = document.DocConfigs.AddNew();
			exactConfig.S3_IsSystem = ZBool.False;
			exactConfig.S3_GC = company.PK;
			exactConfig.S3_OH = client.PK;
			document.DocConfigs.Add(exactConfig);
			AssertEquals("picker.GetDocumentConfig(client, company)", null, picker.GetDocumentConfig(client, null));

			document.DocConfigs.RemoveAndDeleteAll();
			AssertEquals("picker.GetDocumentConfig(client, company)", null, picker.GetDocumentConfig(client, company));
		}
	}
}
