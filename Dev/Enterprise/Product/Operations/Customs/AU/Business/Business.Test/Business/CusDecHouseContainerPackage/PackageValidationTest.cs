using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class PackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTransportModeValidationForPackingUnitCount()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			package.CW_PackQty = 200;
			package.CW_OuterPacks = 123;
			AssertHasMessageError("Packing unit count", package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsNotAllowedOnAir);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			package.Validation.ValidateCW_OuterPacks();
			AssertNoMessageError("Packing unit count", package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsNotAllowedOnAir);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			package.Validation.ValidateCW_OuterPacks();
			AssertNoMessageError("Packing unit count", package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsNotAllowedOnAir);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			package.Validation.ValidateCW_OuterPacks();
			AssertNoMessageError("Packing unit count", package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsNotAllowedOnAir);
		}

		public void TestNumberOfPackagesEnteredForANature1020Or10()
		{
			ZString errorMsg = "You have not entered the Total Number of Packages.";
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			package.CW_PackQty = 0;
			AssertHasMessageErrorContaining(package.CW_PackQtyInfo, errorMsg);

			line.JI_IsPackToBondForLine = false;
			package.CW_PackQty = 2;
			AssertNoMessageErrorContaining(package.CW_PackQtyInfo, errorMsg);

			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_IsPackToBondForLine = true;
			package.CW_PackQty = 0;
			AssertHasMessageErrorContaining(package.CW_PackQtyInfo, errorMsg);

			package.CW_PackQty = 2;
			AssertNoMessageErrorContaining(package.CW_PackQtyInfo, errorMsg);

			package.CW_PackQty = 0;
			AssertHasMessageErrorContaining(package.CW_PackQtyInfo, errorMsg);

			Package package2 = declaration.Packages.AddNew();
			package2.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package2.CW_PackQty = 2;
			package.RunPreSaveValidation();
			package2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(package.CW_PackQtyInfo, errorMsg);
			AssertNoMessageErrorContaining(package2.CW_PackQtyInfo, errorMsg);

			Package package3 = declaration.Packages.AddNew();
			package3.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package3.CW_PackQty = int.MaxValue;
			AssertHasMessageErrorContaining(package3.CW_PackQtyInfo, "The total number of packages exceeds the maximum allowed amount. Check packing lines for data entry errors.");

			package3.CW_PackQty = 50;
			AssertNoMessageErrorContaining(package3.CW_PackQtyInfo, "The total number of packages exceeds the maximum allowed amount. Check packing lines for data entry errors.");
		}

		public void TestWarehouseUnitEnteredForANature1020Or20()
		{
			ZString errorMsg = "You have not entered the Warehouse Number of Packages.";
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			package.CW_InBondPackQty = 0;
			AssertNoWarningContaining(package.CW_InBondPackQtyInfo, errorMsg);

			line.JI_IsPackToBondForLine = true;
			package.Validation.ValidateCW_InBondPackQty();
			AssertHasWarningContaining(package.CW_InBondPackQtyInfo, errorMsg);

			line.JI_IsPackToBondForLine = false;
			package.Validation.ValidateCW_InBondPackQty();
			AssertNoWarningContaining(package.CW_InBondPackQtyInfo, errorMsg);

			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_IsPackToBondForLine = true;
			package.Validation.ValidateCW_InBondPackQty();
			AssertHasWarningContaining(package.CW_InBondPackQtyInfo, errorMsg);

			package.CW_InBondPackQty = 2;
			AssertNoWarningContaining(package.CW_InBondPackQtyInfo, errorMsg);

			package.CW_InBondPackQty = 0;
			AssertHasWarningContaining(package.CW_InBondPackQtyInfo, errorMsg);

			Package package2 = declaration.Packages.AddNew();
			package2.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package2.CW_InBondPackQty = 2;
			package.RunPreSaveValidation();
			package2.RunPreSaveValidation();
			AssertNoWarningContaining(package.CW_InBondPackQtyInfo, errorMsg);
			AssertNoWarningContaining(package2.CW_InBondPackQtyInfo, errorMsg);
		}

		public void TestWarehouseUnit()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			ZString errorMsg = "Warehouse Number of Packages should not be entered if the Declaration is not Nature 10/20 or Nature 20.";
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			package.CW_InBondPackQty = 1;
			AssertHasMessageErrorContaining(package.CW_InBondPackQtyInfo, errorMsg);

			line.JI_IsPackToBondForLine = true;
			package.CW_InBondPackQty = 2;
			AssertNoMessageErrorContaining(package.CW_InBondPackQtyInfo, errorMsg);

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			package.CW_InBondPackQty = 1;
			AssertNoMessageErrorContaining(package.CW_InBondPackQtyInfo, errorMsg);

			line.JI_IsPackToBondForLine = false;
			package.CW_InBondPackQty = 2;
			AssertHasMessageErrorContaining(package.CW_InBondPackQtyInfo, errorMsg);

			line.JI_IsPackToBondForLine = true;
			AssertNoMessageErrorContaining(package.CW_InBondPackQtyInfo, errorMsg);
		}

		public void TestMarksAndNumbers()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			package.CW_MarksAndNos = ZString.Empty;
			AssertHasMessageErrors(package.CW_MarksAndNosInfo);

			package.CW_MarksAndNos = "f00f";
			AssertNoMessageErrors(package.CW_MarksAndNosInfo);

			declaration.JE_TransportMode = ZString.Empty;
			package.CW_MarksAndNos = ZString.Empty;
			AssertNoMessageErrors(package.CW_MarksAndNosInfo);
		}

		public void TestPackingUnitCount()
		{
			package.CW_PackQty = 10;
			package.CW_OuterPacks = 15;
			AssertHasMessageError(package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsGreaterThanPackQty);

			package.CW_OuterPacks = 10;
			AssertNoMessageError(package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsGreaterThanPackQty);

			package.CW_OuterPacks = 0;
			AssertNoMessageError(package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsGreaterThanPackQty);

			package.CW_OuterPacks = 9999;
			AssertNoMessageError(package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsTooLong);

			package.CW_OuterPacks = 10000;
			AssertHasMessageError(package.CW_OuterPacksInfo, PackageValidation.OuterPacksIsTooLong);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			package = declaration.Packages.AddNew();
			houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "house";
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
		}

		Bill houseBill;
		JobDeclaration declaration;
		Package package;

		#endregion
	}
}
