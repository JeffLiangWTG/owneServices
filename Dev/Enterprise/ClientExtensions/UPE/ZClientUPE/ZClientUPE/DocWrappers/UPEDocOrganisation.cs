using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDocOrganisation : DocOrganisation
	{
		protected UPEDocOrganisation(IOrganisationDetails source, BusinessObjectFactory factoryForWrapper)
			: base(source, factoryForWrapper)
		{
		}

		public new static UPEDocOrganisation New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New((OrgHeader)factory.Load(typeof(OrgHeader), pK), factory);
		}

		public new static UPEDocOrganisation New(OrgHeader orgHeader, BusinessObjectFactory factoryForWrapper)
		{
			return (orgHeader != null) ? new UPEDocOrganisation(OrgHeaderSource.New(orgHeader, factoryForWrapper), factoryForWrapper) : null;
		}

		public ZString AccountNumber
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, GlbCompany.CurrentCompany.Country); }
		}

		public ZString UPSContactFax
		{
			get { return UPEDataRegistry.Instance.UPSContactFax.Value; }
		}

		#region Letter of Authority

		public ZDateTime LetterOfAuthorityExpirationDate
		{
			get
			{
				ZDateTime result = OrgHeader.LetterOfAuthorityExpirationDate;
				if (result.IsEmpty)
				{
					result = ZDateTime.Now.AddDays(30);
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected new UPEOrgHeader OrgHeader
		{
			get { return (UPEOrgHeader)base.OrgHeader; }
		}

		#endregion
	}
}
