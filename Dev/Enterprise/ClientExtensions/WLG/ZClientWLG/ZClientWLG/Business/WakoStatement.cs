using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.WLG
{
	public class WakoStatement : Statement
	{
		protected WakoStatement(GlbBranch branch) : base(branch)
		{
		}

		public new static Statement New(GlbBranch branch)
		{
			return new WakoStatement(branch);
		}

		protected override ZBool IsClientSpecific
		{
			get { return ZBool.True; }
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#region Implementation
		#endregion
	}
}
