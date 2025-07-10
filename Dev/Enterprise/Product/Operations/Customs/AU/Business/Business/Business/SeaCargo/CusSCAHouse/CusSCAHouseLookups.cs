using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseLookups : Customs.Business.CusSCAHouseLookups
	{
		public CusSCAHouseLookups(Customs.Business.BaseCusSCAHouse parent)
			: base(parent)
		{
		}

		public new CusSCAHouse Parent
		{
			get { return (CusSCAHouse)base.Parent; }
		}

		public CodeDescriptionPairList MethodsOfPayment
		{
			get { return MethodsOfPaymentCore(); }
		}

		protected CodeDescriptionPairList MethodsOfPaymentCore()
		{
			return Factory.GetCachedValue<CMRMethodsOfPayment>();
		}

		public CodeDescriptionPairList StatusList
		{
			get { return Factory.GetCachedValue<CMRAllStatuses>(); }
		}

		public CodeDescriptionPairList ShipmentStatusList
		{
			get { return Factory.GetCachedValue<CMRShipmentStatuses>(); }
		}

		public OrgHeaderCollection Consignor_List
		{
			get
			{
				if (Parent != null)
				{
					if (!Parent.CA_IsMasterHouse)
					{
						ConsignorCollection result = new ConsignorCollection(Factory);
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", Parent.CA_RL_NK_PortOfOrigin));
						return result;
					}
					else
					{
						ForwarderCollection result = new ForwarderCollection(Factory);
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", Parent.CA_RL_NK_PortOfOrigin));
						return result;
					}
				}
				else
				{
					return new OrgHeaderCollection(Factory);
				}
			}
		}

		public OrgHeaderCollection Consignee_List
		{
			get
			{
				if (Parent != null)
				{
					if (!Parent.CA_IsMasterHouse)
					{
						ConsigneeCollection result = new ConsigneeCollection(Factory);
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", Parent.CA_RL_NK_PortOfDestination));
						return result;
					}
					else
					{
						ForwarderCollection result = new ForwarderCollection(Factory);
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", Parent.CA_RL_NK_PortOfDestination));
						return result;
					}
				}
				else
				{
					return new OrgHeaderCollection(Factory);
				}
			}
		}

		#region NotifyPartyCountryCodes

		public RefCountryCollection NotifyPartyCountryCodes
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion
	}
}
