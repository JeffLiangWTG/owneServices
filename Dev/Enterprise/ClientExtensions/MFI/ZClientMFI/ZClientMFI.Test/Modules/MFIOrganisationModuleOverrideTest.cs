using System;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Testing
{
	class MFIOrganisationModuleOverrideTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestMFIOrgCSVImportMenu()
		{
			ZString menuName = "Import " + MFIOrganisationModuleOverride.orgCSVImportMenuName;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (MFIOrganisationModuleOverrideForTest testModule = new MFIOrganisationModuleOverrideForTest())
			{
				MenuAssertion.AssertHasMenu("Menu item should exist", testModule.FormActionMenu, "&Actions", "D&ata Transfer", menuName);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (MFIOrganisationModuleOverrideForTest testModule = new MFIOrganisationModuleOverrideForTest())
			{
				var dummy = testModule.FormActionMenu; //initialize
				EventHandler handler = testModule.ImportMenuItems[menuName];
				AssertNull("Menu item shouldn't exist", handler);
			}
		}

		class MFIOrganisationModuleOverrideForTest : MFIOrganisationModuleOverride
		{
			public new FilterModuleMenuItemDescriptorCollection ImportMenuItems
			{
				get
				{
					return base.ImportMenuItems;
				}
			}
		}
	}
}
