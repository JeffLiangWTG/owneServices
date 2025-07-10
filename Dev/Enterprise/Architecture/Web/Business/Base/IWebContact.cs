using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	public interface IWebContact : IContactable, IFactoryProvider
	{
		ZGuid OC_OH { get; }
		ZString OC_ContactName { get; }
		ZString OC_Phone { get; }
		ZString OC_Mobile { get; }
		ZString OC_Email { get; }
		ZDateTime OC_WebContractSignedDate { get; }

		IWebOrg ParentWebOrg { get; }

		OrgContact GetContact();

#if DEBUG
		/// <summary>
		/// Sets date in memory for just this object. Does not save to db.
		/// </summary>
		void SetWebContractSignedDateForTest(ZDateTime date);
#endif
	}
}
