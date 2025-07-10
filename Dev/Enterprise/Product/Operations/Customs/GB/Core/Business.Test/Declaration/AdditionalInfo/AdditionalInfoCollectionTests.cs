using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	public class AdditionalInfoCollectionTests : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
	{
		protected override Customs.Business.CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new AdditionalInfoCollection(declaration);
		}

		public void TestIBusinessObjectCollectionImplements()
		{
			var additionalInfos = GetCusSupportingInfoCollection();
			((Integration.Customs.GB.IAdditionalInfoCollection)additionalInfos).AddNew();
			AssertSame(additionalInfos[0], ((Integration.Customs.GB.IAdditionalInfoCollection)additionalInfos)[0]);
		}
	}
}
