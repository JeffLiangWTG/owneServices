using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.MFI
{
	public class MFIStatement : Statement
	{
		internal protected MFIStatement(GlbBranch branch)
			: base(branch)
		{
		}

		public new static Statement New(GlbBranch branch)
		{
			return new MFIStatement(branch);
		}

		protected override ZBool IsClientSpecific
		{
			get { return MFIConstants.NZ.ClientSpecificCondition; }
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		public ZBool StatementIsClientSpecificProperty
		{
			get { return IsClientSpecific; }
		}
	}
}
