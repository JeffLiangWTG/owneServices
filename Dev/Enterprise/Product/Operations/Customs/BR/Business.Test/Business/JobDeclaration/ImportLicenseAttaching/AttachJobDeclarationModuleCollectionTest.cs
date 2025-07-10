using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AttachJobDeclarationModuleCollection))]
	class AttachJobDeclarationModuleCollectionTest : Customs.Business.Testing.BaseJobDeclarationBizoCollectionTest
	{
		public void TestPossibleImportLicenseDeclarationForAttachment_List()
		{
			var importer = OrgHeader.New(Factory);

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			importDeclaration.JE_OH_Importer = importer.PK;

			var attachJobDeclaration = AttachJobDeclarationModuleCollection.GetListForImportLicenseAttaching(importDeclaration);

			var importerFilter = attachJobDeclaration.FilterBusinessObjectDefaults["Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"];
			var entryNumberFilter = attachJobDeclaration.FilterBusinessObjectDefaults["Entry #" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "ComparisonOperator"];

			CombineAssertions(() =>
			{
				AssertEquals("filter: Importer/Supplier", importer.PK, importerFilter.Value);
				AssertEquals("filter: Importer/Supplier", false, importerFilter.IsRemovable);
				AssertEquals("filter: Entry #", ModuleTextFilter.ComparisonConstants.IsNotBlank, entryNumberFilter.Value);
				AssertEquals("filter: Entry #", false, entryNumberFilter.IsRemovable);
			});
		}

		public void TestGetExtraNotification()
		{
			var oOrgHeader1 = Factory.New<OrgHeader>();
			oOrgHeader1.OH_Code = "C1";
			var oOrgHeader2 = Factory.New<OrgHeader>();
			oOrgHeader2.OH_Code = "C2";

			var importDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			importDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			importDeclaration.JE_OH_Importer = oOrgHeader1.PK;

			var attachJobDeclaration = new AttachJobDeclarationModuleCollection(importDeclaration);

			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = jobDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			jobDeclaration.JE_OH_Importer = oOrgHeader1.PK;
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var notificationProvider = attachJobDeclaration as IFilterModuleExtraNotificationProvider;
			Factory.Save();
			AssertContains($"A Job selected from here must be an Import License, have at least one registered Entry available and matches the Import Declaration related.", notificationProvider.GetExtraNotification(jobDeclaration).Message);

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);
			jobDeclaration.JE_OH_Importer = oOrgHeader2.PK;
			Factory.Save();
			AssertContains($"A Job selected from here must be an Import License, have at least one registered Entry available and matches the Import Declaration related.", notificationProvider.GetExtraNotification(jobDeclaration).Message);

			jobDeclaration.JE_OH_Importer = oOrgHeader1.PK;
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertContains($"A Job selected from here must be an Import License, have at least one registered Entry available and matches the Import Declaration related.", notificationProvider.GetExtraNotification(jobDeclaration).Message);

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_RelationType = GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot;
			genPivot.Relation1ID = jobDeclaration.PK;
			genPivot.Relation2ID = entryInstruction.PK;
			Factory.Save();
			AssertContains($"A Job selected from here must be an Import License, have at least one registered Entry available and matches the Import Declaration related.", notificationProvider.GetExtraNotification(jobDeclaration).Message);

			genPivot.Delete();
			Factory.Save();
			AssertNull("Must be null", notificationProvider.GetExtraNotification(jobDeclaration));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AttachJobDeclarationModuleCollection(Factory.New<JobDeclaration>());
	}
}
