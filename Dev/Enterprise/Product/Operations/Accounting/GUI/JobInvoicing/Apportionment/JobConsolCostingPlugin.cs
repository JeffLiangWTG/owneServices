using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class JobConsolCostingPlugin : ApportionmentPlugin
	{
		public JobConsolCostingPlugin(IBusiness hostEntity) : base(hostEntity)
		{
		}

		public override ResourceString MenuName => ResString.GetMultilingualString("be9089bf-962c-49a8-bb8e-9df658c3dd5d", "&Costing");

		public override bool IsSpecifiedNotToAddMenuItem => true;

		protected override Control GetNewUserControl()
		{
			return new NewApportionmentUserControl(Apportionments, isSpecifiedToHideColumnsForGenericConsolCosting: true);
		}
	}
}
