using System;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocContainerRelease))]
	sealed class DocContainerReleaseTest : DocumentWrapperTestCase
	{
		public void TestLogAgaintsShipment()
		{
			IBODocDataProvider provider = Wrapper;

			AssertEquals("BusinessObjectToLogAgainst", Booking, provider.BusinessObjectToLogAgainst);
			AssertEquals("ParentBusinessObject", Booking, provider.ParentBusinessObject);
		}

		public void TestContainerYard()
		{
			Address.Header.OH_FullName = "Full Name";

			AssertEquals("Full Name", Wrapper.ContainerYard.CompanyName);
		}

		public void TestReleaseNumber()
		{
			Header.ReleaseNumber = "YYY";

			Instance.ReleaseNumber = "Blaticus";
			AssertEquals("Blaticus", Wrapper.ReleaseNumber);

			Instance.ReleaseNumber = "";
			AssertEquals("", Wrapper.ReleaseNumber);
		}

		public void TestReleaseNote()
		{
			Header.ContainerReleaseNote = "Fit with self destruct systems.\nActivation code: 'what the f@#$ is that?'.";
			AssertEquals("Fit with self destruct systems.\nActivation code: 'what the f@#$ is that?'.", Wrapper.ReleaseNote);
		}

		public void TestShipment()
		{
			AssertEquals(Booking, Wrapper.Shipment.WrappedObject);
		}

		public void TestReleasedContainers()
		{
			AgencyShipmentContainer container1 = AddContainer("20GP", 1);
			AgencyShipmentContainer container2 = AddContainer("20RE", 1);
			AgencyShipmentContainer container3 = AddContainer("40GP", 1);
			AgencyShipmentContainer container4 = AddContainer("40RE", 1);

			container1.JC_ReleaseNum = "";
			container2.JC_ReleaseNum = "Ref-1";
			container3.JC_ReleaseNum = "Ref-2";
			container4.JC_ReleaseNum = "Ref-2";

			Instance.ReleaseNumber = "Ref-1";
			AssertContainsExactElementsInAnyOrder("Only the Ref-1 container should be returned",
				(c) => c.JC_ContainerCode + ":" + c.JC_ReleaseNum,
				new AgencyShipmentContainer[] { container2 },
				Array.ConvertAll(Wrapper.ReleasedContainers.ToArray(), (w) => (AgencyShipmentContainer)((DocumentWrapper)w).WrappedObject));

			Instance.ReleaseNumber = "Ref-2";
			AssertContainsExactElementsInAnyOrder("Only the Ref-2 containers should be returned",
				(c) => c.JC_ContainerCode + ":" + c.JC_ReleaseNum,
				new AgencyShipmentContainer[] { container3, container4 },
				Array.ConvertAll(Wrapper.ReleasedContainers.ToArray(), (w) => (AgencyShipmentContainer)((DocumentWrapper)w).WrappedObject));
		}

		#region Implementation

		AgencyBookingContainer AddContainer(string typeCode, short count)
		{
			AgencyBookingContainer container = Booking.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, typeCode).PK;
			container.JC_ContainerCount = count;
			return container;
		}

		OrgAddress Address
		{
			get
			{
				if (address == null)
				{
					address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
					address.Header.OH_FullName = "Address";
				}
				return address;
			}
		}
		OrgAddress address;

		AgencyBooking Booking
		{
			get { return booking ?? (booking = Factory.New<AgencyBooking>()); }
		}
		AgencyBooking booking;

		ReleaseHeader Header
		{
			get { return header ?? (header = new ReleaseHeader(Booking, false)); }
		}
		ReleaseHeader header;

		ReleaseInstance Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ReleaseInstance(Header);
					instance.ContainerYardAddress = Address.PK;
				}
				return instance;
			}
		}
		ReleaseInstance instance;

		DocContainerRelease Wrapper
		{
			get { return DocContainerRelease.New(Instance, Factory); }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.BookedContainers.AddNew();

			return new DocumentWrapper[]
			{
				DocContainerRelease.New(new ReleaseHeader(booking, true).Instances.AddNew(), Factory),
				DocContainerRelease.New(new ReleaseHeader(booking, false).Instances.AddNew(), Factory),
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocContainerRelease.New(Instance, Factory);
		}

		#endregion
	}
}
