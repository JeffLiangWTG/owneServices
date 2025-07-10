using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineLookups))]
	sealed class JobComInvoiceLineLookupsBaseOnlyTest : JobComInvoiceLineLookupsAbstractTest
	{
		protected override JobComInvoiceLineLookups GetLookups() => new JobComInvoiceLineLookups(invoiceLine);

		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		public void TestCountryOfOrigins()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "CN", "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "IE", "IE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var list = (ZZRefCusCodeListCombinedCollection)invoiceLine.Lookups.CountryOfOrigins;
			AssertCodeList();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertCodeList();

			void AssertCodeList()
			{
				CombineAssertions($"JE_MessageType: {declaration.JE_MessageType}", () =>
				{
					AssertSame("Cached", list, invoiceLine.Lookups.CountryOfOrigins);
					list.Load();
					var codeList = list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code);
					Assert("Values for AU", codeList.Contains("AU"));
					Assert("Values for CN", codeList.Contains("CN"));
					Assert("Values for IE", codeList.Contains("IE"));
				});
			}
		}

		public void TestCPCList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(
				dataGroupingCode: "IE",
				category: "A",
				procedureCode: "40",
				previousProcedureCode: "78",
				concession: "C37",
				description: "4078C37 CPC",
				shipmentType: MessageTypeList.Codes.Import,
				group: "H1, H5, I1"
			);
			helper.CreateRefCusProcedure(
				dataGroupingCode: "IE",
				category: "A",
				procedureCode: "40",
				previousProcedureCode: "78",
				concession: ZString.Empty,
				description: "4078C37 CPC",
				shipmentType: MessageTypeList.Codes.Import,
				group: "H1, H5, I1"
			);
			helper.CreateRefCusProcedure(
				dataGroupingCode: "IE",
				category: "A",
				procedureCode: "10",
				previousProcedureCode: "00",
				concession: "E51",
				description: "1000 E51",
				shipmentType: MessageTypeList.Codes.Export,
				group: "B1, B4, C1"
			);
			helper.CreateRefCusProcedure(
				dataGroupingCode: "IE",
				category: "A",
				procedureCode: "10",
				previousProcedureCode: "00",
				concession: ZString.Empty,
				description: "1000",
				shipmentType: MessageTypeList.Codes.Export,
				group: "B1, B4, C1"
			);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cpcList = invoiceLine.Lookups.CPCList;

			CombineAssertions("For Imports, CPCList should filter procedures with empty ZZ6_Concession out.", () =>
			{
				AssertEquals("Count", 1, cpcList.Count);
				AssertEquals("ZZ6_Concession", "C37", cpcList[0].ZZ6_Concession);
			});

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			cpcList = invoiceLine.Lookups.CPCList;
			CombineAssertions("For Exports, CPCList should include procedures with empty ZZ6_Concession.", () =>
			{
				AssertEquals("Count", 2, cpcList.Count);
				AssertNotNull("ZZ6_Concession == Empty", cpcList.FirstOrDefault(x => x.ZZ6_Concession == ZString.Empty));
			});
		}
	}
}
