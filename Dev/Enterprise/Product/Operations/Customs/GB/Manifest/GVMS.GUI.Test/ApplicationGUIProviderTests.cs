using System;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTests : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			return result;
		}

		public override void TestBashingForm()
		{
			Assert(true);
		}

		protected override int MaxColumnsOfManifestLayout => 3;
	}
}
