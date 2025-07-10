using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GenerateLicenseJobDeclarationModuleCollection))]
	class GenerateLicenseJobDeclarationModuleCollectionTest : Customs.Business.Testing.BaseJobDeclarationBizoCollectionTest
	{
		public void TestFiltersSelection()
		{
			var importer = OrgHeader.New(Factory);

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			importDeclaration.JE_OH_Importer = importer.PK;

			var attachJobDeclaration = GenerateLicenseJobDeclarationModuleCollection.GetListForGenerateImportLicense(importDeclaration);

			var importerFilter = attachJobDeclaration.FilterBusinessObjectDefaults["Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"];

			CombineAssertions(() =>
			{
				AssertEquals("filter: Importer/Supplier", importer.PK, importerFilter.Value);
				AssertEquals("filter: Importer/Supplier", false, importerFilter.IsRemovable);
			});
		}

		public void TestGetExtraNotification()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "C1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "C2";

			var importDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			importDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			importDeclaration.JE_OH_Importer = orgHeader1.PK;

			var generateCollection = new GenerateLicenseJobDeclarationModuleCollection(importDeclaration);

			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = jobDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			jobDeclaration.JE_OH_Importer = orgHeader2.PK;
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var notificationProvider = generateCollection as IFilterModuleExtraNotificationProvider;
			Factory.Save();
			AssertContains($"A Job selected from here must be an Import License and matches the Import Declaration related.", notificationProvider.GetExtraNotification(jobDeclaration).Message);

			jobDeclaration.JE_OH_Importer = orgHeader1.PK;
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertContains($"A Job selected from here must be an Import License and matches the Import Declaration related.", notificationProvider.GetExtraNotification(jobDeclaration).Message);

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			AssertNull("Must be null", notificationProvider.GetExtraNotification(jobDeclaration));
		}

		public void TestSetDefaultsForNewChild()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "C1";

			var importSiscomexDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			importSiscomexDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			importSiscomexDeclaration.JE_OH_Importer = orgHeader.PK;
			importSiscomexDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			importSiscomexDeclaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Canada;

			var generatedJob = new GenerateLicenseJobDeclarationModuleCollection(importSiscomexDeclaration).AddNew();

			AssertEquals("Importer should be", orgHeader.PK, generatedJob.JE_OH_Importer);
			AssertEquals("Transport Mode should be", TransportTypeList.Codes.Sea, generatedJob.JE_TransportMode);
			AssertEquals("Cargo Provenance should be", Core.Constants.CountryCodes.Canada, generatedJob.JE_GoodsOrigin);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new GenerateLicenseJobDeclarationModuleCollection(Factory.New<JobDeclaration>());
	}
}
