using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAPGADetailsCopyHelperTest : TestCaseWithFactory
	{
		public void TestCopyPGADetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();

			//CFIA PGA
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.CA_CFIAInd = "Y";
			var cfiaPGA = invoiceLine1.CFIAPGAHeader;
			cfiaPGA.CA_AllProgramInd = "Y";
			var lpco1 = cfiaPGA.LPCOViews.AddNew();
			lpco1.CLP_RefNo = "111";
			Factory.Save();

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.CA_CFIAInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine2, invoiceLine1);
			AssertEquals("Y", invoiceLine2.CFIAPGAHeader.CA_AllProgramInd);
			AssertEquals("LPCOs are copied", 1, invoiceLine2.CFIAPGAHeader.LPCOViews.Count);
			AssertEquals("111", invoiceLine2.CFIAPGAHeader.LPCOViews[0].CLP_RefNo);

			//CNSC PGA
			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.CA_CNSCInd = "Y";
			var cnscPGA = invoiceLine3.CNSCPGAHeader;
			cnscPGA.CA_AllProgramInd = "Y";
			var component1 = cnscPGA.Components.AddNew();
			component1.CA_Name = "test1";
			var lpco2 = cnscPGA.LPCOViews.AddNew();
			lpco2.CLP_RefNo = "222";
			Factory.Save();

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.CA_CNSCInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine4, invoiceLine3);
			AssertNotNull("CNSCPGAHeader is copied", invoiceLine4.CNSCPGAHeader);
			AssertEquals("Y", invoiceLine4.CNSCPGAHeader.CA_AllProgramInd);
			AssertEquals("Components are copied", 1, invoiceLine4.CNSCPGAHeader.Components.Count);
			AssertEquals("test1", invoiceLine4.CNSCPGAHeader.Components[0].CA_Name);
			AssertEquals("LPCOs are copied", 1, invoiceLine4.CNSCPGAHeader.LPCOViews.Count);
			AssertEquals("222", invoiceLine4.CNSCPGAHeader.LPCOViews[0].CLP_RefNo);

			//DFO PGA
			var invoiceLine5 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine5.CA_DFOInd = "Y";
			var dfoPGA = invoiceLine5.DFOPGAHeader;
			dfoPGA.CA_ABIProgramInd = "Y";
			var lpco3 = dfoPGA.LPCOViews.AddNew();
			lpco3.CLP_RefNo = "333";
			Factory.Save();

			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.CA_DFOInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine6, invoiceLine5);
			AssertNotNull("DFOPGAHeader is copied", invoiceLine6.DFOPGAHeader);
			AssertEquals("Y", invoiceLine6.DFOPGAHeader.CA_ABIProgramInd);
			AssertEquals("LPCOs are copied", 1, invoiceLine6.DFOPGAHeader.LPCOViews.Count);
			AssertEquals("333", invoiceLine6.DFOPGAHeader.LPCOViews[0].CLP_RefNo);

			//ECCC PGA
			var invoiceLine7 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine7.CA_ECCCInd = "Y";
			var ecccPGA = invoiceLine7.ECCCPGAHeader;
			ecccPGA.CA_ODSProgramInd = "Y";
			var component2 = ecccPGA.Components.AddNew();
			component2.CA_Name = "test2";
			var lpco4 = ecccPGA.LPCOViews.AddNew();
			lpco4.CLP_RefNo = "444";
			Factory.Save();

			var invoiceLine8 = declaration.InvoiceLines.AddNew();
			invoiceLine8.CA_ECCCInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine8, invoiceLine7);
			AssertNotNull("ECCCPGAHeader is copied", invoiceLine8.ECCCPGAHeader);
			AssertEquals("Y", invoiceLine8.ECCCPGAHeader.CA_ODSProgramInd);
			AssertEquals("Components are copied", 1, invoiceLine8.ECCCPGAHeader.Components.Count);
			AssertEquals("test2", invoiceLine8.ECCCPGAHeader.Components[0].CA_Name);
			AssertEquals("LPCOs are copied", 1, invoiceLine8.ECCCPGAHeader.LPCOViews.Count);
			AssertEquals("444", invoiceLine8.ECCCPGAHeader.LPCOViews[0].CLP_RefNo);

			//GAC PGA
			var invoiceLine9 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine9.CA_GACInd = "Y";
			var gacPGA = invoiceLine9.GACPGAHeader;
			gacPGA.CA_AllProgramInd = "Y";
			var lpco5 = gacPGA.LPCOViews.AddNew();
			lpco5.CLP_RefNo = "555";
			Factory.Save();

			var invoiceLine10 = declaration.InvoiceLines.AddNew();
			invoiceLine10.CA_GACInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine10, invoiceLine9);
			AssertNotNull("GACPGAHeader is copied", invoiceLine10.GACPGAHeader);
			AssertEquals("Y", invoiceLine10.GACPGAHeader.CA_AllProgramInd);
			AssertEquals("LPCOs are copied", 1, invoiceLine10.GACPGAHeader.LPCOViews.Count);
			AssertEquals("555", invoiceLine10.GACPGAHeader.LPCOViews[0].CLP_RefNo);

			//HC PGA
			var invoiceLine11 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine11.CA_HCInd = "Y";
			var hcPGA = invoiceLine11.HCPGAHeader;
			hcPGA.CA_APIProgramInd = "Y";
			var lpco6 = hcPGA.LPCOViews.AddNew();
			lpco6.CLP_RefNo = "666";
			var component3 = hcPGA.Components.AddNew();
			component3.CA_Name = "test3";
			Factory.Save();

			var invoiceLine12 = declaration.InvoiceLines.AddNew();
			invoiceLine12.CA_HCInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine12, invoiceLine11);
			AssertNotNull("HCPGAHeader is copied", invoiceLine12.HCPGAHeader);
			AssertEquals("Y", invoiceLine12.HCPGAHeader.CA_APIProgramInd);
			AssertEquals("LPCOs are copied", 2, invoiceLine12.HCPGAHeader.LPCOViews.Count);
			AssertEquals("666", invoiceLine12.HCPGAHeader.LPCOViews[1].CLP_RefNo);
			AssertEquals("Components are copied", 1, invoiceLine12.HCPGAHeader.Components.Count);
			AssertEquals("test3", invoiceLine12.HCPGAHeader.Components[0].CA_Name);

			//NRCan PGA
			var invoiceLine13 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine13.CA_NRCanInd = "Y";
			var nrCANPGA = invoiceLine13.NRCanPGAHeader;
			nrCANPGA.CA_EEFProgramInd = "Y";
			var lpco7 = nrCANPGA.LPCOViews.AddNew();
			lpco7.CLP_RefNo = "777";
			Factory.Save();

			var invoiceLine14 = declaration.InvoiceLines.AddNew();
			invoiceLine14.CA_NRCanInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine14, invoiceLine13);
			AssertNotNull("HCPGAHeader is copied", invoiceLine14.NRCanPGAHeader);
			AssertEquals("Y", invoiceLine14.NRCanPGAHeader.CA_EEFProgramInd);
			AssertEquals("LPCOs are copied", 1, invoiceLine14.NRCanPGAHeader.LPCOViews.Count);
			AssertEquals("777", invoiceLine14.NRCanPGAHeader.LPCOViews[0].CLP_RefNo);

			//PHAC PGA
			var invoiceLine15 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine15.CA_PHACInd = "Y";
			var phacPGA = invoiceLine15.PHACPGAHeader;
			phacPGA.CA_HAPProgramInd = "Y";
			var lpco8 = phacPGA.LPCOViews.AddNew();
			lpco8.CLP_RefNo = "888";
			Factory.Save();

			var invoiceLine16 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine16.CA_PHACInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine16, invoiceLine15);
			AssertNotNull("HCPGAHeader is copied", invoiceLine16.PHACPGAHeader);
			AssertEquals("Y", invoiceLine16.PHACPGAHeader.CA_HAPProgramInd);
			AssertEquals("LPCOs are copied", 1, invoiceLine16.PHACPGAHeader.LPCOViews.Count);
			AssertEquals("888", invoiceLine16.PHACPGAHeader.LPCOViews[0].CLP_RefNo);

			//TC PGA
			var invoiceLine17 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine17.CA_TCInd = "Y";
			var tcPGA = invoiceLine17.TCPGAHeader;
			tcPGA.CA_TPRProgramInd = "Y";
			var lpco9 = tcPGA.LPCOViews.AddNew();
			lpco9.CLP_RefNo = "999";
			Factory.Save();

			var invoiceLine18 = declaration.InvoiceLines.AddNew();
			invoiceLine18.CA_TCInd = "Y";
			CAPGADetailsCopyHelper.CopyPGADetails(invoiceLine18, invoiceLine17);
			AssertNotNull("HCPGAHeader is copied", invoiceLine18.TCPGAHeader);
			AssertEquals("Y", invoiceLine18.TCPGAHeader.CA_TPRProgramInd);
			AssertEquals("LPCOs are copied", 1, invoiceLine18.TCPGAHeader.LPCOViews.Count);
			AssertEquals("999", invoiceLine18.TCPGAHeader.LPCOViews[0].CLP_RefNo);
		}
	}
}
