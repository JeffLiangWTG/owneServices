using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class EManifestColumnsAndFiltersProviderTest : ZFilterStripControlTest
	{
		public void TestEManifestHouseLastestD4NoticeFilter()
		{
			var filter = (ModuleTextFilter)filters["CA Master Latest D4 Notice"];
			AssertNotNull("CA Master Latest D4 Notice", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, "8000", consol1);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, "8000", consol2, consol3, consol4, consol5, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.IsNotBlank, string.Empty, consol1, consol2);
			AssertStatusQuery(filter, SQLComparisonOperator.IsBlank, string.Empty, consol3, consol4, consol5, consol6);
		}

		public void TestEManifestMasterLastestD4NoticeFilter()
		{
			var filter = (ModuleTextFilter)filters["CA Master Latest D4 Notice"];
			AssertNotNull("CA Master Latest D4 Notice", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, "8000", consol1);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, "8000", consol2, consol3, consol4, consol5, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.IsNotBlank, string.Empty, consol1, consol2);
			AssertStatusQuery(filter, SQLComparisonOperator.IsBlank, string.Empty, consol3, consol4, consol5, consol6);
		}

		public void TestEManifestCoseJobStatus()
		{
			var filter = (ModuleTextFilter)filters["CA Close Status"];
			AssertNotNull("CA Close Status filter", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.Clear, consol1, consol5);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, EManifestForwarderJobStatusList.Codes.Clear, consol2, consol3, consol4, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.Error, consol2);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, EManifestForwarderJobStatusList.Codes.Error, consol1, consol3, consol4, consol5, consol6);
		}

		public void TestEManifestCloseMessageStatus()
		{
			var filter = (ModuleTextFilter)filters["CA Close Msg. Sta"];
			AssertNotNull("CA Close Msg. Sta", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ClearOriginal, consol1, consol5);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ClearOriginal, consol2, consol3, consol4, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ErrorOriginal, consol2);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ErrorOriginal, consol1, consol3, consol4, consol5, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.AwaitingOriginal, consol4);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.AwaitingOriginal, consol1, consol2, consol3, consol5, consol6);

			const string notSentCode = "NST";
			AssertStatusQuery(filter, SQLComparisonOperator.Equal, notSentCode, consol3);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, notSentCode, consol1, consol2, consol4, consol5, consol6);
		}

		public void TestEManifestHouseJobStatus()
		{
			var filter = (ModuleTextFilter)filters["CA House Status"];
			AssertNotNull("CA House Status filter", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.NotMatched, consol1, consol4);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, EManifestForwarderJobStatusList.Codes.NotMatched, consol2, consol3, consol5, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.Validated, consol2, consol3);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, EManifestForwarderJobStatusList.Codes.Validated, consol1, consol4, consol5, consol6);
		}

		public void TestEManifestHouseMessageStatus()
		{
			var filter = (ModuleTextFilter)filters["CA House Msg. Sta"];
			AssertNotNull("CA House Msg. Sta", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ClearOriginal, consol3, consol4);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ClearOriginal, consol1, consol2, consol5, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ClearChange, consol1);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ClearChange, consol2, consol3, consol4, consol5, consol6);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ErrorOriginal, consol1, consol2);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ErrorOriginal, consol3, consol4, consol5, consol6);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "0001", "Matched. This is a completeness status notice that indicates that an IID or trade document is linked to all of its directly related trade documents.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "S001", "Positive Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "S002", "Negative Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			base.SetUp();
			consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master.BP_ParentID = consol1.PK;
			master.BP_ParentTableCode = consol1.TablePrefix;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			master.BP_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			master.BP_D4MessageStatus = "8000";
			var house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.NotMatched;
			house.BW_MessageStatus = MessageStatusList.Codes.ClearChange;
			house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			house.BW_MessageStatus = MessageStatusList.Codes.ErrorOriginal;
			house.BW_D4MessageStatus = "8000";

			consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master.BP_ParentID = consol2.PK;
			master.BP_ParentTableCode = consol1.TablePrefix;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			master.BP_MessageStatus = MessageStatusList.Codes.ErrorOriginal;
			master.BP_D4MessageStatus = "0001";
			house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;
			house.BW_MessageStatus = MessageStatusList.Codes.ErrorOriginal;
			house.BW_D4MessageStatus = "S001";
			house = master.HouseBills.AddNew();
			house.BW_D4MessageStatus = "S002";

			consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master.BP_ParentID = consol3.PK;
			master.BP_ParentTableCode = consol1.TablePrefix;
			master.BP_MessageStatus = MessageStatusList.Codes.NotSent;
			house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;
			house.BW_MessageStatus = MessageStatusList.Codes.ClearOriginal;

			consol4 = Factory.NewWithValidTestData<ForwardingConsol>();
			master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master.BP_ParentID = consol4.PK;
			master.BP_ParentTableCode = consol1.TablePrefix;
			master.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.NotMatched;
			house.BW_MessageStatus = MessageStatusList.Codes.ClearOriginal;

			consol5 = Factory.NewWithValidTestData<ForwardingConsol>();
			master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master.BP_ParentID = consol5.PK;
			master.BP_ParentTableCode = consol1.TablePrefix;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			master.BP_MessageStatus = MessageStatusList.Codes.ClearOriginal;

			consol6 = Factory.NewWithValidTestData<ForwardingConsol>();

			Factory.Save();

			filters = new ModuleFilterCollection();
			new CAConsolModuleColumnsAndFiltersProvider().AddFilters(filters, Factory);
		}

		internal static void AssertStatusQuery(ModuleTextFilter filter, SQLComparisonOperator comparisonOperator, string property, params ForwardingConsol[] expectedConsols)
		{
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = property;
			AssertProperConsolsLoaded(filter, expectedConsols);
		}

		internal static void AssertProperConsolsLoaded(ModuleFilter filter, ForwardingConsol[] expectedConsols)
		{
			var consols = expectedConsols[0].Factory.Load<ForwardingConsol>(filter.Query);
			AssertEquals("Proper quantity of consols loaded", expectedConsols.Length, consols.Length);
			foreach (var consol in expectedConsols)
			{
				var consolCached = consol;
				Assert("Consol should be loaded", consols.Any(s => s.PK == consolCached.PK));
			}
		}

		public void TestAddColumns()
		{
			using (var form = new ZForm())
			{
				var collection = new ForwardingConsolCollection(Factory);
				collection.LoadWithMoreFiltering(new ZQuery(JobConsolSchema.PK, consol2.PK));
				EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = new ZFilterStripControlForTesting(collection);
				new CAConsolModuleColumnsAndFiltersProvider().AddColumns(filterControl);
				form.Controls.Add(filterControl);
				form.Show();

				AssertColumn(filterControl.FilteredGrid, "CACloseStatus", new ResourceStringData(), "CA Close Status", false, "ERR - Error in last message, please fix and re-submit");
				AssertColumn(filterControl.FilteredGrid, "CACloseMsgStatus", new ResourceStringData(), "CA Close Msg. Sta", false, "ERO - Error Original");
				AssertColumn(filterControl.FilteredGrid, "CAMasterLatestD4Notice", new ResourceStringData(), "CA Master Latest D4 Notice", false, "0001");
				AssertColumn(filterControl.FilteredGrid, "CAMasterLatestD4NoticeDescription", new ResourceStringData(), "CA Master Latest D4 Notice Description", false, "Matched. This is a completeness status notice that indicates that an IID or trade document is linked to all of its directly related trade documents.");
				AssertColumn(filterControl.FilteredGrid, "CAHouseLatestD4Notice", new ResourceStringData(), "CA House Latest D4 Notice", false, "S001| S002");
				AssertColumn(filterControl.FilteredGrid, "CAHouseLatestD4NoticeDescription", new ResourceStringData(), "CA House Latest D4 Notice Description", false, "Positive Functional Acknowledgement.| Negative Functional Acknowledgement.");
			}
		}

		ForwardingConsol consol1;
		ForwardingConsol consol2;
		ForwardingConsol consol3;
		ForwardingConsol consol4;
		ForwardingConsol consol5;
		ForwardingConsol consol6;
		ModuleFilterCollection filters;

		sealed class ZFilterStripControlForTesting : FilterStripControlTest.DummyZFilterStripControl, IFilterControl
		{
			internal ZFilterStripControlForTesting(IBusinessObjectCollection gridCollection)
				: base(gridCollection, new DummyFilterStripBusinessObject())
			{
			}
		}
	}
}
