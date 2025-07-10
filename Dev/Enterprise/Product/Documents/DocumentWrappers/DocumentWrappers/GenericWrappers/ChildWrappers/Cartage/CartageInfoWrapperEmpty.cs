using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CartageInfoWrapperEmpty : CartageInfoWrapper
	{
		public CartageInfoWrapperEmpty(BusinessObjectFactory factory)
			: base(null, factory)
		{
		}

		#region Properties

		#region Journey Headings

		protected override ZString GetJourneyOnePickUpHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyOneDeliverToHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoPickUpHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoDeliverToHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetPickupOrStorageCommenceDateHeading()
		{
			return ZString.Empty;
		}

		#endregion

		#region Dates

		protected override ZDateTime GetJourneyOnePickUpDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetJourneyOnePickUpRequiredByDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetJourneyOneDeliverToDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetJourneyOneDeliverToRequiredByDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetJourneyTwoPickUpDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetJourneyTwoPickUpRequiredByDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetJourneyTwoDeliverToDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetJourneyTwoDeliverToRequiredByDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetCutOffDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetAvailableDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetCutOffOrAvailableDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetReceivalDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetStorageCommenceDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupOrStorageCommenceDate()
		{
			return ZDateTime.Empty;
		}

		#endregion

		#region Addresses

		protected override AddressWrapperWithIDocDocAddress GetJourneyOnePickUpAddress()
		{
			return GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyOneDeliverToAddress()
		{
			return GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoPickUpAddress()
		{
			return GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoDeliverToAddress()
		{
			return GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		#endregion

		#region Helper

		protected override CartageAdviceHelper GetCartageAdvice()
		{
			return new CartageAdviceHelper(this, Factory);
		}

		#endregion

		#region Other properties

		protected override ZString GetEmailSubjectNumber()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyOnePickUpSlofRef()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyOnePickUpReleaseNum()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoDeliverToSlofRef()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoDeliverToReleaseNum()
		{
			return ZString.Empty;
		}

		protected override ZBool GetPrintAsContainers()
		{
			return false;
		}

		protected override ZBool GetPrintTwoJourneys()
		{
			return false;
		}

		protected override ZBool GetPrintJourneyOne()
		{
			return false;
		}

		protected override ZBool GetPrintJourneyTwo()
		{
			return false;
		}

		protected override ZString GetEquipmentType()
		{
			return ZString.Empty;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			return ZString.Empty;
		}

		protected override ZString GetFullCartageInstructions()
		{
			return ZString.Empty;
		}

		protected override ZString GetLegNotes()
		{
			return ZString.Empty;
		}

		protected override ZBool GetIsAir()
		{
			return false;
		}

		#endregion

		#region Generic Heading Texts

		protected override ZString GetJourneyOnePickUpDateHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyOnePickUpRequiredByDateHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyOneDeliverToDateHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyOneDeliverToRequiredByDateHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoPickUpDateHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoPickUpRequiredByDateHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoDeliverToDateHeading()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoDeliverToRequiredByDateHeading()
		{
			return ZString.Empty;
		}

		#endregion

		#endregion
	}
}
