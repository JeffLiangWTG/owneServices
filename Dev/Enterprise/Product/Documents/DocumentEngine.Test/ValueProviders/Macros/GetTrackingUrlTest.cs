using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetTrackingUrl))]
	sealed class GetTrackingUrlTest : ValueProviderTest
	{
		public void TestReplacementWithInvalidParameters()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement(string.Format("<GetTrackingUrl({0},{1},{2})>", "123456789", nameof(TrackingConstants.BusinessContext.Booking), ZGuid.NewZGuid()), Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertStartsWith("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in GetTrackingUrl Macro: Invalid ContactPK '123456789';",
				Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement(string.Format("<GetTrackingUrl({0},{1},{2})>", ZGuid.NewZGuid(), "InvalidBusinessContext", ZGuid.NewZGuid()), Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertStartsWith("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in GetTrackingUrl Macro: Invalid TrackingBusinessContext type 'InvalidBusinessContext';",
				Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("GetTrackingUrl", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GetTrackingUrl meh>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GETTRACKINGURL(05D8F313-314F-42C6-9790-83DFB5893D09,XXX,6E449683-C509-11CF-AAFA-00AA00B6015C)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTrackingUrl(05D8F313-314F-42C6-9790-83DFB5893D09,Shipment,6E449683-C509-11CF-AAFA-00AA00B6015C)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GETTrackingUrl(          05D8F313-314F-42C6-9790-83DFB5893D09, Declaration , 6E449683-C509-11CF-AAFA-00AA00B6015C )    >", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GetTrackingUrl (,WarehouseOrder, 6E449683-C509-11CF-AAFA-00AA00B6015C)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTrackingUrl(05D8F313-314F-42C6-9790-83DFB5893D09,Order,6E449683-C509-11CF-AAFA-00AA00B6015C )       >", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<  GetTrackingUrl( 05D8F313-314F-42C6-9790-83DFB5893D09,Transaction,   6E449683-C509-11CF-AAFA-00AA00B6015C )       >", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTrackingUrl(05D8F313-314F-42C6-9790-83DFB5893D09, Booking ,   6E449683-C509-11CF-AAFA-00AA00B6015C)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTrackingUrl(05D8F313-314F-42C6-9790-83DFB5893D09, Booking ,   123456789-0)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			ZString originlValue = WebDataRegistry.Instance.WebTrackerUrl.Value;
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/tracking");
			try
			{
				ZGuid contactPK = ZGuid.NewZGuid();
				ZGuid businessContextPK = ZGuid.NewZGuid();
				ZString businessContextNK = "123456789-0";

				string expectedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, TrackingConstants.BusinessContext.Booking, businessContextPK);
				AssertEquals(expectedUrl, ValueProviderToTest.GetReplacement(string.Format("<GetTrackingUrl({0},{1},{2})>", contactPK, nameof(TrackingConstants.BusinessContext.Booking), businessContextPK), Report));

				expectedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, TrackingConstants.BusinessContext.Booking, businessContextNK);
				AssertEquals(expectedUrl, ValueProviderToTest.GetReplacement(string.Format("<GetTrackingUrl({0},{1},{2})>", contactPK, nameof(TrackingConstants.BusinessContext.Booking), businessContextNK), Report));
			}
			finally
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originlValue);
			}
		}

		[ExpectNoExceptions]
		public void TestReplacement_WithInvalidContactPK()
		{
			ValueProviderToTest.GetReplacement("<GetTrackingUrl(invalidPK,Shipment,01-02-03-04)>", Report);
		}

		[ExpectNoExceptions]
		public void TestReplacement_WithInvalidBusinessContext()
		{
			ValueProviderToTest.GetReplacement("<GetTrackingUrl(05D8F313-314F-42C6-9790-83DFB5893D09,mehmehinvalidcontext,6E449683-C509-11CF-AAFA-00AA00B6015C)>", Report);
		}

		[ExpectNoExceptions()]
		public void TestReplacement_WithEmptyBusinessContextPK()
		{
			AssertEquals("Should be empty string", string.Empty, ValueProviderToTest.GetReplacement("<GetTrackingUrl(05D8F313-314F-42C6-9790-83DFB5893D09,Shipment,)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GetTrackingUrl();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Contact.OC_PK", "CBEB9589-A539-4E82-BBDE-33ABD05331A4"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Booking.EB_PK", "6E449683-C509-11CF-AAFA-00AA00B6015C"));
		}
	}
}
