using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(CognosAccGLAccountDescriptorFilterBusinessObject))]
	class CognosAccGLAccountDescriptorFilterBusinessObjectTest : AccGLAccountDescriptorFilterBusinessObjectTest
	{
		public void TestAccountTypes()
		{
			CognosAccGLAccountDescriptorFilterBusinessObject cognosFilter = new CognosAccGLAccountDescriptorFilterBusinessObject(); // FilterBusinessObjectFactory.New(typeof(CognosAccGLAccountDescriptorFilterBusinessObject));
			AccGLAccountDescriptorFilterBusinessObject baseFilter = new AccGLAccountDescriptorFilterBusinessObject(); // FilterBusinessObjectFactory.New(typeof(AccGLAccountDescriptorFilterBusinessObject));
			CodeDescriptionPairList expectedList = baseFilter.AccountTypes;
			expectedList.AddPair(CognosAccGLAccountDescriptor.CognosSubClassificationAccountType, CognosAccGLAccountDescriptor.CognosSubClassificationAccountTypeDescription);
			AssertEquals("Should contain Sub-Classification account type", expectedList.CodesAsString, cognosFilter.AccountTypes.CodesAsString);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CognosAccGLAccountDescriptorFilterBusinessObject();
		}
	}
}
