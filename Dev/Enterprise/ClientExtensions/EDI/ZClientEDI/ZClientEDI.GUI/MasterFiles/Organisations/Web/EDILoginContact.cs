using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Client.EDI.Web.Login
{
	public class EDILoginContact : LoginContact
	{
		protected EDILoginContact(OrgContact contact) : base(contact)
		{
		}

		public override ZString LinkedSystems
		{
			get
			{
				var linkedSystems = new StringBuilder();
				var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, Contact.PK);
				query.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);
				var linkedUserAccounts = Contact.Factory.Load<EdiCustomerUserAccount>(query);
				foreach (var userAccount in linkedUserAccounts)
				{
					var database = userAccount.Database;
					linkedSystems.Append((database.LD_Product == ProductTypes.Codes.CargoWiseOne || database.LD_Product == ProductTypes.Codes.CargoWiseNext)
						? FormattableString.Invariant($"{database.LD_Product}_{database.EnterpriseCode}{database.LD_ServerCode}, ")
						: FormattableString.Invariant($"{database.LD_Product}_{database.LD_LicenceType}, "));
				}

				var result = linkedSystems.ToString(0, linkedSystems.Length > 0 ? linkedSystems.Length - ", ".Length : 0);
				return result;
			}
		}
	}
}
