using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Testing
{
	[TestedType(typeof(JASDataRegistry))]
	public class JASDataRegistryTest : RegistryItemSetTestCase<JASDataRegistry>
	{
		public void TestRegistryItems()
		{
			AssertEquals(25, AllItems.Count);
			AssertVisible(JXCOutgoingDirectoryNameItem);
			AssertVisible(JXCIncomingDirectoryNamesItem);
			AssertVisible(JXCMessagesArchiveDirectoryNameItem);
			AssertVisible(EnableAutoJXCMessagingItem);
			AssertVisible(QueryUserForDirectoryOnManualJXCExportItem);
			AssertVisible(ItemSet.DefaultChargeCodeForAccrualsImportItem);
			AssertVisible(EnableAutoUpdateOnImportItem);
			AssertVisible(ProfitSplitDueDestinationPercentageItem);
			AssertVisible(DisableGsumMessagingItem, true);
			AssertVisible(JXCAirMessagingNotificationGroupItem);
			AssertVisible(JXCOceanMessagingNotificationGroupItem);
			AssertVisible(JXCFinancialMessagingNotificationGroupItem);
			AssertVisible(JXCMiscMessagingNotificationGroupItem);
			AssertVisible(JXCGeneralNotificationGroupItem);
			AssertVisible(AccountingNotificationGroupItem);
			AssertVisible(JASWWOrganisationItem);
			AssertVisible(ItemSet.NettingCycleListItem);
			AssertVisible(ItemSet.NettingPaymentTermsItem);
			AssertVisible(ItemSet.CognosModeMappingItem);
			AssertVisible(CognosAdminGroupItem);
			AssertVisible(CognosSalesGroupItem);
			AssertVisible(CognosShipmentControlGroupItem);
			AssertVisible(IncludeProfitShareChargesItem);
			AssertVisible(BOLBondNameItem);
		}

		internal static void SetJASWWOrganisationItemForTest(BusinessObjectFactory factory)
		{
			OrgHeader newOrg = factory.NewWithValidTestData<OrgHeader>();
			JASDataRegistry.Instance.JASWWOrganisationItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newOrg.PK.ToGuid());
		}

		internal static void UnsetJASWWOrganisationItemForTest()
		{
			JASDataRegistry.Instance.JASWWOrganisationItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#region JXC
		public void TestJXCIncomingDirectoryNames()
		{
			string expected = string.Concat(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), @"\JXC\");
			AssertEquals("Check Default", expected, ItemSet.JXCIncomingDirectoryNames);
			ItemSet.JXCIncomingDirectoryNames = "d:\\Incoming\\456";
			AssertEquals("d:\\Incoming\\456", ItemSet.JXCIncomingDirectoryNames);
		}

		public void TestJXCIncomingDirectoryNamesItem()
		{
			AssertEquals("JXCIncomingDirectoryNames", JXCIncomingDirectoryNamesItem.Name);
			AssertEquals("Incoming JXC Messages Directories", JXCIncomingDirectoryNamesItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCIncomingDirectoryNamesItem.Storage);
			AssertEquals(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86) + "\\JXC\\", JXCIncomingDirectoryNamesItem.Value);
			AssertEquals(typeof(TextRegistryEditorInfo), JXCIncomingDirectoryNamesItem.EditorInfo.GetType());
			AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)JXCIncomingDirectoryNamesItem.EditorInfo).EditorType);
			AssertEquals("Please enter the parent folder of the incoming messages directory. You could specify more than one directory by entering multiple lines.", JXCIncomingDirectoryNamesItem.Hint);
			AssertEquals(ExpectedJXCCategory, JXCIncomingDirectoryNamesItem.Category);
		}

		public void TestJXCOutgoingDirectoryName()
		{
			string expected = string.Concat(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), @"\JXC\JCVI\");
			AssertEquals("Check Default", expected, ItemSet.JXCOutgoingDirectoryName);
			ItemSet.JXCOutgoingDirectoryName = "d:\\Testing\\123";
			AssertEquals("d:\\Testing\\123", ItemSet.JXCOutgoingDirectoryName);
		}

		public void TestJXCOutgoingDirectoryNameItem()
		{
			AssertEquals("JXCOutgoingDirectoryName", JXCOutgoingDirectoryNameItem.Name);
			AssertEquals("Outgoing JXC Messages Dump Directory", JXCOutgoingDirectoryNameItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCOutgoingDirectoryNameItem.Storage);
			AssertEquals(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86) + "\\JXC\\JCVI\\", JXCOutgoingDirectoryNameItem.Value);
			AssertEquals(ExpectedJXCCategory, JXCOutgoingDirectoryNameItem.Category);
		}

		public void TestBOLBondNameItem()
		{
			AssertEquals("BOLBondNum", BOLBondNameItem.Name);
			AssertEquals("BOND Number", BOLBondNameItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, BOLBondNameItem.Storage);
			AssertEquals("Hint", @"The text entered in this field will print on the document after “BILL OF LADING”", BOLBondNameItem.Hint);
			AssertEquals(ExpectedBOLCategory, BOLBondNameItem.Category);
			AssertEquals("Default value", "- BOND # 990404", BOLBondNameItem.Value);
		}

		public void TestJXCMessagesArchiveDirectoryName()
		{
			AssertEquals("", ItemSet.JXCMessagesArchiveDirectoryName);
			ItemSet.JXCMessagesArchiveDirectoryName = "d:\\TotallyWrecked";
			AssertEquals("d:\\TotallyWrecked", ItemSet.JXCMessagesArchiveDirectoryName);
		}

		public void TestJXCMessagesArchiveDirectoryNameItem()
		{
			AssertEquals("JXCMessagesArchiveDirectoryName", JXCMessagesArchiveDirectoryNameItem.Name);
			AssertEquals("JXC Messages Archive Directory", JXCMessagesArchiveDirectoryNameItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCMessagesArchiveDirectoryNameItem.Storage);
			AssertEquals("Outgoing and incoming JXC messages are archived into this directory", JXCMessagesArchiveDirectoryNameItem.Hint);
			AssertEquals("", JXCMessagesArchiveDirectoryNameItem.DefaultValue);
			AssertEquals(ExpectedJXCCategory, JXCMessagesArchiveDirectoryNameItem.Category);
		}

		public void TestEnableAutoJXCMessaging()
		{
			Assert("Default should be true", ItemSet.EnableAutoJXCMessaging);
			ItemSet.EnableAutoJXCMessaging = false;
			Assert(!ItemSet.EnableAutoJXCMessaging);
		}

		public void TestEnableAutoJXCMessagingItem()
		{
			AssertEquals("EnableAutoJXCMessaging", EnableAutoJXCMessagingItem.Name);
			AssertEquals("Enable Automatic JXC Messaging", EnableAutoJXCMessagingItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, EnableAutoJXCMessagingItem.Storage);
			AssertEquals("This enables automatic exporting of JXC messages when certain events occurred (I.e. Printing of MAWB)", EnableAutoJXCMessagingItem.Hint);
			AssertEquals(true, EnableAutoJXCMessagingItem.DefaultValue);
			AssertEquals(ExpectedJXCCategory, EnableAutoJXCMessagingItem.Category);
		}

		public void TestQueryUserForDirectoryOnManualJXCExport()
		{
			Assert("Default should be true", ItemSet.QueryUserForDirectoryOnManualJXCExport);
			ItemSet.QueryUserForDirectoryOnManualJXCExport = false;
			Assert(!ItemSet.QueryUserForDirectoryOnManualJXCExport);
		}

		public void TestQueryUserForDirectoryOnManualJXCExportItem()
		{
			AssertEquals("QueryUserForDirectoryOnManualJXCExport", QueryUserForDirectoryOnManualJXCExportItem.Name);
			AssertEquals("Query User For Directory On Manual JXC Export", QueryUserForDirectoryOnManualJXCExportItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, QueryUserForDirectoryOnManualJXCExportItem.Storage);
			AssertEquals("This enables user to select a directory where the JXC message is going to be exported to. If this is disabled, export files will go to the default export directory.", QueryUserForDirectoryOnManualJXCExportItem.Hint);
			AssertEquals(true, QueryUserForDirectoryOnManualJXCExportItem.DefaultValue);
			AssertEquals(ExpectedJXCCategory, QueryUserForDirectoryOnManualJXCExportItem.Category);
		}

		public void TestDefaultChargeCodeForAccrualsImportItem()
		{
			AssertEquals("DefaultChargeCodeForAccrualsImport", ItemSet.DefaultChargeCodeForAccrualsImportItem.Name);
			AssertEquals("Default Charge Code for Accruals Import", ItemSet.DefaultChargeCodeForAccrualsImportItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.DefaultChargeCodeForAccrualsImportItem.Storage);
			AssertEquals("Please enter a default charge code for importing accruals when charge code cannot be matched", ItemSet.DefaultChargeCodeForAccrualsImportItem.Hint);
			AssertEquals(ExpectedJXCCategory, ItemSet.DefaultChargeCodeForAccrualsImportItem.Category);
			GuidFindBoxRegistryEditorInfo editorInfo = ItemSet.DefaultChargeCodeForAccrualsImportItem.EditorInfo as GuidFindBoxRegistryEditorInfo;
			AssertNotNull("EditorInfo should of type " + nameof(GuidFindBoxRegistryEditorInfo), editorInfo);
			AssertEquals(RegistryFindBoxCollection.AccChargeCode, editorInfo.FindBoxCollection);
			AssertEquals(RegistryFindBoxFilter.MrgDsbOrMjaChargeCode, editorInfo.FindBoxFilter);
		}

		public void TestEnableAutoUpdateOnImport()
		{
			Assert("Default should be false", !ItemSet.EnableAutoUpdateOnImport);
			ItemSet.EnableAutoUpdateOnImport = true;
			Assert(ItemSet.EnableAutoUpdateOnImport);
		}

		public void TestEnableAutoUpdateOnImportItem()
		{
			AssertEquals("EnableAutoUpdateOnImport", EnableAutoUpdateOnImportItem.Name);
			AssertEquals("Enable Automatic Update on JXC Messaging Import", EnableAutoUpdateOnImportItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, EnableAutoUpdateOnImportItem.Storage);
			AssertEquals("If this is enabled, existing consols and/or shipments will be updated during import. If this is disabled (default behaviour), JXC Messages for existing consols and/or shipments will be ignored.", EnableAutoUpdateOnImportItem.Hint);
			AssertEquals(false, EnableAutoUpdateOnImportItem.DefaultValue);
			AssertEquals(ExpectedJXCCategory, EnableAutoUpdateOnImportItem.Category);
		}

		public void TestProfitSplitDueDestinationPercentage()
		{
			AssertEquals("Default Value is 50%", 50, ItemSet.ProfitSplitDueDestinationPercentage);
			ItemSet.ProfitSplitDueDestinationPercentage = 28;
			AssertEquals(28, ItemSet.ProfitSplitDueDestinationPercentage);
		}

		public void TestProfitSplitDueDestinationPercentageItem()
		{
			AssertEquals("ProfitSplitDueDestinationPercentage", ProfitSplitDueDestinationPercentageItem.Name);
			AssertEquals("Profit Split Due Destination Percentage. This is used when exporting JXC Air Profit Share messages.", ProfitSplitDueDestinationPercentageItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ProfitSplitDueDestinationPercentageItem.Storage);
			AssertEquals(ExpectedJXCCategory, ProfitSplitDueDestinationPercentageItem.Category);
			AssertEquals(50, ProfitSplitDueDestinationPercentageItem.DefaultValue);
			IntRegistryDataType dataType = (IntRegistryDataType)ProfitSplitDueDestinationPercentageItem.DataType;
			AssertEquals((double)0, dataType.LowerBound);
			AssertEquals((double)100, dataType.UpperBound);
		}

		StringRegistryItem JXCOutgoingDirectoryNameItem
		{
			get
			{
				if (fJXCOutgoingDirectoryNameItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCOutgoingDirectoryNameItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCOutgoingDirectoryNameItem = (StringRegistryItem)propertyInfo.GetValue(ItemSet, null);
					fJXCOutgoingDirectoryNameItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
				}

				return fJXCOutgoingDirectoryNameItem;
			}
		}

		public void TestDisableGsumMessaging()
		{
			Assert("Default should be false", !ItemSet.DisableGsumMessaging);
			ItemSet.DisableGsumMessaging = true;
			Assert(ItemSet.DisableGsumMessaging);
		}

		public void TestDisableGsumMessagingItem()
		{
			AssertEquals("DisableGsumMessaging", DisableGsumMessagingItem.Name);
			AssertEquals("Disable GSUM Messaging", DisableGsumMessagingItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, DisableGsumMessagingItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForDevelopers, DisableGsumMessagingItem.Options);
			AssertEquals("This is a developer-only trigger to disable GSUM messaging for troubleshooting / emergency purposes", DisableGsumMessagingItem.Hint);
			AssertEquals(false, DisableGsumMessagingItem.DefaultValue);
			AssertEquals(ExpectedJXCCategory, DisableGsumMessagingItem.Category);
		}

		public void TestIncludeProfitShareChargesItem()
		{
			AssertEquals("IncludeProfitShareCharges", IncludeProfitShareChargesItem.Name);
			AssertEquals("Include Profit Share Charges In Ocean Shipment Message", IncludeProfitShareChargesItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, IncludeProfitShareChargesItem.Storage);
			Assert(string.IsNullOrEmpty(IncludeProfitShareChargesItem.Hint));
			AssertEquals(false, IncludeProfitShareChargesItem.DefaultValue);
			AssertEquals(ExpectedJXCCategory, IncludeProfitShareChargesItem.Category);
		}

		StringRegistryItem BOLBondNameItem
		{
			get
			{
				if (fBOLBondNameItem == null)
				{
					PropertyInfo pi = typeof(JASDataRegistry).GetProperty("BOLBondNumItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fBOLBondNameItem = (StringRegistryItem)pi.GetValue(ItemSet, null);
				}

				return fBOLBondNameItem;
			}
		}

		StringRegistryItem JXCIncomingDirectoryNamesItem
		{
			get
			{
				if (fJXCIncomingDirectoryNamesItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCIncomingDirectoryNamesItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCIncomingDirectoryNamesItem = (StringRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJXCIncomingDirectoryNamesItem;
			}
		}

		StringRegistryItem JXCMessagesArchiveDirectoryNameItem
		{
			get
			{
				if (fJXCMessagesArchiveDirectoryNameItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCMessagesArchiveDirectoryNameItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCMessagesArchiveDirectoryNameItem = (StringRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJXCMessagesArchiveDirectoryNameItem;
			}
		}

		BooleanRegistryItem EnableAutoJXCMessagingItem
		{
			get
			{
				if (fEnableAutoJXCMessagingItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("EnableAutoJXCMessagingItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fEnableAutoJXCMessagingItem = (BooleanRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fEnableAutoJXCMessagingItem;
			}
		}

		BooleanRegistryItem QueryUserForDirectoryOnManualJXCExportItem
		{
			get
			{
				if (fQueryUserForDirectoryOnManualJXCExportItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("QueryUserForDirectoryOnManualJXCExportItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fQueryUserForDirectoryOnManualJXCExportItem = (BooleanRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fQueryUserForDirectoryOnManualJXCExportItem;
			}
		}

		BooleanRegistryItem EnableAutoUpdateOnImportItem
		{
			get
			{
				if (fEnableAutoUpdateOnImportItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("EnableAutoUpdateOnImportItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fEnableAutoUpdateOnImportItem = (BooleanRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fEnableAutoUpdateOnImportItem;
			}
		}

		IntRegistryItem ProfitSplitDueDestinationPercentageItem
		{
			get
			{
				if (fProfitSplitDueDestinationPercentageItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("ProfitSplitDueDestinationPercentageItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fProfitSplitDueDestinationPercentageItem = (IntRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fProfitSplitDueDestinationPercentageItem;
			}
		}

		BooleanRegistryItem DisableGsumMessagingItem
		{
			get
			{
				if (fDisableGsumMessagingItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("DisableGsumMessagingItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fDisableGsumMessagingItem = (BooleanRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fDisableGsumMessagingItem;
			}
		}

		BooleanRegistryItem IncludeProfitShareChargesItem
		{
			get
			{
				if (fIncludeProfitShareChargesItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("IncludeProfitShareChargesItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fIncludeProfitShareChargesItem = (BooleanRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fIncludeProfitShareChargesItem;
			}
		}

		StringRegistryItem fJXCOutgoingDirectoryNameItem;
		StringRegistryItem fJXCIncomingDirectoryNamesItem;
		StringRegistryItem fJXCMessagesArchiveDirectoryNameItem;
		BooleanRegistryItem fEnableAutoJXCMessagingItem;
		BooleanRegistryItem fQueryUserForDirectoryOnManualJXCExportItem;
		BooleanRegistryItem fEnableAutoUpdateOnImportItem;
		IntRegistryItem fProfitSplitDueDestinationPercentageItem;
		BooleanRegistryItem fDisableGsumMessagingItem;
		BooleanRegistryItem fIncludeProfitShareChargesItem;
		StringRegistryItem fBOLBondNameItem;
		const string ExpectedJXCCategory = ExpectedCategory + "/JXC";
		const string ExpectedBOLCategory = ExpectedCategory + "/JAS Bill of Lading";
		#endregion
		#region Notification Groups
		public void TestJXCAirMessagingNotificationGroupPK()
		{
			AssertEquals(ZGuid.Empty, ItemSet.JXCAirMessagingNotificationGroupPK);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.JXCAirMessagingNotificationGroupPK = newGuid;
			AssertEquals(newGuid, ItemSet.JXCAirMessagingNotificationGroupPK);
		}

		public void TestJXCAirMessagingNotificationGroupItem()
		{
			AssertEquals("JXCAirMessagingNotificationGroup", JXCAirMessagingNotificationGroupItem.Name);
			AssertEquals("JXC Air Messaging", JXCAirMessagingNotificationGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCAirMessagingNotificationGroupItem.Storage);
			AssertEquals("Please enter the group to send JXC Air Messaging notifications to", JXCAirMessagingNotificationGroupItem.Hint);
			AssertEquals(ExpectedNotificationGroupsCategory, JXCAirMessagingNotificationGroupItem.Category);
		}

		public void TestJXCOceanMessagingNotificationGroupPK()
		{
			AssertEquals(ZGuid.Empty, ItemSet.JXCOceanMessagingNotificationGroupPK);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.JXCOceanMessagingNotificationGroupPK = newGuid;
			AssertEquals(newGuid, ItemSet.JXCOceanMessagingNotificationGroupPK);
		}

		public void TestJXCOceanMessagingNotificationGroupItem()
		{
			AssertEquals("JXCOceanMessagingNotificationGroup", JXCOceanMessagingNotificationGroupItem.Name);
			AssertEquals("JXC Ocean Messaging", JXCOceanMessagingNotificationGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCOceanMessagingNotificationGroupItem.Storage);
			AssertEquals("Please enter the group to send JXC Ocean Messaging notifications to", JXCOceanMessagingNotificationGroupItem.Hint);
			AssertEquals(ExpectedNotificationGroupsCategory, JXCOceanMessagingNotificationGroupItem.Category);
		}

		public void TestJXCFinancialMessagingNotificationGroupPK()
		{
			AssertEquals(ZGuid.Empty, ItemSet.JXCFinancialMessagingNotificationGroupPK);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.JXCFinancialMessagingNotificationGroupPK = newGuid;
			AssertEquals(newGuid, ItemSet.JXCFinancialMessagingNotificationGroupPK);
		}

		public void TestJXCFinancialMessagingNotificationGroupItem()
		{
			AssertEquals("JXCFinancialMessagingNotificationGroup", JXCFinancialMessagingNotificationGroupItem.Name);
			AssertEquals("JXC Financial Messaging", JXCFinancialMessagingNotificationGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCFinancialMessagingNotificationGroupItem.Storage);
			AssertEquals("Please enter the group to send JXC Financial Messaging notifications to", JXCFinancialMessagingNotificationGroupItem.Hint);
			AssertEquals(ExpectedNotificationGroupsCategory, JXCFinancialMessagingNotificationGroupItem.Category);
		}

		public void TestJXCMiscMessagingNotificationGroupPK()
		{
			AssertEquals(ZGuid.Empty, ItemSet.JXCMiscMessagingNotificationGroupPK);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.JXCMiscMessagingNotificationGroupPK = newGuid;
			AssertEquals(newGuid, ItemSet.JXCMiscMessagingNotificationGroupPK);
		}

		public void TestJXCMiscMessagingNotificationGroupItem()
		{
			AssertEquals("JXCMiscMessagingNotificationGroup", JXCMiscMessagingNotificationGroupItem.Name);
			AssertEquals("JXC Miscellaneous Messaging", JXCMiscMessagingNotificationGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCMiscMessagingNotificationGroupItem.Storage);
			AssertEquals("Please enter the group to send JXC Miscellaneous Messaging notifications to", JXCMiscMessagingNotificationGroupItem.Hint);
			AssertEquals(ExpectedNotificationGroupsCategory, JXCMiscMessagingNotificationGroupItem.Category);
		}

		public void TestJXCGeneralNotificationGroupPK()
		{
			AssertEquals(ZGuid.Empty, ItemSet.JXCGeneralNotificationGroupPK);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.JXCGeneralNotificationGroupPK = newGuid;
			AssertEquals(newGuid, ItemSet.JXCGeneralNotificationGroupPK);
		}

		public void TestJXCGeneralNotificationGroupItem()
		{
			AssertEquals("JXCGeneralNotificationGroup", JXCGeneralNotificationGroupItem.Name);
			AssertEquals("JXC General Issues", JXCGeneralNotificationGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, JXCGeneralNotificationGroupItem.Storage);
			AssertEquals("Please enter the group to send JXC General Issues notifications to. Any issues which are not related to a specific message type will be sent to this group.", JXCGeneralNotificationGroupItem.Hint);
			AssertEquals(ExpectedNotificationGroupsCategory, JXCGeneralNotificationGroupItem.Category);
		}

		public void TestAccountingNotificationGroupPK()
		{
			AssertEquals(ZGuid.Empty, ItemSet.AccountingNotificationGroupPK);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.AccountingNotificationGroupPK = newGuid;
			AssertEquals(newGuid, ItemSet.AccountingNotificationGroupPK);
		}

		public void TestAccountingNotificationGroupItem()
		{
			AssertEquals("AccountingNotificationGroup", AccountingNotificationGroupItem.Name);
			AssertEquals("Accounting", AccountingNotificationGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, AccountingNotificationGroupItem.Storage);
			AssertEquals("Please enter the group to send Accounting notifications to", AccountingNotificationGroupItem.Hint);
			AssertEquals(ExpectedNotificationGroupsCategory, AccountingNotificationGroupItem.Category);
		}

		public void TestGetNotificationGroupPKFromMessageCategory()
		{
			ItemSet.JXCAirMessagingNotificationGroupPK = ZGuid.NewZGuid();
			ItemSet.JXCOceanMessagingNotificationGroupPK = ZGuid.NewZGuid();
			ItemSet.JXCFinancialMessagingNotificationGroupPK = ZGuid.NewZGuid();
			ItemSet.JXCMiscMessagingNotificationGroupPK = ZGuid.NewZGuid();
			ItemSet.JXCGeneralNotificationGroupPK = ZGuid.NewZGuid();
			AssertEquals(ItemSet.JXCAirMessagingNotificationGroupPK, ItemSet.GetNotificationGroupPKFromMessageCategory(JXCConstants.MessageCategories.Air));
			AssertEquals(ItemSet.JXCOceanMessagingNotificationGroupPK, ItemSet.GetNotificationGroupPKFromMessageCategory(JXCConstants.MessageCategories.Ocean));
			AssertEquals(ItemSet.JXCFinancialMessagingNotificationGroupPK, ItemSet.GetNotificationGroupPKFromMessageCategory(JXCConstants.MessageCategories.Financial));
			AssertEquals(ItemSet.JXCMiscMessagingNotificationGroupPK, ItemSet.GetNotificationGroupPKFromMessageCategory(JXCConstants.MessageCategories.Miscellaneous));
			AssertEquals(ItemSet.JXCGeneralNotificationGroupPK, ItemSet.GetNotificationGroupPKFromMessageCategory(JXCConstants.MessageCategories.Unknown));
		}

		GuidRegistryItem JXCAirMessagingNotificationGroupItem
		{
			get
			{
				if (fJXCAirMessagingNotificationGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCAirMessagingNotificationGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCAirMessagingNotificationGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJXCAirMessagingNotificationGroupItem;
			}
		}

		GuidRegistryItem JXCOceanMessagingNotificationGroupItem
		{
			get
			{
				if (fJXCOceanMessagingNotificationGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCOceanMessagingNotificationGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCOceanMessagingNotificationGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJXCOceanMessagingNotificationGroupItem;
			}
		}

		GuidRegistryItem JXCFinancialMessagingNotificationGroupItem
		{
			get
			{
				if (fJXCFinancialMessagingNotificationGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCFinancialMessagingNotificationGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCFinancialMessagingNotificationGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJXCFinancialMessagingNotificationGroupItem;
			}
		}

		GuidRegistryItem JXCMiscMessagingNotificationGroupItem
		{
			get
			{
				if (fJXCMiscMessagingNotificationGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCMiscMessagingNotificationGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCMiscMessagingNotificationGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJXCMiscMessagingNotificationGroupItem;
			}
		}

		GuidRegistryItem JXCGeneralNotificationGroupItem
		{
			get
			{
				if (fJXCGeneralNotificationGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JXCGeneralNotificationGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJXCGeneralNotificationGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJXCGeneralNotificationGroupItem;
			}
		}

		GuidRegistryItem AccountingNotificationGroupItem
		{
			get
			{
				if (fAccountingNotificationGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("AccountingNotificationGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fAccountingNotificationGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fAccountingNotificationGroupItem;
			}
		}

		const string ExpectedNotificationGroupsCategory = ExpectedCategory + "/Notification Groups";
		GuidRegistryItem fJXCAirMessagingNotificationGroupItem;
		GuidRegistryItem fJXCOceanMessagingNotificationGroupItem;
		GuidRegistryItem fJXCFinancialMessagingNotificationGroupItem;
		GuidRegistryItem fJXCMiscMessagingNotificationGroupItem;
		GuidRegistryItem fJXCGeneralNotificationGroupItem;
		GuidRegistryItem fAccountingNotificationGroupItem;
		#endregion
		#region Cognos
		public void TestCognosModeMapping()
		{
			CognosModeMapping someMapping = new CognosModeMapping();
			// Ensure we're not asserting empty mapping object
			someMapping.SelectedMode = "AI";
			someMapping.MapDepartments(GlbDepartment.CurrentDepartment);
			ItemSet.CognosModeMappingItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, someMapping);
			AssertEquals(someMapping, ItemSet.CognosModeMapping);
		}

		public void TestCognosModeMappingItem()
		{
			AssertEquals(ExpectedCognosCategory, ItemSet.CognosModeMappingItem.Category);
			AssertEquals(typeof(CognosModeMappingRegistryItem), ItemSet.CognosModeMappingItem.GetType());
		}

		public void TestCognosSalesGroup()
		{
			AssertEquals(ZGuid.Empty, ItemSet.CognosSalesGroup);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.CognosSalesGroup = newGuid;
			AssertEquals(newGuid, ItemSet.CognosSalesGroup);
		}

		public void TestCognosSalesGroupItem()
		{
			AssertEquals("CognosSalesGroup", CognosSalesGroupItem.Name);
			AssertEquals("Cognos Sales Group for Headcount Statistics", CognosSalesGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.System, CognosSalesGroupItem.Storage);
			AssertEquals("Please enter the group to be used when exporting Headcount for Sales records", CognosSalesGroupItem.Hint);
			AssertEquals(ExpectedCognosCategory, CognosSalesGroupItem.Category);
		}

		public void TestCognosAdminGroup()
		{
			AssertEquals(ZGuid.Empty, ItemSet.CognosAdminGroup);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.CognosAdminGroup = newGuid;
			AssertEquals(newGuid, ItemSet.CognosAdminGroup);
		}

		public void TestCognosAdminGroupItem()
		{
			AssertEquals("CognosAdminGroup", CognosAdminGroupItem.Name);
			AssertEquals("Cognos Administration Group for Headcount Statistics", CognosAdminGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.System, CognosAdminGroupItem.Storage);
			AssertEquals("Please enter the group to be used when exporting Headcount for Administration records", CognosAdminGroupItem.Hint);
			AssertEquals(ExpectedCognosCategory, CognosAdminGroupItem.Category);
		}

		public void TestCognosShipmentControlGroup()
		{
			AssertEquals(ZGuid.Empty, ItemSet.CognosShipmentControlGroup);
			ZGuid newGuid = ZGuid.NewZGuid();
			ItemSet.CognosShipmentControlGroup = newGuid;
			AssertEquals(newGuid, ItemSet.CognosShipmentControlGroup);
		}

		public void TestCognosShipmentControlGroupItem()
		{
			AssertEquals("CognosShipmentControlGroup", CognosShipmentControlGroupItem.Name);
			AssertEquals("Cognos Shipment Control Group for Headcount Statistics", CognosShipmentControlGroupItem.Caption);
			AssertEquals(RegistryStorageFlags.System, CognosShipmentControlGroupItem.Storage);
			AssertEquals("Please enter the group to be used when exporting Headcount for Shipment Control records", CognosShipmentControlGroupItem.Hint);
			AssertEquals(ExpectedCognosCategory, CognosShipmentControlGroupItem.Category);
		}

		GuidRegistryItem CognosSalesGroupItem
		{
			get
			{
				if (fCognosSalesGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("CognosSalesGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fCognosSalesGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fCognosSalesGroupItem;
			}
		}

		GuidRegistryItem CognosAdminGroupItem
		{
			get
			{
				if (fCognosAdminGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("CognosAdminGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fCognosAdminGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fCognosAdminGroupItem;
			}
		}

		GuidRegistryItem CognosShipmentControlGroupItem
		{
			get
			{
				if (fCognosShipmentControlGroupItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("CognosShipmentControlGroupItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fCognosShipmentControlGroupItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fCognosShipmentControlGroupItem;
			}
		}

		GuidRegistryItem fCognosSalesGroupItem;
		GuidRegistryItem fCognosAdminGroupItem;
		GuidRegistryItem fCognosShipmentControlGroupItem;
		const string ExpectedCognosCategory = ExpectedCategory + "/Cognos";
		#endregion
		#region Netting
		public void TestNettingCycleListItem()
		{
			AssertEquals("NettingCycleList", ItemSet.NettingCycleListItem.Name);
			AssertEquals("Netting Cycle list for Pre-Matching export", ItemSet.NettingCycleListItem.Caption);
			AssertEquals("Please enter the Netting Cycle Date list in \"dd-MMM-yy\" format. I.e. 05-JAN-05", ItemSet.NettingCycleListItem.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.NettingCycleListItem.Storage);
			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)ItemSet.NettingCycleListItem.EditorInfo;
			AssertEquals(true, editorInfo.ShowCodeColumn);
			AssertEquals(false, editorInfo.ShowDescriptionColumn);
			AssertEquals(CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.CodeFieldCasing);
			AssertEquals(CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.DescriptionFieldCasing);
			AssertEquals("Netting Cycle Date", editorInfo.CodeColumnCaption);
			AssertEquals(typeof(NettingCycleListRegistryDataType), ItemSet.NettingCycleListItem.DataType.GetType());
			CodeDescriptionPairList expectedDefaultValue = new CodeDescriptionPairList();
			expectedDefaultValue.AddPair("10-DEC-04");
			expectedDefaultValue.AddPair("11-JAN-05");
			expectedDefaultValue.AddPair("08-FEB-05");
			expectedDefaultValue.AddPair("09-MAR-05");
			expectedDefaultValue.AddPair("11-APR-05");
			expectedDefaultValue.AddPair("11-MAY-05");
			expectedDefaultValue.AddPair("10-JUN-05");
			expectedDefaultValue.AddPair("11-JUL-05");
			expectedDefaultValue.AddPair("10-AUG-05");
			expectedDefaultValue.AddPair("12-SEP-05");
			expectedDefaultValue.AddPair("11-OCT-05");
			expectedDefaultValue.AddPair("10-NOV-05");
			expectedDefaultValue.AddPair("09-DEC-05");
			AssertEquals("Default value", expectedDefaultValue.ToXMLByteArray(), ItemSet.NettingCycleListItem.DefaultValue.ToXMLByteArray());
			AssertEquals(ExpectedNettingCategory, ItemSet.NettingCycleListItem.Category);
		}

		public void TestUseJobInvoiceNumberAsPreMatchingInvoiceRef()
		{
			Assert("Default should be true", ItemSet.UseJobInvoiceNumberAsPreMatchingInvoiceRef);
			ItemSet.UseJobInvoiceNumberAsPreMatchingInvoiceRef = false;
			Assert(!ItemSet.UseJobInvoiceNumberAsPreMatchingInvoiceRef);
		}

		public void TestUseJobInvoiceNumberAsPreMatchingInvoiceRefItem()
		{
			AssertEquals("UseJobInvoiceNumberAsPreMatchingInvoiceRef", UseJobInvoiceNumberAsPreMatchingInvoiceRefItem.Name);
			AssertEquals("Use Job Invoice Number as Pre-Matching Invoice Ref", UseJobInvoiceNumberAsPreMatchingInvoiceRefItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, UseJobInvoiceNumberAsPreMatchingInvoiceRefItem.Storage);
			AssertEquals("If this is set to false, the system will use the auto-generated transaction number as the Invoice Ref", UseJobInvoiceNumberAsPreMatchingInvoiceRefItem.Hint);
			AssertEquals(true, UseJobInvoiceNumberAsPreMatchingInvoiceRefItem.DefaultValue);
			AssertEquals(ExpectedNettingCategory, UseJobInvoiceNumberAsPreMatchingInvoiceRefItem.Category);
		}

		public void TestNettingPaymentTerms()
		{
			AssertEquals("Default should be 45", 45, ItemSet.NettingPaymentTerms);
			ItemSet.NettingPaymentTerms = 60;
			AssertEquals(60, ItemSet.NettingPaymentTerms);
		}

		public void TestNettingPaymentTermsItem()
		{
			AssertEquals("NettingPaymentTerms", ItemSet.NettingPaymentTermsItem.Name);
			AssertEquals("Netting Payment Terms", ItemSet.NettingPaymentTermsItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.NettingPaymentTermsItem.Storage);
			AssertEquals("The date of shipment departure will drive the aging of the invoice. Please specify the number of days after EOM of the Departure Date to calculate the Maturity Date", ItemSet.NettingPaymentTermsItem.Hint);
			AssertEquals(45, ItemSet.NettingPaymentTermsItem.DefaultValue);
			AssertEquals(ExpectedNettingCategory, ItemSet.NettingPaymentTermsItem.Category);
		}

		BooleanRegistryItem UseJobInvoiceNumberAsPreMatchingInvoiceRefItem
		{
			get
			{
				if (fUseJobInvoiceNumberAsPreMatchingInvoiceRefItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("UseJobInvoiceNumberAsPreMatchingInvoiceRefItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fUseJobInvoiceNumberAsPreMatchingInvoiceRefItem = (BooleanRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fUseJobInvoiceNumberAsPreMatchingInvoiceRefItem;
			}
		}

		BooleanRegistryItem fUseJobInvoiceNumberAsPreMatchingInvoiceRefItem;
		const string ExpectedNettingCategory = ExpectedCategory + "/Netting";
		#endregion
		#region Common
		public void TestJASWWOrganisationItem()
		{
			AssertEquals("JASWWOrganisation", JASWWOrganisationItem.Name);
			AssertEquals("JASWW Organisation", JASWWOrganisationItem.Caption);
			AssertEquals(RegistryStorageFlags.System, JASWWOrganisationItem.Storage);
			AssertEquals("JASWW Organisation where Netting and JXC Messages are sent to. This organisation is also used for mapping things like Container Type, JAS SSL Code, etc...", JASWWOrganisationItem.Hint);
			AssertEquals(ZGuid.Empty, JASWWOrganisationItem.Value);
			AssertEquals(typeof(GuidFindBoxRegistryEditorInfo), JASWWOrganisationItem.EditorInfo.GetType());
			AssertEquals(ExpectedCategory, JASWWOrganisationItem.Category);
		}

		public void TestJASWWOrganisationPK()
		{
			AssertEquals(ZGuid.Empty, ItemSet.JASWWOrganisationPK);
			ZGuid newGuid = ZGuid.NewZGuid();
			JASWWOrganisationItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid.ToGuid());
			AssertEquals(newGuid, ItemSet.JASWWOrganisationPK);
		}

		GuidRegistryItem JASWWOrganisationItem
		{
			get
			{
				if (fJASWWOrganisationItem == null)
				{
					PropertyInfo propertyInfo = typeof(JASDataRegistry).GetProperty("JASWWOrganisationItem", BindingFlags.Instance | BindingFlags.NonPublic);
					fJASWWOrganisationItem = (GuidRegistryItem)propertyInfo.GetValue(ItemSet, null);
				}

				return fJASWWOrganisationItem;
			}
		}

		const string ExpectedCategory = "JAS Client Extensions";
		GuidRegistryItem fJASWWOrganisationItem;
		#endregion
	}
}
