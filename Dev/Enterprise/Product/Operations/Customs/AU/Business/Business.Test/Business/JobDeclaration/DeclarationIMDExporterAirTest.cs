using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeclarationIMDExporter))]
	sealed class DeclarationIMDExporterAirTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "IMD"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Import Declaration"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override string ExpectedBodyText
		{
			get
			{
				return DeclarationIMDExporter.RevenueUndertaking;
			}
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "I am an importer";
			importer.PrimaryRegistrationNumber.Number = "12131212313";
			importer.MainAddress.OA_Address1 = "18 Henricks Avenue,";
			importer.MainAddress.OA_City = "Newington,";
			importer.MainAddress.OA_State = "NSW";
			importer.MainAddress.OA_PostCode = "2127";

			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "I am a supplier";
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "AAA3336347E");

			OrgHeader depot = OrgHeader.New(Factory);
			depot.OH_FullName = "I am a depot";
			OrgAddress depotAddress = depot.Addresses.AddNew();
			depot.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A");

			OrgHeader cTO = OrgHeader.New(Factory);
			cTO.OH_FullName = "I am a CTO";
			OrgAddress cTOAddress = cTO.Addresses.AddNew();
			cTO.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "5678X");

			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123123";
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			dec.JE_MasterBill = "08112344321";
			dec.JE_HouseBill = "H3255";
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OH_Supplier = supplier.PK;
			dec.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			dec.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;

			dec.JE_RL_NKPortOfArrival = "AUSYD";
			dec.JE_RL_NKFinalDestination = "AUMEL";
			dec.JE_VoyageFlightNo = "QF492";
			dec.JE_GoodsDescription = "Shirts and shorts";
			dec.JE_RL_NKPortOfLoading = "USLAX";
			dec.JE_OwnerRef = "IMPORTER REF";
			dec.JE_MessageSubType = "SAC";
			Package pack1 = dec.Packages.AddNew();
			Package pack2 = dec.Packages.AddNew();

			JobComInvoiceGroupHeader groupHeader = dec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader header = groupHeader.JobComInvoiceHeaders.AddNew();

			CusEntryHeader entryHeader = dec.CustomsEntryHeaders.AddNew();

			var mockLine1 = Factory.NewMoq<CusEntryLine>();
			mockLine1.Setup(m => m.CustomsValue).Returns(new Money(1234.56m, JobDeclaration.GetLocalCurrency()));
			mockLine1.Setup(m => m.DutyAmount).Returns(333m);
			mockLine1.Setup(m => m.GSTVATAmount).Returns(444.44m);
			var entryLine1 = mockLine1.Object;
			entryHeader.MergedLines.Add(entryLine1);
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			line1.AddInfo.ZA_ORG = "US";
			line1.JI_InvoiceQuantity = 1m;
			line1.JI_Tariff = "1234.12.12 99";
			line1.JI_CL = entryLine1.PK;
			entryLine1.CL_AdValoremTariff = "1234.12.12 99";
			entryLine1.CL_Description = "Shirts";
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine1.PK;
			line2.AddInfo.ZA_ORG = "US";
			line2.JI_InvoiceQuantity = 2m;
			line2.JI_Tariff = "1234.12.12 99";
			line2.JI_CL = entryLine1.PK;

			var mockLine2 = Factory.NewMoq<CusEntryLine>();
			mockLine2.Setup(m => m.CustomsValue).Returns(new Money(3m, JobDeclaration.GetLocalCurrency()));
			mockLine2.Setup(m => m.DutyAmount).Returns(2m);
			mockLine2.Setup(m => m.GSTVATAmount).Returns(1m);
			CusEntryLine entryLine2 = mockLine2.Object;
			entryHeader.MergedLines.Add(entryLine2);
			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine2.PK;
			line3.AddInfo.ZA_ORG = "CA";
			line3.JI_InvoiceQuantity = 44m;
			line3.JI_Tariff = "9999.99.99 99";
			line3.JI_CL = entryLine2.PK;
			entryLine2.CL_AdValoremTariff = "9999.99.99 99";
			entryLine2.CL_Description = "This will retrieve a substring that begins with many characters from the begin of the string if the string is longer than 128 characters";

			return new DeclarationIMDExporter(dec);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
" ,QF492,08112344321,H3255,,,Shirts,I am a supplier,I am an importer,18 Henricks Avenue  Newington NSW 2127,AUSYD,,YES,12131212313,I am an importer,IMPORTER REF,,AIR,1234A,12341212,99,US,AAA3336347E,1234.56,333.00,444.44,test@example.com,99 123 441 555,Eagle Datamation International,3\r\n" +
" ,QF492,08112344321,H3255,,,This will retrieve a substring that begins with many characters from the begin of the string if the string is longer than 128 ch,I am a supplier,I am an importer,18 Henricks Avenue  Newington NSW 2127,AUSYD,,YES,12131212313,I am an importer,IMPORTER REF,,AIR,1234A,99999999,99,CA,AAA3336347E,3.00,2.00,1.00,test@example.com,99 123 441 555,Eagle Datamation International,44\r\n";
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			currentUserEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			currentyCompanyABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			GlbStaff.CurrentUser.GS_EmailAddress = "test@example.com";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "99 123 441 555";
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
