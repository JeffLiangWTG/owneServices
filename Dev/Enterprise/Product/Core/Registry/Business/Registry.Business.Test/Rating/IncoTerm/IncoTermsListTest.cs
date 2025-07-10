using System;
using System.Collections.Generic;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Incoterms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Registry.Business.Testing
{
	sealed class IncoTermsListTest : TransactionedTestCase
	{
		[TestDate(2100, 1, 1)]
		public void TestIncoTerms()
		{
			var list = new IncoTermsCodeDescriptionPairList(IncoTermsListType.Default);
			var codes = list.Cast<CodeDescriptionPair>().Select(pair => pair.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Incoterms.Incoterms2000.Union(Incoterms.Incoterms2010.Union(Incoterms.Incoterms2020)).ToArray(), codes);

			const string hao = "好";
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter((key) => new ResourceStringData(key, hao));
				foreach (CodeDescriptionPair pair in new IncoTermsCodeDescriptionPairList(IncoTermsListType.Default))
				{
					if (new List<string>() { Constants.IncoTerms.FreeCarrier, Constants.IncoTerms.FreeCarrierBuyer, Constants.IncoTerms.FreeCarrierSeller }.Contains(pair.Code))
					{
						Assert("Incoterms should be translated", pair.Description.Contains(hao));
					}
					else
					{
						Assert("Incoterms should not be translated", !pair.Description.Contains(hao));
					}
				}
			}
		}

		public void TestIncoTermsWithTypeActiveIncludingDomesticTerms()
		{
			CodeDescriptionPairList list = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncludingDomesticTerms);
			string[] codes = (from elem in list.Cast<CodeDescriptionPair>() select elem.Code).ToArray();

			var expectedCodePartList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			expectedCodePartList.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));

			var expectedCodes = (from elem in expectedCodePartList.Cast<CodeDescriptionPair>() select elem.Code).ToArray();
			AssertContainsExactElementsInAnyOrder("Incoterm List should have all expected elements", expectedCodes, codes);
		}

		public void TestIncoTerms2000()
		{
			CodeDescriptionPairList list = new IncoTermsCodeDescriptionPairList(IncoTermsListType.IncoTerms2000);
			string[] codes = (from elem in list.Cast<CodeDescriptionPair>() select elem.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Incoterms.Incoterms2000, codes);
		}

		public void TestIncoTerms2010()
		{
			CodeDescriptionPairList list = new IncoTermsCodeDescriptionPairList(IncoTermsListType.IncoTerms2010);
			string[] codes = (from elem in list.Cast<CodeDescriptionPair>() select elem.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Incoterms.Incoterms2010, codes);
		}

		public void TestIncoTerms2020()
		{
			var list = new IncoTermsCodeDescriptionPairList(IncoTermsListType.IncoTerms2020);
			var codes = list.Cast<CodeDescriptionPair>().Select(pair => pair.Code);

			AssertContainsExactElementsInAnyOrder(Incoterms.Incoterms2020, codes);
		}

		public void TestUseRegistryOverrides()
		{
			var incoTermDefinitions = new IncoTermChargeCodesCollection();
			var def = incoTermDefinitions.AddNew();
			def.IncoTerm = Incoterms.FreeCarrier;
			def.IncoTermDescription = "Splogletthi boglaknees";
			def.Origin = Constants.PaymentParty.Consignee;
			def.Loading = Constants.PaymentParty.Consignee;
			def.Freight = Constants.PaymentParty.Consignee;
			def.Insurance = Constants.PaymentParty.Consignee;
			def.Unloading = Constants.PaymentParty.Consignee;
			def.Destination = Constants.PaymentParty.Consignee;
			def.Brokerage = Constants.PaymentParty.Consignee;
			def.CustomsDuty = Constants.PaymentParty.Consignee;
			def.OriginBrokerage = Constants.PaymentParty.Consignee;

			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incoTermDefinitions))
			{
				var listUsingRegistryOverrides = new IncoTermsCodeDescriptionPairList();
				AssertEquals("Description should equal the overriden registry value by default.", "Splogletthi boglaknees", listUsingRegistryOverrides.GetDescriptionFromCode(Incoterms.FreeCarrier));
			}
		}

		[TestDate(2010, 12, 31)]
		public void TestActiveIncoTerms2000_1()
		{
			AssertActiveIncoTerms(Incoterms.Incoterms2000);
		}

		[TestDate(2000, 1, 10)]
		public void TestActiveIncoTerms2000_2()
		{
			AssertActiveIncoTerms(Incoterms.Incoterms2000);
		}

		[TestDate(2011, 1, 1)]
		public void TestActiveIncoTerms2010_1()
		{
			AssertActiveIncoTerms(Incoterms.Incoterms2010);
		}

		[TestDate(2011, 7, 25)]
		public void TestActiveIncoTerms2010_2()
		{
			AssertActiveIncoTerms(Incoterms.Incoterms2010);
		}

		[TestDate(2019, 01, 01)]
		public void TestActiveIncoTerms_Pre2020()
		{
			AssertActiveIncoTerms(Incoterms.Incoterms2010);
		}

		[TestDate(2020, 01, 01)]
		public void TestActiveIncoTerms_Post2020()
		{
			AssertActiveIncoTerms(Incoterms.Incoterms2020.Union(Incoterms.Incoterms2010));
		}

		[TestDate(2100, 1, 1)]
		public void TestActiveIncoTerms_FailIfDifferentFromGlow()
		{
			AssertContainsExactElementsInAnyOrder("Please update incoterm in GLOW", ActiveIncotermsInGlow, new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));
		}

		void AssertActiveIncoTerms(IEnumerable<string> expectedCodes)
		{
			var list = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			var actualCodes = list.Cast<CodeDescriptionPair>().Select(elem => elem.Code);

			AssertContainsExactElementsInAnyOrder(actualCodes, expectedCodes);
		}

		List<CodeDescriptionPair> ActiveIncotermsInGlow => new List<CodeDescriptionPair>
		{
			new CodeDescriptionPair("CFR", "Cost And Freight"),
			new CodeDescriptionPair("CIF", "Cost, Insurance And Freight"),
			new CodeDescriptionPair("CIP", "Carriage and Insurance Paid To"),
			new CodeDescriptionPair("CPT", "Carriage Paid To"),
			new CodeDescriptionPair("DAP", "Delivered At Place"),
			new CodeDescriptionPair("DAT", "Delivered At Terminal"),
			new CodeDescriptionPair("DDP", "Delivered Duty Paid"),
			new CodeDescriptionPair("DPU", "Delivered at Place Unloaded"),
			new CodeDescriptionPair("EXW", "Ex Works"),
			new CodeDescriptionPair("FAS", "Free Alongside Ship"),
			new CodeDescriptionPair("FC1", "FCA - Free Carrier (seller is responsible for origin and loading)"),
			new CodeDescriptionPair("FC2", "FCA - Free Carrier (buyer is responsible for origin and loading)"),
			new CodeDescriptionPair("FCA", "FCA - Free Carrier (seller is responsible for origin, buyer for loading)"),
			new CodeDescriptionPair("FOB", "Free On Board"),
		};
	}
}
