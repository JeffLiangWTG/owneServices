using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCLREGInfoProviderAddInfoLookups : AutoAUCLREGInfoProviderAddInfoLookups
	{
		public AUCLREGInfoProviderAddInfoLookups(AutoAUCLREGInfoProviderAddInfo parent) : base(parent)
		{
		}

		new CLREGInfoProviderAddInfo Parent
		{
			get { return (CLREGInfoProviderAddInfo)base.Parent; }
		}

		public CodeDescriptionPairList GenderList
		{
			get { return Factory.GetCachedValue<CMRGenderCodes>(); }
		}

		public RefUNLOCOCollection RefUNLOCOs
		{
			get { return refUNLOCOs ?? (refUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection refUNLOCOs;

		public OrgAddressDependentCollection Addresses
		{
			get { return Parent.CLREGInfoProvider.OrganizationAddresses; }
		}

		public OrgContactDependentCollection Contacts
		{
			get { return Parent.CLREGInfoProvider.Contacts; }
		}

		public CodeDescriptionPairList OA_StateListForBusinessAddress
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent.ZA_BsnPort);
				if (port != null)
				{
					list = new OrgCodeLists().State_List(port);
				}
				return list;
			}
		}

		public CodeDescriptionPairList OA_StateListForPostalAddress
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent.ZA_PostPort);
				if (port != null)
				{
					list = new OrgCodeLists().State_List(port);
				}
				return list;
			}
		}

		public CodeDescriptionPairList ABNNominatedClientTypeList
		{
			get
			{
				return Factory.GetCachedValue("ABNNominatedClientTypeList", () => new AUCLREGInfoProviderABNNominatedClientTypeList());
			}
		}

		public CodeDescriptionPairList CACTypeList
		{
			get
			{
				return Factory.GetCachedValue("CACTypeList", () => new AUCLREGInfoProviderCACTypeList());
			}
		}
	}
}
