using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCusEntryHeaderGetsTheRightDocumentSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var dtiAdviceMessage = Factory.New<EDIMessage>();
			dtiAdviceMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			dtiAdviceMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			dtiAdviceMessage.EM_Status = EDIMessage.Status.Queued;
			dtiAdviceMessage.EM_MessageText = E2MessageStandard;
			dtiAdviceMessage.EM_MessageType = "RPA";
			entryHeader.Messages.Add(dtiAdviceMessage);

			var docSupporter = entryHeader.DocumentSupporter;
			AssertType<CusEntryHeaderDocumentSupporter>("entryHeader.DocumentSupporter", docSupporter);
			AssertDocumentWrapper(docSupporter, DataContext.GbCDSEntryHeader, "Enterprise.Customs.GB.DocumentWrappers.DocCDSEntryHeaderWrapper");
			AssertDocumentWrapper(docSupporter, DataContext.SADH, "Enterprise.Customs.GB.DocumentWrappers.DocSADH");
		}

		void AssertDocumentWrapper(DocumentSupporter docSupporter, DataContext dataContext, string wrapperTypeName)
		{
			var wrappers = docSupporter.GetDocumentWrappers(dataContext, null);
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(dataContext)));
			AssertEquals(1, wrappers.Length);
			AssertEquals(wrapperTypeName, wrappers[0].GetType().FullName);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var orgHeader0 = Factory.New<OrgHeader>();
			orgHeader0.OH_Code = "TESTORG1";
			var invoice0 = declaration.Invoices.AddNew();
			invoice0.InvoiceLines.AddNew();
			invoice0.JZ_OH_Supplier = orgHeader0.PK;
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG2";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.JZ_OH_Supplier = orgHeader1.PK;
			var line0 = invoice0.InvoiceLines.AddNew();
			line0.JI_CEI = entryInst.PK;
			new LineMerger(declaration).DoMerge();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			return declaration.ActiveEntryHeaders[0];
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return base.ExcludeDocumentCommandTest(documentCommand) || documentCommand.SU_MenuName == "Tax Estimator";
		}

		public const string E2MessageStandard = @"UNH+00001+CUSRES:D:04A:UN:109790'BGM+RPA::109:DTI-E2++6'DTM+148:201001221616:203'LOC+14+GBFXT::109'LOC+22+071::109'LOC+43+071::109'NAD+DT+GB545733236000'NAD+PB+928093309000::109'NAD+CN+GB928093309000++TIPPITOES LTD'RFF+ABT:071-000525M-22/01/2010:01'RFF+ACD:SAD'RFF+ACF:FCP1'RFF+TN:FCPFEY'RFF+ABO:9GB123456789000-B00001000:N'RFF+UCN:GB/FCP1-477110036'RFF+ABS:00'RFF+AHZ::6'DOC+960+SLDSSI00783'MOA+9:0.00'MOA+74:0.00'MOA+176:0.00'MOA+39:6261.34:USD'CUX+++1.59710000'CST+1'TAX+5'MOA+159:6261.34'MOA+40:6261.34'MOA+1:6261.34'MOA+123:6261.34'MOA+38:6261.34'CNT+11:100'UNT+32+00001'";
	}
}
