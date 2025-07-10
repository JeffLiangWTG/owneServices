using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APComplianceDocumentModule))]
	public class APComplianceDocumentModuleTest : ComplianceDocumentModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.APComplianceDocument;

		protected override SecurityCheckpoint CheckpointForVoid
		{
			get { return Env.Security.VoidPayablesComplianceDocuments; }
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			return factory.NewWithValidTestData<APComplianceDocumentHeader>();
		}

		protected override InvoicingBase CreateInvoice()
		{
			return TestObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
		}

		protected override AccComplianceDocumentHeader NewComplianceDocumentHeader()
		{
			return Factory.NewWithValidTestData<APComplianceDocumentHeader>();
		}
	}
}
