using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.GB;

namespace Enterprise.Customs.GB.Business
{
	public class GBGlbCompanyWrapper : GlbCompanyWrapper, IGBGlbCompanyWrapper
	{
		protected GBGlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		#region ITBPasswordCollection

		[ChildEditable]
		public GlbExternalPasswordCollection_GB GBBPasswordCollection
		{
			get
			{
				if (gbGlbExternalPasswordCollection == null)
				{
					gbGlbExternalPasswordCollection = new GlbExternalPasswordCollection_GB(Company);
					gbGlbExternalPasswordCollection.Load();
					RegisterEditableChildObject(gbGlbExternalPasswordCollection);
				}

				return gbGlbExternalPasswordCollection;
			}
		}

		GlbExternalPasswordCollection_GB gbGlbExternalPasswordCollection;

		#endregion

		IGlbExternalPasswordCollection_GB IGBGlbCompanyWrapper.GBBPasswordCollection => GBBPasswordCollection;

		public virtual IEnumerable<GlbExternalPassword_GB> GetCredentialsToAuthorise() => GBBPasswordCollection.OfType<GlbExternalPassword_GB>().Where(x => x.Status == PasswordStatusList.Codes.Invalid && x.GP_PasswordType == PasswordTypesList.Codes.CDS);

		public override bool IsValidWrapper => Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom;
	}
}
