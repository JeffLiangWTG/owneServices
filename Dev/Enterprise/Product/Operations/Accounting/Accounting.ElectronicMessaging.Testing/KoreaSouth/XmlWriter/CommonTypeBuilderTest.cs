using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class CommonTypeBuilderTest : TestCaseWithFactory
	{
		public void TestBuildTaxInvoiceFreeTextType()
		{
			var rendomText1 = MasterFilesTestHelper.GetRandomString(97);
			var rendomText2 = MasterFilesTestHelper.GetRandomString(95);

			AssertBuildTaxInvoiceFreeTextType(null, @"<TaxInvoiceFreeText xmlns=""TestNameSpace"" />");
			AssertBuildTaxInvoiceFreeTextType("AAA", @"<TaxInvoiceFreeText xmlns=""TestNameSpace"">AAA</TaxInvoiceFreeText>");
			AssertBuildTaxInvoiceFreeTextType(rendomText1, $@"<TaxInvoiceFreeText xmlns=""TestNameSpace"">{rendomText1}</TaxInvoiceFreeText>");
			AssertBuildTaxInvoiceFreeTextType(rendomText1 + "AAA", $@"<TaxInvoiceFreeText xmlns=""TestNameSpace"">{rendomText1}AAA</TaxInvoiceFreeText>");
			AssertBuildTaxInvoiceFreeTextType(rendomText1 + "AAAA", $@"<TaxInvoiceFreeText xmlns=""TestNameSpace"">{rendomText1}...</TaxInvoiceFreeText>");
			AssertBuildTaxInvoiceFreeTextType(rendomText1 + "AAAAA", $@"<TaxInvoiceFreeText xmlns=""TestNameSpace"">{rendomText1}...</TaxInvoiceFreeText>");
			AssertBuildTaxInvoiceFreeTextType(rendomText2 + "  AAAA", $@"<TaxInvoiceFreeText xmlns=""TestNameSpace"">{rendomText2}...</TaxInvoiceFreeText>");
			AssertBuildTaxInvoiceFreeTextType(rendomText2 + " AAAAA", $@"<TaxInvoiceFreeText xmlns=""TestNameSpace"">{rendomText2} A...</TaxInvoiceFreeText>");

			void AssertBuildTaxInvoiceFreeTextType(string value, string expectedXML)
			{
				var builder = new CommonTypeBuilder("TestNameSpace");
				XmlComparison.CompareAndAssertXml(expectedXML, builder.BuildTaxInvoiceFreeTextType("TaxInvoiceFreeText", value).ToString());
			}
		}

		public void TestBuildTaxInvoiceDateType()
		{
			var expectedXmlResult = @"<TaxInvoiceDate xmlns=""TestNameSpace"">20220301</TaxInvoiceDate>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoiceDateType("TaxInvoiceDate", new System.DateTime(2022, 03, 01)).ToString());
		}

		public void TestBuildExchangedIssueDateTimeType()
		{
			var expectedXmlResult = @"<ExchangedIssueDateTime xmlns=""TestNameSpace"">20220301121212</ExchangedIssueDateTime>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildExchangedIssueDateTimeType("ExchangedIssueDateTime", new System.DateTime(2022, 03, 01, 12, 12, 12)).ToString());
		}

		public void TestBuildTaxInvoicePartyIDType()
		{
			var expectedXmlResult = @"<TaxInvoicePartyID xmlns=""TestNameSpace"">AAA</TaxInvoicePartyID>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePartyIDType("TaxInvoicePartyID", "AAA").ToString(), "Occurs : 1..1");

			expectedXmlResult = @"<TaxInvoicePartyID xmlns=""TestNameSpace"" />";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePartyIDType("TaxInvoicePartyID", null).ToString(), "Occurs : 1..1");
		}

		public void TestBuildTaxInvoicePartyTextType()
		{
			var expectedXmlResult = @"<TaxInvoicePartyText xmlns=""TestNameSpace"">AAA</TaxInvoicePartyText>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePartyTextType("TaxInvoicePartyText", "AAA").ToString(), "Occurs : 0..1");

			AssertNull("Occurs : 0..1", builder.BuildTaxInvoicePartyTextType("TaxInvoicePartyText", null));
		}

		public void TestBuildTaxInvoiceBusinessTypeCodeType()
		{
			var expectedXmlResult = @"<TaxInvoiceBusinessTypeCode xmlns=""TestNameSpace"">AAA</TaxInvoiceBusinessTypeCode>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoiceBusinessTypeCodeType("TaxInvoiceBusinessTypeCode", "AAA").ToString());
		}

		public void TestBuildTaxInvoicePartyNameTextType()
		{
			var expectedXmlResult = @"<TaxInvoicePartyNameText xmlns=""TestNameSpace"">AAA</TaxInvoicePartyNameText>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePartyNameTextType("TaxInvoicePartyNameText", "AAA").ToString(), "Occurs : 1..1");

			expectedXmlResult = @"<TaxInvoicePartyNameText xmlns=""TestNameSpace"" />";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePartyNameTextType("TaxInvoicePartyNameText", null).ToString(), "Occurs : 1..1");
		}

		public void TestBuildTaxInvoiceAmountNoFracType()
		{
			var builder = new CommonTypeBuilder("TestNameSpace");

			var expectedXmlResult = @"<TaxInvoiceAmountNoFrac xmlns=""TestNameSpace"">123</TaxInvoiceAmountNoFrac>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoiceAmountNoFracType("TaxInvoiceAmountNoFrac", 123).ToString());
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoiceAmountNoFracType("TaxInvoiceAmountNoFrac", 123.00).ToString());

			expectedXmlResult = @"<TaxInvoiceAmountNoFrac xmlns=""TestNameSpace"">123.4</TaxInvoiceAmountNoFrac>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoiceAmountNoFracType("TaxInvoiceAmountNoFrac", 123.4).ToString());
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoiceAmountNoFracType("TaxInvoiceAmountNoFrac", 123.40).ToString());
		}

		public void TestBuildTaxInvoicePaidAmountType()
		{
			var builder = new CommonTypeBuilder("TestNameSpace");

			var expectedXmlResult = @"<PaidAmount xmlns=""TestNameSpace"">123</PaidAmount>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePaidAmountType("PaidAmount", 123).ToString());
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePaidAmountType("PaidAmount", 123.00).ToString());

			expectedXmlResult = @"<PaidAmount xmlns=""TestNameSpace"">123.4</PaidAmount>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePaidAmountType("PaidAmount", 123.4).ToString());
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildTaxInvoicePaidAmountType("PaidAmount", 123.40).ToString());
		}

		public void TestBuildSpecifiedOrganizationType()
		{
			var expectedXmlResult = @"<SpecifiedOrganization xmlns=""TestNameSpace"">
  <TaxRegistrationID>0123</TaxRegistrationID>
</SpecifiedOrganization>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildSpecifiedOrganizationType("SpecifiedOrganization", "0123").ToString(), "Occurs : 0..1");

			AssertNull("Occurs : 0..1", builder.BuildSpecifiedOrganizationType("SpecifiedOrganization", null));
		}

		public void TestBuildSpecifiedPersonType()
		{
			var rendomText1 = MasterFilesTestHelper.GetRandomString(97);
			var rendomText2 = MasterFilesTestHelper.GetRandomString(95);

			AssertBuildSpecifiedPersonType(null, "<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText />\r\n</SpecifiedPerson>");
			AssertBuildSpecifiedPersonType("AAA", "<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText>AAA</NameText>\r\n</SpecifiedPerson>");
			AssertBuildSpecifiedPersonType(rendomText1, $"<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText>{rendomText1}</NameText>\r\n</SpecifiedPerson>");
			AssertBuildSpecifiedPersonType(rendomText1 + "AAA", $"<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText>{rendomText1}AAA</NameText>\r\n</SpecifiedPerson>");
			AssertBuildSpecifiedPersonType(rendomText1 + "AAAA", $"<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText>{rendomText1}...</NameText>\r\n</SpecifiedPerson>");
			AssertBuildSpecifiedPersonType(rendomText1 + "AAAAA", $"<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText>{rendomText1}...</NameText>\r\n</SpecifiedPerson>");
			AssertBuildSpecifiedPersonType(rendomText2 + "  AAAA", $"<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText>{rendomText2}...</NameText>\r\n</SpecifiedPerson>");
			AssertBuildSpecifiedPersonType(rendomText2 + " AAAAA", $"<SpecifiedPerson xmlns=\"TestNameSpace\">\r\n  <NameText>{rendomText2} A...</NameText>\r\n</SpecifiedPerson>");

			void AssertBuildSpecifiedPersonType(string value, string expectedXML)
			{
				var builder = new CommonTypeBuilder("TestNameSpace");
				XmlComparison.CompareAndAssertXml(expectedXML, builder.BuildSpecifiedPersonType("SpecifiedPerson", value).ToString(), "Occurs : 1..1");
			}
		}

		public void TestBuildSpecifiedAddressType()
		{
			var expectedXmlResult = @"<SpecifiedAddress xmlns=""TestNameSpace"">
  <LineOneText>AAA</LineOneText>
</SpecifiedAddress>";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildSpecifiedAddressType("SpecifiedAddress", "AAA").ToString(), "Occurs : 0..1");

			AssertNull("Occurs : 0..1", builder.BuildSpecifiedAddressType("SpecifiedAddress", null));
		}

		public void TestBuildDefinedContactType()
		{
			var expectedXmlResult = @"<BuildDefinedContact xmlns=""TestNameSpace"">
  <DepartmentNameText>AAA</DepartmentNameText>
  <PersonNameText>BBB</PersonNameText>
  <TelephoneCommunication>CCC</TelephoneCommunication>
  <URICommunication>DDD</URICommunication>
</BuildDefinedContact>
";
			var builder = new CommonTypeBuilder("TestNameSpace");
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildDefinedContactType("BuildDefinedContact", "AAA", "BBB", "CCC", "DDD").ToString(), "Occurs : 0..1");

			AssertNull("Occurs : 0..1", builder.BuildDefinedContactType("BuildDefinedContact", null, null, null, null));

			expectedXmlResult = @"<BuildDefinedContact xmlns=""TestNameSpace"">
  <DepartmentNameText>AAA</DepartmentNameText>
</BuildDefinedContact>
";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildDefinedContactType("BuildDefinedContact", "AAA", null, null, null).ToString(), "Occurs : 0..1");

			expectedXmlResult = @"<BuildDefinedContact xmlns=""TestNameSpace"">
  <PersonNameText>BBB</PersonNameText>
</BuildDefinedContact>
";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildDefinedContactType("BuildDefinedContact", null, "BBB", null, null).ToString(), "Occurs : 0..1");

			expectedXmlResult = @"<BuildDefinedContact xmlns=""TestNameSpace"">
  <TelephoneCommunication>CCC</TelephoneCommunication>
</BuildDefinedContact>
";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildDefinedContactType("BuildDefinedContact", null, null, "CCC", null).ToString(), "Occurs : 0..1");

			expectedXmlResult = @"<BuildDefinedContact xmlns=""TestNameSpace"">
  <URICommunication>DDD</URICommunication>
</BuildDefinedContact>
";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildDefinedContactType("BuildDefinedContact", null, null, null, "DDD").ToString(), "Occurs : 0..1");
		}
	}
}
