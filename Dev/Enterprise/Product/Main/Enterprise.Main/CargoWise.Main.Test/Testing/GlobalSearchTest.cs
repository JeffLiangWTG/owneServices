using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Billing.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.SearchBox;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using GlowIndexQueryService.Tests.Common;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Startup.Testing
{
	sealed class GlobalSearchTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			globalSearch = new TestGlobalSearch();
			// enable the glow service in registry to allow the test/methods to work
			glowIndexForSearchDisposable = GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			globalSearch.ClearExternalSearchResults();
			glowIndexForSearchDisposable?.Dispose();
			base.TearDown();
		}

		TestGlobalSearch globalSearch;
		IDisposable glowIndexForSearchDisposable;

		public void TestDisplayItemCreatorIsNull()
		{
			var gSearch = new GlobalSearch(null);
			AssertNoExceptionThrown(() => gSearch.Search("test"));
			AssertEquals("SearchCore_ParamtersAreNull", ErrorReporter.LastKeyReported);
			AssertEquals("False False True", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGlowIndexSearchToSearchBoxDisplayItemsWithLocalization()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				globalSearch.AddExternalSearchResult("testBizoPk", new Collection<(string Key, string Value)> { ("KeyField1", "Number2") }, "IJobShipment");
				globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
				AssertSearchResultsAreEqual(
					globalSearch.Search("number2").ToSearchBoxDisplayItems(),
					new List<IDisplayItem>
					{
						DisplayItemFactory.CreateHeadingItem("货运(货代)"),
						DisplayItemFactory.CreateSearchItemWithEmptyAction("KeyField1 - Number2")
					});
				ErrorReporter.Clear();
			}
		}

		public void TestGlowIndexSearchToSearchSectionWithLocalization()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				globalSearch.AddExternalSearchResult("testBizoPk", new Collection<(string Key, string Value)> { ("KeyField1", "Number2"), ("KeyField2", "Another") }, "IJobShipment");
				globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
				AssertSearchResultsAreEqual(
					globalSearch.Search("number2").ToSearchSections(),
					["(1) IJobShipment: 货运(货代)"]);
				ErrorReporter.Clear();
			}
		}

		public void TestGlowIndexSearchMultipleResults()
		{
			// we get one valid result back
			globalSearch.AddExternalSearchResult("testBizoPk1", new Collection<(string Key, string Value)> { ("name", "food"), ("description", "bar") }, "IJobShipment");
			globalSearch.AddExternalSearchResult("testBizoPk2", new Collection<(string Key, string Value)> { ("name", "foot"), ("description", "bar") }, "IJobShipment");
			globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
			AssertSearchResultsAreEqual(
				globalSearch.Search("foo bar").ToSearchBoxDisplayItems(),
				new List<IDisplayItem>
				{
					DisplayItemFactory.CreateHeadingItem("Shipments (Forwarding)"),
					DisplayItemFactory.CreateSearchItemWithEmptyAction("description - bar|name - food"),
					DisplayItemFactory.CreateSearchItemWithEmptyAction("description - bar|name - foot"),
				});
			ErrorReporter.Clear();
		}

		public void TestGlowIndexSearchIgnoresRedundantWhitespaces()
		{
			globalSearch.AddExternalSearchResult("testBizoPk", new Collection<(string Key, string Value)> { ("KeyField1", "Number2") }, "IJobShipment");
			globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
			AssertSearchResultsAreEqual(
				globalSearch.Search(" number2 ").ToSearchBoxDisplayItems(),
				new List<IDisplayItem>
				{
					DisplayItemFactory.CreateHeadingItem("Shipments (Forwarding)"),
					DisplayItemFactory.CreateSearchItemWithEmptyAction("KeyField1 - Number2")
				});
			ErrorReporter.Clear();
		}

		public void TestGlowIndexSearchNoResult()
		{
			// we get no results at all
			globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
			AssertSearchResultsAreEqual(globalSearch.Search("test").ToSearchBoxDisplayItems(), Enumerable.Empty<IDisplayItem>());
		}

		public void TestGlowIndexSearchResultHasBlankKeyfields()
		{
			// we get a blank result - this should never happen but we have to make sure we deal with it
			globalSearch.AddExternalSearchResult("", new Collection<(string Key, string Value)> { ("", "") });
			globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
			AssertSearchResultsAreEqual(globalSearch.Search("test").ToSearchBoxDisplayItems(), Enumerable.Empty<IDisplayItem>());
		}

		public void TestGlowIndexSearchEmptySearch()
		{
			// search for nothing, get nothing
			globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
			AssertSearchResultsAreEqual(globalSearch.Search(string.Empty).ToSearchBoxDisplayItems(), Enumerable.Empty<IDisplayItem>());
		}

		public void TestGlowIndexSearchWithWarning()
		{
			globalSearch.AddExternalSearchResult("testBizoPk", new Collection<(string Key, string Value)> { ("KeyField1", "Number2") }, "IJobShipment");
			globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);
			globalSearch.SetExternalSearchResultWarning("Warning message");
			AssertSearchResultsAreEqual(
					globalSearch.Search(" number2 ").ToSearchBoxDisplayItems(),
					new List<IDisplayItem>
					{
						DisplayItemFactory.CreateErrorItem("Warning message"),
						DisplayItemFactory.CreateHeadingItem("Shipments (Forwarding)"),
						DisplayItemFactory.CreateSearchItemWithEmptyAction("KeyField1 - Number2")
					});
			ErrorReporter.Clear();
		}

		public void TestGlowIndexSearchMultipleResultsDoesntHitDB()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			for (var i = 0; i < 100; i++)
			{
				var bizo = factory.NewWithValidTestData<GlbStaff>();
				bizo.GS_FullName = "first" + i.ToString();

				// make some different ones to search
				if (i % 3 == 0)
				{
					bizo.GS_FullName = "third" + i.ToString();

					// Add corresponding glow results
					(var key, var value) = ("FULLNAME", bizo.GS_FullName);
					globalSearch.AddExternalSearchResult(bizo.PK.ToString(), new Collection<(string Key, string Value)> { (key, value) }, "IGlbStaff");
				}
				else
				{
					bizo.GS_FullName = i.ToString();
				}
			}

			factory.Save();
			globalSearch.SetExternalSearchResultStatus(GlowIndexQueryStatus.Success);

			var factory2 = new BusinessObjectFactory();
			var hits = new Dictionary<string, int> { { GlbStaffSchema.Constants.TableName, 0 } }; // assert GlbStaff hit 0 times

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(hits, factory2))
			{
				// act
				var results = globalSearch.Search("thi").ToSearchBoxDisplayItems();

				// assert
				AssertEquals(35, results.Count()); // 34 items + a category/heading item
				AssertEquals("third48", results.ElementAt(17).Data.First());
			}
		}

		public void TestGlowEntityToDisplayItem()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var bizo = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IGlbStaff", new Collection<(string, string)> { ("CODE", "DEN"), ("FULLNAME", "DENNIS REYNOLDS") });
			(ModuleIdentifier ModuleId, string PK) actionResults = (ModuleIDs.NotAssigned, null);
			Action<ModuleOpenerInfo> moduleOpener = (info) => { actionResults = (info.ModuleId, info.BizoPk); };

			Action testAction = () => { };
			var expected = DisplayItemFactory.CreateSearchItem(testAction, new string[] { "DEN", "DENNIS REYNOLDS" });

			// act
			var actual = GlobalSearch.GlowEntityToDisplayItem(giqr, moduleOpener, "den").ToSearchBoxDisplayItem();

			// assert
			AssertNotNull(actual);
			AssertEquals(expected.IsSelectable, actual.IsSelectable);
			AssertContainsExactElementsInAnyOrder(expected.Data, actual.Data);
			AssertEquals(expected.Theme, actual.Theme);

			// call Select() on data item and see Action called, which is 'moduleOpener' in this test
			AssertNotEquals(ModuleIDs.GlbStaff, actionResults.ModuleId);
			AssertNotEquals(bizo.PK.ToString(), actionResults.PK);
			actual.Select();
			AssertEquals(ModuleIDs.GlbStaff, actionResults.ModuleId);
			AssertEquals(bizo.PK.ToString(), actionResults.PK);
		}

		public void TestAllEntityTypesShouldHaveFilters()
		{
			var definedEntityTypes = Enum.GetValues(typeof(EntityType)).Cast<EntityType>();
			foreach (var entityType in definedEntityTypes)
			{
				AssertCollectionContains($"No filters defined for {entityType}", entityType, GlobalSearch.EntityFilterMap.Keys);
			}
		}

		public void TestProcessModuleOpen()
		{
			ErrorReporter.Clear();
			Action<ModuleOpenerInfo> moduleOpener = (info) => { };
			GlobalSearch.ProcessModuleOpen(null, null, moduleOpener);
			AssertEquals("GlowEntityToDisplayItem_ArgumentsWereNull", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestProcessModuleOpen_WhenEnableGlowIndexSearchUsageCollector_ShouldGenerateEDI()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			var helper = new UsageCollectorTestHelper(factory);

			using (GlowRegistry.Instance.GlowIndexSearchUsageCollector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var glowIndexQueryResult = new GlowIndexQueryResult(staff.PK.ToString(), "IGlbStaff");
				GlobalSearch.ProcessModuleOpen(glowIndexQueryResult, "FULLNAME - " + staff.GS_FullName, (info) => { });
			}

			var properties = new List<(string name, object value)>()
			{
				("SearchType", "GlobalSearch"),
				("ModuleID", "GlbStaff(Staff and Resources)")
			};

			Assert("Contains message.", helper.AssertUsageMessagesContains("SPF", properties));
		}

		public void TestGlowEntityToDisplayItem_NullValues()
		{
			CombineAssertions(() =>
			{
				{
					var actual = GlobalSearch.GlowEntityToDisplayItem(null, null, null);
					AssertEquals(null, actual);
					ErrorReporter.Clear();
				}
				{
					var actual = GlobalSearch.GlowEntityToDisplayItem(null, null, "");
					AssertEquals(null, actual);
					ErrorReporter.Clear();
				}
				{
					var giqr = new GlowIndexQueryResult("pk", "entityType");
					var actual = GlobalSearch.GlowEntityToDisplayItem(giqr, null, "");
					AssertEquals(null, actual);
					ErrorReporter.Clear();
				}
				{
					Action<ModuleOpenerInfo> moduleOpener = (info) => { };
					var giqr = new GlowIndexQueryResult("pk", "entityType");
					giqr.KeyFields.Add(("NAME", "TEST"));
					var actual = GlobalSearch.GlowEntityToDisplayItem(giqr, moduleOpener, "TEST").ToSearchBoxDisplayItem();
					actual.Select();
					AssertEquals("Entity entityType, NAME - TEST Not Found. Please try it later.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestGlowIndexQueryResultIsNull()
		{
			CombineAssertions(() =>
			{
				Assert(GlobalSearch.GlowIndexQueryResultIsNull(new GlowIndexQueryResult(null, null, null)));
				Assert(GlobalSearch.GlowIndexQueryResultIsNull(new GlowIndexQueryResult("", null, null)));
				Assert(GlobalSearch.GlowIndexQueryResultIsNull(new GlowIndexQueryResult("", "", null)));
				Assert(!GlobalSearch.GlowIndexQueryResultIsNull(new GlowIndexQueryResult("", "", new Collection<(string, string)>())));
			});
		}

		public void TestGlowEntityToModuleIdentifier()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var bizo = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IGlbStaff", new Collection<(string, string)> { ("firstname", "fred") });

			// act
			var actual = GlobalSearch.GlowEntityToModuleOpenerInfo(giqr);

			// assert
			AssertEquals(ModuleIDs.GlbStaff, actual.ModuleId);
		}

		public void TestGlowEntityToModuleIdentifier_JobDeclarationBelongingToAnotherCompany()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var factory = new BusinessObjectFactory();
				var bizo = factory.NewWithValidTestData<BaseJobDeclaration>();

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = "Test User";
				var differentCompany = factory.New<GlbCompany>();
				var branch = factory.New<GlbBranch>();
				differentCompany.Branches.Add(branch);
				differentCompany.SetCountry("AU");

				var moduleAccess = factory.New<GlbSecurity>();
				moduleAccess.GU_SecurityRight = "CustomsDeclarationEnquiry";
				moduleAccess.GU_GS = staff.PK;
				moduleAccess.GU_SecurityItemIsAllowed = true;

				factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IJobDeclaration");
					var moduleOpenerInfo = GlobalSearch.GlowEntityToModuleOpenerInfo(giqr, factory: factory);

					AssertNotNull("A controller is found for the declaration belonging to another foreign company", moduleOpenerInfo);
					AssertEquals(ModuleIDs.Customs.JobDeclaration, moduleOpenerInfo.ModuleId);

					var declarationCompany = factory.Load<GlbCompany>(bizo.JE_GC);
					var countryCode = declarationCompany.GC_RN_NKCountryCode.ToString() ?? string.Empty;
					var controller = ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(bizo.GetType(), countryCode: countryCode);

					AssertNull(controller.ShowViewForm(bizo));
					AssertContains("When the controller is used to open the declaration, it should throw a warning for opening a declaration belonging to another company", "You are trying to view a declaration that belongs to a different company. Please log into the company", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGlowEntityToModuleIdentifier_JobDeclarationBelongingToAnotherCompany_CusContainer()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var factory = new BusinessObjectFactory();
				var bizo = factory.NewWithValidTestData<BaseCusContainer>();

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = "Test User";
				var differentCompany = factory.New<GlbCompany>();
				var branch = factory.New<GlbBranch>();
				differentCompany.Branches.Add(branch);
				differentCompany.SetCountry("AU");

				var moduleAccess = factory.New<GlbSecurity>();
				moduleAccess.GU_SecurityRight = "CustomsDeclarationEnquiry";
				moduleAccess.GU_GS = staff.PK;
				moduleAccess.GU_SecurityItemIsAllowed = true;

				factory.Save();

				using (var userContext = Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "ICusContainer");
					var moduleOpenerInfo = GlobalSearch.GlowEntityToModuleOpenerInfo(giqr, factory: factory);

					AssertNotNull("A controller is found for the declaration belonging to another foreign company", moduleOpenerInfo);
					AssertEquals(ModuleIDs.Customs.JobDeclaration, moduleOpenerInfo.ModuleId);

					var declarationCompany = factory.Load<GlbCompany>(bizo.Declaration.JE_GC);
					var countryCode = declarationCompany.GC_RN_NKCountryCode.ToString() ?? string.Empty;
					var controller = ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(bizo.Declaration.GetType(), countryCode: countryCode);

					AssertNull(controller.ShowViewForm(bizo.Declaration));
					AssertContains("When the controller is used to open the declaration, it should throw a warning for opening a declaration belonging to another company", "You are trying to view a declaration that belongs to a different company. Please log into the company", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGlowEntityToModuleIdentifier_NullValues()
		{
			ErrorReporter.Clear();
			AssertNull(GlobalSearch.GlowEntityToModuleOpenerInfo(null));
			AssertEquals("GlowEntityToDisplayItem_ArgumentsWereNull", ErrorReporter.LastKeyReported);
			AssertEquals("Param GlowIndexQueryResult can not be null", ErrorReporter.LastMessageReported);

			var keyFields = new Collection<(string, string)>();
			AssertNull(GlobalSearch.GlowEntityToModuleOpenerInfo(new GlowIndexQueryResult("", "TEST", keyFields), "GlowIndexQuery"));
			AssertEquals("Entity TEST, GlowIndexQuery Not Found. Please try it later.", UnitTestUserNotification.Instance.LastMessage.Text);
			ErrorReporter.Clear();
		}

		public void TestGlowEntityToModuleIdentifier_BizoFoundButNoController()
		{
			ErrorReporter.Clear();
			// arrange
			var factory = new BusinessObjectFactory();
			var bizo = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IDummyBusinessObject", new Collection<(string, string)> { ("firstname", "fred") });

			// act
			var result = GlobalSearch.GlowEntityToModuleOpenerInfo(giqr);

			// assert
			AssertNull(result);
			AssertEquals("GlowEntityToDisplayItem_ArgumentsWereNull", ErrorReporter.LastKeyReported);
			AssertEquals($"Module not found. PK={bizo.PK}, EntityType=IDummyBusinessObject, BizoType=CargoWise.EntityFramework.Testing.DummyBaseBusinessObject, Controller=", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetControllerFromBizo_ControllerRelatedBizoProvider()
		{
			using (SetBizoTypeForPrefix(DummyBusinessObject.Schema.TablePrefix, typeof(DummyBusinessObjectWithControllerRelatedBizoProvider)))
			{
				var factory = new BusinessObjectFactory();

				var bizo = factory.New<DummyBusinessObjectWithControllerRelatedBizoProvider>();
				factory.Save();

				var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IDummyBusinessObject", new Collection<(string, string)> { ("firstname", "hank") });

				// act
				var result = GlobalSearch.GlowEntityToModuleOpenerInfo(giqr);

				// assert
				AssertEquals(ModuleIDs.GlbStaff, result.ModuleId);
				AssertNotEquals(bizo.PK, ZGuid.ParseSafe(result.BizoPk));

				var staff = factory.Load<GlbStaff>(ZGuid.ParseSafe(result.BizoPk));
				AssertNotNull(staff);
			}
		}

		public void TestIIncidentEntityFilterCanFilterNumberAndSummary()
		{
			var filterExist = GlobalSearch.EntityFilterMap.TryGetValue(EntityType.IIncidentRequest, out var filter);
			Assert(filterExist);
			var allFields = new string[] { "NUMBER", "SUMMARY", "TestFields", "OtherFields" };
			var filterFields = filter(allFields);
			AssertEquals(2, filterFields.Length);
			AssertContains("NUMBER", filterFields[0]);
			AssertContains("SUMMARY", filterFields[1]);
		}

		IDisposable SetBizoTypeForPrefix(string prefix, Type newType)
		{
			const string prefixMapName = "EnterpriseBusinessObjectPrefixTypes";

			var mock = new Mock<ObjectHandle>();
			mock.Setup(m => m.GetObjectType()).Returns(newType);

			var prefixTypes = (Hashtable)((Hashtable)ObjectFactory.Get(prefixMapName)).Clone();
			prefixTypes[prefix] = mock.Object;

			return ObjectFactory.Substitute("EnterpriseBusinessObjectPrefixTypes", prefixTypes);
		}

		class DummyBusinessObjectWithControllerRelatedBizoProvider : DummyBusinessObject, IGlobalSearchBusinessObjectProvider
		{
			public DummyBusinessObjectWithControllerRelatedBizoProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public BusinessObject BusinessObjectForController
			{
				get
				{
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					Factory.Save();
					return staff;
				}
			}
		}

		IEnumerable<IGlobalSearchResultItem> MakeDisplayEnumerable(string key, int count)
		{
			for (var i = 0; i < count; ++i)
			{
				yield return GlobalSearchResultItem.CreateSearchItem(key, string.Empty, () => { }, key + "_" + i.ToString());
			}
		}
		IList<IGlobalSearchResultItem> MakeDisplayList(string key, int count) => MakeDisplayEnumerable(key, count).ToList();

		public void TestDictionaryToList()
		{
			// arrange
			var items = new Dictionary<string, IList<IGlobalSearchResultItem>>
			{
				{ "a", MakeDisplayList("a", 2) },
				{ "b", MakeDisplayList("b", 9) },
				{ "c", MakeDisplayList("c", 1) },
				{ "d", MakeDisplayList("d", 0) },
			};

			// act
			var result = GlobalSearchResult.Create(string.Empty, GlobalSearch.DictionaryToEnumerable(items)).ToSearchBoxDisplayItems();

			// assert total count, which is sum of list count + number of lists with things in them
			// ie 2+9+1+0=12, +3 list headings = 15
			AssertEquals(15, result.Count());

			// assert headers
			AssertEquals("a", result.ElementAt(0).Data.ElementAt(0));
			AssertEquals("b", result.ElementAt(3).Data.ElementAt(0));
			AssertEquals("c", result.ElementAt(13).Data.ElementAt(0));
		}

		public void TestDictionaryToList_NullValues()
		{
			CombineAssertions(() =>
			{
				{
					AssertNoExceptionThrown(() => GlobalSearch.DictionaryToEnumerable(null));
				}
				{
					var items = new Dictionary<string, IList<IGlobalSearchResultItem>>();
					AssertNoExceptionThrown(() => GlobalSearch.DictionaryToEnumerable(items));
				}
				{
					var items = new Dictionary<string, IList<IGlobalSearchResultItem>> { { "a", null } };
					AssertNoExceptionThrown(() => GlobalSearch.DictionaryToEnumerable(items));
				}
				{
					var list = new List<IGlobalSearchResultItem> { GlobalSearchResultItem.CreateSearchItem("", "", null, "") };
					var items = new Dictionary<string, IList<IGlobalSearchResultItem>> { { "a", list } };
					AssertNoExceptionThrown(() => GlobalSearch.DictionaryToEnumerable(items));
				}
				{
					var list = new List<IGlobalSearchResultItem> { GlobalSearchResultItem.CreateSearchItem("", "", () => { }, null) };
					var items = new Dictionary<string, IList<IGlobalSearchResultItem>> { { "a", list } };
					AssertNoExceptionThrown(() => GlobalSearch.DictionaryToEnumerable(items));
				}
			});
		}

		public void TestGetHumanReadableEntityName()
		{
			AssertEquals("Work Items", GlobalSearch.GetHumanReadableEntityName("IWorkItem"));
			AssertEquals("Staff and Resources", GlobalSearch.GetHumanReadableEntityName("IGlbStaff"));
			AssertEquals("Shipments (Forwarding)", GlobalSearch.GetHumanReadableEntityName("IJobShipment"));
			AssertEquals("IUnknown", GlobalSearch.GetHumanReadableEntityName("IUnknown"));
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("工作项目", GlobalSearch.GetHumanReadableEntityName("IWorkItem"));
				AssertEquals("员工及资源", GlobalSearch.GetHumanReadableEntityName("IGlbStaff"));
				AssertEquals("货运(货代)", GlobalSearch.GetHumanReadableEntityName("IJobShipment"));
				AssertEquals("IUnknown", GlobalSearch.GetHumanReadableEntityName("IUnknown"));
			}
		}

		public void TestGlowIndexQueryResultsToDisplayItems()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var bizo = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IGlbStaff", new Collection<(string, string)> { ("CODE", "DEN"), ("FULLNAME", "DENNIS REYNOLDS") });
			var results = new List<GlowIndexQueryResult> { giqr };
			Action<ModuleOpenerInfo> moduleOpener = (info) => { };

			// act
			var actual = GlobalSearchResult.Create(
							"",
							GlobalSearch.GlowIndexQueryResultsToDisplayItems(results, moduleOpener, "den"))
						.ToSearchBoxDisplayItems();

			// assert
			AssertEquals("There should be 2 items in the list, a heading and a result", 2, actual.Count());

			CombineAssertions(() =>
			{
				var heading = actual.First();
				Assert(!heading.IsSelectable);
				AssertEquals(DisplayItemTheme.DefaultHeading, heading.Theme);
				AssertEquals(1, heading.Data.Count());
				AssertEquals("Staff and Resources", heading.Data.First());

				var item = actual.Last();
				Assert(item.IsSelectable);
				AssertEquals(DisplayItemTheme.DefaultItem, item.Theme);
				AssertEquals(2, item.Data.Count());
				Assert(item.Data.SequenceEqual(new string[] { "DEN", "DENNIS REYNOLDS" }));
			});
		}

		public void TestGlowIndexQueryResultsToDisplayItemsWithSubClass()
		{
			// arrange
			var factory = new BusinessObjectFactory();

			var forwardingConsol = (BusinessObject)factory.New<IForwardingConsol>();
			var container = factory.New<IForwardingContainer>();
			container.JC_ContainerNum = "ABCD123456";
			container.JC_JK = forwardingConsol.PK;

			var bizo = container as BusinessObject;
			bizo.FillWithValidTestData();

			factory.Save();

			var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IJobContainer", new Collection<(string, string)> { ("containernum", "ABCD123456") });
			var results = new List<GlowIndexQueryResult> { giqr };
			Action<ModuleOpenerInfo> moduleOpener = (info) => { };

			// act
			var actual = GlobalSearchResult.Create
						(
							"",
							GlobalSearch.GlowIndexQueryResultsToDisplayItems(results, moduleOpener, "ABCD")
						).ToSearchBoxDisplayItems();
			AssertEquals("There should be 2 items in the list, a heading and a result", 2, actual.Count());

			CombineAssertions(() =>
			{
				var heading = actual.First();
				Assert(!heading.IsSelectable);
				AssertEquals(DisplayItemTheme.DefaultHeading, heading.Theme);
				AssertEquals(1, heading.Data.Count());
				AssertEquals("Containers", heading.Data.First());

				var item = actual.Last();
				Assert(item.IsSelectable);
				AssertEquals(DisplayItemTheme.DefaultItem, item.Theme);
				AssertEquals(1, item.Data.Count());
				AssertEquals("containernum - ABCD123456", item.Data.First());
			});
		}

		public void TestGlowIndexQueryResultsToDisplayItemsWithWarning()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var bizo = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			var giqr = new GlowIndexQueryResult(bizo.PK.ToString(), "IGlbStaff", new Collection<(string, string)> { ("CODE", "DEN"), ("FULLNAME", "DENNIS REYNOLDS") });
			var results = new List<GlowIndexQueryResult> { giqr };
			Action<ModuleOpenerInfo> moduleOpener = (info) => { };

			// act
			var actual = GlobalSearchResult.Create(
							"",
							GlobalSearch.GlowIndexQueryResultsToDisplayItems(results, moduleOpener, "den"),
							"Waning Message\r\nLine2").ToSearchBoxDisplayItems().ToArray();

			// assert
			AssertEquals("There should be 2 items in the list, a heading and a result", 3, actual.Length);

			CombineAssertions(() =>
			{
				var warning = actual.First();
				Assert(!warning.IsSelectable);
				AssertEquals(DisplayItemTheme.DefaultError, warning.Theme);
				AssertEquals(2, warning.Data.Count());
				AssertEquals("Waning Message", warning.Data.First());
				AssertEquals("Line2", warning.Data.Last());

				var heading = actual[1];
				Assert(!heading.IsSelectable);
				AssertEquals(DisplayItemTheme.DefaultHeading, heading.Theme);
				AssertEquals(1, heading.Data.Count());
				AssertEquals("Staff and Resources", heading.Data.First());

				var item = actual.Last();
				Assert(item.IsSelectable);
				AssertEquals(DisplayItemTheme.DefaultItem, item.Theme);
				AssertEquals(2, item.Data.Count());
				Assert(item.Data.SequenceEqual(new string[] { "DEN", "DENNIS REYNOLDS" }));
			});
		}

		static void AssertSearchResultsAreEqual(IEnumerable<IDisplayItem> actualResults, IEnumerable<IDisplayItem> expectedResults)
		{
			string ToString(IDisplayItem item) => string.Join("|", item.Data.OrderBy(x => x));

			AssertContainsExactElementsInAnyOrder(
				expectedResults.Select(ToString),
				actualResults.Select(ToString));
		}

		static void AssertSearchResultsAreEqual(IEnumerable<SearchResultSection> actualResults, IEnumerable<string> expectedResults)
		{
			string ToString(SearchResultSection item) => item.ToString();

			AssertContainsExactElementsInAnyOrder(
				expectedResults,
				actualResults.Select(ToString));
		}

		public class TestGlobalSearch : GlobalSearch
		{
			public string MenuItemName = "test";

			public (string Id, MultilingualString Description, Action Execute) TestCreateMenuItem(ModuleIdentifier moduleIdentifier, string bizoPK)
			{
				return (bizoPK, (NoResString)moduleIdentifier.ExtendedDescription.ToString(), () => { }
				);
			}

			public TestGlobalSearch() : base(null)
			{
				GlowIndexQueryEngine = new MockGlowIndexQuerySearchEngine();
				ModuleOpener = (info) => { };
			}

			public void AddExternalSearchResult(string pk, Collection<(string, string)> values, string entityType = "IJobShipment")
			{
				var item = new GlowIndexQueryResult(pk, entityType);
				foreach (var (key, value) in values)
				{
					Assert(value != null);
					item.KeyFields.Add((key, value));
				}
				((MockGlowIndexQuerySearchEngine)GlowIndexQueryEngine).Results.Results.Add(item);
			}

			public void SetExternalSearchResultStatus(GlowIndexQueryStatus queryResult)
			{
				((MockGlowIndexQuerySearchEngine)GlowIndexQueryEngine).Results.Status = queryResult;
			}

			public void ClearExternalSearchResults()
			{
				((MockGlowIndexQuerySearchEngine)GlowIndexQueryEngine).Results.Results.Clear();
				((MockGlowIndexQuerySearchEngine)GlowIndexQueryEngine).Results.Status = GlowIndexQueryStatus.Uninitialised;
			}

			public void SetExternalSearchResultWarning(string warning)
			{
				((MockGlowIndexQuerySearchEngine)GlowIndexQueryEngine).Results.WarningMessage = warning;
			}
		}
	}
}
