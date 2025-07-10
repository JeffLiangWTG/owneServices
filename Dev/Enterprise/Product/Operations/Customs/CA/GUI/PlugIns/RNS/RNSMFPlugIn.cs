using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.GUI.PlugIns.RNS
{
	public class RNSMFPlugIn : RNSPlugIn
	{
		public RNSMFPlugIn(IRNSPlugInSupport plugInSupport)
			: base(plugInSupport)
		{
		}

		#region Overrides of ZPlugIn

		public override string Name
		{
			get { return "RNS/MF"; }
		}

		protected sealed override ZBool HasUserControl
		{
			get { return ZBool.False; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new RNSMFMenu(this.plugInSupport);
		}
		#endregion
	}
}
