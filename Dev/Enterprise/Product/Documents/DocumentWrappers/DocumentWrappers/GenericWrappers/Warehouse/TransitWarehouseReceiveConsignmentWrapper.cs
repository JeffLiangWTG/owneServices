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
	public class TransitWarehouseReceiveConsignmentWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public TransitWarehouseReceiveConsignmentWrapper(WhsItemReceiveConsignment consignment, BusinessObjectFactory factory)
			: base(consignment, factory)
		{
		}

		#endregion

		#region Headers

		protected override ZString JobNumberHeadingCore => Res.GetString("54d07ee6-ad5c-4d2a-99c6-be7962613e64", "Receive Consignment");

		protected override ZString JobNumberCore => ReceiveConsignmentBO != null ? ReceiveConsignmentBO.WRC_JobID : ZString.Empty;

		protected override ZString SecondaryHeadingCore => Res.GetString("1e85254d-6e39-41de-8ebd-a7a0ae9ba28e", "RCN Reference");

		protected override ZString SecondaryNumberCore => ReceiveConsignmentBO != null ? ReceiveConsignmentBO.WRC_ConsignmentID : ZString.Empty;

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
		{
			var packages = ReceiveConsignmentBO.PackageStates.Select(packageState => packageState.Package).Where(package => package != null).ToList();
			return new TransitPackageWrapperCollection(packages, PackageWrapperCollection.PackLevel.First, Factory);
		}

		#endregion

		#region ReceiveDriverName

		protected override ZString ReceiveDriverNameCore => RelatedRTUIfSingle != null ? RelatedRTUIfSingle.WRH_SignedBy : ZString.Empty;

		WhsItemReceiveTransportationUnit RelatedRTUIfSingle
		{
			get
			{
				if (relatedRTUs == null)
				{
					relatedRTUs = ReceiveConsignmentBO != null ? ReceiveConsignmentBO.PackageStates.Select(p => p.WPS_WRH_TransitReceiveHeader).Distinct().Where(wrh => !wrh.IsEmpty).ToList() : null;
				}
				if (relatedRTUs == null || relatedRTUs.Count != 1)
				{
					return null;
				}
				return Factory.Load<WhsItemReceiveTransportationUnit>(relatedRTUs.Single());
			}
		}

		List<ZGuid> relatedRTUs;

		#endregion

		#region ReceiveDriverSignature

		protected override Image ReceiveDriverSignatureCore
		{
			get
			{
				if (RelatedRTUIfSingle == null)
				{
					return null;
				}
				if (receiveDriverSignature == null || receiveDriverSignature.IsDisposed())
				{
					receiveDriverSignature = new SignatureDrawer(RelatedRTUIfSingle.WRH_SignedBySignature).Image;
				}
				return receiveDriverSignature;
			}
		}

		Image receiveDriverSignature;

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = ReceiveConsignmentBO.Warehouse;
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
					(ReceiveConsignmentBO.Warehouse != null ? ReceiveConsignmentBO.Warehouse.WW_WarehouseNameMultilingual : ZString.Empty),
					Factory);
			}
		}

		ZString WarehouseTitle => Res.GetString("TransitWarehouseReceiveConsignmentWrapper|WarehouseTitle", "Warehouse");

		#endregion

		#region SecondaryReferenceCore

		protected override LabelValuePairWrapper SecondaryReferenceCore => ReceiveConsignmentBO != null
			? new LabelValuePairWrapper(Res.GetString("b2da0c75-b1ac-40bc-ad9b-a8a24a5c91f6", "Reference Number"), ReceiveConsignmentBO.WRC_ConsignmentID, Factory)
			: LabelValuePairWrapper.Empty;

		#endregion

		#region CustomsStatus

		protected override CodeAndDescriptionWrapper CustomsStatusCore => new CodeAndDescriptionWrapper(ReceiveConsignmentBO.WRC_CustomsStatus, new TransitWarehouseCustomsStatuses(), Factory);

		#endregion

		#region Destination

		public override PlaceAndDateWrapper Destination => destination ?? (destination = new PlaceAndDateWrapper(ReceiveConsignmentBO.WRC_RL_NKDestination, ZDateTime.Empty, ZDateTime.Empty, Factory));

		PlaceAndDateWrapper destination;

		#endregion

		#region ReceiveTransportationUnits

		protected override WhsItemReceiveTransportationUnitWrapperCollection GetReceiveTransportationUnits()
		{
			if (ReceiveConsignmentBO == null)
			{
				return new WhsItemReceiveTransportationUnitWrapperCollection(Factory);
			}

			var rtuPKs = ReceiveConsignmentBO.PackageStates.Select(p => p.WPS_WRH_TransitReceiveHeader).Where(wrh => !wrh.IsEmpty).Distinct();
			var asnPKs = ReceiveConsignmentBO.PackageStates.Where(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked).Select(p => p.WPS_WRP_ReceiveExpectedPacking).Where(wrp => !wrp.IsEmpty).Distinct();

			return new WhsItemReceiveTransportationUnitWrapperCollection(rtuPKs, asnPKs, Factory);
		}

		#endregion

		#region FinalisedDate

		protected override LabelValuePairWrapper FinalisedDateCore => (ReceiveConsignmentBO != null)
			? new LabelValuePairWrapper(Res.GetString("792e7493-d04e-4174-a3e3-973da46d233e", "Finalized Date"), ReceiveConsignmentBO.WRC_CompleteTime.ToZDateTime(), Factory)
			: LabelValuePairWrapper.Empty;

		#endregion

		#region WarehouseExpectedArrivalTime

		protected override ZDateTime GetWarehouseExpectedArrivalTime => ReceiveConsignmentBO.WRC_ExpectedArrivalTime;

		#endregion

		#region ExpectedDispatchTime

		protected override ZDateTime ExpectedDispatchTimeCore => ReceiveConsignmentBO.WRC_ExpectedDispatchTime;

		#endregion

		#region ClientRequestedBillToParty

		protected override OrganisationWrapper ClientRequestedBillToPartyCore => new OrganisationWrapper(OrganisationUsageType.BillToParty, ReceiveConsignmentBO.ClientRequestedBillToPartyDocAddress, Factory);

		#endregion

		#region Implementation

		WhsItemReceiveConsignment ReceiveConsignmentBO => receiveConsignmentBO ?? (receiveConsignmentBO = (WhsItemReceiveConsignment)WrappedBO);
		WhsItemReceiveConsignment receiveConsignmentBO;

		#endregion
	}
}
