using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class FiscalReferencesUserControlTest : TestCaseWithFactory
	{
		public void TestColumnWidths()
		{
			using (var control = new FiscalReferencesUserControl())
			{
				var fiscalReferencesGrid = control.FindSingleOrDefault<ZGrid>("FiscalReferencesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CFR_Code", 47, fiscalReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_Code).Width);
					AssertEquals("CFR_Reference", 113, fiscalReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_Reference).Width);
					AssertEquals("OwnerOrgPK", 87, fiscalReferencesGrid.GetColumnStyle(CommonCusReference.Schema.OwnerOrgPK).Width);
					AssertEquals("CFR_OA_Owner", 167, fiscalReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_OA_Owner).Width);
				});
			}
		}

		public void TestCharacterCasing()
		{
			using (var control = new FiscalReferencesUserControl())
			{
				var fiscalReferencesGrid = control.FindSingleOrDefault<ZGrid>("FiscalReferencesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CFR_Code", CharacterCasing.Upper, fiscalReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_Code).CharacterCasing);
					AssertEquals("CFR_Reference", CharacterCasing.Upper, fiscalReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_Reference).CharacterCasing);
					AssertEquals("OwnerOrgPK", CharacterCasing.Upper, fiscalReferencesGrid.GetColumnStyle(CommonCusReference.Schema.OwnerOrgPK).CharacterCasing);
					AssertEquals("CFR_OA_Owner", CharacterCasing.Upper, fiscalReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_OA_Owner).CharacterCasing);
				});
			}
		}

		public void TestZAddressColumnGrouping()
		{
			using (var control = new FiscalReferencesUserControl())
			{
				var fiscalReferencesGrid = control.FindSingleOrDefault<ZGrid>("FiscalReferencesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("OwnerOrgPK", "DFFB1DD9-F071-4421-84C6-F50101145C1A", fiscalReferencesGrid.GetColumnStyle(CommonCusReference.Schema.OwnerOrgPK).GroupName.Key);
					AssertEquals("CFR_OA_Owner", "DFFB1DD9-F071-4421-84C6-F50101145C1A", fiscalReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_OA_Owner).GroupName.Key);
				});
			}
		}
	}
}
