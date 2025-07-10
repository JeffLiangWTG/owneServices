using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class TallyCustomsOutturnListenerTest : TestCaseWithFactory
	{
		public void TestMarksAndNumbers()
		{
			outturn.C5_MarksAndNumbers = "foo";
			AssertEquals("foo", linkOutturn.MarksAndNumbers);
			outturn.C5_MarksAndNumbers = "bar";
			AssertEquals("bar", linkOutturn.MarksAndNumbers);
		}

		public void TestNumberOfPackages()
		{
			outturn.C5_OuterPacks = 5;
			AssertEquals(5, linkOutturn.NumberOfPackages);
			outturn.C5_OuterPacks = 7;
			AssertEquals(7, linkOutturn.NumberOfPackages);
		}

		public void TestPackageType()
		{
			linkOutturn.SetPackageType(Core.Constants.PkgUnit.Bag);
			AssertEquals(CMRPackageTypes.Codes.Bags, outturn.C5_PackagesUnits);
			linkOutturn.SetPackageType(Core.Constants.PkgUnit.Carton);
			AssertEquals(CMRPackageTypes.Codes.Carton, outturn.C5_PackagesUnits);
		}

		public void TestPackagesOutturned()
		{
			linkOutturn.SetPackagesOutturned(5);
			AssertEquals(5, outturn.C5_PackagesOutturned);
			linkOutturn.SetPackagesOutturned(7);
			AssertEquals(7, outturn.C5_PackagesOutturned);
		}

		public void TestDamaged()
		{
			linkOutturn.SetDamaged(true);
			AssertEquals(true, outturn.C5_DamageIndicator);
			linkOutturn.SetDamaged(false);
			AssertEquals(false, outturn.C5_DamageIndicator);
		}

		public void TestPillaged()
		{
			linkOutturn.SetPillaged(true);
			AssertEquals(true, outturn.C5_PillageIndicator);
			linkOutturn.SetPillaged(true);
			AssertEquals(true, outturn.C5_PillageIndicator);
		}

		public void TestUnpackDate()
		{
			outturn.C5_CargoUnpackDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, linkOutturn.UnpackDate);
			AssertEquals(ZDateTime.BrettsBirthday, outturn.C5_CargoUnpackDate);

			linkOutturn.UnpackDate = new ZDateTime(2006, 05, 04);
			AssertEquals(new ZDateTime(2006, 05, 04), outturn.C5_CargoUnpackDate);
			AssertEquals(new ZDateTime(2006, 05, 04), linkOutturn.UnpackDate);
			AssertEquals(new ZDateTime(2006, 05, 04), outturn.C5_CargoReceiptDate);

			linkOutturn.UnpackDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, outturn.C5_CargoUnpackDate);
			AssertEquals(ZDateTime.Empty, outturn.C5_CargoReceiptDate);
			AssertEquals(ZDateTime.Empty, linkOutturn.UnpackDate);

			outturn.C5_CargoReceiptDate = ZDateTime.BrettsBirthday;
			linkOutturn.UnpackDate = new ZDateTime(2006, 05, 04);
			AssertEquals(ZDateTime.BrettsBirthday, outturn.C5_CargoReceiptDate);
			AssertEquals(new ZDateTime(2006, 05, 04), outturn.C5_CargoUnpackDate);
			AssertEquals(new ZDateTime(2006, 05, 04), linkOutturn.UnpackDate);
			linkOutturn.UnpackDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, outturn.C5_CargoUnpackDate);
			AssertEquals(ZDateTime.Empty, linkOutturn.UnpackDate);
			AssertEquals(ZDateTime.BrettsBirthday, outturn.C5_CargoReceiptDate);
		}

		public void TestIsDeleted()
		{
			AssertEquals("precondition", false, outturn.IsDeleted);
			AssertEquals(false, linkOutturn.IsDeleted);
			outturn.Delete();
			AssertEquals(true, linkOutturn.IsDeleted);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<CusOutturnHeader>();
			outturn = header.Outturns.AddNew();
			linkOutturn = new TallyCustomsOutturnListener(outturn);
		}

		CusOutturnHeader header;
		DepotCusOutturn outturn;
		TallyCustomsOutturnListener linkOutturn;

		#endregion
	}
}
