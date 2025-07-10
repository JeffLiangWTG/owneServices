using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.FLF
{
	public class FLFStatement : Statement
	{
		#region Constructors & SubTypeOverrides
		protected FLFStatement(GlbBranch branch)
			: base(branch)
		{
		}

		public new static Statement New(GlbBranch branch)
		{
			return new FLFStatement(branch);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		protected override ZBool IsClientSpecific
		{
			get { return ZBool.True; }
		}
	}
}
