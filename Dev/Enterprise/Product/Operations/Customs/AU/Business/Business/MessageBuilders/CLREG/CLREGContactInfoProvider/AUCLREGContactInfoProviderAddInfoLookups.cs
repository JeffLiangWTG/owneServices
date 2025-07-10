using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCLREGContactInfoProviderAddInfoLookups : AutoAUCLREGContactInfoProviderAddInfoLookups
	{
		public AUCLREGContactInfoProviderAddInfoLookups(AutoAUCLREGContactInfoProviderAddInfo parent) : base(parent)
		{
		}

		new CLREGContactInfoProviderAddInfo Parent
		{
			get { return (CLREGContactInfoProviderAddInfo)base.Parent; }
		}

		public RefUNLOCOCollection RefUNLOCOs
		{
			get { return refUNLOCOs ?? (refUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection refUNLOCOs;

		public CodeDescriptionPairList OA_StateListForContactAddress
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent.ZA_ContPort);
				if (port != null)
				{
					list = new OrgCodeLists().State_List(port);
				}
				return list;
			}
		}

		public CodeDescriptionPairList OA_StateListForContactPostalAddress
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent.ZA_ContPostPort);
				if (port != null)
				{
					list = new OrgCodeLists().State_List(port);
				}
				return list;
			}
		}
	}
}
