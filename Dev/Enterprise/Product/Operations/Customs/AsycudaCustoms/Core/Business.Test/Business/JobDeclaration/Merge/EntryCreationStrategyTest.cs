using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class EntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetKeyForHeader()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationDateOverride = ZDateTime.BrettsBirthday;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_OH_Supplier = org1.PK;
			invoice1.JZ_OH_Buyer = org2.PK;
			var invoice1Line = invoice1.InvoiceLines.AddNew();
			invoice1Line.JI_CEI = entryInstruction1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationDateOverride = ZDateTime.BrettsBirthday;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice2.JZ_OH_Supplier = org1.PK;
			invoice2.JZ_OH_Buyer = org2.PK;
			var invoice2Line = invoice2.InvoiceLines.AddNew();
			invoice2Line.JI_CEI = entryInstruction1.PK;
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry1 = declaration.ActiveEntryHeaders[0];
			AssertCusEntryHeader(entry1, Common.Shared.SharedJobMessageTypeList.Codes.Import, entryInstruction1.PK);
			invoice2Line.JI_CEI = entryInstruction2.PK;
			declaration.DoMerge();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			AssertCusEntryHeader((CusEntryHeader)declaration.ActiveEntryHeaders.FindByPK(invoice1Line.CusEntryLine.CL_CH), Common.Shared.SharedJobMessageTypeList.Codes.Import, entryInstruction1.PK);
			AssertCusEntryHeader((CusEntryHeader)declaration.ActiveEntryHeaders.FindByPK(invoice2Line.CusEntryLine.CL_CH), Common.Shared.SharedJobMessageTypeList.Codes.Import, entryInstruction2.PK);
			invoice2Line.JI_CEI = entryInstruction1.PK;
			foreach (var property in new (ZPropertyInfo info, IZType value)[]
			{
				(invoice2.JZ_ValuationDateOverrideInfo, ZDateTime.BrettsBirthday.AddDays(1)),
				(invoice2.JZ_IncoTermInfo, new ZString(Core.Constants.IncoTerms.CostAndInsurance)),
				(invoice2.JZ_RX_NKInvoice_CurrencyInfo, new ZString(Core.Constants.CurrencyCodes.Australia)),
				(invoice2.JZ_OH_SupplierInfo, org3.PK),
				(invoice2.JZ_OH_BuyerInfo, org3.PK)
			})
			{
				var info = property.info;
				var oldValue = property.info.Value;
				try
				{
					info.Value = property.value;
					declaration.DoMerge();
					var entry1PK = invoice1Line.CusEntryLine.CL_CH;
					var entry2PK = invoice2Line.CusEntryLine.CL_CH;
					AssertNotEquals(entry1PK, entry2PK);
					AssertEquals(2, declaration.ActiveEntryHeaders.Count);
					AssertCusEntryHeader((CusEntryHeader)declaration.ActiveEntryHeaders.FindByPK(entry1PK), Common.Shared.SharedJobMessageTypeList.Codes.Import, entryInstruction1.PK);
					AssertCusEntryHeader((CusEntryHeader)declaration.ActiveEntryHeaders.FindByPK(entry2PK), Common.Shared.SharedJobMessageTypeList.Codes.Import, entryInstruction1.PK);
				}
				finally
				{
					info.Value = oldValue;
				}
			}
		}

		static void AssertCusEntryHeader(CusEntryHeader entry, ZString messageType, ZGuid entryInstructionPK)
		{
			AssertEquals("CH_MessageType", messageType, entry.CH_MessageType);
			AssertEquals("CH_CEI_Instruction", entryInstructionPK, entry.CH_CEI_Instruction);
		}

		public void TestGetKeyForLine()
		{
			var part1PK = Factory.New<OrgSupplierPart>().PK;
			var part2PK = Factory.New<OrgSupplierPart>().PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "86040608";
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine1.JI_PreviousEntryNumber = "123";
			invoiceLine1.JI_PreviousEntryLineNumber = 111;
			invoiceLine1.JI_PrimaryPreference = "pref";
			invoiceLine1.JI_CustomsSecondUnitQty = "T";
			invoiceLine1.JI_CustomsThirdUnitQty = "KT";
			invoiceLine1.JI_BondedWhsUnitQty = "G";
			invoiceLine1.JI_ZZF_NKTaxType = "VAT";
			invoiceLine1.JI_Description = "desc";
			invoiceLine1.JI_OP = part1PK;
			invoiceLine1.JI_Procedure = "66";
			var invoiceLine1SD1 = invoiceLine1.SupportingDocuments.AddNew();
			invoiceLine1SD1.CSI_Code = "B";
			invoiceLine1SD1.CSI_ReferenceNumber = "10";
			var invoiceLine1SD2 = invoiceLine1.SupportingDocuments.AddNew();
			invoiceLine1SD2.CSI_Code = "A";
			invoiceLine1SD2.CSI_ReferenceNumber = "14";
			var invoiceLine1SD3 = invoiceLine1.SupportingDocuments.AddNew();
			invoiceLine1SD3.CSI_Code = "A";
			invoiceLine1SD3.CSI_ReferenceNumber = "11";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "86040608";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CountryOfOrigin = "AU";
			invoiceLine2.JI_PreviousEntryNumber = "123";
			invoiceLine2.JI_PreviousEntryLineNumber = 111;
			invoiceLine2.JI_PrimaryPreference = "pref";
			invoiceLine2.JI_CustomsSecondUnitQty = "T";
			invoiceLine2.JI_CustomsThirdUnitQty = "KT";
			invoiceLine2.JI_BondedWhsUnitQty = "G";
			invoiceLine2.JI_ZZF_NKTaxType = "VAT";
			invoiceLine2.JI_Description = "desc";
			invoiceLine2.JI_OP = part1PK;
			invoiceLine2.JI_Procedure = "66";
			var invoiceLine2SD1 = invoiceLine2.SupportingDocuments.AddNew();
			invoiceLine2SD1.CSI_Code = "A";
			invoiceLine2SD1.CSI_ReferenceNumber = "14";
			var invoiceLine2SD2 = invoiceLine2.SupportingDocuments.AddNew();
			invoiceLine2SD2.CSI_Code = "B";
			invoiceLine2SD2.CSI_ReferenceNumber = "10";
			var invoiceLine2SD3 = invoiceLine2.SupportingDocuments.AddNew();
			invoiceLine2SD3.CSI_Code = "A";
			invoiceLine2SD3.CSI_ReferenceNumber = "11";
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
			var entryLinePK = entry.MergedLines[0].PK;
			AssertEquals(entryLinePK, invoiceLine1.JI_CL);
			AssertEquals(entryLinePK, invoiceLine2.JI_CL);
			invoiceLine2SD2.Delete();
			declaration.DoMerge();
			var entryLinePKs = entry.MergedLines.Select(x => x.PK).ToList();
			AssertEquals(2, entryLinePKs.Count);
			Assert(entryLinePKs.Remove(invoiceLine1.JI_CL));
			Assert(entryLinePKs.Remove(invoiceLine2.JI_CL));
			AssertEquals(0, entryLinePKs.Count);
			invoiceLine1SD1.Delete();
			foreach (var property in new (ZPropertyInfo info, IZType value)[]
			{
				(invoiceLine2.JI_TariffInfo, new ZString("96040608")),
				(invoiceLine2.JI_CustomsUnitQtyInfo, new ZString(Core.Constants.Weight.Pounds)),
				(invoiceLine2.JI_CountryOfOriginInfo, new ZString(Core.Constants.CountryCodes.NewZealand)),
				(invoiceLine2.JI_PreviousEntryNumberInfo, new ZString("456")),
				(invoiceLine2.JI_PreviousEntryLineNumberInfo, new ZShort(2)),
				(invoiceLine2.JI_PrimaryPreferenceInfo, new ZString(Core.Constants.IncoTerms.CostAndInsurance)),
				(invoiceLine2.JI_CustomsSecondUnitQtyInfo, new ZString(Core.Constants.Weight.Pounds)),
				(invoiceLine2.JI_CustomsThirdUnitQtyInfo, new ZString(Core.Constants.Weight.Pounds)),
				(invoiceLine2.JI_BondedWhsUnitQtyInfo, new ZString(Core.Constants.Weight.Pounds)),
				(invoiceLine2.JI_ZZF_NKTaxTypeInfo, new ZString("STD")),
				(invoiceLine2.JI_DescriptionInfo, new ZString("desc1")),
				(invoiceLine2.JI_OPInfo, part2PK), (invoiceLine2.JI_ProcedureInfo, new ZString("67")),
				(invoiceLine2SD3.CSI_CodeInfo, new ZString("C")), (invoiceLine2SD3.CSI_ReferenceNumberInfo, new ZString("12"))
			})
			{
				var info = property.info;
				var oldValue = property.info.Value;
				try
				{
					info.Value = property.value;
					declaration.DoMerge();
					entryLinePKs = entry.MergedLines.Select(x => x.PK).ToList();
					AssertEquals(2, entryLinePKs.Count);
					Assert(entryLinePKs.Remove(invoiceLine1.JI_CL));
					Assert(entryLinePKs.Remove(invoiceLine2.JI_CL));
					AssertEquals(0, entryLinePKs.Count);
				}
				finally
				{
					info.Value = oldValue;
				}
			}
		}
	}
}
