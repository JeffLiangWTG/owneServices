using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

sealed class CusSupplyChainActorsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		AssertEquals(typeof(CusEntryInstruction), userControl.BindingSource.DataSourceType);
	}

	public void TestGridBindingMember()
	{
		AssertEquals("BindingMember", "SupplyChainActors", userControl.SupplyChainActorsGrid.GetBindingMember());
	}

	public void TestAvailableColumns()
	{
		var grid = userControl.SupplyChainActorsGrid;
		AssertSequencesEqual("Columns", GetExpectedColumnDetails(), GetActualColumnDetails());

		IEnumerable<(Type Type, string Name, bool IsMandatory)> GetActualColumnDetails()
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => (c.ColumnStyleType, c.ColumnName, c.IsMandatory));
		}

		IEnumerable<(Type Type, string Name, bool IsMandatory)> GetExpectedColumnDetails()
		{
			yield return (typeof(ZDropEditColumnStyle), nameof(CusSupplyChainActorReference.CFR_Code), true);
			yield return (typeof(ZOrganisationFindBoxColumnStyle), nameof(CusSupplyChainActorReference.OwnerOrgPK), false);
			yield return (typeof(ZTextBoxColumnStyle), nameof(CusSupplyChainActorReference.CFR_Reference), true);
		}
	}

	public void TestGridColumnStyleProperties()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var collection = new CusSupplyChainActorReferenceCollection(entryInstruction);
		using (var control = new CusSupplyChainActorsUserControl())
		{
			var grid = control.SupplyChainActorsGrid;
			grid.SetDataBinding(collection, "");
			control.Show();

			AssertEquals(3, grid.Columns.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CFR_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(Customs.Business.AutoCusReference.Schema.CFR_Code).CharacterCasing);
				AssertEquals("OwnerOrgPK: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(CusSupplyChainActorReference.Schema.OwnerOrgPK).CharacterCasing);
				AssertEquals("CFR_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(Customs.Business.AutoCusReference.Schema.CFR_Reference).CharacterCasing);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new CusSupplyChainActorsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	CusSupplyChainActorsUserControl userControl;
}
