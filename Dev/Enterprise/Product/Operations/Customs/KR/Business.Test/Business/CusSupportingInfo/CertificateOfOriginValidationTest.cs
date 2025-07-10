using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CertificateOfOriginValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			certificateOfOrigin.CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			certificateOfOrigin.CSI_ParentID = invoiceLine.PK;
			certificateOfOrigin.CSI_Type = CusSupportingInfoTypeList.Codes.CertificateOfOrigin;
			certificateOfOrigin.CSI_Code = "B";

			AssertHasMessageErrorContaining(certificateOfOrigin.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			certificateOfOrigin.CSI_Code = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertNoMessageErrors(certificateOfOrigin.CSI_CodeInfo);

			certificateOfOrigin.CSI_Code = CertificateOfOriginIssuedCodeList.Codes.N;
			AssertNoMessageErrors(certificateOfOrigin.CSI_CodeInfo);
		}

		public void TestCheckCSI_SubType()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			certificateOfOrigin.CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			certificateOfOrigin.CSI_ParentID = invoiceLine.PK;
			certificateOfOrigin.CSI_Type = CusSupportingInfoTypeList.Codes.CertificateOfOrigin;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			CheckCSI_SubTypeListValidation();
		}

		void CheckCSI_SubTypeListValidation()
		{
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes._2;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes._4;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes._6;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes._8;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.B;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.C;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.D;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.E;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.F;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.G;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);
			certificateOfOrigin.CSI_SubType = CountryOfOriginDeterminationRuleCodeList.Codes.H;
			AssertNoMessageErrors(certificateOfOrigin.CSI_SubTypeInfo);

			certificateOfOrigin.CSI_SubType = "X";
			AssertHasMessageErrorContaining(certificateOfOrigin.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_RN_NKCountryCode()
		{
			certificateOfOrigin.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AssertNoMessageErrors(certificateOfOrigin.CSI_RN_NKCountryCodeInfo);

			certificateOfOrigin.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrors(certificateOfOrigin.CSI_RN_NKCountryCodeInfo);

			certificateOfOrigin.CSI_RN_NKCountryCode = "XX";
			AssertHasMessageErrorContaining(certificateOfOrigin.CSI_RN_NKCountryCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_Status()
		{
			certificateOfOrigin.CSI_Status = CertificateOfOriginSplitCodeList.Codes.Y;
			AssertNoMessageErrors(certificateOfOrigin.CSI_StatusInfo);
			certificateOfOrigin.CSI_Status = CertificateOfOriginSplitCodeList.Codes.N;
			AssertNoMessageErrors(certificateOfOrigin.CSI_StatusInfo);

			certificateOfOrigin.CSI_Status = "X";
			AssertHasMessageErrorContaining(certificateOfOrigin.CSI_StatusInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.CreateCertificateOfOriginDataIfRequired();
			certificateOfOrigin = invoiceLine.CertificateOfOriginData;
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CertificateOfOrigin certificateOfOrigin;
	}
}
