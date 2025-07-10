
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Utilities
{
	public class JASUnitConverter
	{
		public ZString GetJASPackageUnitFromShipmentOuterPacksType(ZString outerPacksPackType)
		{
			ZString result = "PLT";

			switch (outerPacksPackType)
			{
				case Constants.PkgUnit.Basket:
					result = "BSK";
					break;
				case Constants.PkgUnit.Box:
					result = "BOX";
					break;
				case Constants.PkgUnit.Case:
					result = "CAS";
					break;
				case Constants.PkgUnit.Carton:
					result = "CTN";
					break;
				case Constants.PkgUnit.Container:
					result = "CBC";
					break;
				case Constants.PkgUnit.Crate:
					result = "CRT";
					break;
				case Constants.PkgUnit.Cylinder:
					result = "CYL";
					break;
				case Constants.PkgUnit.Drum:
					result = "DRM";
					break;
				case Constants.PkgUnit.Keg:
					result = "KEG";
					break;
				case Constants.PkgUnit.Package:
					result = "PKG";
					break;
				case Constants.PkgUnit.Pail:
					result = "PAL";
					break;
				case Constants.PkgUnit.Pallet:
					result = "PLT";
					break;
				case Constants.PkgUnit.Piece:
					result = "PCS";
					break;
				case Constants.PkgUnit.Reel:
					result = "REL";
					break;
				case Constants.PkgUnit.Roll:
					result = "ROL";
					break;
				case Constants.PkgUnit.Sheet:
					result = "SHT";
					break;
				case Constants.PkgUnit.Skid:
					result = "SKD";
					break;
				case Constants.PkgUnit.Unit:
					result = "UNT";
					break;

				case Constants.PkgUnit.BaleCompressed:
				case Constants.PkgUnit.BaleUncompressed:
					result = "BLE";
					break;

				case Constants.PkgUnit.Bag:
				case Constants.PkgUnit.BulkBag:
					result = "BAG";
					break;
			}

			return result;
		}

		public ZString GetShipmentOuterPacksTypeFromJASPackageUnit(ZString jASPackageUnit)
		{
			ZString result = Constants.PkgUnit.Pallet;

			switch (jASPackageUnit)
			{
				case "BSK":
					result = Constants.PkgUnit.Basket;
					break;
				case "BOX":
					result = Constants.PkgUnit.Box;
					break;
				case "CAS":
					result = Constants.PkgUnit.Case;
					break;
				case "CTN":
					result = Constants.PkgUnit.Carton;
					break;
				case "CBC":
					result = Constants.PkgUnit.Container;
					break;
				case "CRT":
					result = Constants.PkgUnit.Crate;
					break;
				case "CYL":
					result = Constants.PkgUnit.Cylinder;
					break;
				case "DRM":
					result = Constants.PkgUnit.Drum;
					break;
				case "KEG":
					result = Constants.PkgUnit.Keg;
					break;
				case "PKG":
					result = Constants.PkgUnit.Package;
					break;
				case "PAL":
					result = Constants.PkgUnit.Pail;
					break;
				case "PLT":
					result = Constants.PkgUnit.Pallet;
					break;
				case "PCS":
					result = Constants.PkgUnit.Piece;
					break;
				case "REL":
					result = Constants.PkgUnit.Reel;
					break;
				case "ROL":
					result = Constants.PkgUnit.Roll;
					break;
				case "SHT":
					result = Constants.PkgUnit.Sheet;
					break;
				case "SKD":
					result = Constants.PkgUnit.Skid;
					break;
				case "UNT":
					result = Constants.PkgUnit.Unit;
					break;
				case "BLE":
					result = Constants.PkgUnit.BaleUncompressed;
					break;
				case "BAG":
					result = Constants.PkgUnit.Bag;
					break;
			}

			return result;
		}

		public ZString GetJASTypeOfServiceFromContainerAndDeliveryMode(ForwardingContainer container)
		{
			ZString result = "";

			if (container != null)
			{
				switch (container.JC_DeliveryMode)
				{
					case Core.Constants.DeliveryModes.Codes.CFS_CFS:
						result = "CS";
						break;
					case Core.Constants.DeliveryModes.Codes.CY_CY:
						result = "CY";
						break;
				}

				if (result.IsEmpty && container.Consol != null)
				{
					switch (container.Consol.JK_ConsolMode)
					{
						case Core.Constants.ContainerModes.BreakBulk:
							result = "BB";
							break;
						case Core.Constants.ContainerModes.RollOnRollOff:
							result = "RR";
							break;
					}
				}
			}

			return result;
		}

		public void AssignJASTypeOfServiceToContainerAndDeliveryMode(ZString typeOfService, ForwardingContainer container)
		{
			if (container != null)
			{
				switch (typeOfService)
				{
					case "CS":
						container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
						break;

					case "CY":
						container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
						break;

					case "BB":
						if (container.Consol != null)
						{
							container.Consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
						}
						break;

					case "RR":
						if (container.Consol != null)
						{
							container.Consol.JK_ConsolMode = Core.Constants.ContainerModes.RollOnRollOff;
						}
						break;
				}
			}
		}
		public ZString GetJASContainerType(RefContainer refContainer)
		{
			ZString result = "";

			if (refContainer != null)
			{
				result = GetMappedContainerTypeFromContainer(refContainer);
				if (result.IsEmpty)
				{
					result = refContainer.RC_Code;
				}
			}

			return result;
		}

		public RefContainer GetRefContainerFromJASContainerType(BusinessObjectFactory factory, ZString jasContainerType)
		{
			return GetMappedContainerTypeFromForeignCode(factory, jasContainerType) ?? factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, jasContainerType);
		}

		#region Implementation

		ZString GetMappedContainerTypeFromContainer(RefContainer container)
		{
			ZString result = "";

			JASOrgHeader jASWWOrganisation = (JASOrgHeader)container.Factory.Load(typeof(JASOrgHeader), JASDataRegistry.Instance.JASWWOrganisationPK);
			if (jASWWOrganisation != null)
			{
				ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, container.PK);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, jASWWOrganisation.PK);

				return jASWWOrganisation.Factory.LoadTop1<OrgPatternMatchOverride>(filter)?.OO_ForeignCode ?? ZString.Empty;
			}

			return result;
		}

		RefContainer GetMappedContainerTypeFromForeignCode(BusinessObjectFactory factory, ZString jASContainerType)
		{
			RefContainer result = null;

			JASOrgHeader jASWWOrganisation = (JASOrgHeader)factory.Load(typeof(JASOrgHeader), JASDataRegistry.Instance.JASWWOrganisationPK);
			if (jASWWOrganisation != null)
			{
				ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, jASContainerType);

				var orgPattern = jASWWOrganisation.Factory.LoadTop1<OrgPatternMatchOverride>(filter);
				if (orgPattern != null)
				{
					result = factory.Load<RefContainer>(orgPattern.OO_LocalGuid);
				}
			}

			return result;
		}

		#endregion
	}
}
