using System;
using Enterprise.Customs.IE.PBN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			return result;
		}

		[RequiresSTA]
		public override void TestBashingForm()
		{
			Assert(true);
		}

		protected override int MaxColumnsOfManifestLayout => 3;
	}
}
