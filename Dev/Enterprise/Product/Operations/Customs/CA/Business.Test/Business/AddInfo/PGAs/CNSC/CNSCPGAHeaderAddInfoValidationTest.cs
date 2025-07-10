using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CNSCPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_PackUQ()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			newFactory.Save();

			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = Customs.Business.YesNoList.Codes.Yes;

			var cnscHeader = invoiceLine.CNSCPGAHeader;
			ValidationTestHelper.AssertInvalidCodeMessageError(cnscHeader.CA_PackUQInfo, "XX", "AAA");

			cnscHeader.CA_PackUQ = string.Empty;
			AssertNoMessageErrorContaining(cnscHeader.CA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

			cnscHeader.CA_PackQty = 5m;
			cnscHeader.AddInfoValidation.ValidateCA_PackUQ();
			AssertHasMessageErrorContaining(cnscHeader.CA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_Category()
		{
			header.AddInfoValidation.ValidateCA_Category();
			AssertNoMessageErrorContaining(header.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_Category();
			AssertHasMessageErrorContaining(header.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_Category = CNSCCategories.Codes.NE;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_CategoryInfo, "AA", CNSCCategories.Codes.CNS);
		}
		public void TestCheckCA_PackQty()
		{
			var messageError = "value cannot be zero.";

			header.CA_Category = CNSCCategories.Codes.RD;
			header.AddInfoValidation.ValidateCA_PackQty();
			AssertHasMessageErrorContaining(header.CA_PackQtyInfo, messageError);

			header.CA_Category = CNSCCategories.Codes.NS;
			header.AddInfoValidation.ValidateCA_PackQty();
			AssertHasMessageErrorContaining(header.CA_PackQtyInfo, messageError);

			header.CA_Category = CNSCCategories.Codes.CNS;
			header.AddInfoValidation.ValidateCA_PackQty();
			AssertHasMessageErrorContaining(header.CA_PackQtyInfo, messageError);

			header.CA_Category = CNSCCategories.Codes.NE;
			header.AddInfoValidation.ValidateCA_PackQty();
			AssertNoMessageErrorContaining(header.CA_PackQtyInfo, messageError);

			header.CA_Category = CNSCCategories.Codes.CNS;
			header.CA_PackQty = 10m;
			AssertNoMessageErrorContaining(header.CA_PackQtyInfo, messageError);
		}

		public void TestCheckCA_NNIECRSchePartNo()
		{
			AssertNoMessageErrorContaining(header.CA_NNIECRSchePartNoInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_Category = CNSCCategories.Codes.NE;
			AssertHasMessageErrorContaining(header.CA_NNIECRSchePartNoInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_NNIECRSchePartNo = "XXX";
			AssertNoMessageErrorContaining(header.CA_NNIECRSchePartNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = Customs.Business.YesNoList.Codes.Yes;

			header = invoiceLine.CNSCPGAHeader;
		}
		CNSCPGAHeader header;

		#endregion
	}
}
