using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(AddInfoCusLineTariffDetail))]
	sealed class AddInfoCusLineTariffDetailTest : EU.Business.Testing.AddInfoCusLineTariffDetailTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var tariffDetail = Factory.New<CusLineTariffDetail>();
			return new AddInfoCusLineTariffDetail(tariffDetail.BZ_NAddInfoInfo);
		}
	}
}
