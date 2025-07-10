using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBLookups : Customs.Business.CusHAWBLookups
	{
		public CusHAWBLookups(CusHAWBBase parent)
			: base(parent)
		{
		}

		public new CusHAWBBase Parent
		{
			get { return (CusHAWBBase)base.Parent; }
		}

		public virtual ReadOnlyCodeDescriptionPairList ShipmentTypeList
		{
			get { return Env.Registry.AUCustoms.AirCargoShipmentType; }
		}

		public virtual CodeDescriptionPairList PrepaidCollectList
		{
			get { return Factory.GetCachedValue<CMRMethodsOfPayment>(); }
		}

		public virtual CodeDescriptionPairList PrepaidCollectListForValidation
		{
			get { return PrepaidCollectList; }
		}

		public override ICodeDescriptionPairList UnitOfWeightList
		{
			get
			{
				return Factory.GetCachedValue("AUCusHAWBLookups.UnitOfWeightList", () => new AUCusHAWBLookupsUnitOfWeightList());
			}
		}

		public OrganisationsFindBoxCollection ConsignorList
		{
			get
			{
				OrganisationsFindBoxCollection result = null;

				if (Parent != null)
				{
					if (Parent.CS_IsMasterHouse)
					{
						result = new ForwarderCollection(Factory);
					}
					else
					{
						result = (OrganisationsFindBoxCollection)Consignors;
					}

					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", Parent.CS_RL_NKOrigin));
				}

				return result;
			}
		}

		public OrgHeaderCollection ConsigneeList
		{
			get
			{
				OrgHeaderCollection result = null;

				if (Parent != null)
				{
					if (Parent.CS_IsMasterHouse)
					{
						result = new ForwarderCollection(Factory);
					}
					else
					{
						result = Consignees;
					}

					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", Parent.CS_RL_NKDestination));
				}

				return result;
			}
		}

		public override OrgHeaderCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public override OrgHeaderCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public RefCountryCollection ConsignorCountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public RefCountryCollection ConsigneeCountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public RefUNLOCOCollection OriginList
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public RefUNLOCOCollection DestinationList
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public CodeDescriptionPairList CMRCurrencyList
		{
			get
			{
				return Factory.GetCachedValue("CMRCurrencyList", () => new CusHAWBLookupsCMRCurrencyList());
			}
		}

		public RefCountryStatesCollection ConsignorStateList
		{
			get { return new RefCountryStatesCollection(Factory); }
		}

		public RefCountryStatesCollection ConsigneeStateList
		{
			get { return new RefCountryStatesCollection(Factory); }
		}

		public ModuleMAWBCollection MasterList
		{
			get { return new ModuleMAWBCollection(Factory); }
		}

		public CMRStatuses BaseStatusList
		{
			get { return Factory.GetCachedValue<CMRStatuses>(); }
		}

		#region ContactList

		public OrgContactDependentCollection ConsignorContactList
		{
			get
			{
				OrgContactDependentCollection collection = new OrgContactDependentCollection(Parent.Consignor, Factory);
				collection.Load();
				return collection;
			}
		}

		public OrgContactDependentCollection ConsigneeContactList
		{
			get
			{
				OrgContactDependentCollection collection = new OrgContactDependentCollection(Parent.Consignee, Factory);
				collection.Load();
				return collection;
			}
		}

		#endregion

		#region RefServiceLevelList

		public RefServiceLevelCollection RefServiceLevelList
		{
			get { return new RefServiceLevelCollection(Factory); }
		}

		#endregion

		#region CommercialStatusList

		public ICodeDescriptionPairList CommercialStatusList
		{
			get { return GetNewCommercialStatusList(); }
		}

		protected virtual ICodeDescriptionPairList GetNewCommercialStatusList()
		{
			return Env.Registry.AUCustoms.AirCargoCommercialStatus;
		}

		#endregion
	}
}
