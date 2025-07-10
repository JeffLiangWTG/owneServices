using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class CDSLocationOfGoodsUserControl : ZUserControl
	{
		public CDSLocationOfGoodsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				ChangeLocationOfGoodsVisibility();
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Declaration != null)
			{
				Declaration.JE_LocationQualifierInfo.ValueChanged -= new EventHandler(JE_LocationQualifier_ValueChanged);
				Declaration.JE_Calc_LocationOtherInformationTypeInfo.ValueChanged -= new EventHandler(JE_Calc_LocationOtherInformationType_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Declaration != null)
			{
				Declaration.JE_LocationQualifierInfo.ValueChanged += new EventHandler(JE_LocationQualifier_ValueChanged);
				Declaration.JE_Calc_LocationOtherInformationTypeInfo.ValueChanged += new EventHandler(JE_Calc_LocationOtherInformationType_ValueChanged);
			}
		}

		void JE_LocationQualifier_ValueChanged(object sender, EventArgs e)
		{
			ChangeLocationOfGoodsVisibility();
		}

		void JE_Calc_LocationOtherInformationType_ValueChanged(object sender, EventArgs e)
		{
			ChangeLocationOfGoodsVisibility();
		}

		void ChangeLocationOfGoodsVisibility()
		{
			if (Declaration != null)
			{
				LocationTextBox.Visible = !Declaration.JE_LocationQualifier.IsEmpty || Declaration.Lookups.LocationOfGoods.Count == 0;
				CDSGoodsLocationDropEdit.Visible = Declaration.JE_LocationQualifier.IsEmpty && Declaration.Lookups.LocationOfGoods.Count > 0;
				if (CDSGoodsLocationDropEdit.Visible)
				{
					CDSGoodsLocationDropEdit.SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(107));
				}
			}
		}

		public JobDeclaration Declaration => (JobDeclaration)base.CurrentDataItem;
	}
}
