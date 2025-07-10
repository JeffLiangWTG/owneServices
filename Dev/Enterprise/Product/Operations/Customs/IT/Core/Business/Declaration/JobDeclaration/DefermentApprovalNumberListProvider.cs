using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class DefermentApprovalNumberListProvider
{
	public DefermentApprovalNumberListProvider(JobDeclaration declaration, BusinessObjectFactory factory)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	readonly JobDeclaration declaration;
	readonly BusinessObjectFactory factory;

	public CodeDescriptionPairList GetDefermentApprovalNumberListForImport()
	{
		switch (PaymentMethod)
		{
			case ImportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions:
				return GetDefermentApprovalNumbersForOrgAddress(declaration.Declarant);
			case ImportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions:
				return GetDefermentApprovalNumberListForOrgHeader(declaration.Consignee);
			case ImportDefermentMethodList.Codes.ForwardersAccountFromCustomsDecisions:
				return GetDefermentApprovalNumberListForOrgHeader(declaration.Forwarder);
			case ImportDefermentMethodList.Codes.RepresentativesAccountFromCustomsDecisions:
				return GetDefermentApprovalNumbersForOrgAddress(declaration.Representative);
			default:
				return GetDefaultDefermentApprovalNumberList();
		}
	}

	public CodeDescriptionPairList GetDefermentApprovalNumberListForUcc6Export()
	{
		switch (PaymentMethod)
		{
			case Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions:
				return GetDefermentApprovalNumbersForOrgAddress(declaration.Declarant);
			case Ucc6ExportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions:
				return GetDefermentApprovalNumberListForOrgHeader(declaration.Importer);
			case Ucc6ExportDefermentMethodList.Codes.ForwardersAccountFromCustomsDecisions:
				return GetDefermentApprovalNumberListForOrgHeader(declaration.Forwarder);
			case Ucc6ExportDefermentMethodList.Codes.SuppliersAccountFromCustomsDecisions:
				return GetDefermentApprovalNumberListForOrgHeader(declaration.Supplier);
			case Ucc6ExportDefermentMethodList.Codes.ExportersAccountFromCustomsDecisions:
				return GetDefermentApprovalNumberListForOrgHeader(declaration.ExporterDocAddress?.Organisation);
			default:
				return GetDefaultDefermentApprovalNumberList();
		}
	}

	public CodeDescriptionPairList GetDefaultDefermentApprovalNumberList()
	{
		var source = declaration.VATDeferStrategy.GetPaymentMethodSourceWrapper();
		return source?.GetDefermentApprovalNumberList() ?? new CodeDescriptionPairList();
	}

	#region Implementation

	CodeDescriptionPairList GetCustomsDecisionsAuthorizationNumbers(ZGuid authorizationHolder)
	{
		var holderAuthorizationNumbers = CusAuthorisationHeader.Loader
			.GetAuthorisations(factory, Core.Constants.CountryCodes.Italy, new ZString[] { CusAuthorizationHeaderTypeList.Codes.DeferredPayment }, ZDate.Today, authorizationHolder)
			.Select(x => x.CPH_Number);

		var customsDecisionsAuthorizationNumbers = new CodeDescriptionPairList();

		foreach (var number in holderAuthorizationNumbers)
		{
			customsDecisionsAuthorizationNumbers.AddPair(number);
		}

		return customsDecisionsAuthorizationNumbers;
	}

	CodeDescriptionPairList GetDefermentApprovalNumberListForOrgHeader(OrgHeader orgHeader)
	{
		if (orgHeader is null)
		{
			return new CodeDescriptionPairList();
		}

		return GetCustomsDecisionsAuthorizationNumbers(orgHeader.PK);
	}

	CodeDescriptionPairList GetDefermentApprovalNumbersForOrgAddress(OrgAddress address)
	{
		if (address is null)
		{
			return new CodeDescriptionPairList();
		}

		return GetCustomsDecisionsAuthorizationNumbers(address.OA_OH);
	}

	ZString PaymentMethod => declaration.JE_PaymentMethod;

	#endregion
}
