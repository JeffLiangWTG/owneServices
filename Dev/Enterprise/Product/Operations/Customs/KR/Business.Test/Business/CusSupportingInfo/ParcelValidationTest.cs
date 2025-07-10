using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ParcelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDoNotWorkWhenValidationModeIsNotSet()
		{
			Assert(!declaration.IsMailDeclarationValidationOn);
			parcel.Validation.ValidateCSI_ReferenceNumber();
			parcel.Validation.ValidateCSI_ReferenceNumber2();
			parcel.Validation.ValidateCSI_Code();

			AssertNoMessageErrors(parcel.CSI_ReferenceNumberInfo);
			AssertNoMessageErrors(parcel.CSI_ReferenceNumber2Info);
			AssertNoMessageErrors(parcel.CSI_CodeInfo);
		}

		public void TestCSI_ReferenceNumber()
		{
			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5SI);
			parcel.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining(parcel.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			parcel.CSI_ReferenceNumber = "1234567890";
			AssertHasMessageErrorContaining(parcel.CSI_ReferenceNumberInfo, "This parcel customs number must be 14 characters long.");

			parcel.CSI_ReferenceNumber = "12345678901234";
			AssertNoMessageErrorContaining(parcel.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCSI_ReferenceNumber2()
		{
			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5SI);
			parcel.Validation.ValidateCSI_ReferenceNumber2();
			AssertHasMessageErrorContaining(parcel.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

			parcel.CSI_ReferenceNumber2 = "01";
			AssertNoMessageErrorContaining(parcel.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCSI_Code()
		{
			parcel.CSI_Code = "X";
			AssertHasMessageErrorContaining(parcel.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			parcel.CSI_Code = "";
			AssertNoMessageErrorContaining(parcel.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5SI);
			parcel.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(parcel.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			parcel.CSI_Code = "X";
			AssertHasMessageErrorContaining(parcel.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			parcel.CSI_Code = DeliveryTypeCodeList.Codes.A;
			AssertNoMessageErrorContaining(parcel.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			parcel.CSI_Code = DeliveryTypeCodeList.Codes.B;
			AssertNoMessageErrorContaining(parcel.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			parcel = invoice.Parcels.AddNew();
		}
		JobDeclaration declaration;
		Parcel parcel;
	}
}
