using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class ResetMasterPasswordManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ResetMasterPasswordManager(string email, BusinessObjectFactory factory) : base(factory ?? new BusinessObjectFactory())
		{
			Email = email;
		}

		public ResetMasterPasswordManager(string email, BusinessObjectFactory factory, string orgCodeRestriction) : base(factory ?? new BusinessObjectFactory())
		{
			Email = email;
			OrgCodeRestriction = orgCodeRestriction;
		}

		public string Email { get; }
		public string OrgCodeRestriction { get; }

		protected GlbPersonCollection PersonsForPasswordChange
		{
			get
			{
				if (personsForPasswordChange == null && !string.IsNullOrEmpty(Email))
				{
					var webContactsFilter = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
					webContactsFilter.AddToFilter(OrgContactSchema.OC_Email, Email);
					webContactsFilter.AddToFilter(OrgContactSchema.OC_IsActive, true);
					webContactsFilter.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);

					if (!string.IsNullOrEmpty(OrgCodeRestriction))
					{
						var orgFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
						orgFilter.AddToFilter(OrgHeaderSchema.OH_Code, OrgCodeRestriction);
						webContactsFilter.AddSubQuery(OrgContactSchema.OC_OH, orgFilter, JoinCondition.And);
					}

					var personFilter = new ZDBOnlyQuery(typeof(GlbPerson));
					personFilter.AddSubQuery(webContactsFilter, JoinCondition.And);
					personsForPasswordChange = new GlbPersonCollection(Factory, personFilter);
				}

				return personsForPasswordChange;
			}
		}
		GlbPersonCollection personsForPasswordChange;

		public List<ChangeMasterPasswordPerson> PersonsForBinding
		{
			get
			{
				if (personsForBinding == null)
				{
					personsForBinding = new List<ChangeMasterPasswordPerson>();

					if (!string.IsNullOrEmpty(Email))
					{
						PersonsForPasswordChange.ForEach(x => personsForBinding.Add(ChangeMasterPasswordPerson.New(x)));
					}
				}

				return personsForBinding;
			}
		}

		List<ChangeMasterPasswordPerson> personsForBinding;
	}
}
