using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			return dec;
		}

		protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration)
		{
			return ((JobDeclaration)declaration).CusEntryInstruction;
		}

		public new void TestStatisticalValueManualOverrideIsSetForImports()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invLineCollectionChf = new InvoiceLineViewCollection<JobComInvoiceLine>(dec1);
			var invLineChf = invLineCollectionChf.AddNew();
			AssertEquals(true, invLineChf.ZG_StatisticalValueManualOverride);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec2.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invLineCollectionCdsRegDisabled = new InvoiceLineViewCollection<JobComInvoiceLine>(dec2);
			var invLineCdsRegDisabled = invLineCollectionCdsRegDisabled.AddNew();
			AssertEquals(false, invLineCdsRegDisabled.ZG_StatisticalValueManualOverride);

			GBCustomsDataRegistry.Instance.CDSStatisticalValueManualOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec3.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invLineCollectionCdsRegEnabled = new InvoiceLineViewCollection<JobComInvoiceLine>(dec3);
			var invLineCdsRegEnabled = invLineCollectionCdsRegEnabled.AddNew();
			AssertEquals(true, invLineCdsRegEnabled.ZG_StatisticalValueManualOverride);
		}
	}
}
