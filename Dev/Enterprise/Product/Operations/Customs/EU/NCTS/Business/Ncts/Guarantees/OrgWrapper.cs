using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class OrgHeaderWrapper : NonPersistentBusinessObject
		, IObsoleteValidation
	{
		protected OrgHeaderWrapper(MasterFiles.Business.OrgHeader organisation)
			: base(organisation.Factory)
		{
			this.Organisation = organisation;
		}

		#region static New

		public static OrgHeaderWrapper New(MasterFiles.Business.OrgHeader organisation)
		{
			OrgHeaderWrapper result = null;

			if (organisation != null)
			{
				result = organisation.Factory.GetCachedValue(organisation.PK.ToStringKey(), delegate
				{
					return new OrgHeaderWrapper(organisation);
				});
			}

			return result;
		}

		#endregion

		public NctsGuaranteeUnderOrgHeaderCollection BondDetails
		{
			get
			{
				if (fBondDetails == null)
				{
					fBondDetails = new NctsGuaranteeUnderOrgHeaderCollection(Organisation);
					fBondDetails.Load();
					Organisation.RegisterEditableChildObject(fBondDetails);
				}
				return fBondDetails;
			}
		}
		NctsGuaranteeUnderOrgHeaderCollection fBondDetails;

		public readonly MasterFiles.Business.OrgHeader Organisation;
	}
}
