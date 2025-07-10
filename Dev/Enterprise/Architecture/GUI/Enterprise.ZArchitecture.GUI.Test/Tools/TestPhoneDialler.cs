using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class TestPhoneDialler : PhoneDialler
	{
		protected override bool IsRemoteSessionWithoutRDServices
		{
			get
			{
				return IsRemoteSessionWithoutRDServicesForTest;
			}
		}

		public bool IsRemoteSessionWithoutRDServicesForTest;

		public string UriDialled;
		public bool throwWin32Exception;

		protected override void StartPhoneCall(string sipParameter)
		{
			if (throwWin32Exception)
			{
				throw new Win32Exception("MEH");
			}
			UriDialled = sipParameter;
			//no need to call the base dialler for testing
		}
	}
}
