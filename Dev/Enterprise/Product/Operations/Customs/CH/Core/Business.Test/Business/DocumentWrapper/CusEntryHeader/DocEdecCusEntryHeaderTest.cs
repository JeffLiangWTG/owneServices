using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocEdecCusEntryHeader))]
public class DocEdecCusEntryHeaderTest : DocBaseCusEntryHeaderTest<CusEntryHeader, DocEdecCusEntryHeader>
{
	public void TestImporter() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.Importer);
		AssertType<DocTraderDataWrapper>(EntryHeaderWrapperInternal.Importer);
	});

	public override void TestConsignee() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		AssertType<DocTraderDataWrapper>("No consignee (no exception)", EntryHeaderWrapperInternal.Consignee);

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.SettingDefaults = true;
		orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
		orgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
		orgAddress.SettingDefaults = false;
		orgHeader.Addresses.Add(orgAddress);

		EntryHeaderInternal.Declaration.JE_OH_Consignee = orgHeader.PK;

		AssertType<DocTraderDataWrapper>("Has consignee", EntryHeaderWrapperInternal.Consignee);
	});

	public void TestBusiness() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.Business);
		AssertType<DocEdecBusinessDataWrapper>(EntryHeaderWrapperInternal.Business);
	});

	public void TestEntryLinesCollection()
	{
		AssertNotNull(EntryHeaderWrapperInternal.EntryLines);
		AssertType<DocEdecCusEntryLineCollection>(EntryHeaderWrapperInternal.EntryLines);
	}

	protected override CusEntryHeader GetNewEntryHeader()
	{
		var entryHeader = base.GetNewEntryHeader();
		entryHeader.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		return entryHeader;
	}

	protected override DocEdecCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
	{
		return DocEdecCusEntryHeader.New(entryHeader, Factory);
	}
}
