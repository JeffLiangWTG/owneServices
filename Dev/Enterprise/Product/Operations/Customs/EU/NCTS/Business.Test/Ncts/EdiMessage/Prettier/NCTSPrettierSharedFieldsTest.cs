using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierSharedFieldsTest : TestCase
	{
		public void TestConstructor()
			=> AssertContainsExactElementsInAnyOrder(
				expected: new [] { ("Key1", "Value1"), ("Key2", "Value2") },
				actual: new NCTSPrettierSharedFields(new[] { ("Key1", "Value1"), ("Key2", "Value2") }));

		public void TestMessageTypeFieldName() => AssertEquals("Message Type", sharedFields.MessageTypeFieldName);
		public void TestMessageType() => TestProperty(x => x.MessageType, x => x.MessageTypeFieldName);

		public void TestMessageRecipientFieldName() => AssertEquals("Message Recipient", sharedFields.MessageRecipientFieldName);
		public void TestMessageRecipientType() => TestProperty(x => x.MessageRecipient, x => x.MessageRecipientFieldName);

		public void TestDeclarationTypeFieldName() => AssertEquals("Declaration Type", sharedFields.DeclarationTypeFieldName);
		public void TestDeclarationType() => TestProperty(x => x.DeclarationType, x => x.DeclarationTypeFieldName);

		public void TestAdditionalDeclarationTypeFieldName() => AssertEquals("Additional Declaration Type", sharedFields.AdditionalDeclarationTypeFieldName);
		public void TestAdditionalDeclarationType() => TestProperty(x => x.AdditionalDeclarationType, x => x.AdditionalDeclarationTypeFieldName);

		public void TestLRNFieldName() => AssertEquals("LRN (Local Reference Number)", sharedFields.LRNFieldName);
		public void TestLRN() => TestProperty(x => x.LRN, x => x.LRNFieldName);

		public void TestMRNFieldName() => AssertEquals("MRN (Movement Reference Number)", sharedFields.MRNFieldName);
		public void TestMRN() => TestProperty(x => x.MRN, x => x.MRNFieldName);

		public void TestCustomsOfficeOfDepartureFieldName() => AssertEquals("Customs Office (Departure)", sharedFields.CustomsOfficeOfDepartureFieldName);
		public void TestCustomsOfficeOfDeparture() => TestProperty(x => x.CustomsOfficeOfDeparture, x => x.CustomsOfficeOfDepartureFieldName);

		public void TestCustomsOfficeOfDestinationFieldName() => AssertEquals("Customs Office (Destination)", sharedFields.CustomsOfficeOfDestinationFieldName);
		public void TestCustomsOfficeOfDestination() => TestProperty(x => x.CustomsOfficeOfDestination, x => x.CustomsOfficeOfDestinationFieldName);

		public void TestReducedDatasetIndicatorFieldName() => AssertEquals("Reduced Dataset", sharedFields.ReducedDatasetIndicatorFieldName);
		public void TestReducedDatasetIndicator() => TestProperty(x => x.ReducedDatasetIndicator, x => x.ReducedDatasetIndicatorFieldName);

		public void TestSimplifiedProcedureFieldName() => AssertEquals("Simplified Procedure", sharedFields.SimplifiedProcedureFieldName);
		public void TestSimplifiedProcedure() => TestProperty(x => x.SimplifiedProcedure, x => x.SimplifiedProcedureFieldName);

		public void TestSecurityFieldName() => AssertEquals("Security", sharedFields.SecurityFieldName);
		public void TestSecurity() => TestProperty(x => x.Security, x => x.SecurityFieldName);

		public void TestBindingItineraryFieldName() => AssertEquals("Binding Itinerary", sharedFields.BindingItineraryFieldName);
		public void TestBindingItinerary() => TestProperty(x => x.BindingItinerary, x => x.BindingItineraryFieldName);

		public void TestPrincipleEORIFieldName() => AssertEquals("Principle EORI", sharedFields.PrincipleEORIFieldName);
		public void TestPrincipleEORI() => TestProperty(x => x.PrincipleEORI, x => x.PrincipleEORIFieldName);

		public void TestRepresentativeEORIFieldName() => AssertEquals("Representative EORI", sharedFields.RepresentativeEORIFieldName);
		public void TestRepresentativeEORI() => TestProperty(x => x.RepresentativeEORI, x => x.RepresentativeEORIFieldName);

		public void TestConsigneeEORIFieldName() => AssertEquals("Consignee EORI", sharedFields.ConsigneeEORIFieldName);
		public void TestConsigneeEORI() => TestProperty(x => x.ConsigneeEORI, x => x.ConsigneeEORIFieldName);

		public void TestConsignorEORIFieldName() => AssertEquals("Consignor EORI", sharedFields.ConsignorEORIFieldName);
		public void TestConsignorEORI() => TestProperty(x => x.ConsignorEORI, x => x.ConsignorEORIFieldName);

		public void TestThis()
		{
			sharedFields["TestField"] = "Test";
			Assert("contains added value", sharedFields.Contains(("TestField", "Test")));
			AssertEquals("Added value can be read", "Test", sharedFields["TestField"]);
			sharedFields["TestField"] = string.Empty;
			Assert("contains added value", !sharedFields.Contains(("TestField", "Test")));
		}

		public void TestCount()
		{
			sharedFields["TestField1"] = "Test1";
			sharedFields["TestField2"] = "Test2";
			AssertEquals(2, sharedFields.Count);
		}

		void TestProperty(
			Expression<Func<NCTSPrettierSharedFields, string>> propertyExpression,
			Expression<Func<NCTSPrettierSharedFields, string>> fieldNamePropertyExpression)
			=> CombineAssertions(() =>
		{
			var propertyInfo = (propertyExpression?.Body as MemberExpression)?.Member as PropertyInfo;
			propertyInfo.SetValue(sharedFields, "Test");
			var fieldNameProperty = (fieldNamePropertyExpression?.Body as MemberExpression)?.Member as PropertyInfo;
			var fieldName = (string)fieldNameProperty.GetValue(sharedFields);
			Assert("contains added value", sharedFields.Contains((fieldName, "Test")));
			AssertEquals("Added value can be read", "Test", (string)propertyInfo.GetValue(sharedFields));
			propertyInfo.SetValue(sharedFields, string.Empty);
			Assert("contains added value", !sharedFields.Contains((fieldName, "Test")));
		});

		protected override void SetUp()
		{
			base.SetUp();
			sharedFields = new ();
		}

		NCTSPrettierSharedFields sharedFields;
	}
}
