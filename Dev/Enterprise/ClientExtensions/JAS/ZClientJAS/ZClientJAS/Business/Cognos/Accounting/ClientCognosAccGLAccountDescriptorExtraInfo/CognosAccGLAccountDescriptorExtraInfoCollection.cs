
using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosAccGLAccountDescriptorExtraInfoCollection : BusinessObjectCollection<CognosAccGLAccountDescriptorExtraInfo>
	{
		public CognosAccGLAccountDescriptorExtraInfoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CognosAccGLAccountDescriptorExtraInfoCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
