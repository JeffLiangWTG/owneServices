using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DigitalCertificateRegistryItemEditor))]
	sealed class DigitalCertificateRegistryItemEditorNonDeveloperLoginTest : DigitalCertificateRegistryItemEditorDeveloperLoginTest
	{
		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DigitalCertificateControl);
		}

		protected override void SetUp()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			base.SetUp();
		}
	}
}
