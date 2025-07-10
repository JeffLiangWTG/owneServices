using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5DeclarationSupplyChainActorsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestGridDockStyle()
		{
			AssertEquals("Dock", DockStyle.Fill, userControl.SupplyChainActorsGrid.Dock);
		}

		public void TestGridBindingMember()
		{
			AssertEquals("BindingMember", "CusSupplyChainActors", userControl.SupplyChainActorsGrid.GetBindingMember());
		}

		public void TestAvailableColumns()
		{
			var grid = userControl.SupplyChainActorsGrid;
			AssertSequencesEqual("Columns", GetExpectedColumnDetails(), GetActualColumnDetails());

			IEnumerable<(Type Type, string Name, bool IsMandatory)> GetActualColumnDetails()
			{
				return grid.ColumnStyles.Cast<ZGridColumnInfo>()
					.Select(c => (c.ColumnStyleType, c.ColumnName, c.IsMandatory));
			}

			IEnumerable<(Type Type, string Name, bool IsMandatory)> GetExpectedColumnDetails()
			{
				yield return (typeof(ZDropEditColumnStyle), nameof(CusSupplyChainActorReference.CFR_Code), true);
				yield return (typeof(ZOrganisationFindBoxColumnStyle),
					nameof(CusSupplyChainActorReference.OwnerOrgPK),
					false);
				yield return (typeof(ZTextBoxColumnStyle), nameof(CusSupplyChainActorReference.CFR_Reference), true);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5DeclarationSupplyChainActorsTabUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		Phase5DeclarationSupplyChainActorsTabUserControl userControl;
	}
}
