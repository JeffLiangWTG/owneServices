using System;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.CA.Module.Testing
{
	abstract class CAShipmentGridColumnsProviderTest : CAExternalGridColumnsProviderTest
	{
		internal sealed class MockConsolForm : ZForm
		{
			internal MockConsolForm(CFSLoadListConsol businessEntity)
				: base(businessEntity)
			{
			}

			internal ZGridForTest Grid;

			protected override void OnLoad(EventArgs e) => new CAShipmentGridColumnsProvider().AddColumns(Grid);

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ZGridForTest();

				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
				BindingSource.SetBindingMember(Grid, "Shipments");
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				Controls.Add(Grid);
			}
		}

		internal sealed class ZGridForTest : ZGridWithoutColumnStylesSerialisation, IGridControl
		{
		}
	}
}
