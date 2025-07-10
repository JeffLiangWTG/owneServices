using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationAuthorizationDataProvider : IAuthorizationListDataProvider, IAuthorizationHeaderDataProvider
{
	public JobDeclarationAuthorizationDataProvider(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	#region IAuthorizationListDataProvider

	IEnumerable<ZString> IAuthorizationListDataProvider.AuthorizationTypes => GetAuthorisationTypes();

	IEnumerable<ZGuid> IAuthorizationListDataProvider.GetEligibleHolders()
	{
		return GetEligibleHolders()
			.Select(x => x.PK)
			.Distinct();
	}

	#endregion

	#region IAuthorizationHeaderDataProvider Members

	ZString IAuthorizationHeaderDataProvider.AuthorizationNumber => declaration.ZG_AuthorisationNumber;

	IEnumerable<ZString> IAuthorizationHeaderDataProvider.AuthorizationTypes => GetAuthorisationTypes();

	ZGuid IAuthorizationHeaderDataProvider.HolderPk => GetAuthorisationHolderPK(declaration.ZG_AuthorisationNumber);

	#endregion

	#region Implementation

	IEnumerable<OrgHeader> GetEligibleHolders()
	{
		var eligbleHolders = Enumerable.Empty<OrgHeader>();
		if (IsImport)
		{
			eligbleHolders = GetImportEligibleHolders();
		}
		else if (IsExport)
		{
			eligbleHolders = GetExportEligibleHolders();
		}

		return eligbleHolders.WhereNotNull();
	}

	IEnumerable<OrgHeader> GetImportEligibleHolders() => GetEligibleHolders(declaration.Importer);

	IEnumerable<OrgHeader> GetExportEligibleHolders() => GetEligibleHolders(declaration.Supplier);

	IEnumerable<OrgHeader> GetEligibleHolders(params OrgHeader[] eligibleHolders)
	{
		foreach (var eligibleHolder in eligibleHolders)
		{
			yield return eligibleHolder;
		}

		yield return declaration.DeclarantOrgAddress?.Header;
		yield return declaration.ShippingLine;
		yield return declaration.Forwarder;
		yield return declaration.ContainerTerminalOperatorDocAddress.Organisation;
		yield return declaration.DepotDocAddress.Organisation;
		yield return declaration.ContainerYardDocAddress.Organisation;
		yield return declaration.ControllingAgent;
		yield return declaration.ControllingCustomer;
		yield return declaration.ExternalBroker;
		yield return declaration.RepresentativeOrgAddress?.Header;
		yield return declaration.SellerAddress?.Header;
		yield return declaration.ManufacturerAddress?.Header;
	}

	IEnumerable<ZString> GetAuthorisationTypes()
	{
		if (IsImport)
		{
			return GetImportAuthorisationTypes();
		}

		if (IsExport)
		{
			return GetExportAuthorisationTypes();
		}

		return Enumerable.Empty<ZString>();
	}

	IEnumerable<ZString> GetImportAuthorisationTypes()
	{
		yield return CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport;
		yield return CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport;
		yield return CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
		yield return CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
		yield return CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
	}

	IEnumerable<ZString> GetExportAuthorisationTypes()
	{
		yield return CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport;
	}

	ZBool IsImport => declaration.IsImport;

	ZBool IsExport => declaration.IsExport;

	ZGuid GetAuthorisationHolderPK(ZString authorisationNumber)
	{
		var holderOrganisationCode = new ZString(declaration.AddInfoLookups.AuthorisationNumberList.GetDescriptionFromCode(authorisationNumber) ?? ZString.Empty);
		if (holderOrganisationCode.IsEmpty)
		{
			return ZGuid.Empty;
		}

		return GetEligibleHolders()
			.FirstOrDefault(x => x.OH_Code == holderOrganisationCode)?.PK ?? ZGuid.Empty;
	}

	#endregion
}
