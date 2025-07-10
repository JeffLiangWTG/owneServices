using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CompanyNameAndAddress")]
	public class AddressWrapperWithIDocDocAddress : AddressWrapper, IDocDocAddress
	{
		#region Ctors

		public AddressWrapperWithIDocDocAddress(JobDocAddress docAddress, BusinessObjectFactory factory)
			: base(docAddress, factory)
		{
			if (docAddress != null)
			{
				orgAddress = docAddress.Address;

				if (docAddress.Contact != null)
				{
					selectedContact = new ContactWrapper(docAddress.Contact, factory);
				}
			}
		}

		public AddressWrapperWithIDocDocAddress(OrgAddress address, ContactType contactType, BusinessObjectFactory factory)
			: base(address, contactType, factory)
		{
			orgAddress = address;
		}

		public AddressWrapperWithIDocDocAddress(ZString companyNameAndAddress, BusinessObjectFactory factory)
			: base(companyNameAndAddress, factory)
		{
		}

		public AddressWrapperWithIDocDocAddress(OrgAddress address, ZString contactName, BusinessObjectFactory factory)
			: base(address, contactName, factory)
		{
			orgAddress = address;
		}

		#endregion

		#region Contacts

		public override ZString ContactName
		{
			get { return selectedContact != null ? selectedContact.FullName : base.ContactName; }
		}

		public ZString ContactPhone
		{
			get { return selectedContact != null ? selectedContact.Phone : Phone; }
		}

		#endregion

		#region IDocDocAddress members

		#region ZString Properties

		ZString IDocDocAddress.AccessPoint
		{
			get { return LegacyWrapper.AccessPoint; }
		}

		public ZString Address1
		{
			get { return LegacyWrapper.Address1; }
		}

		public ZString Address2
		{
			get { return LegacyWrapper.Address2; }
		}

		public ZString AddressType
		{
			get { return LegacyWrapper.AddressType; }
		}

		public ZString Code
		{
			get { return LegacyWrapper.Code; }
		}

		ZString IDocDocAddress.CommunicationRequired
		{
			get { return LegacyWrapper.CommunicationRequired; }
		}

		public ZString CompanyNameAddress1Address2City
		{
			get { return LegacyWrapper.CompanyNameAddress1Address2City; }
		}

		public ZString CompanyNameOverride
		{
			get { return LegacyWrapper.CompanyNameOverride; }
		}

		ZString IDocDocAddress.ContainerHandling
		{
			get { return LegacyWrapper.ContainerHandling; }
		}

		public ZString DepotLocalControlledPremisesID
		{
			get { return LegacyWrapper.DepotLocalControlledPremisesID; }
		}

		public ZString Description
		{
			get { return LegacyWrapper.Description; }
		}

		ZString IDocDocAddress.DockHeight
		{
			get { return LegacyWrapper.DockHeight; }
		}

		ZString IDocDocAddress.LabourRequired
		{
			get { return LegacyWrapper.LabourRequired; }
		}

		public ZString Language
		{
			get { return LegacyWrapper.Language; }
		}

		public ZString LocalControlledPremisesID
		{
			get { return LegacyWrapper.LocalControlledPremisesID; }
		}

		public ZString PostalAddress
		{
			get { return LegacyWrapper.PostalAddress; }
		}

		public ZString PostalAddressExcludeName
		{
			get { return LegacyWrapper.PostalAddressExcludeName; }
		}

		public ZString SplitAddress1
		{
			get { return LegacyWrapper.SplitAddress1; }
		}

		public ZString SplitAddress2
		{
			get { return LegacyWrapper.SplitAddress2; }
		}

		public ZString Type
		{
			get { return LegacyWrapper.Type; }
		}

		public ZString UsageComment
		{
			get { return LegacyWrapper.UsageComment; }
		}

		public ZString WarehouseLocalControlledPremisesID
		{
			get { return LegacyWrapper.WarehouseLocalControlledPremisesID; }
		}

		DocUNLOCO IDocDocAddress.Port
		{
			get { return LegacyWrapper.Port; }
		}

		#endregion

		#region ZDateTime Properties

		public ZDateTime DeliverFromTimeOnly
		{
			get { return LegacyWrapper.DeliverFromTimeOnly; }
		}

		public ZDateTime DeliverToTimeOnly
		{
			get { return LegacyWrapper.DeliverToTimeOnly; }
		}

		public ZDateTime DoNotAttendFrom
		{
			get { return LegacyWrapper.DoNotAttendFrom; }
		}

		public ZDateTime DoNotAttendFromForCartageAdvice
		{
			get { return LegacyWrapper.DoNotAttendFromForCartageAdvice; }
		}

		public ZDateTime DoNotAttendTo
		{
			get { return LegacyWrapper.DoNotAttendTo; }
		}

		public ZDateTime DoNotAttendToForCartageAdvice
		{
			get { return LegacyWrapper.DoNotAttendToForCartageAdvice; }
		}

		public ZDateTime PickupFromTimeOnly
		{
			get { return LegacyWrapper.PickupFromTimeOnly; }
		}

		public ZDateTime PickupToTimeOnly
		{
			get { return LegacyWrapper.PickupToTimeOnly; }
		}

		#endregion

		#region ZBool Properties

		public ZBool HasWareHousing
		{
			get { return LegacyWrapper.HasWareHousing; }
		}

		public ZBool Overridden
		{
			get { return LegacyWrapper.Overridden; }
		}

		#endregion

		DocCountry IDocDocAddress.Country
		{
			get { return LegacyWrapper.Country; }
		}

		public DocAddressType DocAddressType
		{
			get { return LegacyWrapper.DocAddressType; }
		}

		DocOrganisation IDocDocAddress.Organisation
		{
			get { return LegacyWrapper.Organisation; }
		}

		#endregion

		#region Implementation

		DocDocAddress LegacyWrapper
		{
			get
			{
				if (legacyWrapper == null)
				{
					JobDocAddress jobDocAddress = WrappedObject as JobDocAddress;
					if (jobDocAddress != null)
					{
						legacyWrapper = DocDocAddress.New(jobDocAddress, Factory);
					}
					else
					{
						OrgAddress orgAddress = WrappedObject as OrgAddress;
						legacyWrapper = DocDocAddress.New(orgAddress ?? Factory.GetNull<OrgAddress>(), Factory);
					}
				}

				return legacyWrapper;
			}
		}
		DocDocAddress legacyWrapper;

		readonly OrgAddress orgAddress;

		internal OrgHeaderSource Organisation
		{
			get
			{
				if (organisation == null && orgAddress != null)
				{
					organisation = OrgHeaderSource.New(orgAddress.Header, Factory);
				}

				return organisation;
			}
		}
		OrgHeaderSource organisation;

		readonly ContactWrapper selectedContact;

		#endregion
	}
}
