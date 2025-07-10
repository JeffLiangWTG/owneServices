using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusAuthorizationUsageUpdaterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusAuthorizationUsageUpdater(null));
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryInstructionAuthorizations_SystemGenerated()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = true;
			authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryInstructionAuthorizations_Rule()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = true;
			var holder = ZGuid.NewZGuid();
			authorisationUsageUpdater.ForAuthorizationType("ABC").When(e => true).ReturnHolderAndNumber(e => (holder, "REFERENCE"));
			authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_Code, NUnit.Framework.Is.EqualTo("ABC").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_OH_Owner, NUnit.Framework.Is.EqualTo(holder));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_Number, NUnit.Framework.Is.EqualTo("REFERENCE").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryInstructionAuthorizations_Rules()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = true;
			var holder1 = ZGuid.NewZGuid();
			var holder2 = ZGuid.NewZGuid();
			authorisationUsageUpdater.ForAuthorizationType("ABC").When(e => true).ReturnHolderAndNumber(e => new[] { (holder1, new ZString("REFERENCE1")), (holder2, new ZString("REFERENCE2")) });
			authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_Code, NUnit.Framework.Is.EqualTo("ABC").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_OH_Owner, NUnit.Framework.Is.EqualTo(holder1));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_Number, NUnit.Framework.Is.EqualTo("REFERENCE1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[1].AGC_OH_Owner, NUnit.Framework.Is.EqualTo(holder2));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[1].AGC_Number, NUnit.Framework.Is.EqualTo("REFERENCE2").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryInstructionAuthorizations_RuleDoesNotTouchUserCreated()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var userCreatedUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			userCreatedUsage.AGC_Code = "ABC";
			userCreatedUsage.AGC_Number = "USERREFERENCE";
			userCreatedUsage.AGC_IsSystemGenerated = false;

			var holder = ZGuid.NewZGuid();
			authorisationUsageUpdater.ForAuthorizationType("ABC").When(e => true).ReturnHolderAndNumber(e => (holder, "REFERENCE"));
			authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_Code, NUnit.Framework.Is.EqualTo("ABC").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages[0].AGC_Number, NUnit.Framework.Is.EqualTo("USERREFERENCE").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryInstructionAuthorizations_UserCreated()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_IsSystemGenerated = false;
			authorizationUsage.AGC_Code = "AAA";
			authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(new ZString[] { "AAA" }, NUnit.Framework.Is.EquivalentTo(entryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Select(x => x.AGC_Code)));
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryInstructionAuthorizations_MultipleInstructions()
		{
			var entryInstruction1 = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = true;
			var entryInstruction2 = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = true;
			entryInstruction2.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = true;
			entryInstruction2.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = false;
			authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryInstruction1.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(0), "All system generated deleted");
				NUnit.Framework.Assert.That(entryInstruction2.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(1), "Single user entered left");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			authorisationUsageUpdater = new CusAuthorizationUsageUpdater(jobDeclaration);
		}
		JobDeclaration jobDeclaration;
		CusAuthorizationUsageUpdater authorisationUsageUpdater;
	}
}
