using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Direction = Enterprise.DocumentEngineCore.DocumentSupport.DocumentDirection;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("CartageInfo")]
	public abstract class CartageInfoWrapper : GenericWrapper
	{
		protected CartageInfoWrapper(BusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		#region New

		public static CartageInfoWrapper New(BusinessObjectFactory factory)
		{
			return new CartageInfoWrapperEmpty(factory);
		}

		public static CartageInfoWrapper New(ForwardingShipment forwardingShipmentToWrap, BusinessObjectFactory factory)
		{
			CartageInfoWrapper result = null;

			if (forwardingShipmentToWrap != null)
			{
				result = new CartageInfoWrapperFromShipment(forwardingShipmentToWrap, factory);
			}

			return result ?? new CartageInfoWrapperEmpty(factory);
		}

		public static CartageInfoWrapper New(CommonContainer containerToWrap, CommonShipment parentShipmentToWrap, BusinessObjectFactory factory)
		{
			CartageInfoWrapper result = null;

			if (containerToWrap != null)
			{
				result = new CartageInfoWrapperFromContainer(containerToWrap, parentShipmentToWrap, factory);
			}

			return result ?? new CartageInfoWrapperEmpty(factory);
		}

		public static CartageInfoWrapper New(IDocCartageAdvice objectToWrap, BusinessObjectFactory factory)
		{
			CartageInfoWrapper result = null;

			if (objectToWrap != null)
			{
				result = new CartageInfoWrapperFromIDocCartageAdvice(objectToWrap, factory);
			}

			return result ?? new CartageInfoWrapperEmpty(factory);
		}

		#endregion

		#region IsImport / IsExport

		protected override ZBool IsExportDocumentCore
		{
			get { return OverridenDocumentDirection.HasValue ? (ZBool)(OverridenDocumentDirection.Value == Direction.DEP) : base.IsExportDocumentCore; }
		}

		protected override ZBool IsImportDocumentCore
		{
			get { return OverridenDocumentDirection.HasValue ? (ZBool)(OverridenDocumentDirection.Value == Direction.ARV) : base.IsImportDocumentCore; }
		}

		internal void OverrideDocumentDirection(Direction overridenDocumentDirection)
		{
			OverridenDocumentDirection = overridenDocumentDirection;
		}

		Direction? OverridenDocumentDirection;

		#endregion

		#region Journey Headings

		public ZString JourneyOnePickUpHeading
		{
			get { return GetJourneyOnePickUpHeading(); }
		}
		protected abstract ZString GetJourneyOnePickUpHeading();

		public ZString JourneyOneDeliverToHeading
		{
			get { return GetJourneyOneDeliverToHeading(); }
		}
		protected abstract ZString GetJourneyOneDeliverToHeading();

		public ZString JourneyTwoPickUpHeading
		{
			get { return GetJourneyTwoPickUpHeading(); }
		}
		protected abstract ZString GetJourneyTwoPickUpHeading();

		public ZString JourneyTwoDeliverToHeading
		{
			get { return GetJourneyTwoDeliverToHeading(); }
		}
		protected abstract ZString GetJourneyTwoDeliverToHeading();

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return GetPickupOrStorageCommenceDateHeading(); }
		}
		protected abstract ZString GetPickupOrStorageCommenceDateHeading();

		#endregion

		#region Dates

		public ZDateTime JourneyOnePickUpDate
		{
			get { return GetJourneyOnePickUpDate(); }
		}
		protected abstract ZDateTime GetJourneyOnePickUpDate();

		public ZDateTime JourneyOnePickUpRequiredByDate
		{
			get { return GetJourneyOnePickUpRequiredByDate(); }
		}
		protected abstract ZDateTime GetJourneyOnePickUpRequiredByDate();

		public ZDateTime JourneyOneDeliverToDate
		{
			get { return GetJourneyOneDeliverToDate(); }
		}
		protected abstract ZDateTime GetJourneyOneDeliverToDate();

		public ZDateTime JourneyOneDeliverToRequiredByDate
		{
			get { return GetJourneyOneDeliverToRequiredByDate(); }
		}
		protected abstract ZDateTime GetJourneyOneDeliverToRequiredByDate();

		public ZDateTime JourneyTwoPickUpDate
		{
			get { return GetJourneyTwoPickUpDate(); }
		}
		protected abstract ZDateTime GetJourneyTwoPickUpDate();

		public ZDateTime JourneyTwoPickUpRequiredByDate
		{
			get { return GetJourneyTwoPickUpRequiredByDate(); }
		}
		protected abstract ZDateTime GetJourneyTwoPickUpRequiredByDate();

		public ZDateTime JourneyTwoDeliverToDate
		{
			get { return GetJourneyTwoDeliverToDate(); }
		}
		protected abstract ZDateTime GetJourneyTwoDeliverToDate();

		public ZDateTime JourneyTwoDeliverToRequiredByDate
		{
			get { return GetJourneyTwoDeliverToRequiredByDate(); }
		}
		protected abstract ZDateTime GetJourneyTwoDeliverToRequiredByDate();

		public ZDateTime CutOffDate
		{
			get { return GetCutOffDate(); }
		}
		protected abstract ZDateTime GetCutOffDate();

		public ZDateTime AvailableDate
		{
			get { return GetAvailableDate(); }
		}
		protected abstract ZDateTime GetAvailableDate();

		public ZDateTime CutOffOrAvailableDate
		{
			get { return GetCutOffOrAvailableDate(); }
		}
		protected abstract ZDateTime GetCutOffOrAvailableDate();

		public ZDateTime ReceivalDate
		{
			get { return GetReceivalDate(); }
		}
		protected abstract ZDateTime GetReceivalDate();

		public ZDateTime StorageCommenceDate
		{
			get { return GetStorageCommenceDate(); }
		}
		protected abstract ZDateTime GetStorageCommenceDate();

		public ZDateTime PickupOrStorageCommenceDate
		{
			get { return GetPickupOrStorageCommenceDate(); }
		}
		protected abstract ZDateTime GetPickupOrStorageCommenceDate();

		#endregion

		#region Addresses

		public AddressWrapperWithIDocDocAddress JourneyOnePickUpAddress
		{
			get { return GetJourneyOnePickUpAddress(); }
		}
		protected abstract AddressWrapperWithIDocDocAddress GetJourneyOnePickUpAddress();

		public AddressWrapperWithIDocDocAddress JourneyOneDeliverToAddress
		{
			get { return GetJourneyOneDeliverToAddress(); }
		}
		protected abstract AddressWrapperWithIDocDocAddress GetJourneyOneDeliverToAddress();

		public AddressWrapperWithIDocDocAddress JourneyTwoPickUpAddress
		{
			get { return GetJourneyTwoPickUpAddress(); }
		}
		protected abstract AddressWrapperWithIDocDocAddress GetJourneyTwoPickUpAddress();

		public AddressWrapperWithIDocDocAddress JourneyTwoDeliverToAddress
		{
			get { return GetJourneyTwoDeliverToAddress(); }
		}
		protected abstract AddressWrapperWithIDocDocAddress GetJourneyTwoDeliverToAddress();

		public AddressWrapperWithIDocDocAddressCollection AddressesWithWareHousing
		{
			get { return GetAddressesWithWareHousing(); }
		}

		protected virtual AddressWrapperWithIDocDocAddressCollection GetAddressesWithWareHousing()
		{
			AddressWrapperWithIDocDocAddressCollection addressesOnCartageAdvice = new AddressWrapperWithIDocDocAddressCollection(Factory);

			if (JourneyOnePickUpAddress != null)
			{
				addressesOnCartageAdvice.Add(JourneyOnePickUpAddress);
			}

			if (JourneyOneDeliverToAddress != null)
			{
				addressesOnCartageAdvice.Add(JourneyOneDeliverToAddress);
			}

			if (PrintTwoJourneys)
			{
				if (JourneyTwoPickUpAddress != null)
				{
					addressesOnCartageAdvice.Add(JourneyTwoPickUpAddress);
				}

				if (JourneyTwoDeliverToAddress != null)
				{
					addressesOnCartageAdvice.Add(JourneyTwoDeliverToAddress);
				}
			}

			return addressesOnCartageAdvice.GetAddressesWithWarehousing();
		}

		#endregion

		#region Contacts

		public ZString JourneyOnePickUpContactName
		{
			get { return GetJourneyOnePickUpContactName(); }
		}

		protected virtual ZString GetJourneyOnePickUpContactName()
		{
			return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.ContactName : ZString.Empty;
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return GetJourneyOnePickUpContactPhone(); }
		}

		protected virtual ZString GetJourneyOnePickUpContactPhone()
		{
			return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.ContactPhone : ZString.Empty;
		}

		public ZString JourneyOneDeliverToContactName
		{
			get { return GetJourneyOneDeliverToContactName(); }
		}

		protected virtual ZString GetJourneyOneDeliverToContactName()
		{
			return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.ContactName : ZString.Empty;
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return GetJourneyOneDeliverToContactPhone(); }
		}

		protected virtual ZString GetJourneyOneDeliverToContactPhone()
		{
			return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.ContactPhone : ZString.Empty;
		}

		public ZString JourneyTwoPickUpContactName
		{
			get { return GetJourneyTwoPickUpContactName(); }
		}

		protected virtual ZString GetJourneyTwoPickUpContactName()
		{
			return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.ContactName : ZString.Empty;
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return GetJourneyTwoPickUpContactPhone(); }
		}

		protected virtual ZString GetJourneyTwoPickUpContactPhone()
		{
			return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.ContactPhone : ZString.Empty;
		}

		public ZString JourneyTwoDeliverToContactName
		{
			get { return GetJourneyTwoDeliverToContactName(); }
		}

		protected virtual ZString GetJourneyTwoDeliverToContactName()
		{
			return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.ContactName : ZString.Empty;
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return GetJourneyTwoDeliverToContactPhone(); }
		}

		protected virtual ZString GetJourneyTwoDeliverToContactPhone()
		{
			return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.ContactPhone : ZString.Empty;
		}

		#endregion

		#region Helpers

		public CartageAdviceHelper CartageAdvice
		{
			get
			{
				if (cartageAdvice == null)
				{
					cartageAdvice = GetCartageAdvice();
				}

				return cartageAdvice;
			}
		}
		protected abstract CartageAdviceHelper GetCartageAdvice();
		CartageAdviceHelper cartageAdvice;

		public DocCommonCartage Cartage
		{
			get
			{
				if (cartage == null)
				{
					cartage = GetCartage();
				}

				return cartage;
			}
		}
		DocCommonCartage cartage;

		protected virtual DocCommonCartage GetCartage()
		{
			// Included for compatibility with legacy. Should not be used in DocStrips.
			return DocCommonCartage.New(Factory.GetNull<CommonCartage>(), Factory);
		}

		#endregion

		#region Other properties

		public ZString EmailSubjectNumber
		{
			get { return GetEmailSubjectNumber(); }
		}
		protected abstract ZString GetEmailSubjectNumber();

		public ZString JourneyOnePickUpSlofRef
		{
			get { return GetJourneyOnePickUpSlofRef(); }
		}
		protected abstract ZString GetJourneyOnePickUpSlofRef();

		public ZString JourneyOnePickUpReleaseNum
		{
			get { return GetJourneyOnePickUpReleaseNum(); }
		}
		protected abstract ZString GetJourneyOnePickUpReleaseNum();

		public ZString JourneyTwoDeliverToSlofRef
		{
			get { return GetJourneyTwoDeliverToSlofRef(); }
		}
		protected abstract ZString GetJourneyTwoDeliverToSlofRef();

		public ZString JourneyTwoDeliverToReleaseNum
		{
			get { return GetJourneyTwoDeliverToReleaseNum(); }
		}
		protected abstract ZString GetJourneyTwoDeliverToReleaseNum();

		public ZBool PrintAsContainers
		{
			get { return GetPrintAsContainers(); }
		}
		protected abstract ZBool GetPrintAsContainers();

		public ZBool PrintTwoJourneys
		{
			get { return GetPrintTwoJourneys(); }
		}
		protected abstract ZBool GetPrintTwoJourneys();

		public ZBool PrintJourneyOne
		{
			get { return GetPrintJourneyOne(); }
		}
		protected abstract ZBool GetPrintJourneyOne();

		public ZBool PrintJourneyTwo
		{
			get { return GetPrintJourneyTwo(); }
		}
		protected abstract ZBool GetPrintJourneyTwo();

		public ZString EquipmentType
		{
			get { return GetEquipmentType(); }
		}
		protected abstract ZString GetEquipmentType();

		public ZString FullHandlingInstructions
		{
			get { return GetFullHandlingInstructions(); }
		}
		protected abstract ZString GetFullHandlingInstructions();

		public ZString FullCartageInstructions
		{
			get { return GetFullCartageInstructions(); }
		}
		protected abstract ZString GetFullCartageInstructions();

		public ZString LegNotes
		{
			get { return GetLegNotes(); }
		}
		protected abstract ZString GetLegNotes();

		public ZBool IsAir
		{
			get { return GetIsAir(); }
		}
		protected abstract ZBool GetIsAir();

		#endregion

		#region Generic Heading Texts

		public ZString JourneyOnePickUpDateHeading
		{
			get { return GetJourneyOnePickUpDateHeading(); }
		}
		protected abstract ZString GetJourneyOnePickUpDateHeading();

		public ZString JourneyOnePickUpRequiredByDateHeading
		{
			get { return GetJourneyOnePickUpRequiredByDateHeading(); }
		}
		protected abstract ZString GetJourneyOnePickUpRequiredByDateHeading();

		public ZString JourneyOneDeliverToDateHeading
		{
			get { return GetJourneyOneDeliverToDateHeading(); }
		}
		protected abstract ZString GetJourneyOneDeliverToDateHeading();

		public ZString JourneyOneDeliverToRequiredByDateHeading
		{
			get { return GetJourneyOneDeliverToRequiredByDateHeading(); }
		}
		protected abstract ZString GetJourneyOneDeliverToRequiredByDateHeading();

		public ZString JourneyTwoPickUpDateHeading
		{
			get { return GetJourneyTwoPickUpDateHeading(); }
		}
		protected abstract ZString GetJourneyTwoPickUpDateHeading();

		public ZString JourneyTwoPickUpRequiredByDateHeading
		{
			get { return GetJourneyTwoPickUpRequiredByDateHeading(); }
		}
		protected abstract ZString GetJourneyTwoPickUpRequiredByDateHeading();

		public ZString JourneyTwoDeliverToDateHeading
		{
			get { return GetJourneyTwoDeliverToDateHeading(); }
		}
		protected abstract ZString GetJourneyTwoDeliverToDateHeading();

		public ZString JourneyTwoDeliverToRequiredByDateHeading
		{
			get { return GetJourneyTwoDeliverToRequiredByDateHeading(); }
		}
		protected abstract ZString GetJourneyTwoDeliverToRequiredByDateHeading();

		protected enum Heading
		{
			CutOffDateHeading,
			DateHeading,
			DeliveryRequiredByHeading,
			EmptyReadyByHeading,
			EmptyRequiredByHeading,
			EmptyReturnByHeading,
			EstimatedDeliveryHeading,
			EstimatedPickupDateHeading,
			PickupRequiredByHeading,
			ReceivalsStartHeading,
			SlotDateHeading,
			Empty
		}

		protected string GetHeadingText(Heading heading)
		{
			switch (heading)
			{
				case Heading.CutOffDateHeading:
					return Res.GetString("0bb62874-d0e6-4909-be56-ed4817af4c72", "Cut Off Date:");
				case Heading.DateHeading:
					return Res.GetString("208b00cd-4a0f-4441-8760-1ccfbb7f069b", "Date:");
				case Heading.DeliveryRequiredByHeading:
					return Res.GetString("ea592990-a0d4-47ee-9bff-19254529eb71", "Delivery Required By:");
				case Heading.EmptyReadyByHeading:
					return Res.GetString("f05cf46f-e985-43b1-8a24-24d09d15ceb0", "Empty Ready By:");
				case Heading.EmptyRequiredByHeading:
					return Res.GetString("ba79ba98-8788-4bc5-addc-b60c4996aec1", "Empty Required By:");
				case Heading.EmptyReturnByHeading:
					return Res.GetString("5da3c0c1-4baf-4b79-8ea2-4bf400ad5a03", "Empty Return By:");
				case Heading.EstimatedDeliveryHeading:
					return Res.GetString("7388ac9b-9504-4ef3-b951-f727600b9588", "Estimated Delivery:");
				case Heading.EstimatedPickupDateHeading:
					return Res.GetString("20d0c8b0-e594-45c5-b679-3dfe015e55ad", "Estimated Pickup Date:");
				case Heading.PickupRequiredByHeading:
					return Res.GetString("f70d752c-9494-4fcf-b669-0eaba55028e7", "Pickup Required By:");
				case Heading.ReceivalsStartHeading:
					return Res.GetString("e75fbc75-dad1-4b7d-8a80-608a5e58eac2", "Receivals Start:");
				case Heading.SlotDateHeading:
					return Res.GetString("d4fa62de-8e70-424b-ae43-454a0228191c", "Slot Date:");
				case Heading.Empty:
					return (NoResString)"";
				default:
					return null;
			}
		}

		#endregion

		#region Address Helpers

		protected AddressWrapperWithIDocDocAddress GetLocalTransportAddress(DocDocAddress docAddress)
		{
			JobDocAddress jobAddress = null;

			if (docAddress != null)
			{
				jobAddress = docAddress.WrappedObject as JobDocAddress;
			}

			return GetLocalTransportAddress(jobAddress);
		}

		protected AddressWrapperWithIDocDocAddress GetLocalTransportAddress(JobDocAddress jobAddress)
		{
			if (jobAddress != null)
			{
				jobAddress.DefaultContactType = ContactType.LocalTransport;
			}

			return new AddressWrapperWithIDocDocAddress(jobAddress, Factory);
		}

		protected AddressWrapperWithIDocDocAddress GetLocalTransportAddress(OrgAddress orgAddress)
		{
			return new AddressWrapperWithIDocDocAddress(orgAddress, ContactType.LocalTransport, Factory);
		}

		#endregion
	}
}
