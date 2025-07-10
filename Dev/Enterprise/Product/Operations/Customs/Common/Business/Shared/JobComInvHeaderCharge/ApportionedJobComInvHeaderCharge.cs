using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	partial class JobComInvCharge
	{
		public override ZBool J7_IsApportionedCharge
		{
			get { return base.J7_IsApportionedCharge; }
			set
			{
				base.J7_IsApportionedCharge = value;

				ReadOnly = J7_IsApportionedCharge;
			}
		}
	}
}


