using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCCI_LineNumber()
		{
			var consignment = Factory.New<CusExitConsignment>();
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			var targetInfo = consignmentItem.CCI_LineNumberInfo;
			var valueCannotBeNegativeMessage = MandatoryValidation.ValueCannotBeNegativeMessage(targetInfo.HumanReadableName);
			var valueCannotBeZeroMessage = MandatoryValidation.ValueCannotBeZeroMessage(targetInfo.HumanReadableName);

			CombineAssertions(() =>
			{
				consignmentItem.CCI_LineNumber = (short)0;
				AssertHasError("CCI_LineNumber = 0", targetInfo, valueCannotBeZeroMessage);

				consignmentItem.CCI_LineNumber = -1;
				AssertHasError("CCI_LineNumber = -1", targetInfo, valueCannotBeNegativeMessage);

				consignmentItem.CCI_LineNumber = (short)1;
				AssertNoErrors("CCI_LineNumber = 1", targetInfo);
			});
		}

		public void TestCheckCCI_LineNumberDuplicates()
		{
			var consignment = Factory.New<CusExitConsignment>();
			var item1 = consignment.CusExitConsignmentItems.AddNew();
			item1.CCI_LineNumber = 1;
			var item2 = consignment.CusExitConsignmentItems.AddNew();
			CombineAssertions(() =>
			{
				item2.CCI_LineNumber = 1;
				AssertHasMessageError("Duplicate ConsignmentItem Line Number", item2.CCI_LineNumberInfo, "Item Number (1) should not be duplicated");

				item2.CCI_LineNumber = 2;
				AssertNoMessageErrors("ConsignmentItem Line Number not duplicated", item2.CCI_LineNumberInfo);
			});
		}

		public void TestCheckCCI_Calc_ReportGrossMass()
		{
			CombineAssertions(() =>
			{
				(_, _, var consignmentItem, _, _) = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);
				using (consignmentItem.SetupReportItemData(null))
				{
					var targetInfo = consignmentItem.CCI_Calc_ReportGrossMassInfo;
					var valueCannotBeNegativeMessage = MandatoryValidation.ValueCannotBeNegativeMessage(targetInfo.HumanReadableName);
					var valueCannotBeZeroMessage = MandatoryValidation.ValueCannotBeZeroMessage(targetInfo.HumanReadableName);

					consignmentItem.CCI_Calc_ShouldReportItem = true;
					consignmentItem.CCI_Calc_ReportGrossMass = -1;
					AssertHasError("CCI_Calc_ReportGrossMass = -1", targetInfo, valueCannotBeNegativeMessage);

					consignmentItem.CCI_Calc_ShouldReportItem = false;
					consignmentItem.Validation.ValidateCCI_Calc_ReportGrossMass();
					AssertNoError("CCI_Calc_ReportGrossMass = -1", targetInfo, valueCannotBeNegativeMessage);

					consignmentItem.CCI_Calc_ShouldReportItem = true;
					consignmentItem.CCI_Calc_ReportGrossMass = 1;
					AssertNoError("CCI_Calc_ReportGrossMass = 1", targetInfo, valueCannotBeNegativeMessage);
					AssertNoMessageError("CCI_Calc_ReportGrossMass = 1", targetInfo, valueCannotBeZeroMessage);

					consignmentItem.CCI_Calc_ReportGrossMass = 0;
					AssertHasMessageError("CCI_Calc_ReportGrossMass = 0", targetInfo, valueCannotBeZeroMessage);

					consignmentItem.CCI_Calc_ShouldReportItem = false;
					consignmentItem.Validation.ValidateCCI_Calc_ReportGrossMass();
					AssertNoMessageError("CCI_Calc_ReportGrossMass = 1", targetInfo, valueCannotBeZeroMessage);
				}
			});
		}

		public void TestCheckCCI_Calc_ReportNetMass()
		{
			CombineAssertions(() =>
			{
				(_, _, var consignmentItem, _, _) = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);
				using (consignmentItem.SetupReportItemData(null))
				{
					var targetInfo = consignmentItem.CCI_Calc_ReportNetMassInfo;
					var valueCannotBeNegativeMessage = MandatoryValidation.ValueCannotBeNegativeMessage(targetInfo.HumanReadableName);
					var valueCannotBeZeroMessage = MandatoryValidation.ValueCannotBeZeroMessage(targetInfo.HumanReadableName);

					consignmentItem.CCI_Calc_ShouldReportItem = true;
					consignmentItem.CCI_Calc_ReportNetMass = -1;
					AssertHasError("CCI_Calc_ReportGrossMass = -1", targetInfo, valueCannotBeNegativeMessage);

					consignmentItem.CCI_Calc_ShouldReportItem = false;
					consignmentItem.Validation.ValidateCCI_Calc_ReportNetMass();
					AssertNoError("CCI_Calc_ReportNetMass = -1", targetInfo, valueCannotBeNegativeMessage);

					consignmentItem.CCI_Calc_ShouldReportItem = true;
					consignmentItem.CCI_Calc_ReportNetMass = 1;
					AssertNoError("CCI_Calc_ReportNetMass = 1", targetInfo, valueCannotBeNegativeMessage);
					AssertNoMessageError("CCI_Calc_ReportNetMass = 1", targetInfo, valueCannotBeZeroMessage);

					consignmentItem.CCI_Calc_ReportNetMass = 0;
					AssertHasMessageError("CCI_Calc_ReportNetMass = 0", targetInfo, valueCannotBeZeroMessage);

					consignmentItem.CCI_Calc_ShouldReportItem = false;
					consignmentItem.Validation.ValidateCCI_Calc_ReportNetMass();
					AssertNoMessageError("CCI_Calc_ReportNetMass = 1", targetInfo, valueCannotBeZeroMessage);
				}
			});
		}

		public void TestCheckCCI_UniqueConsignmentReferenceStatus_ListValidation() => CombineAssertions(() =>
		{
			var item = Factory.New<CusExitHeader>().CusExitConsignments.AddNew().CusExitConsignmentItems.AddNew();
			item.CCI_UniqueConsignmentReferenceStatus = "AH3";
			AssertListValidationInvalidCodeMessageError(item.CCI_UniqueConsignmentReferenceStatusInfo, false);

			var itemUcc6 = Factory.GetUcc6ExitHeader().CusExitConsignments.AddNew().CusExitConsignmentItems.AddNew();
			itemUcc6.CCI_UniqueConsignmentReferenceStatus = "AH3";
			AssertListValidationInvalidCodeMessageError(itemUcc6.CCI_UniqueConsignmentReferenceStatusInfo, true);

			itemUcc6.CCI_UniqueConsignmentReferenceStatus = "DIF";
			AssertListValidationInvalidCodeMessageError(itemUcc6.CCI_UniqueConsignmentReferenceStatusInfo, false);
		});

		public void TestCheckCCI_DiscrepancyStatus() => CombineAssertions(() =>
		{
			var item = Factory.New<CusExitHeader>().CusExitConsignments.AddNew().CusExitConsignmentItems.AddNew();
			item.CCI_DiscrepancyStatus = "asd";
			AssertListValidationInvalidCodeError(item.CCI_DiscrepancyStatusInfo, false);

			var itemUcc6 = Factory.GetUcc6ExitHeader().CusExitConsignments.AddNew().CusExitConsignmentItems.AddNew();
			itemUcc6.CCI_DiscrepancyStatus = ZString.Empty;
			AssertListValidationInvalidCodeError(itemUcc6.CCI_DiscrepancyStatusInfo, false);

			itemUcc6.CCI_DiscrepancyStatus = "asd";
			AssertListValidationInvalidCodeError(itemUcc6.CCI_DiscrepancyStatusInfo, true);

			itemUcc6.CCI_DiscrepancyStatus = DiscrepanciesStatusCodeList.Codes.Missing;
			AssertListValidationInvalidCodeError(itemUcc6.CCI_DiscrepancyStatusInfo, false);
		});
	}
}
