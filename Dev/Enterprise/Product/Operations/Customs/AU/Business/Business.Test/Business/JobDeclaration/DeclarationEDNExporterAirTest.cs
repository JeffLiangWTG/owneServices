using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeclarationEDNExporter))]
	sealed class DeclarationEDNExporterAirTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "EDN"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Export Declaration"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				OrgHeader importer = OrgHeader.New(Factory);
				importer.OH_FullName = "I am an importer";

				OrgHeader supplier = OrgHeader.New(Factory);
				supplier.OH_FullName = "I am a supplier";
				supplier.PrimaryRegistrationNumber.Number = "12131212313";

				JobDeclaration dec = Factory.New<JobDeclaration>();
				dec.JE_DeclarationReference = "B123123";
				dec.JE_MasterBill = "M121313";
				dec.JE_HouseBill = "H3255";
				dec.JE_OH_Importer = importer.PK;
				dec.JE_OH_Supplier = supplier.PK;
				dec.JE_RL_NKFinalDestination = "INBOM";
				dec.JE_ExportDate = new ZDateTime(2005, 11, 26);
				dec.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec.JE_VoyageFlightNo = "QF492";
				dec.JE_GoodsDescription = "Shirts and shorts";
				dec.JE_RL_NKPortOfLoading = "AUBNE";

				JobComInvoiceGroupHeader groupHeader = dec.JobComInvoiceGroupHeaders.Count > 0 ? dec.JobComInvoiceGroupHeaders[0] : dec.JobComInvoiceGroupHeaders.AddNew();
				JobComInvoiceHeader header = groupHeader.JobComInvoiceHeaders.Count > 0 ? groupHeader.JobComInvoiceHeaders[0] : groupHeader.JobComInvoiceHeaders.AddNew();
				RefCurrency auCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				header.JZ_InvoiceAmount = 5000m;
				header.JZ_RX_NKInvoice_Currency = auCurrency.RX_Code;
				BaseJobComInvHeaderCharge charge = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, auCurrency.RX_Code);
				charge.J7_IsDutiable = false;
				charge.J7_IsIncludedInITOT = true;
				JobComInvoiceLine line = header.JobComInvoiceLines.Count > 0 ? header.JobComInvoiceLines[0] : header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "21344199";
				line.AddInfo.ZA_PermitNumbers_Hidden = "L111333";
				line.JI_Description = "Shirts";
				line.JI_LinePrice = 4000m;
				JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
				line2.JI_Tariff = "1111.11.11";
				line2.JI_Description = "This will retrieve a substring that begins with many characters from the begin of the string if the string is longer than 128 characters";
				line2.JI_LinePrice = 1000m;
				dec.ResumeApportionment();
				return new DeclarationEDNExporter(dec);
			}
		}

#if NETFRAMEWORK
		protected override string ExpectedCSVResult
		{
			get
			{
				return
					"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,12131212313,I am an importer,IN,20051126,AIR,QF492,21344199,Shirts,L111333,3680.00,AUBNE,\r\n" +
					"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,12131212313,I am an importer,IN,20051126,AIR,QF492,11111111,This will retrieve a substring that begins with many characters from the begin of the string if the string is longer than 128 ch,,920.00,AUBNE,\r\n";
			}
		}
#else
		protected override string ExpectedCSVResult
		{
			get
			{
				return
					"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,12131212313,I am an importer,IN,20051126,AIR,QF492,21344199,Shirts,L111333,3680.000,AUBNE,\r\n" +
					"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,12131212313,I am an importer,IN,20051126,AIR,QF492,11111111,This will retrieve a substring that begins with many characters from the begin of the string if the string is longer than 128 ch,,920.000,AUBNE,\r\n";
			}
		}
#endif
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			currentUserEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			currentyCompanyABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			GlbStaff.CurrentUser.GS_EmailAddress = "test@example.com";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "123 441";
		}

		protected override void TearDown()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = currentUserEmailAddress;
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = currentyCompanyABN;
			base.TearDown();
		}

		string currentUserEmailAddress;
		string currentyCompanyABN;

		#endregion
	}
}
