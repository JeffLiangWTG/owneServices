using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TGE.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Testing
{
	[TestedType(typeof(TGEDataRegistry))]
	public class TGEDataRegistryTest : RegistryItemSetTestCaseWithFactory<TGEDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Collection Count", 7, AllItems.Count);
			AssertVisible(ItemSet.PMSFileArchiveDirectoryRaw);
			AssertVisible(ItemSet.PMSFileImportDirectoryRaw);
			AssertVisible(ItemSet.CodeMapPMSOrganisationRaw);
			AssertVisible(ItemSet.PMSNotificationGroupRaw);
			AssertVisible(ItemSet.CSSDataTransferSwitchRegistryItem);
			AssertVisible(ItemSet.CSSImportCustomsStatusCodesItem);
			AssertVisible(ItemSet.CSSExportCustomsStatusCodesItem);
		}

		public void TestPMSFlatFileImportDirectory()
		{
			ZString currentValue = ItemSet.PMSFileImportDirectory;
			try
			{
				ItemSet.PMSFileImportDirectory = "incoming";
				AssertEquals("Incoming File Directory Path.", "incoming", ItemSet.PMSFileImportDirectory);
			}
			finally
			{
				ItemSet.PMSFileImportDirectory = currentValue;
			}
		}

		#region CodeMapPMSOrganisation
		public void TestCodeMapPMSOrganisation()
		{
			ZGuid currentValue = ItemSet.CodeMapPMSOrganisation;
			OrgHeader masterOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			try
			{
				AssertNotNull("PreCondition: MasterOrganisation is not null", masterOrganisation);
				ItemSet.CodeMapPMSOrganisation = masterOrganisation.PK;
				OrgHeader pMSOrganisation = Factory.Load<OrgHeader>(ItemSet.CodeMapPMSOrganisation);
				AssertNotNull("PMSOrganisation should not be null", pMSOrganisation);
				AssertEquals("PMS Organisation For Code Mapping", masterOrganisation.PK, pMSOrganisation.PK);
			}
			finally
			{
				ItemSet.CodeMapPMSOrganisation = currentValue;
			}
		}

		#endregion
		#region PMSFlatFileArchiveDirectory
		public void TestPMSFlatFileArchiveDirectory()
		{
			ZString currentValue = ItemSet.PMSFileArchiveDirectory;
			try
			{
				ItemSet.PMSFileArchiveDirectory = "Archive";
				AssertEquals("Archived File Directory Path.", "Archive", ItemSet.PMSFileArchiveDirectory);
			}
			finally
			{
				ItemSet.PMSFileArchiveDirectory = currentValue;
			}
		}

		#endregion
		public new void TestCategoriesAndCaptionsAreLocalizable()
		{
			Assert(true);
		}

		#region PMSNotificationGroup
		public void TestPMSNotificationGroup()
		{
			Guid currentValue = ItemSet.PMSNotificationGroup;
			GlbGroup notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			AssertNotNull("PreCondition: NotificationGroup is not null", notificationGroup);
			ItemSet.PMSNotificationGroup = notificationGroup.PK.ToGuid();
		}

		#endregion
		[TestDate(2006, 11, 10)]
		public void TestDataTransferSwitchRegistryItem()
		{
			AssertEquals("Default ExportCSSDirectory", ZString.Empty, ItemSet.CSSExportDirectory);
			AssertEquals("Default ExportCSSNotifyGroup", true, ItemSet.CSSExportNotifyGroup.IsEmpty);
			ItemSet.CSSDataTransferSwitchRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestHelper.GetValidDataTransferSwitchRegistryBusinessObject());
			AssertEquals("ExportCSSDirectory", Env.TempPath, ItemSet.CSSExportDirectory);
			AssertEquals("ExportCSSsNotifyGroup", false, ItemSet.CSSExportNotifyGroup.IsEmpty);
		}

		public void TestCSSImportCustomsStatusCodesItem()
		{
			AssertEquals("CSSImportCustomsStatusCodesItem empty", 0, ItemSet.CSSImportCustomsStatusCodes.Count);
			TGEEventRegistryBusinessObjectCollection codes = new TGEEventRegistryBusinessObjectCollection();
			TGEEventRegistryBusinessObject cusEvent = codes.AddNew();
			cusEvent.Code = "ADD";
			cusEvent = codes.AddNew();
			cusEvent.Code = "EDT";
			ItemSet.CSSImportCustomsStatusCodes = codes;
			AssertEquals("CSSImportCustomsStatusCodesItem empty", 2, ItemSet.CSSImportCustomsStatusCodes.Count);
		}

		public void TestCSSExportCustomsStatusCodesItem()
		{
			AssertEquals("CSSImportCustomsStatusCodesItem empty", 0, ItemSet.CSSExportCustomsStatusCodes.Count);
			TGEEventRegistryBusinessObjectCollection codes = new TGEEventRegistryBusinessObjectCollection();
			TGEEventRegistryBusinessObject cusEvent = codes.AddNew();
			cusEvent.Code = "ADD";
			cusEvent = codes.AddNew();
			cusEvent.Code = "EDT";
			ItemSet.CSSExportCustomsStatusCodes = codes;
			AssertEquals("CSSImportCustomsStatusCodesItem empty", 2, ItemSet.CSSExportCustomsStatusCodes.Count);
		}

		public void TestCSSInterfaceReadyToGo()
		{
			AssertEquals("CSS Interface not ready.", false, ItemSet.CSSInterfaceReadyToGo);
			TestHelper.SetValidRegistryAll();
			TestHelper.SwitchToCompanyAndBranch(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("CSS Interface not ready.", false, ItemSet.CSSInterfaceReadyToGo);
			TestHelper.RevertToInitialCompanyAndBranch();
			AssertEquals("CSS Interface now ready.", true, ItemSet.CSSInterfaceReadyToGo);
		}

		TGETestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TGETestHelper());
			}
		}

		TGETestHelper testHelper;
	}
}
