using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestHumanReadableName()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = "ABC";
			AssertEquals("Entry Instruction ABC", entryInstruction.HumanReadableName);
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestGuaranteeManagement()
		{
			CombineAssertions(() =>
			{
				var helper = new GuaranteeTestHelper(Factory, 10000m);
				const string entry1Consume = "Entry 1 Consume";
				var guarantee = helper.CreateValidNotLinkedGuarantee(entry1Consume, amount: 6000m, appId: "Job Number 1");
				guarantee.LinkOrUnlink();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 1 Consume 6,000 (available 4,000)", helper.CusGuarantee, 4000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"));
				guarantee.Instruction.ReleaseGuarantees.AddNew();
				var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee.PW_BondAmount = 2000m;
				const string entry1Release = "Entry 1 Release";
				releaseGuarantee.PW_BondNumber2 = entry1Release;
				releaseGuarantee.PW_BondEffectiveDate = ZDate.BrettsBirthday;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 1 Release 2,000 (available 6,000)", helper.CusGuarantee, 6000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"));
				const string entry2Consume = "Entry 2 Consume";
				var guarantee2 = helper.CreateValidNotLinkedGuarantee(entry2Consume, amount: 5000m, appId: "Job Number 2");
				guarantee2.LinkOrUnlink();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 2 Consume 5,000 (available 1,000)", helper.CusGuarantee, 1000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"));
				releaseGuarantee.PW_BondAmount = 500m;
				AssertExceptionThrown<ZCannotSaveException>("Entry 1 Change Release to 500 - Don't allow saving as it will cause available to become -500.", "The guarantee 12345 available amount will be exceeded by 500. The total is 10000 and the available is 1000.", () =>
				{
					Factory.Save();
				});
				releaseGuarantee.PW_BondAmount = 4000m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 1 Change Release to 4,000 (available 3,000)", helper.CusGuarantee, 3000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"), (entry1Release, 2000m, "Job Number 1"));
				GuaranteeTestHelper.AdjustCusGuaranteeAmount(helper.CusGuarantee, 2000m, "ADJ by 2000");
				GuaranteeTestHelper.AssertGuaranteeTransactions("Manual adjustment 2,000 (available 5,000)", helper.CusGuarantee, 5000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"), (entry1Release, 2000m, "Job Number 1"), ("ADJ by 2000", 2000m, "Job Number"));
				releaseGuarantee.PW_BondAmount = 7000m;
				guarantee.Validation.ValidateRemaining();
				AssertHasError("Entry 1 Change Release to 7,000 - Don't allow saving as it only consume 6,000", guarantee.RemainingInfo, "Remaining cannot be negative.");
				releaseGuarantee.PW_BondAmount = 6000m;
				guarantee.Validation.ValidateRemaining();
				AssertNoError("Entry 1 Change Release to 6,000", guarantee.RemainingInfo, "Remaining cannot be negative.");
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 1 Change Release to 6,000 (available 7,000)", helper.CusGuarantee, 7000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"), (entry1Release, 2000m, "Job Number 1"), ("ADJ by 2000", 2000m, "Job Number"), (entry1Release, 2000m, "Job Number 1"));
				guarantee2.PW_BondAmount = 1000m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 2 Change Consume 1,000 (available 11,000)", helper.CusGuarantee, 11000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"), (entry1Release, 2000m, "Job Number 1"), ("ADJ by 2000", 2000m, "Job Number"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, 4000m, "Job Number 2"));
				guarantee2.LinkOrUnlink();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 2 unlink (available 12,000)", helper.CusGuarantee, 12000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"), (entry1Release, 2000m, "Job Number 1"), ("ADJ by 2000", 2000m, "Job Number"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, 4000m, "Job Number 2"), (entry2Consume, 1000m, "Job Number 2"));
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestGuaranteeManagement_ReversingTransactionChecksBursting()
		{
			CombineAssertions(() =>
			{
				var helper = new GuaranteeTestHelper(Factory, 10000m);
				const string entry1Consume = "Entry 1 Consume";
				var guarantee = helper.CreateValidNotLinkedGuarantee(entry1Consume, amount: 6000m, appId: "Job Number 1");
				guarantee.LinkOrUnlink();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 1 Consume 6,000 (available 4,000)", helper.CusGuarantee, 4000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"));
				guarantee.Instruction.ReleaseGuarantees.AddNew();
				var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee.PW_BondAmount = 2000m;
				const string entry1Release = "Entry 1 Release";
				releaseGuarantee.PW_BondNumber2 = entry1Release;
				releaseGuarantee.PW_BondEffectiveDate = ZDate.BrettsBirthday;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 1 Release 2,000 (available 6,000)", helper.CusGuarantee, 6000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"));
				const string entry2Consume = "Entry 2 Consume";
				var guarantee2 = helper.CreateValidNotLinkedGuarantee(entry2Consume, amount: 5000m, appId: "Job Number 2");
				guarantee2.LinkOrUnlink();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Entry 2 Consume 5,000 (available 1,000)", helper.CusGuarantee, 1000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"));
				releaseGuarantee.Delete();
				AssertExceptionThrown<ZCannotSaveException>("Delete Entry 1 Change Release - Don't allow saving as it will cause available to become -1000.", "The guarantee 12345 available amount will be exceeded by 1000. The total is 10000 and the available is 1000.", () =>
				{
					Factory.Save();
				});
				GuaranteeTestHelper.AssertGuaranteeTransactions("Unchanged", helper.CusGuarantee, 1000m, ("Opening", 10000m, "Job Number"), (entry1Consume, -6000m, "Job Number 1"), (entry1Release, 2000m, "Job Number 1"), (entry2Consume, -5000m, "Job Number 2"));
			});
		}

		public void TestGetGuarantee_NotCreateIfMissing()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				AssertNotNull("Pre-condition", instruction.GetGuarantee(false));
				instruction.GetGuarantee(false).Delete();
				AssertNull("Not create if missing", instruction.GetGuarantee(false));
			});
		}

		public void TestGuarantee_CreateNewOnExistingInstruction()
		{
			Factory.RefreshEnabled = false;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			Factory.Save();
			Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.CusBondDetail WHERE PW_PK ='{instruction.Guarantee.PK}'");
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var instructionInNewFactory = newFactory.Load<CusEntryInstruction>(instruction.PK);
			_ = instructionInNewFactory.Guarantee;
			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var instructionInNewFactory2 = newFactory2.Load<CusEntryInstruction>(instruction.PK);
			AssertNull(instructionInNewFactory2.Guarantee);
			instructionInNewFactory.UnlockGuaranteeManagementMutex();
		}

		public void TestGuaranteeManagementMutex()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var mutex = instruction.GuaranteeManagementMutex;
			CombineAssertions(() =>
			{
				AssertEquals("MutexID", MutexIDs.GuaranteeManagement, mutex.MutexID);
				AssertEquals("RecordIdentifier", instruction.PK.ToString(), mutex.RecordIdentifier);
			});
		}

		public void TestGuaranteeManagementMutexText()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertStartsWith("GuaranteeManagementMutexText", "The Guarantee is currently being edited by", instruction.GuaranteeManagementMutexText);
		}

		public void TestLockGuaranteeManagementMutex_InstructionIsNotInDatabase()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(true, instruction.LockGuaranteeManagementMutex());
			instruction.UnlockGuaranteeManagementMutex();
		}

		public void TestLockGuaranteeManagementMutex_ReloadGuaranteeManagementData()
		{
			Factory.RefreshEnabled = false;
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			releaseGuarantee.PW_BondAmount = 0.01m;
			releaseGuarantee.PW_BondNumber2 = "XYZ";
			releaseGuarantee.PW_BondEffectiveDate = ZDate.BrettsBirthday;
			Factory.Save();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var instructionInNewFactory = newFactory.Load<CusEntryInstruction>(guarantee.Instruction.PK);
			instructionInNewFactory.Guarantee.PW_BondAmount = 4.12m;
			instructionInNewFactory.ReleaseGuarantees.Single(x => x.PW_BondNumber2 == "XYZ").Delete();
			var releaseGuaranteeAddedInNewFactory = instructionInNewFactory.ReleaseGuarantees.AddNew();
			releaseGuaranteeAddedInNewFactory.PW_BondAmount = 2.13m;
			releaseGuaranteeAddedInNewFactory.PW_BondNumber2 = "VVV";
			releaseGuaranteeAddedInNewFactory.PW_BondEffectiveDate = ZDate.BrettsBirthday;
			newFactory.Save();
			guarantee.Instruction.LockGuaranteeManagementMutex();
			CombineAssertions(() =>
			{
				AssertEquals("Reloaded", 4.12m, guarantee.PW_BondAmount);
				AssertEquals("releaseGuarantee.ReadOnly", true, releaseGuarantee.ReadOnly);
				AssertHasRowError("releaseGuarantee.RowError", releaseGuarantee, "This row is no longer in the database; please reload the job to pick up the latest changes.");
				var releaseGuaranteeReloaded = guarantee.Instruction.ReleaseGuarantees.Single(x => x.PW_BondNumber2 == "VVV");
				AssertEquals("releaseGuaranteeReloaded.PW_BondAmount", 2.13m, releaseGuaranteeReloaded.PW_BondAmount);
				AssertEquals("releaseGuaranteeReloaded.PW_BondEffectiveDate", ZDate.BrettsBirthday, releaseGuaranteeReloaded.PW_BondEffectiveDate);
			});
			guarantee.Instruction.UnlockGuaranteeManagementMutex();
		}

		public void TestLockGuaranteeManagementMutex_WarnGuaranteeManagementMutexText()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			Factory.Save();
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, instruction.PK.ToString()))
			{
				mutex.Lock();
				instruction.LockGuaranteeManagementMutex();
				CombineAssertions(() =>
				{
					AssertStartsWith("Warning", "The Guarantee is currently being edited by", messageInitiator.Warning);
					AssertEquals("WarningCaption", "Concurrency Warning", messageInitiator.WarningCaption);
				});
				mutex.Unlock();
			}
		}

		public void TestLockGuaranteeManagementMutex_NotWarnMutexText()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			Factory.Save();
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, instruction.PK.ToString()))
			{
				mutex.Lock();
				instruction.LockGuaranteeManagementMutex(false);
				CombineAssertions(() =>
				{
					AssertNullOrEmpty("Warning", messageInitiator.Warning);
					AssertNullOrEmpty("WarningCaption", messageInitiator.WarningCaption);
				});
				mutex.Unlock();
			}
		}

		public void TestUnlockGuaranteeManagementMutex()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var mutex = instruction.GuaranteeManagementMutex;
			mutex.Lock();
			instruction.UnlockGuaranteeManagementMutex();
			AssertEquals(false, mutex.IsLocked);
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestAddGuaranteeTransactions_ConsumingGuarantee_AddDifferenceTransaction()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			CombineAssertions(() =>
			{
				GuaranteeTestHelper.AssertGuaranteeTransactions("Initial", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));

				guarantee.PW_BondAmount = 4.56m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("4.56m", helper.CusGuarantee, 95.44m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference", -1.42m, "Job Number"));

				guarantee.PW_BondAmount = 0.98m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("0.98m", helper.CusGuarantee, 99.02m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference", -1.42m, "Job Number"), ("Entry Reference", 3.58m, "Job Number"));
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestAddGuaranteeTransactions_ConsumingGuarantee_AddDifferenceTransaction_AmountIsMore_Bursting()
		{
			const string message = "The guarantee 12345 available amount will be exceeded by 100. The total is 100 and the available is 96.86.";

			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			CombineAssertions(() =>
			{
				GuaranteeTestHelper.AssertGuaranteeTransactions("Initial", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				guarantee.PW_BondAmount = 200m;
				AssertExceptionThrown<ZCannotSaveException>("Cannot save", message, () =>
				{
					Factory.Save();
				});
				GuaranteeTestHelper.AssertGuaranteeTransactions("Unchanged", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				AssertHasWarning("PW_BondAmountInfo", guarantee.PW_BondAmountInfo, message);
				var instruction = guarantee.Instruction;
				var guaranteeManagementMutex = instruction.GuaranteeManagementMutex;
				AssertEquals("Still Locked", true, guaranteeManagementMutex.IsLocked && guaranteeManagementMutex.HasLock);
				instruction.UnlockGuaranteeManagementMutex();
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestAddGuaranteeTransactions_ReleaseGuarantee_AddTransaction()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			Factory.Save();
			CombineAssertions(() =>
			{
				GuaranteeTestHelper.AssertGuaranteeTransactions("Initial", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee.PW_BondNumber2 = "Entry Reference 2";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Release guarantee saved", helper.CusGuarantee, 97.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"));
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestAddGuaranteeTransactions_ReleaseGuarantee_AddDifferenceTransaction()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			CombineAssertions(() =>
			{
				GuaranteeTestHelper.AssertGuaranteeTransactions("Initial", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee.PW_BondNumber2 = "Entry Reference 2";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Release guarantee saved", helper.CusGuarantee, 97.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"));
				releaseGuarantee.PW_BondAmount = 2.1m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Amount amended", helper.CusGuarantee, 98.96m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"), ("Entry Reference 2", 1.1m, "Job Number"));
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestAddGuaranteeTransactions_ReleaseGuarantee_DeletedByOthers()
		{
			Factory.RefreshEnabled = false;
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			CombineAssertions(() =>
			{
				GuaranteeTestHelper.AssertGuaranteeTransactions("Initial", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee.PW_BondNumber2 = "Entry Reference 2";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Release guarantee saved", helper.CusGuarantee, 97.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"));
				var newFactory = new BusinessObjectFactory();
				var instructionInNewFactory = newFactory.Load<CusEntryInstruction>(guarantee.Instruction.PK);
				instructionInNewFactory.ReleaseGuarantees[0].Delete();
				newFactory.Save();
				helper.CusGuarantee.ReloadLatestTransactions();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Reversing", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"), ("Entry Reference 2", -1m, "Job Number"));
				releaseGuarantee.PW_BondAmount = 2.1m;
				try
				{
					Factory.Save();
				}
				catch (ZSaveConcurrencyException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Not exceptional", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"), ("Entry Reference 2", -1m, "Job Number"));
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestAddGuaranteeTransactions_ReleaseGuarantee_AddReversingTransaction()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			CombineAssertions(() =>
			{
				GuaranteeTestHelper.AssertGuaranteeTransactions("Initial", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee.PW_BondNumber2 = "Entry Reference 2";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Release guarantee saved", helper.CusGuarantee, 97.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"));
				releaseGuarantee.Delete();
				Factory.Save();
				GuaranteeTestHelper.AssertGuaranteeTransactions("Reversing transaction added", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference 2", 1m, "Job Number"), ("Entry Reference 2", -1m, "Job Number"));
			});
		}

		public void TestAddGuaranteeTransactions_CusGuarantee_OnAddTransactionLockedByOthers()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "CPH" + guarantee.CusGuarantee.PK))
			{
				mutex.Lock();
				guarantee.PW_BondAmount = 10m;
				try
				{
					Factory.Save();
				}
				catch (ZCannotSaveException e)
				{
					AssertContains("is already in the process of adding transaction for", e.Message);
				}
				mutex.Unlock();
				guarantee.Instruction.UnlockGuaranteeManagementMutex();
			}
		}

		public void TestCanDelete_IfHasLinkedGuarantee()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				instruction.Guarantee.PW_CPH_Guarantee = CreateCusGuarantee().PK;
				AssertEquals("PW_CPH_Guarantee entered, not linked", true, instruction.CanDelete);
				instruction.Guarantee.PW_Status = GuaranteeStatusList.Codes.Linked;
				AssertEquals("PW_CPH_Guarantee entered, linked", false, instruction.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete_IfHasLinkedGuarantee()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "CEI";
			instruction.Guarantee.PW_CPH_Guarantee = CreateCusGuarantee().PK;
			instruction.Guarantee.PW_Status = GuaranteeStatusList.Codes.Linked;
			AssertEquals("Entry Instruction with Declaration Type CEI has a linked guarantee and cannot be deleted.", instruction.ReasonForNotAbleToDelete);
		}

		public void TestAllInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure()
		{
			var procedure1 = CusBondDetailValidationTest.CreateNotConsumedAndNotReleasedProcedure(Factory);
			var procedure2 = CusBondDetailValidationTest.CreateNotConsumedAndNotReleasedProcedure(Factory);
			procedure2.ZZ6_ProcedureCode = "YY";
			procedure2.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure2.ZZ6_IsGuaranteeReleased = YesNoList.Codes.No;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No invoice line", false, instruction.AllInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_Procedure = procedure2.ZZ6_ProcedureCode;
				AssertEquals("Any invoice line", false, instruction.AllInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure);
				invoiceLine1.JI_Procedure = procedure1.ZZ6_ProcedureCode;
				AssertEquals("Invoice line uses wrong guarantee procedure", false, instruction.AllInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure);
				procedure1.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
				procedure1.ZZ6_IsGuaranteeReleased = YesNoList.Codes.No;
				AssertEquals("Invoice line uses right guarantee procedure", true, instruction.AllInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure);
			});
		}

		public void TestAllInvoiceLinesUseReleaseGuaranteeProcedure()
		{
			var procedure1 = CusBondDetailValidationTest.CreateNotConsumedAndNotReleasedProcedure(Factory);
			var procedure2 = CusBondDetailValidationTest.CreateNotConsumedAndNotReleasedProcedure(Factory);
			procedure2.ZZ6_ProcedureCode = "YY";
			procedure2.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No invoice line", false, instruction.AllInvoiceLinesUseReleaseGuaranteeProcedure);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_Procedure = procedure2.ZZ6_ProcedureCode;
				AssertEquals("Any invoice line", false, instruction.AllInvoiceLinesUseReleaseGuaranteeProcedure);
				invoiceLine1.JI_Procedure = procedure1.ZZ6_ProcedureCode;
				AssertEquals("Invoice line uses wrong guarantee procedure", false, instruction.AllInvoiceLinesUseReleaseGuaranteeProcedure);
				procedure1.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
				AssertEquals("Invoice line uses right guarantee procedure", true, instruction.AllInvoiceLinesUseReleaseGuaranteeProcedure);
			});
		}

		public void TestCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CustomsValue = 3.14m;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals(3.14m, instruction.CustomsValue);
		}

		public void TestNetWeightKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 1.12m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals(1.12m, instruction.NetWeightKilograms);
		}

		public void TestCustomsQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 2.24m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals(2.24m, instruction.CustomsQuantity);
		}

		public void TestRemainingCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CustomsValue = 3.14m;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;

			instruction.RiskManagements.AddNew().CSI_Value = 0.03m;
			instruction.RiskManagements.AddNew().CSI_Value = 0.9m;
			AssertEquals(2.21m, instruction.RemainingCustomsValue);
		}

		public void TestRemainingNetWeightKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 1.12m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;

			instruction.RiskManagements.AddNew().CSI_Quantity = 0.03m;
			instruction.RiskManagements.AddNew().CSI_Quantity = 0.9m;
			AssertEquals(0.19m, instruction.RemainingNetWeightKilograms);
		}

		public void TestRemainingCustomsQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 2.24m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;

			instruction.RiskManagements.AddNew().CSI_Quantity2 = 0.03m;
			instruction.RiskManagements.AddNew().CSI_Quantity2 = 0.9m;
			AssertEquals(1.31m, instruction.RemainingCustomsQuantity);
		}

		public void TestHasRiskValue_RemainingCustomsValue()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;

				var riskManagement1 = instruction.RiskManagements.AddNew();
				riskManagement1.CSI_Value = 2m;
				riskManagement1.CSI_Quantity = 1m;
				riskManagement1.CSI_Quantity2 = 1.5m;

				var riskManagement2 = instruction.RiskManagements.AddNew();
				riskManagement2.CSI_Value = 7m;
				riskManagement2.CSI_Quantity = 4m;
				riskManagement2.CSI_Quantity2 = 1.5m;

				CombineAssertions(() =>
				{
					AssertGreaterThan(instruction.RemainingCustomsValue, ZDecimal.Zero);
					AssertEquals(ZDecimal.Zero, instruction.RemainingNetWeightKilograms);
					AssertEquals(ZDecimal.Zero, instruction.RemainingCustomsQuantity);
					AssertEquals(true, instruction.HasRiskValue);
				});
			}
		}

		public void TestHasRiskValue_RemainingNetWeightKilograms()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;

				var riskManagement1 = instruction.RiskManagements.AddNew();
				riskManagement1.CSI_Value = 3m;
				riskManagement1.CSI_Quantity = 1m;
				riskManagement1.CSI_Quantity2 = 1.5m;

				var riskManagement2 = instruction.RiskManagements.AddNew();
				riskManagement2.CSI_Value = 7m;
				riskManagement2.CSI_Quantity = 2.6m;
				riskManagement2.CSI_Quantity2 = 1.5m;

				CombineAssertions(() =>
				{
					AssertEquals(ZDecimal.Zero, instruction.RemainingCustomsValue);
					AssertGreaterThan(instruction.RemainingNetWeightKilograms, ZDecimal.Zero);
					AssertEquals(ZDecimal.Zero, instruction.RemainingCustomsQuantity);
					AssertEquals(true, instruction.HasRiskValue);
				});
			}
		}

		public void TestHasRiskValue_RemainingCustomsQuantity()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 6m;

				var riskManagement1 = instruction.RiskManagements.AddNew();
				riskManagement1.CSI_Value = 3m;
				riskManagement1.CSI_Quantity = 1m;
				riskManagement1.CSI_Quantity2 = 1.5m;

				var riskManagement2 = instruction.RiskManagements.AddNew();
				riskManagement2.CSI_Value = 7m;
				riskManagement2.CSI_Quantity = 4m;
				riskManagement2.CSI_Quantity2 = 1.5m;

				CombineAssertions(() =>
				{
					AssertEquals(ZDecimal.Zero, instruction.RemainingCustomsValue);
					AssertEquals(ZDecimal.Zero, instruction.RemainingNetWeightKilograms);
					AssertGreaterThan(instruction.RemainingCustomsQuantity, ZDecimal.Zero);
					AssertEquals(true, instruction.HasRiskValue);
				});
			}
		}

		public void TestHasRiskValue_NoRemainingValue()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;

				var riskManagement1 = instruction.RiskManagements.AddNew();
				riskManagement1.CSI_Value = 3m;
				riskManagement1.CSI_Quantity = 1m;
				riskManagement1.CSI_Quantity2 = 1.5m;

				var riskManagement2 = instruction.RiskManagements.AddNew();
				riskManagement2.CSI_Value = 7m;
				riskManagement2.CSI_Quantity = 4m;
				riskManagement2.CSI_Quantity2 = 1.5m;

				CombineAssertions(() =>
				{
					AssertEquals(ZDecimal.Zero, instruction.RemainingCustomsValue);
					AssertEquals(ZDecimal.Zero, instruction.RemainingNetWeightKilograms);
					AssertEquals(ZDecimal.Zero, instruction.RemainingCustomsQuantity);
					AssertEquals(false, instruction.HasRiskValue);
				});
			}
		}

		public void TestEntryNumberValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("", instruction.EntryNumber);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "EN0001";
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals("EN0001", instruction.EntryNumber);
		}

		public void TestEntryNumber()
		{
			AssertEquals("Entry Number", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.EntryNumber)).Caption);
		}

		public void TestCustomsValue_Caption()
		{
			AssertEquals("Customs Value", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.CustomsValue)).Caption);
		}

		public void TestNetWeightKilograms_Caption()
		{
			AssertEquals("Net Weight", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.NetWeightKilograms)).Caption);
		}

		public void TestCustomsQuantity_Caption()
		{
			AssertEquals("Customs Quantity", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.CustomsQuantity)).Caption);
		}

		public void TestCustomsValue_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryInstruction), nameof(CusEntryInstruction.CustomsValue), false, x => x.DecimalPlaces == 2);
		}

		public void TestNetWeightKilograms_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryInstruction), nameof(CusEntryInstruction.NetWeightKilograms), false, x => x.DecimalPlaces == 3);
		}

		public void TestCustomsQuantity_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryInstruction), nameof(CusEntryInstruction.CustomsQuantity), false, x => x.DecimalPlaces == 5);
		}

		public void TestNetWeightUQ()
		{
			AssertEquals(Core.Constants.Weight.Kilograms, Factory.New<CusEntryInstruction>().NetWeightUQ);
		}

		public void TestRemainingCustomsValue_Caption()
		{
			AssertEquals("Customs Value", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.RemainingCustomsValue)).Caption);
		}

		public void TestRemainingNetWeightKilograms_Caption()
		{
			AssertEquals("Net Weight", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.RemainingNetWeightKilograms)).Caption);
		}

		public void TestRemainingCustomsQuantity_Caption()
		{
			AssertEquals("Customs Quantity", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.RemainingCustomsQuantity)).Caption);
		}

		public void TestRemainingCustomsValue_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryInstruction), nameof(CusEntryInstruction.RemainingCustomsValue), false, x => x.DecimalPlaces == 2);
		}

		public void TestRemainingNetWeightKilograms_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryInstruction), nameof(CusEntryInstruction.RemainingNetWeightKilograms), false, x => x.DecimalPlaces == 3);
		}

		public void TestRemainingCustomsQuantity_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryInstruction), nameof(CusEntryInstruction.RemainingCustomsQuantity), false, x => x.DecimalPlaces == 5);
		}

		public void TestRiskManagements()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("IsRegisteredEditableChildObject", true, entryInstruction.IsRegisteredEditableChildObject(entryInstruction.RiskManagements));
		}

		public void TestRiskManagements_ChildEditableAttribute()
		{
			AssertHasCustomAttribute<ChildEditableAttribute>(typeof(CusEntryInstruction), nameof(CusEntryInstruction.RiskManagements), false, x => x.Value);
		}

		public void TestICusSupportingInfoTypeSupporter_GetCusSupportingInfoTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var provider = (Integration.Customs.ICusSupportingInfoTypeSupporter)declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(typeof(RiskManagement), provider.GetCusSupportingInfoTypes()[Constants.CusSupportingInfoTypes.RiskManagement]);
		}

		public void TestICusSupportingInfoTypeSupporter_GetFetchStrategies()
		{
			var declaration = Factory.New<JobDeclaration>();
			var provider = (Integration.Customs.ICusSupportingInfoTypeSupporter)declaration.CustomsEntryInstructions.AddNew();
			AssertType<CusSupportingInfoTypeSupporterFetchStrategy>(provider.GetFetchStrategies().Single());
		}

		public void TestASY_LocalReferenceNumber_Caption()
		{
			AssertEquals("Local Reference Number", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusEntryInstruction>().ASY_LocalReferenceNumberInfo).Caption);
		}

		public void TestASY_LocalReferenceNumber_NoEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_LocalReferenceNumber = "NUMBER1";
			AssertEquals("CusEntryInstruction.ASY_LocalReferenceNumber", "NUMBER1", instruction.ASY_LocalReferenceNumber);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var instruction2 = newFactory.Load<CusEntryInstruction>(instruction.PK);
			AssertEquals("CusEntryInstruction.ASY_LocalReferenceNumber - saved", "NUMBER1", instruction2.ASY_LocalReferenceNumber);

			instruction2.ASY_LocalReferenceNumber = "";
			AssertEquals("CusEntryInstruction.ASY_LocalReferenceNumber can change to be empty when not merge yet", "", instruction2.ASY_LocalReferenceNumber);
		}

		public void TestASY_LocalReferenceNumber_Merge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "B001000";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_LocalReferenceNumber = "DUPLICATE";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("DUPLICATE", entry.CH_BGMReference);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B002000";
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;
			AssertEquals(0, declaration2.ActiveEntryHeaders.Count);

			CombineAssertions("Local Reference Number Changed", () =>
			{
				instruction2.OnLocalReferenceNumberChangedToReMerge += OnLocalReferenceNumberChangedToReMerge_NotCancel;

				instruction2.ASY_LocalReferenceNumber = "NUMBER1";
				declaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration2.DoMerge();
				var entry2 = declaration2.ActiveEntryHeaders[0];
				AssertEquals("DoMerge and 1 entry", 1, declaration2.ActiveEntryHeaders.Count);
				AssertEquals("Entry number was set", "NUMBER1", entry2.CH_BGMReference);
				Factory.Save();

				instruction2.ASY_LocalReferenceNumber = "NUMBER1";
				AssertEquals("ASY_LocalReferenceNumber not chagned and keep the same", "NUMBER1", instruction2.ASY_LocalReferenceNumber);

				instruction2.ASY_LocalReferenceNumber = "NUMBER2";
				AssertEquals("ASY_LocalReferenceNumber was chagned", "NUMBER2", instruction2.ASY_LocalReferenceNumber);
				AssertEquals("Entry number was changed", "NUMBER2", entry2.CH_BGMReference);

				instruction2.ASY_LocalReferenceNumber = "duplicate";
				AssertHasErrors("Has validation error that there's another Job B001000 has the same entry BGMReference", instruction2.ASY_LocalReferenceNumberInfo);
				AssertEquals("ASY_LocalReferenceNumber was chagned and have validation error", "duplicate", instruction2.ASY_LocalReferenceNumber);
				AssertEquals("Entry reference not changed due to ASY_LocalReferenceNumber validation error", "NUMBER2", entry2.CH_BGMReference);

				instruction2.OnLocalReferenceNumberChangedToReMerge -= OnLocalReferenceNumberChangedToReMerge_NotCancel;
				instruction2.OnLocalReferenceNumberChangedToReMerge += OnLocalReferenceNumberChangedToReMerge_Cancel;

				instruction2.ASY_LocalReferenceNumber = "number3";
				AssertEquals("ASY_LocalReferenceNumber was not chagned due to merge event cancelled", "duplicate", instruction2.ASY_LocalReferenceNumber);
				AssertEquals("Entry BGMReference was not changed", "NUMBER2", entry2.CH_BGMReference);
			});
		}

		public void TestASY_LocalReferenceNumber_ForJobWithDefaultEntryBGMReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "B002000";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("B002000/1", entry.CH_BGMReference);

			CombineAssertions("Local Reference Number Changed", () =>
			{
				instruction.OnLocalReferenceNumberChangedToReMerge += OnLocalReferenceNumberChangedToReMerge_NotCancel;
				instruction.OnLocalReferenceNumberChangedToEmpty += OnLocalReferenceNumberChangedToEmpty;
				instruction.OnLocalReferenceNumberChangedWhenHasWarehouseTransaction += OnLocalReferenceNumberChangedWhenHasWarehouseTransaction;

				instruction.ASY_LocalReferenceNumber = "B002000/1";
				AssertEquals("ASY_LocalReferenceNumber was chagned", "B002000/1", instruction.ASY_LocalReferenceNumber);
				AssertEquals("Entry BGMReference still the same", "B002000/1", entry.CH_BGMReference);

				countOnLocalReferenceNumberChangedToEmpty = 0;
				instruction.ASY_LocalReferenceNumber = "";
				AssertEquals("ASY_LocalReferenceNumber can not change to be empty when have merged", "B002000/1", instruction.ASY_LocalReferenceNumber);
				AssertEquals("Entry BGMReference still the same", "B002000/1", entry.CH_BGMReference);
				AssertEquals("OnLocalReferenceNumberChangedToEmpty invoked", 1, countOnLocalReferenceNumberChangedToEmpty);

				countOnLocalReferenceNumberChangedWhenHasWarehouseTransaction = 0;
				entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				instruction.ASY_LocalReferenceNumber = "change";
				AssertEquals("ASY_LocalReferenceNumber can not change if entry header has warehouse transaction", "B002000/1", instruction.ASY_LocalReferenceNumber);
				AssertEquals("Entry BGMReference still the same", "B002000/1", entry.CH_BGMReference);
				AssertEquals("OnLocalReferenceNumberChangedWhenHasWarehouseTransaction invoked", 1, countOnLocalReferenceNumberChangedWhenHasWarehouseTransaction);
			});
		}

		void OnLocalReferenceNumberChangedToReMerge_Cancel(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

		void OnLocalReferenceNumberChangedToReMerge_NotCancel(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = false;
		}

		void OnLocalReferenceNumberChangedToEmpty(object sender, EventArgs e)
		{
			countOnLocalReferenceNumberChangedToEmpty++;
		}
		int countOnLocalReferenceNumberChangedToEmpty;

		void OnLocalReferenceNumberChangedWhenHasWarehouseTransaction(object sender, EventArgs e)
		{
			countOnLocalReferenceNumberChangedWhenHasWarehouseTransaction++;
		}
		int countOnLocalReferenceNumberChangedWhenHasWarehouseTransaction;

		public void TestASY_PortOfExit_Caption()
		{
			AssertEquals("Customs Office of Destination/Exit", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusEntryInstruction>().ASY_PortOfExitInfo).Caption);
		}

		public void TestASY_PortOfExit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, "Botswana");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsUQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Botswana, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ARIA", "Ariamsvlei", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_PortOfExit = "ARIA";
			AssertEquals("CusEntryInstruction.ASY_PortOfExit", "ARIA", instruction.ASY_PortOfExit);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var instruction2 = newFactory.Load<CusEntryInstruction>(instruction.PK);
			AssertEquals("CusEntryInstruction.ASY_PortOfExit - saved", "ARIA", instruction2.ASY_PortOfExit);
		}

		public void TestLinkedGuaranteeNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "CEI";

			AssertEquals("", entryInstruction.LinkedGuaranteeNumber);

			var guarantee = entryInstruction.Guarantee;
			var guaranteeHeader = CreateCusGuarantee();
			guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;

			AssertEquals(guaranteeHeader.CPH_Number, entryInstruction.LinkedGuaranteeNumber);
		}

		public void TestCusBondDetail()
		{
			AssertNotNull(Factory.New<CusEntryInstruction>().Guarantee);
		}

		public void TestCusBondDetailAreDeleted()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var guarantees = new List<CommonCusBondDetail>
			{
				entryInstruction.Guarantee,
				entryInstruction.ReleaseGuarantees.AddNew()
			};
			for (var i = 0; i < 10; i++)
			{
				var guarantee = Factory.New<CusBondDetail>();
				guarantee.Parent = entryInstruction;
				guarantees.Add(guarantee);
				var second = Factory.New<SecondCusBondDetail>();
				second.Parent = entryInstruction;
				guarantees.Add(second);
			}
			entryInstruction.Delete();
			guarantees.ForEach(x => AssertEquals(true, x.IsDeleted));
		}

		public void TestIsInventorySelectionEnabled()
		{
			AssertEquals("IsInventorySelectionEnabled", false, Factory.New<CusEntryInstruction>().IsInventorySelectionEnabled);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("IsInventorySelectionEnabled", false, entryInstruction.IsInventorySelectionEnabled);

			declaration.JE_MessageType = "EXP";
			AssertEquals("IsInventorySelectionEnabled", true, entryInstruction.IsInventorySelectionEnabled);

			declaration.JE_MessageType = "EXW";
			AssertEquals("IsInventorySelectionEnabled", true, entryInstruction.IsInventorySelectionEnabled);
		}

		public void TestSupportClone()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_Style = "T1";
			instruction.CEI_DateForDuty = new ZDateTime(2020, 7, 21);
			var cloneInstruction = instruction.Clone();
			var cloneIns = (CusEntryInstruction)cloneInstruction;
			AssertEquals(instruction.CEI_Style, cloneIns.CEI_Style);
			AssertEquals(instruction.CEI_DateForDuty, cloneIns.CEI_DateForDuty);

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = "T1";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 7, 21);

			var cloneDec = declaration.TemplateCopy();
			var deepCloneDec = (JobDeclaration)cloneDec;
			AssertEquals(entryInstruction.CEI_Style, deepCloneDec.CusEntryInstruction.CEI_Style);
			AssertEquals(ZDateTime.Empty, deepCloneDec.CusEntryInstruction.CEI_DateForDuty);
		}

		public void TestJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<JobDeclaration>(entryInstruction.JobDeclaration);
		}

		public void TestLookups()
		{
			AssertType<CusEntryInstructionLookups>(Factory.New<CusEntryInstruction>().Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusEntryInstructionValidation>(Factory.New<CusEntryInstruction>().Validation);
		}

		public void TestCusInBondPermitsHeaders()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var cusInBondPermitsHeaders = entryInstruction.CusInBondPermitsHeaders;
			CombineAssertions(() =>
			{
				AssertType("cusInBondPermitsHeaders type", typeof(CusInBondMoveHeaderCollection), cusInBondPermitsHeaders);
				AssertEquals("cusInBondPermitsHeaders count", 0, cusInBondPermitsHeaders.Count);
			});
		}

		public void TestCusInBondPermitsHeadersAreDeleted()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var permit = entryInstruction.CusInBondPermitsHeaders.AddNew();
			CombineAssertions(() =>
			{
				Assert("before delete", !permit.IsDeleted);
				entryInstruction.Delete();
				Assert("after delete", permit.IsDeleted);
			});
		}

		public void TestIsTransitPermitsTabPageVisible_WhenNoInvoiceLineLinked()
		{
			var entryInstruction = CreateEntryInstruction();
			AssertEquals(false, entryInstruction.IsTransitPermitsTabPageVisible);
		}

		public void TestIsTransitPermitsTabPageVisible_WhenProcedureNull()
		{
			var (entryInstruction, invoiceLine) = CreateInvoiceLineLinkedToEntryInstruction();
			CombineAssertions(() =>
			{
				AssertNull(invoiceLine.CusProcedure);
				AssertEquals(false, entryInstruction.IsTransitPermitsTabPageVisible);
			});
		}

		public void TestIsTransitPermitsTabPageVisible_WhenTransitProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var transitProcedure = helper.CreateOrFindExistingRefCusProcedure(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "10", "71", "F61", "", "IMP", "10P");
			transitProcedure.ZZ6_IntoWarehouse = YesNoList.Codes.No;
			transitProcedure.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			transitProcedure.ZZ6_IsTransit = YesNoList.Codes.Yes;
			Factory.Save();

			var (entryInstruction, invoiceLine) = CreateInvoiceLineLinkedToEntryInstruction();
			SetInvoiceLineProcedure(invoiceLine, transitProcedure);

			AssertEquals(true, entryInstruction.IsTransitPermitsTabPageVisible);
		}

		public void TestIsTransitPermitsTabPageVisible_WhenNonTransitProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var transitProcedure = helper.CreateOrFindExistingRefCusProcedure(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "11", "71", "F61", "", "IMP", "10P");
			transitProcedure.ZZ6_IntoWarehouse = YesNoList.Codes.No;
			transitProcedure.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			transitProcedure.ZZ6_IsTransit = YesNoList.Codes.No;
			Factory.Save();

			var (entryInstruction, invoiceLine) = CreateInvoiceLineLinkedToEntryInstruction();
			SetInvoiceLineProcedure(invoiceLine, transitProcedure);

			AssertEquals(false, entryInstruction.IsTransitPermitsTabPageVisible);
		}

		public void TestIsRiskTabPageTabVisible_WhenNoInvoiceLineLinked_RiskEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNoInvoiceLineLinked(true);
		}

		public void TestIsRiskTabPageTabVisible_WhenNoInvoiceLineLinked_RiskNotEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNoInvoiceLineLinked(false);
		}

		void AssertIsRiskTabPageTabVisible_WhenNoInvoiceLineLinked(bool riskEnabled)
		{
			using (CreateDisposableRiskEnabledSwitcher(riskEnabled))
			{
				var entryInstruction = CreateEntryInstruction();
				AssertEquals($"when riskEnabled = {riskEnabled}", false, entryInstruction.IsRiskTabPageTabVisible);
			}
		}

		public void TestIsRiskTabPageTabVisible_WhenNoInvoiceLineLinkedButRiskManagementRecordExists_RiskEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNoInvoiceLineLinkedButRiskManagementRecordExists(true);
		}

		public void TestIsRiskTabPageTabVisible_WhenNoInvoiceLineLinkedButRiskManagementRecordExists_RiskNotEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNoInvoiceLineLinkedButRiskManagementRecordExists(false);
		}

		void AssertIsRiskTabPageTabVisible_WhenNoInvoiceLineLinkedButRiskManagementRecordExists(bool riskEnabled)
		{
			using (CreateDisposableRiskEnabledSwitcher(riskEnabled))
			{
				var entryInstruction = CreateEntryInstruction();
				entryInstruction.RiskManagements.AddNew();
				AssertEquals($"when riskEnabled = {riskEnabled}", riskEnabled, entryInstruction.IsRiskTabPageTabVisible);
			}
		}

		public void TestIsRiskTabPageTabVisible_WhenProcedureNull_RiskEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenProcedureNull(true);
		}

		public void TestIsRiskTabPageTabVisible_WhenProcedureNull_RiskNotEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenProcedureNull(false);
		}

		void AssertIsRiskTabPageTabVisible_WhenProcedureNull(bool riskEnabled)
		{
			using (CreateDisposableRiskEnabledSwitcher(riskEnabled))
			{
				var (entryInstruction, invoiceLine) = CreateInvoiceLineLinkedToEntryInstruction();
				CombineAssertions(() =>
				{
					AssertNull(invoiceLine.CusProcedure);
					AssertEquals($"when riskEnabled = {riskEnabled}", false, entryInstruction.IsRiskTabPageTabVisible);
				});
			}
		}

		public void TestIsRiskTabPageTabVisible_WhenProcedureNullButRiskManagementRecordExists_RiskEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenProcedureNullButRiskManagementRecordExists(true);
		}

		public void TestIsRiskTabPageTabVisible_WhenProcedureNullButRiskManagementRecordExists_RiskNotEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenProcedureNullButRiskManagementRecordExists(false);
		}

		void AssertIsRiskTabPageTabVisible_WhenProcedureNullButRiskManagementRecordExists(bool riskEnabled)
		{
			using (CreateDisposableRiskEnabledSwitcher(riskEnabled))
			{
				var entryInstruction = CreateEntryInstruction();
				entryInstruction.RiskManagements.AddNew();
				AssertEquals($"when riskEnabled = {riskEnabled}", riskEnabled, entryInstruction.IsRiskTabPageTabVisible);
			}
		}

		public void TestIsRiskTabPageTabVisible_WhenIntoRegimeProcedure_RiskEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenIntoRegimeProcedure(true);
		}

		public void TestIsRiskTabPageTabVisible_WhenIntoRegimeProcedure_RiskNotEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenIntoRegimeProcedure(false);
		}

		void AssertIsRiskTabPageTabVisible_WhenIntoRegimeProcedure(bool riskEnabled)
		{
			var procedures = CreateIntoRegimeProcedure();
			Factory.Save();
			using (CreateDisposableRiskEnabledSwitcher(riskEnabled))
			{
				var (entryInstruction, invoiceLine) = CreateInvoiceLineLinkedToEntryInstruction();
				CombineAssertions(() =>
				{
					foreach (var procedure in procedures)
					{
						SetInvoiceLineProcedure(invoiceLine, procedure);
						AssertEquals($"when riskEnabled = {riskEnabled}", riskEnabled, entryInstruction.IsRiskTabPageTabVisible);
					}
				});
			}
		}

		public void TestIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedure_RiskEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedure(true);
		}

		public void TestIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedure_RiskNotEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedure(false);
		}

		void AssertIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedure(bool riskEnabled)
		{
			var procedure = CreateNonIntoRegimeProcedure();
			Factory.Save();

			using (CreateDisposableRiskEnabledSwitcher(riskEnabled))
			{
				var (entryInstruction, invoiceLine) = CreateInvoiceLineLinkedToEntryInstruction();
				SetInvoiceLineProcedure(invoiceLine, procedure);
				AssertEquals($"when riskEnabled = {riskEnabled}", false, entryInstruction.IsRiskTabPageTabVisible);
			}
		}

		public void TestIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedureButRiskManagementRecordExists_RiskEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedureButRiskManagementRecordExists(true);
		}

		public void TestIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedureButRiskManagementRecordExists_RiskNotEnabled()
		{
			AssertIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedureButRiskManagementRecordExists(false);
		}

		void AssertIsRiskTabPageTabVisible_WhenNonIntoRegimeProcedureButRiskManagementRecordExists(bool riskEnabled)
		{
			var procedure = CreateNonIntoRegimeProcedure();
			Factory.Save();

			using (CreateDisposableRiskEnabledSwitcher(riskEnabled))
			{
				var (entryInstruction, invoiceLine) = CreateInvoiceLineLinkedToEntryInstruction();
				SetInvoiceLineProcedure(invoiceLine, procedure);
				entryInstruction.RiskManagements.AddNew();
				AssertEquals($"when riskEnabled = {riskEnabled}", riskEnabled, entryInstruction.IsRiskTabPageTabVisible);
			}
		}

		static IDisposable CreateDisposableRiskEnabledSwitcher(bool riskEnabled)
		{
			return Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Universal.Constants.FunctionalityTypes.Risk,
				MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				ZDateTime.Today,
				value: riskEnabled);
		}

		static void SetInvoiceLineProcedure(JobComInvoiceLine invoiceLine, Universal.RefCusProcedure procedure)
		{
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
		}

		(CusEntryInstruction EntryInstruction, JobComInvoiceLine InvoiceLine) CreateInvoiceLineLinkedToEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			return (entryInstruction, invoiceLine);
		}

		CusEntryInstruction CreateEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryInstructions.AddNew();
		}

		Universal.RefCusProcedure[] CreateIntoRegimeProcedure()
		{
			var countryCode = MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "20", "71", "F61", "", "IMP", "10P");
			procedure1.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;

			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "21", "71", "F61", "", "IMP", "10P");
			procedure2.ZZ6_IntoInwardProcessing = YesNoList.Codes.Yes;

			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "22", "71", "F61", "", "IMP", "10P");
			procedure3.ZZ6_IntoOutwardProcessing = YesNoList.Codes.Yes;

			var procedure4 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "23", "71", "F61", "", "IMP", "10P");
			procedure4.ZZ6_IntoTemporaryImport = YesNoList.Codes.Yes;

			var procedure5 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "24", "71", "F61", "", "IMP", "10P");
			procedure5.ZZ6_IntoTemporaryExport = YesNoList.Codes.Yes;

			var procedure6 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "25", "71", "F61", "", "IMP", "10P");
			procedure6.ZZ6_IsTransit = YesNoList.Codes.Yes;

			return new[] { procedure1, procedure2, procedure3, procedure4, procedure5, procedure6 };
		}

		Universal.RefCusProcedure CreateNonIntoRegimeProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "19", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = YesNoList.Codes.No;
			procedure.ZZ6_IntoInwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_IntoOutwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_IntoTemporaryImport = YesNoList.Codes.No;
			procedure.ZZ6_IntoTemporaryExport = YesNoList.Codes.No;
			procedure.ZZ6_IsTransit = YesNoList.Codes.No;
			return procedure;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryInstructions.AddNew();
		}

		CusGuaranteeHeader CreateCusGuarantee()
		{
			var cusGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuarantee.CPH_RN_NKCountryCode = MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusGuarantee.CPH_ApplicationCode = "GUA";
			cusGuarantee.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusGuarantee.CPH_EndDate = ZDate.Today.AddDays(1);
			cusGuarantee.CPH_Number = "CPH1";
			return cusGuarantee;
		}
	}
}
