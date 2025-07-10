using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPEDeclarationFromAirCargoCreatorTest : TestCaseWithFactory
	{
		public void TestCreateCore()
		{
			UPECusMAWB.CM_ArrivalDate = new ZDateTime(1999, 10, 8);
			HouseAirCargo.CS_RS_NK_ServiceLevel = "1";
			HouseAirCargo.CS_HAWB = "12345678901234567890";
			HouseAirCargo.WayBillShort = "98765432101";
			HouseAirCargo.CS_Weight = 10;
			HouseAirCargo.CS_WeightUQ = Core.Constants.Weight.Pounds;
			JobDeclaration declaration = (JobDeclaration)Creator.CreateIgnoreWarnings();
			AssertEquals("Should be trimmed to 18 characters", "123456789012345678", declaration.JE_OwnerRef);
			AssertEquals("98765432101", declaration.JE_AgentsReference);
			AssertEquals(Core.Constants.IncoTerms.FreeOnBoard, declaration.JE_ShipmentIncoTerm);
			AssertEquals("1", declaration.JE_RS_NKServiceLevel);
			AssertEquals(Core.Constants.Weight.Kilograms, declaration.JE_TotalWeightUnit);
			AssertEquals(4.5m, declaration.JE_TotalWeight);
			AssertEquals(new ZDateTime(2004, 4, 19), declaration.JE_DateAtOrigin);
			AssertEquals(new ZDateTime(2004, 4, 19), declaration.JE_ExportDate);
			AssertEquals(new ZDateTime(1999, 10, 8), declaration.JE_DateOfFirstArrival);
		}

		public void TestImportInvoiceLines()
		{
			HouseAirCargo.CS_HAWB = "1Z3824AR6640806327";
			HouseAirCargo.CS_Weight = 10;
			HouseAirCargo.CS_WeightUQ = Core.Constants.Weight.Pounds;
			JobDeclaration declaration = (JobDeclaration)Creator.CreateIgnoreWarnings();
			AssertEquals(1, declaration.Invoices.Count);
			ZQuery invoiceNumberFilter = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "1Z3824AR6640806327");
			JobComInvoiceHeader invoice1Z3824AR6640806327 = (JobComInvoiceHeader)(new List<BaseJobComInvoiceHeader>(declaration.Invoices.Find(invoiceNumberFilter))[0]);
			AssertEquals(Core.Constants.IncoTerms.FreeOnBoard, invoice1Z3824AR6640806327.JZ_IncoTerm);
			AssertEquals(4, invoice1Z3824AR6640806327.JobComInvoiceLines.Count);
			AssertEquals("USD", invoice1Z3824AR6640806327.Invoice_Currency.RX_Code);
			AssertEquals(4.5m, invoice1Z3824AR6640806327.JZ_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, invoice1Z3824AR6640806327.JZ_WeightUQ);
			ZQuery partNumberFilter = new ZQuery(JobComInvoiceLineSchema.JI_PartNo, "7783461");
			JobComInvoiceLine invLine = (JobComInvoiceLine)invoice1Z3824AR6640806327.JobComInvoiceLines.Find(partNumberFilter)[0];
			AssertEquals(1m, invLine.JI_InvoiceQuantity);
			AssertEquals(Core.Constants.PkgUnit.Piece, invLine.JI_InvoiceUQ);
			AssertEquals("THE MOST COMFORTABLE TSHIRT EVER! OUR 100% COTTON HANES BEEFYT IS PRESHRUNK DURABLE AND GUARANTEE", invLine.JI_Description);
			AssertEquals(13.99m, invLine.JI_LinePrice);
			AssertEquals("0100", invLine.JI_Tariff);
			AssertEquals("US", invLine.CountryOfOrigin.Code);
		}

		public void TestConsignee_ComesFromImporterIfSpecified()
		{
			OrgHeader consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			HouseAirCargo.RequiresConsigneeMatch = true;
			HouseAirCargo.UPEConsigneeAddress.P3_OH_MatchOrg = consigneeOrg.PK;
			BaseJobDeclaration newDeclarationWithoutImporter = Creator.CreateIgnoreWarnings();
			AssertEquals("Consignee should be used as the consignee on the dec if no importer exists", consigneeOrg.PK, newDeclarationWithoutImporter.JE_OH_Importer);
			CreateNewAirCargoImporterAddress(HouseAirCargo);
			HouseAirCargo.UPEImporterAddress.P3_OH_MatchOrg = importerOrg.PK;
			BaseJobDeclaration newDeclarationWithAImporter = Creator.CreateIgnoreWarnings();
			AssertEquals("Consignee should be used when one is approved", consigneeOrg.PK, newDeclarationWithAImporter.JE_OH_Importer);
		}

		public void TestAddAdditionalInvoiceLines()
		{
			HouseAirCargo.CS_Weight = 10;
			HouseAirCargo.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			JobDeclaration declaration = (JobDeclaration)Creator.CreateIgnoreWarnings();
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals("FIRSTHOUSE", declaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals(10m, declaration.Invoices[0].JZ_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, declaration.Invoices[0].JZ_WeightUQ);
			AssertEquals(4, declaration.Invoices[0].JobComInvoiceLines.Count);
			Creator.AddAdditionalInvoiceLines("SECONDHOUSE", 10.55m, Core.Constants.Weight.Kilograms, null, null);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(4, declaration.Invoices[0].JobComInvoiceLines.Count);
			Creator.AddAdditionalInvoiceLines("SECONDHOUSE", 11.44m, Core.Constants.Weight.Pounds, declaration, null);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(4, declaration.Invoices[0].JobComInvoiceLines.Count);
			_500000LineCollection a500000LineCollection = new _500000LineCollection();
			a500000LineCollection.Add(new _500000Line("US2795AU9639040422              DE01E70GJRGB60000065744051475    5  LBS         USD         USD          USD                             C0000051867QF12            NN1ZE01E706643404080                 5      AKE35125QF                                                                                                                                                                "));
			Creator.AddAdditionalInvoiceLines("SECONDHOUSE", 10.55m, Core.Constants.Weight.Kilograms, null, a500000LineCollection);
			AssertEquals(1, declaration.Invoices.Count);
			Creator.AddAdditionalInvoiceLines("SECONDHOUSE", 13.75m, Core.Constants.Weight.Pounds, declaration, a500000LineCollection);
			AssertEquals(2, declaration.Invoices.Count);
			AssertEquals("SECONDHOUSE", declaration.Invoices[1].JZ_InvoiceNumber);
			AssertEquals(13.75m, declaration.Invoices[1].JZ_Weight);
			AssertEquals(Core.Constants.Weight.Pounds, declaration.Invoices[1].JZ_WeightUQ);
			AssertEquals(1, declaration.Invoices[1].JobComInvoiceLines.Count);
		}

		#region Implementation
		OrgPatternMatchAddress CreateNewAirCargoImporterAddress(UPECusHAWB airCargo)
		{
			OrgPatternMatchAddress result = Factory.New<OrgPatternMatchAddress>();
			result.P3_ParentID = airCargo.PK;
			result.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			return result;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			UPECusMAWB = Factory.New<UPECusMAWB>();
			HouseAirCargo = Factory.New<UPECusHAWB>();
			HouseAirCargo.CS_CM = UPECusMAWB.PK;
			HouseAirCargo.CS_HAWB = "FIRSTHOUSE";
			SetLevel1Record(HouseAirCargo);
			Creator = new UPEDeclarationFromAirCargoCreator(HouseAirCargo);
		}

		UPECusHAWB HouseAirCargo;
		UPECusMAWB UPECusMAWB;
		UPEDeclarationFromAirCargoCreator Creator;
		void SetLevel1Record(UPECusHAWB uPECusHAWB)
		{
			Level1Record level1Record = new Level1Record();
			level1Record.AddRecordLines(new ZString[] { "US2795AU9639040422              D3824ARFY9JH2000003824ARFY9JH           N 1 19   LBS US          USDNNNN           USDAKE32156QF    09642             USD         USD5100      USDN0 N   NEDI  19APR200419 LBS         USDD1    NNN NN  NN  N USD           USD    T1                      19APR20040000           5100       P/PNRE       19   LBSNNC0000051901QF12            N N 1   N ", "US2795AU9639040422              D3824ARFY9JH2020001Z3824AR6640806327                 19     19     2    N                                                            7709705048E                AU09639  S1AU9639TF.113B2004-04-21                              Y                                                                             AUDNNNNNBI                                  ", "US2795AU9639040422              D3824ARFY9JH300000073706003824AR    BIOTECH CORP                       107 OAKWOOD DRIVE                                                     GLASTONBURY                                            CT060332481US 18606338111                                                                               11644                                         ", "US2795AU9639040422              D3824ARFY9JH400000170520138AU0495926SNOWSILL, SONYA                                             15 LEANDER ST                                                         FALCON                                                   6210     AU 0895343637                  SNOWSILL    SNOWSILL                                                 237           ", "US2795AU9639040422              DA4W82133K3G401000        8AU0596227WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789                                                                                        237           ", "US2795AU9639040422              D4A14T9J3YYD5000001   EA THE MOST COMFORTABLE TSHIRT EVER! OUR 100% COTTON HANES BEEFYT IS PRESHRUNK DURABLE AND GUARANTEE       1399      USD7906969             US0100                               AU7783461                                                                                                                                          ", "US2795AU9639040422              D4A14T9J3YYD5010002   PK OUR SUPER SOFT 100% COMBED COTTON RIBBED BABY DOLL TSHIRT FROM AMERICAN APPAREL WILL KEEP YOU IN ST     1699      USD7906969             US                                   AU7784328                                                                                                                                          ", "US2795AU9639040422              D4A14T9J3YYD5050003   EA ENJOY THE COMFORT OF OUR ROOMY 100% COTTON OPEN FLY BOXERS FROM ROBINSON APPAREL.  GREAT FOR UNDERWE    1299      USD7906970             US                                   AU10414834                                                                                                                                         ", "US2795AU9639040422              D4A14T9J3YYD5060004   EA THE PERFECT CASUAL WEAR FOR THE OFFICE OUR ANVIL GOLF SHIRTS ARE MADE OF 100% PRESHRUNK HEAVYWEIGHT     1699      USD7906969             US                                   AU6633334                                                                                                                                          ", "US2795AU9639040422              D3947808NNSR60000099999999999    11 LBS         USD         USD          USD                             C0000092779UPS6901         NY1ZAT27736792092649                 10     AAY89758UPS                                                                                                                                                               " });
			HouseAirCargo.Level1Record = level1Record;
		}
		#endregion
	}
}
