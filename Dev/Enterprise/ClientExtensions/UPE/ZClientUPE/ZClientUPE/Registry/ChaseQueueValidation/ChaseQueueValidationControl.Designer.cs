using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class ChaseQueueValidationControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid ChaseQueueValidationGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.ChaseQueueValidationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChaseQueueValidationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Registry.Business.ChaseQueueValidationCollection);
			// 
			// ChaseQueueValidationGrid
			// 
			this.ChaseQueueValidationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChaseQueueValidationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.ChaseQueueValidation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.ChaseQueueValidation)(null)).DayOfTheWeek)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.ChaseQueueValidation)(null)).DayOfTheWeekCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Registry.Business.ChaseQueueValidation)(null)).TimeFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Registry.Business.ChaseQueueValidation)(null)).TimeTo)));
			this.ChaseQueueValidationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "DayOfTheWeekCollection";
			zDropEditColumnStyleInfo1.Caption = "Day";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "DayOfTheWeek";
			zDateEditColumnStyleInfo1.Caption = "Time From";
			zDateEditColumnStyleInfo1.ColumnName = "TimeFrom";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo2.Caption = "Time To";
			zDateEditColumnStyleInfo2.ColumnName = "TimeTo";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.ChaseQueueValidationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChaseQueueValidationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ChaseQueueValidationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ChaseQueueValidationGrid.GridId = "eeacf310-9625-4fea-950f-3c1501f6d388";
			this.ChaseQueueValidationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChaseQueueValidationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChaseQueueValidationGrid.LayoutKey = "zGrid1";
			this.ChaseQueueValidationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChaseQueueValidationGrid.Name = "ChaseQueueValidationGrid";
			this.ChaseQueueValidationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 384, true);
			this.ChaseQueueValidationGrid.TabIndex = 0;
			// 
			// ChaseQueueValidationControl
			// 
			this.Controls.Add(this.ChaseQueueValidationGrid);
			this.Name = "ChaseQueueValidationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 384, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChaseQueueValidationGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
