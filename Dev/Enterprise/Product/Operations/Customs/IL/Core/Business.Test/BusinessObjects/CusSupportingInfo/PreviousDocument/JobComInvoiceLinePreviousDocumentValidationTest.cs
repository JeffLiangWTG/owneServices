using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class JobComInvoiceLinePreviousDocumentValidationTest : PreviousDocumentValidationTest
	{
		public void TestCheckCSI_Code()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeType = helper.CreateCusCodeType("ENSTY", "IL Entry Style");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeType.ZZK_CodeType, "1", ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeType.ZZK_CodeType, "2", ZDateTime.BrettsBirthday, ZDateTime.Today);
			factory.Save();

			PreviousDocument.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining(PreviousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_Code = "BAD";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_Code = "1";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			PreviousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(PreviousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			PreviousDocument.CSI_ReferenceNumber2 = ZString.Empty;
			AssertHasMessageErrorContaining(PreviousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
		}
		public void TestCheckCSI_ItemNumber()
		{
			PreviousDocument.CSI_ItemNumber = ZInt.Zero;
			AssertHasMessageErrorContaining(PreviousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_Quantity()
		{
			PreviousDocument.CSI_Quantity = ZDecimal.Zero;
			AssertHasMessageErrorContaining(PreviousDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_UnitOfQuantity()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeTypeI = helper.CreateCusCodeType("CUSUQ", "IL Customs Units Of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeI.ZZK_CodeType, "ANN", ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeI.ZZK_CodeType, "T3C", ZDateTime.BrettsBirthday, ZDateTime.Today);
			factory.Save();

			PreviousDocument.CSI_UnitOfQuantity = ZString.Empty;
			AssertHasMessageErrorContaining(PreviousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(PreviousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_UnitOfQuantity = "BAD";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(PreviousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_UnitOfQuantity = "ANN";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(PreviousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		PreviousDocument PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					var jobDeclaration = Factory.New<JobDeclaration>();
					var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
					var jobComInvoiceLine = (JobComInvoiceLine)jobComInvoiceHeader.InvoiceLines.AddNew();
					previousDocument = jobComInvoiceLine.PreviousDocuments.AddNew();
				}
				return previousDocument;
			}
		}

		PreviousDocument previousDocument;

		#endregion
	}
}
