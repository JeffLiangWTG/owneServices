using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TestCaseWithFactoryTest : TestCaseWithFactoryBaseTest
	{
		protected override Type ExpectedConnectionType
		{
			get { return typeof(DbConnection); }
		}

		#region Db Hits

		public void TestAssertDbHits_ShouldFailWhenTableNotHit()
		{
			var cleanFactory = Factory.CreateNewFactory();
			var ex = AssertExceptionThrown<AssertionFailedError>(() =>
			{
				AssertDbHits(new Dictionary<string, int>
				{
					{ "ThisIsNotATable", 1000 },
				}, cleanFactory);
			});

			AssertContains("ThisIsNotATable", ex.Message);
			AssertContains("1000", ex.Message);
		}

		public void TestAssertDbHitsForAllFactories_ShouldTrackTransientFactories()
		{
			var hits = new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 10 },
			};

			using (AssertDbHitsForAllFactories(hits, includeFactoryPredicate: factory => factory.NameForDebugging.StartsWith("AssertDbHitsForAllFactories_Factory")))
			{
				for (var i = 0; i < 10; i++)
				{
					CreateFactoryAndHitDummyBizoTable("AssertDbHitsForAllFactories_Factory");
				}
			}
		}

		public void TestAssertDbHitsForAllFactories_WhenOptionToIgnoreHitsFromBeforeUsingBlock_ShouldRemoveStartingHits()
		{
			GC.Collect(); // if this test is run twice in a row sometimes the factories form the previous run hang around
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Test Factory 1" };
			var factory2 = new BusinessObjectFactory { NameForDebugging = "Test Factory 2" };

			factory1.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid()));
			factory2.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Description, "Shalala"));

			using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 1 } }))
			{
				factory1.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Decimal, 123m));
			}
		}

		static void CreateFactoryAndHitDummyBizoTable(string factoryName)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = factoryName };

			factory.GetDatabaseCount(typeof(DummyBusinessObject));
		}

		public void TestAssertDbHits_WithoutUsefulQueryInfo_ButNoActualHits_ShouldNotPromptToUseBetterFunction()
		{
			var hits = new Dictionary<string, int>() { { DummyBizoSchema.Constants.TableName, 69 } };
			var ex = AssertExceptionThrown<AssertionFailedError>(() => AssertDbHits(hits, Factory));

			AssertNotContains("Should not prompt to use function with extra query info because there weren't any actual hits, so no more useful is to be had.", PromptToUseMoreUsefulDbHitsFunction, ex.Message);
		}

		public void TestAssertDbHits_WithoutUsefulQueryInfo_WithActualHit_ShouldPromptToUseBetterFunction()
		{
			Factory.Load<DummyBusinessObject>(ZGuid.NewZGuid());

			var hits = new Dictionary<string, int>() { { DummyBizoSchema.Constants.TableName, 69 } };
			var ex = AssertExceptionThrown<AssertionFailedError>(() => AssertDbHits(hits, Factory));

			AssertContains("Should prompt to use function with extra query info because it's much easier for finding the cause of db hits without having to set breakpoints in TableHitCounter.", PromptToUseMoreUsefulDbHitsFunction, ex.Message);
		}

		public void TestAssertDbHits_WithUsefulQueryInfo_WithActualHit_ShouldNotPromptToUseBetterFunction()
		{
			var hits = new Dictionary<string, int>() { { DummyBizoSchema.Constants.TableName, 69 } };
			var ex = AssertExceptionThrown<AssertionFailedError>(() =>
			{
				using (AssertDbHitsWithUsefulQueryInformation(hits, Factory))
				{
					Factory.Load<DummyBusinessObject>(ZGuid.NewZGuid());
				}
			});

			AssertNotContains("Should prompt to use function with extra query info because it's much easier for finding the cause of db hits without having to set breakpoints in TableHitCounter.", PromptToUseMoreUsefulDbHitsFunction, ex.Message);
		}

		const string PromptToUseMoreUsefulDbHitsFunction = "Use BusinessObjectFactory.EnableTableHitQueryCollection, or AssertDbHitsWithUsefulQueryInformation to include a full listing of the actual queries and stacktraces for specific factories and the tables passed in with the expectedHitCounts parameter.";

		public void TestAssertDbHitsWithUsefulQueryInformation_ShouldIncludeQueryAndStacktrace()
		{
			void HitDummyTable_FirstWay()
			{
				Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Description, "Borten"));
			}

			void HitDummyTable_SecondWay()
			{
				Factory.Load<DummyBusinessObject>(ZGuid.NewZGuid());
			}

			void HitDummyTable_ThirdAndFinalWay()
			{
				Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Description, "Malcurnbull"));
			}

			HitDummyTable_FirstWay();

			var ex = AssertExceptionThrown<AssertionFailedError>(() =>
			{
				using (AssertDbHitsWithUsefulQueryInformation(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 69 } }, Factory))
				{
					HitDummyTable_SecondWay();
					HitDummyTable_ThirdAndFinalWay();
					HitDummyTable_ThirdAndFinalWay();
				}
			});

			var moreEasilyReadVersion = ex.Message.Replace("<br>", Environment.NewLine).Replace("<br/>", Environment.NewLine).Replace("<br />", Environment.NewLine);

			AssertNotContains("Should not contain query text produced by first hit to DummyBizo table because it happened before we were tracking hits.", "WHERE Z0_Description = 'Borten'", ex.Message);
			AssertContains("Should contain query text produced by second hit to DummyBizo table", "WHERE Z0_PK =", ex.Message);
			AssertContains("Should contain query text produced by third hit to DummyBizo table", "WHERE Z0_Description = 'Malcurnbull'", ex.Message);

			AssertContains("Should contain stacktrace of the second hit to DummyBizo table", "HitDummyTable_SecondWay", ex.Message);
			AssertContains("Should contain stacktrace of the second hit to DummyBizo table", "HitDummyTable_ThirdAndFinalWay", ex.Message);
		}

		#endregion

		public void TestRollbackOfAdditionalFactoryMainConnection()
		{
			AdditionalFactory = new BusinessObjectFactory();
			Type rowFactoryConnectionType = AdditionalFactory.RowFactory.DbConnection.GetType();
			AssertEquals("AdditionalFactory should be using DbConnection: " + rowFactoryConnectionType.FullName, true, rowFactoryConnectionType.IsSubclassOf(typeof(DbConnection)));
			AssertEquals("AdditionalFactory should be using same connection as Factory", Factory.RowFactory.DbConnection, AdditionalFactory.RowFactory.DbConnection);
			AdditionalFactoryShouldRollback = true;
			AssertEquals("AdditionalFactory connection should be in transaction. Using MainConnection", true, AdditionalFactory.RowFactory.DbConnection.IsInTransaction);
		}

		public void TestRollbackOfAdditionalFactoryExtraConnection()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AdditionalFactory = new BusinessObjectFactory(connection);
				AdditionalFactoryShouldRollback = true;
				AssertEquals("Connection should not be in transaction initially", false, AdditionalFactory.RowFactory.DbConnection.IsInTransaction);
				RegisterDbConnectionToRollback(AdditionalFactory.RowFactory.DbConnection);
				AssertEquals("Connection should be in transaction", true, AdditionalFactory.RowFactory.DbConnection.IsInTransaction);
			}
		}

		public void TestAdditionalFactoryExtraConnectionWithoutRollback()
		{
			var connection = Db.NewExtraConnectionToMainDb();
			AdditionalFactory = new BusinessObjectFactory(connection);
			AdditionalFactoryShouldRollback = false;
			AssertEquals("Connection should not be in transaction initially", false, AdditionalFactory.RowFactory.DbConnection.IsInTransaction);
			AdditionalFactory.RowFactory.DbConnection.BeginTransaction();
			AssertEquals("Connection should be in transaction", true, AdditionalFactory.RowFactory.DbConnection.IsInTransaction);
		}

		public void TestAssertContainsExactElementsInAnyOrder()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "string" }, new[] { new ZString("string") });
			AssertContainsExactElementsInAnyOrder(new[] { new ZString("string") }, new[] { "string" });
			AssertContainsExactElementsInAnyOrder("It should work with a message specified as well, and yet...", new[] { "string" }, new[] { new ZString("string") });
			AssertContainsExactElementsInAnyOrder("It should work with a message specified as well, and yet...", new[] { new ZString("string") }, new[] { "string" });

			AssertExceptionThrown<AssertionFailedError>(() => AssertContainsExactElementsInAnyOrder(new[] { "something else" }, new[] { new ZString("string") }));
			AssertExceptionThrown<AssertionFailedError>(() => AssertContainsExactElementsInAnyOrder(new[] { new ZString("something else") }, new[] { "string" }));
			var ex = AssertExceptionThrown<AssertionFailedError>(() => AssertContainsExactElementsInAnyOrder("It should work with a message specified as well, and yet...", new[] { "string" }, new[] { new ZString("something else") }));
			AssertContains("yet...", ex.Message);
			ex = AssertExceptionThrown<AssertionFailedError>(() => AssertContainsExactElementsInAnyOrder("It should work with a message specified as well, and yet...", new[] { new ZString("string") }, new[] { "something else" }));
			AssertContains("yet...", ex.Message);
		}

		public void TestAssertQueryResults()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Byte = 1;
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Byte = 2;
			var dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Byte = 3;
			var dummy4 = Factory.New<DummyBusinessObject>();
			dummy4.Z0_Byte = 4;

			AssertQueryResults(new ZQuery(DummyBizoSchema.Z0_Byte, new byte[] { 2, 3 }),
				(dummy1, false),
				(dummy2, true),
				(dummy3, true),
				(dummy4, false)
			);

			AssertQueryResults(new ZQuery(DummyBizoSchema.Z0_Byte, new byte[] { 2, 3 }),
				(dummy4, false),
				(dummy3, true),
				(dummy2, true),
				(dummy1, false)
			);

			AssertQueryResults(new ZQuery(DummyBizoSchema.Z0_Byte, new byte[] { 2, 3 }),
				(dummy3, true),
				(dummy4, false)
			);

			var ex = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertQueryResults(new ZQuery(DummyBizoSchema.Z0_Byte, new byte[] { 2, 3 }),
					(dummy1, true),
					(dummy2, false)
				)
			);

			AssertContains("should contain query", "Z0_Byte", ex.Message);
			AssertContains("should contain both errors", dummy1.PK.ToString(), ex.Message, true);
			AssertContains("should contain both errors", dummy2.PK.ToString(), ex.Message, true);
			AssertNotContains("should not include unrelated", dummy3.PK.ToString(), ex.Message, true);
			AssertNotContains("should not include unrelated", dummy4.PK.ToString(), ex.Message, true);
		}

		public void TestAssertCodeDescriptionPairList()
		{
			var emptyList = new CodeDescriptionPairList();
			var listOne = new CodeDescriptionPairList();
			listOne.AddPair("ONE", "1");
			var listTwo = new CodeDescriptionPairList();
			listTwo.AddPair("ONE", "1");
			listTwo.AddPair("TWO", "2");

			AssertExceptionThrown<ArgumentNullException>("When list is null and expecting []",
				() => AssertCodeDescriptionPairList(null));
			AssertExceptionThrown<ArgumentNullException>("When list is empty and expecting null",
				() => AssertCodeDescriptionPairList(emptyList, codes: null));

			AssertNoExceptionThrown("When list is empty and expecting []",
				() => AssertCodeDescriptionPairList(emptyList));
			AssertExceptionThrown<AssertionFailedError>("When list is empty and expecting [ {ONE, 1} ]",
				() => AssertCodeDescriptionPairList(emptyList, ("ONE", "1")));

			AssertNoExceptionThrown("When list is [ {ONE, 1} ] and expecting [ {ONE, 1} ]",
				() => AssertCodeDescriptionPairList(listOne, ("ONE", "1")));
			AssertExceptionThrown<AssertionFailedError>("When list is [ {ONE, 1} ] and expecting [ {ONE, 2} ]",
				() => AssertCodeDescriptionPairList(listOne, ("ONE", "2")));
			AssertExceptionThrown<AssertionFailedError>("When list is [ {ONE, 1} ] and expecting [ {TWO, 1} ]",
				() => AssertCodeDescriptionPairList(listOne, ("TWO", "1")));

			AssertNoExceptionThrown("When list is [ {ONE, 1}, {TWO, 2} ] and expecting [ {ONE, 1}, {TWO, 2} ]",
				() => AssertCodeDescriptionPairList(listTwo, ("ONE", "1"), ("TWO", "2")));
			AssertNoExceptionThrown("When list is [ {ONE, 1}, {TWO, 2} ] and expecting [ {TWO, 2}, {ONE, 1} ]",
				() => AssertCodeDescriptionPairList(listTwo, ("TWO", "2"), ("ONE", "1")));
			AssertExceptionThrown<AssertionFailedError>("When list is [ {ONE, 1}, {TWO, 2} ] and expecting [ {ONE, 1}, {ONE, 1} ]",
				() => AssertCodeDescriptionPairList(listTwo, ("ONE", "1"), ("ONE", "1")));
			AssertExceptionThrown<AssertionFailedError>("When list is [ {ONE, 1}, {TWO, 2} ] and expecting [ {ONE, 1} ]",
				() => AssertCodeDescriptionPairList(listTwo, ("ONE", "1")));
			AssertExceptionThrown<AssertionFailedError>("When list is [ {ONE, 1}, {TWO, 2} ] and expecting [ {TWO, 2} ]",
				() => AssertCodeDescriptionPairList(listTwo, ("TWO", "2")));
		}

		public void TestAssertCached()
		{
			const string customMessage = "my-message";
			string cache = null;
			Func<string> getNewValue = () => Guid.NewGuid().ToString();
			Func<string> getCachedValue = () => cache ??= getNewValue();

			AssertNoExceptionThrown("When values are same and no custom message is passed", () => AssertCached(getCachedValue));

			AssertNoExceptionThrown("When values are same and a custom message is passed", () => AssertCached(customMessage, getCachedValue));

			var assertError1 = AssertExceptionThrown<AssertionFailedError>(() => AssertCached(getNewValue));
			AssertEquals("When values are different, default message should be returned", "Cached expected same", assertError1.Message);

			var assertError2 = AssertExceptionThrown<AssertionFailedError>(() => AssertCached(customMessage, getNewValue));
			AssertEquals("When values are different, custom message should be returned", $"{customMessage} expected same", assertError2.Message);
		}

		public void TestAssertArgumentExceptionThrown()
		{
			const string defaultAssertMessage = "Expected an exception of type [System.ArgumentException] but the following exception was thrown instead:\n\n";
			const string customAssertMessage = "my-message";
			const string param = null;
			const string otherParam = null;
			AssertFail("When not null", () => Argument.NotNull(42, nameof(param)));
			AssertFail("When wrong ParamName", () => Argument.NotNull(otherParam, nameof(otherParam)));
			AssertFail("When wrong Message", () => throw new ArgumentException("actual message", nameof(param)), "expected message");
			AssertFail("When wrong exception", () => throw new ());

			var exception = AssertArgumentExceptionThrown(nameof(param), () => Argument.NotNull(param, nameof(param)));
			AssertEquals("When argument is null, ParamName", nameof(param), exception.ParamName);

			var assertionFailedError1 = AssertExceptionThrown<AssertionFailedError>(() => AssertArgumentExceptionThrown(nameof(param), () => throw new()));
			AssertStartsWith("When default assert message", Html(defaultAssertMessage), assertionFailedError1.Message);

			var assertionFailedError2 = AssertExceptionThrown<AssertionFailedError>(() => AssertArgumentExceptionThrown(customAssertMessage, nameof(param), () => throw new()));
			AssertStartsWith("When custom assert message", Html($"{customAssertMessage}.\n{defaultAssertMessage}"), assertionFailedError2.Message);

			void AssertFail(string message, AnonymousMethod codeToRun, string exceptionMessage = null)
			{
				AssertExceptionThrown<AssertionFailedError>(message, () => AssertArgumentExceptionThrown(nameof(param), codeToRun, exceptionMessage));
			}
		}

		public void TestAssertArgumentExceptionThrownGeneric()
		{
			const string defaultAssertMessage = "Expected an exception of type [System.ArgumentNullException] but the following exception was thrown instead:\n\n";
			const string customAssertMessage = "my-message";
			const string param = null;
			const string otherParam = null;
			AssertFail<ArgumentException>("When not null", () => Argument.NotNull(42, nameof(param)));
			AssertFail<ArgumentException>("When wrong ParamName", () => Argument.NotNull(otherParam, nameof(otherParam)));
			AssertFail<ArgumentException>("When wrong Message", () => throw new ArgumentException("actual message", nameof(param)), "expected message");
			AssertFail<ArgumentNullException>("When wrong exception", () => Argument.GreaterThan(1, 2, nameof(param)));

			var exception = AssertArgumentExceptionThrown<ArgumentNullException>(nameof(param), () => Argument.NotNull(param, nameof(param)));
			AssertType<ArgumentNullException>("When argument is null, Return type", exception);
			AssertEquals("When argument is null, ParamName", nameof(param), exception.ParamName);

			var assertionFailedError1 = AssertExceptionThrown<AssertionFailedError>(() => AssertArgumentExceptionThrown<ArgumentNullException>(nameof(param), () => throw new()));
			AssertStartsWith("When default assert message", Html(defaultAssertMessage), assertionFailedError1.Message);

			var assertionFailedError2 = AssertExceptionThrown<AssertionFailedError>(() => AssertArgumentExceptionThrown<ArgumentNullException>(customAssertMessage, nameof(param), () => throw new()));
			AssertStartsWith("When custom assert message", Html($"{customAssertMessage}.\n{defaultAssertMessage}"), assertionFailedError2.Message);

			void AssertFail<TException>(string message, AnonymousMethod codeToRun, string exceptionMessage = null) where TException : ArgumentException
			{
				AssertExceptionThrown<AssertionFailedError>(message, () => AssertArgumentExceptionThrown<TException>(nameof(param), codeToRun, exceptionMessage));
			}
		}

		public void TestAssertEntityHasProperty()
		{
			_ = AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
				.HasProperty(x => x.Z0_Decimal);

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
					.HasProperty(x => "I am not a property")
			);
			AssertEquals("Could not find property System.String within CargoWise.EntityFramework.Testing.TestCaseWithFactoryTest+DummyBusinessObjectWithDecimalPlacesAttributeForTest with provided propertySelector<br />" +
				"  Expected:  x => x.MyProperty<br />" +
				"  But found: x => \"I am not a property\"", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithAttribute()
		{
			_ = AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
				.HasProperty(x => x.Z0_Decimal)
				.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 3);

			_ = AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
				.HasProperty(x => x.Z0_AnotherDecimal)
				.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlacesMember == "DecimalPlaces");

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
					.HasProperty(x => x.Z0_Decimal)
					.WithAttribute<DecimalPrecisionAttribute>()
			);
			AssertEquals("DummyBusinessObjectWithDecimalPlacesAttributeForTest::Z0_Decimal should have DecimalPrecisionAttribute<br>" +
				" expected <div style='background-color: rgb(230,255,230)'>0</div> not to be equal to <div style='background-color: rgb(255,230,230)'>0</div>", assertError.Message);

			assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
					.HasProperty(x => x.Z0_Decimal)
					.WithAttribute<DecimalPrecisionAttribute>(because: "Reasons")
			);
			AssertEquals("Reasons<br>" +
				"DummyBusinessObjectWithDecimalPlacesAttributeForTest::Z0_Decimal should have DecimalPrecisionAttribute<br>" +
				" expected <div style='background-color: rgb(230,255,230)'>0</div> not to be equal to <div style='background-color: rgb(255,230,230)'>0</div>", assertError.Message);

			assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
					.HasProperty(x => x.Z0_Decimal)
					.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 42)
			);
			AssertEquals("DummyBusinessObjectWithDecimalPlacesAttributeForTest::Z0_Decimal should have DecimalPlacesAttribute<br />" +
				"  Where:   x => (x.DecimalPlaces == 42)", assertError.Message);

			assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithDecimalPlacesAttributeForTest>()
					.HasProperty(x => x.Z0_Decimal)
					.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 42, because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithDecimalPlacesAttributeForTest::Z0_Decimal should have DecimalPlacesAttribute<br />" +
				"  Where:   x => (x.DecimalPlaces == 42)<br />" +
				"  Because: Reasons", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithNullValue()
		{
			_ = AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
				.HasProperty(x => x.Z0_NVarCharMax)
				.WithCaption("TEST caption");

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
					.HasProperty(x => x.Z0_NVarCharMax)
					.WithFullDescription("SOME caption", because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithCaptionsForTest::Z0_NVarCharMax should have ResourceStringDataAttribute<br />" +
				"  Where:   FullDescription is 'SOME caption'<br />" +
				"  But was: NULL<br />" +
				"  Because: Reasons", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithCaption()
		{
			_ = AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
				.HasProperty(x => x.Z0_NVarChar)
				.WithCaption("TEST caption");

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
					.HasProperty(x => x.Z0_NVarChar)
					.WithCaption("DIFFERENT caption", because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithCaptionsForTest::Z0_NVarChar should have ResourceStringDataAttribute<br />" +
				"  Where:   Caption is 'DIFFERENT caption'<br />" +
				"  But was: 'TEST caption'<br />" +
				"  Because: Reasons", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithShortCaption()
		{
			_ = AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
				.HasProperty(x => x.Z0_NVarChar)
				.WithShortCaption("TEST short caption");

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
					.HasProperty(x => x.Z0_NVarChar)
					.WithShortCaption("DIFFERENT short caption", because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithCaptionsForTest::Z0_NVarChar should have ResourceStringDataAttribute<br />" +
				"  Where:   ShortCaption is 'DIFFERENT short caption'<br />" +
				"  But was: 'TEST short caption'<br />" +
				"  Because: Reasons", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithMediumCaption()
		{
			_ = AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
				.HasProperty(x => x.Z0_NVarChar)
				.WithMediumCaption("TEST medium caption");

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
					.HasProperty(x => x.Z0_NVarChar)
					.WithMediumCaption("DIFFERENT medium caption", because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithCaptionsForTest::Z0_NVarChar should have ResourceStringDataAttribute<br />" +
				"  Where:   MediumCaption is 'DIFFERENT medium caption'<br />" +
				"  But was: 'TEST medium caption'<br />" +
				"  Because: Reasons", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithFullDescription()
		{
			_ = AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
				.HasProperty(x => x.Z0_NVarChar)
				.WithFullDescription("TEST full description");

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithCaptionsForTest>()
					.HasProperty(x => x.Z0_NVarChar)
					.WithFullDescription("DIFFERENT full description", because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithCaptionsForTest::Z0_NVarChar should have ResourceStringDataAttribute<br />" +
				"  Where:   FullDescription is 'DIFFERENT full description'<br />" +
				"  But was: 'TEST full description'<br />" +
				"  Because: Reasons", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithList()
		{
			_ = AssertEntity<DummyBusinessObjectWithListForTest>()
				.HasProperty(x => x.Z0_NVarChar)
				.WithList("ActualCodeList");

			var assertError = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithListForTest>()
					.HasProperty(x => x.Z0_NVarChar)
					.WithList("ExpectedCodeList", because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithListForTest::Z0_NVarChar should have ListAttribute<br />" +
				"  Where:   ListDataSourceMember is 'ExpectedCodeList'<br />" +
				"  But was: 'ActualCodeList'<br />" +
				"  Because: Reasons", assertError.Message);
		}

		public void TestAssertEntityHasPropertyWithMaxLength()
		{
			_ = AssertEntity<DummyBusinessObjectWithMaxLengthForTest>()
				.HasProperty(x => x.Z0_Description)
				.WithMaxLength("ActualMaxLength");

			_ = AssertEntity<DummyBusinessObjectWithMaxLengthForTest>()
				.HasProperty(x => x.Z0_NVarChar)
				.WithMaxLength(42);

			var assertError1 = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithMaxLengthForTest>()
					.HasProperty(x => x.Z0_Description)
					.WithMaxLength("ExpectedMaxLength", because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithMaxLengthForTest::Z0_Description should have MaxLengthAttribute<br />" +
				"  Where:   MaxLengthMember is 'ExpectedMaxLength'<br />" +
				"  But was: 'ActualMaxLength'<br />" +
				"  Because: Reasons", assertError1.Message);

			var assertError2 = AssertExceptionThrown<AssertionFailedError>(() =>
				AssertEntity<DummyBusinessObjectWithMaxLengthForTest>()
					.HasProperty(x => x.Z0_NVarChar)
					.WithMaxLength(666, because: "Reasons")
			);
			AssertEquals("DummyBusinessObjectWithMaxLengthForTest::Z0_NVarChar should have MaxLengthAttribute<br />" +
				"  Where:   MaxLength is 666<br />" +
				"  But was: 42<br />" +
				"  Because: Reasons", assertError2.Message);
		}

		public void TestAssertHasDecimalPlacesAttribute()
		{
			AssertExceptionThrown<ArgumentNullException>("When the ZPropertyInfo argument is null", () => AssertHasDecimalPlacesAttribute(null, 2));

			var dummyBizO = Factory.New<DummyBusinessObjectWithDecimalPlacesAttributeForTest>();

			AssertNoExceptionThrown(() => AssertHasDecimalPlacesAttribute(dummyBizO.Z0_DecimalInfo, 3));
			AssertNoExceptionThrown(() => AssertHasDecimalPlacesAttribute(dummyBizO.Z0_AnotherDecimalInfo, 2));

			AssertExceptionThrown<AssertionFailedError>("When DummyBusinessObject.Z0_AnotherNumber has DecimalPlacesAttribute but it points to an nonexistent property",
				() => AssertHasDecimalPlacesAttribute(dummyBizO.Z0_AnotherNumberInfo, 0));

			AssertExceptionThrown<AssertionFailedError>("When DummyBusinessObject.Z0_Decimal has DecimalPlacesAttribute but it does not meet the criteria Expected decimal places: 2",
				() => AssertHasDecimalPlacesAttribute(dummyBizO.Z0_DecimalInfo, 2));

			AssertExceptionThrown<AssertionFailedError>("When DummyBusinessObject.Z0_Money does not have DecimalPlacesAttribute",
				() => AssertHasDecimalPlacesAttribute(dummyBizO.Z0_MoneyInfo, 4));

			var childDummyBizO = Factory.New<ChildDummyBusinessObjectWithoutDecimalPlacesAttributeForTest>();
			var assertError = AssertExceptionThrown<AssertionFailedError>(
				"When ChildDummyBusinessObject.Z0_Decimal does not have DecimalPlacesAttribute",
				() => AssertHasDecimalPlacesAttribute("My assertion message", childDummyBizO.Z0_DecimalInfo, 2)
			);
			AssertEquals("My assertion message<br>" +
				"ChildDummyBusinessObjectWithoutDecimalPlacesAttributeForTest::Z0_Decimal should have DecimalPlacesAttribute<br>" +
				" expected <div style='background-color: rgb(230,255,230)'>0</div> not to be equal to <div style='background-color: rgb(255,230,230)'>0</div>",
				assertError.Message);

			assertError = AssertExceptionThrown<AssertionFailedError>(
				"When no custom assertion message has been specified",
				() => AssertHasDecimalPlacesAttribute(childDummyBizO.Z0_DecimalInfo, 2)
			);
			AssertEquals("ChildDummyBusinessObjectWithoutDecimalPlacesAttributeForTest::Z0_Decimal should have DecimalPlacesAttribute<br>" +
				" expected <div style='background-color: rgb(230,255,230)'>0</div> not to be equal to <div style='background-color: rgb(255,230,230)'>0</div>",
				assertError.Message);
		}

		public override void RunBare()
		{
			AssertEquals(TransactionLevelErrorMessages.Initial, false, Factory.RowFactory.DbConnection.IsInTransaction);
			AssertEquals(TransactionLevelErrorMessages.Initial, 0, Factory.RowFactory.DbConnection.AppTransactionCount);
			base.RunBare();
			AssertEquals(TransactionLevelErrorMessages.After, false, Factory.RowFactory.DbConnection.IsInTransaction);
			AssertEquals(TransactionLevelErrorMessages.After, 0, Factory.RowFactory.DbConnection.AppTransactionCount);
			if (AdditionalFactory != null)
			{
				if (AdditionalFactoryShouldRollback)
				{
					AssertEquals("AdditionalFactory should have been rolled back", false, AdditionalFactory.RowFactory.DbConnection.IsInTransaction);
					AssertEquals("AdditionalFactory AppTransactionCount", 0, AdditionalFactory.RowFactory.DbConnection.AppTransactionCount);
				}
				else
				{
					AssertEquals("AdditionalFactory should still be in transaction", true, AdditionalFactory.RowFactory.DbConnection.IsInTransaction);
					RollbackAndDisposeConnectionUsedForTesting(AdditionalFactory.RowFactory.DbConnection);
				}
			}
		}
		BusinessObjectFactory AdditionalFactory;
		bool AdditionalFactoryShouldRollback;

		void RollbackAndDisposeConnectionUsedForTesting(DbConnection connection)
		{
			int currentTransactionCount = connection.AppTransactionCount;
			while (currentTransactionCount > 0)
			{
				connection.RollbackTransaction();
				AssertEquals("Transaction count should lower by 1 as it is rolledback", currentTransactionCount - 1, connection.AppTransactionCount);
				currentTransactionCount = connection.AppTransactionCount;
			}
			AssertEquals("Transaction count should be 0 after test", 0, connection.AppTransactionCount);
			AssertEquals("Connection should not be in transaction", false, connection.IsInTransaction);
			connection.Dispose();
		}

		class DummyBusinessObjectWithCaptionsForTest : DummyBusinessObject
		{
			public DummyBusinessObjectWithCaptionsForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ResourceStringData("3EE7CAC6-1EC5-4DB4-8045-798229BB2092",
				Caption = "TEST caption",
				MediumCaption = "TEST medium caption",
				ShortCaption = "TEST short caption",
				FullDescription = "TEST full description")]
			public override ZString Z0_NVarChar { get => base.Z0_NVarChar; set => base.Z0_NVarChar = value; }

			[ResourceStringData("17D0ECAB-F4AF-4AE9-8D38-61610603597F",
				Caption = "TEST caption")]
			public override ZString Z0_NVarCharMax { get => base.Z0_NVarCharMax; set => base.Z0_NVarCharMax = value; }
		}

		class DummyBusinessObjectWithMaxLengthForTest : DummyBusinessObject
		{
			public DummyBusinessObjectWithMaxLengthForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[MaxLength(42)]
			public override ZString Z0_NVarChar { get => base.Z0_NVarChar; set => base.Z0_NVarChar = value; }

			[MaxLength("ActualMaxLength")]
			public override ZString Z0_Description { get; set; }
		}

		class DummyBusinessObjectWithListForTest : DummyBusinessObject
		{
			public DummyBusinessObjectWithListForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[List("ActualCodeList")]
			public override ZString Z0_NVarChar { get => base.Z0_NVarChar; set => base.Z0_NVarChar = value; }
		}

		class DummyBusinessObjectWithDecimalPlacesAttributeForTest : DummyBusinessObject
		{
			public DummyBusinessObjectWithDecimalPlacesAttributeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[DecimalPlaces(3)]
			public override ZDecimal Z0_Decimal { get => base.Z0_Decimal; set => base.Z0_Decimal = value; }

			[DecimalPlaces(nameof(DecimalPlaces))]

			public override ZDecimal Z0_AnotherDecimal { get => base.Z0_AnotherDecimal; set => base.Z0_AnotherDecimal = value; }

			[DecimalPlaces("Not Existing Property")]
			public override ZInt Z0_AnotherNumber { get => base.Z0_AnotherNumber; set => base.Z0_AnotherNumber = value; }

			int DecimalPlaces => 2;
		}

		class ChildDummyBusinessObjectWithoutDecimalPlacesAttributeForTest : DummyBusinessObject
		{
			public ChildDummyBusinessObjectWithoutDecimalPlacesAttributeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
