using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.Business.CusEntryLine;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPENTHeaderProvider))]
	class EXPENTHeaderProviderTest : AESHeaderProviderAbstractTest<EXPENTHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPENTHeaderProvider(null));
		}

		public void TestIsContainerized()
		{
			foreach (var containerFlag in new ZString[] { Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.ULD, Core.Constants.ContainerModes.Containerised })
			{
				declaration.JE_ContainerMode = containerFlag;
				AssertEquals($"IsContainerised = '{containerFlag}'", true, expentrovider.IsContainerized);
			}
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Miscellaneous IsContainerised", false, expentrovider.IsContainerized);
		}

		public void TestTransportEquipments()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(2, expentrovider.TransportEquipments.Count);
		}

		public void TestTransportEquipments_NotPopulated()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(false, expentrovider.TransportEquipments.Any());
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
				AssertEquals("EoriNumber", "GREOR1", Provider.Consignee.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS1", Provider.Consignee.EoriBranchSuffix);
				AssertEquals("Name", "Importer", Provider.Consignee.Name);
				AssertEquals("Line", "Address1", Provider.Consignee.Address);
				AssertEquals("City", "City", Provider.Consignee.City);
				AssertEquals("Postcode", "2730018", Provider.Consignee.Postcode);
				AssertEquals("Country", "US", Provider.Consignee.Country);
			});
		}

		public void TestConsignee_Empty()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_OA_ConsigneeAddress = declaration.ImporterDocumentaryAddress.E2_OA_Address;
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_OA_ConsigneeAddress = Guid.Empty;
			entryHeader.ResetInvoiceHeadersAndLines();
			entryLine.InvoiceLines.ReloadFromLocalCache();
			AssertNull(Provider.Consignee);
		}

		public void TestPreviousDocuments()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoice.PreviousDocuments.AddNew();
			invoice.PreviousDocuments.AddNew();
			AssertEquals(2, Provider.PreviousDocuments.Count);
		}

		public void TestPreviousDocuments_HasExportDataMessage()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var message = Factory.New<AesEDIMessage>();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2021, 11, 04);
			message.EM_ApplicationReference = "DEXPDF";
			message.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(message);
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var previousDocument = invoice.PreviousDocuments.AddNew();
			previousDocument.CSI_SystemCreateTimeUtc = new ZDateTime(2021, 11, 03);
			previousDocument.CSI_Code = "1234";
			var previousDocument2 = invoice.PreviousDocuments.AddNew();
			previousDocument2.CSI_SystemCreateTimeUtc = new ZDateTime(2021, 11, 06);
			previousDocument2.CSI_Code = "ABCD";
			AssertEquals("ABCD", Provider.PreviousDocuments.Single().FullType);
		}

		public void TestCreateTimeOfLatestExportDataMessage()
		{
			AssertEquals(ZDateTime.Empty, Provider.CreateTimeOfLatestExportDataMessage);
		}

		public void TestCreateTimeOfLatestExportDataMessage_HasExportDataMessage()
		{
			var message = Factory.New<AesEDIMessage>();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2021, 11, 02);
			message.EM_ApplicationReference = "DEXPDF";
			message.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(message);
			var message2 = Factory.New<AesEDIMessage>();
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2021, 11, 04);
			message2.EM_ApplicationReference = "DEXPDG";
			message2.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(message2);
			var message3 = Factory.New<AesEDIMessage>();
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2021, 11, 07);
			message3.EM_ApplicationReference = "DEXPED";
			message3.EM_LinkedObject = message3;
			entryHeader.Messages.Add(message3);
			Factory.Save();

			AssertEquals(new ZDateTime(2021, 11, 04), Provider.CreateTimeOfLatestExportDataMessage);
		}

		public void TestLines()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			entryLine.CL_LineNumber = 1;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;

			var entryLine3 = entryHeader.MergedLines.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			entryLine3.CL_LineNumber = 3;

			action.EntryLines.Cast<ExportEntryLine>().First(x => x.LineNumber == 2).ShouldSend = false;
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 3 }, Provider.Lines.Select(x => x.LineNumber));
		}

		public void TestDeliveryTerms()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "1";
			invoice1.JZ_IncoTermPlace = "SYD";
			invoice1.ZG_AgreedPlaceCode = "AU";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			entryLine.CL_LineNumber = 1;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "1";
			invoice2.JZ_IncoTermPlace = "SYD";
			invoice2.ZG_AgreedPlaceCode = "AU";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;

			var deliveryTerms = Provider.DeliveryTerms;
			CombineAssertions(() =>
			{
				AssertEquals("IncotermCode", "1", deliveryTerms.IncotermCode);
				AssertEquals("Location", "SYD", deliveryTerms.Location);
				AssertEquals("Country", "AU", deliveryTerms.Country);
			});
		}

		protected override IEnumerable<Expression<Func<EXPENTHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Consignee;
		}

		protected override EXPENTHeaderProvider GetProvider() => new EXPENTHeaderProvider(action);

		protected override void SetUp()
		{
			base.SetUp();
			var importerAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			importerAddress.Header.OH_FullName = "Importer";
			importerAddress.Address1 = "Address1";
			importerAddress.City = "City";
			importerAddress.Postcode = "2730018";
			importerAddress.OA_RN_NKCountryCode = "US";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			action = new ExportEntryMessageSendingAction(entryHeader);
		}
		CusEntryLine entryLine;
		ExportEntryMessageSendingAction action;
		IEXPENTHeader expentrovider => base.Provider;
	}
}
