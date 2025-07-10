using System.Collections.Generic;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(NACCSDataImporter))]

sealed class NACCSDataImporterTest : TestCaseWithFactory
{
	public void TestTryImportDeclarationRegistrationCopy()
	{
		var declaration = ImportJobDeclaration;
		declaration.JE_MergeBy = "TRF";
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
		var charge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 2000m, invoiceHeader.JZ_RX_NKInvoice_Currency);
		charge.J7_IsDutiable = true;
		var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
		var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
		line1.JI_CEI = entryInstruction1.PK;
		line2.JI_CEI = entryInstruction2.PK;

		GlbCompany.CurrentCompany.GC_IsReciprocal = true;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
		var entryLine2 = declaration.ActiveEntryHeaders[1].MergedLines[0];
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		entryLine1 = cusEntryHeader.AllEntryLines.AddNew();
		entryLine2 = cusEntryHeader.AllEntryLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		entryLine2.CL_LineNumber = 2;
		var cusEntryLines = cusEntryHeader.AllEntryLines;

		var edaMockRegistrationCopy = new Mock<IExportDeclarationRegistrationCopy>();
		var edaItems = new List<EDAItem>
		{
			new EDAItem
			{
				ColumnNumber = "1",
				CustomsValue = 1199999991999,
				OriginalColumnNumber = "AA",
				CustomsValueSummary = 9999999991999,
				PriceReconfirmationType = "H"
			},
			new EDAItem
			{
				ColumnNumber = "2",
				CustomsValue = 1,
				OriginalColumnNumber = "BB",
				CustomsValueSummary = 9,
				PriceReconfirmationType = "L"
			}
		};

		edaMockRegistrationCopy.Setup(r => r.Items).Returns(edaItems);

		cusEntryHeader.TryImportExpDeclarationRegistrationCopy(edaMockRegistrationCopy.Object);

		CombineAssertions(() =>
			{
				AssertEquals("CL_ConfirmedCustomsValue", new ZDecimal(1199999991999), entryLine1.CL_ConfirmedCustomsValue);
				AssertEquals("CL_ParentLineNumber", "AA", entryLine1.CL_ParentLineNumber);
				AssertEquals("CL_MergedCustomsValue", new ZDecimal(9999999991999), entryLine1.CL_MergedCustomsValue);
				AssertEquals("CL_PriceCheck", "H", entryLine1.CL_PriceCheck);

				AssertEquals("CL_ConfirmedCustomsValue", new ZDecimal(1), entryLine2.CL_ConfirmedCustomsValue);
				AssertEquals("CL_ParentLineNumber", "BB", entryLine2.CL_ParentLineNumber);
				AssertEquals("CL_MergedCustomsValue", new ZDecimal(9), entryLine2.CL_MergedCustomsValue);
				AssertEquals("CL_PriceCheck", "L", entryLine2.CL_PriceCheck);
			});

		var idaMockRegistrationCopy = new Mock<IImportDeclarationRegistrationCopy>();
		var idaItems = new List<IDAItem>
		{
			new IDAItem
			{
				ColumnNumber = 1,
				OriginalColumnNumber = 77,
				PriceReconfirmationType = "G",
				DutyAmount = 1199999991999,
				DutyAmountSummary = 9999999991999,
			},
			new IDAItem
			{
				ColumnNumber = 2,
				OriginalColumnNumber = 99,
				PriceReconfirmationType = "H",
				DutyAmount = 1,
				DutyAmountSummary = 9,
			}
		};

		idaMockRegistrationCopy.Setup(r => r.Items).Returns(idaItems);

		cusEntryHeader.TryImportImpDeclarationRegistrationCopy(idaMockRegistrationCopy.Object);

		CombineAssertions(() =>
		{
			AssertEquals("CL_ConfirmedCustomsValue", new ZDecimal(1199999991999), entryLine1.CL_ConfirmedCustomsValue);
			AssertEquals("CL_ParentLineNumber", "77", entryLine1.CL_ParentLineNumber);
			AssertEquals("CL_MergedCustomsValue", new ZDecimal(9999999991999), entryLine1.CL_MergedCustomsValue);
			AssertEquals("CL_PriceCheck", "G", entryLine1.CL_PriceCheck);

			AssertEquals("CL_ConfirmedCustomsValue", new ZDecimal(1), entryLine2.CL_ConfirmedCustomsValue);
			AssertEquals("CL_ParentLineNumber", "99", entryLine2.CL_ParentLineNumber);
			AssertEquals("CL_MergedCustomsValue", new ZDecimal(9), entryLine2.CL_MergedCustomsValue);
			AssertEquals("CL_PriceCheck", "H", entryLine2.CL_PriceCheck);
		});
	}

	JobDeclaration ImportJobDeclaration
	{
		get
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			return declaration;
		}
	}

	class EDAItem : IExportItem
	{
		public string ColumnNumber { get; set; }

		public string OriginalColumnNumber { get; set; }

		public string PriceReconfirmationType { get; set; }

		public decimal? CustomsValue { get; set; }

		public decimal? CustomsValueSummary { get; set; }
	}

	class IDAItem : IImportItem
	{
		public short? ColumnNumber { get; set; }

		public short? OriginalColumnNumber { get; set; }

		public string PriceReconfirmationType { get; set; }

		public decimal? DutyAmount { get; set; }

		public decimal? DutyAmountSummary { get; set; }
	}
}
