using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(AddInfoCusLineTariffDetail))]
	public class AddInfoCusLineTariffDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var tariffDetail = Factory.New<CusLineTariffDetail>();
			return new AddInfoCusLineTariffDetail(tariffDetail.BZ_NAddInfoInfo);
		}
	}
}

