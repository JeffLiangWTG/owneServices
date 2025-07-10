using System;
using System.Windows.Forms;
using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI
{
	public class OrgSupplierPartFormCustomsPlugin : Customs.GUI.OrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
			this.part = part;
		}

		readonly OrgSupplierPart part;

		protected override Control GetNewUserControl() => new OrgSupplierPartFormCustomsControl();

		protected override Customs.Business.BaseCusClassPartPivot[] CusClassPartPivots => part != null ? part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.China) : Array.Empty<Customs.Business.BaseCusClassPartPivot>();
	}
}
