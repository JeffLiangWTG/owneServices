using CargoWise.EntityFramework;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(Customs.Business.BaseJobDeclaration declaration) => declaration.CustomsEntryInstructions.FirstOrAddNew();

		protected override Customs.Business.BaseJobDeclaration GetMeANewJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			return dec;
		}

		protected override void SetUp()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			base.SetUp();
		}
	}
}
