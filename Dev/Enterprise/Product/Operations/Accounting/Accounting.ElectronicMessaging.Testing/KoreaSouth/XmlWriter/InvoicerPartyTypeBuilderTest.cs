using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.MasterFiles.Business.Testing;
using Moq;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class InvoicerPartyTypeBuilderTest : TestCaseWithFactory
	{
		public void TestBuildInvoicerParty()
		{
			var additionalInfo = GetMockedInvoicerPartyAdditionalInfo();

			var element = new InvoicerPartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoicerParty");

			AssertEquals($@"<InvoicerParty xmlns=""TestNameSpace"">
  <ID>ID</ID>
  <TypeCode>TypeCode</TypeCode>
  <NameText>NameText</NameText>
  <ClassificationCode>ClassificationCode</ClassificationCode>
  <SpecifiedOrganization>
    <TaxRegistrationID>TaxRegistrationID</TaxRegistrationID>
  </SpecifiedOrganization>
  <SpecifiedPerson>
    <NameText>SpecifiedPersonNameText</NameText>
  </SpecifiedPerson>
  <DefinedContact>
    <PersonNameText>DefinedContactPersonName</PersonNameText>
    <TelephoneCommunication>DefinedContactTel</TelephoneCommunication>
    <URICommunication>DefinedContactURICommunication</URICommunication>
  </DefinedContact>
  <SpecifiedAddress>
    <LineOneText>SpecifiedAddressLineOneText</LineOneText>
  </SpecifiedAddress>
</InvoicerParty>", element.ToString());
		}

		public void TestBuildEmptyInvoicerParty()
		{
			var additionalInfo = new Mock<IInvoicerPartyAdditionalInfo>();
			var element = new InvoicerPartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoicerParty");

			AssertEquals($@"<InvoicerParty xmlns=""TestNameSpace"">
  <ID></ID>
  <NameText></NameText>
  <SpecifiedPerson>
    <NameText></NameText>
  </SpecifiedPerson>
</InvoicerParty>", element.ToString());
		}

		public void TestBuildInvoicerParty_WithLongSpecifiedPersonNameText()
		{
			var rendomText = MasterFilesTestHelper.GetRandomString(97);
			var additionalInfo = GetMockedInvoicerPartyAdditionalInfo();

			additionalInfo.SetupGet(x => x.SpecifiedPersonNameText).Returns(rendomText + "AAAA");
			var element = new InvoicerPartyTypeBuilder("TestNameSpace", additionalInfo.Object).Build("InvoicerParty");
			AssertContains($"<SpecifiedPerson>\r\n    <NameText>{rendomText}...</NameText>\r\n  </SpecifiedPerson>", element.ToString());
		}

		Mock<IInvoicerPartyAdditionalInfo> GetMockedInvoicerPartyAdditionalInfo()
		{
			var additionalInfo = new Mock<IInvoicerPartyAdditionalInfo>();

			additionalInfo.SetupGet(x => x.ID).Returns("ID");
			additionalInfo.SetupGet(x => x.TypeCode).Returns("TypeCode");
			additionalInfo.SetupGet(x => x.NameText).Returns("NameText");
			additionalInfo.SetupGet(x => x.ClassificationCode).Returns("ClassificationCode");
			additionalInfo.SetupGet(x => x.TaxRegistrationID).Returns("TaxRegistrationID");
			additionalInfo.SetupGet(x => x.SpecifiedPersonNameText).Returns("SpecifiedPersonNameText");
			additionalInfo.SetupGet(x => x.SpecifiedAddressLineOneText).Returns("SpecifiedAddressLineOneText");

			additionalInfo.SetupGet(x => x.DefinedContactPersonName).Returns("DefinedContactPersonName");
			additionalInfo.SetupGet(x => x.DefinedContactTel).Returns("DefinedContactTel");
			additionalInfo.SetupGet(x => x.DefinedContactURICommunication).Returns("DefinedContactURICommunication");

			return additionalInfo;
		}
	}
}

