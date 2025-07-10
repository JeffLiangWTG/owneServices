using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public static class PlaceOfUseOrProcessingLayoutAssertions
	{
		public static void AssertControlVisibleForCertainQualifiers(PanelLayout layout, ControlReference control, PlaceOfUseOrProcessing location, params string[] visibleForQualifiers)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				var qualifierList = new CusGoodsLocationQualifierList();
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.PostcodeAddress);
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.GnssCoordinates);

				foreach (var qualifier in qualifierList.GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					Assertion.AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(visibleForQualifiers), layout.IsVisible(control, location));
				}
			});
		}

		public static void AssertControlVisibleWhenQualifierIsNotEmpty(PanelLayout layout, ControlReference control, CusGoodsLocation location)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				var qualifierList = new CusGoodsLocationQualifierList();
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.PostcodeAddress);
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.GnssCoordinates);

				foreach (var qualifier in qualifierList.GetAllCodes())
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
