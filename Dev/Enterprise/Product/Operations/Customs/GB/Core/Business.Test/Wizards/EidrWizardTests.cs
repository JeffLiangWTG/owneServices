using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Wizards.EIDR.Testing
{
	class EidrWizardTests : TestCaseWithFactory
	{
		[TestDate(2010, 11, 12)]
		public void TestEidrWizardDefaults()
		{
			var dec = Factory.New<JobDeclaration>();
			var wizardManager = new EidrWizardManager(dec);

			CombineAssertions("EIDR Wizard defaults", () =>
			{
				AssertEquals("default CPC to 4000000", "4000000", wizardManager.EidrWizard.CPC);
				AssertEquals("invoice number", "", wizardManager.EidrWizard.InvoiceNumber);
				AssertEquals("default to today", "12-Nov-10 00:00:00", wizardManager.EidrWizard.DateOfImport.ToString());
				AssertEquals("additional reference numbers", "", wizardManager.EidrWizard.AdditionalReferenceNumbers);
				AssertEquals("warehouse", ZGuid.Empty, wizardManager.EidrWizard.WarehouseOrgAddressPk);
				AssertEquals("description of goods", "", wizardManager.EidrWizard.DescriptionOfGoods);
				AssertEquals("customs value in GBP", ZDecimal.Zero, wizardManager.EidrWizard.CustomsValueInGBP);
				AssertEquals("number of packages", ZInt.Zero, wizardManager.EidrWizard.NumberOfPackages);
				AssertEquals("type of packages", "", wizardManager.EidrWizard.TypeOfPackages);
				AssertEquals("net mass in KG", ZDecimal.Zero, wizardManager.EidrWizard.NetMassInKG);
				AssertEquals("default to branch org proxy", dec.Branch.OrgProxy.MainAddress.PK, wizardManager.EidrWizard.DeclarantOrgAddressPk);
				AssertEquals("transport mode", "", wizardManager.EidrWizard.TransportMode);
				AssertEquals("country of origin", "", wizardManager.EidrWizard.CountryOfOrigin);
				AssertEquals("EIDR type", "", wizardManager.EidrWizard.EIDRType);
				AssertEquals("default supplementary declaration due date to today plus 6 months", "12-May-11 00:00:00", wizardManager.EidrWizard.SupplementaryDeclarationDueDate.ToString());
			});
		}

		[TestDate(2010, 11, 12)]
		public void TestEidrWizardProperties()
		{
			var warehouse = Factory.NewWithValidTestData<OrgAddress>();
			var declarant = Factory.NewWithValidTestData<OrgAddress>();
			var dec = Factory.New<JobDeclaration>();
			var wizardManager = new EidrWizardManager(dec);

			wizardManager.EidrWizard.CPC = "CPC";
			wizardManager.EidrWizard.InvoiceNumber = "INV123";
			wizardManager.EidrWizard.DateOfImport = ZDate.Today.AddDays(1);
			wizardManager.EidrWizard.AdditionalReferenceNumbers = "ARN";
			wizardManager.EidrWizard.WarehouseOrgAddressPk = warehouse.PK;
			wizardManager.EidrWizard.DescriptionOfGoods = "DOG";
			wizardManager.EidrWizard.CustomsValueInGBP = 123.45m;
			wizardManager.EidrWizard.NumberOfPackages = 111;
			wizardManager.EidrWizard.TypeOfPackages = "BAG";
			wizardManager.EidrWizard.NetMassInKG = 69.96m;
			wizardManager.EidrWizard.DeclarantOrgAddressPk = declarant.PK;
			wizardManager.EidrWizard.TransportMode = "AIR";
			wizardManager.EidrWizard.CountryOfOrigin = "ZA";
			wizardManager.EidrWizard.EIDRType = "CFS";
			wizardManager.EidrWizard.SupplementaryDeclarationDueDate = ZDate.Today.AddDays(2);

			AssertEquals("CPC", wizardManager.EidrWizard.CPC);
			AssertEquals("INV123", wizardManager.EidrWizard.InvoiceNumber);
			AssertEquals("13-Nov-10 00:00:00", wizardManager.EidrWizard.DateOfImport.ToString());
			AssertEquals("ARN", wizardManager.EidrWizard.AdditionalReferenceNumbers);
			AssertEquals(warehouse.PK, wizardManager.EidrWizard.WarehouseOrgAddressPk);
			AssertEquals("DOG", wizardManager.EidrWizard.DescriptionOfGoods);
			AssertEquals(123.45m, wizardManager.EidrWizard.CustomsValueInGBP);
			AssertEquals(111, wizardManager.EidrWizard.NumberOfPackages);
			AssertEquals("BAG", wizardManager.EidrWizard.TypeOfPackages);
			AssertEquals(69.96m, wizardManager.EidrWizard.NetMassInKG);
			AssertEquals(declarant.PK, wizardManager.EidrWizard.DeclarantOrgAddressPk);
			AssertEquals("AIR", wizardManager.EidrWizard.TransportMode);
			AssertEquals("ZA", wizardManager.EidrWizard.CountryOfOrigin);
			AssertEquals("CFS", wizardManager.EidrWizard.EIDRType);
			AssertEquals("14-Nov-10 00:00:00", wizardManager.EidrWizard.SupplementaryDeclarationDueDate.ToString());
		}

		[TestDate(2010, 11, 12)]
		public void TestCreateEidrWizard()
		{
			var warehouse = Factory.NewWithValidTestData<OrgAddress>();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var wizardManager = new EidrWizardManager(dec);
			wizardManager.EidrWizard.InvoiceNumber = "INV123";
			wizardManager.EidrWizard.AdditionalReferenceNumbers = "Reference1, Reference2, Reference3";
			wizardManager.EidrWizard.WarehouseOrgAddressPk = warehouse.PK;
			wizardManager.EidrWizard.DescriptionOfGoods = "Super Furry Animals";
			wizardManager.EidrWizard.CustomsValueInGBP = 111.11m;
			wizardManager.EidrWizard.NumberOfPackages = 99;
			wizardManager.EidrWizard.TypeOfPackages = "BOX";
			wizardManager.EidrWizard.NetMassInKG = 222.22m;
			wizardManager.EidrWizard.TransportMode = "SEA";
			wizardManager.EidrWizard.CountryOfOrigin = "ZA";
			wizardManager.EidrWizard.EIDRType = "CFS";

			wizardManager.CreateEidr();

			AssertEquals("Defaulted", "IMP", dec.JE_MessageType);
			AssertEquals("Defaulted", "ISD", dec.JE_DeclarationType);
			AssertEquals("Defaulted", "Z", dec.JE_EntrySubStyle);
			AssertEquals("SEA", dec.JE_TransportMode);
			AssertEquals("Super Furry Animals", dec.JE_GoodsDescription);
			AssertEquals("Defaulted to today", "12-Nov-10 00:00:00", dec.JE_EntryAuthorisationDate.ToString());
			AssertEquals("Defaulted to today", "12-Nov-10 00:00:00", dec.JE_DateOfArrival.ToString());
			AssertEquals(222.22m, dec.JE_TotalWeight);
			AssertEquals("ZA", dec.JE_GoodsOrigin);
			AssertEquals(99, dec.JE_TotalNoOfPacks);
			AssertEquals("BOX", dec.JE_TotalNoOfPacksPackType);
			AssertEquals("CFS", dec.JE_EidrType);
			AssertEquals("Defaulted to declarant org proxy address", "EDI CUSTOMS BROKERS 10 HUTCHESON STREET ALBION QLD 4010 AUSTRALIA", dec.Declarant.AddressAsASingleLine);
			AssertEquals("Defaulted to 6 months from today", "12-May-11 00:00:00", dec.JE_SuppDecDueDate.ToString());
			AssertEquals(warehouse.PK, dec.WarehouseDocAddress.E2_OA_Address);
			AssertEquals(1, dec.Invoices.Count);
			AssertEquals(1, dec.InvoiceLines.Count);

			var cusEntryInstruction = dec.CusEntryInstruction;
			AssertEquals(warehouse.PK, cusEntryInstruction.CEI_OA_Warehouse2);

			var invoiceHeader = dec.Invoices[0];
			AssertEquals("INV123", invoiceHeader.JZ_InvoiceNumber);
			AssertEquals(111.11m, invoiceHeader.JZ_InvoiceAmount);
			AssertEquals("GBP", invoiceHeader.JZ_RX_NKInvoice_Currency);

			var invoiceLine = dec.InvoiceLines[0];
			AssertEquals("4000000", invoiceLine.JI_Procedure);
			AssertEquals("Super Furry Animals", invoiceLine.JI_Description);
			AssertEquals("ZA", invoiceLine.JI_CountryOfOrigin);
			AssertEquals(111.11m, invoiceLine.JI_LinePrice);
			AssertEquals(222.22m, invoiceLine.JI_NetWeight);
			AssertEquals("KG", invoiceLine.JI_WeightUQ);

			var previousDocs = invoiceLine.PreviousDocuments;
			AssertEquals(3, previousDocs.Count);
			AssertEquals("Reference1", previousDocs[0].CSI_ReferenceNumber);
			AssertEquals("ZZZ", previousDocs[0].CSI_Code);
			AssertEquals("Z", previousDocs[0].CSI_SubType);
			AssertEquals("Reference2", previousDocs[1].CSI_ReferenceNumber);
			AssertEquals("ZZZ", previousDocs[1].CSI_Code);
			AssertEquals("Z", previousDocs[1].CSI_SubType);
			AssertEquals("Reference3", previousDocs[2].CSI_ReferenceNumber);
			AssertEquals("ZZZ", previousDocs[2].CSI_Code);
			AssertEquals("Z", previousDocs[2].CSI_SubType);
		}
	}

	[TestedType(typeof(EidrWizard))]
	public class EidrWizardNPBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			return new EidrWizard(dec);
		}
	}
}
