using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CancellationRequestMessageBuilderTests : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var helper = new DeclarationTestHelper(Factory);
			var org1 = helper.MakeOrganisation1();
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB025115100006", Core.Constants.CountryCodes.UnitedKingdom);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OA_DeclarantAddress = org1.MainAddress.PK;
			var entry = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
			var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, "GB");
			mrn.CE_EntryNum = "MRN123";
			entry.CH_CustomsMessageRemarks = "Reason Desc";
			entry.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_NotRequired;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entry.Declaration);

			var builder = new CancellationRequestMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entry), new CusdecMessageFunction.Deleted().GetCdsFunctionCode());
			AssertEquals(@"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <WCODataModelVersionCode>3.6</WCODataModelVersionCode>
  <WCOTypeName>DEC</WCOTypeName>
  <ResponsibleCountryCode>GB</ResponsibleCountryCode>
  <ResponsibleAgencyName>HMRC</ResponsibleAgencyName>
  <AgencyAssignedCustomizationVersionCode>v2.1</AgencyAssignedCustomizationVersionCode>
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>13</FunctionCode>
    <FunctionalReferenceID>LRNREFERENCEPLACEHOLDERF6AB79A7845D49FA82919554DED4F991</FunctionalReferenceID>
    <ID>MRN123</ID>
    <TypeCode>INV</TypeCode>
    <AdditionalInformation>
      <StatementDescription>Reason Desc</StatementDescription>
      <StatementTypeCode>AES</StatementTypeCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>06A</DocumentSectionCode>
      </Pointer>
    </AdditionalInformation>
    <Amendment>
      <ChangeReasonCode>1</ChangeReasonCode>
    </Amendment>
  </Declaration>
</MetaData>", builder.Build());
		}
	}
}
