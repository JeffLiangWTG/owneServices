using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestParent()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestValidateEntryLineQuantitiesNotApplyForCADEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.NotMerge;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			for (var i = 0; i <= 4000; i++)
			{
				entry.AllEntryLines.AddNew();
			}
			entry.Validation.ValidateAll();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(4001, entry.MergedLines.Count);
			AssertNoRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.");
		}

		public void TestValidateEntryLineQuantitiesWhenB3MergeByIsNON()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.NotMerge;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			for (var i = 0; i <= 4000; i++)
			{
				entry.AllEntryLines.AddNew();
			}
			entry.Validation.ValidateAll();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(4001, entry.MergedLines.Count);
			AssertHasRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.");

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			entry.Validation.ValidateAll();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(4001, entry.MergedLines.Count);
			AssertNoRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entry.Validation.ValidateAll();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(4001, entry.MergedLines.Count);
			AssertNoRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.");
		}

		public void TestValidateEntryLineQuantities()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.ProductNumber;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			for (var i = 0; i <= 4000; i++)
			{
				entry.AllEntryLines.AddNew();
			}
			entry.Validation.ValidateAll();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(4001, entry.MergedLines.Count);
			AssertHasRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please reduce the number of entry lines and create an additional entry if necessary.");

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			entry.Validation.ValidateAll();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(4001, entry.MergedLines.Count);
			AssertNoRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please reduce the number of entry lines and create an additional entry if necessary.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entry.Validation.ValidateAll();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(4001, entry.MergedLines.Count);
			AssertNoRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please reduce the number of entry lines and create an additional entry if necessary.");
		}
	}
}
