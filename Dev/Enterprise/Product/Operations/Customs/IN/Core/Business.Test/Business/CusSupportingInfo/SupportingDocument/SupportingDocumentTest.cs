using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SupportingDocument))]
sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
{
	public void TestCSI_LineNo()
	{
		AssertCaptions(SupportingInfo.CSI_LineNoInfo, "Serial Number", "Serial No.", "Sr. No.");
	}

	public void TestCSI_ReferenceNumber2()
	{
		AssertCaptions(SupportingInfo.CSI_ReferenceNumber2Info, "Image Reference Number", "Image Ref. No.", "IRN");
		AssertEquals("MaxLength", SupportingDocument.Schema.CSI_ReferenceNumber2MaxLength, SupportingInfo.CSI_ReferenceNumber2Info.MaxLength);
	}

	public void TestCSI_Code()
	{
		AssertCaptions(SupportingInfo.CSI_CodeInfo, "Document Type Code", "Doc. Type", "Type");
	}

	public void TestCSI_IssuerType()
	{
		AssertCaptions(SupportingInfo.CSI_IssuerTypeInfo, "Code", "Code", "Code");
		AssertEquals("MaxLength", SupportingDocument.Schema.CSI_IssuerTypeMaxLength, SupportingInfo.CSI_IssuerTypeInfo.MaxLength);
	}

	public void TestOrganisationCaption()
	{
		AssertCaptions(SupportingInfo.OrganizationPKInfo, "Organization", "Org.", "Org.");
	}

	public void TestLookups()
	{
		AssertType<SupportingDocumentLookups>(SupportingInfo.Lookups);
	}

	public void TestValidation()
	{
		AssertType<SupportingDocumentValidation>(SupportingInfo.Validation);
	}

	public void TestCSI_LineNoSequence()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var supportingDocItem1 = entryInstruction.SupportingDocuments.AddNew();
		var supportingDocItem2 = entryInstruction.SupportingDocuments.AddNew();
		var supportingDocItem3 = entryInstruction.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", 1, supportingDocItem1.CSI_LineNo);
			AssertEquals("Order 2", 2, supportingDocItem2.CSI_LineNo);
			AssertEquals("Order 3", 3, supportingDocItem3.CSI_LineNo);

			supportingDocItem2.CSI_LineNo = 5;
			AssertEquals("Order 1", 1, supportingDocItem1.CSI_LineNo);
			AssertEquals("Order 3 after recalculate", 3, supportingDocItem2.CSI_LineNo);
			AssertEquals("Order 2 after recalculate", 2, supportingDocItem3.CSI_LineNo);

			entryInstruction.SupportingDocuments.Remove(supportingDocItem2);
			AssertEquals("Order 1 stay same", 1, supportingDocItem1.CSI_LineNo);
			AssertEquals("Order 3 change to 2", 2, supportingDocItem3.CSI_LineNo);

			supportingDocItem1.Delete();
			AssertEquals("Order 2 change to 1", 1, supportingDocItem3.CSI_LineNo);
		});
	}

	public void TestISequenceNumberLine()
	{
		CombineAssertions(() =>
		{
			var sequenceLine = (IHugeSequenceNumberLine)SupportingInfo;
			AssertEquals("FKToHeader", SupportingInfo.Parent.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZInt)1, sequenceLine.SequenceNumber);
		});
	}

	public void TestOrganisationAddress()
	{
		var organisationAddress = SupportingInfo.OrganizationAddress;

		AssertNotNull("OrganisationAddress should return a non-null address", organisationAddress);
		AssertType<JobDocAddress>("OrganisationAddress should be an instance of JobDocAddress", organisationAddress);
	}

	public void TestOrganisationPK()
	{
		var organisationPK = new ZGuid();
		SupportingInfo.OrganizationAddress.OrganisationPK = organisationPK;
		AssertEquals("OrganisationPK should be set correctly", organisationPK, SupportingInfo.OrganizationPK);
	}

	public void TestSupportedAddressTypes()
	{
		var supportedAddressTypes = ((IDocAddresses)SupportingInfo).SupportedAddressTypes;
		AssertEquals("SupportedAddressTypes should contain SupportingDocumentOrganisation", 1, supportedAddressTypes.Count);
		AssertEquals("SupportedAddressTypes should be SupportingDocumentOrganisation", DocAddressType.SupportingDocumentOrganizationAddress, supportedAddressTypes[0]);
	}

	public void TestPiggyBackedDocAddressValidation()
	{
		var jobDocAddress = Factory.New<JobDocAddress>();
		var validation = ((IDocAddresses)SupportingInfo).PiggyBackedDocAddressValidation(jobDocAddress);
		AssertNotNull("PiggyBackedDocAddressValidation should return a non-null validation object", validation);
		AssertType<SupportingDocumentJobDocAddressValidation>("PiggyBackedDocAddressValidation should return SupportingDocumentJobDocAddressValidation", validation);
	}

	public void TestClearCSI_IssuerTypeOnOrganisationPKChanged()
	{
		var supportingInfo = SupportingInfo;
		var orgAddress = supportingInfo.OrganizationAddress;
		orgAddress.OrganisationPK = ZGuid.NewZGuid();
		supportingInfo.CSI_IssuerType = "GST";
		AssertEquals("Initial CSI_IssuerType should be set", "GST", supportingInfo.CSI_IssuerType);

		orgAddress.OrganisationPK = ZGuid.Empty;
		AssertEquals("CSI_IssuerType should be cleared when OrganizationPK becomes empty", ZString.Empty, supportingInfo.CSI_IssuerType);
	}

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewBusinessObject(factory);
	}

	void AssertCaptions(ZPropertyInfo info, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(info);
		AssertNotNull("Res String data", resData);
		AssertEquals("Caption", expectedCaption, resData.Caption);
		AssertEquals("MediumCaption", expectedMediumCaption, resData.MediumCaption);
		AssertEquals("ShortCaption", expectedShortCaption, resData.ShortCaption);
	}

	SupportingDocument GetNewBusinessObject(BusinessObjectFactory factory)
	{
		return factory.NewWithValidTestData<CusEntryInstruction>().SupportingDocuments.AddNew();
	}

	SupportingDocument SupportingInfo => supportingInfo ??= GetNewBusinessObject(Factory);
	SupportingDocument supportingInfo;
}
