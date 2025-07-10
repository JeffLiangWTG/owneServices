using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Core.Environment.Internal;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryItemSetTest : TransactionedTestCase
	{
		public void TestCombineCategories()
		{
			AssertEquals("CombineCategories()", "", RegistryItemSet.CombineCategories());
			AssertEquals("CombineCategories(null, null)", "", RegistryItemSet.CombineCategories(null, null));
			AssertEquals("CombineCategories(\" \", \" / \")", "", RegistryItemSet.CombineCategories((NoResString)" ", (NoResString)" / "));
			AssertEquals("CombineCategories(\" x\", \" / \")", "x", RegistryItemSet.CombineCategories((NoResString)" x", (NoResString)" / "));
			AssertEquals("CombineCategories(\" \", \" y/ \")", "y", RegistryItemSet.CombineCategories((NoResString)" ", (NoResString)" y/ "));
			AssertEquals("CombineCategories(\"x\", \"y\")", "x/y", RegistryItemSet.CombineCategories((NoResString)"x", (NoResString)"y"));
			AssertEquals("CombineCategories(\"x/\", \"y\")", "x/y", RegistryItemSet.CombineCategories((NoResString)"x/", (NoResString)"y"));
			AssertEquals("CombineCategories(\"x\", \"/y\")", "x/y", RegistryItemSet.CombineCategories((NoResString)"x", (NoResString)"/y"));
			AssertEquals("CombineCategories(\"/x/\", \"/y/\")", "x/y", RegistryItemSet.CombineCategories((NoResString)"/x/", (NoResString)"/y/"));
			AssertEquals("CombineCategories(\"x\", \"y\", \"z\")", "x/y/z", RegistryItemSet.CombineCategories((NoResString)"x", (NoResString)"y", (NoResString)"z"));
		}

		public void TestFindByName()
		{
			AssertNull("FindByName(null)", ItemSet.FindByName(null));
			AssertNull("FindByName(\"\")", ItemSet.FindByName(""));
			AssertNull("FindByName(\"x\")", ItemSet.FindByName("x"));
			AssertEquals("FindByName(\"STRING_ITEM_1\")", ItemSet.StringItem1, ItemSet.FindByName("STRING_ITEM_1"));
			AssertEquals("FindByName(\"STRING_item_2\")", ItemSet.StringItem2, ItemSet.FindByName("STRING_item_2"));
		}

		public void TestGetAllItems()
		{
			IRegistryItem[] items = ItemSet.GetAllItems();
			AssertEquals("Length", 6, items.Length);
			AssertEquals("[0].Name", ItemSet.BooleanItem.Name, items[0].Name);
			AssertEquals("[1]", ItemSet.StringItem1, items[1]);
			AssertEquals("[2]", ItemSet.StringItem2, items[2]);
			AssertEquals("[5]", ItemSet.ExplosiveItem.HasOption(RegistryOptions.CannotCallParameterlessValueGetter), items[5].HasOption(RegistryOptions.CannotCallParameterlessValueGetter));

			StringRegistryItem additionalItem = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			ItemSet.AddAdditionalItem(additionalItem);

			items = ItemSet.GetAllItems();
			AssertEquals("Length", 7, items.Length);
			AssertEquals("[0].Name", ItemSet.BooleanItem.Name, items[0].Name);
			AssertEquals("[1]", ItemSet.StringItem1, items[1]);
			AssertEquals("[2]", ItemSet.StringItem2, items[2]);
			AssertEquals("[5]", ItemSet.ExplosiveItem, items[5]);
			AssertEquals("[6]", additionalItem, items[6]);
		}

		public void TestGetItem()
		{
			IRegistryItemDictionaryInternals dictionaryInternals = RegistryItemDictionary.Instance;

			IRegistryItem item1 = ItemSet.StringItem1;
			IRegistryItem item2 = ItemSet.StringItem2;

			RegistryItemHolder holder1 = dictionaryInternals.GetHolder("STRING_ITEM_1");
			RegistryItemHolder holder2 = dictionaryInternals.GetHolder("STRING_ITEM_2");

			Assert("StringItem1's ElapsedSinceLastUse", holder1.ElapsedSinceLastUse.TotalSeconds < 1);
			Assert("StringItem2's ElapsedSinceLastUse", holder2.ElapsedSinceLastUse.TotalSeconds < 1);

			var span = new TimeSpan(0, 10, 0);
			holder1.ElapsedSinceLastUse = span;
			holder2.ElapsedSinceLastUse = span;

			AssertEquals("StringItem1 should be the same instance.", item1, ItemSet.StringItem1);
			Assert("StringItem1's ElapsedSinceLastUse", dictionaryInternals.GetHolder("STRING_ITEM_1").ElapsedSinceLastUse.TotalSeconds < 1);
			AssertEquals("StringItem2's ElapsedSinceLastUse", span, dictionaryInternals.GetHolder("STRING_ITEM_2").ElapsedSinceLastUse);

			dictionaryInternals.Purge("STRING_ITEM_1");
			Assert("StringItem1 should be a different instance.", ItemSet.StringItem1 != item1);
			AssertEquals("StringItem2 should be the same instance.", item2, ItemSet.StringItem2);

			Assert("StringItem1's ElapsedSinceLastUse", dictionaryInternals.GetHolder("STRING_ITEM_1").ElapsedSinceLastUse.TotalSeconds < 1);
			Assert("StringItem2's ElapsedSinceLastUse", dictionaryInternals.GetHolder("STRING_ITEM_2").ElapsedSinceLastUse.TotalSeconds < 1);
		}

		public void TestGetNonCachedItem()
		{
			AssertNotEquals("BooleanItem should be a different instance.", ItemSet.BooleanItem, ItemSet.BooleanItem);
			AssertNull("BooleanItem should not be in the dictionary.", RegistryItemDictionary.Instance.GetItem("BOOLEAN_ITEM"));
		}

		public void TestGetUndefinedCodeDescriptionPairList()
		{
			CodeDescriptionPairList list = ItemSet.GetUndefinedCodeDescriptionPairList((NoResString)"Category/SubCategory");
			AssertEquals("Count", 1, list.Count);
			AssertEquals("GetDescriptionFromCode(\"UDF\")", "Undefined - You can modify this in the System Registry, under Category/SubCategory", list.GetDescriptionFromCode("UDF"));
		}

		public void TestRemoveItemFromCacheIfOlderThan()
		{
			IRegistryItemDictionaryInternals dictionaryInternals = RegistryItemDictionary.Instance;

			IRegistryItem item1 = ItemSet.StringItem1;
			IRegistryItem item2 = ItemSet.StringItem2;

			RegistryItemHolder holder1 = dictionaryInternals.GetHolder("STRING_ITEM_1");
			RegistryItemHolder holder2 = dictionaryInternals.GetHolder("STRING_ITEM_2");
			Assert(holder1.ElapsedSinceLastUse.TotalSeconds < 1);
			Assert(holder2.ElapsedSinceLastUse.TotalSeconds < 1);

			var span = new TimeSpan(0, 0, -1);

			ItemSet.RemoveItemFromCacheIfOlderThan(item1.Name, span);
			Assert("StringItem1 should be a different instance.", ItemSet.StringItem1 != item1);
			Assert("StringItem1's ElapsedSinceLastUse", dictionaryInternals.GetHolder(item2.Name).ElapsedSinceLastUse.TotalSeconds < 1);

			span += new TimeSpan(0, 10, 0);

			ItemSet.RemoveItemFromCacheIfOlderThan(item2.Name, span);
			AssertEquals("StringItem2 should be the same instance.", item2, ItemSet.StringItem2);
			Assert("StringItem2's ElapsedSinceLastUse", dictionaryInternals.GetHolder(item2.Name).ElapsedSinceLastUse.TotalSeconds < 1);

			holder2.ElapsedSinceLastUse = span + new TimeSpan(0, 0, 1);
			ItemSet.RemoveItemFromCacheIfOlderThan(item2.Name, span);
			Assert("StringItem2 should be a different instance.", ItemSet.StringItem2 != item2);
			Assert("StringItem2's ElapsedSinceLastUse", dictionaryInternals.GetHolder("STRING_ITEM_2").ElapsedSinceLastUse.TotalSeconds < 1);
		}

		public void TestFranceAndOverseasDepartmentsShouldIncludeDOMs()
		{
			var list = new List<Guid>
			{
				Enterprise.Core.Constants.CountryGuids.France,
				Enterprise.Core.Constants.CountryGuids.FrenchGuiana,
				Enterprise.Core.Constants.CountryGuids.Mayotte,
				Enterprise.Core.Constants.CountryGuids.Guadeloupe,
				Enterprise.Core.Constants.CountryGuids.Martinique,
				Enterprise.Core.Constants.CountryGuids.Reunion,
				Enterprise.Core.Constants.CountryGuids.SaintBarthelemy,
				Enterprise.Core.Constants.CountryGuids.SaintMartin
			};
			AssertContainsExactElementsInAnyOrder(RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments, list);
		}

		public void TestUKIsRemovedFromEU()
		{
			AssertCollectionNotContains("EuropeanUnion filter should not contain UnitedKingdom", Enterprise.Core.Constants.CountryGuids.UnitedKingdom, RegistryItemSet.CountryFilterPKs.EuropeanUnion);
		}

		public void TestTraceRegistryAccess()
		{
			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			var dateTimeRegistryItem = ItemSet.DateTimeItem;
			var expectedString = """

				Registry Path: /
				Registry Key: DATETIME_ITEM
				Registry Data Type: DateTimeRegistryDataType
				Value: 0001-01-01 00:00:00.000
				""";
			AssertEquals("Trace registry access", expectedString, dummyTracer.Traces[0]);

			var codePairRegistryItem = ItemSet.CodePairItem;
			expectedString = """

				Registry Path: /
				Registry Key: CODEPAIR_ITEM
				Registry Data Type: CodePairRegistryDataType
				Value: 
				""";
			AssertEquals("Trace registry access", expectedString, dummyTracer.Traces[1]);

			var valueGetterForbiddenItem = ItemSet.ExplosiveItem;
			expectedString = """

				Registry Path: /
				Registry Key: EXPLOSIVE_ITEM
				Registry Data Type: IntRegistryDataType
				Value: ******** (Error Retrieving Value: System.NotSupportedException)
				""";

			AssertEquals("Trace registry access", expectedString, dummyTracer.Traces[2]);
		}

		public void TestTraceSensitiveRegistryAccess()
		{
			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			var registryItem = ItemSet.StringItem1;
			AssertContains("Trace empty string registry item", "Value: ******** (is empty)", dummyTracer.Traces[0]);

			using (ItemSet.StringItem1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "temporary string"))
			{
				registryItem = ItemSet.StringItem1;
				AssertContains("Trace empty string registry item", "Value: ******** (is not empty)", dummyTracer.Traces[2]);
			}

			var datetimeRegistryItem = ItemSet.DateTimeItem;
			AssertNotContains("Tracing registries of acceptable sub-datatypes should not censor the value", "Value: ********", dummyTracer.Traces[3]);

			var codePairRegistryItem = ItemSet.CodePairItem;
			AssertNotContains("Tracing registries of acceptable registry data types should not censor the value", "Value: ********", dummyTracer.Traces[4]);
		}

		public void TestTraceRegistryAccess_LogsToErrorReporter_WhenExceptionThrown()
		{
			ItemSet.DateTimeItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 08, 02));

			var writer = new DummyMessageWriter();
			var config = new TraceSourceConfiguration()
			{
				TraceSourceSettings = new[]
				{
					new TraceSourceSettingsConfiguration() { SourceLevel = System.Diagnostics.SourceLevels.All, TraceSourceName = CoreTraceSourceCodes.Registry, TraceFilter = "" },
				},
				MessageWriter = writer,
				GetTracePrefix = () => throw new ApplicationException("Boom!"),
			};
			var tracer = ObjectFactory.Get<ITracer>();

			var tracerConfig = (ITracerConfiguration)tracer;
			tracerConfig.InitializeTraceSources(config);

			var value = ItemSet.DateTimeItem.Value;
			AssertEquals("Exception from tracing should not prevent registry returning values", new DateTime(2022, 08, 02), value);

			CombineAssertions("Unhandled exception when tracing should be sent to ErrorReporter", () =>
			{
				AssertType<ApplicationException>(ErrorReporter.LastExceptionReported);
				AssertEquals("RegistryItemSet.TraceRegistryAccess", ErrorReporter.LastKeyReported);
				AssertEquals("Unhandled exception was caught tracing registry item access.", ErrorReporter.LastMessageReported);
			});
			AssertEquals("Exception should prevent tracing", 0, writer.Messages.Count);

			ErrorReporter.Clear();
		}

		public void TestTraceRegistryAccess_ErrorNotReportedForCannotCallParameterlessValueGetter()
		{
			var tracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(tracer);

			var value = ItemSet.ExplosiveItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Tracing should not break registry access", 42, value);

			CombineAssertions("Registry access should not trigger ErrorReporter", () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertNullOrEmpty(ErrorReporter.LastKeyReported);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			});
			AssertEquals("Tracing should work as usual", 1, tracer.Traces.Count);

			ErrorReporter.Clear();
		}

		MockRegistryItemSet ItemSet
		{
			get
			{
				if (itemSet == null)
				{
					itemSet = new MockRegistryItemSet();
				}
				return itemSet;
			}
		}

		MockRegistryItemSet itemSet;

		#region class MockRegistryItemSet

		class MockRegistryItemSet : RegistryItemSet
		{
			public override bool IsForProductivityWise => false;

			public BooleanRegistryItem BooleanItem
			{
				get
				{
					return GetNonCachedItem(delegate
					{
						return new BooleanRegistryItem("BOOLEAN_ITEM", null, null, null, RegistryStorageFlags.System, false);
					});
				}
			}

			public IRegistryItem StringItem1
			{
				get
				{
					return GetItem("STRING_ITEM_1", delegate
					{
						return new StringRegistryItem("STRING_ITEM_1", null, null, null, RegistryStorageFlags.System);
					});
				}
			}

			public StringRegistryItem StringItem2
			{
				get
				{
					return GetItem("STRING_ITEM_2", delegate
					{
						return new StringRegistryItem("STRING_ITEM_2", null, null, null, RegistryStorageFlags.System);
					});
				}
			}

			public DateTimeRegistryItem DateTimeItem
			{
				get
				{
					return GetItem("DATETIME_ITEM", delegate
					{
						return new DateTimeRegistryItem("DATETIME_ITEM", null, null, null, RegistryStorageFlags.System);
					});
				}
			}

			public CodePairRegistryItem CodePairItem
			{
				get
				{
					return GetItem("CODEPAIR_ITEM", delegate
					{
						return new CodePairRegistryItem("CODEPAIR_ITEM", null, null, null, OLookUpEditType.AccountOrderType, RegistryStorageFlags.System);
					});
				}
			}

			public IntRegistryItem ExplosiveItem
			{
				get
				{
					return GetItem("EXPLOSIVE_ITEM", delegate
					{
						return new IntRegistryItem("EXPLOSIVE_ITEM", null, null, null, RegistryStorageFlags.System, RegistryOptions.CannotCallParameterlessValueGetter, 42);
					});
				}
			}

			public void AddAdditionalItem(IRegistryItem item)
			{
				if (additionalItems == null)
				{
					additionalItems = new List<IRegistryItem>();
				}
				additionalItems.Add(item);
			}

			public new CodeDescriptionPairList GetUndefinedCodeDescriptionPairList(MultilingualString itemPath)
			{
				return base.GetUndefinedCodeDescriptionPairList(itemPath);
			}

			protected override IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
			{
				return additionalItems;
			}

			public new void RemoveItemFromCacheIfOlderThan(string key, TimeSpan age)
			{
				base.RemoveItemFromCacheIfOlderThan(key, age);
			}

			List<IRegistryItem> additionalItems;
		}

		#endregion
	}
}
