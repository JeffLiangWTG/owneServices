using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NCTS5DepartureCustomsStatusList = Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers.Testing
{
	[TestedType(typeof(NctsHeaderDocumentWrapper))]
	sealed class NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNew()
		{
			using (TemporarilySetNCTSPhase5TransitionPeriod(true))
			{
				AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => NctsHeaderDocumentWrapper.New(null, Factory));
				AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => NctsHeaderDocumentWrapper.New(header, null));

				AssertType<NctsHeaderDocumentWrapper>("Instance of NctsHeaderDocumentWrapper expected", NctsHeaderDocumentWrapper.New(header, Factory));
			}
		}
		  
		public void TestNewForPhase5()
		{
			using (TemporarilySetNCTSPhase5TransitionPeriod(false))
			{
				AssertType<Phase5NctsHeaderDocumentWrapper>("Instance of Phase5NctsHeaderDocumentWrapper expected", NctsHeaderDocumentWrapper.New(header, Factory));
			}
		}

		public void TestBoxDTimeLimitDate()
		{
			CombineAssertions(() =>
			{
				movementHeader.BM_ExportDate = new ZDateTime(2023, 12, 17);
				var wrapper = GetWrapper(header);
				AssertEquals(nameof(wrapper.BOXDTIMELIMITDATE), "17-12-2023", wrapper.BOXDTIMELIMITDATE);
			});
		}

		public void TestBox44DocsAndCerts()
		{
			CombineAssertions(() =>
			{
				var wrapper = GetWrapper(header);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX441DOCSANDCERTS), "", wrapper.BOX441DOCSANDCERTS);

				var additionalInfo1 = header.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo1.CSI_Code = "Z-INF";
				additionalInfo1.CSI_ReferenceNumber = "inf-1";
				additionalInfo1.CSI_Description = "inf description";

				var additionalInfo2 = header.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo2.CSI_Code = "A-INF";
				additionalInfo2.CSI_ReferenceNumber = "inf-2";
				additionalInfo2.CSI_Description = "inf description";

				var additionalInfo3 = header.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo3.CSI_Code = "A-INF";
				additionalInfo3.CSI_ReferenceNumber = "inf-1";
				additionalInfo3.CSI_Description = "inf description";

				var additionalReference1 = header.AdditionalDocuments.AddNew();
				additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalReference1.CSI_Code = "REF1";
				additionalReference1.CSI_ReferenceNumber = "ref-1";
				additionalReference1.CSI_Description = "ref description";

				var transportDocument1 = header.AdditionalDocuments.AddNew();
				transportDocument1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				transportDocument1.CSI_Code = "TRA1";
				transportDocument1.CSI_ReferenceNumber = "tra-1";
				transportDocument1.CSI_Description = "tra description";

				var transportDocument2 = header.AdditionalDocuments.AddNew();
				transportDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				transportDocument2.CSI_Code = "TRA2";
				transportDocument2.CSI_ReferenceNumber = "tra-2";
				transportDocument2.CSI_Description = "tra 2 description";

				var supportingDocument = header.MovementHeader.SupportingDocuments.AddNew();
				supportingDocument.CSI_LineNo = 1;
				supportingDocument.CSI_Code = "9100";
				supportingDocument.CSI_ReferenceNumber = "3278923";
				supportingDocument.CSI_ItemNumber = 1;
				supportingDocument.CSI_ReferenceNumber2 = "Some description";

				var usage = header.MovementHeader.CusAuthorizationUsages.AddNew();
				var customer = Factory.NewWithValidTestData<OrgHeader>();
				customer.OH_RL_NKClosestPort = "GBLON";
				usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				usage.AGC_Number = "TST_ATH_001";
				usage.AGC_OH_Owner = customer.PK;

				wrapper = GetWrapper(header);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX441DOCSANDCERTS), "ACR TST_ATH_001;9100 3278923 1 Some description;TRA1 tra-1;TRA2 tra-2;REF1 ref-1;A-INF inf-1;A-INF inf-2;Z-INF inf-1", wrapper.BOX441DOCSANDCERTS);
			});
		}

		public void TestBox52Guarantee()
		{
			CombineAssertions(() =>
			{
				var wrapper = GetWrapper(header);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX52GUARANTEE), "", wrapper.BOX52GUARANTEE);

				var guarantee = movementHeader.Guarantees.AddNew();
				guarantee.PW_BondType = "ABC";
				guarantee = movementHeader.Guarantees.AddNew();
				guarantee.PW_BondType = "CDE";

				wrapper = GetWrapper(header);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX52GUARANTEE), "ABC;CDE", wrapper.BOX52GUARANTEE);
			});
		}

		public void TestBox51TransitOffice()
		{
			CombineAssertions(() =>
			{
				var wrapper = GetWrapper(header);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE1), "", wrapper.BOX51TRANSITOFFICE1);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE2), "", wrapper.BOX51TRANSITOFFICE2);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE3), "", wrapper.BOX51TRANSITOFFICE3);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE4), "", wrapper.BOX51TRANSITOFFICE4);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE5), "", wrapper.BOX51TRANSITOFFICE5);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE6), "", wrapper.BOX51TRANSITOFFICE6);

				var customsOfficesForDeparture = header.MovementHeader.CustomsOfficesForDeparture;

				var transitOffice1 = customsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				transitOffice1.CY_Data = "IEDUB401";
				var transitOffice2 = customsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				transitOffice2.CY_Data = "IEDUB402";
				var transitOffice3 = customsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				transitOffice3.CY_Data = "IEDUB403";
				var transitOffice4 = customsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				transitOffice4.CY_Data = "IEDUB404";
				var transitOffice5 = customsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				transitOffice5.CY_Data = "IEDUB405";
				var transitOffice6 = customsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				transitOffice6.CY_Data = "IEDUB406";

				wrapper = GetWrapper(header);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE1), "IEDUB401", wrapper.BOX51TRANSITOFFICE1);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE2), "IEDUB402", wrapper.BOX51TRANSITOFFICE2);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE3), "IEDUB403", wrapper.BOX51TRANSITOFFICE3);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE4), "IEDUB404", wrapper.BOX51TRANSITOFFICE4);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE5), "IEDUB405", wrapper.BOX51TRANSITOFFICE5);
				AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX51TRANSITOFFICE6), "IEDUB406", wrapper.BOX51TRANSITOFFICE6);
			});
		}

		public void TestBoxS29Lines()
		{
			var bill = header.Bills.AddNew();
			var billItem = bill.GoodsItems.AddNew();
			billItem.BY_HarmonisedTariff = "1234567";

			var wrapper = GetWrapper(header);
			AssertEquals("BoxS29Lines should get item from Bills.GoodsItems", 1, wrapper.BoxS29Lines.Count);
			AssertEquals("Should get correct items from Bills.GoodsItems", "1234567", wrapper.BoxS29Lines[0].BOX33COMMODITY);
		}

		public void TestBox5Items()
		{
			var bill1 = header.Bills.AddNew();
			_ = bill1.GoodsItems.AddNew();
			_ = bill1.GoodsItems.AddNew();
			_ = bill1.GoodsItems.AddNew();
			var bill2 = header.Bills.AddNew();
			_ = bill2.GoodsItems.AddNew();

			var wrapper = GetWrapper(header);
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

			var wrapper = GetWrapper(header);
			AssertEquals(header.TotalNumberOfPackages.ToString(), wrapper.BOX6PACKAGES.ToString());
		}

		public void TestBox35GrossMass()
		{
			var wrapper = GetWrapper(header);
			CombineAssertions(() =>
			{
				AssertEquals("BOX35GROSSMASS", "0", wrapper.BOX35GROSSMASS);

				movementHeader.BM_GrossWeight = 100.01;
				AssertEquals("BOX35GROSSMASS", "100.01", wrapper.BOX35GROSSMASS);
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

		protected override BusinessObject GetNewBusinessObject() => GetWrapper(header);

		NctsHeaderDocumentWrapper GetWrapper(NctsHeader header)
		{
			using (TemporarilySetNCTSPhase5TransitionPeriod(true))
			{
				return (NctsHeaderDocumentWrapper)NctsHeaderDocumentWrapper.New(header, Factory);
			}
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

		IDisposable TemporarilySetNCTSPhase5TransitionPeriod(bool isInTransitionPeriod) =>
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				ZDate.Today,
				isInTransitionPeriod);

		NctsHeader header;
		NctsDepartureMovementHeader movementHeader;
	}
}
