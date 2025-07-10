using System;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public class ApplicationGUIProvider : ApplicationGUIProviderBase
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);
	}
}
