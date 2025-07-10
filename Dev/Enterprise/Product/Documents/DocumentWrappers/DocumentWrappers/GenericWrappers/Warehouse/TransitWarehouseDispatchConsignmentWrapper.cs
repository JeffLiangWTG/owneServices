using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class TransitWarehouseDispatchConsignmentWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public TransitWarehouseDispatchConsignmentWrapper(WhsItemDispatchConsignment consignment, BusinessObjectFactory factory)
			: base(consignment, factory)
		{
			dispatchConsignmentBO = consignment;
		}

		#endregion

		#region DispatchDriverName

		protected override ZString DispatchDriverNameCore => RelatedDTUIfSingle != null ? RelatedDTUIfSingle.WDH_SignedBy : ZString.Empty;

		WhsItemDispatchTransportationUnit RelatedDTUIfSingle
		{
			get
			{
				if (relatedDTUs == null)
				{
					relatedDTUs = DispatchConsignmentBO != null ? DispatchConsignmentBO.PackageStates.Select(p => p.WPS_WDH_TransitDispatchHeader).Distinct().Where(wdh => !wdh.IsEmpty).ToList() : null;
				}
				if (relatedDTUs == null || relatedDTUs.Count != 1)
				{
					return null;
				}
				return Factory.Load<WhsItemDispatchTransportationUnit>(relatedDTUs.Single());
			}
		}

		List<ZGuid> relatedDTUs;

		#endregion

		#region DispatchDriverSignature

		protected override Image DispatchDriverSignatureCore
		{
			get
			{
				if (RelatedDTUIfSingle == null)
				{
					return null;
				}
				if ((dispatchDriverSignature == null || dispatchDriverSignature.IsDisposed()))
				{
					dispatchDriverSignature = new SignatureDrawer(RelatedDTUIfSingle.WDH_SignedBySignature).Image;
				}
				return dispatchDriverSignature;
			}
		}

		Image dispatchDriverSignature;

		#endregion

		#region Headers

		protected override ZString JobNumberHeadingCore => Res.GetString("0cfed55a-191d-44a7-80c1-d0179dfe15a8", "Dispatch Consignment");

		protected override ZString JobNumberCore => DispatchConsignmentBO != null ? DispatchConsignmentBO.WDC_JobID : ZString.Empty;

		protected override ZString SecondaryHeadingCore => Res.GetString("45012adf-93a0-4217-bb63-13465008a91b", "DCN Reference");

		protected override ZString SecondaryNumberCore => DispatchConsignmentBO != null ? DispatchConsignmentBO.WDC_ConsignmentID : ZString.Empty;

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = DispatchConsignmentBO.Warehouse;
				return warehouse != null
					? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(WarehouseTitle, warehouse, Factory))
					: null;
			}
		}

		#endregion

		#region WarehouseName

		public override LabelValuePairWrapper WarehouseName
		{
			get
			{
				return new LabelValuePairWrapper(WarehouseTitle,
					(DispatchConsignmentBO.Warehouse != null ? DispatchConsignmentBO.Warehouse.WW_WarehouseNameMultilingual : ZString.Empty),
					Factory);
			}
		}

		ZString WarehouseTitle => Res.GetString("TransitWarehouseDispatchConsignmentWrapper|WarehouseTitle", "Warehouse");

		#endregion

		#region ExpectedDispatchTime

		protected override ZDateTime ExpectedDispatchTimeCore => DispatchConsignmentBO.WDC_ExpectedDispatchTime.ToZDateTime();

		#endregion

		#region IsAuthorizedForDispatch

		protected override ZBool IsAuthorizedForDispatchCore => DispatchConsignmentBO.WDC_IsAuthorizedForDispatch;

		#endregion

		#region IsSplit

		protected override ZBool IsSplitCore => DispatchConsignmentBO.IsSplit;

		#endregion

		#region AllowPartialLoading

		protected override ZBool AllowPartialLoadingCore => DispatchConsignmentBO.WDC_AllowPartialLoading;

		#endregion

		#region SecondaryReferenceCore

		protected override LabelValuePairWrapper SecondaryReferenceCore => DispatchConsignmentBO != null
			? new LabelValuePairWrapper(Res.GetString("b2da0c75-b1ac-40bc-ad9b-a8a24a5c91f6", "Reference Number"), DispatchConsignmentBO.WDC_ConsignmentID, Factory)
			: LabelValuePairWrapper.Empty;

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
		{
			var packages = DispatchConsignmentBO.PackageStates.Select(packageState => packageState.Package).Where(package => package != null).ToList();

			return new TransitPackageWrapperCollection(packages, PackageWrapperCollection.PackLevel.First, Factory);
		}

		#endregion

		#region DispatchLoadLists

		protected override WhsItemDispatchLoadListWrapperCollection GetDispatchLoadLists()
		{
			return new WhsItemDispatchLoadListWrapperCollection(DispatchConsignmentBO.PackageStates.Select(p => p.DispatchLoadList).Distinct().Where(d => d != null).ToList(), Factory);
		}

		#endregion

		#region TotalInnerPackLines

		protected override ZShort TotalInnerPackLinesCore => (ZShort)DispatchConsignmentBO.PackageStates.Where(pkg => pkg.Package.KP_KPH_PackageHeader == ZGuid.Empty
			&& pkg.Package.KP_KP_TopHandlingUnitPackage != ZGuid.Empty
			&& pkg.TopHandlingUnit.WPS_UnitType == "HU")
			.Sum(pkg => pkg.Package.KP_PackageQty);

		#endregion

		#region TotalInners

		protected override ZShort TotalInnersCore => (ZShort)DispatchConsignmentBO.PackageStates.Where(pkg => pkg.Package.KP_KP_TopHandlingUnitPackage != ZGuid.Empty
			&& pkg.TopHandlingUnit.WPS_UnitType == "HU")
			.Sum(pkg => pkg.Package.KP_PackageQty);

		#endregion

		#region TotalOverpacks

		protected override ZShort TotalOverpacksCore => (ZShort)DispatchConsignmentBO.PackageStates.Count(pkg => pkg.WPS_UnitType == "OVP" && pkg.Package.KP_KP_ParentPackage == ZGuid.Empty);

		#endregion

		#region Destination

		public override PlaceAndDateWrapper Destination => destination ?? (destination = new PlaceAndDateWrapper(DispatchConsignmentBO.WDC_RL_NKDestination, ZDateTime.Empty, ZDateTime.Empty, Factory));

		PlaceAndDateWrapper destination;

		#endregion

		#region CompleteTime

		protected override ZDateTime CompleteTimeCore => DispatchConsignmentBO.WDC_CompleteTime.ToLocalZDateTime();

		#endregion

		#region ClientRequestedBillToParty

		protected override OrganisationWrapper ClientRequestedBillToPartyCore => new OrganisationWrapper(OrganisationUsageType.BillToParty, DispatchConsignmentBO.ClientRequestedBillToPartyDocAddress, Factory);

		#endregion

		#region Implementation

		WhsItemDispatchConsignment DispatchConsignmentBO => dispatchConsignmentBO ?? (dispatchConsignmentBO = (WhsItemDispatchConsignment)WrappedBO);
		WhsItemDispatchConsignment dispatchConsignmentBO;

		#endregion
	}
}
