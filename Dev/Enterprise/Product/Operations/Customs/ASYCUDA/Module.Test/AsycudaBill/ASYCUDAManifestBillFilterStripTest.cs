using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AsycudaBill = Enterprise.Customs.ASYCUDA.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(ASYCUDAManifestBillFilterStrip))]
	sealed class ASYCUDAManifestBillFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestAgentTypeFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_AgentType = "CLD";
			asyheader1.AMA_RN_NKCountry = "US";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_AgentType = "FWB";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var bil2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.AgentType];
			filter.IsActive = true;
			filter.Property = "CLD";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bil2.MatchesFilter(filterObj.Filter));
		}

		public void TestClone()
		{
			var filterObj = new ASYCUDAManifestBillFilterStrip(true);
			AssertNotNull(filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Country]);
			var clonedFilterObj = filterObj.Clone();
			AssertNotNull(clonedFilterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Country]);
			filterObj = new ASYCUDAManifestBillFilterStrip(false);
			AssertNull(filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Country]);
			clonedFilterObj = filterObj.Clone();
			AssertNull(clonedFilterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Country]);
		}

		public void TestMasterBillNumberFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_RN_NKCountry = "US";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "123";

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var bil2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.MasterBillNumber];
			filter.IsActive = true;
			filter.Property = "123";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bil2.MatchesFilter(filterObj.Filter));
		}

		public void TestPackReferenceNumberFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "PackRefNo1";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			var packLine1 = pack1.PackedItemForTesting();
			var entry = Factory.New<CusEntryNumber>();
			entry.CE_EntryType = "ASY";
			entry.CE_ParentID = packLine1.PK;
			entry.CE_EntryStatus = "SNT";
			entry.CE_RN_NKCountryCode = "";
			entry.CE_EntryNum = "1";
			entry.CE_ParentTable = "AsycudaPackedItem";

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			asyheader2.AMA_JobReference = "PackRefNo2";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packLine2 = pack2.PackedItemForTesting();
			var entry2 = Factory.New<CusEntryNumber>();
			entry2.CE_EntryType = "ASY";
			entry2.CE_ParentID = packLine2.PK;
			entry2.CE_EntryStatus = "SNT";
			entry2.CE_RN_NKCountryCode = "";
			entry2.CE_EntryNum = "2";
			entry2.CE_ParentTable = "AsycudaPackedItem";
			Factory.Save();
			pack1.LinePrice = 7.89;
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackReferenceNumber];
			filter.IsActive = true;
			filter.Property = "1";
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);

			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == bill1.PK));
			AssertEquals(false, headers.Any(x => x.PK == bill2.PK));
		}

		public void TestPackReferenceNumberTypeFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "PackRefNo1";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			var packLine1 = pack1.PackedItemForTesting();
			var entry = Factory.New<CusEntryNumber>();
			entry.CE_EntryType = "ASY";
			entry.CE_ParentID = packLine1.PK;
			entry.CE_EntryStatus = "SNT";
			entry.CE_RN_NKCountryCode = "";
			entry.CE_EntryNum = "1";
			entry.CE_ParentTable = "AsycudaPackedItem";

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "PackRefNo2";
			asyheader2.AMA_MasterBill = "456";
			asyheader2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packLine2 = pack2.PackedItemForTesting();
			var entry2 = Factory.New<CusEntryNumber>();
			entry2.CE_EntryType = "ABL";
			entry2.CE_ParentID = packLine2.PK;
			entry2.CE_EntryStatus = "SNT";
			entry2.CE_RN_NKCountryCode = "";
			entry2.CE_EntryNum = "2";
			entry2.CE_ParentTable = "AsycudaPackedItem";
			Factory.Save();
			pack1.LinePrice = 7.89;
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Singapore))
			{
				var filterObj = new ASYCUDAManifestBillFilterStrip();
				var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackReferenceNumberType];
				filter.IsActive = true;
				filter.Property2 = "ASY";
				var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				dbOnlyQuery.AddToFilter(filterObj.Filter);

				var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
				Assert(headers.Any(x => x.PK == bill1.PK));
				Assert(!headers.Any(x => x.PK == bill2.PK));
			}
		}

		[TestDate(2017, 6, 16)]
		public void TestDateFilters()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header1.AMA_E_DEP = ZDateTime.Today.AddDays(1);
			header1.AMA_E_ARV = ZDateTime.Today.AddDays(100);
			var bill1 = header1.Bills.AddNew();
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header2.AMA_E_DEP = ZDateTime.Today.AddDays(100);
			header2.AMA_E_ARV = ZDateTime.Today.AddDays(1);
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleDateFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.EstimatedDateOfDeparture];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			var filterObjArv = new ASYCUDAManifestBillFilterStrip();
			var filterArv = (ModuleDateFilter)filterObjArv[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.EstimatedTimeArrival];
			filterArv.IsActive = true;
			filterArv.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filterArv.Property2 = ZDateTime.Today.AddDays(7);
			Assert(!bill1.MatchesFilter(filterObjArv.Filter));
			Assert(bill2.MatchesFilter(filterObjArv.Filter));
		}

		public void TestCountryFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill = asyheader1.Bills.AddNew();

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			var bil2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNkFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Country];
			filter.IsActive = true;
			filter.Property = "ZA";
			Assert(!bill.MatchesFilter(filterObj.Filter));
			Assert(bil2.MatchesFilter(filterObj.Filter));
		}

		public void TestActiveStatusFilterCombinesManifestAndBill()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			asyheader1.AMA_JobReference = "M1";
			asyheader1.IsCancelled = false;
			var billM1a = asyheader1.Bills.AddNew();
			billM1a.IsCancelled = false;
			var billM1b = asyheader1.Bills.AddNew();
			billM1b.IsCancelled = true;

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			asyheader2.AMA_JobReference = "M2";
			asyheader2.IsCancelled = true;
			var billM2a = asyheader2.Bills.AddNew();
			billM2a.IsCancelled = false;
			Factory.Save();

			var filterStrip = new ASYCUDAManifestBillFilterStrip();
			filterStrip.AddActiveStatusFilters(typeof(AsycudaBill));
			filterStrip.LoadModuleFilters();
			var filter = (ModuleTextFilter)filterStrip["Active Status"];
			filter.IsActive = true;

			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					filter.Property = FilterStripBusinessObject.StatusAll;
					Assert(billM1a.MatchesFilter(filterStrip.Filter));
					Assert(billM1b.MatchesFilter(filterStrip.Filter));
					Assert(billM2a.MatchesFilter(filterStrip.Filter));

					filter.Property = FilterStripBusinessObject.StatusActive;
					Assert("billM1a is Active", billM1a.MatchesFilter(filterStrip.Filter));
					Assert("billM1b is Inactive", !billM1b.MatchesFilter(filterStrip.Filter));
					Assert("billM2a is Inactive due to Header being cancelled", !billM2a.MatchesFilter(filterStrip.Filter));
					AssertEquals("(ABL_ISACTIVE = 1 AND ABL_AMA IN (SELECT AMA_PK FROM DBO.ASYCUDAMANIFESTHEADER WHERE AMA_ISACTIVE = 1)) AND ABL_BOLTYPE <> 'BOL'", filterStrip.Filter.LiteralTextADO.ToUpper());

					filter.Property = FilterStripBusinessObject.StatusInactive;
					Assert(!billM1a.MatchesFilter(filterStrip.Filter));
					Assert(billM1b.MatchesFilter(filterStrip.Filter));
					Assert(billM2a.MatchesFilter(filterStrip.Filter));
					AssertEquals("(ABL_ISACTIVE = 0 OR ABL_AMA IN (SELECT AMA_PK FROM DBO.ASYCUDAMANIFESTHEADER WHERE AMA_ISACTIVE = 0)) AND ABL_BOLTYPE <> 'BOL'", filterStrip.Filter.LiteralTextADO.ToUpper());
				}
			});
		}

		public void TestManifestCustomsStatus()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference += "1";
			var entry = Factory.New<CusEntryNumber>();
			entry.CE_EntryType = "ASY";
			entry.Parent = asyheader1;
			entry.CE_EntryStatus = "SNT";
			entry.CE_RN_NKCountryCode = "ZA";
			var bill = asyheader1.Bills.AddNew();

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference += "2";
			var entry2 = Factory.New<CusEntryNumber>();
			entry2.CE_EntryType = "ASY";
			entry2.Parent = asyheader2;
			entry2.CE_EntryStatus = "SNT";
			entry2.CE_RN_NKCountryCode = "AU";
			var bil2 = asyheader2.Bills.AddNew();

			var asyheader3 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader3.AMA_JobReference += "3";
			var entry3 = Factory.New<CusEntryNumber>();
			entry3.CE_EntryType = "ASY";
			entry3.Parent = asyheader3;
			entry3.CE_EntryStatus = "SNT";
			entry3.CE_RN_NKCountryCode = "";
			var bil3 = asyheader3.Bills.AddNew();

			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var filterObj = new ASYCUDAManifestBillFilterStrip();
				var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ManifestStatus];
				filter.IsActive = true;
				filter.Property2 = "SNT";

				Assert(bill.MatchesFilter(filterObj.Filter));
				Assert(!bil2.MatchesFilter(filterObj.Filter));
				Assert(bil3.MatchesFilter(filterObj.Filter));
			}
		}

		public void TestManifestMessageStatus()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MessageStatus = "QUE";
			asyheader1.AMA_RN_NKCountry = "US";

			var bill = asyheader1.Bills.AddNew();
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ManifestMsgStatus];
			filter.IsActive = true;
			filter.Property2 = "QUE";
			Assert(bill.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestBillCargoStatus()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_CargoStatus = "FUL";

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.BillCargoStatus];
			filter.IsActive = true;
			filter.Property2 = "FUL";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestLocalReferenceNumberFilter()
		{
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			var bill1 = header1.Bills.AddNew();
			var entry1 = Factory.New<CusEntryNumber>();
			entry1.CE_EntryType = "LRN";
			entry1.Parent = header1;
			entry1.CE_EntryNum = "LRN1";

			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			var bill2 = header2.Bills.AddNew();
			var entry2 = Factory.New<CusEntryNumber>();
			entry2.CE_EntryType = "LRN";
			entry2.Parent = header2;
			entry2.CE_EntryNum = "LRN2";

			var header3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			var bill3 = header3.Bills.AddNew();
			var entry3 = Factory.New<CusEntryNumber>();
			entry3.CE_EntryType = "PRE";
			entry3.Parent = header3;
			entry3.CE_EntryNum = "LRN1";

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.LocalReferenceNumber];
			filter.IsActive = true;
			filter.Property = "LRN1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
			Assert(!bill3.MatchesFilter(filterObj.Filter));
		}

		public void TestNKFilters()
		{
			AssertBillNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Origin, AsycudaBillSchema.ABL_RL_NKOrigin, 11, AsycudaBillSchema.ABL_RL_NKOrigin.MaxLength);
			AssertBillNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.FinalDestination, AsycudaBillSchema.ABL_RL_NKFinalDestination, 12, AsycudaBillSchema.ABL_RL_NKFinalDestination.MaxLength);
			AssertBillNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalGoodsValueCurrency, AsycudaBillSchema.ABL_RX_NKFreightValueCurrency, 110, AsycudaBillSchema.ABL_RX_NKFreightValueCurrency.MaxLength);
			AssertBillNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalFreightValueCurrency, AsycudaBillSchema.ABL_RX_NKTransportValueCurrency, 111, AsycudaBillSchema.ABL_RX_NKTransportValueCurrency.MaxLength);
			AssertBillNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalInsuranceValueCurrency, AsycudaBillSchema.ABL_RX_NKInsuranceValueCurrency, 112, AsycudaBillSchema.ABL_RX_NKInsuranceValueCurrency.MaxLength);
			AssertBillNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalCustomsValueCurrency, AsycudaBillSchema.ABL_RX_NKCustomsValueCurrency, 113, AsycudaBillSchema.ABL_RX_NKCustomsValueCurrency.MaxLength);
		}

		public void TestTextFilters()
		{
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalQtyUnit, AsycudaBillSchema.ABL_ManifestUQ, 13);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalGrossWeightUnit, AsycudaBillSchema.ABL_GrossWeightUQ, 14);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalVolumeUnit, AsycudaBillSchema.ABL_VolumeUQ, 15);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.GoodsDescription, AsycudaBillSchema.ABL_GoodsDescription, 16);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Marks, AsycudaBillSchema.ABL_MarksAndNumbers, 17);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Remarks, AsycudaBillSchema.ABL_Remarks, 18);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PrepaidCollect, AsycudaBillSchema.ABL_PrepaidCollect, 19);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CarrierReference, AsycudaBillSchema.ABL_CarrierReference, 114);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.UCRNumber, AsycudaBillSchema.ABL_UCRNumber, 115);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.BillType, AsycudaBillSchema.ABL_BolType, 116);
			AssertBillTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.SpecialCargoCode, AsycudaBillSchema.ABL_SpecialCargoCode, 118);
		}

		public void TestNumericFilters()
		{
			AssertNumericFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalQty, AsycudaBillSchema.ABL_ManifestQty);
			AssertNumericFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalGrossWeight, AsycudaBillSchema.ABL_GrossWeight);
			AssertNumericFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalVolume, AsycudaBillSchema.ABL_Volume);
			AssertNumericFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalGoodsValue, AsycudaBillSchema.ABL_FreightValue);
			AssertNumericFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalFreightValue, AsycudaBillSchema.ABL_TransportValue);
			AssertNumericFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalInsuranceValue, AsycudaBillSchema.ABL_InsuranceValue);
			AssertNumericFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TotalCustomsValue, AsycudaBillSchema.ABL_CustomsValue);
		}

		public void TestBillCountryTextFilters()
		{
			AssertBillCountryTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.BillIssuer, AsycudaBillSchema.ABL_BillIssuer, 10);
			AssertBillCountryTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ShipmentType, AsycudaBillSchema.ABL_ShipmentType, 11);
			AssertBillCountryTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.GoodsLocation, AsycudaBillSchema.ABL_GoodsLocation, 12);
			AssertBillCountryTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.LocationInformation, AsycudaBillSchema.ABL_LocationInformation, 13);
		}

		public void TestPackLineFilters()
		{
			AssertPackLineNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackOrigin, AsycudaPackedItemSchema.API_RN_NKGoodsOrigin, 23);
			AssertPackLineTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCustomsDescription, AsycudaPackedItemSchema.API_GoodsDescription, 24);
			AssertPackLineTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCustomsTariff, AsycudaPackedItemSchema.API_Tariff, 25);
			AssertNumericPackLineFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCustomsQty, AsycudaPackedItemSchema.API_CustomsQty);
			AssertNumericPackLineFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCustomsValue, AsycudaPackedItemSchema.API_CustomsValue);
			AssertNumericPackLineFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCustomsDutyAmount, AsycudaPackedItemSchema.API_DutyAmount);
			AssertNumericPackLineFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCustomsTaxAmount, AsycudaPackedItemSchema.API_TaxAmount);
		}

		public void TestPackFilters()
		{
			AssertNumericPackFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackQty, AsycudaPackSchema.APA_PackQty);
			AssertNumericPackFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackWeight, AsycudaPackSchema.APA_Weight);
			AssertNumericPackFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackVolume, AsycudaPackSchema.APA_Volume);
			AssertPackTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackGoodsDescription, AsycudaPackSchema.APA_GoodsDescription, 31);
			AssertPackTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCommodityCode, AsycudaPackSchema.APA_CommodityCode, 32);
		}

		public void TestHeaderTextFilters()
		{
			AssertTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.JobReference, AsycudaManifestHeaderSchema.AMA_JobReference, 4);
			AssertTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.VesselName, AsycudaManifestHeaderSchema.AMA_VesselName, 5);
			AssertTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.VoyageNumber, AsycudaManifestHeaderSchema.AMA_Voyage, 6);
			AssertTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.TransportMode, AsycudaManifestHeaderSchema.AMA_TransportMode, 7);
			AssertHeaderTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ContainerMode, AsycudaManifestHeaderSchema.AMA_ContainerMode, 8);
			AssertHeaderTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.VehicleReg, AsycudaManifestHeaderSchema.AMA_VehicleRegistration, 9);
			AssertCountryHeaderTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Nature, AsycudaManifestHeaderSchema.AMA_Nature);
			AssertCountryHeaderNKFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.FirstArrivalPort, AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival);
			AssertCountryHeaderTextFilter(Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CarrierCode, AsycudaManifestHeaderSchema.AMA_CarrierCode);
		}

		public void TestManifestTypeFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			var bill1 = asyheader1.Bills.AddNew();
			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			var bill2 = asyheader2.Bills.AddNew();
			var asyheader3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.China, "ASY");
			var bill3 = asyheader3.Bills.AddNew();
			var asyheader4 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "EFM");
			var bill4 = asyheader4.Bills.AddNew();
			var asyheader5 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Canada, "ASY");
			var bill5 = asyheader5.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = filterObj[AsycudaFilterStrip.FilterConstants.ManifestType] as DataGroupingRelatedFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			AssertEquals("filter.Category", FilterCategories.ModesAndTypes, filter.Category);
			AssertEquals("filter.Description", "Manifest Type", filter.Description);
			AssertEquals("filter.MultilingualDescription", "Manifest Type", filter.MultilingualDescription);
			AssertEquals("filter.Property2FieldType", FieldType.TextDropEdit, filter.Property2FieldType);
			AssertEquals("filter.Property2MaxLength", AsycudaManifestHeaderSchema.AMA_ManifestType.MaxLength, filter.Property2MaxLength);
			AssertEquals("filter.Property2ResourceString.Caption", "Manifest Type", filter.Property2ResourceString.Caption);
			AssertEquals("filter.UseProperty2ListGetterWhenProperty1IsEmpty", true, filter.UseProperty2ListGetterWhenProperty1IsEmpty);

			filter.Property1 = Core.Constants.CountryCodes.UnitedStates;
			filter.Property2 = ZString.Empty;
			var zq = filterObj.Filter;
			var bills = Factory.Load<AsycudaBill>(zq);
			AssertEquals(5, bills.Length);
			Assert(bill1.MatchesFilter(zq));
			Assert(bill2.MatchesFilter(zq));
			Assert(bill3.MatchesFilter(zq));
			Assert(bill4.MatchesFilter(zq));
			Assert(bill5.MatchesFilter(zq));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "ASY";
			zq = filterObj.Filter;
			bills = Factory.Load<AsycudaBill>(zq);
			AssertEquals(2, bills.Length);
			Assert(!bill1.MatchesFilter(zq));
			Assert(!bill2.MatchesFilter(zq));
			Assert(bill3.MatchesFilter(zq));
			Assert(!bill4.MatchesFilter(zq));
			Assert(bill5.MatchesFilter(zq));

			filter.Property1 = Core.Constants.CountryCodes.Canada;
			filter.Property2 = "ASY";
			zq = filterObj.Filter;
			bills = Factory.Load<AsycudaBill>(zq);
			AssertEquals(1, bills.Length);
			Assert(!bill1.MatchesFilter(zq));
			Assert(!bill2.MatchesFilter(zq));
			Assert(!bill3.MatchesFilter(zq));
			Assert(!bill4.MatchesFilter(zq));
			Assert(bill5.MatchesFilter(zq));

			filter.Property1 = Core.Constants.CountryCodes.SouthAfrica;
			filter.Property2 = "ALH";
			zq = filterObj.Filter;
			bills = Factory.Load<AsycudaBill>(zq);
			AssertEquals(1, bills.Length);
			Assert(!bill1.MatchesFilter(zq));
			Assert(bill2.MatchesFilter(zq));
			Assert(!bill3.MatchesFilter(zq));
			Assert(!bill4.MatchesFilter(zq));
			Assert(!bill5.MatchesFilter(zq));

			var filter2 = filterObj[AsycudaFilterStrip.FilterConstants.ManifestType] as DataGroupingRelatedFilter;
			filter2.IsActive = true;
			filter2.Property1 = Core.Constants.CountryCodes.SouthAfrica;
			filter2.Property2 = "ALH";
			AssertSame("Property2List Cached", filter.Property2List, filter2.Property2List);
		}

		public void TestCustomsOfficeFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.Bills.RemoveAndDeleteAll();
			asyheader1.AMA_JobReference = "office1";
			asyheader1[AsycudaManifestHeaderSchema.AMA_CustomsOffice] = "1";
			asyheader1.RegistrationNumber = "1";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "MB1";
			bill1.ABL_BillIssueDate = ZDate.Today;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.Bills.RemoveAndDeleteAll();
			asyheader2.AMA_JobReference = "office2";
			asyheader2[AsycudaManifestHeaderSchema.AMA_CustomsOffice] = "2";
			asyheader2.RegistrationNumber = "2";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_BillNumber = "MB2";
			bill2.ABL_BillIssueDate = ZDate.Today;

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsOffice];
			filter.IsActive = true;
			filter.Property2 = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestPackMessageStatusFilter()
		{
			var asyheader1 = Factory.New<DummyAsycudaManifestHeader>();
			asyheader1.AMA_ManifestType = "XYZ";
			asyheader1.AMA_JobReference = "MAN000001";
			var bill = asyheader1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packLine = pack.PackedItemForTesting();
			packLine.API_MessageStatus = string.Empty;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packLine2 = pack2.PackedItem;
			packLine2.API_MessageStatus = "NOT";

			var asyheader3 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader3.AMA_JobReference = "MAN000003";
			var bill3 = asyheader3.Bills.AddNew();
			var pack3 = bill3.Packs.AddNew();
			var packLine3 = pack3.PackedItem;
			packLine3.API_MessageStatus = "SNT";

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackMessageStatus];
			filter.IsActive = true;
			filter.Property2 = "NOT";
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);

			var billCountries = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertContainsExactElementsInAnyOrder(new[] { bill2.PK }, billCountries.Select(b => b.PK));
		}

		public void TestCustomsNumberAndType()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.CustomsEntryNumber = "1";
			bill1.CustomsEntryNumberType = "3";

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.CustomsEntryNumber = "2";
			bill2.CustomsEntryNumberType = "4";
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			var filterObj2 = new ASYCUDAManifestBillFilterStrip();
			var filter2 = (CountryRelatedFilter)filterObj2[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumberType];
			filter2.IsActive = true;
			filter2.Property2 = "3";
			Assert(bill1.MatchesFilter(filterObj2.Filter));
			Assert(!bill2.MatchesFilter(filterObj2.Filter));
		}

		public void TestCustomsNumber_IsBlank()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert("billBlankEntry does have a Customs Entry Number associated to the bill but it is blank - this manifest should be found by this filter", billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry does not have a Customs Entry Number associated to the bill - should be found by this filter", billNoEntry.MatchesFilter(filterObj.Filter));
			Assert("billUPSEntry does have a Customs Entry Number - should be excluded by filter", !billUPSEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsNumber_IsNotBlank()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Assert("billUPSEntry does have a Customs Entry Number - should be included by filter", billUPSEntry.MatchesFilter(filterObj.Filter));
			Assert("billBlankEntry does have a Customs Entry Number associated to the bill but it is blank - this manifest should be excluded by this filter", !billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry does not have a Customs Entry Number associated to the bill - should be excluded by this filter", !billNoEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsNumber_Contains()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "20B";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Assert("billUPSEntry customs no (UPS20B2423) contains this sequence", billUPSEntry.MatchesFilter(filterObj.Filter));
			Assert("billDHLEntry Excluded. Number does not contain 20B", !billDHLEntry.MatchesFilter(filterObj.Filter));
			Assert("billBlankEntry Excluded. Number is Blank", !billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry Excluded. Number does not exist", !billNoEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsNumber_NotContains()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "20B";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Assert("billUPSEntry Excluded. customs no (UPS20B2423) contains this sequence", !billUPSEntry.MatchesFilter(filterObj.Filter));
			Assert("billDHLEntry Included. Number does not contain 20B", billDHLEntry.MatchesFilter(filterObj.Filter));
			Assert("billBlankEntry Included. Number is Blank", billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry Included. Number does not exist", billNoEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsNumber_Exact()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "DHL27491";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert("billDHLEntry matches this filter", billDHLEntry.MatchesFilter(filterObj.Filter));
			Assert("billUPSEntry Excluded. Number does not match", !billUPSEntry.MatchesFilter(filterObj.Filter));
			Assert("billBlankEntry Excluded. Number is Blank", !billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry Excluded. Number does not exist", !billNoEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsNumber_NotEqual()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "DHL27491";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert("billDHLEntry Excluded. manifestHeader4 matches this filter", !billDHLEntry.MatchesFilter(filterObj.Filter));
			Assert("billUPSEntry Included. Number is not DHL27491", billUPSEntry.MatchesFilter(filterObj.Filter));
			Assert("billBlankEntry Included. Number is Blank", billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry Included. Number does not exist", billNoEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsNumber_StartsWith()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "UPS";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Assert("billUPSEntry customs no (UPS20B2423) starts with this sequence", billUPSEntry.MatchesFilter(filterObj.Filter));
			Assert("billUPSEntry2 customs no (UPS47K2850) starts with this sequence", billUPSEntry2.MatchesFilter(filterObj.Filter));
			Assert("billDHLEntry Excluded. Number does not starts with UPS", !billDHLEntry.MatchesFilter(filterObj.Filter));
			Assert("billBlankEntry Excluded. Number is Blank", !billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry Excluded. Number does not exist", !billNoEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsNumber_DoesNotStartWith()
		{
			SetUpManifestsForFilter();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsNumber];
			filter.IsActive = true;
			filter.Property = "UPS";
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Assert("billUPSEntry customs no (UPS20B2423) starts with this sequence", !billUPSEntry.MatchesFilter(filterObj.Filter));
			Assert("billUPSEntry2 customs no (UPS47K2850) starts with this sequence", !billUPSEntry2.MatchesFilter(filterObj.Filter));
			Assert("billDHLEntry Included. Number does not start with UPS", billDHLEntry.MatchesFilter(filterObj.Filter));
			Assert("billBlankEntryIncluded. Number is Blank", billBlankEntry.MatchesFilter(filterObj.Filter));
			Assert("billNoEntry Included. Number does not exist", billNoEntry.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsJobNumber()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.CustomsJobNumber = "1";

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.CustomsJobNumber = "2";
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsJobNumber];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestDiscountValueCurrency()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";

			var bill1 = asyheader1.Bills.AddNew();
			bill1.DiscountValueCurrency = "1";

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.DiscountValueCurrency = "2";
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.DiscountValueCurrency];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestOtherChargesValueCurrency()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";

			var bill1 = asyheader1.Bills.AddNew();
			bill1.OtherChargesValueCurrency = "1";
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.OtherChargesValueCurrency = "2";
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.OtherChargesValueCurrency];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestPackCustomsStatus()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var asyheader1 = Factory.New<DummyAsycudaManifestHeader>();
				asyheader1.AMA_ManifestType = "XYZ";
				asyheader1.AMA_JobReference = 22 + "";
				var bill = asyheader1.Bills.AddNew();
				var pack = bill.Packs.AddNew();
				pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
				var packLine = pack.PackedItemForTesting();
				packLine[AsycudaPackedItemSchema.API_PackStatus] = "1";

				var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				asyheader2.FillWithValidTestData();
				asyheader2.AMA_JobReference = 22 + 1 + "1";
				var bill2 = asyheader2.Bills.AddNew();
				var pack2 = bill2.Packs.AddNew();
				pack2.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
				var packLine2 = pack2.PackedItemForTesting();
				packLine2[AsycudaPackedItemSchema.API_PackStatus] = "2";
				Factory.Save();

				var filterObj = new ASYCUDAManifestBillFilterStrip();
				var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.PackCustomsStatus];
				filter.IsActive = true;
				filter.Property2 = "1";

				var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				dbOnlyQuery.AddToFilter(filterObj.Filter);

				var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
				AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any());
				AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill.PK));
				AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill2.PK));
			}
		}

		public void TestRegistrationNumberFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			var cusEntryNum1 = Factory.New<CusEntryNumber>();
			cusEntryNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			cusEntryNum1.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			cusEntryNum1.CE_ParentID = bill1.PK;
			cusEntryNum1.CE_EntryNum = "1";
			cusEntryNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;
			asyheader1.RegistrationNumber = "1";

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			var shippingAgent2 = Factory.New<OrgHeader>();
			shippingAgent2.OH_Code = "DDD";
			shippingAgent2.OH_FullName = "SomethingElse";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shippingAgent2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "CT";
			orgAddress2.OA_State = "CT";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "ZA";
			asyheader2.AMA_OA_ShippingAgent = orgAddress2.PK;
			asyheader2.RegistrationNumber = "2";

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.BillRegistrationNumber];
			filter.IsActive = true;
			filter.Property = bill1.RegistrationNumber;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestManifestRegistrationNumberFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;
			asyheader1.RegistrationNumber = "1";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			var shippingAgent2 = Factory.New<OrgHeader>();
			shippingAgent2.OH_Code = "DDD";
			shippingAgent2.OH_FullName = "SomethingElse";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shippingAgent2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "CT";
			orgAddress2.OA_State = "CT";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "ZA";
			asyheader2.AMA_OA_ShippingAgent = orgAddress2.PK;
			asyheader2.RegistrationNumber = "2";
			var bill2 = asyheader2.Bills.AddNew();

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ManifestRegistrationNumber];
			filter.IsActive = true;
			filter.Property = asyheader1.RegistrationNumber;

			var bills = Factory.Load<AsycudaBill>(filterObj.Filter);
			AssertEquals(true, bills.Any());
			AssertEquals(true, bills.Any(x => x.PK == bill1.PK));
			AssertEquals(false, bills.Any(x => x.PK == bill2.PK));
		}

		public void TestShippingAgentAddressAndNameFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			var shippingAgent2 = Factory.New<OrgHeader>();
			shippingAgent2.OH_Code = "DDD";
			shippingAgent2.OH_FullName = "SomethingElse";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shippingAgent2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "CT";
			orgAddress2.OA_State = "CT";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "ZA";
			asyheader2.AMA_OA_ShippingAgent = orgAddress2.PK;

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ShippingAgentAddress];
			filter.IsActive = true;
			filter.Property = orgAddress.PK;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			var filterObj2 = new ASYCUDAManifestBillFilterStrip();
			var filter2 = (ModuleTextFilter)filterObj2[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ShippingAgentName];
			filter2.IsActive = true;
			filter2.Property = shippingAgent1.OH_FullName;

			Assert(bill1.MatchesFilter(filterObj2.Filter));
			Assert(!bill2.MatchesFilter(filterObj2.Filter));
		}

		public void TestRegistrationDateFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;
			asyheader1.RegistrationNumber = "1";
			asyheader1.RegistrationDate = ZDateTime.UtcNow;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			var shippingAgent2 = Factory.New<OrgHeader>();
			shippingAgent2.OH_Code = "DDD";
			shippingAgent2.OH_FullName = "SomethingElse";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shippingAgent2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "CT";
			orgAddress2.OA_State = "CT";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "ZA";
			asyheader2.AMA_OA_ShippingAgent = orgAddress2.PK;
			asyheader2.RegistrationNumber = "2";
			asyheader2.RegistrationDate = ZDateTime.UtcNow.AddDays(-10);

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleDateFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.RegistrationDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(-5);
			filter.Property2 = ZDateTime.UtcNow.AddDays(5);

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestManifestRegistrationDateFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "MB1";
			asyheader1.AMA_MasterBillIssueDate = ZDate.Today;
			var bill1 = asyheader1.Bills.AddNew();
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "MB2";
			asyheader2.AMA_MasterBillIssueDate = ZDate.Today.AddDays(-10);
			var bill2 = asyheader2.Bills.AddNew();

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleDateFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.IssueDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(-5);
			filter.Property2 = ZDateTime.UtcNow.AddDays(5);

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestBillMsgStatusFilter_NotSent()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			bill1.ABL_CustomsValue = 3;
			bill1.ABL_MessageStatus = string.Empty;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_CustomsValue = 6;
			bill2.ABL_MessageStatus = "NOT";
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.BillMsgStatus];
			filter.IsActive = true;
			filter.Property2 = "NOT";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestManifestMsgStatusFilter_NotSent()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_MessageStatus = string.Empty;
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			bill1.ABL_CustomsValue = 3;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			asyheader1.AMA_MessageStatus = "NOT";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_CustomsValue = 6;
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.ManifestMsgStatus];
			filter.IsActive = true;
			filter.Property2 = "NOT";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsValueFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			bill1.ABL_CustomsValue = 3;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_CustomsValue = 6;
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNumberRangeFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CustomsValue];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 4;
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestConsigneeFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "CCC";
			consignee1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = consignee1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_OA_Consignee = orgAddress.PK;

			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;

			var asyheader2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "DDD";
			consignee2.OH_FullName = "Consignee2";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = consignee2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "NY";
			orgAddress2.OA_State = "NY";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "US";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_OA_Consignee = orgAddress2.PK;

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Consignee];
			filter.IsActive = true;
			filter.Property = consignee1.PK;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestShipperFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var shipper1 = Factory.New<OrgHeader>();
			shipper1.OH_Code = "CCC";
			shipper1.OH_FullName = "Shipper";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shipper1.PK;
			orgAddress.OA_Address1 = "Shipper Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_OA_Shipper = orgAddress.PK;

			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var shipper2 = Factory.New<OrgHeader>();
			shipper2.OH_Code = "DDD";
			shipper2.OH_FullName = "Shipper2";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shipper2.PK;
			orgAddress2.OA_Address1 = "Shipper Address2";
			orgAddress2.OA_City = "NY";
			orgAddress2.OA_State = "NY";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "US";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_OA_Shipper = orgAddress2.PK;

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.Shipper];
			filter.IsActive = true;
			filter.Property = shipper1.PK;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestNotifyPartyFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var notifyParty1 = Factory.New<OrgHeader>();
			notifyParty1.OH_Code = "CCC";
			notifyParty1.OH_FullName = "NotifyParty";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = notifyParty1.PK;
			orgAddress.OA_Address1 = "NotifyParty Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_OA_NotifyParty = orgAddress.PK;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_Code = "DDD";
			notifyParty2.OH_FullName = "NotifyParty2";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = notifyParty2.PK;
			orgAddress2.OA_Address1 = "NotifyParty Address2";
			orgAddress2.OA_City = "NY";
			orgAddress2.OA_State = "NY";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "US";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_OA_NotifyParty = orgAddress2.PK;

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.NotifyParty];
			filter.IsActive = true;
			filter.Property = notifyParty1.PK;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestLoadDischargeFilter()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN000001";
			header1.AMA_RL_NKPortOfLoading = "AUSYD";
			header1.AMA_RL_NKPortOfDischarge = "JPTKY";
			var bill1 = header1.Bills.AddNew();
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN000002";
			header2.AMA_RL_NKPortOfLoading = "AUMEL";
			header2.AMA_RL_NKPortOfDischarge = "SGSIN";
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleLocationFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.LoadDischarge];
			AssertEquals("Loading", filter.ItemDescription1.Caption);
			AssertEquals("Discharge", filter.ItemDescription2.Caption);
			filter.IsActive = true;

			filter.Property1 = "AU";
			filter.Property2 = ZString.Empty;
			AssertEquals("header1 match filter", true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, bill2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "SGSIN";
			AssertEquals("header1 match filter", false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, bill2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "USLAX";
			AssertEquals("header1 match filter", false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, bill2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "JP";
			AssertEquals("header1 match filter", true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, bill2.MatchesFilter(filterObj.Filter));

			filter.Property1 = "AUMEL";
			AssertEquals("header1 match filter", false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, bill2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZString.Empty;
			AssertEquals("header1 match filter", true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, bill2.MatchesFilter(filterObj.Filter));
		}

		[TestDate(2017, 6, 16)]
		public void TestAuditFilters()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN000001";
			header1.AMA_E_DEP = ZDateTime.Today.AddDays(1);
			header1.AMA_E_ARV = ZDateTime.Today.AddDays(100);
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_SystemCreateUser = "ZZ";
			bill1.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN000002";
			header2.AMA_E_DEP = ZDateTime.Today.AddDays(100);
			header2.AMA_E_ARV = ZDateTime.Today.AddDays(1);
			var bill2 = header2.Bills.AddNew();
			bill2.ABL_SystemCreateUser = "XX";
			bill2.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(8);
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filterCreateOnWeb = (ModuleTextFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CreatedOnWeb];
			filterCreateOnWeb.IsActive = true;
			filterCreateOnWeb.Property = "WEB";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			filterObj = new ASYCUDAManifestBillFilterStrip();
			var filterCreateTime = (ModuleDateFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CreatedTime];
			filterCreateTime.IsActive = true;
			filterCreateTime.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filterCreateTime.Property2 = ZDateTime.Today.AddDays(7);
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			filterObj = new ASYCUDAManifestBillFilterStrip();
			var filterCreateUser = (ModuleNkFilter)filterObj[Module.ASYCUDAManifestBillFilterStrip.FilterConstants.CreatingUser];
			filterCreateUser.IsActive = true;
			filterCreateUser.Property = "ZZ";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomFieldFilter()
		{
			var filterCollection = new ASYCUDAManifestBillFilterStrip().ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);

			CreateWorkflowWithCustomFields(WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode);

			Factory.Save();

			filterCollection = new ASYCUDAManifestBillFilterStrip().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["boolField"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ASYCUDAManifestBillFilterStrip();

		void AssertNumericFilter(ZString filterName, SchemaNumericColumn schemaCol)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = filterName + "1";
			var bill = asyheader1.Bills.AddNew();
			bill[schemaCol] = 2;
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = filterName + "2";
			var bill2 = asyheader2.Bills.AddNew();
			bill2[schemaCol] = 8;
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNumberRangeFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 3;
			Assert(bill.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		void AssertTextFilter(ZString filterName, SchemaStringColumn schemaCol, int index)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = string.Format("{0}1", filterName);
			asyheader1[schemaCol] = "1";
			var bill = asyheader1.Bills.AddNew();
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = string.Format("{0}2", filterName);
			asyheader2[schemaCol] = "2";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		void AssertBillTextFilter(ZString filterName, SchemaStringColumn schemaCol, int index, int stringLength = 1)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = string.Format("{0}1", filterName);
			var bill = asyheader1.Bills.AddNew();
			bill[schemaCol] = "1".PadRight(stringLength, '0');
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = string.Format("{0}2", filterName);
			var bill2 = asyheader2.Bills.AddNew();
			bill2[schemaCol] = "2".PadRight(stringLength, '0');
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1".PadRight(stringLength, '0');

			Assert(bill.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		void AssertBillNKFilter(ZString filterName, SchemaStringColumn schemaCol, int index, int stringLength = 1)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = string.Format("{0}1", filterName);
			var bill = asyheader1.Bills.AddNew();
			bill[schemaCol] = "1".PadRight(stringLength, '0');
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = string.Format("{0}2", filterName);
			var bill2 = asyheader2.Bills.AddNew();
			bill2[schemaCol] = "2".PadRight(stringLength, '0');
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNkFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1".PadRight(stringLength, '0');

			Assert(bill.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		void AssertBillCountryTextFilter(ZString filterName, SchemaStringColumn schemaCol, int index)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = string.Format("{0}1", filterName);
			var bill = asyheader1.Bills.AddNew();
			bill[schemaCol] = "1";
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = string.Format("{0}2", filterName);
			var bill2 = asyheader2.Bills.AddNew();
			bill2[schemaCol] = "2";
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		void AssertNumericPackLineFilter(ZString filterName, SchemaNumericColumn schemaCol)
		{
			var asyheader1 = Factory.New<DummyAsycudaManifestHeader>();
			asyheader1.AMA_ManifestType = "XYZ";
			asyheader1.AMA_JobReference = string.Format("{0}1", filterName);
			var bill = asyheader1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packLine = pack.PackedItemForTesting();
			packLine[schemaCol] = 2;
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = string.Format("{0}2", filterName);
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packLine2 = pack2.PackedItemForTesting();
			packLine2[schemaCol] = 10;
			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNumberRangeFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 3;
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);

			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any());
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill.PK));
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill2.PK));
		}

		void AssertNumericPackFilter(ZString filterName, SchemaNumericColumn schemaCol)
		{
			var asyheader1 = Factory.New<DummyAsycudaManifestHeader>();
			asyheader1.AMA_ManifestType = "XYZ";
			asyheader1.AMA_JobReference = string.Format("{0}1", filterName);
			var bill = asyheader1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack[schemaCol] = 2;
			var aPI = pack.PackedItemForTesting();

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = string.Format("{0}2", filterName);
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2[schemaCol] = 10;
			var aPI2 = pack2.PackedItemForTesting();

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNumberRangeFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 3;

			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);

			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any());
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill.PK));
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill2.PK));
		}

		void AssertPackLineTextFilter(ZString filterName, SchemaStringColumn schemaCol, int index)
		{
			var asyheader1 = Factory.New<DummyAsycudaManifestHeader>();
			asyheader1.AMA_ManifestType = "XYZ";
			asyheader1.AMA_JobReference = index + "";
			var bill = asyheader1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packLine = pack.PackedItemForTesting();
			packLine[schemaCol] = "1";
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = index + 1 + "1";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packLine2 = pack2.PackedItemForTesting();
			packLine2[schemaCol] = "2";
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);

			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any());
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill.PK));
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill2.PK));
		}

		void AssertPackLineNKFilter(ZString filterName, SchemaStringColumn schemaCol, int index)
		{
			var asyheader1 = Factory.New<DummyAsycudaManifestHeader>();
			asyheader1.AMA_ManifestType = "XYZ";
			asyheader1.AMA_JobReference = index + "";
			var bill = asyheader1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packLine = pack.PackedItemForTesting();
			packLine[schemaCol] = "1";
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = index + 1 + "1";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packLine2 = pack2.PackedItemForTesting();
			packLine2[schemaCol] = "2";
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNkFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);

			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any());
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill.PK));
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill2.PK));
		}

		void AssertPackTextFilter(ZString filterName, SchemaStringColumn schemaCol, int index)
		{
			var asyheader1 = Factory.New<DummyAsycudaManifestHeader>();
			asyheader1.AMA_ManifestType = "XYZ";
			asyheader1.AMA_JobReference = index + "";
			var bill = asyheader1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack[schemaCol] = "1";
			var aPI = pack.PackedItemForTesting();
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = index + 1 + "1";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2[schemaCol] = "2";
			var aPI2 = pack2.PackedItemForTesting();
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);

			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any());
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill.PK));
			AssertEquals("Pack is not supported when SupportsAsycudaPacks is false", false, headers.Any(x => x.PK == bill2.PK));
		}

		void AssertHeaderTextFilter(ZString filterName, SchemaStringColumn schemaCol, int index)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = index + "";
			asyheader1[schemaCol] = "1";
			var bill = asyheader1.Bills.AddNew();
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = index + 1 + "1";
			asyheader2[schemaCol] = "2";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";

			var headers = Factory.Load<AsycudaBill>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == bill.PK));
			AssertEquals(false, headers.Any(x => x.PK == bill2.PK));
		}

		void AssertCountryHeaderTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.Bills.RemoveAndDeleteAll();
			asyheader1.AMA_JobReference = filterName + "1";
			asyheader1[schemaCol] = "1";
			asyheader1.RegistrationNumber = "1";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "MB1";
			bill1.ABL_BillIssueDate = ZDate.Today;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.Bills.RemoveAndDeleteAll();
			asyheader2.AMA_JobReference = filterName + "2";
			asyheader2[schemaCol] = "2";
			asyheader2.RegistrationNumber = "2";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_BillNumber = "MB2";
			bill2.ABL_BillIssueDate = ZDate.Today;

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		void AssertCountryHeaderNKFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			asyheader1.Bills.RemoveAndDeleteAll();
			asyheader1.AMA_JobReference = filterName + "1";
			asyheader1[schemaCol] = "1";
			asyheader1.RegistrationNumber = "1";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "MB1";
			bill1.ABL_BillIssueDate = ZDate.Today;

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			asyheader2.Bills.RemoveAndDeleteAll();
			asyheader2.AMA_JobReference = filterName + "2";
			asyheader2[schemaCol] = "2";
			asyheader2.RegistrationNumber = "2";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_BillNumber = "MB2";
			bill2.ABL_BillIssueDate = ZDate.Today;

			Factory.Save();

			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleNkFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		void CreateWorkflowWithCustomFields(ZString workflowDescriptorCode)
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = workflowDescriptorCode;

			var customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "stringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "intField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var customField3 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "boolField";
			customField3.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var customField4 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "dateTimeField";
			customField4.XC_Type = AddOnColumnDataType.Codes.Datetime;

			WorkflowCustomFieldsFilter.ClearCache();
		}

		void SetUpManifestsForFilter()
		{
			var manifestHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN000001";
			manifestHeader.AMA_MasterBill = "123";
			manifestHeader.AMA_ManifestType = "MGI";
			manifestHeader.AMA_MessageStatus = "QUE";
			billNoEntry = manifestHeader.Bills.AddNew();

			var manifestHeaderWithBlankCustomsNo = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeaderWithBlankCustomsNo.AMA_JobReference = "MAN000002";
			manifestHeaderWithBlankCustomsNo.AMA_MasterBill = "271";
			manifestHeaderWithBlankCustomsNo.AMA_ManifestType = "MGI";
			manifestHeaderWithBlankCustomsNo.AMA_MessageStatus = "QUE";
			billBlankEntry = manifestHeaderWithBlankCustomsNo.Bills.AddNew();
			billBlankEntry.CustomsEntryNumber = "";
			billBlankEntry.CustomsEntryNumberType = "3";

			var manifestHeader4 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeader4.AMA_JobReference = "MAN000003";
			manifestHeader4.AMA_MasterBill = "M7290";
			manifestHeader4.AMA_ManifestType = "MGI";
			manifestHeader4.AMA_MessageStatus = "QUE";
			billDHLEntry = manifestHeader4.Bills.AddNew();
			billDHLEntry.CustomsEntryNumber = "DHL27491";
			billDHLEntry.CustomsEntryNumberType = "3";

			var manifestHeaderWithCustomsNo = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeaderWithCustomsNo.AMA_JobReference = "MAN000004";
			manifestHeaderWithCustomsNo.AMA_MasterBill = "128";
			manifestHeaderWithCustomsNo.AMA_ManifestType = "MGI";
			manifestHeaderWithCustomsNo.AMA_MessageStatus = "QUE";
			billUPSEntry = manifestHeaderWithCustomsNo.Bills.AddNew();
			billUPSEntry.CustomsEntryNumber = "UPS20B2423";
			billUPSEntry.CustomsEntryNumberType = "3";

			var manifestHeader5 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeader5.AMA_JobReference = "MAN000005";
			manifestHeader5.AMA_MasterBill = "310";
			manifestHeader5.AMA_ManifestType = "MGI";
			manifestHeader5.AMA_MessageStatus = "QUE";
			billUPSEntry2 = manifestHeader5.Bills.AddNew();
			billUPSEntry2.CustomsEntryNumber = "UPS47K2850";
			billUPSEntry2.CustomsEntryNumberType = "3";
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposableGlobalManifestApplicationBusinessProvider = ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", ObjectFactory.Get<IEnumerable>("GlobalManifestApplicationBusinessProvider").Cast<ApplicationBusinessProvider>().Append(new DummyApplicationBusinessProvider("XYZ", Core.Constants.CountryCodes.UnitedStates)).ToList());
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableGlobalManifestApplicationBusinessProvider.Dispose();
		}

		AsycudaBill billNoEntry;
		AsycudaBill billBlankEntry;
		AsycudaBill billDHLEntry;
		AsycudaBill billUPSEntry;
		AsycudaBill billUPSEntry2;
		IDisposable disposableGlobalManifestApplicationBusinessProvider;
	}
}
