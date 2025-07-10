using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class SingleLineEntryManagerTest : TestCaseWithFactory
	{
		public void TestCPCCode()
		{
			var dec = Factory.New<JobDeclaration>();
			var manager = new SingleLineEntryManager(dec);
			manager.Execute();
			AssertEquals("1000001", dec.InvoiceLines[0].JI_Procedure);
			dec.JE_MessageType = "IMP";
			manager = new SingleLineEntryManager(dec);
			manager.Execute();
			AssertEquals("4000000", dec.InvoiceLines[0].JI_Procedure);
		}

		public void TestDescription()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_GoodsDescription = "POO";
			SingleLineEntryManager manager = new SingleLineEntryManager(dec);
			manager.Execute();
			AssertEquals("POO", dec.InvoiceLines[0].JI_Description);
		}

		public void TestGrossMassFromDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TotalWeight = 123m;
			dec.JE_TotalWeightUnit = "LB";
			SingleLineEntryManager manager = new SingleLineEntryManager(dec);
			manager.Execute();
			AssertEquals(123m, dec.InvoiceLines[0].JI_Weight);
			AssertEquals("LB", dec.InvoiceLines[0].JI_WeightUQ);
		}

		public void TestPackagesFromDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TotalNoOfPacks = 123;
			dec.JE_TotalNoOfPacksPackType = "PK";

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "XX";
			pack1.RP_Type = "CIP";
			pack1.RP_CustomsCountry = dec.CountryCode;
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = "PK";

			Factory.Save();

			SingleLineEntryManager manager = new SingleLineEntryManager(dec);
			manager.Execute();

			var pivot = (InvoiceLinePackagePivot)dec.InvoiceLines[0].PackagesPivot.ToArray()[0];
			AssertEquals(123, pivot.CHC_NumberOfPacks);
			AssertEquals("XX", pivot.Package.CW_PackType);
		}

		public void TestNoMessageErrorFromPackagesOnTariff()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MasterBill = "M";
			var cw = dec.Bills[0].PackingGroups[0].Packages.AddNew();
			cw.CW_PackQty = 1;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			dec.JE_TotalNoOfPacks = 123;
			dec.JE_TotalNoOfPacksPackType = "PK";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "packaging details");

			invoiceLine.RemoveRowNotification(invoiceLine.RowNotifications.GetMessageErrors().FirstOrDefault(x => x.Message.StartsWith("This line has no packaging details.")));

			SingleLineEntryManager manager = new SingleLineEntryManager(dec);
			manager.Execute();
			AssertNoRowMessageErrorContaining(invoiceLine, "packaging details");
		}

		public void TestBox44NoLIC99()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			var manager = new SingleLineEntryManager(dec);
			manager.Execute();
			AssertEquals(0, dec.InvoiceLines[0].AdditionalInfos.Count);
			manager.SingleLineEntry.CreateLIC99 = true;
			manager.Execute();
			AssertEquals(1, dec.InvoiceLines[0].AdditionalInfos.Count);
		}

		public void TestCurrency()
		{
			var dec = Factory.New<JobDeclaration>();
			SingleLineEntryManager manager = new SingleLineEntryManager(dec);
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			manager.SingleLineEntry.Currency = uSD.PK;
			manager.Execute();
			AssertEquals(uSD.RX_Code, dec.Invoices[0].JZ_RX_NKInvoice_Currency);
		}

		public void TestNetWeight()
		{
			var dec = Factory.New<JobDeclaration>();
			SingleLineEntryManager manager = new SingleLineEntryManager(dec);

			CombineAssertions(() =>
			{
				manager.SingleLineEntry.NetWeight = 123.4m;
				manager.Execute();
				AssertEquals("less than 3 decimal places digit", 123.4m, dec.InvoiceLines[0].JI_NetWeight);

				manager.SingleLineEntry.NetWeight = 2.123m;
				manager.Execute();
				AssertEquals("3 decimal places digit", 2.123m, dec.InvoiceLines[0].JI_NetWeight);

				manager.SingleLineEntry.NetWeight = 3.1235;
				manager.Execute();
				AssertEquals("more than 3 decimal places digit - value rounded", 3.124m, dec.InvoiceLines[0].JI_NetWeight);
			});
		}

		public void TestCurrencyDefaulting()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";
			var supplier = Factory.New<OrgHeader>();

			RefCurrency jPY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");

			OrgSupplierBuyerLink link1 = supplier.BuyerLinks.AddNew(importer);
			link1.OL_RX_NKDefaultCurrency = jPY.RX_Code;
			OrgSupplierBuyerLink link2 = supplier.BuyerLinks.AddNew(importer);
			link2.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Germany;
			link2.OL_RX_NKDefaultCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OH_Supplier = supplier.PK;
			SingleLineEntryManager manager = new SingleLineEntryManager(dec);
			AssertEquals(jPY.PK, manager.SingleLineEntry.Currency);

			dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Germany;
			dec.JE_OH_Supplier = supplier.PK;
			manager = new SingleLineEntryManager(dec);
			AssertEquals(Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates).PK, manager.SingleLineEntry.Currency);
		}

		public void TestPackTypeConversion()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TotalNoOfPacks = 600;
			dec.JE_TotalNoOfPacksPackType = "BOX";
			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = "BOX";
			refPack.RP_CustomsPack = "BX";
			refPack.RP_ConversionFactor = 1m;
			SingleLineEntryManager manager = new SingleLineEntryManager(dec);
			manager.Execute();
			var pivot = (InvoiceLinePackagePivot)dec.InvoiceLines[0].PackagesPivot.ToArray()[0];
			AssertEquals("BX", pivot.Package.CW_PackType);
		}

		public void TestExecutedSuccessfully()
		{
			var dec = Factory.New<JobDeclaration>();
			var manager = new SingleLineEntryManager(dec);
			AssertEquals(false, manager.ExecutedSuccessfully);
			manager.Execute();
			AssertEquals(true, manager.ExecutedSuccessfully);
		}
	}
}
