using System;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePackedItemDetailsControl : ZUserControl
	{
		public UCC6TemporaryStoragePackedItemDetailsControl()
		{
			InitializeComponent();
			UnitDropEditOfCustomsValueFalse();
			InitializeTariffFindBox();
		}

		void UnitDropEditOfCustomsValueFalse()
		{
			var unitDropEdit = CustomsValueCalcDropEdit.Controls.Find("UnitDropEdit", true).FirstOrDefault() as ZDropEdit;
			if (unitDropEdit != null)
			{
				unitDropEdit.ReadOnly = true;
			}
		}

		protected virtual Type ExpectedDataSourceType => typeof(TemporaryStorageHeader);

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
			return Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff;
		}

		ZString GetDataGroupingForUniversalTariff()
		{
			return temporaryStorageHeader.DataGrouping;
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
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "Bills.PackedItems.API_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TemporaryStoragePackedItem)(((TemporaryStorageBill)(((TemporaryStorageHeader)(null)).Bills.SyncRoot)).PackedItems.SyncRoot)).API_FormattedTariff);
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 55, true);
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffCodeFindBox.PreBoundMaxLength = 10;
			this.TariffCodeFindBox.ShouldResize = false;
			this.TariffCodeFindBox.ShowDescriptionBox = true;
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.TariffCodeFindBox.TabIndex = 0;
			this.Controls.Add(this.TariffCodeFindBox);
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
		}

		internal Universal.GUI.TariffFindBox TariffCodeFindBox;

		TemporaryStorageHeader temporaryStorageHeader => ((TemporaryStorageHeader)((IDataBoundControl)TariffCodeFindBox).DataSource);
	}
}
