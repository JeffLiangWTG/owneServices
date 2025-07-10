using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(UnloadingRemarkAddInfo))]
	public class UnloadingRemarkAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCaption()
		{
			AssertEquals("No. of Seals", DataBoundResourceStrings.GetDataForProperty(unloadingRemarkAddInfo.G9_NoOfSealsInfo).Caption);
		}

		public void TestNcsTestHeader()
		{
			AssertEquals(header, unloadingRemarkAddInfo.NctsHeader);
		}

		public void TestParent()
		{
			AssertType<CusAddInfo<UnloadingRemarkAddInfo>>(unloadingRemarkAddInfo.Parent);
		}

		public void TestG9_NoOfSeals_ReadOnly()
		{
			var noOfSealsInfo = unloadingRemarkAddInfo.G9_NoOfSealsInfo;
			CombineAssertions(() =>
			{
				AssertEquals("State Of Seals empty value", true, noOfSealsInfo.ReadOnly);
				unloadingRemarkAddInfo.G9_StateOfSealsOk = YesNoList.Codes.No;
				AssertEquals("State of seals N", false, noOfSealsInfo.ReadOnly);
				unloadingRemarkAddInfo.G9_StateOfSealsOk = YesNoList.Codes.Yes;
				AssertEquals("State of seals Y", true, noOfSealsInfo.ReadOnly);
			});
		}

		public void TestYesNoListsAreTranslatable()
		{
			NCTSTestHelper.AssertYesNoListsAreTranslatable(unloadingRemarkAddInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => unloadingRemarkAddInfo;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			unloadingRemarkAddInfo = header.UnloadingRemark;
		}

		NctsHeader header;
		UnloadingRemarkAddInfo unloadingRemarkAddInfo;
	}
}
