using System;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public class ApplicationGUIProviderSS : ApplicationGUIProviderBase
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProviderSS);
	}
}
