
using Enterprise.Client.JAS.Business.Cognos;

using Enterprise.MasterFiles.Module;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Module
{
	public class CognosAccGLAccountDescriptorFilterBusinessObject : AccGLAccountDescriptorFilterBusinessObject
	{
		public CognosAccGLAccountDescriptorFilterBusinessObject()
		{
		}

		public override CodeDescriptionPairList AccountTypes
		{
			get
			{
				CodeDescriptionPairList result = base.AccountTypes;
				result.AddPair(CognosAccGLAccountDescriptor.CognosSubClassificationAccountType, CognosAccGLAccountDescriptor.CognosSubClassificationAccountTypeDescription);

				return result;
			}
		}
	}
}
