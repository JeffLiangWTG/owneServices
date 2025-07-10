using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationWebAddressValidationExtension))]
	sealed class CusGoodsLocationWebAddressValidationExtensionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCusGoodsLocationAddressValidationExtensionRegisterAndCleanup()
		{
			var layoutProvider = new CusGoodsLocationLayout();
			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(layoutProvider);
			}
		}
	}
}
