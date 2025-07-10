using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TransportCoTrackingURL))]
	sealed class TransportCoTrackingURLTest : ValueProviderTest
	{
		#region Regular Expression Pattern Matching

		public void TestIsResponsibleForReplacing()
		{
			const string macroName = "TransportCoTrackingURL";
			foreach (string shouldMatch in new string[]
				{
					$"<{macroName}(a,b)>",
					$"< {macroName} ( a, b) >",
					$"<{macroName} ( abc , 123 ) >",
					$"< {macroName} (1 ,abc)>",
					$"<  \t{macroName}\t ( missingref,) >",
					$"<  \t{macroName}\t ( missingref, ) >",
					$"<  \t{macroName}\t ( missingref , ) >"
				})
			{
				IsResponsibleForReplacing(shouldMatch);
			}
		}

		public void TestNotResponsibleForReplacing()
		{
			foreach (string shouldntMatch in new string[]
				{
					"<>",
					"<TransportCoTrackingURL>",
					"<TransportCoTracking(a,b)>",
					"<TransportCoTracking(a)>",
					"<TransportCoTracking(,)>",
					"<TransportCoTrackingURL(a,b)/>",
					" <TransportCoTrackingURL(a,b)>",
					"<TransportCoTrackingURL(a,b)> ",
					"<Transport Co Tracking URL(a,b)>"
				})
			{
				Assert($"shouldn't match {shouldntMatch}", !GetNewValueProvider().IsResponsibleForReplacing(shouldntMatch, Passes.FirstPass));
			}
		}

		void IsResponsibleForReplacing(string exp)
		{
			IsResponsibleForReplacingWithCasing(exp);
			IsResponsibleForReplacingWithCasing(exp.ToLower());
			IsResponsibleForReplacingWithCasing(exp.ToUpper());
		}

		void IsResponsibleForReplacingWithCasing(string exp) =>
			Assert($"should match {exp}", GetNewValueProvider().IsResponsibleForReplacing(exp, Passes.FirstPass));

		#endregion

		#region TestMacroReplacement

		public void TestTransportRefCaseUnchanged()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com?v=(*CargoWiseREF*)");
			factory.Save();

			CheckSucceeds("<TransportCoTrackingURL(CARRIER,MiXeDcAsE)>", @"http://carrier.com?v=MiXeDcAsE");
		}

		public void TestWhitespaceIsTrimmed()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com?v=(*CargoWiseREF*)");
			factory.Save();

			CheckSucceeds("<TransportCoTrackingURL (  \tCARRIER\t\t ,  hello\t )>", @"http://carrier.com?v=hello");
		}

		public void TestMidInsertURL()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com?v=(*CargoWiseREF*);p=somethingelse");
			factory.Save();

			CheckSucceeds("<TransportCoTrackingURL(CARRIER,hello)>", @"http://carrier.com?v=hello;p=somethingelse");
		}

		public void TestIPLinkAddress()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://127.0.0.1/(*CargoWiseREF*)");
			factory.Save();

			CheckSucceeds("<TransportCoTrackingURL(CARRIER,123)>", @"http://127.0.0.1/123");
		}

		public void TestBlankRef()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://127.0.0.1/(*CargoWiseREF*)");
			factory.Save();

			CheckSucceeds("<TransportCoTrackingURL(CARRIER,)>", @"http://127.0.0.1/");
		}

		public void TestCorrectOrgSelected()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "ORGNUM1", OrgWebUrlList.Codes.CartageTracking, @"http://first.com/(*CargoWiseREF*)");
			AddOrgWithOneURL(factory, "ORGNUM2", OrgWebUrlList.Codes.CartageTracking, @"http://second.com/(*CargoWiseREF*)");
			factory.Save();

			CheckSucceeds("<TransportCoTrackingURL(ORGNUM2,NotTheFirstOrg)>", @"http://second.com/NotTheFirstOrg");
		}

		public void TestOrgWithTwoCRTsFirstPicked()
		{
			var factory = new BusinessObjectFactory();
			var org = AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com/first/(*CargoWiseREF*)");
			AddOrgWebURL(factory, org, OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com/second/(*CargoWiseREF*)");
			factory.Save();

			CheckSucceeds("<TransportCoTrackingURL(CARRIER,1stCRTChosen)>", @"http://carrier.com/first/1stCRTChosen");
		}

		public void TestNoOrgsDefinedFails()
		{
			CheckFails("<TransportCoTrackingURL(NOTHING,similartotestbelow)>");
		}

		public void TestNonExistingOrgFails()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com?v=(*CargoWiseREF*)");
			factory.Save();

			CheckFails("<TransportCoTrackingURL(DIFFORG,similartotestabove)>");
		}

		public void TestBadOHFormatFails()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com?v=(*CargoWiseREF*)");
			factory.Save();

			CheckFails("<TransportCoTrackingURL(The user has used a wrong data field like name,similartotestabove)>");
		}

		public void TestOrgWithoutURLFails()
		{
			var factory = new BusinessObjectFactory();
			AddOrg(factory, "CARRIER");
			factory.Save();

			CheckFails("<TransportCoTrackingURL(CARRIER,doesnthaveanywebsites)>");
		}

		public void TestOrgWithoutCRTURLFails()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.MainWebsite, @"http://carrier.com?v=(*CargoWiseREF*)");
			factory.Save();

			CheckFails("<TransportCoTrackingURL(CARRIER,MainWebsiteNotCRT)>");
		}

		public void TestIllegalURLFormatFails()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"this isn't a good URL, but contains insert (*CargoWiseREF*)");
			factory.Save();

			CheckFails("<TransportCoTrackingURL(CARRIER,will fail url validation)>");
		}

		#endregion

		#region Implementation

		static OrgHeader AddOrgWithOneURL(BusinessObjectFactory factory, string orgCode, string urlType, string url)
		{
			var org = AddOrg(factory, orgCode);
			AddOrgWebURL(factory, org, urlType, url);
			return org;
		}

		static OrgHeader AddOrg(BusinessObjectFactory factory, string orgCode)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = orgCode;
			return org;
		}

		static void AddOrgWebURL(BusinessObjectFactory factory, OrgHeader org, string urlType, string url)
		{
			var orgWeb = factory.New<OrgWebURL>();
			orgWeb.PU_OH = org.PK;
			orgWeb.PU_Type = urlType;
			orgWeb.PU_URL = url;
		}

		void CheckSucceeds(string macro, string expectedURL)
		{
			var o = GetNewValueProvider().GetReplacement(macro, Report);
			AssertType<ExcelHyperlink>(o);
			CheckExcelHyperlink((ExcelHyperlink)o, expectedURL);
		}

		void CheckFails(string macro)
		{
			var o = GetNewValueProvider().GetReplacement(macro, Report);
			AssertType<string>(o);
			AssertEquals("", (string)o);
		}

		static void CheckExcelHyperlink(ExcelHyperlink link, string expectedURL)
		{
			AssertEquals(expectedURL, link.LinkLocation);
			AssertEquals(expectedURL, link.TextToShow);
			AssertEquals("", link.TargetFrame);
			AssertEquals("", link.TextMark);
			AssertEquals("", link.Tooltip);
			AssertEquals(FlexCel.Core.THyperLinkType.URL, link.Type);
		}

		protected override ValueProvider GetNewValueProvider() => new TransportCoTrackingURL();

		protected override void PrepareDataForExamplesEvaluate()
		{
			var factory = new BusinessObjectFactory();
			AddOrgWithOneURL(factory, "CARRIER", OrgWebUrlList.Codes.CartageTracking, @"http://carrier.com?v=(*CargoWiseREF*)");
			factory.Save();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("CarrierCode", "Carrier"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("CarrierRef", "Hello"));
		}

		#endregion
	}
}
