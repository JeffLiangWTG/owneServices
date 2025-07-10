using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSCancelDeclarationEDIMessagePrettierTests : TestCaseWithFactory
	{
		public void TestMakeHumanReadable()
		{
			var message = Factory.New<CDSCancelDeclarationEDIMessage>();
			message.EM_MessageText = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <WCODataModelVersionCode>3.6</WCODataModelVersionCode>
  <WCOTypeName>DEC</WCOTypeName>
  <ResponsibleCountryCode>GB</ResponsibleCountryCode>
  <ResponsibleAgencyName>HMRC</ResponsibleAgencyName>
  <AgencyAssignedCustomizationVersionCode>v2.1</AgencyAssignedCustomizationVersionCode>
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>13</FunctionCode>
    <FunctionalReferenceID>7-B00001311</FunctionalReferenceID>
    <ID>MRN123</ID>
    <TypeCode>INV</TypeCode>
    <AdditionalInformation>
      <StatementDescription>RRR</StatementDescription>
      <StatementTypeCode>AES</StatementTypeCode>
    </AdditionalInformation>
    <Amendment>
      <ChangeReasonCode>1</ChangeReasonCode>
    </Amendment>
  </Declaration>
</MetaData>";

			var prettier = new CDSCancelDeclarationEDIMessagePrettier(message);
			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H1>Request to Cancel</H1><p><strong>MRN: </strong>MRN123<br><strong>Functional Reference ID: </strong>7-B00001311<br><strong>Reason Code: </strong>1 - Cancel - Declaration is no longer required<br><strong>Reason Description: </strong>RRR</p>", prettier.MakeHumanReadable());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_UCR = "DUCR";
			declaration.JE_MasterUCR = "MUCR";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGMREF";
			message.EM_LinkedObject = entryHeader;

			var associateRequestEDIMessagePrettier = new AssociateRequestEDIMessagePrettier(message);
			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Associate BGMREF into consol MUCR</H3>", associateRequestEDIMessagePrettier.MakeHumanReadable());

			var closeRequestEDIMessagePrettier = new CloseRequestEDIMessagePrettier(message);
			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Close consol MUCR</H3>", closeRequestEDIMessagePrettier.MakeHumanReadable());

			var disassociateRequestEDIMessagePrettier = new DisassociateRequestEDIMessagePrettier(message);
			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Dissassociate BGMREF from consol MUCR</H3>", disassociateRequestEDIMessagePrettier.MakeHumanReadable());
		}
	}
}
