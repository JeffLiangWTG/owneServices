using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationElement))]
	sealed class BillOfLadingNumberCustomisationElementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMatches(NumberCustomisationElementCategories categories)
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key", "name", "description", NumberCustomisationElementCategories.Standard, 3, null));

			AssertEquals("Standard", true, element.Matches(NumberCustomisationElementCategories.Standard));
			AssertEquals("Liner & Agency", false, element.Matches(NumberCustomisationElementCategories.LinerAgency));
			AssertEquals("Standard|LinerAgency", true, element.Matches(NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency));
		}

		public void TestChangingOrderRelatedElementOrders()
		{
			const string errorText = "The Order has been duplicated and must be unique.";

			var customisation = new BillOfLadingNumberCustomisation();

			var element1 = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode];
			element1.Include = true;
			element1.Order = 1;

			var element2 = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode];
			element2.Include = true;
			element2.Order = 2;

			var element3 = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode];
			element3.Include = true;
			element3.Order = 3;

			AssertNoNotifications("precondition:", element1);
			AssertNoNotifications("precondition:", element2);
			AssertNoNotifications("precondition:", element3);

			element2.Order = 1;
			AssertHasError("has conflict", element1.OrderInfo, errorText);
			AssertHasError("has conflict", element2.OrderInfo, errorText);
			AssertNoNotifications("no conflict", element3.OrderInfo);

			element2.Order = 3;
			AssertNoNotifications("no conflict", element1.OrderInfo);
			AssertHasError("has conflict", element2.OrderInfo, errorText);
			AssertHasError("has conflict", element3.OrderInfo, errorText);

			element2.Order = 2;
			AssertNoNotifications("no conflict", element1.OrderInfo);
			AssertNoNotifications("no conflict", element2.OrderInfo);
			AssertNoNotifications("no conflict", element3.OrderInfo);
		}

		public void TestSerialisation()
		{
			var strategy = new CommonElementStrategy("blah", "", "", NumberCustomisationElementCategories.Standard, 5, null);

			BillOfLadingNumberCustomisation customisation1 = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element1 = new BillOfLadingNumberCustomisationElement(customisation1, strategy);

			element1.Include = true;
			element1.Order = 4;
			element1.Detail = "X";
			element1.Fountain = true;
			Assert("SetDefaultValues: CheckDigit should be true", element1.CheckDigit);
			element1.CheckDigit = false;

			string xml;

			using (System.IO.StringWriter stream = new System.IO.StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				element1.WriteXml(writer);
				writer.Flush();
				xml = stream.ToString();
			}

			const string expectedXml =
				"<Element key=\"blah\">" +
					"<Order>4</Order>" +
					"<Fountain>Y</Fountain>" +
					"<CheckDigit>N</CheckDigit>" +
					"<Detail>X</Detail>" +
				"</Element>" +
				"";

			AssertMultilineASCIIEquals("", expectedXml, xml);

			BillOfLadingNumberCustomisation customisation2 = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element2 = new BillOfLadingNumberCustomisationElement(customisation2, strategy);

			using (System.IO.StringReader stream = new System.IO.StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				element2.ReadXml(reader);
			}

			AssertEquals("Order", element1.Order, element2.Order);
			AssertEquals("Include", element1.Include, element2.Include);
			AssertEquals("Detail", element1.Detail, element2.Detail);
			AssertEquals("Fountain", element1.Fountain, element2.Fountain);
			AssertEquals("CheckDigit", element1.CheckDigit, element2.CheckDigit);
		}

		public void TestCopyValuesFrom()
		{
			var strategy = new CommonElementStrategy("blah", "", "", NumberCustomisationElementCategories.Standard, 5, null);

			BillOfLadingNumberCustomisation customisation1 = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element1 = new BillOfLadingNumberCustomisationElement(customisation1, strategy);

			element1.Include = true;
			element1.Order = 4;
			element1.Detail = "X";
			element1.Fountain = true;
			Assert("SetDefaultValues: CheckDigit should be true", element1.CheckDigit);
			element1.CheckDigit = false;

			BillOfLadingNumberCustomisationElement element2 = new BillOfLadingNumberCustomisationElement(customisation1, strategy);
			element2.CopyValuesFrom(element1);
			AssertEquals("Order", element1.Order, element2.Order);
			AssertEquals("Include", element1.Include, element2.Include);
			AssertEquals("Detail", element1.Detail, element2.Detail);
			AssertEquals("Fountain", element1.Fountain, element2.Fountain);
			AssertEquals("CheckDigit", element1.CheckDigit, element2.CheckDigit);
		}

		public void TestDetailSet_ShouldSetFountainToFalse_WhenDetailIsMacro()
		{
			AssertDetailEffectOnFountain(new ClientCodedElementStrategy("blah"), true, true);
			AssertDetailEffectOnFountain(new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded1), false, true);
			AssertDetailEffectOnFountain(new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded2), false, true);
			AssertDetailEffectOnFountain(new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded2), false, true);
		}

		void AssertDetailEffectOnFountain(ClientCodedElementStrategy strategy, bool resultForMacro, bool resultForNonMaco)
		{
			var customisation = new BillOfLadingNumberCustomisation();
			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);

			element.Include = true;
			element.Order = 4;
			element.Detail = "X";
			element.Fountain = true;

			element.Detail = "<XYZ>";
			Assert($"Fountain should be {resultForMacro}", element.Fountain.Equals(resultForMacro));
			Assert($"Fountain Readonly should be !{resultForMacro}", !element.FountainInfo.ReadOnly.Equals(resultForMacro));

			element.Fountain = true;
			element.Detail = "XYZ>P<";
			Assert($"Fountain should be {resultForNonMaco}", element.Fountain.Equals(resultForNonMaco));
			Assert($"Fountain ReadOnly should be !{resultForNonMaco}", !element.FountainInfo.ReadOnly.Equals(resultForNonMaco));

			element.Fountain = true;
			element.Detail = "XYZ>P";
			Assert($"Fountain should be {resultForNonMaco}", element.Fountain.Equals(resultForNonMaco));
			Assert($"Fountain ReadOnly should be !{resultForNonMaco}", !element.FountainInfo.ReadOnly.Equals(resultForNonMaco));

			element.Fountain = true;
			element.Detail = "XYZP<";
			Assert($"Fountain should be {resultForNonMaco}", element.Fountain.Equals(resultForNonMaco));
			Assert($"Fountain ReadOnly should be !{resultForNonMaco}", !element.FountainInfo.ReadOnly.Equals(resultForNonMaco));
		}

		public void TestDecimalPlacesForBinding()
		{
			var comStrategy = new CommonElementStrategy("blah", "", "", NumberCustomisationElementCategories.Standard, 5, null);
			var intStrategy = new IntStrategyForTest();
			var decStrategy = new DecimalStrategyForTest();
			var byteStrategy = new ByteStrategyForTest();

			var customisation = new BillOfLadingNumberCustomisation();

			var comElement = new BillOfLadingNumberCustomisationElement(customisation, comStrategy);
			var intElement = new BillOfLadingNumberCustomisationElement(customisation, intStrategy);
			var decElement = new BillOfLadingNumberCustomisationElement(customisation, decStrategy);
			var byteElement = new BillOfLadingNumberCustomisationElement(customisation, byteStrategy);

			AssertEquals(2, comElement.DecimalPlacesForBinding);
			AssertEquals(0, intElement.DecimalPlacesForBinding);
			AssertEquals(6, decElement.DecimalPlacesForBinding);
			AssertEquals(0, byteElement.DecimalPlacesForBinding);
		}

		public void TestOrder_ReadOnly()
		{
			var comStrategy = new CommonElementStrategy("blah", "", "", NumberCustomisationElementCategories.Standard, 5, null);
			var customisation = new BillOfLadingNumberCustomisation();
			var comElement = new BillOfLadingNumberCustomisationElement(customisation, comStrategy);
			AssertEquals(false, comElement.Strategy.OrderReadOnly);
			comElement.OverrideStrategy(new ReadOnlyStrategyForTest());
			AssertEquals(true, comElement.Strategy.OrderReadOnly);
		}

		public void TestInclude_ReadOnly()
		{
			var comStrategy = new CommonElementStrategy("blah", "", "", NumberCustomisationElementCategories.Standard, 5, null);
			var customisation = new BillOfLadingNumberCustomisation();
			var comElement = new BillOfLadingNumberCustomisationElement(customisation, comStrategy);
			AssertEquals(false, comElement.Strategy.IncludeReadOnly);
			comElement.OverrideStrategy(new ReadOnlyStrategyForTest());
			AssertEquals(true, comElement.Strategy.IncludeReadOnly);
		}

		public void TestCalcMaxGeneratedLength()
		{
			var mockStrategy = new Mock<IElementStrategy>();
			var customisation = new BillOfLadingNumberCustomisation();
			var element = new BillOfLadingNumberCustomisationElement(customisation, mockStrategy.Object);
			mockStrategy.Setup(x => x.CalcMaxGeneratedLength(element)).Returns(20250221);

			var elementAsCalcMaxGeneratedLength = element as ICalcMaxGeneratedLength;
			AssertNotNull(elementAsCalcMaxGeneratedLength);

			AssertEquals(20250221, elementAsCalcMaxGeneratedLength.CalcMaxGeneratedLength);
			mockStrategy.Verify(x => x.CalcMaxGeneratedLength(It.IsAny<BillOfLadingNumberCustomisationElement>()), Times.Exactly(1));
		}

		public void TestRegExForDataType()
		{
			var mockStrategy = new Mock<IElementStrategy>();
			var customisation = new BillOfLadingNumberCustomisation();
			var element = new BillOfLadingNumberCustomisationElement(customisation, mockStrategy.Object);
			mockStrategy.Setup(x => x.GetRegExForDataType(element)).Returns("Dummy Regex String");

			AssertEquals("Dummy Regex String", element.RegExForDataType);
			mockStrategy.Verify(x => x.GetRegExForDataType(It.IsAny<BillOfLadingNumberCustomisationElement>()), Times.Exactly(1));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var strategy = new CommonElementStrategy("key", "name", "", NumberCustomisationElementCategories.Standard, 1, null);

			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			customisation.UnFilteredElements.RemoveAll();
			customisation.UnFilteredElements.Add(new BillOfLadingNumberCustomisationElement(customisation, strategy));

			return customisation.UnFilteredElements[0];
		}

		class StrategyForTest : ElementStrategy
		{
			public override string Key
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return BillOfLadingNumberCustomisationElement.Keys.SequenceNumber; }
			}
			public override string Name
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return "Sequence Number"; }
			}
			public override bool Force
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return true; }
			}
			public override byte DefaultOrder
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return 50; }
			}

			public override bool UseDetail
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return true; }
			}
			public override string DefaultDetail => "8";

			public override int DetailMaxLength => 3;

			public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
			{
				return ZInt.ParseSafe(parent.Detail, 8);
			}

			public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
			{
				throw new System.NotImplementedException();
			}
		}

		class ReadOnlyStrategyForTest : ElementStrategy
		{
			public override bool OrderReadOnly => true;

			public override bool IncludeReadOnly => true;

			public override string Key => "key";

			public override string Name => "name";

			public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
			{
				return 1;
			}

			public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
			{
				throw new System.NotImplementedException();
			}
		}

		class IntStrategyForTest : StrategyForTest
		{
			public override FieldType DetailFieldType
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return FieldType.Integer; }
			}
		}

		class DecimalStrategyForTest : StrategyForTest
		{
			public override FieldType DetailFieldType
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return FieldType.Decimal; }
			}
		}

		class ByteStrategyForTest : StrategyForTest
		{
			public override FieldType DetailFieldType
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return FieldType.Byte; }
			}
		}

		#endregion
	}
}
