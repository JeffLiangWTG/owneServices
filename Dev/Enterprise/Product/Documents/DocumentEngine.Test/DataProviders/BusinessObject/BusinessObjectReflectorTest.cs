using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class BusinessObjectReflectorTest : TestCaseWithFactory
	{
		public void TestShouldNotTranslateFieldWhichHasDocumentMacroIgnoreAttributeOrComeFromIPasswordStored()
		{
			var dummy = Factory.New<DummyPasswordStoredBODocSupportable>();
			dummy.Z0_VarCharMax = "Text VarCharMax";
			dummy.Dummy_PasswordHash = new ZBlob(new byte[] { 1 });
			dummy.Dummy_PasswordSalt = new ZBlob(new byte[] { 2 });

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<Z0_VarCharMax>] {C}-[<Dummy_PasswordHash>] {D}-[<Dummy_PasswordSalt>] {E}-[<PasswordHash>] {F}-[<PasswordSalt>]
{A}-[#EndOfReport]", "UnitTest");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var expectedResult = @"{B}-[Text VarCharMax]
";
				AssertMultilineASCIIEquals("DocumentMacroIgnore and IPasswordStored property should be ignored.", expectedResult, excelInterface.WorkSheets.First().ToString());
			}
		}

		public void TestFormatFunctionWithManyBrackets()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<Collection.Format(""{Number} ({Text})"", Comma)>]
{A}-[#EndOfReport]", "UnitTest");

			var dummy = Factory.New<DummyDocumentSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_VarCharMax = "AAA";
			child1.Z0_Number = 111;

			var child2 = dummy.Collection.AddNew();
			child2.Z0_VarCharMax = "BBB";
			child2.Z0_Number = 222;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("Brackets should be displayed correctly.", "{B}-[111 (AAA), 222 (BBB)]", excelInterface.WorkSheets.First().ToString());
			}
		}

		public void TestEndToEnd()
		{
			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = "Text Parent";

			var element1 = dummy.Collection.AddNew();
			element1.Z0_Number = 1;
			element1.Z0_VarCharMax = "Text 1";

			var element2 = dummy.Collection.AddNew();
			element2.Z0_Number = 2;
			element2.Z0_VarCharMax = "Text 2";

			var element3 = dummy.Collection.AddNew();
			element3.Z0_Number = 3;
			element3.Z0_VarCharMax = "Text 3";

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.SU_IsSystemDefined = ZBool.True;
			documentCommand.Parent = dummy;

			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<Z0_VarCharMax>]
{B}-[<Collection.Count>]
{B}-[<Collection[First].Z0_VarCharMax>]
{B}-[<Collection[1].Z0_VarCharMax>]
{B}-[<Collection[2].Z0_VarCharMax>]
{B}-[<Collection[3].Z0_VarCharMax>]
{B}-[<Collection[First].Format(""{Z0_Number}-{Z0_VarCharMax}"")>]
{B}-[<Collection.Total(""Z0_Number"")>]
{B}-[<Collection.Find({Z0_Number} == 1).Z0_VarCharMax>]
{B}-[<Collection.Find({Z0_Number} == 2).Z0_VarCharMax>]
{B}-[<Collection.Find({Z0_Number} == 3).Z0_VarCharMax>]
{B}-[<Collection.Find({Z0_Number} == 3).Format(""Number: {Z0_Number}    Text: {Z0_VarCharMax}"")>]
{B}-[<CollectionZ0_VarCharMax>]
{A}-[#EndOfReport]");

			var template = helper.CreateTemplate(factory, "Test");
			template.SO_DataContext = ".DummyBODocSupportable";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			AssertEquals("printJobs.Length", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);

				AssertMultilineASCIIEquals("",
@"{B}-[Text Parent]
{B}-[3]
{B}-[Text 1]
{B}-[Text 1]
{B}-[Text 2]
{B}-[Text 3]
{B}-[1-Text 1]
{B}-[6]
{B}-[Text 1]
{B}-[Text 2]
{B}-[Text 3]
{B}-[Number: 3    Text: Text 3]
{B}-[Text 1]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestSimpleProperty()
		{
			DoTest(typeof(DocAlpha), "FortyTwo",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("FortyTwo").GetGetMethod())
					});
		}

		[ExpectNoExceptions]
		public void TestPublicPropertyWithNoGetter()
		{
			DoTest(typeof(DocWithWeirdProperties), "IHaveNoGetter", null);
		}

		[ExpectNoExceptions]
		public void TestPublicPropertyWithNoPublicGetter()
		{
			DoTest(typeof(DocWithWeirdProperties), "IHaveNoPublicGetter", null);
		}

		public void TestNotCaseSensitive()
		{
			DoTest(typeof(DocAlpha), "fOrTyTwO",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("FortyTwo").GetGetMethod())
					});
		}

		public void TestReflectTypeWithNoPropertyIdentifier()
		{
			DoTest(typeof(DocBravo), "",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetMethod("ToString"))
					});
		}

		public void TestPropertyToString()
		{
			DoTest(typeof(DocAlpha), "Parent",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Parent").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocAlpha).GetMethod("ToString"))
					});
		}

		public void TestPropertyToStringOnEnumPropertyType()
		{
			DoTest(typeof(DocCharlie), "VisibleBecauseIAmAnEnum",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocCharlie).GetProperty("VisibleBecauseIamAnEnum").GetGetMethod()),
						new MethodInfoChainLink(typeof(System.DayOfWeek).GetMethod("ToString", Array.Empty<Type>()))
					});
		}

		public void TestPropertyToStringOnDifferentType()
		{
			DoTest(typeof(DocBravo), "Alpha",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Alpha").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocAlpha).GetMethod("ToString"))
					});
		}

		public void TestPropertyProperty()
		{
			DoTest(typeof(DocAlpha), "ParentSixTimesNine",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Parent").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("SixTimesNine").GetGetMethod())
					});
		}

		public void TestPropertyDotProperty()
		{
			DoTest(typeof(DocAlpha), "Parent.SixTimesNine",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Parent").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("SixTimesNine").GetGetMethod())
					});
		}

		public void TestNonExistantPropertyNameAtStart()
		{
			DoTest(typeof(DocAlpha), "garbage that does not exist", null);
		}

		public void TestNonExistantPropertyNameAtEnd()
		{
			DoTest(typeof(DocAlpha), "ParentSixTimesNineGARBAGE", null);
		}

		public void TestHiddenBecauseItsNotAZTypeOrBusinessObject()
		{
			DoTest(typeof(DocCharlie), "HiddenBecauseItsNotAZTypeOrBusinessObject", null);
		}

		public void TestHiddenBecauseOfAccessibilityProtected()
		{
			DoTest(typeof(DocCharlie), "HiddenBecauseOfAccessibilityProtected", null);
		}

		public void TestHiddenBecauseOfAccessibilityProtectedInternal()
		{
			DoTest(typeof(DocCharlie), "HiddenBecauseOfAccessibilityProtectedInternal", null);
		}

		public void TestHiddenBecauseOfAccessibilityPrivate()
		{
			DoTest(typeof(DocCharlie), "HiddenBecauseOfAccessibilityPrivate", null);
		}

		public void TestHiddenBecauseOfAccessibilityInternal()
		{
			DoTest(typeof(DocCharlie), "HiddenBecauseOfAccessibilityInternal", null);
		}

		public void TestAmbiguityIsResolvedInFavourOfTheLongestLeftmostIdentifier()
		{
			DoTest(typeof(DocDelta), "FooBarBaz",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocDelta).GetProperty("FooBar").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocFooBar).GetProperty("Baz").GetGetMethod())
					});
		}

		public void TestDotsOverrideAmbiguityInLongestCase()
		{
			DoTest(typeof(DocDelta), "FooBar.Baz",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocDelta).GetProperty("FooBar").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocFooBar).GetProperty("Baz").GetGetMethod())
					});
		}

		public void TestDotsOverrideAmbiguityInShortestCase()
		{
			DoTest(typeof(DocDelta), "Foo.BarBaz",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocDelta).GetProperty("Foo").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocFoo).GetProperty("BarBaz").GetGetMethod())
					});
		}

		public void TestAmbiguityOnFirstLevel()
		{
			DoTest(typeof(DocDelta), "FooNerf",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocDelta).GetProperty("FooNerf").GetGetMethod())
					});
		}

		public void TestCollection()
		{
			Assert("Test data is not set up correctly; this is not a proper BusinessObjectCollection", BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DocCharlieCollection)) == typeof(DocCharlie));
			DoTest(typeof(DocBravo), "Charlies",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod())
					});
		}

		public void TestPropertyOfElementInCollection()
		{
			Assert("Test data is not set up correctly; this is not a proper BusinessObjectCollection", BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DocCharlieCollection)) == typeof(DocCharlie));
			DoTest(typeof(DocBravo), "CharliesVisible",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlie).GetProperty("Visible").GetGetMethod())
					});
		}

		public void TestPropertyOfInterface()
		{
			Assert("Test data is not set up correctly; this is not a proper BusinessObjectCollection", BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DocCharlieCollection)) == typeof(DocCharlie));
			DoTest(typeof(DocBravo), "CharliesSomething",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(IDocZulu).GetProperty("Something").GetGetMethod())
					});
		}

		public void TestMoreThanOneCandidatePropertyOfInterfaceShouldBeSorted()
		{
			Assert("Test data is not set up correctly; this is not a proper BusinessObjectCollection", BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DocCharlieCollection)) == typeof(DocCharlie));
			DoTest(typeof(DocBravo), "CharliesSomethingElse",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(IDocZulu).GetProperty("SomethingElse").GetGetMethod())
					});
		}

		public void TestPropertyOfElementInCollectionStartingAtCollection()
		{
			Assert("Test data is not set up correctly; this is not a proper BusinessObjectCollection", BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DocCharlieCollection)) == typeof(DocCharlie));
			DoTest(typeof(DocCharlieCollection), "Visible",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlie).GetProperty("Visible").GetGetMethod())
					});
		}

		public void TestItemSimilarPropertyOfElementInCollection()
		{
			Assert("Test data is not set up correctly; this is not a proper BusinessObjectCollection", BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DocCharlieCollection)) == typeof(DocCharlie));
			DoTest(typeof(DocBravo), "Charlies.ItemPrice.FortyTwo",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlie).GetProperty("ItemPrice").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("FortyTwo").GetGetMethod())
					});
		}

		public void TestPropertyOfCollection()
		{
			DoTest(typeof(DocCharlieCollection), "OneTwoThree",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("OneTwoThree").GetGetMethod())
						});
		}

		public void TestPropertyReturningInterface()
		{
			DoTest(typeof(DocAlpha), "ThingSomething",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Thing").GetGetMethod()),
						new MethodInfoChainLink(typeof(IDocZulu).GetProperty("Something").GetGetMethod())
					});
		}

		public void TestPropertyReturningInterfaceWithTypeFromObjectFactoryAttributeAndNoRegistration()
		{
			DoTest(typeof(DocAlpha), "Thing2.SomethingElseEntirely",
				null);
		}

		public void TestPropertyReturningInterfaceWithTypeFromObjectFactoryAttributeAndRegistration()
		{
			using var toDispose = ObjectFactory.Substitute<IDocZulu>(new DocYankee());

			DoTest(typeof(DocAlpha), "Thing2.SomethingElseEntirely",
				new MethodInfoChainLink[] {
					new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Thing2").GetGetMethod()),
					new MethodInfoChainLink(typeof(DocYankee).GetProperty(nameof(DocYankee.SomethingElseEntirely)).GetGetMethod())
				});
		}

		public void TestUsingFormatOnDocumentWrapperObjectWithEmptyFormatString()
		{
			DoTest(typeof(DocAlpha), "Format(\"\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(FormatStringInterpreter).GetMethod("Format", new Type[] { typeof(BusinessObject), typeof(ZString) }), new object[] { ZString.Empty })
					});
		}

		public void TestUsingFormatOnDocumentWrapperObject()
		{
			DoTest(typeof(DocAlpha), "Format(\"{SixTimesNine} Plus\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(FormatStringInterpreter).GetMethod("Format", new Type[] { typeof(BusinessObject), typeof(ZString) }), new object[] { new ZString("{SixTimesNine} Plus") })
					});
		}

		public void TestUsingFormatOnRelatedObjectOfDocumentWrapperObject()
		{
			DoTest(typeof(DocAlpha), "Parent.Format(\"{SixTimesNine} Plus\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Parent").GetGetMethod()),
						new MethodInfoChainLink(typeof(FormatStringInterpreter).GetMethod("Format", new Type[] { typeof(BusinessObject), typeof(ZString) }), new object[] { new ZString("{SixTimesNine} Plus") })
					});
		}

		public void TestUsingFormatOnRelatedObjectOfOnRelatedObjectOfDocumentWrapperObject()
		{
			DoTest(typeof(DocAlpha), "Parent.Parent.Format(\"{SixTimesNine} Plus\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Parent").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocAlpha).GetProperty("Parent").GetGetMethod()),
						new MethodInfoChainLink(typeof(FormatStringInterpreter).GetMethod("Format", new Type[] { typeof(BusinessObject), typeof(ZString) }), new object[] { new ZString("{SixTimesNine} Plus") })
					});
		}

		public void TestUsingFormatOnChildCollectionWhenItsUsedAsTheDataSourceForABodySection()
		{
			DoTest(typeof(DocCharlieCollection), ".Format(\"{Visible} to form\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(FormatStringInterpreter).GetMethod("Format", new Type[] { typeof(BusinessObject), typeof(ZString) }), new object[] { new ZString("{Visible} to form") })
					});
		}

		public void TestPropertyOfElementInCollectionOfCollection()
		{
			Assert("Test data is not set up correctly; this is not a proper BusinessObjectCollection", BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DocCharlieCollection)) == typeof(DocCharlie));
			DoTest(typeof(DocBravo), "CharliesChildrenVisible",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlie).GetProperty("Children").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlie).GetProperty("Visible").GetGetMethod())
					});
		}

		public void TestUsingFormatOnChildDocDataCollectionWhenAccessedAsAProperty()
		{
			DoTest(typeof(DocBravo), "Charlies.Format(\"{Visible} to form\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Visible} to form"), ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero })
					});
		}

		public void TestUsingFormatOnChildDocDataCollectionWhenAccessedAsAPropertyWithDelimiterSpecified()
		{
			DoTest(typeof(DocBravo), "Charlies.Format(\"{Visible} to form\", NewLine)",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Visible} to form"), new ZString("NewLine"), ZString.Empty, ZString.Empty, ZInt.Zero })
					});
		}

		public void TestUsingFormatOnChildDocDataCollectionWhenAccessedAsAPropertyWithDelimiterSpecifiedThatWasNeverKnownToManOrGod()
		{
			DoTest(typeof(DocBravo), "Charlies.Format(\"{Visible} to form\", NeverHeardOfThisFrickinDelimiter)",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Visible} to form"), new ZString("NeverHeardOfThisFrickinDelimiter"), ZString.Empty, ZString.Empty, ZInt.Zero })
					});
		}

		public void TestUsingFormatOnChildIBusinessCollectionWhenAccessedAsAProperty()
		{
			DoTest(typeof(DocBravo), "Dummies.Format(\"{Visible} to form\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Dummies").GetGetMethod()),
						new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Visible} to form"), ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero })
					});
		}

		public void TestUsingFormatOnChildIBusinessCollectionWhenAccessedAsAPropertyWithDelimiterSpecified()
		{
			DoTest(typeof(DocBravo), "Dummies.Format(\"{Visible} to form\", NewLine)",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Dummies").GetGetMethod()),
						new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Visible} to form"), new ZString("NewLine"), ZString.Empty, ZString.Empty, ZInt.Zero })
					});
		}

		public void TestUsingFormatOnChildIBusinessCollectionWhenAccessedAsAPropertyWithDelimiterSpecifiedThatWasNeverKnownToManOrGod()
		{
			DoTest(typeof(DocBravo), "Dummies.Format(\"{Visible} to form\", NeverHeardOfThisFrickinDelimiter)",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Dummies").GetGetMethod()),
						new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Visible} to form"), new ZString("NeverHeardOfThisFrickinDelimiter"), ZString.Empty, ZString.Empty, ZInt.Zero })
					});
		}

		public void TestUsingFormatOnChildBusinessCollectionWhenPassedWhenGivenAGroupByArgument()
		{
			DoTest(typeof(DocBravo), "Dummies.Format(\"{Mode} x {Count}\", Comma, \"\", \"{Mode}\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Dummies").GetGetMethod()),
						new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Mode} x {Count}"), new ZString("Comma"), ZString.Empty, new ZString("{Mode}"), ZInt.Zero })
					});
		}

		public void TestCollectionIndexerOnItsOwnGetsAToStringOfAChild()
		{
			DoTest(typeof(DocBravo), "Charlies[First]",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod(), "First"),
						new MethodInfoChainLink(typeof(DocCharlie).GetMethod("ToString"))
					});
		}

		public void TestCollectionIndexerWithAPropertyOfAChild()
		{
			DoTest(typeof(DocBravo), "Charlies[First].Visible",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod(), "First"),
						new MethodInfoChainLink(typeof(DocCharlie).GetProperty("Visible").GetGetMethod())
					});
		}

		public void TestCollectionIndexerWithAFormatterFromTheChild()
		{
			DoTest(typeof(DocBravo), "Charlies[First].Format(\"{Visible}\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod(), "First"),
						new MethodInfoChainLink(typeof(FormatStringInterpreter).GetMethod("Format", new Type[] { typeof(BusinessObject), typeof(ZString) }), new object[] { new ZString("{Visible}") })
					});
		}

		public void TestUsingTotalOnChildCollectionWhenAccessedAsAProperty()
		{
			DoTest(typeof(DocBravo), "Charlies.Total(\"IntegerField\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Total", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString) }), new object[] { new ZString("IntegerField"), ZString.Empty, ZString.Empty })
					});
		}

		public void TestUsingTotalOnChildCollectionWhenAccessedAsAPropertyWithDelimiterSpecified()
		{
			DoTest(typeof(DocBravo), "Charlies.Total(\"IntegerField\", 1)",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Total", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString) }), new object[] { new ZString("IntegerField"), new ZString("1"), ZString.Empty })
					});
		}

		public void TestUsingTotalOnChildCollectionWhenAccessedAsAPropertyWithDelimiterAndFilterSpecified()
		{
			DoTest(typeof(DocBravo), "Charlies.Total(\"IntegerField\", 1, \"<Z0_VarCharMax>\" == \"Yes\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Total", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString) }), new object[] { new ZString("IntegerField"), new ZString("1"), "\"<Z0_VarCharMax>\" == \"Yes\"" })
					});
		}

		public void TestUsingDocDataValueWithOneParameter()
		{
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), ZString.Empty })
					});
		}

		public void TestUsingDocDataValueWithTwoParameters()
		{
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"{Visible}\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("{Visible}") })
					});
		}

		public void TestUsingDocDataValueWithTwoParametersAndRogueBrackets()
		{
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"(\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("(") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \")\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString(")") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"\\(\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("\\(") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"\\)\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("\\)") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"(()())()((\\(\\)\\(\\(\\))\\((\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("(()())()((\\(\\)\\(\\(\\))\\((") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"\\A\\B\\C\\\")",
				new MethodInfoChainLink[] {
					new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("\\A\\B\\C\\") })
				});
		}

		public void TestUsingDocDataValueWithTwoParametersAndRogueQuotationMarks()
		{
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"\"\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("\"") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"\"\"\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("\"\"") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", \"\\\"\")",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("\\\"") })
					});
			DoTest(typeof(DocBravo), "DocDataValue(\"Jerry Springer\", a\"b\"c\\\"d\\\"e\"f\\\"g\"h\\\"i\\\"j\"k\"l)",
		new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { new ZString("Jerry Springer"), new ZString("a\"b\"c\\\"d\\\"e\"f\\\"g\"h\\\"i\\\"j\"k\"l") })
					});
		}

		public void TestNewPropertyTakesPrecedenceOverParentClass()
		{
			DoTest(typeof(DocZChildMaster), "SomeProperty.Goodbye",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocZChildMaster).GetProperty("SomeProperty", typeof(DocZChildDetail)).GetGetMethod()),
						new MethodInfoChainLink(typeof(DocZChildDetail).GetProperty("Goodbye").GetGetMethod())
					});
		}

		public void TestNewPropertyTakesPrecedenceOverParentClassEvenWhenGetPropertiesResultsAreReversed()
		{
			BusinessObjectReflectorType = typeof(BusinessObjectReflectorWithGetPropertiesResultsReversed);
			TestNewPropertyTakesPrecedenceOverParentClass();
		}

		public void TestUsingCountOnChildDocDataCollection()
		{
			DoTest(typeof(DocBravo), "Charlies.Count",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Charlies").GetGetMethod()),
						new MethodInfoChainLink(typeof(DocCharlieCollection).GetProperty("Count", typeof(int)).GetGetMethod())
					});
		}

		public void TestUsingCountOnChildIBusinessCollection()
		{
			DoTest(typeof(DocBravo), "Dummies.Count",
					new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocBravo).GetProperty("Dummies").GetGetMethod()),
						new MethodInfoChainLink(typeof(DummyBusinessObjectCollection).GetProperty("Count", typeof(int)).GetGetMethod())
					});
		}

		public void TestCollectionIndexerZStringArrayReflectType()
		{
			DoTest(typeof(DocFoo), "NerfsFamily[1]", new MethodInfoChainLink[] {
						new MethodInfoChainLink(typeof(DocFoo).GetProperty("NerfsFamily").GetGetMethod(), "1") });
		}

		public void TestIAllowMacroAccessToAllPublicPropertiesInterface()
		{
			var obj = new ClassWithAccessablePropertiesNotOnInterface("Property", "NestedProperty");
			var objInterface = obj as IAllowMacroAccessToAllPublicProperties;

			var propertyName = nameof(ClassWithAccessablePropertiesNotOnInterface.MyProperty);
			var methodInfoChainLink = new BusinessObjectReflector().GetMethodInfoChain(typeof(IAllowMacroAccessToAllPublicProperties), objInterface, propertyName);
			AssertEquals(1, methodInfoChainLink?.Length);
			var value = methodInfoChainLink[0].ReflectOutObject(objInterface, objInterface);
			AssertEquals("Property", value);
		}

		public void TestIAllowMacroAccessToAllPublicPropertiesInterface_NestedProperty()
		{
			var obj = new ClassWithAccessablePropertiesNotOnInterface("Property", "NestedProperty");
			var objInterface = obj as IAllowMacroAccessToAllPublicProperties;

			var propertyName = "NestedPreperty.MyProperty";
			var methodInfoChainLink = new BusinessObjectReflector().GetMethodInfoChain(typeof(IAllowMacroAccessToAllPublicProperties), objInterface, propertyName);
			AssertEquals(2, methodInfoChainLink?.Length);

			object value = objInterface;
			foreach (var methodInfo in methodInfoChainLink)
			{
				value = methodInfo.ReflectOutObject(value, objInterface);
			}
			AssertEquals("NestedProperty", value);
		}

		#region Implementation

		internal class DummyPasswordStoredBODocSupportable : DummyBODocSupportable, IPasswordStored
		{
			public DummyPasswordStoredBODocSupportable(BusinessObjectFactory factory, DataRow dataRow) : base(factory, dataRow)
			{
			}

			public int PasswordHashIterations => throw new NotImplementedException();

			ZBlob IPasswordStored.PasswordSalt => Dummy_PasswordSalt;

			ZBlob IPasswordStored.PasswordHash => Dummy_PasswordHash;

			[DocumentMacroIgnore]
			public ZBlob Dummy_PasswordSalt { get; set; }

			[DocumentMacroIgnore]
			public ZBlob Dummy_PasswordHash { get; set; }

			public bool VerifyPassword(WTG.Foundation.Cryptography.UserSecrets.IUserSecretsContext userSecretsContext, string password)
			{
				throw new NotImplementedException();
			}
		}

		class BusinessObjectReflectorWithGetPropertiesResultsReversed : BusinessObjectReflector
		{
			protected override void DoSomethingToPropertiesArrayForTesting(PropertyInfo[] properties)
			{
				Array.Reverse(properties);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BusinessObjectReflectorType = typeof(BusinessObjectReflector);
			BusinessObjectReflector.ClearCacheForTesting();
		}
		Type BusinessObjectReflectorType;

		void DoTest(Type typeToReflect, string propertyIdentifier, MethodInfoChainLink[] expectedInfos)
		{
			var reflector = (BusinessObjectReflector)Activator.CreateInstance(BusinessObjectReflectorType);
			MethodInfoChainLink[] actualInfos = reflector.GetMethodInfoChain(typeToReflect, null, propertyIdentifier);

			if (expectedInfos == null)
			{
				AssertEquals("Should not have found a chain", null, actualInfos);
			}
			else
			{
				AssertNotNull("MethodInfos should not be null (Expected " + expectedInfos.Length.ToString() + " MethodInfos)", actualInfos);
				AssertEquals("Number of MethodInfos in Chain", expectedInfos.Length, actualInfos.Length);
				for (int i = 0; i < actualInfos.Length; i++)
				{
					MethodInfoChainLink expectedInfo = expectedInfos[i];
					MethodInfoChainLink actualInfo = actualInfos[i];
					AssertNotNull("actualInfo.MethodInfo should never be null at MethodInfoChainLink " + i, actualInfo.MethodInfo);
					AssertNotNull("expectedInfo should never be null either. Check at MethodInfoChainLink " + i, expectedInfo.MethodInfo);
					AssertEquals(".MethodInfo.ReflectedType should be the same at MethodInfoChainLink " + i, expectedInfo.MethodInfo.ReflectedType, actualInfo.MethodInfo.ReflectedType);
					AssertEquals(".MethodInfo should be the same at MethodInfoChainLink " + i, expectedInfo.MethodInfo, actualInfo.MethodInfo);
					AssertEquals(".Index should be the same at MethodInfoChainLink " + i, expectedInfo.Index, actualInfo.Index);
					object[] expectedParameters = expectedInfo.Parameters;
					object[] actualParameters = actualInfo.Parameters;
					AssertEquals(".Parameters == null should be the same at MethodInfoChainLink " + i, expectedParameters == null, actualParameters == null);
					if (expectedParameters != null)
					{
						AssertEquals(".Parameters.Length should be the same at MethodInfoChainLink " + i, expectedParameters.Length, actualParameters.Length);

						for (int parameterIndex = 0; parameterIndex < expectedParameters.Length; parameterIndex++)
						{
							object expectedParameter = expectedParameters[parameterIndex];
							object actualParameter = actualParameters[parameterIndex];
							AssertEquals("Parameter should be the same at MethodInfoChainLink " + i + ", Parameter Number " + parameterIndex, expectedParameter, actualParameter);
						}
					}
				}
			}
		}

		#region Dummy docwrappers that will be reflected upon

		interface IDocZulu
		{
			ZString Something { get; }
			ZString SomethingElse { get; }
		}

		class DocAlpha : DocumentWrapper
		{
			public DocAlpha()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public ZInt FortyTwo
			{
				get { return 42; }
			}

			public bool IsExport
			{
				get { return true; }
			}

			public ZString SixTimesNine
			{
				get { return "Fifty four"; }
			}

			public DocAlpha Parent
			{
				get { return new DocAlpha(); }
			}

			public IDocZulu Thing
			{
				get { return new DocYankee(); }
			}

			[ResolveTypeFromObjectFactoryForDocData]
			public IDocZulu Thing2
			{
				get { return new DocYankee(); }
			}

			public override string ToString()
			{
				return "Alpha";
			}
		}

		class DocBravo : DocumentWrapper
		{
			public DocBravo()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return "Bravo";
			}

			public ZString Orange
			{
				get { return "A citrus fruit"; }
			}

			public DocBravoCollection Children
			{
				get
				{
					var result = new DocBravoCollection(Factory);
					result.AddNew();
					result.AddNew();
					return result;
				}
			}

			public DocCharlieCollection Charlies
			{
				get { return new DocCharlieCollection(Factory); }
			}

			public DocAlpha Alpha
			{
				get { return new DocAlpha(); }
			}

			public DummyBusinessObjectCollection Dummies
			{
				get
				{
					var collection = new DummyBusinessObjectCollection(Factory);
					return collection;
				}
			}

			public bool IsExport
			{
				get { return true; }
			}
		}

		class DocCharlie : DocumentWrapper, IDocZulu
		{
			public DocCharlie()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{
				this.children = new DocCharlieCollection(Factory);
			}

			readonly DocCharlieCollection children;

			public override string ToString()
			{
				return "Charlie";
			}

			public DocCharlieCollection Children
			{
				get { return children; }
			}

			public ZString Visible
			{
				get { return "got me"; }
			}

			public ZInt IntegerField
			{
				get { return 34; }
			}

			public DocAlpha ItemPrice
			{
				get { return new DocAlpha(); }
			}

			public string HiddenBecauseItsNotAZTypeOrBusinessObject
			{
				get { return "shouldn't get me"; }
			}

			public DayOfWeek VisibleBecauseIamAnEnum
			{
				get { return DayOfWeek.Friday; }
			}

			protected ZString HiddenBecauseOfAccessibilityProtected
			{
				get { return "shouldn't get me"; }
			}

			protected internal ZString HiddenBecauseOfAccessibilityProtectedInternal
			{
				get { return "shouldn't get me"; }
			}

			internal ZString HiddenBecauseOfAccessibilityInternal
			{
				get { return "shouldn't get me"; }
			}

			ZString IDocZulu.Something
			{
				get { return "Charlie implementing Zulu"; }
			}

			ZString IDocZulu.SomethingElse
			{
				get { return "Charlie implementing Zulu else"; }
			}
		}

		class DocBravoCollection : DocumentWrapperCollection
		{
			public DocBravoCollection(BusinessObjectFactory factory)
				: base(factory)
			{ }

			public new DocBravo this[int index]
			{
				get { return (DocBravo)base[index]; }
			}
		}

		class DocCharlieCollection : DocumentWrapperCollection
		{
			public DocCharlieCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new DocCharlie this[int index]
			{
				get { return this[index]; }
			}

			public new DocCharlie this[string index]
			{
				get { return this[index]; }
			}

			public ZInt OneTwoThree
			{
				get { return 123; }
			}
		}

		class DocFooBar : DocumentWrapper
		{
			public DocFooBar()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return "Foo bar";
			}

			public ZString Baz
			{
				get { return "Baz"; }
			}
		}

		class DocFoo : DocumentWrapper
		{
			public DocFoo()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return "Foo";
			}

			public ZString BarBaz
			{
				get { return "Bar Baz"; }
			}

			public ZString Nerf
			{
				get { return "Foo's nerf"; }
			}

			public ZString[] NerfsFamily => new ZString[] { "1", "2" };

			public ZPropertyInfo NerfsFamilyInfo => GetZPropertyInfo(nameof(NerfsFamily));
		}

		class ConcreteNonPersistentBusinessObject : NonPersistentBusinessObject
		{ }

		class DocDelta : DocumentWrapper
		{
			public DocDelta()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return "Delta";
			}

			public DocFoo Foo
			{
				get { return new DocFoo(); }
			}

			public DocFooBar FooBar
			{
				get { return new DocFooBar(); }
			}

			public ZString FooNerf
			{
				get { return "Delta's foo nerf"; }
			}
		}

		class DocYankee : DocumentWrapper, IDocZulu
		{
			public DocYankee()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return "Yankee";
			}

			public ZString Something
			{
				get { return "Yankee implementing Zulu"; }
			}

			public ZString SomethingElse
			{
				get { return "Yankee implementing Zulu else"; }
			}

			public ZString SomethingElseEntirely
			{
				get { return "Yankee's own"; }
			}
		}

		class DocParentMaster : DocumentWrapper
		{
			public DocParentMaster()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return "DocParentMaster";
			}

			public DocParentDetail SomeProperty
			{
				get { return null; }
			}
		}

		class DocParentDetail : DocumentWrapper
		{
			public DocParentDetail()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return "DocParentDetail";
			}

			public ZString Hello
			{
				get { return "Hello"; }
			}
		}

		class DocZChildMaster : DocParentMaster
		{
			public new DocZChildDetail SomeProperty
			{
				get { return null; }
			}
		}

		class DocZChildDetail : DocParentDetail
		{
			public ZString Goodbye
			{
				get { return "Goodbye"; }
			}
		}

		class DocWithWeirdProperties : DocumentWrapper
		{
			public ZString IHaveNoGetter
			{
				set { _ = value; }
			}

			public ZString IHaveNoPublicGetter
			{
				private get { return "blah"; }
				set { _ = value; }
			}
		}

		class ClassWithAccessablePropertiesNotOnInterface : IAllowMacroAccessToAllPublicProperties
		{
			public ClassWithAccessablePropertiesNotOnInterface(ZString propertyValue, ZString nestedPropertyValue)
			{
				MyProperty = propertyValue;
				NestedPreperty = new ClassWithAccessablePropertiesNotOnInterface(nestedPropertyValue);
			}

			ClassWithAccessablePropertiesNotOnInterface(ZString nestedPropertyValue)
			{
				MyProperty = nestedPropertyValue;
			}

			public ZString MyProperty { get; }

			public IAllowMacroAccessToAllPublicProperties NestedPreperty { get; }
		}

		#endregion

		#endregion
	}
}
