using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(CognosDebtorMapping))]
	class CognosDebtorMappingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestExtraInfo()
		{
			CognosDebtorMapping mapping = Factory.New<CognosDebtorMapping>();
			CognosAccGLAccountDescriptorExtraInfo extraInfo = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			extraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			mapping.T8_T9 = extraInfo.PK;
			AssertEquals("Should be loaded from the foreign key", extraInfo.PK, mapping.ExtraInfo.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CognosDebtorMapping result = (CognosDebtorMapping)base.GetNewBusinessObjectForDeleteTest(factory);
			result.ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			return result;
		}
	}
}
