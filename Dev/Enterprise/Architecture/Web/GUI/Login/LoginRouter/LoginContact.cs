using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class LoginContact
	{
		#region New

		public static LoginContact New(OrgContact contact)
		{
			var type = TypeDecider.GetTypeForBinding(typeof(LoginContact));
			var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(OrgContact) }, null);
			return (LoginContact)constructor.Invoke(new object[] { contact });
		}

		protected LoginContact(OrgContact contact)
		{
			this.Contact = contact;
		}

		#endregion

		public OrgContact Contact { get; }

		public ZString OrganisationCode => Contact.OrganisationCode;

		public ZString OrganisationName => Contact.WorkingAddressCompanyName;

		public ZString Email => Contact.OC_Email;

		public ZBool PrimaryWorkplaceFlag => Contact.OC_IsPrimaryContact;

		public ZString PrimaryWorkplace => Contact.OC_IsPrimaryContact.ToString();

		public virtual ZString LinkedSystems => ZString.Empty;
	}
}
