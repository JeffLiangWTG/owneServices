using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsAdditionalInfo))]
sealed class NctsAdditionalInfoTest : EU.NCTS.Business.Testing.NctsAdditionalInfoTest<NctsAdditionalInfo>
{
	public void TestLookups()
	{
		var additionalInfo = CreateAdditionalInfo(Factory);
		AssertType<NctsAdditionalInfoPhase5Lookups>(additionalInfo.Lookups);
	}

	public void TestRepresentativeReadOnly()
	{
		var additionalInfo = CreateAdditionalInfo(Factory);
		var header = additionalInfo.Header;

		CombineAssertions(() =>
		{
			AssertEquals("Representative should not be not readonly when there are no additional documents", false, header.MovementHeader.Representative.ReadOnly);

			var additionalDocument1 = header.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_Code = "4009";
			additionalDocument1.CSI_SubType = "XYZ";

			AssertEquals("Representative should not be readonly when the NctsHeader does not have an additional document with code 4009, Type OTH and SubType REF (1)", false, header.MovementHeader.Representative.ReadOnly);

			var additionalDocument2 = header.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_Code = "1000";
			additionalDocument2.CSI_SubType = "REF";

			AssertEquals("Representative should not be readonly when the NctsHeader does not have an additional document with code 4009, Type OTH and SubType REF (2)", false, header.MovementHeader.Representative.ReadOnly);

			var additionalDocument3 = header.AdditionalDocuments.AddNew();
			additionalDocument3.CSI_Code = "4009";

			AssertEquals("Representative should be readonly when the NctsHeader has an additional document with code 4009, Type OTH and SubType REF", true, header.MovementHeader.Representative.ReadOnly);

			additionalDocument3.Delete();
			AssertEquals("Representative should not be readonly when the only valid additional info is deleted.", false, header.MovementHeader.Representative.ReadOnly);

			additionalDocument2.CSI_Code = "4009";
			AssertEquals("Representative should not be readonly when additional document 2 is changed to code 4009", true, header.MovementHeader.Representative.ReadOnly);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => CreateAdditionalInfo(Factory);

	NctsAdditionalInfo CreateAdditionalInfo(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		return goodsItem.AdditionalInfos.AddNew();
	}
}
