using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ConsolidatedDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCongruency()
		{
			const string Message = "Consolidated declarations must be of the same Importer, Transport Mode, Dec Type, Master Bill and Discharge ETA.";
			CombineAssertions(() =>
			{
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for congruent declarations", consolidatedDeclaration, Message);
				consolidatedDeclaration.JobDeclarations[1].JE_OH_Importer = Factory.New<OrgHeader>().PK;
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Importer", consolidatedDeclaration, Message);
				consolidatedDeclaration.JobDeclarations[1].JE_OH_Importer = consolidatedDeclaration.JobDeclarations[0].JE_OH_Importer;
				consolidatedDeclaration.JobDeclarations[1].JE_TransportMode = "@@@";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Transport Mode", consolidatedDeclaration, Message);
				consolidatedDeclaration.JobDeclarations[1].JE_TransportMode = consolidatedDeclaration.JobDeclarations[0].JE_TransportMode;
				consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = "@@@";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Dec Type", consolidatedDeclaration, Message);
				consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = consolidatedDeclaration.JobDeclarations[0].JE_MessageSubType;
				consolidatedDeclaration.JobDeclarations[1].JE_MasterBill = "@@@";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Master Bill", consolidatedDeclaration, Message);
				consolidatedDeclaration.JobDeclarations[1].JE_MasterBill = consolidatedDeclaration.JobDeclarations[0].JE_MasterBill;
				consolidatedDeclaration.JobDeclarations[1].JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Discharge ETA", consolidatedDeclaration, Message);
				consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for empty selection", consolidatedDeclaration, Message);
				consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for single selection", consolidatedDeclaration, Message);
			});
		}

		public void TestMaxLineCountValidation()
		{
			foreach (var declaration in consolidatedDeclaration.JobDeclarations)
			{
				_ = declaration.ActiveEntryHeaders.AddNew();

				foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
				{
					_ = entryHeader.MergedLines.AddNew();
					_ = entryHeader.MergedLines.AddNew();
				}
			}

			using (AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12))
			{
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowErrorContaining(consolidatedDeclaration, "The total number of entry lines");
				var newDeclaration = ConsolidatedDeclarationTestHelper.CreateJobDeclarationReadyForConsolidation<BaseJobDeclaration>(Factory);
				newDeclaration.ActiveEntryHeaders[0].MergedLines.AddNew();
				consolidatedDeclaration.JobDeclarations.Add(newDeclaration);

				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Has error when number of lines is greater than the limit", consolidatedDeclaration,
			"The total number of entry lines (13) has exceeded the maximum number Customs accepts and this consolidated entry will fail. Currently the maximum number Customs accepts is 12.");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
		}

		ConsolidatedDeclaration consolidatedDeclaration;
	}
}
