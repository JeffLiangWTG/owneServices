using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PackagesLimitHelperTest : TestCaseWithFactory
	{
		public void TestCheckPackageUnitCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MasterBill = "MB1";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			declaration.Packages.RemoveAndDeleteAll();
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Bag;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BaleCompressed;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BaleUncompressed;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Basket;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Bottle;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Box;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BreakBulk;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BulkBag;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Bundle;
			var package10 = declaration.Packages.AddNew();
			package10.CW_PackType = Core.Constants.PkgUnit.Carton;

			var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;

			npbos.OfType<BaseCusLinkPackage>().ForEach(x => x.IsLinked = true);
			PackagesLimitHelper.CheckPackageUnitCount(invoiceLine, npbos);
			Assert(npbos.Any(x => x.RowMessageErrors.Any(y => y.Message == "No more than 9 different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined.")));

			package10.CW_PackType = Core.Constants.PkgUnit.Bundle;
			Assert(!npbos.Any(x => x.RowMessageErrors.Any(y => y.Message == "No more than 9 different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined.")));

			package10.CW_PackType = Core.Constants.PkgUnit.Carton;
			npbos.OfType<BaseCusLinkPackage>().First().IsLinked = false;
			Assert(!npbos.Any(x => x.RowMessageErrors.Any(y => y.Message == "No more than 9 different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined.")));
		}

		public void TestCheckPackageUnitCount_ConsiderMarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MasterBill = "MB1";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;

			declaration.Packages.RemoveAndDeleteAll();
			for (var i = 0; i < 9; i++)
			{
				declaration.Packages.AddNew();
			}

			var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			npbos.OfType<BaseCusLinkPackage>().ForEach(x => x.IsLinked = true);
			bool hasErrorMsg()
			{
				return npbos.Any(x => x.RowMessageErrors.Any(y => y.Message == "No more than 9 different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined."));
			}

			PackagesLimitHelper.CheckPackageUnitCount(invoiceLine, npbos);
			Assert(!hasErrorMsg());

			foreach (var item in declaration.Packages)
			{
				item.CW_MarksAndNos = "N/M";
			}
			npbos.OfType<BaseCusLinkPackage>().ForEach(x => x.IsLinked = true);
			PackagesLimitHelper.CheckPackageUnitCount(invoiceLine, npbos);
			Assert(!hasErrorMsg());

			declaration.Packages.AddNew().CW_MarksAndNos = "N/M";
			npbos.OfType<BaseCusLinkPackage>().ForEach(x => x.IsLinked = true);
			PackagesLimitHelper.CheckPackageUnitCount(invoiceLine, npbos);
			Assert(hasErrorMsg());
		}
	}
}
