using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusContainerInvoiceLinePivotValidationTest : TestCaseWithFactory
	{
		public void TestValidateNetWeightInKG()
		{
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			var nonPersistentContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0];
			nonPersistentContainer.IsForInvoiceLine = true;
			var pivot = nonPersistentContainer.Pivot;
			Assert("Pre-condition", !nonPersistentContainer.NetWeightInKGInfo.HasMessageErrors());
			nonPersistentContainer.NetWeightInKG = 12;
			Assert("Net Weight has message errors as IMA1 net weight cannot be entered for fish", nonPersistentContainer.NetWeightInKGInfo.HasMessageErrors());
			AssertHasMessageError(pivot.C2_NetWeightInfo, "IMA1 net weight can only be entered when produce type is dairy.");
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			nonPersistentContainer.NetWeightInKG = 345;
			Assert("Net Weight is valid as IMA1 net weight can be entered for dairy", !nonPersistentContainer.NetWeightInKGInfo.HasMessageErrors());
			AssertNoMessageError(pivot.C2_NetWeightInfo, "IMA1 net weight can only be entered when produce type is dairy.");
			header.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			nonPersistentContainer.NetWeightInKG = 12;
			Assert("Net Weight validation is only applicable to Quarantine", !nonPersistentContainer.NetWeightInKGInfo.HasMessageErrors());
			AssertNoMessageError(pivot.C2_NetWeightInfo, "IMA1 net weight can only be entered when produce type is dairy.");
		}

		public void TestValidateGrossWeightInKG()
		{
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			var nonPersistentContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0];
			nonPersistentContainer.IsForInvoiceLine = true;
			var pivot = nonPersistentContainer.Pivot;
			Assert("Pre-condition", !nonPersistentContainer.GrossWeightInKGInfo.HasMessageErrors());
			nonPersistentContainer.GrossWeightInKG = 12;
			Assert("Gross Weight has message errors as IMA1 gross weight cannot be entered for fish", nonPersistentContainer.GrossWeightInKGInfo.HasMessageErrors());
			AssertHasMessageError(pivot.C2_GrossWeightInfo, "IMA1 gross weight can only be entered when produce type is dairy.");
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			nonPersistentContainer.GrossWeightInKG = 145;
			Assert("Gross Weight is valid as IMA1 gross weight can be entered for dairy", !nonPersistentContainer.GrossWeightInKGInfo.HasMessageErrors());
			AssertNoMessageError(pivot.C2_GrossWeightInfo, "IMA1 gross weight can only be entered when produce type is dairy.");
			header.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			nonPersistentContainer.GrossWeightInKG = 12;
			Assert("Gross Weight validation is only applicable to Quarantine", !nonPersistentContainer.GrossWeightInKGInfo.HasMessageErrors());
			AssertNoMessageError(pivot.C2_GrossWeightInfo, "IMA1 gross weight can only be entered when produce type is dairy.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleExportDeclaration();
			var declaration = helper.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var invoiceHeader = helper.Declaration.Invoices[0];
			header = invoiceHeader.QuarantineExDocHeader;
			container = helper.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234567";
			container.CO_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}
		QuarantineExDocHeader header;
		JobComInvoiceLine invoiceLine;
		CusContainer container;
	}
}
