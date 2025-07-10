using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(CognosCreditorMapping))]
	class CognosCreditorMappingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestExtraInfo()
		{
			CognosCreditorMapping mapping = Factory.New<CognosCreditorMapping>();
			CognosAccGLAccountDescriptorExtraInfo extraInfo = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			extraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			mapping.T7_T9 = extraInfo.PK;
			AssertEquals("Should be loaded from the foreign key", extraInfo.PK, mapping.ExtraInfo.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CognosCreditorMapping result = (CognosCreditorMapping)base.GetNewBusinessObjectForDeleteTest(factory);
			result.ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			return result;
		}
	}
}
