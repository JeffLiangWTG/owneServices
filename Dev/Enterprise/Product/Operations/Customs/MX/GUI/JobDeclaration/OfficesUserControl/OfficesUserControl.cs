using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public partial class CustomsAreaUserControl : ZUserControl
	{
		public CustomsAreaUserControl()
		{
			InitializeComponent();
		}

		JobDeclaration Declaration => (JobDeclaration)base.CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is JobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				JE_MessageTypeInfo_ValueChanged(null, null);
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				if (Declaration.IsExport || Declaration.IsImport)
				{
					EntryOrExitAreaFindBox.GetExtension<LabelCaptionRenderer>().Caption = Declaration.JE_LocationOfGoodsInfo.Description;
				}
			}
		}
	}
}
