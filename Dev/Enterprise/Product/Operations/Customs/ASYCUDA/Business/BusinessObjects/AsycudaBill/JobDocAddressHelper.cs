using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ASYCUDA.Business
{
	class JobDocAddressHelper<T> where T : BusinessObject, IDocAddresses
	{
		public JobDocAddressHelper(T parent)
		{
			this.parent = parent;
		}
		readonly T parent;

		#region Shipper
		internal JobDocAddress Shipper
		{
			get
			{
				if (shipperJobDocAddress == null || shipperJobDocAddress.IsDeleted)
				{
					if (shipperJobDocAddress != null)
					{
						shipperJobDocAddress.DocAddressChanged -= new EventHandler(ShipperJobDocAddressChanged);
					}
					shipperJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ShipperJobDocAddressRequirement);
					shipperJobDocAddress.DocAddressChanged += new EventHandler(ShipperJobDocAddressChanged);
				}
				return shipperJobDocAddress;
			}
		}
		JobDocAddress shipperJobDocAddress;

		void ShipperJobDocAddressChanged(object sender, EventArgs e)
		{
			Shipper.Validation.ValidateE2_OA_Address();
		}

		internal JobDocAddressRequirement ShipperJobDocAddressRequirement
		{
			get
			{
				if (shipperJobDocAddressRequirement == null)
				{
					shipperJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
					JobDocAddressManager.AddRequirement(shipperJobDocAddressRequirement);
				}
				return shipperJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement shipperJobDocAddressRequirement;
		#endregion

		#region Consignee
		internal JobDocAddress Consignee
		{
			get
			{
				if (consigneeJobDocAddress == null || consigneeJobDocAddress.IsDeleted)
				{
					if (consigneeJobDocAddress != null)
					{
						consigneeJobDocAddress.DocAddressChanged -= new EventHandler(ConsigneeJobDocAddressChanged);
					}
					consigneeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeJobDocAddressRequirement);
					consigneeJobDocAddress.DocAddressChanged += new EventHandler(ConsigneeJobDocAddressChanged);
				}
				return consigneeJobDocAddress;
			}
		}
		JobDocAddress consigneeJobDocAddress;

		void ConsigneeJobDocAddressChanged(object sender, EventArgs e)
		{
			Consignee.Validation.ValidateE2_OA_Address();
		}

		internal JobDocAddressRequirement ConsigneeJobDocAddressRequirement
		{
			get
			{
				if (consigneeJobDocAddressRequirement == null)
				{
					consigneeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress, ContactType.Consignee);
					JobDocAddressManager.AddRequirement(consigneeJobDocAddressRequirement);
				}
				return consigneeJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement consigneeJobDocAddressRequirement;
		#endregion

		#region Notify
		internal JobDocAddress NotifyParty
		{
			get
			{
				if (notifyPartyJobDocAddress == null || notifyPartyJobDocAddress.IsDeleted)
				{
					if (notifyPartyJobDocAddress != null)
					{
						notifyPartyJobDocAddress.DocAddressChanged -= new EventHandler(NotifyPartyJobDocAddressChanged);
					}
					notifyPartyJobDocAddress = DocAddresses.FindOrCreateWithRequirement(NotifyPartyJobDocAddressRequirement);
					notifyPartyJobDocAddress.DocAddressChanged += new EventHandler(NotifyPartyJobDocAddressChanged);
				}
				return notifyPartyJobDocAddress;
			}
		}
		JobDocAddress notifyPartyJobDocAddress;

		void NotifyPartyJobDocAddressChanged(object sender, EventArgs e)
		{
			NotifyParty.Validation.ValidateE2_OA_Address();
		}

		internal JobDocAddressRequirement NotifyPartyJobDocAddressRequirement
		{
			get
			{
				if (notifyPartyJobDocAddressRequirement == null)
				{
					notifyPartyJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty, ContactType.NotifyParty);
					JobDocAddressManager.AddRequirement(notifyPartyJobDocAddressRequirement);
				}
				return notifyPartyJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement notifyPartyJobDocAddressRequirement;
		#endregion

		[ChildEditable(true)]
		internal JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (jobDocAddressDependentCollection == null)
				{
					jobDocAddressDependentCollection = new JobDocAddressDependentCollection(parent);
					jobDocAddressDependentCollection.Load();
					parent.RegisterEditableChildObject(jobDocAddressDependentCollection);
				}
				return jobDocAddressDependentCollection;
			}
		}
		JobDocAddressDependentCollection jobDocAddressDependentCollection;

		internal JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
					return ShipperJobDocAddressRequirement;
				case DocAddressType.ConsigneeDocumentaryAddress:
					return ConsigneeJobDocAddressRequirement;
				case DocAddressType.NotifyParty:
					return NotifyPartyJobDocAddressRequirement;
				default:
					return null;
			}
		}

		internal JobDocAddressManager JobDocAddressManager
		{
			get { return jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager()); }
		}
		JobDocAddressManager jobDocAddressManager;
	}
}
