using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.Container_Wrappers
{
	public class DocFreightBaseContainerHelperClassForCartageAdvice : DocFreightBaseContainerHelperClass
	{
		public DocFreightBaseContainerHelperClassForCartageAdvice(CommonContainer baseContainer, BusinessObjectFactory factoryToWrap)
			: base(baseContainer, factoryToWrap)
		{
		}

		#region Overrides

		public override DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				if (fJourneyOnePickUpAddress == null)
				{
					fJourneyOnePickUpAddress = DocDocAddress.New(Header.Addresses[0], Factory);
				}
				return fJourneyOnePickUpAddress;
			}
		}
		DocDocAddress fJourneyOnePickUpAddress;

		public override DocDocAddress JourneyOneDeliverToAddressForExport
		{
			get { return DocDocAddress.New(Header.Addresses[0], Factory); }
		}

		public override DocDocAddress JourneyOneDeliverToAddressForImport
		{
			get { return DocDocAddress.New(Header.Addresses[0], Factory); }
		}

		public override DocDocAddress JourneyTwoPickUpAddressForExport
		{
			get { return DocDocAddress.New(Header.Addresses[0], Factory); }
		}

		public override DocDocAddress JourneyTwoPickUpAddressForImport
		{
			get { return DocDocAddress.New(Header.Addresses[0], Factory); }
		}

		public override DocDocAddress JourneyTwoDeliverToAddress
		{
			get
			{
				if (fJourneyTwoDeliverToAddress == null)
				{
					fJourneyTwoDeliverToAddress = DocDocAddress.New(Header.Addresses[0], Factory);
				}
				return fJourneyTwoDeliverToAddress;
			}
		}
		DocDocAddress fJourneyTwoDeliverToAddress;

		#endregion

		public OrgContact LocalTransportContact
		{
			get { return fLocalTransportContact; }
		}

		public OrgContact AllTypeContact
		{
			get { return fAllTypeContact; }
		}

		#region Implementation

		protected OrgHeader Header;
		protected OrgContact fLocalTransportContact;
		protected OrgContact fAllTypeContact;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Header = Factory.New<OrgHeader>();
			Header.MainAddress.OA_Phone = "333";
			Header.Addresses.AddNew();

			fLocalTransportContact = Header.Contacts.AddNew();
			fAllTypeContact = Header.Contacts.AddNew();

			fLocalTransportContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			fAllTypeContact.Documents.AddNew().OD_DocumentGroup = ContactType.All.Code;

			fLocalTransportContact.Documents[0].OD_DefaultContact = true;
			fAllTypeContact.Documents[0].OD_DefaultContact = true;

			fLocalTransportContact.OC_ContactName = "AAA";
			fAllTypeContact.OC_ContactName = "BBB";

			fLocalTransportContact.OC_Phone = "111";
			fAllTypeContact.OC_Phone = "222";
		}

		#endregion
	}
}
