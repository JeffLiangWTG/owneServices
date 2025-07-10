using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PreviousExpDecLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestReferenceNumber()
		{
			previousExpDecLine.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousExpDecLine.CSI_ReferenceNumber = "123450600007X";
			AssertNoMessageErrorContaining(previousExpDecLine.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestReferenceNumber2()
		{
			previousExpDecLine.Validation.ValidateCSI_ReferenceNumber2();
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

			previousExpDecLine.CSI_ReferenceNumber2 = "01";
			AssertNoMessageErrorContaining(previousExpDecLine.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestItemNumber()
		{
			previousExpDecLine.Validation.ValidateCSI_ItemNumber();
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousExpDecLine.CSI_ItemNumber = 1;
			AssertNoMessageErrorContaining(previousExpDecLine.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUQ()
		{
			previousExpDecLine.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrors(previousExpDecLine.CSI_UnitOfQuantityInfo);

			previousExpDecLine.CSI_Quantity = 1;
			previousExpDecLine.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "You have not entered");

			previousExpDecLine.CSI_UnitOfQuantity = "U";
			AssertNoMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "You have not entered");

			previousExpDecLine.CSI_ReferenceNumber = "123452500001X";
			previousExpDecLine.CSI_ReferenceNumber2 = "1";
			previousExpDecLine.CSI_ItemNumber = 1;

			var decLine2 = invoiceLine.PreviousExpDecLineCollection.AddNew();
			decLine2.CSI_ReferenceNumber = "123452500001X";
			decLine2.CSI_ReferenceNumber2 = "1";
			decLine2.CSI_ItemNumber = 1;
			decLine2.CSI_UnitOfQuantity = "U";
			var decLine3 = invoiceLine.PreviousExpDecLineCollection.AddNew();
			decLine3.CSI_ReferenceNumber = "123452500001X";
			decLine3.CSI_ReferenceNumber2 = "1";
			decLine3.CSI_ItemNumber = 1;
			decLine3.CSI_UnitOfQuantity = "U";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check");
			previousExpDecLine.CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check");
			decLine2.CSI_UnitOfQuantity = Core.Constants.Weight.Grams;
			decLine3.CSI_UnitOfQuantity = "U";
			previousExpDecLine.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check");

			decLine2.CSI_UnitOfQuantity = Core.Constants.Volume.Litre;
			decLine3.CSI_UnitOfQuantity = Core.Constants.Weight.Tonnes;
			previousExpDecLine.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check");

			decLine2.CSI_UnitOfQuantity = Core.Constants.Weight.Ounces;
			previousExpDecLine.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check");

			decLine2.CSI_UnitOfQuantity = Core.Constants.Volume.CubicDecimetres;
			decLine3.CSI_UnitOfQuantity = Core.Constants.Volume.CubicMetres;
			previousExpDecLine.CSI_UnitOfQuantity = Core.Constants.Area.SquareCentimetre;
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check");

			decLine2.CSI_UnitOfQuantity = Core.Constants.Area.SquareCentimetre;
			decLine3.CSI_UnitOfQuantity = Core.Constants.Area.SquareCentimetre;
			previousExpDecLine.CSI_UnitOfQuantity = Core.Constants.Area.SquareCentimetre;
			AssertNoMessageErrorContaining(previousExpDecLine.CSI_UnitOfQuantityInfo, "There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check");
		}

		public void TestQuantity()
		{
			previousExpDecLine.CSI_Quantity = 1;
			AssertNoMessageErrors(previousExpDecLine.CSI_QuantityInfo);

			previousExpDecLine.CSI_Quantity = 0;
			AssertNoMessageErrors(previousExpDecLine.CSI_QuantityInfo);

			previousExpDecLine.CSI_Quantity = -1;
			AssertHasMessageErrorContaining(previousExpDecLine.CSI_QuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		protected override void SetUp()
		{
			base.SetUp();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Tariff = "123452500001U";
			previousExpDecLine = invoiceLine.PreviousExpDecLineCollection.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		PreviousExpDecLine previousExpDecLine;
	}
}
