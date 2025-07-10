using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	internal sealed class WebContact : IWebContact
	{
		public WebContact(WebFactory webUserFactory, OrgContact contact)
		{
			webFactory = webUserFactory;
			IsActive = contact.OC_IsActive;
			OC_ContactName = contact.OC_ContactName;
			OC_Email = contact.Email;
			OC_OH = contact.OC_OH;
			OC_Phone = contact.OC_Phone;
			OC_Mobile = contact.OC_Mobile;
			OC_WebContractSignedDate = contact.OC_WebContractSignedDate;

			lightweightContact = new LightweightBizo<OrgContact>(contact);
			ParentWebOrg = new WebOrg(webUserFactory, contact.ParentOrg);
		}

		readonly WebFactory webFactory;
		LightweightBizo<OrgContact> lightweightContact;

		public ZGuid PK => lightweightContact.PK;
		public bool IsActive { get; }
		public ZGuid OC_OH { get; }
		public ZString OC_ContactName { get; }
		public ZString OC_Phone { get; }
		public ZString OC_Mobile { get; }
		public ZString OC_Email { get; }
		public ZDateTime OC_WebContractSignedDate { get; private set; }

		public string Email => OC_Email;
		public string Mobile => OC_Mobile;
		public string Name => OC_ContactName;

		public IWebOrg ParentWebOrg { get; }

		public BusinessObjectFactory Factory => webFactory.Factory;

		public OrgContact GetContact() => lightweightContact.GetHeavy(this);

		public IContactable[] GetNestedContacts(string parentContactDescription)
		{
			return ((IContactable)GetContact()).GetNestedContacts(parentContactDescription);
		}

		/// <summary>
		/// Fetch the latest value of OC_WebContractSignedDate from the DB
		/// set it to the current time and save back to DB.
		/// Update any copies of the date in memory.
		/// </summary>
		public void SetUserWebContractSignedAndSaveToDb()
		{
			var userFactory = new BusinessObjectFactory();
			var user = userFactory.Load<OrgContact>(PK);

			user.OC_WebContractSignedDate = ZDateTime.Now;
			userFactory.Save();

			if (OC_WebContractSignedDate != user.OC_WebContractSignedDate)
			{
				// Update the date in memory.
				// Don't need thread locking here, since IIS will prevent multithread access to the session.
				OC_WebContractSignedDate = user.OC_WebContractSignedDate;
				var contactToReload = GetContact();
				contactToReload.Reload();
				lightweightContact = new LightweightBizo<OrgContact>(contactToReload);
			}
		}

#if DEBUG
		public void SetWebContractSignedDateForTest(ZDateTime date)
		{
			OC_WebContractSignedDate = date;
		}
#endif
	}
}
