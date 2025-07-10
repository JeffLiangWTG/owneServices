using System;
using System.Linq;
using System.Web.Security.AntiXss;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class ChangeMasterPasswordPerson : NonPersistentBusinessObject
	{
		#region New

		public static ChangeMasterPasswordPerson New(GlbPerson person)
		{
			return new ChangeMasterPasswordPerson(person);
		}

		protected ChangeMasterPasswordPerson(GlbPerson person)
		{
			Person = person;
		}

		#endregion

		public GlbPerson Person { get; }

		public ZString Name => Person.PER_FullName;

		public virtual ZString RelatedAccounts
		{
			get
			{
				var accounts = Person.ContactCollection.Cast<OrgContact>().Where(x => x.OC_IsActive && x.OC_WebAccessEnabled).OrderBy(x => x.OrganisationCode)
					.Select(x => FormattableString.Invariant($"<p><b>{AntiXssEncoder.HtmlEncode(AntiXssEncoder.HtmlEncode(x.WorkingAddressCompanyName, false), false)}</b><br />{x.OrganisationCode} - {x.OC_Email}</p>")); // Non-translatable company codes
				return string.Join(string.Empty, accounts);
			}
		}
	}
}
