using System;

namespace Enterprise.Security
{
	public sealed class SecurityCertificate
	{
		[ThreadStatic]
		static SecurityCertificate denied;
		[ThreadStatic]
		static SecurityCertificate granted;
		readonly bool isAllowed;

		SecurityCertificate(bool isAllowed)
		{
			this.isAllowed = isAllowed;
		}

		public static SecurityCertificate Denied
		{
			get { return denied ?? (denied = new SecurityCertificate(false)); }
		}

		public static SecurityCertificate Granted
		{
			get { return granted ?? (granted = new SecurityCertificate(true)); }
		}

		public bool IsAllowed
		{
			get { return isAllowed; }
		}
	}
}
