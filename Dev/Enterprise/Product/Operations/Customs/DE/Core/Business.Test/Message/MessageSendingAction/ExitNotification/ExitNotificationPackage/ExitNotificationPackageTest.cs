using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitNotificationPackage))]
	class ExitNotificationPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldSend()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Select", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationPackage), nameof(ExitNotificationPackage.ShouldSend)).Caption);
				AssertEquals(true, exitNotificationPackage.ShouldSend);
			});
		}

		public void TestItemNo()
		{
			short expected = 12;
			CombineAssertions(() =>
			{
				AssertEquals("Item No.", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationPackage), nameof(ExitNotificationPackage.ItemNo)).Caption);
				AssertEquals(expected, exitNotificationPackage.ItemNo);
			});
		}

		public void TestLineNo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Line No.", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationPackage), nameof(ExitNotificationPackage.LineNo)).Caption);
				AssertEquals("13", exitNotificationPackage.LineNo);
			});
		}

		public void TestPackQTY()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Pack QTY", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationPackage), nameof(ExitNotificationPackage.PackQTY)).Caption);
				AssertEquals(17u, exitNotificationPackage.PackQTY);
				exitNotificationPackage.PackQTY = 12u;
				AssertEquals(12u, exitNotificationPackage.PackQTY);
			});
		}

		public void TestPackType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Pack Type", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationPackage), nameof(ExitNotificationPackage.PackType)).Caption);
				AssertEquals("PK", exitNotificationPackage.PackType);
			});
		}

		public void TestMarksNumbers()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Marks & Numbers", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationPackage), nameof(ExitNotificationPackage.MarksNumbers)).Caption);
				AssertEquals("marks&numbers", exitNotificationPackage.MarksNumbers);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new ExitNotificationPackage(exitPackage);

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			exitHeader.CEH_ReferenceNumber = "R12345678";
			exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = "M12345678";
			exitDetail.CED_Status = "AAA";
			exitItem = exitDetail.CusExitItems.AddNew();
			exitItem.CXI_LineNumber = 12;
			exitPackage = exitItem.Packages.AddNew();
			exitPackage.B5_PackageID = "13";
			exitPackage.B5_MarksAndNumbers = "marks&numbers";
			exitPackage.B5_UnitCount = 17;
			exitPackage.B5_UnitType = "PK";
			exitNotificationPackage = (ExitNotificationPackage)GetNewBusinessObject();
			exitNotificationPackage.ShouldSend = true;
		}
		ExitNotificationPackage exitNotificationPackage;
		CusExitControlHeader exitHeader;
		CusExitDetail exitDetail;
		CusExitItem exitItem;
		CusExitItemPackage exitPackage;
	}
}
