using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	class RegistryItemTest : TransactionedTestCase
	{
		public void TestGetLocation()
		{
			IRegistryDataType dataType = new TestRegistryDataType();
			IRegistryItem registryItem = new RegistryItemImpl("Name", (NoResString)"HELLO/WORLD", (NoResString)"Caption", (NoResString)"Hint", dataType, RegistryStorageFlags.System);
			AssertEquals("Location", "HELLO -> WORLD -> Caption", registryItem.GetLocation());
		}

		public void TestDoNotReportInnerInnerOutOfMemoryExceptionAsBogusXml()
		{
			var testDataType = new TestRegistryItemImpl(GetDataTypeThatThrowsOOM);
			try
			{
				testDataType.Deserialise(Array.Empty<byte>());
				Assert("Exception should have been thrown", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Exception was thrown OK", "You suck", ex.Message);
			}
			AssertEquals("The error reporter should not have reported this", null, ErrorReporter.LastExceptionReported);
		}

		public void TestDeserialise_WithoutUserInteractive_ThrowsRegistryJsonExceptionWithName()
		{
			var originalGlobalValue = Globals.IsWeb;
			using (Globals.SetIsWebForTest(true))
			{
				var testDataType = new TestRegistryItemImpl(GetDataTypeThatThrowsJsonException);
				var registryJsonException = AssertExceptionThrown<RegistryJsonException>("Invalid Json Registry should throw RegistryJsonException.", "Invalid JSON Registry!", () => testDataType.Deserialise(Array.Empty<byte>()));
				AssertEquals("Registry name should match from the exception", "DummyRegistryItem", registryJsonException.RegistryName);
			}
		}

		public void TestDeserialise_WithUserInteractive_OnJsonException_ShowsErrorToUser()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var testDataType = new TestRegistryItemImpl(GetDataTypeThatThrowsJsonException);
				var item = testDataType.Deserialise(Array.Empty<byte>());
				var message = UnitTestUserNotification.Instance.LastMessage;

				AssertNull("Registry item should be null.", item);
				AssertEquals("The pop up message should have Error as caption", "Error", message.Caption);
				AssertContains("The pop up message should have DummyRegistryItem in text", "DummyRegistryItem", message.Text);

				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		class TestRegistryItemImpl : RegistryItemImpl
		{
			public TestRegistryItemImpl(Func<IRegistryDataType> getDataTypeThatThrowsException)
				: base(name: "DummyRegistryItem", caption: null, hint: null, dataType: getDataTypeThatThrowsException(), editorInfo: null, storage: RegistryStorageFlags.All, options: RegistryOptions.Default, defaultValue: null, useDefaultDefaultValue: true, categories: null)
			{ }
		}

		static IRegistryDataType GetDataTypeThatThrowsOOM()
		{
			var innerInnerOOM = new OutOfMemoryException("Because 96GB is just not enough");
			var innerInvalidOperationException = new InvalidOperationException("Invalid operation", innerInnerOOM);
			var upperMopstException = new Exception("You suck", innerInvalidOperationException);

			return GetDataTypeThatThrowsException(upperMopstException);
		}

		static IRegistryDataType GetDataTypeThatThrowsJsonException()
		{
			return GetDataTypeThatThrowsException(new JsonException());
		}

		static IRegistryDataType GetDataTypeThatThrowsException(Exception exception)
		{
			var mock = new Mock<IRegistryDataType>();
			mock.Setup(o => o.Deserialise(It.IsAny<byte[]>())).Throws(exception);

			return mock.Object;
		}

		#region Test the Constructor

		public void TestConstructor()
		{
			IRegistryDataType dataType = new TestRegistryDataType();
			RegistryEditorInfo editorInfo = new NumericRegistryEditorInfo(0);

			IRegistryItem registryItem = new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", dataType, RegistryStorageFlags.System);
			AssertRegistryItem(registryItem, "Name", "Category", "Caption", "Hint", dataType, dataType.DefaultEditorInfo, RegistryStorageFlags.System, RegistryOptions.Default, "", new string[] { "Category" });

			registryItem = new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", dataType, RegistryStorageFlags.System, RegistryOptions.IsHidden);
			AssertRegistryItem(registryItem, "Name", "Category", "Caption", "Hint", dataType, dataType.DefaultEditorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "", new string[] { "Category" });

			registryItem = new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", dataType, RegistryStorageFlags.System, "Default");
			AssertRegistryItem(registryItem, "Name", "Category", "Caption", "Hint", dataType, dataType.DefaultEditorInfo, RegistryStorageFlags.System, RegistryOptions.Default, "Default", new string[] { "Category" });

			registryItem = new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", dataType, RegistryStorageFlags.System, RegistryOptions.IsHidden, "Default");
			AssertRegistryItem(registryItem, "Name", "Category", "Caption", "Hint", dataType, dataType.DefaultEditorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "Default", new string[] { "Category" });

			registryItem = new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "Default");
			AssertRegistryItem(registryItem, "Name", "Category", "Caption", "Hint", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "Default", new string[] { "Category" });

			registryItem = new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "Default", true);
			AssertRegistryItem(registryItem, "Name", "Category", "Caption", "Hint", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "", new string[] { "Category" });

			registryItem = new RegistryItemImpl("Name", (NoResString)"Caption", (NoResString)"Hint", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "Default", false, (NoResString)"Category1", (NoResString)"Category2");
			AssertRegistryItem(registryItem, "Name", "Category1", "Caption", "Hint", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsHidden, "Default", new string[] { "Category1", "Category2" });
		}

		void AssertRegistryItem(IRegistryItem registryItem, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, IRegistryDataType expectedDataType,
			IRegistryEditorInfo expectedEditorInfo, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, object expectedDefaultValue, string[] expectedCategories)
		{
			AssertEquals("Name", expectedName, registryItem.Name);
			AssertEquals("Category", expectedCategory, registryItem.Category);
			AssertEquals("Caption", expectedCaption, registryItem.Caption);
			AssertEquals("Hint", expectedHint, registryItem.Hint);
			AssertEquals("DataType", expectedDataType, registryItem.DataType);
			AssertEquals("EditorInfo", expectedEditorInfo, registryItem.EditorInfo);
			AssertEquals("Storage", expectedStorage, registryItem.Storage);
			AssertEquals("Options", expectedOptions, registryItem.Options);
			AssertEquals("DefaultValue", expectedDefaultValue, registryItem.DefaultValue);
			AssertEquals("Categories.Length", expectedCategories.Length, registryItem.Categories.Length);

			for (int i = 0; i < expectedCategories.Length; i++)
			{
				AssertEquals("Categories[" + i + "]", expectedCategories[i], registryItem.Categories[i]);
			}
		}

		#endregion

		public void TestChangeDBConnection()
		{
			var item = new StringRegistryItem("TestItem", null, null, null, RegistryStorageFlags.All, RegistryOptions.NotCached, "Test Default");
			var connectionAdjustable = item.Inner as IConnectionAdjustable;
			AssertNotNull(connectionAdjustable);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Result");

			using (Db.DisposableActionForDbConnection())
			using (var newConnection = Db.NewExtraConnectionWithTargetDatabaseSpecificCredentials(Db.ServerName, Db.DatabaseName))
			using (connectionAdjustable.SetTemporaryConnection(newConnection))
			{
				newConnection.CloseConnection();
				AssertEquals("Test Default", item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			}

			AssertEquals("Test Result", item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		[ExpectNoExceptions]
		public void TestInvalidIsVisibleThrowsNoException()
		{
			// Thanks Timothy Stiles
			BinaryRegistryItem item = new BinaryRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.All);
			item.CountryFilterPKs = new List<Guid>();
			((List<Guid>)item.CountryFilterPKs).Add(Guid.NewGuid());
			AssertEquals(false, item.IsVisible(Guid.NewGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestProposedValue()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", null, null, null, RegistryStorageFlags.All, false.ToString());

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Real Value In Enterprise");
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Real Value In Company");

			AssertEquals("GetProposeValue in Enterprise", null, ((IRegistryItemInternals)item).GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetProposedValue in Company", null, ((IRegistryItemInternals)item).GetProposedValue(CompanyPK, Guid.Empty, Guid.Empty));

			((IRegistryItemInternals)item).SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue In Enterprise");
			((IRegistryItemInternals)item).SetProposedValue(CompanyPK, Guid.Empty, Guid.Empty, "NewValue In Company");

			AssertEquals("GetProposedValue in Enterprise", "NewValue In Enterprise", ((IRegistryItemInternals)item).GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetProposedValue in Company", "NewValue In Company", ((IRegistryItemInternals)item).GetProposedValue(CompanyPK, Guid.Empty, Guid.Empty));

			((IRegistryItemInternals)item).ClearProposedCache();

			AssertEquals("GetProposeValue in Enterprise", null, ((IRegistryItemInternals)item).GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetProposedValue in Company", null, ((IRegistryItemInternals)item).GetProposedValue(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestHasActualValue()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", null, null, null, RegistryStorageFlags.All, false.ToString());

			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)item).HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)item).HasActualValue(CompanyPK, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true.ToString());
			AssertEquals("HasActualValue", true, ((IRegistryItemInternals)item).HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)item).HasActualValue(CompanyPK, Guid.Empty, Guid.Empty));

			item.SetValue(CompanyPK, Guid.Empty, Guid.Empty, false.ToString());
			AssertEquals("HasActualValue", true, ((IRegistryItemInternals)item).HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("HasActualValue", true, ((IRegistryItemInternals)item).HasActualValue(CompanyPK, Guid.Empty, Guid.Empty));

			((IRegistryItemInternals)item).DeleteValue(CompanyPK, Guid.Empty, Guid.Empty);
			AssertEquals("HasActualValue", true, ((IRegistryItemInternals)item).HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)item).HasActualValue(CompanyPK, Guid.Empty, Guid.Empty));

			((IRegistryItemInternals)item).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)item).HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)item).HasActualValue(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestHasActualValueIsCached()
		{
			int previousCount = Db.Connection.ExecutedCommandCount;

			IRegistryItem item = new StringRegistryItem("TestItem", null, null, null, RegistryStorageFlags.All);
			IRegistryItemInternals itemInternals = (IRegistryItemInternals)item;

			bool hasValue = itemInternals.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty);
			hasValue = itemInternals.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty);
			hasValue = itemInternals.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty);

			AssertEquals("HasActualValue() should be cached.", previousCount + 1, Db.Connection.ExecutedCommandCount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCacheDeserialisedValue()
		{
			string testPath = Path.Combine(TestCase.BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs");
			string gifFile = Path.Combine(testPath, "small.gif");

			using (Image gif = Image.FromFile(gifFile))
			{
				IRegistryItem item = new ImageRegistryItem("TestImageItem", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
				IRegistryItemInternals itemInternals = (IRegistryItemInternals)item;
				bool hasValue = itemInternals.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("HasActualValue", false, hasValue);

				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gif);
				Image imageValue = (Image)item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				Assert("IsImageEqual by value", Utilities.IsImageEqual(gif, imageValue));
				AssertEquals("The stored value should not be the same instance as the original item. This is because we only want the stored value to be updated after explicitly calling RegistryItemImpl.SetValue. If they were the same instance, then it would change whenever the original item is changed.", false, ReferenceEquals(gif, imageValue));
			}
		}

		public void TestCurrentValueToUse()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", null, null, null, RegistryStorageFlags.All, false.ToString());
			IRegistryItemInternals itemInternals = (IRegistryItemInternals)item;

			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.Empty;
			Guid departmentPK = Guid.NewGuid();

			AssertEquals("ItemInternals.GetCurrentValueToUse()", ValueToUse.DefaultValue, itemInternals.GetCurrentValueToUse(companyPK, branchPK, departmentPK));

			item.SetValue(companyPK, branchPK, departmentPK, "Value");
			AssertEquals("ItemInternals.GetCurrentValueToUse()", ValueToUse.SavedValue, itemInternals.GetCurrentValueToUse(companyPK, branchPK, departmentPK));

			itemInternals.SetCurrentValueToUse(companyPK, branchPK, departmentPK, ValueToUse.ProposedValue);
			AssertEquals("ItemInternals.GetCurrentValueToUse()", ValueToUse.ProposedValue, itemInternals.GetCurrentValueToUse(companyPK, branchPK, departmentPK));
			itemInternals.SetCurrentValueToUse(companyPK, branchPK, departmentPK, ValueToUse.DefaultValue);
			AssertEquals("ItemInternals.GetCurrentValueToUse()", ValueToUse.DefaultValue, itemInternals.GetCurrentValueToUse(companyPK, branchPK, departmentPK));

			companyPK = Guid.Empty;
			branchPK = Guid.NewGuid();
			itemInternals.SetCurrentValueToUse(companyPK, branchPK, departmentPK, ValueToUse.ProposedValue);
			AssertEquals("ItemInternals.GetCurrentValueToUse()", ValueToUse.ProposedValue, itemInternals.GetCurrentValueToUse(companyPK, branchPK, departmentPK));

			itemInternals.ClearCurrentValueToUseCache();
			AssertEquals("ItemInternals.GetCurrentValueToUse()", ValueToUse.DefaultValue, itemInternals.GetCurrentValueToUse(companyPK, Guid.Empty, departmentPK));
			AssertEquals("ItemInternals.GetCurrentValueToUse()", ValueToUse.DefaultValue, itemInternals.GetCurrentValueToUse(Guid.Empty, branchPK, departmentPK));
		}

		public void TestItem()
		{
			const string Name = "TestItem";
			const string HintText = "This is a test";
			const string Caption = "A Nice Test Item";

			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			StringRegistryItem item = new StringRegistryItem(Name, (NoResString)"", (NoResString)Caption, (NoResString)HintText, RegistryStorageFlags.All, "Default");

			AssertEquals("Hint", HintText, item.Hint);
			AssertEquals("DefaultValue", "Default", item.DefaultValue);
			AssertEquals("Caption", Caption, item.Caption);
			AssertEquals("DataType", RegistryDataTypes.StringType.GetType(), item.DataType.GetType());

			AssertEquals("IsOnlyForDevelopers", false, item.HasOption(RegistryOptions.IsOnlyForDevelopers));
			AssertEquals("IsReadOnly", false, item.IsReadOnly);

			AssertEquals("StorageLevel", RegistryStorageFlags.All, RegistryStorageFlags.All & item.Storage);
			AssertEquals("StorageLevel", RegistryStorageFlags.Branch, RegistryStorageFlags.Branch & item.Storage);
			AssertEquals("StorageLevel", RegistryStorageFlags.BranchDepartment, RegistryStorageFlags.BranchDepartment & item.Storage);
			AssertEquals("StorageLevel", RegistryStorageFlags.Company, RegistryStorageFlags.Company & item.Storage);
			AssertEquals("StorageLevel", RegistryStorageFlags.CompanyDepartment, RegistryStorageFlags.CompanyDepartment & item.Storage);
			AssertEquals("StorageLevel", RegistryStorageFlags.System, RegistryStorageFlags.System & item.Storage);
			AssertEquals("StorageLevel", RegistryStorageFlags.SystemDepartment, RegistryStorageFlags.SystemDepartment & item.Storage);
		}

		public void TestGetSetValueForCompanyAndBranch()
		{
			GuidRegistryItem item = new GuidRegistryItem("Test", (NoResString)"", (NoResString)"Test", (NoResString)"Test", RegistryStorageFlags.All);

			bool exceptionThrown = false;
			try
			{
				AssertEquals("GetValue", "", ((IRegistryItem)item).GetValueWithoutFallback(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			}
			catch (ArgumentException)
			{
				exceptionThrown = true;
			}
			AssertEquals("ExceptionThrown", true, exceptionThrown);

			exceptionThrown = false;
			try
			{
				((IRegistryItem)item).SetValue(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
			}
			catch (ArgumentException)
			{
				exceptionThrown = true;
			}
			AssertEquals("ExceptionThrown", true, exceptionThrown);
		}

		public void TestSetDepartmentsAllowed()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BinaryRegistryItem item1 = new BinaryRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.All);
			BinaryRegistryItem onlyForSupportItem = new BinaryRegistryItem("OnlyForSupportItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.All, RegistryOptions.IsOnlyForSupport);
			BinaryRegistryItem item2 = new BinaryRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.All);

			Guid airPK = GetDepartmentPK("TLA");
			IGlbDepartment airDepartment = factory.Load<IGlbDepartment>(airPK);
			Guid airImportPK = GetDepartmentPK("CIA");
			IGlbDepartment airImportDepartment = factory.Load<IGlbDepartment>(airImportPK);
			Guid airExportPK = GetDepartmentPK("CEA");
			IGlbDepartment airExportDepartment = factory.Load<IGlbDepartment>(airExportPK);
			Guid seaPK = GetDepartmentPK("TLC");
			IGlbDepartment seaDepartment = factory.Load<IGlbDepartment>(seaPK);
			Guid seaImportPK = GetDepartmentPK("CIS");
			IGlbDepartment seaImportDepartment = factory.Load<IGlbDepartment>(seaImportPK);
			Guid seaExportPK = GetDepartmentPK("CES");
			IGlbDepartment seaExportDepartment = factory.Load<IGlbDepartment>(seaExportPK);

			AssertEquals("DepartmentsAllowed", DepartmentFlags.All, item2.DepartmentsAllowed); // all should be default
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think

			item2.DepartmentsAllowed = DepartmentFlags.Air;
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("IsVisible", false, onlyForSupportItem.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
				AssertEquals("IsVisible", false, onlyForSupportItem.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
				AssertEquals("IsVisible", false, onlyForSupportItem.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
				AssertEquals("IsVisible", false, onlyForSupportItem.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
				AssertEquals("IsVisible", false, onlyForSupportItem.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
				AssertEquals("IsVisible", false, onlyForSupportItem.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
				AssertEquals("IsVisible", false, onlyForSupportItem.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think
			}

			item1.DepartmentsAllowed = DepartmentFlags.Air;
			AssertEquals("IsVisible", true, item1.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", true, item1.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", true, item1.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", false, item1.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", false, item1.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", false, item1.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item1.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think

			item2.DepartmentsAllowed = DepartmentFlags.ImportAir;
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think

			item2.DepartmentsAllowed = DepartmentFlags.ExportAir;
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think

			item2.DepartmentsAllowed = DepartmentFlags.Sea;
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think

			item2.DepartmentsAllowed = DepartmentFlags.ImportSea;
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think

			item2.DepartmentsAllowed = DepartmentFlags.ExportSea;
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airPK, airDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airImportPK, airImportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, airExportPK, airExportDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaPK, seaDepartment));
			AssertEquals("IsVisible", false, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaImportPK, seaImportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, seaExportPK, seaExportDepartment));
			AssertEquals("IsVisible", true, item2.IsVisible(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));//think
		}

		Guid GetDepartmentPK(string departmentCode)
		{
			return (Guid)Db.Connection.ExecuteScalar(
				"select " + GlbDepartmentSchema.PK.Name +
				" from " + GlbDepartmentSchema.Constants.SqlSchemaName + "." + GlbDepartmentSchema.Constants.TableName +
				" where " + GlbDepartmentSchema.GE_Code.Name + " = @code",
				cmd => cmd.AddParameterBasedOnDbColumn("@code", departmentCode, GlbDepartmentSchema.GE_Code));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestCheckCompanyStorageFlag()
		{
			IntRegistryItem item = new IntRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.BranchDepartment);
			((IRegistryItem)item).GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestCheckBranchStorageFlag()
		{
			IntRegistryItem item = new IntRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.CompanyDepartment);
			((IRegistryItem)item).GetValueWithoutFallback(Guid.Empty, BranchPK, Guid.Empty);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestCheckSystemStorageFlag1()
		{
			IntRegistryItem item = new IntRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.System);
			((IRegistryItem)item).GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestCheckSystemStorageFlag2()
		{
			IntRegistryItem item = new IntRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.System);
			((IRegistryItem)item).GetValueWithoutFallback(Guid.Empty, BranchPK, Guid.Empty);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestCheckSystemStorageFlag3()
		{
			IntRegistryItem item = new IntRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"It is a Test", RegistryStorageFlags.System);
			((IRegistryItem)item).GetValueWithoutFallback(Guid.Empty, Guid.Empty, DepartmentPK);
		}

		public void TestGetFallBackValueAtAllLevels()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.All, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", "Default", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SystemWide");
			AssertEquals("GetFallBackValueAtAllLevels", "SystemWide", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(Guid.Empty, Guid.Empty, DepartmentPK, "DepartmentWide");
			AssertEquals("GetFallBackValueAtAllLevels", "DepartmentWide", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "CompanyWide");
			AssertEquals("GetFallBackValueAtAllLevels", "CompanyWide", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(CompanyPK, Guid.Empty, DepartmentPK, "CompanyDepartmentWide");
			AssertEquals("GetFallBackValueAtAllLevels", "CompanyDepartmentWide", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(Guid.Empty, BranchPK, Guid.Empty, "BranchWide");
			AssertEquals("GetFallBackValueAtAllLevels", "BranchWide", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
			AssertEquals("GetFallBackValueAtAllLevels", "BranchWide", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(Guid.Empty, BranchPK, Guid.Empty));

			item.SetValue(Guid.Empty, BranchPK, DepartmentPK, "BranchDepartmentWide");
			AssertEquals("GetFallBackValueAtAllLevels", "BranchDepartmentWide", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestIsCached()
		{
			StringRegistryItem cachedItem = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.All, "Default");
			StringRegistryItem unCachedItem = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.All, RegistryOptions.NotCached, "Default");

			unCachedItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestValue");
			AssertEquals("GetValue cached", "TestValue", cachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			unCachedItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
			AssertEquals("GetValue cached", "TestValue", cachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetValue UnCached", "NewValue", unCachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetFallBackValueAtAllLevels not cached because uses fallback", "NewValue", cachedItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetFallBackValueAtAllLevels UnCached", "NewValue", unCachedItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			cachedItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue2");
			AssertEquals("GetValue cached", "NewValue2", cachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetValue UnCached", "NewValue2", unCachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetFallBackValueAtAllLevels cached", "NewValue2", cachedItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetFallBackValueAtAllLevels UnCached", "NewValue2", unCachedItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGetFallBackLevel()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.All, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.System, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SystemWide");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.System, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(Guid.Empty, Guid.Empty, DepartmentPK, "DepartmentWide");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.SystemDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "CompanyWide");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.Company, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(CompanyPK, Guid.Empty, DepartmentPK, "CompanyDepartmentWide");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.CompanyDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(Guid.Empty, BranchPK, Guid.Empty, "BranchWide");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.Branch, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(Guid.Empty, BranchPK, DepartmentPK, "BranchDepartmentWide");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.BranchDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			//now test them on different levels
			((IRegistryItemInternals)item).DeleteValue(CompanyPK, Guid.Empty, Guid.Empty);
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.SystemDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, Guid.Empty, Guid.Empty));

			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.CompanyDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, Guid.Empty, DepartmentPK));

			((IRegistryItemInternals)item).DeleteValue(CompanyPK, Guid.Empty, DepartmentPK);
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.SystemDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, Guid.Empty, DepartmentPK));

			((IRegistryItemInternals)item).DeleteValue(Guid.Empty, BranchPK, DepartmentPK);
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.Branch, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestGetFallBackLevelForRegistryThatDoesntFallBackThroughAllLevels()
		{
			StringRegistryItem item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.All, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.System, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.SystemDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.Company, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.CompanyDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.Branch, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));

			item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.BranchDepartment, "Default");
			AssertEquals("GetFallBackValueAtAllLevels", RegistryStorageFlags.BranchDepartment, ((IRegistryItemInternals)item).GetFallBackLevel(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestSetDefaultValueWillBePreservedButCanBeDeleted()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", (NoResString)"", (NoResString)"Item", (NoResString)"Test", RegistryStorageFlags.All, "Default");
			AssertEquals("Fallback value by default", "Default", (string)((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "CompanyLevel");
			AssertEquals("Fallback value when Company level is set", "CompanyLevel", (string)((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			item.SetValue(CompanyPK, Guid.Empty, DepartmentPK, "Default");
			AssertEquals("Fallback value when company department level is set to Default", "Default", (string)((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));

			((IRegistryItemInternals)item).DeleteValue(CompanyPK, Guid.Empty, DepartmentPK);
			AssertEquals("Fallback value when company department level is deleted", "CompanyLevel", (string)((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestCachingOfFallBackItems()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", null, null, null, RegistryStorageFlags.Branch | RegistryStorageFlags.Company);
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Company");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Branch");
			AssertEquals("GetFallBackValueAtAllLevels", "Branch", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "NewBranch");
			AssertEquals("GetFallBackValueAtAllLevels", "NewBranch", ((IRegistryItemInternals)item).GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
		}

		public void TestGetCurrentValueFromProposedValueAccessor()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All, "");
			IRegistryItemInternals itemInternals = item;

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Enterprise");
			item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "Enterprise Department");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Company");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "Company Department");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Branch");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Branch Department");

			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "Enterprise", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "Enterprise Department", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "Company", itemInternals.GetCurrentValueFromProposedValueAccessor(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "Company Department", itemInternals.GetCurrentValueFromProposedValueAccessor(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "Branch", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "Branch Department", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, "New Enterprise");
			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "New Enterprise Department");
			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "New Company");
			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "New Company Department");
			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "New Branch");
			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "New Branch Department");

			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);

			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "New Enterprise", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "New Enterprise Department", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "New Company", itemInternals.GetCurrentValueFromProposedValueAccessor(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "New Company Department", itemInternals.GetCurrentValueFromProposedValueAccessor(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "New Branch", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "New Branch Department", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);

			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "", itemInternals.GetCurrentValueFromProposedValueAccessor(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "", itemInternals.GetCurrentValueFromProposedValueAccessor(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));
			AssertEquals("GetCurrentValueFromProposedValueAccessor()", "", itemInternals.GetCurrentValueFromProposedValueAccessor(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
		}

		public void TestCountryFilterPK()
		{
			IRegistryItem item = new StringRegistryItem("TestItem", null, null, null, RegistryStorageFlags.Branch | RegistryStorageFlags.Company);
			AssertEquals("IsVisible", true, item.IsVisible(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("IsVisible", true, item.IsVisible(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			item.CountryFilterPKs = new[] { new Guid("DD0E0EA6-BF30-4876-9C2B-51BB5185FDE2") }; // random guid
			AssertEquals("Is not Visible when country filer pks is not null", false, item.IsVisible(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("Is not Visible when country filer pks is not null", false, item.IsVisible(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			item.CountryFilterPKs = null;
			AssertExceptionThrown("Exception thrown when CountryFilterPKs is null. If you want to allow all countries visible (aka no filter at all), set it to Enumerable.Empty", typeof(ArgumentNullException), delegate
			{ item.IsVisible(Guid.Empty, Guid.Empty, Guid.Empty); });

			item = new MockRegistryItemWithOtherCountry("TestItem", "", "", "", RegistryDataTypes.StringType, RegistryStorageFlags.Branch | RegistryStorageFlags.Company);
			AssertEquals("IsVisible", false, item.IsVisible(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("IsVisible", false, item.IsVisible(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
		}

		public void TestRegistryItemWithDifferentNameWillWorkWhileCached()
		{
			IRegistryItem item = new StringRegistryItem("TestName1", null, null, null, RegistryStorageFlags.Company);
			item.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, "NewValue");

			item.Name = "TestName2";
			AssertEquals("GetValue when name is different", "", item.Value);
		}

		public void TestLocations()
		{
			AssertEquals("SMTPServer Location", "Physical Server -> Mail -> Outgoing -> SMTP -> SMTP Server", ((IRegistryItemInternals)EnvProxy.Instance.Registry.RawRegistry.SMTPServer).Location);
			AssertEquals("AUCCompanyCertificatePassword Location", "Customs -> Country or Region Specific -> Australia -> CMR -> Private Key Password", ((IRegistryItemInternals)EnvProxy.Instance.Registry.RawRegistry.AUCCompanyCertificatePassword).Location);
		}

		public void TestLocationForCategory()
		{
			var item = new StringRegistryItem("TestName1", new MultilingualString[] { (NoResString)"HELLO/WORLD", (NoResString)"GOODBYE/WORLD" }, (NoResString)"Caption", (NoResString)"Hint", new StringRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default);
			IRegistryItemInternals internalItem = item;
			AssertEquals("Location", "HELLO -> WORLD -> Caption", internalItem.LocationForCategory("SDFS"));
			AssertEquals("Cannot get the location path for registry 'TestName1' as it has no category matching 'SDFS'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertEquals("Location", "HELLO -> WORLD -> Caption", internalItem.LocationForCategory("HELLO/WORLD"));
			AssertEquals("", ErrorReporter.LastMessageReported);
			AssertEquals("Location", "GOODBYE -> WORLD -> Caption", internalItem.LocationForCategory("GOODBYE/WORLD"));
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestIsReadOnly()
		{
			IRegistryItem item = new StringRegistryItem("TestName1", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsReadOnly, "");
			Assert("ReadOnly", item.IsReadOnly);
		}

		public void TestIsLockedDown()
		{
			IRegistryItem item = new StringRegistryItem("TestName3", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "");
			AssertEquals("Pre-condition: should not be locked down", false, item.IsLockedDown);
			AssertEquals("Pre-condition: Should not be read only", false, item.IsReadOnly);

			EnvProxy.SetHostedLocationForTest("SYD");

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				if (!item.IsLockedDown)
				{
					//debugging information, remove once cause is known
					AssertEquals(string.Format("IsHostedWithCargowise: {0}", EnvProxy.HostedLocation), true, EnvProxy.IsHostedWithCargowise);
					AssertEquals("HasOption", true, item.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
					AssertEquals("WebUser is not Programmer", false, EnvProxy.Instance.CurrentUser.IsDeveloper);
				}
				AssertEquals("Should be locked down", true, item.IsLockedDown);
				AssertEquals("Should be read only", true, item.IsReadOnly);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.SupportUserName))
			{
				AssertEquals("Should not be locked down", false, item.IsLockedDown);
				AssertEquals("Should not be read only", false, item.IsReadOnly);
			}

			EnvProxy.SetHostedLocationForTest("NCW");

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("Should not be locked down since not hosted with CargoWise", false, item.IsLockedDown);
				AssertEquals("Should not be read onlysince not hosted with CargoWise ", false, item.IsReadOnly);
			}
		}

		public void TestIsLockedDown_MissingHostingInfo()
		{
			IRegistryItem item = new StringRegistryItem("TestNameX", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "");

			EnvProxy.SetHostedLocationForTest("");

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("Should not be locked down for licence with no hosting information", false, item.IsLockedDown);
				AssertEquals("Should not be read only for licence with no hosting information", false, item.IsReadOnly);
			}
		}

		#region TestValueProperty

		public void TestValueProperty()
		{
			TestValueProperty(RegistryStorageFlags.System, false, false, false);
			TestValueProperty(RegistryStorageFlags.SystemDepartment, false, false, true);
			TestValueProperty(RegistryStorageFlags.Company, true, false, false);
			TestValueProperty(RegistryStorageFlags.CompanyDepartment, true, false, true);
			TestValueProperty(RegistryStorageFlags.Branch, false, true, false);
			TestValueProperty(RegistryStorageFlags.BranchDepartment, false, true, true);

			TestValueProperty(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true, false, false);
			TestValueProperty(RegistryStorageFlags.All, true, false, false);
		}

		public void TestValueProperty(RegistryStorageFlags storageLevel, bool specifyCompany, bool specifyBranch, bool specifyDepartment)
		{
			IRegistryItem item = new StringRegistryItem("x", (NoResString)"x", (NoResString)"x", (NoResString)"x", storageLevel);
			item.SetValue(
				specifyCompany ? EnvProxy.Instance.CurrentCompany.PK : Guid.Empty,
				specifyBranch ? EnvProxy.Instance.CurrentBranch.PK : Guid.Empty,
				specifyDepartment ? EnvProxy.Instance.CurrentDepartment.PK : Guid.Empty, "value");

			AssertEquals("Should find the value ok", "value", item.Value);
		}

		#endregion

		public void TestDefaultValue()
		{
			RegistryItemImpl registryItem1 = new RegistryItemImpl("", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.System, "Default Text.");
			DummyRegistryItemImpl registryItem2 = new DummyRegistryItemImpl("", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.System, "Default Text.");
			RegistryItemImpl registryItem3 = new RegistryItemImpl("", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.System);

			AssertEquals("RegistryItem1.DefaultValue", "Default Text.", registryItem1.DefaultValue);
			AssertEquals("RegistryItem2.DefaultValue", Guid.Empty.ToString() + Guid.Empty.ToString() + Guid.Empty.ToString(), registryItem2.DefaultValue);
			AssertEquals("RegistryItem3.DefaultValue", "", registryItem3.DefaultValue);

			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid guid3 = Guid.NewGuid();

			AssertEquals("RegistryItem1.GetDefaultValue()", "Default Text.", registryItem1.GetDefaultValue(guid1, guid2, guid3));
			AssertEquals("RegistryItem2.GetDefaultValue()", guid1.ToString() + guid2.ToString() + guid3.ToString(), registryItem2.GetDefaultValue(guid1, guid2, guid3));
			AssertEquals("RegistryItem3.GetDefaultValue()", "", registryItem3.GetDefaultValue(guid1, guid2, guid3));
		}

		public void TestDefaultValueIsClone()
		{
			TestRegistryDataType dataType = new TestRegistryDataType();
			dataType.IsTestingClone = true;
			RegistryItemImpl item = new RegistryItemImpl("", null, null, null, dataType, RegistryStorageFlags.System, "Default Text.");
			AssertEquals("DefaultValue", "Cloned Default Text.", item.DefaultValue);
			AssertEquals("Value", "Cloned Default Text.", item.Value);
			AssertEquals("GetValue()", "Cloned Default Text.", item.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGetAndSetGuidValue()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				var registryItem3 = new RegistryItemImpl("RegistryItem3", null, null, null, new GuidRegistryDataType(), RegistryStorageFlags.All, Guid.Empty);
				AssertEquals("IsPKReferencedByRegistry should return false for the empty guid", false, registryDataAccessor.IsPKReferencedByRegistry(Guid.Empty));

				var value2ForRegistryItem3 = Guid.NewGuid();
				registryItem3.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value2ForRegistryItem3);
				AssertEquals("IsPKReferencedByRegistry should return true for the non-empty guid", true, registryDataAccessor.IsPKReferencedByRegistry(value2ForRegistryItem3));
			}
		}

		public void TestGetAndSetValue()
		{
			RegistryItemImpl registryItem1 = new RegistryItemImpl("ApplePie", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.All, "Apple Pie");
			DummyRegistryItemImpl registryItem2 = new DummyRegistryItemImpl("BlueberryPie", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.All, "Blueberry Pie");

			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			// System Level
			AssertEquals("RegistryItem1.GetValue()", "Apple Pie", registryItem1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("RegistryItem2.GetValue()", Guid.Empty.ToString() + Guid.Empty.ToString() + Guid.Empty.ToString(), registryItem2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			registryItem1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "System 1");
			AssertEquals("RegistryItem1.GetValue()", "System 1", registryItem1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			registryItem2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "System 2");
			AssertEquals("RegistryItem2.GetValue()", "System 2", registryItem2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			// System Department Level
			AssertEquals("RegistryItem1.GetValue()", "Apple Pie", registryItem1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, departmentPK));
			AssertEquals("RegistryItem2.GetValue()", Guid.Empty.ToString() + Guid.Empty.ToString() + departmentPK.ToString(), registryItem2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, departmentPK));

			registryItem1.SetValue(Guid.Empty, Guid.Empty, departmentPK, "System Department 1");
			registryItem2.SetValue(Guid.Empty, Guid.Empty, departmentPK, "System Department 2");

			AssertEquals("RegistryItem1.GetValue()", "System Department 1", registryItem1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, departmentPK));
			AssertEquals("RegistryItem2.GetValue()", "System Department 2", registryItem2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, departmentPK));

			// Company Level
			AssertEquals("RegistryItem1.GetValue()", "Apple Pie", registryItem1.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			AssertEquals("RegistryItem2.GetValue()", companyPK.ToString() + Guid.Empty.ToString() + Guid.Empty.ToString(), registryItem2.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));

			registryItem1.SetValue(companyPK, Guid.Empty, Guid.Empty, "Company 1");
			registryItem2.SetValue(companyPK, Guid.Empty, Guid.Empty, "Company 2");

			AssertEquals("RegistryItem1.GetValue()", "Company 1", registryItem1.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			AssertEquals("RegistryItem2.GetValue()", "Company 2", registryItem2.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));

			// Company Department Level
			AssertEquals("RegistryItem1.GetValue()", "Apple Pie", registryItem1.GetValueWithoutFallback(companyPK, Guid.Empty, departmentPK));
			AssertEquals("RegistryItem2.GetValue()", companyPK.ToString() + Guid.Empty.ToString() + departmentPK.ToString(), registryItem2.GetValueWithoutFallback(companyPK, Guid.Empty, departmentPK));

			registryItem1.SetValue(companyPK, Guid.Empty, departmentPK, "Company Department 1");
			registryItem2.SetValue(companyPK, Guid.Empty, departmentPK, "Company Department 2");

			AssertEquals("RegistryItem1.GetValue()", "Company Department 1", registryItem1.GetValueWithoutFallback(companyPK, Guid.Empty, departmentPK));
			AssertEquals("RegistryItem2.GetValue()", "Company Department 2", registryItem2.GetValueWithoutFallback(companyPK, Guid.Empty, departmentPK));

			// Branch Level
			AssertEquals("RegistryItem1.GetValue()", "Apple Pie", registryItem1.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty));
			AssertEquals("RegistryItem2.GetValue()", Guid.Empty.ToString() + branchPK.ToString() + Guid.Empty.ToString(), registryItem2.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty));

			registryItem1.SetValue(Guid.Empty, branchPK, Guid.Empty, "Branch 1");
			registryItem2.SetValue(Guid.Empty, branchPK, Guid.Empty, "Branch 2");

			AssertEquals("RegistryItem1.GetValue()", "Branch 1", registryItem1.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty));
			AssertEquals("RegistryItem2.GetValue()", "Branch 2", registryItem2.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty));

			// Branch Department Level
			AssertEquals("RegistryItem1.GetValue()", "Apple Pie", registryItem1.GetValueWithoutFallback(Guid.Empty, branchPK, departmentPK));
			AssertEquals("RegistryItem2.GetValue()", Guid.Empty.ToString() + branchPK.ToString() + departmentPK.ToString(), registryItem2.GetValueWithoutFallback(Guid.Empty, branchPK, departmentPK));

			registryItem1.SetValue(Guid.Empty, branchPK, departmentPK, "Branch Department 1");
			registryItem2.SetValue(Guid.Empty, branchPK, departmentPK, "Branch Department 2");

			AssertEquals("RegistryItem1.GetValue()", "Branch Department 1", registryItem1.GetValueWithoutFallback(Guid.Empty, branchPK, departmentPK));
			AssertEquals("RegistryItem2.GetValue()", "Branch Department 2", registryItem2.GetValueWithoutFallback(Guid.Empty, branchPK, departmentPK));
		}

		public void TestGetDefaultValue()
		{
			var registryItem = new RegistryItemImpl("RegistryItemName", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.All, "DefaultValue");
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertEquals("Return the default value if the fallback value is null", "DefaultValue", registryItem.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestDeleteRecord()
		{
			string keyName = Guid.NewGuid().ToString();
			string testValue = "Apple Pie";
			byte[] testValueAsBytes = Encoding.Unicode.GetBytes(testValue);

			var registryItem = new RegistryItemImpl(keyName, null, null, null, new StringRegistryDataType(), RegistryStorageFlags.All, string.Empty);
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				AssertEquals("accessor.GetBinaryValue(keyName, ownerPK, Guid.Empty)", null, registryDataAccessor.GetBinaryValue(keyName, Guid.Empty, Guid.Empty));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testValue);
				AssertEquals("accessor.GetBinaryValue(keyName, ownerPK, Guid.Empty)", testValueAsBytes, registryDataAccessor.GetBinaryValue(keyName, Guid.Empty, Guid.Empty));

				((IRegistryItemInternals)registryItem).DeleteRecord(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("accessor.GetBinaryValue(keyName, ownerPK, Guid.Empty)", null, registryDataAccessor.GetBinaryValue(keyName, Guid.Empty, Guid.Empty));
			}
		}

		public void TestSetValueUsesCustomDefaultValueToValidate()
		{
			TestRegistryDataType dataType1 = new TestRegistryDataType();
			TestRegistryDataType dataType2 = new TestRegistryDataType();

			RegistryItemImpl registryItem1 = new RegistryItemImpl("ApplePie", null, null, null, dataType1, RegistryStorageFlags.All, "Apple Pie");
			DummyRegistryItemImpl registryItem2 = new DummyRegistryItemImpl("BlueberryPie", "", "", "", dataType2, RegistryStorageFlags.All, "Blueberry Pie");

			AssertEquals("Precondition: DataType1.Validate() should not be called yet.", false, dataType1.IsValidateCalled);
			AssertEquals("Precondition: DataType2.Validate() should not be called yet.", false, dataType2.IsValidateCalled);

			Guid companyPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			registryItem1.SetValue(companyPK, Guid.Empty, departmentPK, "Apple Pie");
			registryItem2.SetValue(companyPK, Guid.Empty, departmentPK, companyPK.ToString() + Guid.Empty.ToString() + departmentPK.ToString());

			AssertEquals("DataType1.Validate() should not be called.", false, dataType1.IsValidateCalled);
			AssertEquals("DataType2.Validate() should not be called.", false, dataType2.IsValidateCalled);

			registryItem1.SetValue(companyPK, Guid.Empty, departmentPK, "Orange Pie");
			registryItem2.SetValue(companyPK, Guid.Empty, departmentPK, "Blueberry Pie");

			AssertEquals("DataType1.Validate() should be called.", true, dataType1.IsValidateCalled);
			AssertEquals("DataType2.Validate() should be called.", true, dataType2.IsValidateCalled);

			dataType1.IsValidatedOnSetEvenIfEqualDefaultValue = true;
			dataType2.IsValidatedOnSetEvenIfEqualDefaultValue = true;

			dataType1.IsValidateCalled = false;
			dataType2.IsValidateCalled = false;

			registryItem1.SetValue(companyPK, Guid.Empty, departmentPK, "Apple Pie");
			registryItem2.SetValue(companyPK, Guid.Empty, departmentPK, companyPK.ToString() + Guid.Empty.ToString() + departmentPK.ToString());

			AssertEquals("DataType1.Validate() should not be called.", true, dataType1.IsValidateCalled);
			AssertEquals("DataType2.Validate() should not be called.", true, dataType2.IsValidateCalled);
		}

		public void TestRetrieverObtainedWhenGettingValue()
		{
			DummyRegistryItemImpl registryItem1 = new DummyRegistryItemImpl("", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.System);
			DummyRegistryItemImpl registryItem2 = new DummyRegistryItemImpl("", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.All);

			object x = registryItem1.Value;
			object y = registryItem2.Value;

			AssertEquals("RegistryItem1.LastObtainedRetriever.CompanyPK", Guid.Empty, registryItem1.LastObtainedRetriever.CompanyPK);
			AssertEquals("RegistryItem1.LastObtainedRetriever.BranchPK", Guid.Empty, registryItem1.LastObtainedRetriever.BranchPK);
			AssertEquals("RegistryItem1.LastObtainedRetriever.DepartmentPK", Guid.Empty, registryItem1.LastObtainedRetriever.DepartmentPK);
			AssertEquals("RegistryItem1.LastObtainedRetriever.Level", RegistryStorageFlags.System, registryItem1.LastObtainedRetriever.Level);

			AssertEquals("RegistryItem2.LastObtainedRetriever.CompanyPK", EnvProxy.Instance.CurrentCompany.PK, registryItem2.LastObtainedRetriever.CompanyPK);
			AssertEquals("RegistryItem2.LastObtainedRetriever.BranchPK", EnvProxy.Instance.CurrentBranch.PK, registryItem2.LastObtainedRetriever.BranchPK);
			AssertEquals("RegistryItem2.LastObtainedRetriever.DepartmentPK", EnvProxy.Instance.CurrentDepartment.PK, registryItem2.LastObtainedRetriever.DepartmentPK);
			AssertEquals("RegistryItem2.LastObtainedRetriever.Level", RegistryStorageFlags.BranchDepartment, registryItem2.LastObtainedRetriever.Level);
		}

		public void TestIsValueMandatory()
		{
			IRegistryItem item = new DummyRegistryItemImpl("", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.System);
			AssertEquals("IsValueMandatory", false, item.IsValueMandatory);

			item.Options = RegistryOptions.IsValueMandatory;
			AssertEquals("IsValueMandatory", true, item.IsValueMandatory);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestCannotCallParameterlessValueGetter()
		{
			IRegistryItem item = new DummyRegistryItemImpl("", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.System);
			item.Options |= RegistryOptions.CannotCallParameterlessValueGetter;
			object value = item.Value;
		}

		public void TestInvalidXmlExceptionThrown()
		{
			ExceptionReporterTestListener.Instance.Clear();
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
				{
					registryDataAccessor.SetBinaryValue("---", Guid.Empty, Guid.Empty, Encoding.Unicode.GetBytes("invalid"), Guid.Empty, "", false, false);
				}

				var item = new RegistryItemImpl("---", null, null, null, new CodeDescriptionPairListRegistryDataType(3), RegistryStorageFlags.System, new CodeDescriptionPairList(OLookUpEditType.Gender));
				AssertExceptionThrown(typeof(XmlException), () => item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region TestInvalidOperationErrorReported()

		public void TestInvalidOperationExceptionThrown()
		{
			ExceptionReporterTestListener.Instance.Clear();
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
				{
					registryDataAccessor.SetBinaryValue("---", Guid.Empty, Guid.Empty, new byte[] { 4, 5, 6 }, Guid.Empty, "", false, false);
				}

				var item = new RegistryItemImpl("---", null, null, null, new DodgyRegistryDataType(new byte[] { 0, 1, 2 }), RegistryStorageFlags.System);
				AssertExceptionThrown(typeof(InvalidOperationException), () => item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestInvalidXmlExceptionThrownOnUserInteractiveEnv()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
				{
					registryDataAccessor.SetBinaryValue("TestRegistryItem", Guid.Empty, Guid.Empty, Encoding.Unicode.GetBytes("invalid"), Guid.Empty, "", false, false);
				}

				var defaultValue = new CodeDescriptionPairList(OLookUpEditType.Gender);
				var item = new RegistryItemImpl("TestRegistryItem", null, null, null, new CodeDescriptionPairListRegistryDataType(3), RegistryStorageFlags.System, defaultValue);
				var itemValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				var message = UnitTestUserNotification.Instance.LastMessage;

				AssertEquals("Default value returned", defaultValue, itemValue);
				AssertEquals("The pop up message should have Error as caption", "Error", message.Caption);
				AssertContains("The pop up message should have TestRegistryItem in text", "TestRegistryItem", message.Text);

				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestInvalidOperationExceptionThrownOnUserInteractiveEnv()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
				{
					registryDataAccessor.SetBinaryValue("TestRegistryItem", Guid.Empty, Guid.Empty, new byte[] { 4, 5, 6 }, Guid.Empty, "", false, false);
				}

				var item = new RegistryItemImpl("TestRegistryItem", null, null, null, new DodgyRegistryDataType(Encoding.Unicode.GetBytes("Default Value")), RegistryStorageFlags.System);
				var itemValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				var message = UnitTestUserNotification.Instance.LastMessage;

				AssertEquals("Default value returned", "Default Value", Encoding.Unicode.GetString((byte[])itemValue));
				AssertEquals("The pop up message should have Error as caption", "Error", message.Caption);
				AssertContains("The pop up message should have TestRegistryItem in text", "TestRegistryItem", message.Text);

				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		class DodgyRegistryDataType : RegistryDataType<byte[]>
		{
			public DodgyRegistryDataType(byte[] defaultValue)
				: base(RegistryDataTypes.Codes.Binary, defaultValue)
			{
			}

			protected override byte[] SerialiseCore(byte[] value)
			{
				throw new NotImplementedException();
			}

			protected override byte[] DeserialiseCore(byte[] value)
			{
				throw new InvalidOperationException();
			}

			public override bool IsDefaultValueImmutable => false;
		}

		#endregion

		public void TestDefaultValueWithNoEnv()
		{
			IRegistryItem item = new RegistryItemImpl("", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.System, "default");
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				AssertEquals("default", item.DefaultValue);
			}
		}

		public void TestCheckValueDataType()
		{
			const int intValue = 100;
			const string stringValue = "testValue";

			var registryItemString = new DummyRegistryItemImpl("TestRegistryItem", "TestCategory", "TestCaption", "TestHint", new StringRegistryDataType(), RegistryStorageFlags.System);
			AssertExceptionThrown(typeof(ArgumentException),
				"Input value data type is incorrect, it must be System.String. Name = TestRegistryItem, Category = TestCategory, Caption = TestCaption, DataType = Enterprise.ZArchitecture.Environment.StringRegistryDataType, DataType.DataType = System.String, Value = 100, Value.GetType() = System.Int32.",
				() => registryItemString.CheckValueDataType(intValue));

			var registryItemDecimal = new DummyRegistryItemImpl("TestRegistryItem2", "TestCategory2", "TestCaption2", "TestHint2", new DecimalRegistryDataType(), RegistryStorageFlags.System);
			AssertExceptionThrown(typeof(ArgumentException),
				"Input value data type is incorrect, it must be System.Decimal. Name = TestRegistryItem2, Category = TestCategory2, Caption = TestCaption2, DataType = Enterprise.ZArchitecture.Environment.DecimalRegistryDataType, DataType.DataType = System.Decimal, Value = testValue, Value.GetType() = System.String.",
				() => registryItemDecimal.CheckValueDataType(stringValue));

			var registryItemInt = new DummyRegistryItemImpl("TestRegistryItem3", "TestCategory3", "TestCaption3", "TestHint3", new IntRegistryDataType(), RegistryStorageFlags.System);
			AssertNoExceptionThrown(() => registryItemInt.CheckValueDataType(intValue));
		}

		#region Has Value for any Level

		public void TestHasValueForAnyLevel()
		{
			AssertHasValueForAnyLevel(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertHasValueForAnyLevel(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, true);
			AssertHasValueForAnyLevel(Guid.Empty, Guid.Empty, Guid.NewGuid(), false);
			AssertHasValueForAnyLevel(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertHasValueForAnyLevel(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, true);
			AssertHasValueForAnyLevel(Guid.NewGuid(), Guid.Empty, Guid.Empty, false);
			AssertHasValueForAnyLevel(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.NewGuid(), false);
			AssertHasValueForAnyLevel(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, true);
			AssertHasValueForAnyLevel(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, true);
			AssertHasValueForAnyLevel(Guid.Empty, Guid.NewGuid(), EnvProxy.Instance.CurrentDepartment.PK, false);
			AssertHasValueForAnyLevel(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.NewGuid(), false);
		}

		void AssertHasValueForAnyLevel(Guid companyPK, Guid branchPK, Guid departmentPK, bool expectedHasValueForAnyLevel)
		{
			DummyRegistryItemImpl item = new DummyRegistryItemImpl("HasValueForAnyLevel", "", "", "", new StringRegistryDataType(), RegistryStorageFlags.All);
			IRegistryItemInternals itemInternals = item;
			item.SetValue(companyPK, branchPK, departmentPK, "");
			AssertEquals("HasValueForAnyLevel()", expectedHasValueForAnyLevel, itemInternals.HasValueForAnyLevel());
			itemInternals.DeleteValue(companyPK, branchPK, departmentPK);
		}

		#endregion

		#region TestGetRegistryItemPK

		public void TestGetRegistryItemPK()
		{
			Guid companyPk = Guid.NewGuid();
			Guid branchPk = Guid.NewGuid();
			Guid departmentPk = Guid.NewGuid();

			RegistryItemImpl item = new RegistryItemImpl("RegItem1", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.All);
			AssertEquals("There is no value in db yet", Guid.Empty, ((IRegistryItemInternals)item).GetRegistryItemPK(companyPk, Guid.Empty, departmentPk));
			AssertEquals("There is no value in db yet", Guid.Empty, ((IRegistryItemInternals)item).GetRegistryItemPK(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(companyPk, Guid.Empty, departmentPk, null);
			Guid itemPk = ((IRegistryItemInternals)item).GetRegistryItemPK(companyPk, Guid.Empty, departmentPk);
			AssertNotEquals("Registry item PK should not be empty", Guid.Empty, itemPk);
			AssertEquals("There is no value for specified fallback level", Guid.Empty, ((IRegistryItemInternals)item).GetRegistryItemPK(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("There is no value for specified fallback level", Guid.Empty, ((IRegistryItemInternals)item).GetRegistryItemPK(Guid.Empty, Guid.Empty, departmentPk));
			AssertEquals("There is no value for specified fallback level", Guid.Empty, ((IRegistryItemInternals)item).GetRegistryItemPK(companyPk, Guid.Empty, Guid.Empty));
			AssertEquals("There is no value for specified fallback level", Guid.Empty, ((IRegistryItemInternals)item).GetRegistryItemPK(Guid.Empty, branchPk, Guid.Empty));
			AssertEquals("There is no value for specified fallback level", Guid.Empty, ((IRegistryItemInternals)item).GetRegistryItemPK(Guid.Empty, branchPk, departmentPk));

			item.SetValue(companyPk, Guid.Empty, departmentPk, "abracadabra");
			AssertEquals("Registry item PK should be permanent", itemPk, ((IRegistryItemInternals)item).GetRegistryItemPK(companyPk, Guid.Empty, departmentPk));

			RegistryItemImpl item2 = new RegistryItemImpl("RegItem1", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.All);
			AssertEquals("Registry item PK should be permanent", itemPk, ((IRegistryItemInternals)item2).GetRegistryItemPK(companyPk, Guid.Empty, departmentPk));
		}

		#endregion

		#region Preserve Test Data

		public void TestPreserveTestValue()
		{
			IRegistryItem item = new RegistryItemImpl("", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, "default");
			Assert("default", item.Options.HasFlag(RegistryOptions.PreserveTestValue));
		}

		#endregion

		#region AuditDetails

		[ExpectNoExceptions]
		public void TestAuditDetails()
		{
			var environmentMock = new Mock<IEnvironment>();
			var envMock = new Mock<IEnv>();

			var companyMock = new Mock<ICompany>();
			companyMock.Setup(x => x.PK).Returns(CompanyPK);
			environmentMock.Setup(x => x.CurrentCompany).Returns(companyMock.Object);

			var branchMock = new Mock<IBranch>();
			branchMock.Setup(x => x.PK).Returns(BranchPK);
			environmentMock.Setup(x => x.CurrentBranch).Returns(branchMock.Object);

			var departmentMock = new Mock<IDepartment>();
			departmentMock.Setup(x => x.PK).Returns(DepartmentPK);
			environmentMock.Setup(x => x.CurrentDepartment).Returns(departmentMock.Object);

			var userMock = new Mock<IUser>();
			userMock.Setup(x => x.Initials).Returns("~BP");
			environmentMock.Setup(x => x.CurrentUser).Returns(userMock.Object);

			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				var registryItem = new RegistryItemImpl("AUDITTEST1", null, null, null, new StringRegistryDataType(), RegistryStorageFlags.All, "Apple Pie");

				// System Level
				AssertAuditDetails(registryItem, Guid.Empty, Guid.Empty, Guid.Empty);

				// System Department Level
				AssertAuditDetails(registryItem, Guid.Empty, Guid.Empty, DepartmentPK);

				// Company Level
				AssertAuditDetails(registryItem, CompanyPK, Guid.Empty, Guid.Empty);

				// Company Department Level
				AssertAuditDetails(registryItem, CompanyPK, Guid.Empty, DepartmentPK);

				// Branch Level
				AssertAuditDetails(registryItem, Guid.Empty, BranchPK, Guid.Empty);

				// Branch Level
				AssertAuditDetails(registryItem, Guid.Empty, BranchPK, DepartmentPK);
			}
		}

		void AssertAuditDetails(RegistryItemImpl item, Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			item.SetValue(companyPk, branchPk, departmentPk, companyPk.ToString() + branchPk.ToString() + departmentPk.ToString());
			Assert("SystemCreateTimeUtc", (ZDateTime.Now - item.SystemCreateTimeUtc).TotalDays < 1);
			Assert("SystemLastEditTimeUtc", (ZDateTime.Now - item.SystemLastEditTimeUtc).TotalDays < 1);
			AssertEquals("SystemCreateUser", "~BP", item.SystemCreateUser);
			AssertEquals("SystemLastEditUser", "~BP", item.SystemLastEditUser);

			((IRegistryItemInternals)item).DeleteRecord(companyPk, branchPk, departmentPk);
			AssertEquals("SystemCreateTimeUtc", ZDateTime.Empty, item.SystemCreateTimeUtc);
			AssertEquals("SystemLastEditTimeUtc", ZDateTime.Empty, item.SystemLastEditTimeUtc);
			AssertEquals("SystemLastEditTimeUtc", string.Empty, item.SystemCreateUser);
			AssertEquals("SystemLastEditTimeUtc", string.Empty, item.SystemLastEditUser);
		}

		#endregion

		#region Registry Option CacheExpensiveDefaultValue

		public void TestRegistryOption_CacheExpensiveDefaultValue_DoesNotCacheWhenNotSet()
		{
			var uncachedItem = new RegistryItemImpl_ForExpensiveDefaultValueTest("UncachedRegistry", options: RegistryOptions.Default);
			AssertEquals("Precondition", 0, uncachedItem.CountOfGetDefaultValueCoreCalls);

			var value = uncachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CombineAssertions("Getting non-overridden registry value should evaluate the default value", () =>
			{
				AssertEquals("Value", "TheDefault", value);
				AssertEquals("CountOfGetDefaultValueCoreCalls", 1, uncachedItem.CountOfGetDefaultValueCoreCalls);
			});

			var value2 = uncachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CombineAssertions("Getting uncached, non-overridden registry value should evaluate the default value each time", () =>
			{
				AssertEquals("Value", "TheDefault", value2);
				AssertEquals("CountOfGetDefaultValueCoreCalls", 2, uncachedItem.CountOfGetDefaultValueCoreCalls);
			});

			var defaultValue = uncachedItem.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty);
			CombineAssertions("GetDefaultValue() should evaluate the default value, no matter what caching is configured", () =>
			{
				AssertEquals("Value", "TheDefault", defaultValue);
				AssertEquals("CountOfGetDefaultValueCoreCalls", 3, uncachedItem.CountOfGetDefaultValueCoreCalls);
			});

			using (uncachedItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Other Value"))
			{
				AssertEquals("Setting a registry value evaluates the default twice. (This is implementation specific and subject to change)", 5, uncachedItem.CountOfGetDefaultValueCoreCalls);

				var overriddenValue = uncachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				CombineAssertions("Getting overridden registry value should not evaluate the default value", () =>
				{
					AssertEquals("Value", "Some Other Value", overriddenValue);
					AssertEquals("CountOfGetDefaultValueCoreCalls", 5, uncachedItem.CountOfGetDefaultValueCoreCalls);
				});
			}
		}

		public void TestRegistryOption_CacheExpensiveDefaultValue_DoesCacheWhenSet()
		{
			var cachedItem = new RegistryItemImpl_ForExpensiveDefaultValueTest("CachedRegistry", options: RegistryOptions.CacheExpensiveDefaultValue);
			AssertEquals("Precondition", 0, cachedItem.CountOfGetDefaultValueCoreCalls);

			var value = cachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CombineAssertions("Getting non-overridden registry value should evaluate the default value", () =>
			{
				AssertEquals("Value", "TheDefault", value);
				AssertEquals("CountOfGetDefaultValueCoreCalls", 1, cachedItem.CountOfGetDefaultValueCoreCalls);
			});

			var value2 = cachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CombineAssertions("Getting cached, non-overridden registry value should only evaluate the default value once", () =>
			{
				AssertEquals("Value", "TheDefault", value2);
				AssertEquals("CountOfGetDefaultValueCoreCalls", 1, cachedItem.CountOfGetDefaultValueCoreCalls);
			});

			var defaultValue = cachedItem.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty);
			CombineAssertions("GetDefaultValue() should evaluate the default value, no matter what caching is configured", () =>
			{
				AssertEquals("Value", "TheDefault", defaultValue);
				AssertEquals("CountOfGetDefaultValueCoreCalls", 2, cachedItem.CountOfGetDefaultValueCoreCalls);
			});

			((IRegistryItemInternals)cachedItem).ClearCache();
			var value3 = cachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CombineAssertions("Clearing the cache should trigger an evaluation of the default value", () =>
			{
				AssertEquals("Value", "TheDefault", value3);
				AssertEquals("CountOfGetDefaultValueCoreCalls", 3, cachedItem.CountOfGetDefaultValueCoreCalls);
			});

			using (cachedItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Other Value"))
			{
				AssertEquals("Setting a registry value evaluates the default once. (This is implementation specific and subject to change)", 4, cachedItem.CountOfGetDefaultValueCoreCalls);

				var overriddenValue = cachedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				CombineAssertions("Getting overridden registry value should not evaluate the default value", () =>
				{
					AssertEquals("Value", "Some Other Value", overriddenValue);
					AssertEquals("CountOfGetDefaultValueCoreCalls", 4, cachedItem.CountOfGetDefaultValueCoreCalls);
				});
			}
		}

		public void TestRegistryOption_CacheExpensiveDefaultValue_CachesPerCompanyWhenSet()
		{
			var cachedItemForCompany = new RegistryItemImpl_ForExpensiveDefaultValueTest("CachedCompanyRegistry", storage: RegistryStorageFlags.Company, options: RegistryOptions.CacheExpensiveDefaultValue);
			AssertEquals("Precondition", 0, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			var company1 = Guid.NewGuid();
			var company2 = Guid.NewGuid();

			_ = cachedItemForCompany.GetValueWithoutFallback(company1, Guid.Empty, Guid.Empty);
			AssertEquals("Getting non-overridden registry value should evaluate the default value", 1, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			_ = cachedItemForCompany.GetValueWithoutFallback(company1, Guid.Empty, Guid.Empty);
			AssertEquals("Getting cached, non-overridden registry value should not evaluate the default value", 1, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			_ = cachedItemForCompany.GetValueWithoutFallback(company2, Guid.Empty, Guid.Empty);
			AssertEquals("Getting non-overridden registry value for different company should evaluate the default value", 2, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			_ = cachedItemForCompany.GetValueWithoutFallback(company2, Guid.Empty, Guid.Empty);
			AssertEquals("Getting cached, non-overridden registry value for different company should not evaluate the default value", 2, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			_ = cachedItemForCompany.GetDefaultValue(company1, Guid.Empty, Guid.Empty);
			AssertEquals("GetDefaultValue() should evaluate the default value, no matter what caching is configured", 3, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			_ = cachedItemForCompany.GetDefaultValue(company2, Guid.Empty, Guid.Empty);
			AssertEquals("GetDefaultValue() for different company should evaluate the default value, no matter what caching is configured", 4, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			((IRegistryItemInternals)cachedItemForCompany).ClearCache();
			_ = cachedItemForCompany.GetValueWithoutFallback(company1, Guid.Empty, Guid.Empty);
			AssertEquals("Clearing the cache should trigger an evaluation of the default value", 5, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);

			_ = cachedItemForCompany.GetValueWithoutFallback(company2, Guid.Empty, Guid.Empty);
			AssertEquals("Clearing the cache should trigger an evaluation of the default value for a different company", 6, cachedItemForCompany.CountOfGetDefaultValueCoreCalls);
		}

		class RegistryItemImpl_ForExpensiveDefaultValueTest : RegistryItemImpl
		{
			public RegistryItemImpl_ForExpensiveDefaultValueTest(string name, RegistryStorageFlags storage = RegistryStorageFlags.System, RegistryOptions options = RegistryOptions.Default)
				: base(name, null, null, null, new StringRegistryDataType(), storage, options, "TheDefault")
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				++CountOfGetDefaultValueCoreCalls;
				return base.GetDefaultValueCore(companyPK, branchPK, departmentPK);
			}

			public int CountOfGetDefaultValueCoreCalls;
		}

		#endregion

		#region Implementation

		readonly Guid CompanyPK = Guid.NewGuid();
		readonly Guid DepartmentPK = Guid.NewGuid();
		readonly Guid BranchPK = Guid.NewGuid();

		#region class TestRegistryDataType

		class TestRegistryDataType : StringRegistryDataType
		{
			public override bool IsDefaultValueImmutable => false;

			public new bool IsValidatedOnSetEvenIfEqualDefaultValue
			{
				get { return base.IsValidatedOnSetEvenIfEqualDefaultValue; }
				set { isValidatedOnSetEvenIfEqualDefaultValue = value; }
			}

			public bool IsTestingClone
			{
				get { return isTestingClone; }
				set { isTestingClone = value; }
			}

			public bool IsValidateCalled
			{
				get { return isValidateCalled; }
				set { isValidateCalled = value; }
			}

			protected override bool IsFallBackMergeValuesImplementedCore
			{
				get { return true; }
			}

			protected override string FallBackMergeValuesCore(string fallBackValue, string value)
			{
				return fallBackValue + "|" + value;
			}

			protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
				isValidateCalled = true;
			}

			protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore
			{
				get { return isValidatedOnSetEvenIfEqualDefaultValue; }
			}

			protected override string CloneValue(string value)
			{
				return (IsTestingClone) ? "Cloned " + value : base.CloneValue(value);
			}

			bool isTestingClone;
			bool isValidateCalled;
			bool isValidatedOnSetEvenIfEqualDefaultValue;
		}

		#endregion

		#region class MockRegistryItemWithOtherCountry

		class MockRegistryItemWithOtherCountry : RegistryItemImpl
		{
			public MockRegistryItemWithOtherCountry(string name, string category, string caption, string hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, (NoResString)category, (NoResString)caption, (NoResString)hint, dataType, storage)
			{
			}

			public override IEnumerable<Guid> CountryFilterPKs
			{
				get { return new[] { Guid.NewGuid() }; }
			}
		}

		#endregion

		protected override void TearDown()
		{
			EnvProxy.SetHostedLocationForTest(null);
			base.TearDown();
		}
		#endregion
	}

	#region class DummyRegistryItemImpl

	public class DummyRegistryItemImpl : RegistryItemImpl
	{
		public DummyRegistryItemImpl(string name, string category, string caption, string hint, IRegistryDataType dataType, RegistryStorageFlags storage)
			: base(name, (NoResString)category, (NoResString)caption, (NoResString)hint, dataType, storage)
		{
		}

		public DummyRegistryItemImpl(string name, string category, string caption, string hint, IRegistryDataType dataType, RegistryStorageFlags storage, object defaultValue)
			: base(name, (NoResString)category, (NoResString)caption, (NoResString)hint, dataType, storage, defaultValue)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return companyPK.ToString() + branchPK.ToString() + departmentPK.ToString();
		}

		protected override RegistryItemFallBackValueAccessor GetRetriever()
		{
			LastObtainedRetriever = base.GetRetriever();
			return LastObtainedRetriever;
		}

		public RegistryItemFallBackValueAccessor LastObtainedRetriever;
	}

	#endregion
}
