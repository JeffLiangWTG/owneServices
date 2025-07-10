using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUStateCodeLookups : ZLookups
	{
		public AUStateCodeLookups(AUStateCode parent)
			: base(parent)
		{
		}

		AUStateCode AUStateCode
		{
			get { return (AUStateCode)Parent; }
		}

		public CodeDescriptionPairList AUStateCodeList
		{
			get
			{
				if (list == null && AUStateCode.Parent != null)
				{
					list = AUStateCode.Parent.JI_AUStatesList;
				}
				return list;
			}
		}
		CodeDescriptionPairList list;
	}
}
