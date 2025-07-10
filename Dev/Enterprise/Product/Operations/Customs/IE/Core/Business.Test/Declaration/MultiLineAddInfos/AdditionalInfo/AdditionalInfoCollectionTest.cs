using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	sealed class AdditionalInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewType()
		{
			AssertType<AdditionalInfo>(additionalInfos.AddNew());
		}

		public void TestContainsAdditionalInfoOfCode()
		{
			var additionalInfo = additionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalInfo.CSI_Code = SupportingDocumentCodes._N740;
			additionalInfo = additionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalInfo.CSI_Code = SupportingDocumentCodes._N741;
			AssertEquals("TRA, { N325, N740 }", true, additionalInfos.ContainsAdditionalInfoOfCode(AdditionalInfoSubTypeList.Codes.TransportDocument, new ZString[] { SupportingDocumentCodes._N325, SupportingDocumentCodes._N740 }));
			AssertEquals("INF, { N325, N740 }", false, additionalInfos.ContainsAdditionalInfoOfCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation, new ZString[] { SupportingDocumentCodes._N325, SupportingDocumentCodes._N740 }));
			AssertEquals("TRA, { N325 }", false, additionalInfos.ContainsAdditionalInfoOfCode(AdditionalInfoSubTypeList.Codes.TransportDocument, new ZString[] { SupportingDocumentCodes._N325 }));
		}

		public void TestSetDefaultsForNewChild_UCC5AndImport()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				var additionalInfo = additionalInfos.AddNew();
				AssertEquals("INF", additionalInfo.CSI_SubType);
			}
		}

		public void TestSetDefaultsForNewChild()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				var additionalInfo = additionalInfos.AddNew();
				AssertEquals(AdditionalInfoSubTypeList.Codes.AdditionalInformation, additionalInfo.CSI_SubType);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var additionalInfo = additionalInfos.AddNew();
				AssertEquals(string.Empty, additionalInfo.CSI_SubType);
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest() => additionalInfos;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			additionalInfos = declaration.AdditionalInfos;
		}
		JobDeclaration declaration;
		AdditionalInfoCollection additionalInfos;
	}
}
