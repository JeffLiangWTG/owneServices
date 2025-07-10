using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportInvoiceLinePackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPackQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 1;
			var packingGroup = declaration.PackingGroups[0];
			packingGroup.Packages.RemoveAndDeleteAll();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var pack1 = packingGroup.Packages.AddNew();
			var pack2 = packingGroup.Packages.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV1";

			var inv1Line1 = invoice1.InvoiceLines.AddNew();
			inv1Line1.JI_CEI = instruction1.PK;
			var inv1Line2 = invoice1.InvoiceLines.AddNew();
			inv1Line2.JI_CEI = instruction1.PK;
			using (invoice1.SuspendLineNumberRenumberingForDataImport())
			{
				inv1Line2.JI_LineNo = 3;
				inv1Line1.JI_LineNo = 4;
			}
			var inv2Line1 = invoice2.InvoiceLines.AddNew();
			inv2Line1.JI_CEI = instruction1.PK;
			var inv2Line2 = invoice2.InvoiceLines.AddNew();
			inv2Line2.JI_CEI = instruction2.PK;
			using (invoice2.SuspendLineNumberRenumberingForDataImport())
			{
				inv2Line2.JI_LineNo = 5;
				inv2Line1.JI_LineNo = 6;
			}

			inv1Line1.ZG_IsMainPack = true;
			inv1Line2.ZG_IsMainPack = true;
			inv2Line1.ZG_IsMainPack = true;
			inv2Line2.ZG_IsMainPack = true;

			var inv1Line1Package1 = inv1Line1.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack1.PK);
			inv1Line1Package1.IsLinked = true;
			var inv1Line1Package2 = inv1Line1.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack2.PK);
			inv1Line1Package2.IsLinked = false;

			var inv1Line2Package1 = inv1Line2.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack1.PK);
			inv1Line2Package1.IsLinked = false;
			var inv1Line2Package2 = inv1Line2.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack2.PK);
			inv1Line2Package2.IsLinked = true;
			inv1Line2Package2.PackQty = 1;

			var inv2Line1Package1 = inv2Line1.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack1.PK);
			inv2Line1Package1.IsLinked = true;
			inv2Line1Package1.PackQty = 1;
			var inv2Line1Package2 = inv2Line1.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack2.PK);
			inv2Line1Package2.IsLinked = false;

			var inv2Line2Package1 = inv2Line2.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack1.PK);
			inv2Line2Package1.IsLinked = true;
			inv2Line2Package1.PackQty = 1;
			var inv2Line2Package2 = inv2Line2.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.PackagePk == pack2.PK);
			inv2Line2Package2.IsLinked = true;
			inv2Line2Package2.PackQty = 1;

			CombineAssertions(() =>
			{
				inv1Line1Package1.PackQty = 1;
				inv1Line1Package2.Validation.ValidatePackQty();
				AssertHasMessageError("1 - inv1Line1Package1", inv1Line1Package1.PackQtyInfo, $"There is an Invoice Line ({inv2Line1.InvoiceAndLineReference}) marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0.");
				AssertNoMessageErrors("1 - inv1Line1Package2", inv1Line1Package2.PackQtyInfo);
				AssertNoMessageErrors("1 - inv1Line2Package1", inv1Line2Package1.PackQtyInfo);
				AssertNoMessageErrors("1 - inv1Line2Package2", inv1Line2Package2.PackQtyInfo);
				AssertHasMessageError("1 - inv2Line1Package1", inv2Line1Package1.PackQtyInfo, $"There is an Invoice Line ({inv1Line1.InvoiceAndLineReference}) marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0.");
				AssertNoMessageErrors("1 - inv2Line1Package2", inv2Line1Package2.PackQtyInfo);
				AssertNoMessageErrors("1 - inv2Line2Package1", inv2Line2Package1.PackQtyInfo);
				AssertNoMessageErrors("1 - inv2Line2Package2", inv2Line2Package2.PackQtyInfo);

				inv2Line1Package1.PackQty = 0;
				inv1Line1Package1.Validation.ValidatePackQty();
				AssertHasMessageError("2 - inv1Line1Package1", inv1Line1Package1.PackQtyInfo, $"There is an Invoice Line ({inv2Line1.InvoiceAndLineReference}) marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0.");
				AssertNoMessageErrors("2 - inv2Line1Package1", inv2Line1Package1.PackQtyInfo);

				inv2Line1Package1.IsLinked = false;
				inv2Line1Package1.Validation.ValidatePackQty();
				inv1Line1Package1.Validation.ValidatePackQty();
				AssertNoMessageErrors("3 - inv1Line1Package1", inv1Line1Package1.PackQtyInfo);
				AssertNoMessageErrors("3 - inv2Line1Package1", inv2Line1Package1.PackQtyInfo);

				inv1Line2Package1.IsLinked = true;
				inv1Line2Package1.PackQty = 1;
				AssertHasMessageError("4 - inv1Line2Package1", inv1Line2Package1.PackQtyInfo, $"There is an Invoice Line ({inv1Line1.InvoiceAndLineReference}) marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0.");
				inv1Line1Package1.Validation.ValidatePackQty();
				AssertHasMessageError("4 - inv1Line1Package1", inv1Line1Package1.PackQtyInfo, $"There is an Invoice Line ({inv1Line2.InvoiceAndLineReference}) marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0.");

				inv1Line2.JI_CEI = ZGuid.Empty;
				inv1Line2Package1.Validation.ValidatePackQty();
				AssertNoMessageErrors("5 - inv1Line2Package1", inv1Line2Package1.PackQtyInfo);
				inv1Line1Package1.Validation.ValidatePackQty();
				AssertNoMessageErrors("5 - inv1Line1Package1", inv1Line1Package1.PackQtyInfo);
			});
		}
	}
}
