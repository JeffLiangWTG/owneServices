using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryItemSetLocatorTest : TransactionedTestCase
	{
		public void TestRegistryItemSetsLoadedWhenProductivityWiseEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			var itemSets = Locator.GetRegistryItemSets().Select(s => s.ToString().Split('.').Last()).ToArray();
			var message = FormattableString.Invariant($"If your registry item set causes this test to fail, consider if it contains registry items related to supply chain modules. If it exists entirely to support these modules, exclude the entire registry item set using {nameof(RegistryItemSet.IsForProductivityWise)}. If the registry items are for modules that are also included in ProductivityWise, then the set should be added to this list. It is also possible to hide specific supply chain-related registry items in PW mode, but this is not desirable for maintainability purposes.");

			AssertContainsExactElementsInAnyOrder(message, ProductivityWiseRegistryItemSets, itemSets);
		}

		static string[] ProductivityWiseRegistryItemSets => new[]
		{
			"AccountingConfigurationRegistry",
			"AccountingElectronicMessagingRegistry",
			"ActiveDirectoryRegistry",
			"ArchiveManagerDataRegistry",
			"BMSRegistry",
			"AccountingDataRegistry",
			"DocumentsDataRegistry",
			"eAdaptorRegistry",
			"EDIClientRegistry",
			"eHubMessagingRegistry",
			"eServicesRegistry",
			"FtpRegistry",
			"GlowRegistry",
			"NotificationDataRegistry",
			"OrganisationsDataRegistry",
			"OrganisationRegistry",
			"PhysicalServerDataRegistry",
			"ProcessManagementRegistry",
			"RawDataRegistry",
			"RefDataRepoRegistry",
			"RemoteDatabaseRegistry",
			"ReferenceFilesDataRegistry",
			"SystemDataRegistry",
			"WebDataRegistry",
			"WorkflowDataRegistry",
		};

		public void TestRegistryItemSetSpringIds()
		{
			CombineAssertions(delegate
			{
				foreach (RegistryItemSet itemSet in (ArrayList)ObjectFactory.Get("RegistryItemSets"))
				{
					var concreteTypeName = itemSet.GetType().Name;
					var springId = "RegistryItemSet_" + concreteTypeName;
					Type springType = null;

					try
					{
						springType = ObjectFactory.Get(springId).GetType();

						AssertEquals(
							string.Format("The Spring ID of RegistryItemSet [{0}] must be [{1}].", itemSet.GetType().FullName, springId),
							itemSet.GetType(),
							springType);
					}
					catch
					{
						Fail(string.Format(@"The Spring ID [{0}] does not exist. Check id in [RegistryItemSetsConfiguration.xml].
The concrete type loaded was {1} and this does not match a registry item set of the same exact name (prefixed with RegistryItemSet_).
Even if your registry item set correctly loads the relevant type, this test ensures that the type name and the last half of the ID
attribute in the defining OBJECT node in EnterpriseApplicationConfiguration.xml match.", springId, concreteTypeName));
					}
				}
			});
		}

		public void TestGetRegistryItemSet()
		{
			var locator = new RegistryItemSetLocator();
			AssertEquals("locator.GetRegistryItemSet(\"FreightDataRegistry\").GetType()", typeof(FreightDataRegistry), locator.GetRegistryItemSet("FreightDataRegistry").GetType());
		}

		public void TestGetRegistryItemSets()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(null))
			{
				var itemSets = Locator.GetRegistryItemSets();
				AssertEquals("GetRegistryItemSets().Length", true, itemSets.Any());
				int standardItemSetCount = itemSets.Count();
				AssertContainsStandardRegistryItemSets(itemSets);

				var mockClientHook = new Mock<ClientHook>();
				using (ClientHookLoader.Instance.OverrideClientHookForTest(mockClientHook.Object))
				{
					itemSets = Locator.GetRegistryItemSets();
					mockClientHook.Setup(m => m.AdditionalRegistryItemSet).Returns((IRegistryItemSet)null);
					var count = itemSets.Count();
					mockClientHook.VerifyAll();

					AssertEquals("GetRegistryItemSets().Length", standardItemSetCount, count);
					AssertContainsStandardRegistryItemSets(itemSets);

					itemSets = Locator.GetRegistryItemSets();

					var itemSet = new MockRegistryItemSet();
					mockClientHook.Setup(m => m.AdditionalRegistryItemSet).Returns(itemSet);
					var list = itemSets.ToList();
					mockClientHook.VerifyAll();

					AssertEquals("GetRegistryItemSets().Length", standardItemSetCount + 1, list.Count);
					AssertContainsStandardRegistryItemSets(itemSets);
					AssertCollectionContains(itemSet, list);
				}
			}
		}

		public void TestGetRegistryItemsReferencingPK()
		{
			Guid newGuid = Guid.NewGuid();

			IRegistryItem[] items = MockLocator.GetRegistryItemsReferencingPK(newGuid);
			AssertEquals("Items.Length", 0, items.Length);

			TestItem1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			TestItem2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);

			items = MockLocator.GetRegistryItemsReferencingPK(Guid.NewGuid());
			AssertEquals("Items.Length", 0, items.Length);

			items = MockLocator.GetRegistryItemsReferencingPK(newGuid);
			AssertEquals("Items.Length", 2, items.Length);
			AssertCollectionContains("GetRegistryItemsReferencingPK() should contain TestItem1.", TestItem1, items);
			AssertCollectionContains("GetRegistryItemsReferencingPK() should contain TestItem2.", TestItem2, items);
		}

		public void TestGetFormattedListOfRegistryItemsReferencingPK()
		{
			Guid newGuid = Guid.NewGuid();

			string message = MockLocator.GetFormattedListOfRegistryItemsReferencingPK(newGuid);
			AssertEquals("Message", "", message);

			TestItem1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			TestItem2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);

			message = MockLocator.GetFormattedListOfRegistryItemsReferencingPK(Guid.NewGuid());
			AssertEquals("Message", "", message);

			message = MockLocator.GetFormattedListOfRegistryItemsReferencingPK(newGuid);
			AssertEquals("Message", "• TestCategory1/TestCaption1\r\n• TestCategory2/TestCaption2", message);

			message = MockLocator.GetFormattedListOfRegistryItemsReferencingPK(newGuid, false, ", ");
			AssertEquals("Message", "TestCategory1/TestCaption1, TestCategory2/TestCaption2", message);
		}

		public void TestRegistryItemNamesAreNotDuplicated()
		{
			RegistryTester.AssertItemNamesAreUnique(Locator.GetAllRegistryItems());
		}

		public void TestNoRegistryItemCatagoryDuplicates()
		{
			var regex = new Regex("^(.*?)/\\1(/|=>)");
			var itemsWithRepeatingCatagories = Locator.GetAllRegistryItems()
				.SelectMany(item => item.Categories.Select(catagory => catagory + "=>" + item.Caption))
				.Where(name => !string.IsNullOrEmpty(name) && regex.IsMatch(name))
				.ToList();

			Assert("Following catagories are repeated in sub catagories: (" + itemsWithRepeatingCatagories.Count + ") \r\n" + String.Join("\r\n", itemsWithRepeatingCatagories), itemsWithRepeatingCatagories.Count == 0);
		}

		public void TestRegistryItemFullDisplayPathsAreNotDuplicated()
		{
			CombineAssertions("Registry items should not have same same Category => Caption:", () =>
				{
					var hash = new Dictionary<string, IRegistryItem>();
					foreach (var registryItem in Locator.GetAllRegistryItems())
					{
						if (!(registryItem is LinkRegistryItem))
						{
							if (!registryItem.Options.HasFlag(RegistryOptions.IsHidden))
							{
								string fullPath = registryItem.Category + "=>" + registryItem.Caption;
								IRegistryItem existingItem;
								if (hash.TryGetValue(fullPath, out existingItem))
								{
									if (existingItem.Name != registryItem.Name)
									{
										var message = $@"	{existingItem.Name}
															{registryItem.Name}
															Category=>Caption:
															{fullPath}";
										Fail(message);
									}
								}
								else
								{
									hash.Add(fullPath, registryItem);
									Assert(true);
								}
							}
						}
					}
				});
		}

		public void TestRegistryCategoriesUseTheSameResourceStringKeys()
		{
			using (var mockRes = Res.UseMockData())
			{
				var eng = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
				mockRes.SetResourceGetter(key =>
				{
					return new ResourceStringData(key, key);
				});

				var hash = new Dictionary<string, string>();

				CombineAssertions(delegate
				{
					foreach (var registryItem in Locator.GetAllRegistryItems())
					{
						var multilingaulRegistryItem = registryItem as IMultilingualRegistryItem;
						if (multilingaulRegistryItem != null)
						{
							foreach (var category in multilingaulRegistryItem.CategoriesMultilingual)
							{
								if (category != null)
								{
									var subCategories = ((string)category).Split('/');
									var englishSubCategories = category.GetUnresolvedString().Split('/');
									bool isClean = subCategories.Length == englishSubCategories.Length;
									Assert("Use CombineCategories instead of putting '/' in category string: " + (string)category + " != " + category.GetUnresolvedString(), isClean);
									if (isClean)
									{
										for (int i = 0; i < subCategories.Length; i++)
										{
											var subCategory = string.Join("/", subCategories, 0, i + 1);
											var englishSubCategory = string.Join("/", englishSubCategories, 0, i + 1);
											string previousValue;
											if (hash.TryGetValue(englishSubCategory, out previousValue))
											{
												AssertEquals(string.Format("The sytem registry category \"{0}\" is defined using two different resource string keys or key combinations, it should be defined using only one unique resource string key or key combination ({1})", englishSubCategory, registryItem.Name), previousValue, subCategory);
											}
											else
											{
												hash.Add(englishSubCategory, subCategory);
											}
										}
									}
								}
							}
						}
					}
				});
			}
		}

		public void TestGetAllRegistryItemsReportsDuplicatesSilently()
		{
			mockLocator = new MockRegistryItemSetLocator();
			MockRegistryItemSet itemSet1 = new MockRegistryItemSet();
			itemSet1.Items.Add(TestItem1);
			itemSet1.Items.Add(TestItem1);
			mockLocator.ItemSets.Add(itemSet1);

			ExceptionReporterTestListener.Instance.Clear();
			var allItems = mockLocator.GetAllRegistryItems();
			AssertEquals("Should only be one item as the duplicate should be filtered out", 1, allItems.Count());
			AssertEquals("Wrong name of test item", TestItem1.Name, allItems.First().Name);
			AssertEquals("An error should have been reported silently for the duplicate", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		RegistryItemSetLocator Locator
		{
			get
			{
				if (locator == null)
				{
					locator = new RegistryItemSetLocator();
				}
				return locator;
			}
		}

		MockRegistryItemSetLocator MockLocator
		{
			get
			{
				if (mockLocator == null)
				{
					mockLocator = new MockRegistryItemSetLocator();
					MockRegistryItemSet itemSet1 = new MockRegistryItemSet();
					MockRegistryItemSet itemSet2 = new MockRegistryItemSet();
					itemSet1.Items.Add(TestItem2);
					itemSet2.Items.Add(TestItem1);
					mockLocator.ItemSets.Add(itemSet1);
					mockLocator.ItemSets.Add(itemSet2);
				}
				return mockLocator;
			}
		}

		GuidRegistryItem TestItem1
		{
			get
			{
				if (testItem2 == null)
				{
					testItem2 = new GuidRegistryItem("TestItem1", (NoResString)"TestCategory1", (NoResString)"TestCaption1", (NoResString)"", RegistryStorageFlags.All);
				}
				return testItem2;
			}
		}

		GuidRegistryItem TestItem2
		{
			get
			{
				if (testItem1 == null)
				{
					testItem1 = new GuidRegistryItem("TestItem2", (NoResString)"TestCategory2", (NoResString)"TestCaption2", (NoResString)"", RegistryStorageFlags.All);
				}
				return testItem1;
			}
		}

		void AssertContainsStandardRegistryItemSets(IEnumerable<RegistryItemSet> itemSets)
		{
			AssertCollectionContains("RawRegistry", Env.Registry.RawRegistry, itemSets);
			AssertCollectionContains("FreightDataRegistry", FreightDataRegistry.Instance, itemSets);
			AssertCollectionContains("LinerAgencyDataRegistry", LinerAgencyDataRegistry.Instance, itemSets);
			AssertCollectionContains("OceanCarrierDataRegistry", OceanCarrierDataRegistry.Instance, itemSets);
			AssertCollectionContains("SystemDataRegistry", SystemDataRegistry.Instance, itemSets);
			AssertCollectionContains("OrdersDataRegistry", OrdersDataRegistry.Instance, itemSets);
			AssertCollectionContains("RatingDataRegistry", RatingDataRegistry.Instance, itemSets);
			AssertCollectionContains("LocalCartageDataRegistry", LocalCartageDataRegistry.Instance, itemSets);
			AssertCollectionContains("CFSDataRegistry", CFSDataRegistry.Instance, itemSets);
			AssertCollectionContains("PhysicalServerDataRegistry", PhysicalServerDataRegistry.Instance, itemSets);
			AssertCollectionContains("OrganisationsDataRegistry", OrganisationsDataRegistry.Instance, itemSets);
			AssertCollectionContains("NotificationDataRegistry", NotificationDataRegistry.Instance, itemSets);
			AssertCollectionContains("WorkflowDataRegistry", WorkflowDataRegistry.Instance, itemSets);
			AssertCollectionContains("WarehouseDataRegistry", Business.Warehouse.WarehouseDataRegistry.Instance, itemSets);
			AssertCollectionContains("WebDataRegistry", WebDataRegistry.Instance, itemSets);
		}

		RegistryItemSetLocator locator;
		MockRegistryItemSetLocator mockLocator;
		GuidRegistryItem testItem1;
		GuidRegistryItem testItem2;

		#region class MockRegistryItemSet

		class MockRegistryItemSet : RegistryItemSet
		{
			public override bool IsForProductivityWise => false;

			public List<IRegistryItem> Items
			{
				get
				{
					if (items == null)
					{
						items = new List<IRegistryItem>();
					}
					return items;
				}
			}

			protected override IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
			{
				return items;
			}

			List<IRegistryItem> items;
		}

		#endregion

		#region class MockRegistryItemSetLocator

		class MockRegistryItemSetLocator : RegistryItemSetLocator
		{
			public List<RegistryItemSet> ItemSets
			{
				get
				{
					if (itemSets == null)
					{
						itemSets = new List<RegistryItemSet>();
					}
					return itemSets;
				}
			}

			protected override IEnumerable<RegistryItemSet> GetRegistryItemSetsCore()
			{
				return ItemSets;
			}

			List<RegistryItemSet> itemSets;
		}

		#endregion
	}
}
