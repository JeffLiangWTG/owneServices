using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Customs.CA.Business.InterfaceImplementations.Testing
{
	sealed class JobDeclarationCustomsChargesTest : TestCaseWithFactory
	{
		public void TestIsCustomsChargesActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var decAsICustomsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration);
			AssertEquals("Declaration.IsActive - from ICustomsCharges", true, decAsICustomsCharges.IsActive);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Declaration.IsActive - from ICustomsCharges", false, decAsICustomsCharges.IsActive);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("Declaration.IsActive - from ICustomsCharges", true, decAsICustomsCharges.IsActive);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("Declaration.IsActive - from ICustomsCharges", true, decAsICustomsCharges.IsActive);
		}

		public void TestGetEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var customsCharges = new JobDeclarationCustomsChargesHelper(declaration);
			AssertEquals("null only", 1, customsCharges.GetEntriesExposed.Length);
			AssertNull("is a null object", customsCharges.GetEntriesExposed[0]);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertEquals("Still 1 null header", 1, customsCharges.GetEntriesExposed.Length);
			AssertNull("is a null object", customsCharges.GetEntriesExposed[0]);
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals("1 header", 1, customsCharges.GetEntriesExposed.Length);
			AssertEquals("B3 header", entryHeader2.PK, customsCharges.GetEntriesExposed[0].PK);
		}

		public void TestGetEntries_B2B3X()
		{
			AssertGetEntries_B2B3X(JobMessageTypeList.Codes.B2Adjustments);
			AssertGetEntries_B2B3X(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertGetEntries_B2B3X(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var customsCharges = new JobDeclarationCustomsChargesHelper(declaration);
			AssertEquals("null only", 0, customsCharges.GetEntriesExposed.Length);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Still 1 null header", 1, customsCharges.GetEntriesExposed.Length);
			AssertNotNull("is a null object", customsCharges.GetEntriesExposed[0]);
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("2 header", 2, customsCharges.GetEntriesExposed.Length);
		}
	}
}
