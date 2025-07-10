using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class GDMBasicUserControl : ZUserControl
	{
		public GDMBasicUserControl()
		{
			InitializeComponent();
			InitializeTariffFindBox();
		}

		protected virtual Type ExpectedDataSourceType => typeof(GuidedDecisionMakingBasic);

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
			return GDMBasic.TariffType;
		}

		ZString GetDataGroupingForUniversalTariff()
		{
			return GDMBasic.DataGrouping;
		}

		ZDateTime GetEffectiveDateForUniversalTariff()
		{
			return GDMBasic.EffectiveDate;
		}

		void SetTariffFindBoxProperty()
		{
			this.TariffCodeFindBox.SuspendLayout();
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GuidedDecisionMakingBasic)(null)).FormattedTariff);
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 25, true);
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffCodeFindBox.PreBoundMaxLength = 10;
			this.TariffCodeFindBox.ShouldResize = false;
			this.TariffCodeFindBox.ShowDescriptionBox = true;
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.TariffCodeFindBox.TabIndex = 2;
			this.Controls.Add(this.TariffCodeFindBox);
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
		}

		internal Universal.GUI.TariffFindBox TariffCodeFindBox;

		GuidedDecisionMakingBasic GDMBasic => ((GuidedDecisionMakingBasic)((IDataBoundControl)TariffCodeFindBox).DataSource);
	}
}
