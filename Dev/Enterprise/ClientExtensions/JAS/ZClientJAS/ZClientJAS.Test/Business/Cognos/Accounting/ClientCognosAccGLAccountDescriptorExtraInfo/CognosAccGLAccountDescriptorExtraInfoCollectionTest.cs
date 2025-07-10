using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	[TestedType(typeof(CognosAccGLAccountDescriptorExtraInfoCollection))]
	class CognosAccGLAccountDescriptorExtraInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CognosAccGLAccountDescriptorExtraInfoCollection(Factory);
		}
	}
}
