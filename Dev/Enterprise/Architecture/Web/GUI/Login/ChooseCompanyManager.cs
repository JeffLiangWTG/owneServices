using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI
{
	public class ChooseCompanyManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ChooseCompanyManager(ZGlobal appInstance, IEnumerable<Guid> contactPKs) : base(new BusinessObjectFactory())
		{
			AppInstance = appInstance;
			ContactPKs = contactPKs;
		}

		ZGlobal AppInstance { get; }
		IEnumerable<Guid> ContactPKs { get; }

		public OrgContactCollection LoginContacts
		{
			get
			{
				if (loginContacts == null)
				{
					var query = new ZQuery(OrgContactSchema.PK, ContactPKs);
					var lloginContacts = new OrgContactCollection(Factory, query);
					lloginContacts.Load();
					loginContacts = lloginContacts;
				}

				return loginContacts;
			}
		}

		OrgContactCollection loginContacts;

		public void SetRememberMe(OrgContact contactForLogin)
		{
			if (AppInstance.ApplicationCookie != null && AppInstance.ApplicationCookie.CookieExist())
			{
				var password = AppInstance.ApplicationCookie.GetUserPassword();
				AppInstance.ApplicationCookie.Remove();
				AppInstance.ApplicationCookie.WriteUser(contactForLogin.OrgCode, contactForLogin.OC_Email, password);
			}
		}
	}
}
