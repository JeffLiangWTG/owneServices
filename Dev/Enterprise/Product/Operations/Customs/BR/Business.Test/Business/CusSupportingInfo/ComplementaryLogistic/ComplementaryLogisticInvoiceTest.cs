using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ComplementaryLogisticInvoice))]
	public class ComplementaryLogisticInvoiceTest : Customs.Business.Testing.CusSupportingInfoTest<ComplementaryLogisticInvoice>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ComplementaryLogisticInvoiceCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<ComplementaryLogisticInvoice>();
			AssertEquals(CusSupportingInfoTypeList.Codes.ComplementaryLogisticInvoice, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		public void TestCnpjSupplierNfeKey()
		{
			OrgHeader oTestSupplier0 = Factory.New<OrgHeader>();
			OrgHeader oTestSupplier1 = Factory.New<OrgHeader>();
			oTestSupplier1.PrimaryRegistrationNumber.Number = "58.500.398/-04";
			OrgHeader oTestSupplier2 = Factory.New<OrgHeader>();
			oTestSupplier2.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvLine = testInvoice.JobComInvoiceLines.AddNew();
			var testLogisticInvoice = testInvLine.ComplementaryLogisticInvoiceCollection.AddNew();
			AssertEquals("NoSupplierOrg", ZString.Empty, testLogisticInvoice.SupplierCNPJ);
			testDeclaration.JE_OH_Supplier = oTestSupplier0.PK;
			AssertEquals("SupplierOrgHasNoCNPJ", ZString.Empty, testLogisticInvoice.SupplierCNPJ);
			testDeclaration.JE_OH_Supplier = oTestSupplier1.PK;
			AssertEquals("Using Declaration's Supplier", "5850039804", testLogisticInvoice.SupplierCNPJ);
			testInvoice.JZ_OA_SupplierAddress = oTestSupplier2.MainAddress.PK;
			AssertEquals("Using InvHeader's Supplier", "58500398000105", testLogisticInvoice.SupplierCNPJ);
		}

		protected override IEnumerable<ComplementaryLogisticInvoice> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var complementaryInvoiceCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ComplementaryLogisticInvoiceCollection.AddNew();
			complementaryInvoiceCusSupporting.CSI_ReferenceNumber = "1";
			complementaryInvoiceCusSupporting.CSI_LineNo = 1;
			yield return complementaryInvoiceCusSupporting;
		}
	}
}
