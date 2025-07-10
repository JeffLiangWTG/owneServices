using System;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DLMMergeStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
	{
		public void TestLinesWithSameDescriptionsMerge()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60";
			line1.JI_Description = "Beer";
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60";
			line2.JI_Description = "Beer";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestLinesWithDifferentDescriptions()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "AIR";

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60";
			line1.JI_Description = "Beer";
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60";
			line2.JI_Description = "Beer 2";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestExportMergeForDLM()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B001231223";
			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60";
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60";
			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("CH_BGMReference", "B001231223", declaration.CustomsEntryHeaders[0].CH_BGMReference);
			AssertEquals("EntryNumber", "B001231223", declaration.CustomsEntryHeaders[0].EntryNumber);
			AssertEquals("MessageType", MessageTypeList.Codes.DataLoadingModule, declaration.CustomsEntryHeaders[0].CH_MessageType);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestExportMergeForG7Export()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B001231223";
			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60 00";
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60 00";
			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("CH_BGMReference", "B001231223", declaration.CustomsEntryHeaders[0].CH_BGMReference);
			AssertEquals("EntryNumber", "", declaration.CustomsEntryHeaders[0].EntryNumber);
			AssertEquals("MessageType", MessageTypeList.Codes.G7Export, declaration.CustomsEntryHeaders[0].CH_MessageType);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public override void TestGetKeyForHeader()
		{
			Customs.Business.MergeKey mergeKey = new Customs.Business.MergeKey(1);
			mergeKey.Add(new CargoWise.Types.ZString(MessageTypeList.Codes.DataLoadingModule));
			AssertEquals(mergeKey, strategy.GetKeyForHeader(invoiceLine));
		}

		public void TestGetKeyForLine()
		{
			Customs.Business.MergeKey mergeKey = strategy.GetKeyForLine(invoiceLine);
			Assert(mergeKey.Contains(invoiceLine.JI_Tariff));
			Assert(mergeKey.Contains(invoiceLine.EffectiveCountryOfOrigin));
			Assert(mergeKey.Contains(invoiceLine.JI_CustomsUnitQty));
		}

		#region Implementation
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		ExportMergeStrategy strategy;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			strategy = new ExportMergeStrategy(declaration, MessageTypeList.Codes.DataLoadingModule);
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		#endregion Implementation
	}
}
