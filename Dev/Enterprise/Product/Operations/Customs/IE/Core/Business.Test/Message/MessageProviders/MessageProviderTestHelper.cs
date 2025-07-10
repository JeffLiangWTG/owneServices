using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	static class MessageProviderTestHelper
	{
		public static void SetupForCusSupportingInfo(BusinessObjectFactory factory, bool headerOnly, IEnumerable<(string csiCode, string description)> codes, string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var euCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eun = helper.CreateNewOrGetExistingDataGrouping(euCountryCode, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);

			string[] level = headerOnly ? new[] { "HEADER" } : new[] { "ITEM", "HEADER" };
			var attributeNameValuePairs = new Dictionary<string, string[]> { { "Level", level } };

			foreach (var code in codes)
			{
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(
					euCountryCode,
					codeTypes: new[] { codeType },
					code: code.csiCode,
					description: code.description,
					attributeNameValuePairs: attributeNameValuePairs,
					ZDateTime.MinSmallDateTimeValue,
					ZDateTime.MaxSmallDateTimeValue
				);
			}
			factory.Save();
		}

		public static void SetupForCusSupportingInfo(BusinessObjectFactory factory, bool headerOnly, params string[] codes)
		{
			SetupForCusSupportingInfo(factory, headerOnly, codes.Select(code => (code, code + " Description")));
		}

		public static void SetupForCusSupportingInfo(BusinessObjectFactory factory, bool headerOnly, string codeType, params string[] codes)
		{
			SetupForCusSupportingInfo(factory, headerOnly, codes.Select(code => (code, code + " Description")), codeType: codeType);
		}

		public static (EntryLineWrapper secondEntryLineWrapper, JobComInvoiceLine secondInvoiceLine) SetupSecondLine(EntryHeaderWrapper entryHeaderWrapper)
		{
			var invoiceLine2 = entryHeaderWrapper.RandomInvoiceHeader.JobComInvoiceLines.AddNew();
			var entryLine2 = entryHeaderWrapper.EntryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			return (new EntryLineWrapper(entryLine2, entryHeaderWrapper), invoiceLine2);
		}

		public static (EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) SetupBasicTestBizObjs(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
			return (entryHeaderWrapper, new EntryLineWrapper(entryLine, entryHeaderWrapper));
		}

		public static (EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper, JobComInvoiceLine[] invoiceLines) SetupBasicTestBizObjsMultipleInvoiceLinesSingleEntryLine(BusinessObjectFactory factory)
		{
			(var entryHeaderWrapper, var entryLineWrapper) = SetupBasicTestBizObjs(factory);

			var invoice = entryLineWrapper.RandomInvoiceHeader;
			var anotherInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			anotherInvoiceLine.JI_CEI = entryLineWrapper.Instruction.PK;
			entryLineWrapper.EntryLine.InvoiceLines.Add(anotherInvoiceLine);
			entryHeaderWrapper.EntryHeader.ResetInvoiceHeadersAndLines();
			return (entryHeaderWrapper, entryLineWrapper, new[] { entryLineWrapper.RandomInvoiceLine, anotherInvoiceLine });
		}
	}
}
