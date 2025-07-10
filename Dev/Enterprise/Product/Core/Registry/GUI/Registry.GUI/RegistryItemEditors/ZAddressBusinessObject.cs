using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Registry.GUI
{
	class ZAddressBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string SelectedAddress = "SelectedAddress";
		}

		#endregion

		public ZAddressBusinessObject() : base(new BusinessObjectFactory())
		{
			SelectedAddress_ZAddress.DefaultAddressType = AddressType.OFC;
			ReadOnly = true;
		}

		#region Organisations

		public OrgHeaderCollection OrgHeaders
		{
			get
			{
				if (fOrgHeaders == null)
				{
					fOrgHeaders = new OrgHeaderCollection(Factory);
				}
				return fOrgHeaders;
			}
		}
		OrgHeaderCollection fOrgHeaders;

		#endregion

		#region ZAddress

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress SelectedAddress_ZAddress
		{
			get
			{
				if (fSelectedAddress_ZAddress == null)
				{
					fSelectedAddress_ZAddress = new ZAddress(SelectedAddressInfo);
				}
				return fSelectedAddress_ZAddress;
			}
		}
		ZAddress fSelectedAddress_ZAddress;

		#endregion

		#region SelectedAddress

		public ZGuid SelectedAddress
		{
			get { return fSelectedAddress; }
			set
			{
				if (SelectedAddress != value)
				{
					fSelectedAddress = value;
					HasChanges = true;
				}
				if (!IsValidationSuspended)
				{
					ValidateSelectedAddress();
				}
				SelectedAddressInfo.RefreshBinding();
			}
		}

		public virtual void ValidateSelectedAddress()
		{
			if (!ReadOnly)
			{
				fErrorMessage = (SelectedAddress.IsEmpty || !SelectedAddress.IsValid) ? Res.GetString("739ed02f-98de-4efa-9db4-630401e839d6", "Please select a valid address from an organization.") : "";
			}
		}

		public string ErrorMessage
		{
			get { return fErrorMessage ?? string.Empty; }
		}
		string fErrorMessage;

		public ZPropertyInfo SelectedAddressInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedAddress); }
		}

		ZGuid fSelectedAddress;

		#endregion
	}
}
