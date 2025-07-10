using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class CommodityTypeWithGrossMassProviderTest : CommodityTypeProviderTest<CommodityTypeWithGrossMassProvider>
	{
		public void TestGrossMass()
		{
			var line1 = invoiceLines[0];
			line1.JI_Weight = 10m;
			line1.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			var line2 = invoiceLines[1];
			line2.JI_Weight = 100m;
			line2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("GrossMass", 10100m, Provider.GrossMass);
		}

		public void TestCommodity_GrossMassOnlyOnMainPackEntryLine()
		{
			(var secondEntryLineWrapper, var secondInvoiceLine) = MessageProviderTestHelper.SetupSecondLine(entryHeaderWrapper);
			var entryLine = entryLineWrapper.EntryLine;
			entryLine.CL_LineNumber = 1;
			var secondEntryLine = secondEntryLineWrapper.EntryLine;
			secondEntryLine.CL_LineNumber = 2;
			var declaration = entryLineWrapper.Declaration;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 1;
			var packingGroup = declaration.PackingGroups[0];
			packingGroup.Packages.RemoveAndDeleteAll();
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var invoiceLine = invoiceLines[0];
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			invoiceLine.JI_Weight = 40m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 35m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			secondInvoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			secondInvoiceLine.JI_Weight = 0.060m;
			secondInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			secondInvoiceLine.JI_NetWeight = 55m;
			secondInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			var invoiceLine3 = invoiceLines[1];
			invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			invoiceLine3.JI_Weight = 55000m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Grams;
			invoiceLine3.JI_NetWeight = 35m;
			invoiceLine3.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.ZG_IsMainPack = ZBool.True;
			secondInvoiceLine.ZG_IsMainPack = ZBool.False;
			invoiceLine3.ZG_IsMainPack = ZBool.False;
			var provider1 = new CommodityTypeWithGrossMassProvider(entryLineWrapper);
			var provider2 = new CommodityTypeWithGrossMassProvider(secondEntryLineWrapper);
			AssertEquals("1 - GrossMass", 155m, provider1.GrossMass);
			AssertEquals("2 - GrossMass", 0m, provider2.GrossMass);
		}

		// Add UTs for new added IE613 properties here.

		protected override CommodityTypeWithGrossMassProvider GetProvider() => new CommodityTypeWithGrossMassProvider(entryLineWrapper);
	}
}
