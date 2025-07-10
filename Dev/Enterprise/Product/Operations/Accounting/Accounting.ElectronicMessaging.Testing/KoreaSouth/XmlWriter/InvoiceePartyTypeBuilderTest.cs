using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.MasterFiles.Business.Testing;
using Moq;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class InvoiceePartyTypeBuilderTest : TestCaseWithFactory
	{
		public void TestBuildInvoiceeParty()
		{
			var additionalInfo = GetMockedInvoiceePartyAdditionalInfo();
			var element = new InvoiceePartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoiceeParty");

			AssertEquals($@"<InvoiceeParty xmlns=""TestNameSpace"">
  <ID>ID</ID>
  <TypeCode>TypeCode</TypeCode>
  <NameText>NameText</NameText>
  <ClassificationCode>ClassificationCode</ClassificationCode>
  <SpecifiedOrganization>
    <TaxRegistrationID>TaxRegistrationID</TaxRegistrationID>
    <BusinessTypeCode>BusinessTypeCode</BusinessTypeCode>
  </SpecifiedOrganization>
  <SpecifiedPerson>
    <NameText>SpecifiedPersonNameText</NameText>
  </SpecifiedPerson>
  <PrimaryDefinedContact>
    <PersonNameText>PrimaryDefinedContactPersonName</PersonNameText>
    <TelephoneCommunication>PrimaryDefinedContactTel</TelephoneCommunication>
    <URICommunication>PrimaryDefinedContactURICommunication</URICommunication>
  </PrimaryDefinedContact>
  <SecondaryDefinedContact>
    <PersonNameText>SecondaryDefinedContactPersonName</PersonNameText>
    <TelephoneCommunication>SecondaryDefinedContactTel</TelephoneCommunication>
    <URICommunication>SecondaryDefinedContactURICommunication</URICommunication>
  </SecondaryDefinedContact>
  <SpecifiedAddress>
    <LineOneText>SpecifiedAddressLineOneText</LineOneText>
  </SpecifiedAddress>
</InvoiceeParty>", element.ToString());
		}

		public void TestBuildEmptyInvoiceeParty()
		{
			var additionalInfo = new Mock<IInvoiceePartyAdditionalInfo>();
			var element = new InvoiceePartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoiceeParty");

			AssertEquals($@"<InvoiceeParty xmlns=""TestNameSpace"">
  <ID></ID>
  <NameText></NameText>
  <SpecifiedOrganization>
    <BusinessTypeCode></BusinessTypeCode>
  </SpecifiedOrganization>
  <SpecifiedPerson>
    <NameText></NameText>
  </SpecifiedPerson>
</InvoiceeParty>", element.ToString());
		}

		public void TestBuildSpecifiedOrganization_TaxRegistrationID()
		{
			var additionalInfo = new Mock<IInvoiceePartyAdditionalInfo>();

			additionalInfo.SetupGet(x => x.TaxRegistrationID).Returns(string.Empty);
			var element = new InvoiceePartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoiceeParty");
			AssertNotContains("TaxRegistrationID Occurs : 0..1", "TaxRegistrationID", element.ToString());

			additionalInfo.Reset();
			additionalInfo.SetupGet(x => x.TaxRegistrationID).Returns("Test");
			element = new InvoiceePartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoiceeParty");
			AssertContains("TaxRegistrationID Occurs : 0..1", $"<TaxRegistrationID>Test</TaxRegistrationID>", element.ToString());
		}

		public void TestBuildInvoiceeParty_WithLongSpecifiedPersonNameText()
		{
			var rendomText = MasterFilesTestHelper.GetRandomString(97);
			var additionalInfo = GetMockedInvoiceePartyAdditionalInfo();

			additionalInfo.SetupGet(x => x.SpecifiedPersonNameText).Returns(rendomText + "AAAA");
			var element = new InvoiceePartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoiceeParty");
			AssertContains($"<SpecifiedPerson>\r\n    <NameText>{rendomText}...</NameText>\r\n  </SpecifiedPerson>", element.ToString());
		}

		Mock<IInvoiceePartyAdditionalInfo> GetMockedInvoiceePartyAdditionalInfo()
		{
			var additionalInfo = new Mock<IInvoiceePartyAdditionalInfo>();

			additionalInfo.SetupGet(x => x.ID).Returns("ID");
			additionalInfo.SetupGet(x => x.TypeCode).Returns("TypeCode");
			additionalInfo.SetupGet(x => x.NameText).Returns("NameText");
			additionalInfo.SetupGet(x => x.ClassificationCode).Returns("ClassificationCode");
			additionalInfo.SetupGet(x => x.TaxRegistrationID).Returns("TaxRegistrationID");
			additionalInfo.SetupGet(x => x.BusinessTypeCode).Returns("BusinessTypeCode");
			additionalInfo.SetupGet(x => x.SpecifiedPersonNameText).Returns("SpecifiedPersonNameText");
			additionalInfo.SetupGet(x => x.SpecifiedAddressLineOneText).Returns("SpecifiedAddressLineOneText");

			additionalInfo.SetupGet(x => x.PrimaryDefinedContactPersonName).Returns("PrimaryDefinedContactPersonName");
			additionalInfo.SetupGet(x => x.PrimaryDefinedContactTel).Returns("PrimaryDefinedContactTel");
			additionalInfo.SetupGet(x => x.PrimaryDefinedContactURICommunication).Returns("PrimaryDefinedContactURICommunication");

			additionalInfo.SetupGet(x => x.SecondaryDefinedContactPersonName).Returns("SecondaryDefinedContactPersonName");
			additionalInfo.SetupGet(x => x.SecondaryDefinedContactTel).Returns("SecondaryDefinedContactTel");
			additionalInfo.SetupGet(x => x.SecondaryDefinedContactURICommunication).Returns("SecondaryDefinedContactURICommunication");

			return additionalInfo;
		}
	}
}

