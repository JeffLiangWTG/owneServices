using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ConsolEDNExporter))]
	public class ConsolEDNExporterSEATest : CMRDataExporterCSVTest
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

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_LloydsNumber = "1231234";
			vessel.RV_Code = "VESS";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_UniqueConsignRef = "C99119911";

			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2005, 10, 18);
			transport.JW_VoyageFlight = "1234";
			transport.JW_Vessel = "VESS";
			transport.JW_VoyageFlight = "1234";

			CommonShipment ship1 = consol.Shipments.AddNew();
			ship1.JS_UniqueConsignRef = "S1";

			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			dec1.JE_JS = ship1.PK;
			dec1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec1.JE_DeclarationReference = "B123123";
			dec1.JE_MasterBill = "M121313";
			dec1.JE_HouseBill = "H3255";
			dec1.JE_OH_Importer = importer.PK;
			dec1.JE_OH_Supplier = supplier.PK;
			dec1.JE_RL_NKFinalDestination = "INBOM";
			dec1.JE_ExportDate = new ZDateTime(2005, 11, 26);
			dec1.JE_GoodsDescription = "Shirts and shorts";
			dec1.JE_RL_NKPortOfLoading = "AUBNE";

			CommonShipment ship2 = consol.Shipments.AddNew();
			ship2.CustomsEntryNumberType = "EXDC";
			ship2.CustomsEntryNumber = "123";
			ship2.JS_UniqueConsignRef = "S2";

			JobDeclaration dec2 = Factory.New<JobDeclaration>();
			dec2.JE_JS = ship2.PK;
			dec2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec2.JE_DeclarationReference = "B235235";
			dec2.JE_MasterBill = "M325235";
			dec2.JE_HouseBill = "H222";
			dec2.JE_OH_Importer = importer.PK;
			dec2.JE_OH_Supplier = supplier.PK;
			dec2.JE_RL_NKFinalDestination = "INBOM";
			dec2.JE_ExportDate = new ZDateTime(2005, 11, 26);
			dec2.JE_GoodsDescription = "Just shorts";
			dec2.JE_RL_NKPortOfLoading = "AUBNE";

			JobComInvoiceGroupHeader groupHeader = dec2.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader header = groupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "2134.41.99";
			line.AddInfo.ZA_PermitNumbers_Hidden = "L111333";
			line.JI_LinePrice = 123.45m;
			line.JI_Description = "Pink shorts";

			JobComInvoiceHeader header2 = groupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line2 = header2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "1213.43.55";
			line2.AddInfo.ZA_PermitNumbers_Hidden = "L325235235";
			line2.JI_LinePrice = 666.66m;
			line2.JI_Description = "Purple shorts";

			JobComInvoiceLine line3 = header2.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "98784444";
			line3.AddInfo.ZA_PermitNumbers_Hidden = "L00001111";
			line3.JI_LinePrice = 444.44m;
			line3.JI_Description = "Lime shorts";

			CommonShipment ship3 = consol.Shipments.AddNew();
			ship3.CustomsEntryNumberType = "CAN";
			ship3.CustomsEntryNumber = "123";
			ship3.JS_UniqueConsignRef = "S3";

			JobDeclaration dec3 = Factory.New<JobDeclaration>();
			dec3.JE_JS = ship3.PK;
			dec3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec3.JE_DeclarationReference = "B235236";
			dec3.JE_MasterBill = "M325235";
			dec3.JE_HouseBill = "H333";
			dec3.JE_OH_Importer = importer.PK;
			dec3.JE_OH_Supplier = supplier.PK;
			dec3.JE_RL_NKFinalDestination = "INBOM";
			dec3.JE_ExportDate = new ZDateTime(2005, 11, 26);
			dec3.JE_GoodsDescription = "Just shorts";
			dec3.JE_RL_NKPortOfLoading = "AUBNE";

			JobComInvoiceGroupHeader groupHeader2 = dec3.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader header3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line4 = header3.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "77777777";
			line4.JI_LinePrice = 777m;
			line4.JI_Description = "Peach shorts";

			CommonShipment ship4 = consol.Shipments.AddNew();
			ship4.JS_UniqueConsignRef = "S4";

			JobDeclaration dec4 = Factory.New<JobDeclaration>();
			dec4.JE_JS = ship4.PK;
			dec4.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec4.JE_DeclarationReference = "B235236";
			dec4.JE_MasterBill = "M325235";
			dec4.JE_HouseBill = "H888";
			dec4.JE_OH_Importer = importer.PK;
			dec4.JE_OH_Supplier = supplier.PK;
			dec4.JE_RL_NKFinalDestination = "INBOM";
			dec4.JE_ExportDate = new ZDateTime(2005, 11, 26);
			dec4.JE_GoodsDescription = "Just shorts";
			dec4.JE_RL_NKPortOfLoading = "AUMEL";

			JobComInvoiceGroupHeader groupHeader3 = dec4.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader header4 = groupHeader3.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line5 = header4.JobComInvoiceLines.AddNew();
			line5.JI_Tariff = "88888888";
			line5.JI_LinePrice = 888m;
			line5.JI_Description = "Lemon shorts";

			return new ConsolEDNExporter(consol);
		}

#if NETFRAMEWORK
		protected override string ExpectedCSVResult
		{
			get
			{
				return
"Eagle Datamation International,123 441,test@example.com,S2,M325235,H222,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,21344199,Pink shorts,L111333,123.45,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,S2,M325235,H222,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,12134355,Purple shorts,L325235235,666.66,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,S2,M325235,H222,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,98784444,Lime shorts,L00001111,444.44,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,S4,M325235,H888,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,88888888,Lemon shorts,,888.00,AUMEL,1234\r\n";
			}
		}
#else
		protected override string ExpectedCSVResult
		{
			get
			{
				return
"Eagle Datamation International,123 441,test@example.com,S2,M325235,H222,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,21344199,Pink shorts,L111333,123.450,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,S2,M325235,H222,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,12134355,Purple shorts,L325235235,666.660,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,S2,M325235,H222,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,98784444,Lime shorts,L00001111,444.440,AUBNE,1234\r\n" +
"Eagle Datamation International,123 441,test@example.com,S4,M325235,H888,I am a supplier,AAA3336347E,I am an importer,IN,20051126,SEA,1231234,88888888,Lemon shorts,,888.000,AUMEL,1234\r\n";
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
