using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	internal class CSSAirExportConverterTest : CSSConverterTest<CSSAirExportConverter>
	{
		protected override IList<BusinessObject> GetPopulatedBizos()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			result.Add(TestHelper.GetJobDec());
			result.Add(TestHelper.GetShipment());
			return result;
		}

		protected override CSSAirExportConverter NewCSSConverter()
		{
			return new CSSAirExportConverter(TestHelper.Notifications, TestHelper.SharedFactory);
		}
	}
}
