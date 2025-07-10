using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaPackedItemDetailsControl : ZUserControl
	{
		public AsycudaPackedItemDetailsControl()
		{
			InitializeComponent();
			InitializeTariffFindBox();
		}

		protected virtual Type ExpectedDataSourceType => typeof(Business.AsycudaManifestHeader);

		void InitializeTariffFindBox()
		{
			CreateUniversalTariffFindBox();
			SetTariffFindBoxProperty();
		}

		void CreateUniversalTariffFindBox()
		{
			var tariffFindBox = new Universal.GUI.TariffFindBox();
			tariffFindBox.GetTariffType = GetTariffTypeForUniversalTariff;
			tariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			tariffFindBox.GetEffectiveDate = GetEffectiveDateForUniversalTariff;
			this.TariffCodeFindBox = tariffFindBox;
		}

		ZString GetTariffTypeForUniversalTariff()
		{
			return Universal.Constants.TariffTypes.Import;
		}

		ZString GetDataGroupingForUniversalTariff()
		{
			return Core.Constants.CountryCodes.Israel;
		}

		ZDateTime GetEffectiveDateForUniversalTariff()
		{
			return ZDateTime.Today;
		}

		void SetTariffFindBoxProperty()
		{
			this.TariffCodeFindBox.SuspendLayout();
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "PackedItems.API_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.AsycudaPackedItem)(((Business.AsycudaBill)(null)).PackedItems.SyncRoot)).API_FormattedTariff);
			this.TariffCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(36, 55, true);
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.ModuleID = ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffCodeFindBox.PreBoundMaxLength = 10;
			this.TariffCodeFindBox.ShouldResize = false;
			this.TariffCodeFindBox.ShowDescriptionBox = true;
			this.TariffCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.TariffCodeFindBox.TabIndex = 0;
			this.Controls.Add(this.TariffCodeFindBox);
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
		}

		internal Universal.GUI.TariffFindBox TariffCodeFindBox;
	}
}
