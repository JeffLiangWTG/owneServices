using System;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceHeaderPrivateTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestProductAuditActionCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CustomsDataRegistry.Instance.ExportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddWarningValidation, invoice.ProductAuditActionCoreForTesting());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CACustomsDataRegistry.Instance.ReleaseHighValueProductAudit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddMessageErrorValidation, invoice.ProductAuditActionCoreForTesting());

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var orgImpAddInfo1 = OrgImpAddInfo.Get(org1);
			orgImpAddInfo1.ZO_ACROSSHighValueProductAuditAction = ProductAuditActions.Codes.AddWarningValidation;
			invoice.JZ_OH_Supplier = org1.PK;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddWarningValidation, invoice.ProductAuditActionCoreForTesting());

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			var orgImpAddInfo2 = OrgImpAddInfo.Get(org2);
			orgImpAddInfo2.ZO_ACROSSHighValueProductAuditAction = ProductAuditActions.Codes.AddMessageErrorValidation;
			invoice.JZ_OH_Buyer = org2.PK;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddMessageErrorValidation, invoice.ProductAuditActionCoreForTesting());
		}

		public void TestGetProductAuditActionFromOrgHeaderPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "orgHeader";
			var orgImpAddInfo = OrgImpAddInfo.Get(orgHeader);
			invoice.JZ_OH_Buyer = orgHeader.PK;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.RegistryDefault, invoice.GetProductAuditActionFromOrgHeaderPK(orgHeader.PK, JobDeclaration.CAProductAuditType.ACROSSHigh));
			orgImpAddInfo.ZO_ACROSSHighValueProductAuditAction = ProductAuditActions.Codes.AddWarningValidation;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddWarningValidation, invoice.GetProductAuditActionFromOrgHeaderPK(orgHeader.PK, JobDeclaration.CAProductAuditType.ACROSSHigh));
			orgImpAddInfo.ZO_B3HighValueProductAuditAction = ProductAuditActions.Codes.AddMessageErrorValidation;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddMessageErrorValidation, invoice.GetProductAuditActionFromOrgHeaderPK(orgHeader.PK, JobDeclaration.CAProductAuditType.B3High));
			orgImpAddInfo.ZO_B3LowValueProductAuditAction = ProductAuditActions.Codes.AddWarningValidation;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddWarningValidation, invoice.GetProductAuditActionFromOrgHeaderPK(orgHeader.PK, JobDeclaration.CAProductAuditType.B3Low));
			orgImpAddInfo.ZO_ACROSSLowValueProductAuditAction = ProductAuditActions.Codes.AddMessageErrorValidation;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddMessageErrorValidation, invoice.GetProductAuditActionFromOrgHeaderPK(orgHeader.PK, JobDeclaration.CAProductAuditType.ACROSSLow));
		}

		public void TestGetProductAuditActionFromRegistry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.NoAction, invoice.GetProductAuditActionFromRegistry(JobDeclaration.CAProductAuditType.ACROSSLow));
			CACustomsDataRegistry.Instance.ReleaseHighValueProductAudit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddWarningValidation, invoice.GetProductAuditActionFromRegistry(JobDeclaration.CAProductAuditType.ACROSSHigh));
			CACustomsDataRegistry.Instance.EntryHighValueProductAudit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddMessageErrorValidation, invoice.GetProductAuditActionFromRegistry(JobDeclaration.CAProductAuditType.B3High));
			CACustomsDataRegistry.Instance.EntryLowValueProductAudit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddWarningValidation, invoice.GetProductAuditActionFromRegistry(JobDeclaration.CAProductAuditType.B3Low));
			CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
			AssertEquals("Product Audit Action", ProductAuditActions.Codes.AddMessageErrorValidation, invoice.GetProductAuditActionFromRegistry(JobDeclaration.CAProductAuditType.ACROSSLow));
		}
	}
}
