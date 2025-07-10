using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryItemTagTest : TransactionedTestCase
	{
		public void TestGetStorage()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Storage", RegistryStorageFlags.All, item.Storage);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Company);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Storage", RegistryStorageFlags.Company, item.Storage);
		}

		public void TestGetHint()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Hint", "TestHint", item.Hint);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Hint", "TestHint2", item.Hint);
		}

		public void TestShouldValidate()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Company);
			regItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Hi World");
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Env.CurrentCompany.PK;
			AssertEquals("item.ShouldValidate", false, item.ShouldValidate(false));
			AssertEquals("item.ShouldValidate", false, item.ShouldValidate(true));
			((IRegistryItemInternals)regItem).SetProposedValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Helllo World");
			AssertEquals("item.ShouldValidate", true, item.ShouldValidate(false));
			AssertEquals("item.ShouldValidate", true, item.ShouldValidate(true));
		}

		public void TestShouldValidate_IsValidatedOnSetEvenIfEqualDefaultValue()
		{
			var dataType = new Mock<IRegistryDataType>();
			dataType.Setup(m => m.DataType).Returns(typeof(bool));
			dataType.Setup(m => m.IsValidatedOnSetEvenIfEqualDefaultValue).Returns(true);

			var item = new Mock<IRegistryItemInternals>();
			item.Setup(m => m.DataType).Returns(dataType.Object);

			var tag = new RegistryItemTag(item.Object);
			//Override but no change, so false 
			AssertEquals("item.ShouldValidate", false, tag.ShouldValidate(true));
			//Non-override and no change (default), true 
			AssertEquals("item.ShouldValidate", true, tag.ShouldValidate());
		}

		public void TestNewValue()
		{
			IRegistryItem regItem = RatingDataRegistry.Instance.EntryChargeTypesAndCodes;
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Env.CurrentCompany.PK;
			var collection = item.NewValue as Business.Customs.EntryChargeTypeSettingCollection;
			AssertNotNull(collection);
		}

		public void TestGetCategory()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Category", "Category", item.Category);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category2", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Category", "Category2", item.Category);
		}

		public void TestGetCaption()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Caption", "TestCaption", item.Caption);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.Caption", "TestCaption2", item.Caption);
		}

		public void TestGetDataType()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.DataType", typeof(StringRegistryDataType), item.DataType.GetType());

			regItem = new IntRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.DataType", typeof(IntRegistryDataType), item.DataType.GetType());
		}

		public void TestGetEditorInfo()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);

			regItem = new IntRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			regItem.EditorInfo = new NumericRegistryEditorInfo(0);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.EditorInfo", typeof(NumericRegistryEditorInfo), item.EditorInfo.GetType());
		}

		public void TestGetIsReadOnly()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, RegistryOptions.IsReadOnly);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.IsReadOnly", true, item.IsReadOnly);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.IsReadOnly", false, item.IsReadOnly);
		}

		public void TestGetHasReadOnlyOption()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, RegistryOptions.IsReadOnly);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals(true, item.HasReadOnlyOption);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			item = new RegistryItemTagForTest(regItem);
			AssertEquals(false, item.HasReadOnlyOption);
		}

		public void TestGetLockedDown()
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			IRegistryItem regItem = new StringRegistryItem("TestName3", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "");
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Pre-condition: should not be locked down", false, item.IsLockedDown);

			using (EnvProxy.Instance.SetTemporaryUserContext("CWPostMaster", EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Should be locked down", true, item.IsLockedDown);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Should not be locked down", false, item.IsLockedDown);
			}
		}

		public void TestGetAndSetNewValue()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "NewValue";
			AssertEquals("RegItem.GetProposedValue()", "NewValue", ((IRegistryItemInternals)regItem).GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Item.NewValue", "NewValue", item.NewValue);

			item.CompanyPKForTest = Guid.NewGuid();
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "NewValue2";
			AssertEquals("RegItem.GetProposedValue()", "NewValue2", ((IRegistryItemInternals)regItem).GetProposedValue(item.CompanyPKForTest, Guid.Empty, Guid.Empty));
			AssertEquals("Item.NewValue", "NewValue2", item.NewValue);
		}

		public void TestGetValue()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Value1");
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			AssertEquals("Item.GetValue()", "Value1", item.GetValue());

			item.IsChanged = true;
			item.NewValue = "Value2";
			AssertEquals("Item.GetValue()", "Value2", item.GetValue());

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			Guid companyPK = Guid.NewGuid();
			regItem.SetValue(companyPK, Guid.Empty, Guid.Empty, "Value3");
			item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = companyPK;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			AssertEquals("Item.GetValue()", "Value3", item.GetValue());

			item.IsChanged = true;
			item.NewValue = "Value4";
			AssertEquals("Item.GetValue()", "Value4", item.GetValue());
		}

		public void TestGetAndSetHasValue()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			AssertEquals("Item.HasValue", false, item.HasValue);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Value2");
			RegistryItemTagForTest item2 = new RegistryItemTagForTest(regItem);
			item2.CompanyPKForTest = Guid.Empty;
			item2.BranchPKForTest = Guid.Empty;
			item2.DepartmentPKForTest = Guid.Empty;
			AssertEquals("Item2.HasValue", true, item2.HasValue);

			item.HasValue = true;
			item2.HasValue = false;
			AssertEquals("Item.HasValue", true, item.HasValue);
			AssertEquals("Item2.HasValue", false, item2.HasValue);
		}

		public void TestSaveAllValues()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "Value1";
			Assert("ClearAll() should not have been called", !item.IsClearAllCalled);
			item.SaveAllValues();
			Assert("ClearAll() should have been called during SetValue()", item.IsClearAllCalled);
			AssertEquals("Item.GetValue()", "", item.GetValue());
			Assert("Item.NewValue should be null", item.NewValue == null);
			Assert("Item's IsChanged property should be false", !item.IsChanged);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			Guid companyPK = Guid.NewGuid();
			item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = companyPK;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "Value2";
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();
			AssertEquals("Item.GetValue()", "Value2", item.GetValue());
			Assert("Item.NewValue should be null", item.NewValue == null);
			Assert("Item's IsChanged property should be false", !item.IsChanged);

			IRegistryItem regItem2 = new StringRegistryItem("TestItem2", (NoResString)"Category2", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.All);
			RegistryItemTagForTest item2 = new RegistryItemTagForTest(regItem2);
			item2.NewValue = "Value3";
			item2.IsChanged = true;
			item2.HasValue = true;
			item.IsChanged = true;
			item.HasValue = false;
			item.CompanyPKForTest = Guid.Empty;
			item.IsChanged = true;
			item.HasValue = true;
			item.NewValue = "Value4";
			item.SaveAllValues();
			AssertEquals("Item.GetValue()", "Value4", item.GetValue());
			Assert("Item.NewValue should be null", item.NewValue == null);
			Assert("Item's IsChanged property should be false", !item.IsChanged);
			item.CompanyPKForTest = companyPK;
			AssertEquals("Item.GetValue()", "", item.GetValue());
			Assert("Item.NewValue should be null", item.NewValue == null);
			Assert("Item's IsChanged property should be false", !item.IsChanged);
			AssertEquals("Item2.GetValue()", "Value3", item2.GetValue());
			AssertEquals("Item2.NewValue", "Value3", item2.NewValue);
			Assert("Item2's IsChanged property should be true", item2.IsChanged);
		}

		public void TestSaveAllValuesGeneratesLogs()
		{
			IRegistryItem registryItem = new StringRegistryItem("UniquelyTestItem", (NoResString)"Category", (NoResString)"UniquelyTestCaption", (NoResString)"UniquelyTestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(registryItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "Value1";
			item.IsChanged = true;
			item.HasValue = true;

			AssertEquals("Precondition: Registry should not exist", Guid.Empty, item.GetRegistryItemPK());

			item.SaveAllValues();
			AssertEquals(1, ((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs().Count);
			AssertEquals("EDT", ((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs()[0].SL_SE_NKEvent);

			item.NewValue = null;
			item.IsChanged = true;
			item.HasValue = false;
			item.SaveAllValues();
			var logs = ((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs();
			AssertEquals(2, logs.Count);
			Assert("RST", logs.Any(x => ((StmALog)x).SL_SE_NKEvent == "RST"));
			Assert("EDT", logs.Any(x => ((StmALog)x).SL_SE_NKEvent == "EDT"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900275. CodeDescriptionPairListRegistryItem throws InvalidCastException when supplied NoResString instead of ResourceString.")]
		public void TestSaveAllValuesBuildLogReferenceForLogs()
		{
			using (SystemDataRegistry.Instance.EnableEnhancedLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CodeDescriptionPairList originalValue = new CodeDescriptionPairList();
				originalValue.AddPair("AAA", ResString.GetMultilingualString("AAA", "Apple"));
				originalValue.AddPair("BBB", ResString.GetMultilingualString("BBB", "Banana"));

				IRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("TestItem", (NoResString)"TestCaption", (NoResString)"TestHint", 3, RegistryStorageFlags.System, originalValue);
				((CodeDescriptionPairListRegistryItem)registryItem).OnBuildLogReference = (x) => { return "test log reference"; };
				RegistryItemTagForTest item = new RegistryItemTagForTest(registryItem);
				item.CompanyPKForTest = Guid.Empty;
				item.BranchPKForTest = Guid.Empty;
				item.DepartmentPKForTest = Guid.Empty;

				CodeDescriptionPairList newValue = new CodeDescriptionPairList(originalValue);
				newValue.RemoveCode("BBB");
				item.NewValue = newValue;
				item.IsChanged = true;
				item.HasValue = true;

				AssertEquals("Precondition: Registry should not exist", Guid.Empty, item.GetRegistryItemPK());

				item.SaveAllValues();
				AssertEquals(1, ((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs().Count);
				AssertEquals("EDT", ((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs()[0].SL_SE_NKEvent);
				AssertEquals("test log reference", ((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs()[0].SL_Reference);

				item.NewValue = null;
				item.IsChanged = true;
				item.HasValue = false;
				item.SaveAllValues();
				AssertEquals(2, ((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs().Count);
				StmALog rstLog = (StmALog)((IStmALogParent)new RegistryItemLogs(item.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs().First(x => ((StmALog)x).SL_SE_NKEvent == "RST");
				AssertNotNull("Reset log should exists", rstLog);
				AssertEquals("test log reference", rstLog.SL_Reference);

				// multi-line log reference
				var registryItem2 = new CodeDescriptionPairListRegistryItem("TestItem2", (NoResString)"TestCaption", (NoResString)"TestHint", 3, RegistryStorageFlags.System, originalValue);
				registryItem2.OnBuildLogReference = (x) => { return "\r\nline 1 \r\n\r line 2\n"; };
				RegistryItemTagForTest item2 = new RegistryItemTagForTest(registryItem2);
				item2.CompanyPKForTest = Guid.Empty;
				item2.BranchPKForTest = Guid.Empty;
				item2.DepartmentPKForTest = Guid.Empty;
				item2.NewValue = newValue;
				item2.IsChanged = true;
				item2.HasValue = true;
				item2.SaveAllValues();
				var allLogs = ((IStmALogParent)new RegistryItemLogs(item2.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs();
				AssertEquals(2, allLogs.Count);
				var log1 = allLogs.Cast<StmALog>().First(x => x.SL_Reference == "line 1");
				var log2 = allLogs.Cast<StmALog>().First(x => x.SL_Reference == "line 2");
				AssertEquals("EDT", log1.SL_SE_NKEvent);
				AssertEquals("EDT", log2.SL_SE_NKEvent);

				// reset
				var registryItem3 = new CodeDescriptionPairListRegistryItem("TestItem3", (NoResString)"TestCaption", (NoResString)"TestHint", 3, RegistryStorageFlags.System | RegistryStorageFlags.Company, originalValue);
				IRegistryItem regItem = registryItem3;
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
				regItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, originalValue);
				RegistryItemTagForTest item3 = new RegistryItemTagForTest(registryItem3);
				item3.CompanyPKForTest = Env.CurrentCompany.PK;
				item3.BranchPKForTest = Guid.Empty;
				item3.DepartmentPKForTest = Guid.Empty;
				item3.NewValue = new CodeDescriptionPairList();
				item3.IsChanged = true;
				item3.HasValue = false;
				ReadOnlyCodeDescriptionPairList logNewValue = null;
				registryItem3.OnBuildLogReference = (args) =>
				{
					logNewValue = (ReadOnlyCodeDescriptionPairList)args.NewValue;
					return "reset";
				};
				item3.SaveAllValues();
				AssertArrayEqualsByElements("NewValue is fallback to system level", newValue.ToArray(), logNewValue.Cast<ICodeDescription>().ToArray());
				allLogs = ((IStmALogParent)new RegistryItemLogs(item3.GetRegistryItemPK(), new BusinessObjectFactory())).Logs.GetAllLogs();
				AssertEquals(1, allLogs.Count);
				log1 = allLogs.Cast<StmALog>().First(x => x.SL_SE_NKEvent == "RST");
				AssertEquals("reset", log1.SL_Reference);
			}
		}

		public void TestSaveAllValuesOnAllValuesSavedAction()
		{
			var i = 0;
			IRegistryItemInternals regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All)
			{
				OnAllValuesSavedAction = () => { i++; }
			};
			var item = new RegistryItemTagForTest(regItem)
			{
				CompanyPKForTest = Guid.Empty,
				BranchPKForTest = Guid.Empty,
				DepartmentPKForTest = Guid.Empty,
				NewValue = "Value",
				IsChanged = true,
				HasValue = true
			};
			AssertEquals(0, i);
			item.SaveAllValues();
			AssertEquals(1, i);

			item.NewValue = "Value2";
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();
			AssertEquals(2, i);

			i = 0;
			regItem = new IntRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All)
			{
				OnAllValuesSavedAction = () => { i++; }
			};
			item = new RegistryItemTagForTest(regItem)
			{
				CompanyPKForTest = Guid.Empty,
				BranchPKForTest = Guid.Empty,
				DepartmentPKForTest = Guid.Empty,
				NewValue = 111,
				IsChanged = true,
				HasValue = true
			};
			item.SaveAllValues();
			AssertEquals(1, i);

			item.NewValue = 222;
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();
			AssertEquals(2, i);
		}

		public void TestSaveAllValuesOnUpdateAction()
		{
			testvalueForOnUpdateAction = "";
			IRegistryItemInternals regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			regItem.OnUpdateAction = OnUpdateForTestForStringItem;
			var item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "Value";
			item.IsChanged = true;
			item.HasValue = true;
			AssertEquals("", testvalueForOnUpdateAction);
			item.SaveAllValues();
			AssertEquals("Value", testvalueForOnUpdateAction);
			item.NewValue = "Value2";
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();
			AssertEquals("Value2", testvalueForOnUpdateAction);

			testvalueForOnUpdateAction = "";
			regItem = new IntRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			regItem.OnUpdateAction = OnUpdateForTestForIntItem;
			item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = 111;
			item.IsChanged = true;
			item.HasValue = true;
			AssertEquals("", testvalueForOnUpdateAction);
			item.SaveAllValues();
			AssertEquals("111", testvalueForOnUpdateAction);
			item.NewValue = 222;
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();
			AssertEquals("222", testvalueForOnUpdateAction);
		}

		public void TestBeforeUpdateAction()
		{
			string value = null;

			IRegistryItemInternals regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);

			regItem.BeforeUpdateAction = (companyPk, branchPk, departmentPk, currentValue) =>
			{
				value = currentValue?.ToString() ?? string.Empty;
			};

			var item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "TST001";
			item.IsChanged = true;
			item.HasValue = true;

			AssertNull("Default to null.", value);

			item.SaveAllValues();
			AssertEquals("Should be the previous value.", "", value);

			item.NewValue = "TST002";
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();

			AssertEquals("Should be the previous value.", "TST001", value);
		}

		public void TestOnDeleteAction()
		{
			string value = null;

			IRegistryItemInternals regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);

			regItem.OnDeleteAction = (companyPk, branchPk, departmentPk, oldValue) =>
			{
				value = oldValue?.ToString() ?? string.Empty;
			};

			var item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			item.NewValue = "TST001";
			item.IsChanged = true;
			item.HasValue = true;

			AssertNull("Default to null.", value);

			item.SaveAllValues();
			AssertNull("Should not log any values.", value);

			item.NewValue = string.Empty;
			item.IsChanged = true;
			item.HasValue = false;
			item.SaveAllValues();

			AssertEquals("Should be the previous value.", "TST001", value);
		}

		[UseSnapshotProtection]
		public void TestSaveAllValuesRollbackWhenOnUpdateActionThrowsException()
		{
			using (RunNonTransactioned()) //SaveAllValues() runs within a transaction and we are testing if the value rolled back
			{
				IRegistryItemInternals regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, options: RegistryOptions.NotCached);
				RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
				item.CompanyPKForTest = Guid.Empty;
				item.BranchPKForTest = Guid.Empty;
				item.DepartmentPKForTest = Guid.Empty;
				item.NewValue = "Value1";
				item.IsChanged = true;
				item.HasValue = true;
				item.SaveAllValues();
				AssertEquals(false, item.IsChanged);
				AssertEquals("Value1", item.RegistryItem.GetValueWithoutFallback(item.CompanyPKForTest, item.BranchPKForTest, item.DepartmentPKForTest));

				item.NewValue = "Value2";
				item.IsChanged = true;
				item.HasValue = true;
				item.SaveAllValues();
				AssertEquals(false, item.IsChanged);
				AssertEquals("Value2", item.RegistryItem.GetValueWithoutFallback(item.CompanyPKForTest, item.BranchPKForTest, item.DepartmentPKForTest));

				regItem.OnUpdateAction = (Guid companyPK, Guid branchPK, Guid departmentPK, object registryValue) => throw new Exception("Random exception for testing");
				item.NewValue = "Value3";
				item.IsChanged = true;
				item.HasValue = true;
				AssertExceptionThrown<Exception>("Should throw", "Random exception for testing", item.SaveAllValues);
				AssertEquals("It's not saved successfully", true, item.IsChanged);
				AssertEquals("Should rollback to Value2 as it's not save successfully", "Value2", item.RegistryItem.GetValueWithoutFallback(item.CompanyPKForTest, item.BranchPKForTest, item.DepartmentPKForTest));

				var reLoadItem = new RegistryItemTagForTest(regItem);
				AssertEquals("Should rollback to Value2", "Value2", reLoadItem.GetValue());
			}
		}

		void OnUpdateForTestForStringItem(Guid companyPK, Guid branchPK, Guid departmentPK, object registryValue)
		{
			testvalueForOnUpdateAction = (string)registryValue;
		}

		void OnUpdateForTestForIntItem(Guid companyPK, Guid branchPK, Guid departmentPK, object registryValue)
		{
			testvalueForOnUpdateAction = registryValue?.ToString() ?? string.Empty;
		}

		public void TestSetFallback()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);

			// System Fallback
			FallbackLevel systemFallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			item.SetFallback(systemFallback);
			AssertEquals("CompanyPK", Guid.Empty, item.CompanyPKForTest);
			AssertEquals("BranchPK", Guid.Empty, item.BranchPKForTest);
			AssertEquals("DepartmentPK", Guid.Empty, item.DepartmentPKForTest);

			// System Department Fallback
			Guid departmentGuid = Guid.NewGuid();
			FallbackLevel departmentFallback = new FallbackLevel(Guid.Empty, Guid.Empty, departmentGuid);
			item.SetFallback(departmentFallback);
			AssertEquals("CompanyPK", Guid.Empty, item.CompanyPKForTest);
			AssertEquals("BranchPK", Guid.Empty, item.BranchPKForTest);
			AssertEquals("DepartmentPK", departmentGuid, item.DepartmentPKForTest);

			// Company Fallback
			Guid companyGuid = Guid.NewGuid();
			FallbackLevel companyFallback = new FallbackLevel(companyGuid, Guid.Empty, Guid.Empty);
			item.SetFallback(companyFallback);
			AssertEquals("CompanyPK", companyGuid, item.CompanyPKForTest);
			AssertEquals("BranchPK", Guid.Empty, item.BranchPKForTest);
			AssertEquals("DepartmentPK", Guid.Empty, item.DepartmentPKForTest);

			// Company Department Fallback
			departmentFallback = new FallbackLevel(companyGuid, Guid.Empty, departmentGuid);
			item.SetFallback(departmentFallback);
			AssertEquals("CompanyPK", companyGuid, item.CompanyPKForTest);
			AssertEquals("BranchPK", Guid.Empty, item.BranchPKForTest);
			AssertEquals("DepartmentPK", departmentGuid, item.DepartmentPKForTest);

			// Branch Fallback
			Guid branchGuid = Guid.NewGuid();
			FallbackLevel branchFallback = new FallbackLevel(companyGuid, branchGuid, Guid.Empty);
			item.SetFallback(branchFallback);
			AssertEquals("CompanyPK", Guid.Empty, item.CompanyPKForTest);
			AssertEquals("BranchPK", branchGuid, item.BranchPKForTest);
			AssertEquals("DepartmentPK", Guid.Empty, item.DepartmentPKForTest);

			// Branch Department Fallback
			departmentFallback = new FallbackLevel(companyGuid, branchGuid, departmentGuid);
			item.SetFallback(departmentFallback);
			AssertEquals("CompanyPK", Guid.Empty, item.CompanyPKForTest);
			AssertEquals("BranchPK", branchGuid, item.BranchPKForTest);
			AssertEquals("DepartmentPK", departmentGuid, item.DepartmentPKForTest);
		}

		public void TestGetAndSetIsChanged()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			Assert("IsChanged should be false", !item.IsChanged);

			item.IsChanged = true;
			item.CompanyPKForTest = Guid.NewGuid();
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			Assert("IsChanged should be false", !item.IsChanged);

			item.CompanyPKForTest = Guid.Empty;
			Assert("IsChanged should be true", item.IsChanged);
		}

		public void TestGetDefault()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, "DefaultValue");
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.DefaultValue", "DefaultValue", item.DefaultValue);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category2", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company, "DefaultValue2");
			item = new RegistryItemTagForTest(regItem);
			AssertEquals("Item.DefaultValue", "DefaultValue2", item.DefaultValue);
		}

		public void TestCustomDefaultValue()
		{
			IRegistryItem item = new DummyRegistryItemImpl("", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.System);

			RegistryItemTag tag = new RegistryItemTag(item);
			AssertEquals("DefaultValue", Guid.Empty.ToString() + Guid.Empty.ToString() + Guid.Empty.ToString(), tag.DefaultValue);

			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			tag.SetFallback(new FallbackLevel(companyPK, branchPK, departmentPK));
			AssertEquals("DefaultValue", Guid.Empty.ToString() + branchPK.ToString() + departmentPK.ToString(), tag.DefaultValue);
		}

		public void TestGetAndSetCurrentFallbackIsInError()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			Assert("CurrentFallbackIsInError should be false", !item.CurrentFallbackIsInError);
			item.CurrentFallbackIsInError = true;

			item.CompanyPKForTest = Guid.NewGuid();
			Assert("CurrentFallbackIsInError should be false", !item.CurrentFallbackIsInError);

			item.CompanyPKForTest = Guid.Empty;
			Assert("CurrentFallbackIsInError should be true", item.CurrentFallbackIsInError);
		}

		public void TestGetAnyFallbackHasError()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
			item.CompanyPKForTest = Guid.Empty;
			item.BranchPKForTest = Guid.Empty;
			item.DepartmentPKForTest = Guid.Empty;
			Assert("AnyFallbackHasError should be false", !item.AnyFallbackHasError);

			item.CurrentFallbackIsInError = true;
			Assert("AnyFallbackHasError should be true", item.AnyFallbackHasError);

			item.CurrentFallbackIsInError = false;
			item.CompanyPKForTest = Guid.Empty;
			item.CurrentFallbackIsInError = true;
			Assert("AnyFallbackHasError should be true", item.AnyFallbackHasError);

			item.CurrentFallbackIsInError = false;
			Assert("AnyFallbackHasError should be false", !item.AnyFallbackHasError);
		}

		public void TestGetIsInErrorCustomPK()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);

			FallbackLevel systemFallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			FallbackLevel departmentFallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.NewGuid());

			item.SetFallback(systemFallback);
			item.CurrentFallbackIsInError = false;
			item.SetFallback(departmentFallback);
			item.CurrentFallbackIsInError = true;

			Assert("SystemFallback should not be in error", !item.GetIsInErrorCustomPK(systemFallback));
			Assert("DepartmentFallback should be in error", item.GetIsInErrorCustomPK(departmentFallback));
		}

		public void TestMustOverrideDefaultValue()
		{
			IRegistryItem item = new StringRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.All);
			var tag = new RegistryItemTag(item);

			item.Options = RegistryOptions.MustOverrideDefaultValue;
			AssertEquals("MustOverrideDefaultValue", true, tag.MustOverrideDefaultValue);

			item.Options = RegistryOptions.Default;
			AssertEquals("MustOverrideDefaultValue", false, tag.MustOverrideDefaultValue);

			item.Options = RegistryOptions.MustOverrideDefaultValue;
			tag.SetFallback(new FallbackLevel(Guid.NewGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("MustOverrideDefaultValue", true, tag.MustOverrideDefaultValue);

			item.Options = RegistryOptions.MustOverrideDefaultValue | RegistryOptions.NotMustOverrideDefaultValueForCompanies;
			AssertEquals("MustOverrideDefaultValue", false, tag.MustOverrideDefaultValue);

			tag.SetFallback(new FallbackLevel(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty));
			AssertEquals("MustOverrideDefaultValue", true, tag.MustOverrideDefaultValue);

			tag.SetFallback(new FallbackLevel(Guid.NewGuid(), Guid.Empty, Guid.NewGuid()));
			AssertEquals("MustOverrideDefaultValue", true, tag.MustOverrideDefaultValue);

			tag.SetFallback(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("MustOverrideDefaultValue", true, tag.MustOverrideDefaultValue);
		}

		[ExpectNoExceptions]
		public void TestValidateItem()
		{
			var item = new Mock<IRegistryItemInternals>();
			var dataType = new Mock<IRegistryDataType>();

			item.Setup(m => m.DataType).Returns(dataType.Object);
			dataType.Setup(m => m.DataType).Returns(typeof(bool));
			dataType.Setup(m =>
				m.Validate(item.Object, It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

			var tag = new RegistryItemTag(item.Object);
			tag.ValidateItem();

			dataType.Verify(m =>
					m.Validate(item.Object, It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()),
				Times.Once);
		}

		[ExpectNoExceptions]
		public void TestValidateItem_WhenRunningPreSaveValidation()
		{
			var item = new Mock<IRegistryItemInternals>();
			var dataType = new Mock<IRegistryDataType>();

			item.Setup(m => m.DataType).Returns(dataType.Object);
			dataType.Setup(m => m.DataType).Returns(typeof(bool));
			dataType.Setup(m => m.ValidateBeforeRegistryFormSave(item.Object, It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
			var tag = new RegistryItemTag(item.Object);
			tag.ValidateItem(true);
			dataType.Verify(
				m => m.ValidateBeforeRegistryFormSave(item.Object, It.IsAny<object>(), It.IsAny<Guid>(),
					It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestValidateItem_DefaultValue()
		{
			var item = new Mock<IRegistryItemInternals>();
			var dataType = new Mock<IRegistryDataType>();
			item.Setup(m => m.DataType).Returns(dataType.Object);

			item.Setup(m => m.GetDefaultValue(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("DefaultValue");
			item.Setup(m => m.GetCurrentValueToUse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(ValueToUse.DefaultValue);
			item.Setup(m => m.GetProposedValue(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("NewValue");
			dataType.Setup(m => m.DataType).Returns(typeof(bool));
			dataType.Setup(m => m.Validate(It.IsAny<IRegistryItem>(), It.Is<object>(o => (string)o == "DefaultValue"),
				It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

			var tag = new RegistryItemTag(item.Object);
			tag.ValidateItem();
			dataType.Verify(m => m.Validate(It.IsAny<IRegistryItem>(), It.Is<object>(o => (string)o == "DefaultValue"),
				It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestValidateItem_OverrideValue()
		{
			var item = new Mock<IRegistryItemInternals>();
			var dataType = new Mock<IRegistryDataType>();
			item.Setup(m => m.DataType).Returns(dataType.Object);

			item.Setup(m => m.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty)).Returns("DefaultValue");
			item.Setup(m => m.GetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty)).Returns(ValueToUse.ProposedValue);
			item.Setup(m => m.GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty)).Returns("NewValue");
			dataType.Setup(m => m.DataType).Returns(typeof(bool));
			dataType.Setup(m => m.Validate(It.IsAny<IRegistryItem>(), It.Is<object>(o => (string)o == "NewValue"),
				It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

			var tag = new RegistryItemTag(item.Object);
			tag.ValidateItem(false);
			dataType.Verify(m => m.Validate(It.IsAny<IRegistryItem>(), It.Is<object>(o => (string)o == "NewValue"),
				It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
		}

		#region Implementation

		protected override void TearDown()
		{
			EnvProxy.SetHostedLocationForTest(null);
			base.TearDown();
		}

		string testvalueForOnUpdateAction;

		#endregion
	}

	public class RegistryItemTagForTest : RegistryItemTag
	{
		public bool IsClearAllCalled;

		public RegistryItemTagForTest(IRegistryItem item)
			: base(item)
		{
		}

		public Guid CompanyPKForTest
		{
			get { return CompanyPK; }
			set { CompanyPK = value; }
		}

		public Guid BranchPKForTest
		{
			get { return BranchPK; }
			set { BranchPK = value; }
		}

		public Guid DepartmentPKForTest
		{
			get { return DepartmentPK; }
			set { DepartmentPK = value; }
		}

		public override void ClearAll()
		{
			base.ClearAll();
			IsClearAllCalled = true;
		}
	}
}
