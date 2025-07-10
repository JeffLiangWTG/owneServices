using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SuppressField))]
	sealed class SuppressFieldTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<SuppressField>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(SomeField)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (AnotherField)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(ETA)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (ETA)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(ETD)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (ETD)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(ATA)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (ATA)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(ATD)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (ATD)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(FlightNumber)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (FlightNumber)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(TransportInfo)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (TransportInfo)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(Carrier)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (Carrier)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(DeclarationExportDate)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (DeclarationExportDate)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(DeclarationDateAtOrigin)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (DeclarationDateAtOrigin)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(DeclarationFolio)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (DeclarationFolio)  >", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<SuppressField(MasterBill)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< SuppressField (MasterBill)  >", Passes.FirstPass));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacement()
		{
			DummyDocDataProvider dummySource = new DummyDocDataProvider();

			using (Report report = new Report(new DocumentPack(), ExcelTemplate, dummySource, "Test_" + ExcelTemplate.TemplateName, ContactType.Consignor, null, DocumentDirection.ANY, false))
			{
				PrepareRenderer(report);
				AssertEquals("", ValueProviderToTest.GetReplacement("<SuppressField(bla)>", report));
				AssertEquals("false", ValueProviderToTest.GetReplacement("<SuppressField(MasterBill)>", report));
				AssertEquals("", ValueProviderToTest.GetReplacement("<SuppressField(MasterBill, ETD)>", report));
				AssertEquals("", ValueProviderToTest.GetReplacement("<SuppressField()>", report));
				AssertEquals("dummySource should receive correct ContactType at this point to use it in Suppression later", ContactType.Consignor, dummySource.LastContactTypePassed);
			}

			dummySource.SuppressionReturnValue = true;

			using (Report report = new Report(new DocumentPack(), ExcelTemplate, dummySource, "Test_" + ExcelTemplate.TemplateName, ContactType.Consignee, null, DocumentDirection.ANY, false))
			{
				PrepareRenderer(report);
				AssertEquals("", ValueProviderToTest.GetReplacement("<SuppressField(bla)>", report));
				AssertEquals("true", ValueProviderToTest.GetReplacement("<SuppressField(MasterBill)>", report));
				AssertEquals("", ValueProviderToTest.GetReplacement("<SuppressField(MasterBill, ETD)>", report));
				AssertEquals("", ValueProviderToTest.GetReplacement("<SuppressField()>", report));
				AssertEquals("dummySource should receive correct ContactType at this point to use it in Suppression later", ContactType.Consignee, dummySource.LastContactTypePassed);
			}
		}

		protected override ValueProvider GetNewValueProvider() => new SuppressField();

		protected override void PrepareDataForExamplesEvaluate()
		{
			var dummySource = new DummyDocDataProvider();
			dummySource.SuppressionReturnValue = true;
			Report = new Report(new DocumentPack(), ExcelTemplate, dummySource, "Test_" + ExcelTemplate.TemplateName, ContactType.Consignee, null, DocumentDirection.ANY, false);
			PrepareRenderer();
		}

		sealed class DummyDocDataProvider : IControlFlightDetailsSuppression, IBODocDataProvider
		{
			public bool ShouldSuppressFlightDetails(SuppressFields fieldType, ContactType contactType)
			{
				LastContactTypePassed = contactType;
				return SuppressionReturnValue;
			}

			public ContactType LastContactTypePassed { get; set; }

			public bool SuppressionReturnValue { get; set; }

			public void SetDocWrapperContext(Dictionary<string, object> constants) { }

			public ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => ZString.Empty;

			public DocWrapperCopyInfo AdditionalCopyInfo => null;

			public string[] ImageNamesToRemove => System.Array.Empty<string>();

			public BusinessObject BusinessObjectToLogAgainst => null;

			public BusinessObject ParentBusinessObject => null;

			public IZType GetCustomField(string fieldName, string typeName) => ZString.Empty;

			public string GetCustomFieldCodeDescription(string fieldName, string typeName) => string.Empty;

			public ZDateTime GetEventLastDateTime(string eventCode) => ZDateTime.Empty;
		}
	}
}
