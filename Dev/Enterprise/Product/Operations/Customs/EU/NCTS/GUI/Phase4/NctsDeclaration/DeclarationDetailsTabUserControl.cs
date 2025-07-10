using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class DeclarationDetailsTabUserControl : ZUserControl
	{
		public DeclarationDetailsTabUserControl()
		{
			InitializeComponent();
		}

		protected const string IsVisibleForBindingString = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			BrokerFindBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

			header = dataSource as NctsHeader;
			if (header != null)
			{
				var departureMovementHeader = header.MovementHeader;
				departureMovementHeader.IsSimplifiedNctsProcedureInfo.ValueChanged -= IsSimplifiedNctsProcedureInfo_ValueChanged;
				departureMovementHeader.IsSimplifiedNctsProcedureInfo.ValueChanged += IsSimplifiedNctsProcedureInfo_ValueChanged;
				IsSimplifiedNctsProcedureInfo_ValueChanged(null, EventArgs.Empty);

				header.BH_OverrideFreightDefaultsInfo.ValueChanged -= BH_OverrideFreightDefaultsInfo_ValueChanged;
				header.BH_OverrideFreightDefaultsInfo.ValueChanged += BH_OverrideFreightDefaultsInfo_ValueChanged;
				BH_OverrideFreightDefaultsInfo_ValueChanged(null, EventArgs.Empty);

				departureMovementHeader.BM_InBondEntryTypeInfo.ValueChanged -= DeclarationTypeInfo_ValueChanged;
				departureMovementHeader.BM_InBondEntryTypeInfo.ValueChanged += DeclarationTypeInfo_ValueChanged;
				DeclarationTypeInfo_ValueChanged(null, EventArgs.Empty);

				BrokerFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, header, nameof(NctsHeader.IsBrokerNeeded), false, DataSourceUpdateMode.Never));

				InitTabsVisibility();
			}

			ValuationDateDateEdit.Enabled = header == null || header.MovementReferenceNumber.IsEmpty;
		}

		protected void BH_OverrideFreightDefaultsInfo_ValueChanged(object sender, EventArgs e)
		{
			OverrideFreightDefaults.Visible = header.IsPluggedIn;
		}

		protected virtual void IsSimplifiedNctsProcedureInfo_ValueChanged(object sender, EventArgs e)
		{
			if (header != null)
			{
				GoodsLocationSimplifiedGroupBox.Location = GoodsLocationNormalGroupBox.Location;

				MoveControlsBelowGoodsLocation();
				if (header.MovementHeader.IsSimplifiedNctsProcedure)
				{
					GoodsLocationSimplifiedGroupBox.Visible = true;
					GoodsLocationNormalGroupBox.Visible = false;
				}
				else
				{
					GoodsLocationNormalGroupBox.Visible = true;
					GoodsLocationSimplifiedGroupBox.Visible = false;
				}
			}
		}

		void InitTabsVisibility()
		{
			PortOfDispatchFindBox.Visible = header.Configuration?.FullLoadPortSupport ?? false;
			CountryOfDispatchDropEdit.Visible = !PortOfDispatchFindBox.Visible;
		}

		protected virtual void MoveControlsBelowGoodsLocation()
		{
		}

		protected void DeclarationTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (header != null && header.MovementHeader.IsTIRDeclaration)
			{
				TirCarnetNumberTextBox.Visible = true;
				TirCarnetExpiryDateEdit.Visible = true;
			}
			else
			{
				TirCarnetNumberTextBox.Visible = false;
				TirCarnetExpiryDateEdit.Visible = false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (header != null)
			{
				header.MovementHeader.IsSimplifiedNctsProcedureInfo.ValueChanged -= IsSimplifiedNctsProcedureInfo_ValueChanged;
				header.BH_OverrideFreightDefaultsInfo.ValueChanged -= BH_OverrideFreightDefaultsInfo_ValueChanged;
				header.MovementHeader.BM_InBondEntryTypeInfo.ValueChanged -= DeclarationTypeInfo_ValueChanged;
			}
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetupLayout();
		}

		protected void SetupLayout()
		{
			var guaranteesUserControlType = GetGuaranteesUserControlType();
			if (guaranteesUserControlType != null && GuaranteesZDynamicUserControl.UserControlType != guaranteesUserControlType)
			{
				GuaranteesZDynamicUserControl.UserControlType = guaranteesUserControlType;
			}

			var containersAndSealsUserControlType = GetContainersAndSealsUserControlType();
			if (containersAndSealsUserControlType != null && ContainersAndSealsZDynamicUserControl.UserControlType != containersAndSealsUserControlType)
			{
				ContainersAndSealsZDynamicUserControl.UserControlType = containersAndSealsUserControlType;
			}
		}

		protected virtual Type GetContainersAndSealsUserControlType() => typeof(ContainersAndSealsUserControl);

		protected virtual Type GetGuaranteesUserControlType() => typeof(GuaranteesUserControl);

		NctsHeader header;
	}
}
