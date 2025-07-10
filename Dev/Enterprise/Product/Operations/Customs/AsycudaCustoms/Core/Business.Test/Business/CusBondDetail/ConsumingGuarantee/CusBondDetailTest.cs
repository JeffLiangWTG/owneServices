using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusBondDetail))]
	class CusBondDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPW_BondAmount_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var helper = new GuaranteeTestHelper(Factory);
				var guarantee = helper.CreateValidNotLinkedGuarantee();
				AssertEquals("No release guarantee", false, guarantee.PW_BondAmountInfo.ReadOnly);
				guarantee.Instruction.ReleaseGuarantees.AddNew();
				AssertEquals("Release guarantee exists", true, guarantee.PW_BondAmountInfo.ReadOnly);
			});
		}

		public void TestCustomsCurrency()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidNotLinkedGuarantee();
			CombineAssertions(() =>
			{
				AssertEquals("Not set", ZString.Empty, guarantee.CustomsCurrency);
				guarantee.Instruction.InvoiceLines.First().InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Congo;
				AssertEquals("Set", Core.Constants.CurrencyCodes.Congo, guarantee.CustomsCurrency);
			});
		}

		public void TestCustomsCurrency_Caption()
		{
			AssertEquals("Currency", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusBondDetail>().CustomsCurrencyInfo).Caption);
		}

		public void TestPW_BondType_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(g => g.PW_BondTypeInfo);
		}

		public void TestPW_CPH_Guarantee_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(g => g.PW_CPH_GuaranteeInfo);
		}

		public void TestPW_BondAmount_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(g => g.PW_BondAmountInfo);
		}

		public void TestPW_BondNumber2_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(g => g.PW_BondNumber2Info);
		}

		public void TestPW_BondEffectiveDate_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(g => g.PW_BondEffectiveDateInfo);
		}

		public void TestPW_BondType_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(g => g.PW_BondTypeInfo);
		}

		public void TestPW_CPH_Guarantee_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(g => g.PW_CPH_GuaranteeInfo);
		}

		public void TestPW_BondAmount_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(g => g.PW_BondAmountInfo);
		}

		public void TestPW_BondNumber2_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(g => g.PW_BondNumber2Info);
		}

		public void TestPW_BondEffectiveDate_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(g => g.PW_BondEffectiveDateInfo);
		}

		public void TestUpdateReleaseGuaranteesWithCusGuarantee_PW_CPH_GuaranteeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondAmount = 1m;
			guarantee.PW_BondNumber2 = "VWG";
			guarantee.PW_BondEffectiveDate = ZDateTime.Now;
			var releaseGuarantee1 = instruction.ReleaseGuarantees.AddNew();
			var releaseGuarantee2 = instruction.ReleaseGuarantees.AddNew();
			var cusGuarantee = new GuaranteeTestHelper(Factory).CusGuarantee;
			guarantee.PW_CPH_Guarantee = cusGuarantee.PK;
			CombineAssertions(() =>
			{
				AssertEquals("releaseGuarantee1", cusGuarantee.PK, releaseGuarantee1.PW_CPH_Guarantee);
				AssertEquals("releaseGuarantee2", cusGuarantee.PK, releaseGuarantee2.PW_CPH_Guarantee);
			});
		}

		public void TestClearReleaseGuaranteesIfNeeded_PW_BondAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondAmount = 1m;
			guarantee.PW_BondNumber2 = "VWG";
			guarantee.PW_BondEffectiveDate = ZDateTime.Now;
			instruction.ReleaseGuarantees.AddNew();
			instruction.ReleaseGuarantees.AddNew();
			guarantee.PW_BondAmount = ZDecimal.Zero;
			AssertEquals(0, instruction.ReleaseGuarantees.Count);
		}

		public void TestClearReleaseGuaranteesIfNeeded_PW_BondNumber2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondAmount = 1m;
			guarantee.PW_BondNumber2 = "VWG";
			guarantee.PW_BondEffectiveDate = ZDateTime.Now;
			instruction.ReleaseGuarantees.AddNew();
			instruction.ReleaseGuarantees.AddNew();
			guarantee.PW_BondNumber2 = ZString.Empty;
			AssertEquals(0, instruction.ReleaseGuarantees.Count);
		}

		public void TestClearReleaseGuaranteesIfNeeded_PW_BondEffectiveDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondAmount = 1m;
			guarantee.PW_BondNumber2 = "VWG";
			guarantee.PW_BondEffectiveDate = ZDateTime.Now;
			instruction.ReleaseGuarantees.AddNew();
			instruction.ReleaseGuarantees.AddNew();
			guarantee.PW_BondEffectiveDate = ZDateTime.Empty;
			AssertEquals(0, instruction.ReleaseGuarantees.Count);
		}

		public void TestCanAddReleaseGuarantees()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			CombineAssertions(() =>
			{
				AssertEquals("All empty", false, guarantee.CanAddReleaseGuarantees);
				guarantee.PW_BondAmount = 1m;
				AssertEquals("PW_BondAmount entered, 2 empty left", false, guarantee.CanAddReleaseGuarantees);
				guarantee.PW_BondNumber2 = "VWG";
				AssertEquals("PW_BondNumber2 entered, 1 empty left", false, guarantee.CanAddReleaseGuarantees);
				guarantee.PW_BondEffectiveDate = ZDateTime.Now;
				AssertEquals("PW_BondEffectiveDate entered, no empty left", true, guarantee.CanAddReleaseGuarantees);
			});
		}

		public void TestRemaining()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			CombineAssertions(() =>
			{
				guarantee.PW_BondAmount = 3m;
				AssertEquals("No release guarantee", 3m, guarantee.Remaining);
				var releaseGuarantee1 = instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee1.PW_BondAmount = 1m;
				AssertEquals("1 release guarantee", 2m, guarantee.Remaining);
				var releaseGuarantee2 = instruction.ReleaseGuarantees.AddNew();
				releaseGuarantee2.PW_BondAmount = 1m;
				AssertEquals("2 release guarantees", 1m, guarantee.Remaining);
			});
		}

		public void TestHumanReadableName()
		{
			var guarantee = Factory.New<CusBondDetail>();
			guarantee.PW_BondNumber2 = "REF1";
			guarantee.PW_BondEffectiveDate = new ZDateTime(2021, 7, 15, 8, 18, 1);
			AssertEquals("Guarantee (Reference Number 'REF1', Issue Date '15 Jul 2021 08:18')", guarantee.HumanReadableName);
		}

		public void TestIsLinkButtonEnabled()
		{
			CombineAssertions(() =>
			{
				var cusGuarantee = new GuaranteeTestHelper(Factory).CusGuarantee;
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var guarantee = entryInstruction.Guarantee;
				AssertEquals("Default", false, guarantee.IsLinkButtonEnabled);
				guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
				guarantee.PW_CPH_Guarantee = cusGuarantee.PK;
				guarantee.PW_BondNumber2 = "REF";
				guarantee.PW_BondEffectiveDate = ZDateTime.Now;
				guarantee.PW_BondAmount = 1m;
				AssertEquals("Yes", true, guarantee.IsLinkButtonEnabled);
				entryInstruction.ReleaseGuarantees.AddNew();
				AssertEquals("No once release guarantee added", false, guarantee.IsLinkButtonEnabled);
			});
		}

		public void TestIsLinkButtonEnabled_EmptyAmountAndLinked()
		{
			var cusGuarantee = new GuaranteeTestHelper(Factory).CusGuarantee;
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = entryInstruction.Guarantee;
			guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
			guarantee.PW_CPH_Guarantee = cusGuarantee.PK;
			guarantee.PW_BondNumber2 = "REF";
			guarantee.PW_BondEffectiveDate = ZDateTime.Now;
			guarantee.PW_BondAmount = 0m;
			guarantee.PW_Status = GuaranteeStatusList.Codes.Linked;
			AssertEquals(true, guarantee.IsLinkButtonEnabled);
		}

		public void TestIsContinuous()
		{
			CombineAssertions(() =>
			{
				var guarantee = Factory.New<CusBondDetail>();
				guarantee.PW_BondType = GuaranteeBondTypeList.Codes.SingleTransaction;
				AssertEquals("No for SingleTransaction", false, guarantee.IsContinuous);
				guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
				AssertEquals("Yes for Continuous", true, guarantee.IsContinuous);
				guarantee.PW_BondType = ZString.Empty;
				AssertEquals("No for Empty", false, guarantee.IsContinuous);
			});
		}

		public void TestHasReleaseGuarantees()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var guarantee = entryInstruction.Guarantee;
				AssertEquals("No release guarantee", false, guarantee.HasReleaseGuarantees);
				entryInstruction.ReleaseGuarantees.AddNew();
				AssertEquals("Release guarantee added", true, guarantee.HasReleaseGuarantees);
			});
		}

		public void TestPW_CPH_Guarantee_ReadOnly()
		{
			AssertReadOnlyOnStatus(x => x.PW_CPH_GuaranteeInfo);
		}

		public void TestPW_BondType_ReadOnly()
		{
			AssertReadOnlyOnStatus(x => x.PW_BondTypeInfo);
		}

		public void TestPW_BondNumber2_ReadOnly()
		{
			AssertReadOnlyOnStatus(x => x.PW_BondNumber2Info);
		}

		public void TestPW_BondEffectiveDate_ReadOnly()
		{
			AssertReadOnlyOnStatus(x => x.PW_BondEffectiveDateInfo);
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestLink_UserAgreesToSaveJobFirst() => AssertLink(true);

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestLink_UserRejectsToSaveJobFirst() => AssertLink(false);

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestLink_SaveMeetsException()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidNotLinkedGuarantee();
			Factory.Saving += Factory_Saving;
			try
			{
				guarantee.LinkOrUnlink();
			}
			catch (Exception)
			{
			}
			finally
			{
				Factory.Saving -= Factory_Saving;
			}
			CombineAssertions(() =>
			{
				AssertEquals("PW_Status", GuaranteeStatusList.Codes.NotLinked, guarantee.PW_Status);
				GuaranteeTestHelper.AssertGuaranteeTransactions("unchanged", helper.CusGuarantee, 100m, ("Opening", 100m, "Job Number"));
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestLink_Burst()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidNotLinkedGuarantee();
			guarantee.OnLinkOrUnlinkAskingSaveJobFirst += (s, e) => e.Cancel = false;
			guarantee.PW_BondAmount = 1000m;
			guarantee.LinkOrUnlink();
			CombineAssertions(() =>
			{
				AssertEquals("PW_Status", GuaranteeStatusList.Codes.NotLinked, guarantee.PW_Status);
				GuaranteeTestHelper.AssertGuaranteeTransactions("unchanged", helper.CusGuarantee, 100m, ("Opening", 100m, "Job Number"));
				const string message = "The guarantee 12345 available amount will be exceeded by 900. The total is 100 and the available is 100.";
				AssertHasWarningContaining("Warning", guarantee.PW_BondAmountInfo, message);
				AssertEquals("Notify User", message, ((SendsMessagesToCustomsShutterUpperer)guarantee.Instruction.JobDeclaration.MessageInitiator).Warning);
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestUnlink_UserAgreesToSaveJobFirst() => AssertUnlink(true);

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestUnlink_UserRejectsToSaveJobFirst() => AssertUnlink(false);

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestUnlink_SaveMeetsException()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			Factory.Saving += Factory_Saving;
			try
			{
				guarantee.LinkOrUnlink();
			}
			catch (Exception)
			{
			}
			finally
			{
				Factory.Saving -= Factory_Saving;
			}
			CombineAssertions(() =>
			{
				AssertEquals("PW_Status", GuaranteeStatusList.Codes.Linked, guarantee.PW_Status);
				GuaranteeTestHelper.AssertGuaranteeTransactions("unchanged", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
			});
		}

		[TestDate(2020, 11, 25, 12, 42, 0)]
		public void TestUnlink_RollbackShouldConsiderAppID()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			guarantee.OnLinkOrUnlinkAskingSaveJobFirst += (s, e) => e.Cancel = false;
			GuaranteeTestHelper.AdjustCusGuaranteeAmount(guarantee.CusGuarantee, 1m, "Entry Reference");
			guarantee.LinkOrUnlink();
			GuaranteeTestHelper.AssertGuaranteeTransactions("A transaction is added", helper.CusGuarantee, 101m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference", 1m, "Job Number"), ("Entry Reference", 3.14m, "Job Number"));
		}

		public void TestUnableToLink()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateInvalidNotLinkedGuarantee();
			guarantee.LinkOrUnlink();
			CombineAssertions("Unable to Link", () =>
			{
				var messageInitiator = (SendsMessagesToCustomsShutterUpperer)guarantee.Instruction.JobDeclaration.MessageInitiator;
				AssertEquals("Caption", "Unable to Link", messageInitiator.WarningCaption);
				AssertEquals("Message", "A valid guarantee should be entered.\r\nEntry Acquitted Date should be empty.\r\n", messageInitiator.Warning);
				AssertEquals("Status unchanged", GuaranteeStatusList.Codes.NotLinked, guarantee.PW_Status);
			});

			guarantee.Instruction.JobDeclaration.MessageInitiator.WarnUserAboutSomething(string.Empty, string.Empty);
			guarantee.Instruction.EntryHeader.CH_BondAcquittedDate = ZDate.Empty;
			guarantee.PW_CPH_Guarantee = helper.CusGuarantee.PK;

			guarantee.LinkOrUnlink();
			CombineAssertions("Success", () =>
			{
				var messageInitiator = (SendsMessagesToCustomsShutterUpperer)guarantee.Instruction.JobDeclaration.MessageInitiator;
				AssertEquals("Caption", ZString.Empty, messageInitiator.WarningCaption);
				AssertEquals("Message", ZString.Empty, messageInitiator.Warning);
				AssertEquals("Status changed", GuaranteeStatusList.Codes.Linked, guarantee.PW_Status);
			});
		}

		public void TestPW_BondType_Caption()
		{
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusBondDetail>().PW_BondTypeInfo).Caption);
		}

		public void TestPW_CPH_Guarantee_Caption()
		{
			AssertEquals("Guarantee", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusBondDetail>().PW_CPH_GuaranteeInfo).Caption);
		}

		public void TestPW_Status_Caption()
		{
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusBondDetail>().PW_StatusInfo).Caption);
		}

		public void TestLinkButtonLabel_Default()
		{
			AssertEquals("Link", Factory.New<CusBondDetail>().LinkButtonLabel);
		}

		public void TestLinkButtonLabel_IfLinked()
		{
			var guarantee = Factory.New<CusBondDetail>();
			guarantee.PW_Status = GuaranteeStatusList.Codes.Linked;
			AssertEquals("Unlink", guarantee.LinkButtonLabel);
		}

		public void TestLoadOrCreate()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var actual = CusBondDetail.LoadOrCreate(entryInstruction);
				AssertNotNull("Created", actual);
				AssertSame("CusBondDetail.Load", CusBondDetail.Load(entryInstruction), actual);
				AssertEquals("CusBondDetail.Create", false, CusBondDetail.Create(entryInstruction).Equals(actual));
			});
		}

		public void TestLoadOrCreate_NullInstruction()
		{
			AssertNull(CusBondDetail.LoadOrCreate(null));
		}

		public void TestLoad_NullInstruction()
		{
			AssertNull(CusBondDetail.Load(null));
		}

		public void TestCreate_NullInstruction()
		{
			AssertNull(CusBondDetail.Create(null));
		}

		public void TestLoadOrCreateUseOrderingByPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var correctGuarantee = entryInstruction.Guarantee;
			for (var i = 0; i < 6; i++)
			{
				var guarantee = Factory.New<CusBondDetail>();
				guarantee.Parent = entryInstruction;
				if (correctGuarantee.PK > guarantee.PK)
				{
					correctGuarantee = guarantee;
				}
			}

			((IDbConnected)Factory).Connection.ExecuteNonQuery($"INSERT dbo.CusBondDetail (PW_PK, PW_ParentID, PW_ParentTableCode) VALUES('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', '{declaration.PK.ToString()}', 'JE')");
			AssertSame(correctGuarantee, CusBondDetail.LoadOrCreate(entryInstruction));
			Factory.Save();

			var factory = new BusinessObjectFactory();
			factory.AddFetchHint(CusBondDetailSchema.PW_ParentID, entryInstruction.PK);
			entryInstruction = factory.Load<CusEntryInstruction>(entryInstruction.PK);
			AssertEquals(correctGuarantee.PK, CusBondDetail.LoadOrCreate(entryInstruction).PK);
			AssertEquals("System should not cause more db hits; ie load should load all instead of using ", 1, factory.GetTableHitCount(CusBondDetail.Schema.TableName));
		}

		public void TestValidation()
		{
			AssertType<CusBondDetailValidation>(Factory.New<CusBondDetail>().Validation);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GuaranteeActivityCodeList.Codes.ConsumesGuarantee, Factory.New<CusBondDetail>().PW_ActivityCode);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			return entryInstruction.Guarantee;
		}

		void AssertReadOnlyOnStatus(Func<CusBondDetail, ZPropertyInfo> getPropertyInfo)
		{
			CombineAssertions(() =>
			{
				var guarantee = Factory.New<CusBondDetail>();
				guarantee.PW_Status = GuaranteeStatusList.Codes.Linked;
				AssertEquals("Linked", true, getPropertyInfo(guarantee).ReadOnly);
				guarantee.PW_Status = GuaranteeStatusList.Codes.NotLinked;
				AssertEquals("Unlinked", false, getPropertyInfo(guarantee).ReadOnly);
			});
		}

		void AssertLink(bool doesUserAgreeToSaveJobFirst)
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidNotLinkedGuarantee();
			guarantee.OnLinkOrUnlinkAskingSaveJobFirst += (s, e) => e.Cancel = !doesUserAgreeToSaveJobFirst;
			guarantee.LinkOrUnlink();
			CombineAssertions(() =>
			{
				AssertEquals("PW_Status", doesUserAgreeToSaveJobFirst ? GuaranteeStatusList.Codes.Linked : GuaranteeStatusList.Codes.NotLinked, guarantee.PW_Status);
				if (doesUserAgreeToSaveJobFirst)
				{
					GuaranteeTestHelper.AssertGuaranteeTransactions("A transaction is added", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				}
				else
				{
					GuaranteeTestHelper.AssertGuaranteeTransactions("unchanged", helper.CusGuarantee, 100m, ("Opening", 100m, "Job Number"));
				}
			});
		}

		void AssertUnlink(bool doesUserAgreeToSaveJobFirst)
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			guarantee.OnLinkOrUnlinkAskingSaveJobFirst += (s, e) => e.Cancel = !doesUserAgreeToSaveJobFirst;
			guarantee.LinkOrUnlink();
			CombineAssertions(() =>
			{
				AssertEquals("PW_Status", doesUserAgreeToSaveJobFirst ? GuaranteeStatusList.Codes.NotLinked : GuaranteeStatusList.Codes.Linked, guarantee.PW_Status);
				if (doesUserAgreeToSaveJobFirst)
				{
					GuaranteeTestHelper.AssertGuaranteeTransactions("A transaction is added", helper.CusGuarantee, 100m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"), ("Entry Reference", 3.14m, "Job Number"));
				}
				else
				{
					GuaranteeTestHelper.AssertGuaranteeTransactions("unchanged", helper.CusGuarantee, 96.86m, ("Opening", 100m, "Job Number"), ("Entry Reference", -3.14m, "Job Number"));
				}
			});
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			throw new ZSaveException(new ZDataException(new Exception("ZSaveException message"), null, null), Factory);
		}

		void AssertSetProperty_LockGuaranteeManagementMutex(Func<CusBondDetail, ZPropertyInfo> getPropertyInfo)
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			CombineAssertions(() =>
			{
				var guaranteeManagementMutex = guarantee.Instruction.GuaranteeManagementMutex;
				AssertEquals("Pre-condition", false, guaranteeManagementMutex.IsLocked && guaranteeManagementMutex.HasLock);
				getPropertyInfo(guarantee).Value = getPropertyInfo(guarantee).DefaultValue;
				AssertEquals("Locked by current", true, guaranteeManagementMutex.IsLocked && guaranteeManagementMutex.HasLock);
				AssertEquals(getPropertyInfo(guarantee).Name, getPropertyInfo(guarantee).DefaultValue, getPropertyInfo(guarantee).Value);
				guarantee.Instruction.UnlockGuaranteeManagementMutex();
			});
		}

		void AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(Func<CusBondDetail, ZPropertyInfo> getPropertyInfo)
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var count = 0;
			getPropertyInfo(guarantee).ValueChanged += (s, e) => count++;
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, guarantee.Instruction.PK.ToString()))
			{
				mutex.Lock();
				getPropertyInfo(guarantee).Value = getPropertyInfo(guarantee).DefaultValue;
				CombineAssertions(() =>
				{
					AssertEquals("Unchanged", getPropertyInfo(guarantee).OriginalValue, getPropertyInfo(guarantee).Value);
					AssertEquals("PropertyInfo refreshes", 1, count);
				});
				mutex.Unlock();
			}
		}
	}
}
