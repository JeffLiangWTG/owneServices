using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(DocumentBrandingControl))]
	sealed class DocumentBrandingControlTest : ClientAndAgentBrandingControlTest
	{
		protected override RegistryZUserControl GetNewControl()
		{
			var control = new DocumentBrandingControl();
			MissingResourceStringChecker.ExcludeFromTest(control.UseGenericCheckBox);
			return control;
		}
	}
}
