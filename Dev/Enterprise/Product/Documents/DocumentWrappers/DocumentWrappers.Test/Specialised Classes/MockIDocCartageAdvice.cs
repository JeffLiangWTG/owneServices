using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class MockIDocCartageAdvice : DocBaseWrapper, IDocCartageAdvice
	{
		public MockIDocCartageAdvice(DummyBusinessObject bO, BusinessObjectFactory factoryToWrap)
			: base(bO, factoryToWrap)
		{
		}

		#region IDocCartageAdvice Members

		public ZString EmailSubjectNumber
		{
			get { return ""; }
		}

		public MultilingualString JourneyOnePickUpHeading
		{
			get { return (NoResString)""; }
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get { return (NoResString)""; }
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get { return (NoResString)""; }
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get { return (NoResString)""; }
		}

		public DocDocAddress JourneyOnePickUpAddress
		{
			get { return null; }
		}

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get { return null; }
		}

		public DocDocAddress JourneyTwoPickUpAddress
		{
			get { return null; }
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get { return null; }
		}

		public ZString JourneyOnePickUpContactName
		{
			get { return ""; }
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return ""; }
		}

		public ZString JourneyOneDeliverToContactName
		{
			get { return ""; }
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return ""; }
		}

		public ZString JourneyTwoPickUpContactName
		{
			get { return ""; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return ""; }
		}

		public ZString JourneyTwoDeliverToContactName
		{
			get { return ""; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return ""; }
		}

		public ZBool PrintAsContainers
		{
			get { return false; }
		}

		public ZBool PrintTwoJourneys
		{
			get { return false; }
		}

		public ZString EquipmentType
		{
			get { return ""; }
		}

		public ZString FullHandlingInstructions
		{
			get { return ""; }
		}

		public ZString FullCartageInstructions
		{
			get { return ""; }
		}

		public DocDocAddressCollection AddressesWithWareHousing
		{
			get { return new DocDocAddressCollection(Factory); }
		}

		public CartageAdviceHelper CartageAdvice
		{
			get
			{
				if (fCartageAdvice == null)
				{
					fCartageAdvice = new CartageAdviceHelper(this, Factory);
				}
				return fCartageAdvice;
			}
		}
		CartageAdviceHelper fCartageAdvice;

		public ZBool IsAir
		{
			get { return fIsAir; }
			set { fIsAir = value; }
		}
		ZBool fIsAir;

		public ZDateTime CartageCutOffDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CartageAvailableDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CartageReceivalDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
