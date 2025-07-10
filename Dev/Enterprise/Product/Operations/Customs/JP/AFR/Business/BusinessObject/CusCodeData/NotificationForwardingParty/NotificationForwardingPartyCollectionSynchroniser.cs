using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class NotificationForwardingPartyCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public NotificationForwardingPartyCollectionSynchroniser(ForwardingShipment source, JPAFRBills destination)
			: base(source, destination)
		{
		}

		protected new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		protected new JPAFRBills Destination
		{
			get { return (JPAFRBills)base.Destination; }
		}

		JPAFRHeader DestinationHeader
		{
			get { return Destination.Header; }
		}

		JobDocAddress SourceConsigneeAddress
		{
			get { return Source.ConsigneeDocumentaryAddress; }
		}

		GlbCompany DestinationParentCompany
		{
			get
			{
				var destinationHeader = DestinationHeader;
				return (destinationHeader == null || destinationHeader.Company == null) ? GlbCompany.CurrentCompany : destinationHeader.Company;
			}
		}

		#region BusinessObjectCollectionSynchroniser

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			return null;
		}

		protected override void HookElementSynchronisers()
		{
			var consignee = Destination.Consignee;
			var sourceAddress = SourceConsigneeAddress;
			if (Source != null && !Source.IsDeleted && sourceAddress != null)
			{
				NotificationForwardingPartySynchroniser existingSynchroniser = null;
				if ((existingSynchroniser = ElementSynchronisers.FindMatchingSource<NotificationForwardingPartySynchroniser>(sourceAddress)) == null)
				{
					if (NotificationForwardingPartySynchroniser.GetRelatedCCDCodeFromAddress(sourceAddress, DestinationParentCompany) != null)
					{
						var newNFP = Destination.NotificationForwardingParties.AddNew();
						var newSynchroniser = new NotificationForwardingPartySynchroniser(newNFP, Source.ConsigneeDocumentaryAddress, DestinationHeader);
						ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(newSynchroniser, IsEnabled, DetectEnabled);
					}
				}
				else
				{
					existingSynchroniser.SetEnabled(IsEnabled, DetectEnabled);
				}
			}
		}

		protected void ReHookElementSynchronisers()
		{
			var sourceAddress = SourceConsigneeAddress;
			if (sourceAddress != null && !sourceAddress.IsDeleted && sourceAddress.HasRealOrganisation
				&& Destination.Header != null)
			{
				var targetCCDCode = NotificationForwardingPartySynchroniser.GetRelatedCCDCodeFromAddress(sourceAddress, DestinationParentCompany);
				NotificationForwardingPartySynchroniser synchroniser = null;
				if ((synchroniser = ElementSynchronisers.FindMatchingSource<NotificationForwardingPartySynchroniser>(sourceAddress)) != null)
				{
					if (targetCCDCode != null)
					{
						synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					}
					else
					{
						var destinationNFP = synchroniser.Destination;
						destinationNFP.Delete();
						if (destinationNFP.IsDeleted)
						{
							ElementSynchronisers.Remove(synchroniser);
						}
					}
				}
				else
				{
					if (targetCCDCode != null)
					{
						var targetNFP = Destination.NotificationForwardingParties.AddNew();
						var newSynchroniser = new NotificationForwardingPartySynchroniser(targetNFP, sourceAddress, Destination.Header);
						ElementSynchronisers.Add(newSynchroniser, IsEnabled, DetectEnabled);
						newSynchroniser.Synchronise();
					}
				}
			}
		}

		#endregion

		protected override void HookEvents()
		{
			UnHookEvents();
			HookOnSourceAddress();
		}

		protected override void UnHookEvents()
		{
			var consignee = SourceConsigneeAddress;
			if (consignee != null)
			{
				consignee.E2_OA_AddressInfo.ValueChanged -= RehookElementSynchroniserIfNeeded;
			}
		}

		void RehookElementSynchroniserIfNeeded(object sender, System.EventArgs e)
		{
			ReHookElementSynchronisers();
		}

		void HookOnSourceAddress()
		{
			var consignee = SourceConsigneeAddress;
			if (consignee != null && !consignee.IsDeleted)
			{
				consignee.E2_OA_AddressInfo.ValueChanged += RehookElementSynchroniserIfNeeded;
			}
		}

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && Destination.ShouldSynchronise)
			{
				DeleteUnlinkedNotificationForwardingParties();
			}
		}

		void DeleteUnlinkedNotificationForwardingParties()
		{
			var destinationNFPList = new List<NotificationForwardingParty>(Destination.NotificationForwardingParties);
			foreach (var nfp in destinationNFPList.Where(nfp => ElementSynchronisers.FindMatchingDestination<NotificationForwardingPartySynchroniser>(nfp) == null))
			{
				nfp.Delete();
			}
		}
	}
}
