using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.HK.Business.Testing
{
	[TestedType(typeof(HKDataRegistry))]
	class HKDataRegistryTest : RegistryItemSetTestCaseWithFactory<HKDataRegistry>
	{
		public void TestISACFTPPassiveMode()
		{
			TestRegistryItem(ItemSet.ISACFTPPassiveMode,
				"ISACFTPPassiveMode",
				HKDataRegistry.Categories.Customs_HongKong_FTPSettings,
				"FTP Passive Mode",
				"The FTP Passive Mode for ISAC server. Set to Yes to cause the FTP message process to select passive mode when communicating with GLS (ISAC Traxon).",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				true);
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.ISACFTPServerOutputAddress.CountryFilterPKs);
		}

		public void TestISACFTPServerOutputAddress()
		{
			TestRegistryItem(ItemSet.ISACFTPServerOutputAddress,
					"ISACFTPServerOutputAddress",
					HKDataRegistry.Categories.Customs_HongKong_FTPSettings,
					"FTP Server Output Address",
					"The FTP server output address for ISAC. It should be entered in the following format: ftp://{SERVER_NAME}/{OUTPUT_DIRECTORY}",
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					TextEditorType.TextBox, "");
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.ISACFTPServerOutputAddress.CountryFilterPKs);
		}

		public void TestISACFTPServerOutputAddress_OnUpdateAction_FTPAddress()
		{
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Server>SYDSP-STST-T6.sand.Wtg.Zone</Server>
          <Port>49</Port>
          <ReceiveFolder>/path1/path2/path3</ReceiveFolder>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction("ftp://SYDSP-STST-T6.sand.Wtg.Zone:49/path1/path2/path3", expectedXml);
		}

		public void TestISACFTPServerOutputAddress_OnUpdateAction_FTPEXAddress()
		{
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Server>SYDSP-STST-T6.sand.Wtg.Zone</Server>
          <ReceiveFolder>/path1/</ReceiveFolder>
          <Port>21</Port>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction("ftpex://SYDSP-STST-T6.sand.Wtg.Zone:21/path1/", expectedXml);
		}

		public void TestISACFTPServerOutputAddress_OnUpdateAction_SFTPAddress()
		{
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Server>SYDSP-STST-T6.sand.Wtg.Zone</Server>
          <ReceiveFolder>/path1/</ReceiveFolder>
          <Port>21</Port>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction("sftp://SYDSP-STST-T6.sand.Wtg.Zone/path1/?user=tester", expectedXml);
		}

		public void TestISACFTPServerOutputAddress_OnUpdateAction_FTPSAddress()
		{
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Server>SYDSP-STST-T6.sand.Wtg.Zone</Server>
          <Port>8080</Port>
          <ReceiveFolder>/Path1/Path2</ReceiveFolder>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction("ftps://SYDSP-STST-T6.sand.Wtg.Zone:8080/Path1/Path2", expectedXml);
		}

		public void TestISACFTPServerOutputAddress_OnUpdateAction_WithoutPort()
		{
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Server>SYDSP-STST-T6.sand.Wtg.Zone</Server>
          <Port>21</Port>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction("ftp://SYDSP-STST-T6.sand.Wtg.Zone", expectedXml);

			expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Server>SYDSP-STST-T6.sand.Wtg.Zone</Server>
          <ReceiveFolder>/path1/path2/path3</ReceiveFolder>
          <Port>21</Port>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction("ftp://SYDSP-STST-T6.sand.Wtg.Zone/path1/path2/path3", expectedXml);
		}

		public void TestISACFTPServerOutputAddress_OnUpdateAction_WithInvalidPort()
		{
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Server>SYDSP-STST-T6.sand.Wtg.Zone</Server>
          <Port>21</Port>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction("ftp://SYDSP-STST-T6.sand.Wtg.Zone:", expectedXml);
			AssertISACFTPServerOutputAddressOnUpdateAction("ftp://SYDSP-STST-T6.sand.Wtg.Zone:p", expectedXml);
		}

		public void TestISACFTPServerOutputAddress_OnUpdateAction_WithoutPrefix()
		{
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP />
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertISACFTPServerOutputAddressOnUpdateAction(string.Empty, expectedXml);
			AssertISACFTPServerOutputAddressOnUpdateAction("servername/path1", expectedXml);
		}

		void AssertISACFTPServerOutputAddressOnUpdateAction(string value, string expectedXml)
		{
			HKDataRegistry.Instance.ISACFTPServerOutputAddress_OnUpdateAction(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
			HKDataRegistry.Instance.ISACFTPServerOutputAddress.OnAllValuesSaved();

			var zQuery = new ZDBOnlyQuery(typeof(EDIInterchange));
			zQuery.AddToFilter(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";

			var xml = Factory.LoadTop1<EDIInterchange>(zQuery);

			AssertEquals("The XML should be properly generated with the credential.", expectedXml, xml.EI_BodyText);
		}

		public void TestISACFTPPassword()
		{
			TestRegistryItem(ItemSet.ISACFTPPassword,
					"ISACFTPPassword",
					HKDataRegistry.Categories.Customs_HongKong_FTPSettings,
					"FTP Password",
					"The FTP password for ISAC server",
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					TextEditorType.Password, "");
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.ISACFTPPassword.CountryFilterPKs);
		}

		public void TestISACFTPPassword_OnUpdateAction()
		{
			var registry = HKDataRegistry.Instance;

			registry.ISACFTPPassword_OnUpdateAction(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "pwd");
			registry.ISACFTPPassword.OnAllValuesSaved();

			var zQuery = new ZDBOnlyQuery(typeof(EDIInterchange));
			zQuery.AddToFilter(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";

			var xml = Factory.LoadTop1<EDIInterchange>(zQuery);
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <Password>Password</Password>
          <Port>21</Port>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertNotNull("One message should be generated", xml);
			AssertEquals("The XML should be properly generated with the credential", expectedXml, Regex.Replace(xml.EI_BodyText, "<Password>.*</Password>", "<Password>Password</Password>"));

			registry.ISACFTPPassword_OnUpdateAction(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "");

			registry.ISACFTPPassword.OnAllValuesSaved();

			xml = Factory.LoadTop1<EDIInterchange>(zQuery);
			var expectedEmptyXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP />
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertEquals("The empty XML should be properly generated", expectedEmptyXml, xml.EI_BodyText);
		}

		public void TestISACFTPUserName()
		{
			TestRegistryItem(ItemSet.ISACFTPUserName,
					"ISACFTPUserName",
					HKDataRegistry.Categories.Customs_HongKong_FTPSettings,
					"FTP User Name",
					"The FTP user name for ISAC server",
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					TextEditorType.TextBox, "");
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.ISACFTPUserName.CountryFilterPKs);
		}

		public void TestISACFTPUserName_OnUpdateAction()
		{
			var registry = HKDataRegistry.Instance;

			registry.ISACFTPUserName_OnUpdateAction(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "VWG");
			registry.ISACFTPUserName.OnAllValuesSaved();

			var zQuery = new ZDBOnlyQuery(typeof(EDIInterchange));
			zQuery.AddToFilter(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";

			var xml = Factory.LoadTop1<EDIInterchange>(zQuery);
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP>
          <UserName>VWG</UserName>
          <Port>21</Port>
        </FTP>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertNotNull("One message should be generated", xml);
			AssertEquals("The XML should be properly generated with the credential", expectedXml, xml.EI_BodyText);

			registry.ISACFTPUserName_OnUpdateAction(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			registry.ISACFTPUserName.OnAllValuesSaved();

			xml = Factory.LoadTop1<EDIInterchange>(zQuery);
			var expectedEmptyXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP />
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertEquals("The empty XML should be properly generated", expectedEmptyXml, xml.EI_BodyText);
		}

		public void TestCosacAgentCode()
		{
			TestRegistryItem(ItemSet.CosacAgentCode,
					"CosacAgentCode",
					HKDataRegistry.Categories.Customs_HongKong,
					"COSAC Agent Code",
					"The agent party id used for ISAC",
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue,
					TextEditorType.TextBox, "");
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.CosacAgentCode.CountryFilterPKs);
		}

		public void TestHKTraxonSenderID()
		{
			TestRegistryItem(ItemSet.HKTraxonSenderID,
					"Traxon Sender ID",
					HKDataRegistry.Categories.Customs_HongKong,
					"ISAC Sender ID",
					"",
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue,
					TextEditorType.TextBox, "");
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.HKTraxonSenderID.CountryFilterPKs);
		}

		public void TestHKTraxonSenderID_OnUpdateAction()
		{
			var registry = HKDataRegistry.Instance;

			registry.HKTraxonSenderID_OnUpdateAction(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "SenderId");
			registry.HKTraxonSenderID.OnAllValuesSaved();

			var zQuery = new ZDBOnlyQuery(typeof(EDIInterchange));
			zQuery.AddToFilter(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";

			var xml = Factory.LoadTop1<EDIInterchange>(zQuery);
			var expectedXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"" Reference=""SenderId"">
        <FTP />
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertNotNull("One message should be generated", xml);
			AssertEquals("The XML should be properly generated with the credential", expectedXml, xml.EI_BodyText);

			registry.HKTraxonSenderID_OnUpdateAction(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			registry.HKTraxonSenderID.OnAllValuesSaved();

			xml = Factory.LoadTop1<EDIInterchange>(zQuery);
			var expectedEmptyXml = $@"<Configuration {XmlAttrHelper.GetXmlns()} Name=""GLSHKConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""PIMA"">
        <FTP />
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertEquals("The empty XML should be properly generated", expectedEmptyXml, xml.EI_BodyText);
		}

		public void TestHKTraxonRecipientReferencePassword()
		{
			TestRegistryItem(ItemSet.HKTraxonRecipientReferencePassword,
				"Traxon Recipient Reference",
				HKDataRegistry.Categories.Customs_HongKong,
				"ISAC Recipient Reference Password",
				"",
				RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue,
					TextEditorType.TextBox, "");
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.HKTraxonRecipientReferencePassword.CountryFilterPKs);
		}

		public void TestHKTraxonOutputDirectory()
		{
			TestGenericRegistryItem(ItemSet.HKTraxonOutputDirectory,
					"Traxon Output Directory",
					HKDataRegistry.Categories.Customs_HongKong,
					"ISAC Output Directory",
					"",
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue,
					"");
			AssertEquals("EditorInfo.EditorType", TextEditorType.DirectoryBrowser, ((TextRegistryEditorInfo)ItemSet.HKTraxonOutputDirectory.EditorInfo).EditorType);
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.HKTraxonOutputDirectory.CountryFilterPKs);
		}

		public void TestGroupToCopyTraxonResponseEmailsTo()
		{
			TestRegistryItem(ItemSet.GroupToCopyTraxonResponseEmailsTo,
					"Group To Copy Traxon Response Emails To",
					HKDataRegistry.Categories.Customs_HongKong,
					"Group To Copy ISAC Response Emails To", "",
					RegistryStorageFlags.Company,
					RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
					RegistryFindBoxCollection.GlbGroup,
					Guid.Empty);
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.GroupToCopyTraxonResponseEmailsTo.CountryFilterPKs);
		}

		public void TestSendOtherCustomsInformation()
		{
			var collection = new CountryListCollection(Factory);
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();

			var country1 = collection.AddNew();
			var country2 = collection.AddNew();

			country1.CountryPK = guid1;
			country2.CountryPK = guid2;

			using (ItemSet.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { guid1, guid2 }))
			{
				AssertEquals("SendOtherCustomsInformation", ItemSet.SendOtherCustomsInformation.Name);
				AssertEquals("Customs/Country or Region Specific/Hong Kong", ItemSet.SendOtherCustomsInformation.Category);
				AssertEquals("Send Other Customs Information", ItemSet.SendOtherCustomsInformation.Caption);
				AssertEquals(@"When ""Send Other Customs Information"" is true. The system will send selected additional information in the ISAC message for exports including Consignee/Shipper/Also Notify Contact Details and Trader Identification Number providing the country of discharge of the Consol is included in this list of countries. If you include HK in this list, the additional information will be sent for imports also. The decision to send is based solely on the countries in this list matching the port of discharge on a Consol.", ItemSet.SendOtherCustomsInformation.Hint);
				AssertEquals(RegistryStorageFlags.Company, ItemSet.SendOtherCustomsInformation.Storage);
				AssertContainsExactElementsInAnyOrder(new[] { guid1, guid2 }, ItemSet.SendOtherCustomsInformation.Value);
				AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.SendOtherCustomsInformation.CountryFilterPKs);
			}
		}

		public void TestSendHSCode()
		{
			TestRegistryItem(ItemSet.SendHsCode,
				"SendHsCode",
				HKDataRegistry.Categories.Customs_HongKong,
				"Send HS Code",
				"If set to \"Yes\" the system will generate the HS Commodity Code segments in the ISAC Message.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				false);
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.SendHsCode.CountryFilterPKs);
		}

		public void TestTruncateSupplementaryCustomsInformation()
		{
			TestRegistryItem(ItemSet.TruncateOtherCustomsInformation,
				"TruncateOtherCustomsInformation",
				HKDataRegistry.Categories.Customs_HongKong,
				"Truncate Supplementary Customs Information",
				"If set to \"Yes\" the system will truncate the Supplementary Customs Information at 35 characters.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true);
			AssertEquals(RegistryItemSet.CountryFilterPKs.HongKong, ItemSet.TruncateOtherCustomsInformation.CountryFilterPKs);
		}

		GlbCompany Company
		{
			get
			{
				if (company == null)
				{
					company = Factory.NewWithValidTestData<GlbCompany>();
					company.GC_Code = "ABC";

					Factory.Save();
				}

				return company;
			}
		}
		GlbCompany company;
	}
}
