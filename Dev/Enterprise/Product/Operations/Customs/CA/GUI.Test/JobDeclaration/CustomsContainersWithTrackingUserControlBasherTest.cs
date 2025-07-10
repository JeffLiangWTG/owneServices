using System;
using NUnit.Framework;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CustomsContainersWithTrackingUserControl))]
	sealed class CustomsContainersWithTrackingUserControlBasherTest : ImportCustomsUserControlBasherTest
	{
		protected override Type UserControlToBashType => typeof(CustomsContainersWithTrackingUserControl);

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashing();
			return declaration;
		}
	}
}
#endif
