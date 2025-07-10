using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class CommissionFinalizerFilterControl : ZFilterStripCommonControl
	{
		[Obsolete("Use the constructor that takes a filter biz obj, this constructor is just for the designer", true)]
		public CommissionFinalizerFilterControl()
		{
		}

		public CommissionFinalizerFilterControl(CommissionFinalizer finalizer, CommissionFinalizerFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
			this.finalizer = finalizer;

			InitializeComponent();
			InitializeLayout();
		}

		const string CommissionFinalizerFilterBusinessObjectLayoutContext = "CommissionFinalizerFilterFilterBusinessObjectLayoutContext";

		#region Grid Collection

		public override IBusinessObjectCollection GridCollection
		{
			get { return finalizer.ViewCommissionLineCollection; }
		}
		readonly CommissionFinalizer finalizer;

		#endregion

		#region Layout

		void InitializeLayout()
		{
			((IFilterStripBusinessObjectInternals)FilterBusinessObject).LayoutContext = CommissionFinalizerFilterBusinessObjectLayoutContext;

			this.AddStripButton.Visible = false;
			this.ToolStripAddGroupButton.Visible = false;
		}

		protected override int MaximumAllowableQueriesPerSqlStatement => OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.Value;

		#region Item Grids

		ICommissionFinalizerItemGridsControl ItemGridsControl
		{
			get
			{
				if (itemGridsControl == null)
				{
					if (OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
					{
						itemGridsControl = new CommissionFinalizerItemGridsControl();
						((Control)itemGridsControl).AllowOverlap(ToolStrip);
						((Control)itemGridsControl).AllowOverlap(ToolStripHelp);
					}
					else
					{
						itemGridsControl = new GlobalCommissionFinalizerItemGridsControl();
					}
				}

				return itemGridsControl;
			}
		}
		ICommissionFinalizerItemGridsControl itemGridsControl;

		protected override Control ControlForLayout
		{
			get { return (Control)ItemGridsControl; }
		}

		#endregion

		#region Top Level Grid

		protected override ZFilterGrid GetNewFilteredGrid()
		{
			return ItemGridsControl.TopLevelGrid;
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();
			Grid.ReadOnly = false;
		}

		#endregion

		#endregion

		#region FilterStrips

		protected override bool ShouldAddEmptyFilterStripOnReset
		{
			get { return false; }
		}

		#endregion

		#region Bind

		protected override void Bind()
		{
			if (!isBound)
			{
				BindCore();
				isBound = true;
			}
		}

		bool isBound;

		protected void BindCore()
		{
			this.SetDataBinding(finalizer, "");
		}

		#endregion
	}
}
