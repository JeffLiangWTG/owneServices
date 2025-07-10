using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SuspensionDrawback))]
	public class SuspensionDrawbackTest : Customs.Business.Testing.CusSupportingInfoTest<SuspensionDrawback>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().SuspensionDrawbackCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<SuspensionDrawback>();
			AssertEquals(CusSupportingInfoTypeList.Codes.SuspensionDrawback, supporting.CSI_Type);
		}

		protected override IEnumerable<SuspensionDrawback> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var drawbackCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().SuspensionDrawbackCollection.AddNew();
			drawbackCusSupporting.CSI_ReferenceNumber = "1";
			drawbackCusSupporting.CSI_LineNo = 1;
			drawbackCusSupporting.CSI_IsSupplierBeneficiary = ZBool.True;
			yield return drawbackCusSupporting;
		}

		public void TestSettingCSI_IsSupplierBeneficiary()
		{
			var testOrg = Factory.New<OrgHeader>();
			testOrg.FillWithValidTestData();
			testOrg.PrimaryRegistrationNumber.Number = "97-442-7700/00126";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = testOrg.PK;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var susDrawback = invLine.SuspensionDrawbackCollection.AddNew();
			susDrawback.CSI_ReferenceNumber = "12345678901234";
			AssertEquals(false, susDrawback.CSI_IsSupplierBeneficiary);
			AssertEquals("12345678901234", susDrawback.CSI_ReferenceNumber);

			susDrawback.CSI_SubType = TypeSuspensionDrawbackList.Codes.Common;
			AssertEquals(true, susDrawback.CSI_IsSupplierBeneficiary);
			AssertEquals("97442770000126", susDrawback.CSI_ReferenceNumber);

			susDrawback.CSI_SubType = TypeSuspensionDrawbackList.Codes.Intermediate;
			AssertEquals(false, susDrawback.CSI_IsSupplierBeneficiary);
			AssertEquals(string.Empty, susDrawback.CSI_ReferenceNumber);

			susDrawback.CSI_SubType = TypeSuspensionDrawbackList.Codes.Common;
			AssertEquals(true, susDrawback.CSI_IsSupplierBeneficiary);
			AssertEquals("97442770000126", susDrawback.CSI_ReferenceNumber);

			susDrawback.CSI_IsSupplierBeneficiary = false;
			AssertEquals(false, susDrawback.CSI_IsSupplierBeneficiary);
			AssertEquals(string.Empty, susDrawback.CSI_ReferenceNumber);
		}

		public void TestSettingCSI_IsSupplierBeneficiary_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var susDrawback = invLine.SuspensionDrawbackCollection.AddNew();
			susDrawback.CSI_SubType = TypeSuspensionDrawbackList.Codes.Common;
			AssertEquals(false, susDrawback.Beneficiary_ReadOnly);
			susDrawback.CSI_SubType = TypeSuspensionDrawbackList.Codes.Intermediate;
			AssertEquals(true, susDrawback.Beneficiary_ReadOnly);
			susDrawback.CSI_SubType = TypeSuspensionDrawbackList.Codes.GenericIntermediary;
			AssertEquals(true, susDrawback.Beneficiary_ReadOnly);
		}
	}
}
