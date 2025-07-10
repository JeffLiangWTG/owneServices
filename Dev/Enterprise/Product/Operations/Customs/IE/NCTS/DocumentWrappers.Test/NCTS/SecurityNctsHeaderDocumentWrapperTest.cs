using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using NUnit.Framework;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers.Testing
{
	[TestedType(typeof(SecurityNctsHeaderDocumentWrapper))]
	sealed class SecurityNctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNew()
		{
			var header = Factory.New<NctsHeader>();
			AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(null, Factory));
			AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(header, null));
			AssertNotNull("Instance of SecurityNctsHeaderDocumentWrapper expected", SecurityNctsHeaderDocumentWrapper.New(header, Factory));
		}

		public void TestBox5Items()
		{
			var bill1 = header.Bills.AddNew();
			_ = bill1.GoodsItems.AddNew();
			_ = bill1.GoodsItems.AddNew();
			_ = bill1.GoodsItems.AddNew();
			var bill2 = header.Bills.AddNew();
			_ = bill2.GoodsItems.AddNew();

			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
			AssertEquals(header.TotalNumberOfItems.ToString(), wrapper.BOX5ITEMS.ToString());
		}

		public void TestBox6Packages()
		{
			var bill = header.Bills.AddNew();
			var item1 = bill.GoodsItems.AddNew();
			var package1A = item1.Packages.AddNew();
			package1A.B5_UnitCount = 5;
			var package1B = item1.Packages.AddNew();
			package1B.B5_UnitCount = 6;

			var item2 = bill.GoodsItems.AddNew();
			var package2A = item2.Packages.AddNew();
			package2A.B5_UnitCount = 1;

			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
			AssertEquals(header.TotalNumberOfPackages.ToString(), wrapper.BOX6PACKAGES.ToString());
		}

		public void TestBox7_UCRAndLRN()
		{
			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX7REFERENCENUMBERS) + " returns empty string when no LRN or UCR set", ZString.Empty, wrapper.BOX7REFERENCENUMBERS);
				header.MovementHeader.BM_UniqueConsignmentReference = "8812";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX7REFERENCENUMBERS) + " shows the correct values when UCR (BM_UniqueConsignmentReference) is set", "8812", wrapper.BOX7REFERENCENUMBERS);
				header.MovementHeader.BM_PaperlessInbondNum = "1234V01";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX7REFERENCENUMBERS) + " shows the correct value when UCR and LRN is set", "8812; 1234V01", wrapper.BOX7REFERENCENUMBERS);
				header.MovementHeader.BM_UniqueConsignmentReference = ZString.Empty;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX7REFERENCENUMBERS) + " shows the correct values when LRN only is set", "1234V01", wrapper.BOX7REFERENCENUMBERS);
			});
		}

		public void TestBox21_ActiveBorderTransportMeans()
		{
			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX21BORDERTRANSPORTID) + " returns 3 dashes when no Transport ID at border set", "---", wrapper.BOX21BORDERTRANSPORTID);
				header.MovementHeader.BM_TOLCarrierID = "ULYSSES";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX21BORDERTRANSPORTID) + " returns correct value when Transport ID at border is set", "ULYSSES", wrapper.BOX21BORDERTRANSPORTID);
				header.MovementHeader.BM_RN_NKTOLCarrierNationality = "IE";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX21BORDERTRANSPORTID) + " returns correct value when Transport ID at border is set", "ULYSSES; IE", wrapper.BOX21BORDERTRANSPORTID);
			});
		}

		public void TestBox51_BindingIntinerary()
		{
			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX51TRANSITOFFICE1), "0", wrapper.BOX51TRANSITOFFICE1);

				var officeOfTransit1 = movementHeader.CustomsOfficesForDeparture.AddNew();
				officeOfTransit1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				officeOfTransit1.CY_Data = "IEROS100";
				wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX51TRANSITOFFICE1), "0; IEROS100", wrapper.BOX51TRANSITOFFICE1);

				var country1 = header.CountriesOfRouting.AddNew();
				country1.CY_Data = Core.Constants.CountryCodes.Ireland;
				var country2 = header.CountriesOfRouting.AddNew();
				country2.CY_Data = Core.Constants.CountryCodes.UnitedKingdom;
				wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX51TRANSITOFFICE1), "1; IEROS100", wrapper.BOX51TRANSITOFFICE1);

				var officeOfTransit2 = movementHeader.CustomsOfficesForDeparture.AddNew();
				officeOfTransit2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				officeOfTransit2.CY_Data = "GBPEM100";
				wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOX51TRANSITOFFICE2), "GBPEM100", wrapper.BOX51TRANSITOFFICE2);
			});
		}

		public void TestBoxD_LimitDateFormat()
		{
			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDTIMELIMITDATE) + " returns empty string when no BM_ExportDate is set", ZString.Empty, wrapper.BOXDTIMELIMITDATE);
				header.MovementHeader.BM_ExportDate = new ZDateTime(2025, 05, 18);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDTIMELIMITDATE) + " shows the correct date when BM_ExportDate is set with date", "2025-05-18", wrapper.BOXDTIMELIMITDATE);
				header.MovementHeader.BM_ExportDate = ZDateTime.Empty;
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXDTIMELIMITDATE) + " returns empty string when BM_ExportDate is set with empty date", ZString.Empty, wrapper.BOXDTIMELIMITDATE);
			});
		}

		public void TestBoxS28_Seals()
		{
			CombineAssertions(() =>
			{
				var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS28SEALSNUMBER) + " is 0", "0", wrapper.BOXS28SEALSNUMBER);
				var container1 = header.DepartureHeaderContainers.AddNew();
				container1.Seal1 = "1234";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS28SEALSNUMBER) + " is 1", "1; 1234", wrapper.BOXS28SEALSNUMBER);
				container1.Seal2 = "3456";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS28SEALSNUMBER) + " is 2", "2; 1234; 3456", wrapper.BOXS28SEALSNUMBER);
				var addSeal = container1.AdditionalSeals.AddNew();
				addSeal.BK_SealNumber = "5678";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS28SEALSNUMBER) + " is 3", "3; 5678; 1234; 3456", wrapper.BOXS28SEALSNUMBER);
				var container2 = header.DepartureHeaderContainers.AddNew();
				container2.Seal1 = "7890";
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS28SEALSNUMBER) + " is 4", "4; 5678; 1234; 3456; 7890", wrapper.BOXS28SEALSNUMBER);
			});
		}

		public void TestBoxS29Lines()
		{
			var bill = header.Bills.AddNew();
			var billItem1 = bill.GoodsItems.AddNew();
			billItem1.BY_LineNo = 1;
			billItem1.BY_DeclarationGoodsItemNumber = 10111;
			var billItem2 = bill.GoodsItems.AddNew();
			billItem2.BY_LineNo = 2;
			billItem2.BY_DeclarationGoodsItemNumber = 10112;

			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
			AssertEquals("BoxS29Lines should get items from Bills.GoodsItems", 2, wrapper.BoxS29Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("BOX32ITEM should get correct item numbers from Bills.GoodsItems[0].BY_DeclarationGoodsItemNumber", "1 / 10111", wrapper.BoxS29Lines[0].BOX32ITEM);
				AssertEquals("BOX32ITEM should get correct item numbers from Bills.GoodsItems[1]", "2 / 10112", wrapper.BoxS29Lines[1].BOX32ITEM);
			});
		}
		public void TestNotReleasedWatermark()
		{
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			var wrapper = GetWrapper(header);
			AssertEquals(Tools.GetNotReleasedCaption(), wrapper.NOTRELEASEDWATERMARK);

			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			wrapper = GetWrapper(header);
			AssertEquals(ZString.Empty, wrapper.NOTRELEASEDWATERMARK);

			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
			wrapper = GetWrapper(header);
			AssertEquals(ZString.Empty, wrapper.NOTRELEASEDWATERMARK);
		}

		SecurityNctsHeaderDocumentWrapper GetWrapper(NctsHeader header)
		{
			return SecurityNctsHeaderDocumentWrapper.New(header, Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return SecurityNctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader = Factory.New<NctsDepartureMovementHeader>();
			movementHeader.BM_BH = header.PK;
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
		NctsDepartureMovementHeader movementHeader;
	}
}
