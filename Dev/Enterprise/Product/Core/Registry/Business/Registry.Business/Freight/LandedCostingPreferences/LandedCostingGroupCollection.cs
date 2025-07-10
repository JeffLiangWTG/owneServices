using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class LandedCostingGroupCollection : RegistryBusinessObjectCollectionTemplate
	{
		public LandedCostingGroupCollection()
		{
		}

		public LandedCostingGroupCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new LandedCostingGroup this[int i]
		{
			get { return (LandedCostingGroup)Elements[i]; }
		}

		public new LandedCostingGroup AddNew()
		{
			return (LandedCostingGroup)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LandedCostingGroup(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LandedCostingGroupCollection(fallbackLevel, factory);
		}

		#region Get Landed Costing Group from ChargeCode/ChargeGroup

		public string GetLandedCostingGroupNameFromID(ZByte groupID)
		{
			foreach (LandedCostingGroup lCGroup in this)
			{
				if (lCGroup.GroupID == groupID)
				{
					return lCGroup.GroupName;
				}
			}
			return "";
		}

		public string GetLandedCostingGroupDistributionCodeFromID(ZByte groupID)
		{
			foreach (LandedCostingGroup lCGroup in this)
			{
				if (lCGroup.GroupID == groupID)
				{
					return lCGroup.CostDistributionCode;
				}
			}
			return "";
		}

		public LandedCostingGroup GetLandedCostingGroupFromChargeCode(BusinessObject accChargeCode, LandedCostingGroup excludeFromSearch)
		{
			if (accChargeCode != null)
			{
				ZString aC_ChargeGroup = (ZString)accChargeCode[AccChargeCodeSchema.Constants.AC_ChargeGroup];

				foreach (LandedCostingGroup landedCostingGroup in this)
				{
					if (excludeFromSearch == null || landedCostingGroup != excludeFromSearch)
					{
						foreach (ChargeGroupAndChargeCode charge in landedCostingGroup.Charges)
						{
							bool chargeGroupMatchAndNoExclusion = (charge.ChargeGroupCode == aC_ChargeGroup) &&
								(!charge.IsExcluded || charge.ChargeCodePK != accChargeCode.PK);

							bool chargeCodeInclusionMatch = (charge.ChargeGroupCode != aC_ChargeGroup) &&
								!charge.IsExcluded && charge.ChargeCodePK == accChargeCode.PK;

							if (chargeGroupMatchAndNoExclusion || chargeCodeInclusionMatch)
							{
								return landedCostingGroup;
							}
						}
					}
				}
			}

			return null;
		}

		public LandedCostingGroup GetLandedCostingGroupFromChargeGroup(string chargeGroupCode, LandedCostingGroup excludeFromSearch)
		{
			foreach (LandedCostingGroup landedCostingGroup in this)
			{
				if (excludeFromSearch == null || landedCostingGroup != excludeFromSearch)
				{
					foreach (ChargeGroupAndChargeCode charge in landedCostingGroup.Charges)
					{
						if (charge.ChargeGroupCode == chargeGroupCode)
						{
							return landedCostingGroup;
						}
					}
				}
			}

			return null;
		}

		#endregion
	}
}
