using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderCarrierValidation : JobDocAddressValidation
	{
		#region Constructor

		public JPAFRHeaderCarrierValidation(AutoJobDocAddress parent, JPAFRHeader jpAFRHeader)
			: base(parent)
		{
			this.jpAFRHeader = jpAFRHeader;
		}

		readonly JPAFRHeader jpAFRHeader;

		#endregion

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.DocAddressType == DocAddressType.Carrier && Parent.HasRealOrganisation)
			{
				var carrierCode = Parent.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Japan);
				if (carrierCode.IsEmpty)
				{
					Parent.OrganisationPKInfo.AddWarning(ValidationConstants.Header.CarrierOrgHasNoJPCarrierCode);
				}
				else if (carrierCode.Length > 4)
				{
					Parent.OrganisationPKInfo.AddWarning(ValidationConstants.Header.CarrierOrgHaveJPCarrierCodeReachedMaxAllowed);
				}

				if (jpAFRHeader != null && jpAFRHeader.Consol != null)
				{
					if (!jpAFRHeader.Consol.Transports.Cast<Transport>().Any(transport => transport.CarrierPK == Parent.OrganisationPK))
					{
						Parent.OrganisationPKInfo.AddMessageError(ValidationConstants.Header.CarrierNotOnRouting);
					}
				}
			}
		}
	}
}
