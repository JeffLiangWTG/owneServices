#if WINZOR
	using WinzorFramework;
#endif
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	#region ZDummyFilterForm

	public class ZDummyFilterStripBaseControlForm : ZForm, IZDummyFilterStripForm
	{
		protected ZDummyFilterStripBaseControlForm() : this(new DummyFilterStripBusinessObject()) { }

		protected ZDummyFilterStripBaseControlForm(FilterStripBusinessObject filterBizO)
		{
			SetupFilter(filterBizO);
		}

		public ZDummyFilterStripBaseControlForm(DummyBusinessObject dummy)
			: this(dummy, new DummyFilterStripBusinessObject())
		{ }

		public ZDummyFilterStripBaseControlForm(DummyBusinessObject dummy, FilterStripBusinessObject filterBizO)
			: base(dummy)
		{
			SetupFilter(filterBizO);
		}

		void SetupFilter(FilterStripBusinessObject filterBizO)
		{
			InitialiseGrid();
			FilterBizO = filterBizO;
			FilterControl = GetNewDummyZFilterStripControl(FilterBizO);
			Controls.Add(FilterControl);
			AnotherTextBox = new ZTextBox();
			Controls.Add(AnotherTextBox);
		}

		void InitialiseGrid()
		{
			this.Grid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			//
			// FilteredGrid
			//
			Grid.AllowNavigation = false;
			Grid.CaptionVisible = false;
			Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			Grid.LayoutKey = "FilteredGrid";
			Grid.Name = "FilteredGrid";
			Grid.ReadOnly = true;
			Grid.ShouldSetErrorsOnTabPage = false;
			Grid.BorderStyle = System.Windows.Forms.BorderStyle.None;

			//
			// ControlForLayout
			//
			Grid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			Grid.Location = ControlDpiScalingHelper.NewScaledPoint(0, 152);
			Grid.Size = ControlDpiScalingHelper.NewScaledSize(758, 22);
			Grid.TabIndex = 6;

			this.Controls.Add(this.Grid);
			this.Controls.SetChildIndex(this.Grid, 0);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public ZGrid Grid { get; private set; }
		public FilterStripBusinessObject FilterBizO { get; private set; }
		public ZFilterStripBaseControl FilterControl { get; private set; }
		public IFilterStripBaseControlForTest FilterControlExposed { get { return (IFilterStripBaseControlForTest)FilterControl; } }
		public ZTextBox AnotherTextBox { get; private set; }

		protected virtual ZFilterStripBaseControl GetNewDummyZFilterStripControl(FilterStripBusinessObject filterBizO)
		{
			return new DummyZFilterStripBaseControl(Grid, filterBizO);
		}

		public FilterStripBusinessObject GetFilterBizO()
		{
			return FilterBizO;
		}

		public ZFilterStripBaseControl GetFilterControl()
		{
			return FilterControl;
		}

		IFilterStripBaseControlForTest IZDummyFilterStripForm.GetFilterControlExposed()
		{
			return (IFilterStripBaseControlForTest)FilterControl;
		}

		public ZTextBox GetAnotherTextBox()
		{
			return AnotherTextBox;
		}

		public ZGrid GetGrid()
		{
			return Grid;
		}
	}

	#endregion
}
