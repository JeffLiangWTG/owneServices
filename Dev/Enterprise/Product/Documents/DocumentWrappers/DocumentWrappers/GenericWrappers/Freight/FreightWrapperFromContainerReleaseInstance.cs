using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	internal sealed class FreightWrapperFromContainerReleaseInstance : FreightWrapper
	{
		public FreightWrapperFromContainerReleaseInstance(ReleaseInstance instance, BusinessObjectFactory factory)
			: base(instance, factory)
		{
			this.instance = instance;
		}
		readonly ReleaseInstance instance;

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return new CodeAndDescriptionWrapper(instance.ReleaseType, instance.Lookups.ReleaseType_List, Factory);
		}

		protected sealed override ZString GetBookingReference()
		{
			return instance.Header.Shipment.JS_CFSReference;
		}

		protected override ZString GetGoodsDescription()
		{
			return instance.Header.Shipment.JS_GoodsDescription;
		}

		protected override OrganisationWrapper GetBookingParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.BookingParty, instance.Header.Shipment.BookingPartyDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var wrapper = new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, instance.Header.Shipment.ReceivingForwarderAddress, Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var wrapper = new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, instance.Header.Shipment.SendingForwarderAddress, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor, instance.Header.Shipment.ConsignorDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, instance.Header.Shipment.ConsigneeDocumentaryAddress, Factory);
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("57d99f52-0e3f-4ed0-969a-e30ae5ba4c50", "Release Number");
		}

		protected override ZString GetJobNumber()
		{
			return instance.ReleaseNumber;
		}

		protected override ZString GetSecondaryHeading()
		{
			return Res.GetString("c8f7005c-95c7-4f91-aa1f-5659036491c5", "Shipment Number");
		}

		protected override ZString GetSecondaryNumber()
		{
			return instance.Header.Shipment.JS_UniqueConsignRef;
		}

		protected override OrganisationWrapper GetPrincipal()
		{
			return new OrganisationWrapper(OrganisationUsageType.Principal, instance.Header.Shipment.Principal, ContactType.ShippingLine, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(instance.Header.Shipment, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(instance, Factory);
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return instance.Shipment.PK;
		}

		protected override ClientAndAgentBrandingBusinessObject AlternativeBranding
		{
			get { return ((IDocumentSupportable)instance).DocumentSupporter.GetAlternativeBranding(); }
		}

		#region IBODocDataProvider Members

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return instance.Shipment; }
		}

		protected override BusinessObject ParentBusinessObject
		{
			get { return instance.Shipment; }
		}

		#endregion

		#region Wrappers

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			var wrapper = new ShipmentExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, instance.Header.Shipment.SendingAgentAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			var wrapper = new OrganisationWrapper(OrganisationUsageType.ImportAgent, instance.Header.Shipment.ReceivingAgentAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		#endregion

		#region Collections

		protected override NoteWrapperCollection GetTextNotes()
		{
			var noteTextWrapper = new NoteTextWrapper(instance.Header.ContainerReleaseNote, ResString.GetMultilingualString("464ea4b0-8460-4b9e-9654-28ee6714fd39", "Container Release Note"), ZDateTime.Empty, Factory);
			var noteWrapperCollection = new NoteWrapperCollection(Factory);
			noteWrapperCollection.Add(noteTextWrapper);
			return noteWrapperCollection;
		}

		#endregion
	}
}
