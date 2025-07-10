using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusGoodsLocation = Enterprise.Customs.EU.Business.CusGoodsLocation;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public static class CusGoodsLocationLayoutAssertions
	{
		public static void AssertControlVisibleForCertainQualifiers(PanelLayout layout, ControlReference control, CusGoodsLocation location, params string[] visibleForQualifiers)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					Assertion.AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(visibleForQualifiers), layout.IsVisible(control, location));
				}
			});
		}

		public static void AssertControlInvisibleForCertainQualifiers(PanelLayout layout, ControlReference control, CusGoodsLocation location, params string[] invisibleForQualifiers)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					Assertion.AssertEquals($"CGL_Qualifier = '{qualifier} should be hidden'", !qualifier.In(invisibleForQualifiers), layout.IsVisible(control, location));
				}
			});
		}

		public static void AssertControlVisibleWhenQualifierIsNotEmpty(PanelLayout layout, ControlReference control, CusGoodsLocation location)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					Assertion.AssertEquals($"CGL_Qualifier = '{qualifier}'", true, layout.IsVisible(control, location));
				}

				location.CGL_Qualifier = ZString.Empty;
				Assertion.AssertEquals("CGL_Qualifier empty", false, layout.IsVisible(control, location));
			});
		}
	}
}
