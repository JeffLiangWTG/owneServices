using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IN.Business;

sealed class JobComInvoiceHeaderJobDocAddressValidation : JobDocAddressValidation
{
	public JobComInvoiceHeaderJobDocAddressValidation(JobDocAddress parent, JobComInvoiceHeader invoiceHeader) : base(parent)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
	}

	readonly JobComInvoiceHeader invoiceHeader;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();
		var parent = Parent;
		var organisationPK = parent.OrganisationPK;
		var organisationPKInfo = parent.OrganisationPKInfo;
		if (invoiceHeader.IsExport)
		{
			switch (parent.DocAddressType)
			{
				case DocAddressType.BuyingParty when !parent.E2_AddressOverride:
					MandatoryValidation.MessageErrorIfNotEntered(organisationPKInfo);
					break;

				case DocAddressType.AuthorizedEconomicOperatorAddress:
					if (organisationPK.IsEmpty && !invoiceHeader.JZ_AuthorizedEconomicOperatorRole.IsEmpty)
					{
						organisationPKInfo.AddMessageError(Res.GetString("DA09BCDA-2594-4755-988B-ABB3081C1453", "You have not entered an AEO"));
					}
					if (!organisationPK.IsEmpty)
					{
						if (invoiceHeader.AuthorizedEconomicOperatorCode.IsEmpty)
						{
							organisationPKInfo.AddMessageError(Res.GetString("70A580B6-BA18-4C4A-AF8F-7C2D68D4E37A", "AEO Code not entered under selected Organization."));
						}
						if (invoiceHeader.AuthorizedEconomicOperatorCountry.IsEmpty)
						{
							organisationPKInfo.AddMessageError(Res.GetString("B5B0793A-D2C3-4AC4-90E0-77EFAFFA6A9E", "AEO Country not entered under selected Organization."));
						}
					}
					break;
			}
		}
	}
}
