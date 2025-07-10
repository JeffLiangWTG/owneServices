//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmServiceHostValidation
//
//    This class should be used for overriding validation in AutoStmServiceHostValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceHostValidation : AutoStmServiceHostValidation
	{
		public StmServiceHostValidation(AutoStmServiceHost parent) : base(parent)
		{
		}

		protected override void CheckSH_ProxyPort()
		{
			if (!Parent.SH_ProxyAutoDetect)
			{
				MandatoryValidation.CheckNotNegative(Parent.SH_ProxyPortInfo);
				CompareValidation.CheckLessThanOrEqualTo(Parent.SH_ProxyPortInfo, 65535);
				if (!Parent.SH_ProxyHost.IsEmpty)
				{
					MandatoryValidation.CheckNotZero(Parent.SH_ProxyPortInfo);
				}
			}
		}

		protected override void CheckSH_ProxyUserName()
		{
			if (!Parent.SH_ProxyAutoDetect && Parent.SH_ProxyAuthentication)
			{
				MandatoryValidation.CheckEntered(Parent.SH_ProxyUserNameInfo);
			}
		}

		protected override void CheckSH_ProxyPassword()
		{
			if (!Parent.SH_ProxyAutoDetect && Parent.SH_ProxyAuthentication)
			{
				MandatoryValidation.CheckEntered(Parent.SH_ProxyPasswordInfo);
			}
		}

		#region Implementation

		public new StmServiceHost Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (StmServiceHost)base.Parent; }
		}

		#endregion
	}
}

