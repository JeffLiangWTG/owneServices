using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	internal class CSSAirImportConverterTest : CSSConverterTest<CSSAirImportConverter>
	{
		protected override IList<BusinessObject> GetPopulatedBizos()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			result.Add(TestHelper.GetCusHawb());
			return result;
		}

		protected override CSSAirImportConverter NewCSSConverter()
		{
			return new CSSAirImportConverter(TestHelper.Notifications, TestHelper.SharedFactory);
		}
	}
}
