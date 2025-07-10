using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(MercosulForeignDeclaration))]
	public class MercosulForeignDeclarationTest : Customs.Business.Testing.CusSupportingInfoTest<MercosulForeignDeclaration>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().MercosulForeignDeclarations.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<MercosulForeignDeclaration>();
			AssertEquals(CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<MercosulForeignDeclaration> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var mercosulForeignDeclaration = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().MercosulForeignDeclarations.AddNew();

			mercosulForeignDeclaration.CSI_Description = "1";
			mercosulForeignDeclaration.CSI_ItemNumber = 1;
			yield return mercosulForeignDeclaration;
		}

		public void TestDescriptionMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var mercosulForeignDeclaration = declaration.CustomsEntryInstructions.AddNew().MercosulForeignDeclarations.AddNew();

			AssertEquals("CSI_DescriptionInfo.MaxLength should be equal to", 70, mercosulForeignDeclaration.CSI_DescriptionInfo.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("CSI_DescriptionInfo.MaxLength should be equal to", 16, mercosulForeignDeclaration.CSI_DescriptionInfo.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("CSI_DescriptionInfo.MaxLength should be equal to", 16, mercosulForeignDeclaration.CSI_DescriptionInfo.MaxLength);
		}

		public void TestReferenceNumberMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var mercosulForeignDeclaration = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().MercosulForeignDeclarations.AddNew();

			AssertEquals("CSI_ReferenceNumberInfo.MaxLength should be equal to", 7, mercosulForeignDeclaration.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals("CSI_ReferenceNumberInfo2.MaxLength should be equal to", 7, mercosulForeignDeclaration.CSI_ReferenceNumber2Info.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("CSI_ReferenceNumberInfo.MaxLength should be equal to", 4, mercosulForeignDeclaration.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals("CSI_ReferenceNumberInfo2.MaxLength should be equal to", 4, mercosulForeignDeclaration.CSI_ReferenceNumber2Info.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("CSI_ReferenceNumberInfo.MaxLength should be equal to", 4, mercosulForeignDeclaration.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals("CSI_ReferenceNumberInfo2.MaxLength should be equal to", 4, mercosulForeignDeclaration.CSI_ReferenceNumber2Info.MaxLength);
		}

		public void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var mercosulForeignDeclaration = declaration.CustomsEntryInstructions.AddNew().MercosulForeignDeclarations.AddNew();

			AssertEquals("MercosulForeignDeclarationValidation Validation", typeof(MercosulForeignDeclarationValidation), mercosulForeignDeclaration.Validation.GetType());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("MercosulForeignDeclarationValidation Validation", typeof(CusSupportingInfoValidation), mercosulForeignDeclaration.Validation.GetType());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("MercosulForeignDeclarationValidation Validation", typeof(MercosulForeignDeclarationValidation), mercosulForeignDeclaration.Validation.GetType());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("MercosulForeignDeclarationValidation Validation", typeof(CusSupportingInfoValidation), mercosulForeignDeclaration.Validation.GetType());
		}
	}
}
