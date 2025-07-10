using System;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class OwnerDocAddressUserControl : ZUserControl
	{
		public OwnerDocAddressUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var declaration = dataSource as EMCSJobDeclaration;
			if (declaration != null)
			{
				declaration.ZG_GuarantorTypeInfo.ValueChanged -= ZG_GuarantorTypeInfo_ValueChanged;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (declaration != null)
			{
				declaration.ZG_GuarantorTypeInfo.ValueChanged += ZG_GuarantorTypeInfo_ValueChanged;
				ZG_GuarantorTypeInfo_ValueChanged(this, EventArgs.Empty);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var declaration = DataSource as EMCSJobDeclaration;
				if (declaration != null)
				{
					declaration.ZG_GuarantorTypeInfo.ValueChanged -= ZG_GuarantorTypeInfo_ValueChanged;
				}
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void ZG_GuarantorTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = DataSource as EMCSJobDeclaration;
			OwnerDocAddressControl.Enabled = declaration?.OwnerDocumentaryAddress_Enabled ?? false;
		}
	}
}
