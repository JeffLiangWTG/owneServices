using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeclarationEDNExporter))]
	sealed class DeclarationEDNExporterSeaTest : CMRDataExporterCSVTest
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
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "I am an importer";

			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "I am a supplier";
			supplier.MiscServ.OM_RX_NKEXDefCurrency = JobDeclaration.LocalCurrencyConstantCode;
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "AAA3336347E");
			var cusCode = supplier.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_LloydsNumber = "1231234";
			vessel.RV_Code = "VESS";

			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123123";
			dec.JE_MasterBill = "M121313";
			dec.JE_HouseBill = "H3255";
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_RL_NKFinalDestination = "INBOM";
			dec.JE_ExportDate = new ZDateTime(2005, 11, 26);
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec.JE_VoyageFlightNo = "1234";
			dec.JE_VesselName = "VESS";
			dec.JE_GoodsDescription = "Shirts and shorts";
			dec.JE_RL_NKPortOfLoading = "AUBNE";

			JobComInvoiceGroupHeader groupHeader = dec.JobComInvoiceGroupHeaders.Count > 0 ? dec.JobComInvoiceGroupHeaders[0] : dec.JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceHeader header = groupHeader.JobComInvoiceHeaders.Count > 0 ? groupHeader.JobComInvoiceHeaders[0] : groupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.Count > 0 ? header.JobComInvoiceLines[0] : header.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "21344199";
			line.AddInfo.ZA_PermitNumbers_Hidden = "L111333";
			line.JI_Description = "Shirts";
			line.JI_LinePrice = 1234.56m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "1111.11.11";
			line2.JI_Description = "Shorts";
			line2.JI_LinePrice = 999.99m;
			return new DeclarationEDNExporter(dec);
		}

#if NETFRAMEWORK
		protected override string ExpectedCSVResult
		{
			get
			{
				return
"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,21344199,Shirts,L111333,1234.56,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,11111111,Shorts,,999.99,AUBNE,1234\r\n";
			}
		}
#else
		protected override string ExpectedCSVResult
		{
			get
			{
				return
"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,21344199,Shirts,L111333,1234.560,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,B123123,M121313,H3255,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,11111111,Shorts,,999.990,AUBNE,1234\r\n";
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
