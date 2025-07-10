using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLineSupportingDocumentsManager
{
	public InvoiceLineSupportingDocumentsManager(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		Argument.NotNull(invoiceLine.Factory, nameof(invoiceLine.Factory));
	}
	readonly JobComInvoiceLine invoiceLine;

	public void AddCustomsDecisionsSupportingDocumentIfApplicable()
	{
		(var documentType, var documentReference, _) = GetApplicableCustomsDecisionSupportingDocument();
		if (!documentType.IsEmpty)
		{
			var supportingDocument = invoiceLine.SupportingDocuments.GetFirstSupportingDocumentOrAddNewIfNotExists(documentType);
			supportingDocument.CSI_ReferenceNumber = documentReference;
		}
	}

	public void ValidateCustomsDecisionsSupportingDocument()
	{
		(var applicableCustomsDecisionSupportingDocumentType, _, var docRuleValueFrom) = GetApplicableCustomsDecisionSupportingDocument();

		if (!applicableCustomsDecisionSupportingDocumentType.IsEmpty && !invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == applicableCustomsDecisionSupportingDocumentType))
		{
			invoiceLine.AddRowMessageError(GetMessageError(docRuleValueFrom));
		}
	}

	(ZString documentType, ZString documentReference, ZString docRuleValueFrom) GetApplicableCustomsDecisionSupportingDocument()
	{
		ZString documentType = ZString.Empty;
		ZString documentReference = ZString.Empty;
		ZString docRuleValueFrom = ZString.Empty;

		if (Declaration != null && invoiceLine.CusProcedure != null)
		{
			var (authOwnerPK, declarantPK) = GetAuthoristationOwners();
			if (!(authOwnerPK.IsEmpty && declarantPK.IsEmpty))
			{
				var procedureType = GetProcedureType();
				var authorisation = GetAuthorisation(authOwnerPK, declarantPK, procedureType);
				if (authorisation != null)
				{
					var docRule = GetDocRule(authorisation);
					docRuleValueFrom = docRule?.CPR_ValueFrom ?? ZString.Empty;
					documentType = !docRuleValueFrom.IsEmpty ? docRuleValueFrom : GetDefaultSupportingDocumentType(authorisation, procedureType);
					documentReference = authorisation.CPH_Number;
				}
			}
		}

		return (documentType, documentReference, docRuleValueFrom);
	}

	string GetMessageError(string docRuleValueFrom)
	{
		ZString messageError = ZString.Empty;
		var procedureType = GetProcedureType();
		switch (procedureType)
		{
			case ProcedureType.IntoWarehouse:
				messageError = ValidationCaptions.InvoiceLine.GetCustomsDecisionsIntoWarehouseMissingDocumentCaption(docRuleValueFrom);
				break;
			case ProcedureType.InwardProcessing:
				messageError = ValidationCaptions.InvoiceLine.GetCustomsDecisionsInwardProcessingMissingDocumentCaption(docRuleValueFrom);
				break;
			case ProcedureType.OutwardProcessing:
				messageError = ValidationCaptions.InvoiceLine.GetCustomsDecisionsOutwardProcessingMissingDocumentCaption(docRuleValueFrom);
				break;
			default:
				break;
		}
		return messageError;
	}

	(ZGuid AuthOwnerPK, ZGuid DeclarantPK) GetAuthoristationOwners()
	{
		OrgHeader authOwner = null;
		if (Declaration.IsImport)
		{
			authOwner = Declaration.Importer;
		}
		else if (Declaration.IsExport)
		{
			authOwner = Declaration.Supplier;
		}

		return (authOwner?.PK ?? ZGuid.Empty, Declaration.Declarant?.Header.PK ?? ZGuid.Empty);
	}

	CusAuthorisationRule GetDocRule(CusAuthorisationHeader authorisation) => authorisation.CusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == ITCusAuthorisationRuleTypeList.Codes.Document);

	CusAuthorisationHeader GetAuthorisation(ZGuid authOwnerPK, ZGuid declarantPK, ProcedureType procedureType)
	{
		CusAuthorisationHeader[] authorisationHeaders = Enumerable.Empty<CusAuthorisationHeader>().ToArray();
		var authOwnerPKs = new[] { authOwnerPK, declarantPK }.Where(x => !x.IsEmpty).ToArray();

		switch (procedureType)
		{
			case ProcedureType.IntoWarehouse:
				authorisationHeaders = GetWarehouseAuthorisations(authOwnerPKs);
				break;
			case ProcedureType.InwardProcessing:
			case ProcedureType.OutwardProcessing:
				authorisationHeaders = GetInwardAuthorisations(authOwnerPKs, procedureType);
				break;
			default:
				break;
		}

		return TakeFirstAuthorisationHeaderOrderByOwner(authOwnerPK, declarantPK, authorisationHeaders);
	}

	CusAuthorisationHeader TakeFirstAuthorisationHeaderOrderByOwner(ZGuid authOwnerPK, ZGuid declarantPK, CusAuthorisationHeader[] authorisationHeaders)
	{
		return System.Array.Empty<CusAuthorisationHeader>()
					.Concat(authorisationHeaders.Where(x => x.CPH_OH_PermitHolder == authOwnerPK).OrderBy(x => x.CPH_Number))
					.Concat(authorisationHeaders.Where(x => x.CPH_OH_PermitHolder == declarantPK).OrderBy(x => x.CPH_Number))
					.FirstOrDefault();
	}

	ZString GetDefaultSupportingDocumentType(CusAuthorisationHeader authorisationHeader, ProcedureType procedureType)
	{
		ZString supportingDocumentType = ZString.Empty;
		switch (procedureType)
		{
			case ProcedureType.IntoWarehouse:
				supportingDocumentType = UniversalReferenceConstants.SupportingDocumentTypes.AuthorizationTypeSupportingDocumentTypeCorrelation[authorisationHeader.CPH_Type];
				break;
			case ProcedureType.InwardProcessing:
				supportingDocumentType = UniversalReferenceConstants.SupportingDocumentTypes.C601;
				break;
			case ProcedureType.OutwardProcessing:
				supportingDocumentType = UniversalReferenceConstants.SupportingDocumentTypes.C019;
				break;
			default:
				break;
		}
		return supportingDocumentType;
	}

	CusAuthorisationHeader[] GetWarehouseAuthorisations(ZGuid[] permitHeaderPks)
	{
		var ccpValue = invoiceLine.EntryInstruction?.WarehouseIDFor27 ?? ZString.Empty;
		return CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, customsWarehouseAuthorisationTypes.ToArray(), permitHeaderPks, Core.Constants.CountryCodes.Italy, Declaration.DateOfValuation, ITCusAuthorisationRuleTypeList.Codes.Location, ccpValue);
	}

	CusAuthorisationHeader[] GetInwardAuthorisations(ZGuid[] permitHeaderPks, ProcedureType procedureType)
	{
		var authType = procedureType == ProcedureType.InwardProcessing ? CusAuthorizationHeaderTypeList.Codes.InwardProcessing : CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
		return CusAuthorisationHeader.Loader.GetAuthorisations(Factory, Core.Constants.CountryCodes.Italy, new ZString[] { authType }, Declaration.DateOfValuation, permitHeaderPks);
	}

	ProcedureType GetProcedureType()
	{
		var procedure = invoiceLine.CusProcedure;
		ProcedureType procedureType = ProcedureType.Undefined;
		if (procedure != null)
		{
			if (procedure.IsIntoWarehouse())
			{
				procedureType = ProcedureType.IntoWarehouse;
			}
			else if (procedure.IsIntoInwardProcessing())
			{
				procedureType = ProcedureType.InwardProcessing;
			}
			else if (procedure.IsIntoOutwardProcessing())
			{
				procedureType = ProcedureType.OutwardProcessing;
			}
		}
		return procedureType;
	}

	BusinessObjectFactory Factory => invoiceLine.Factory;
	JobDeclaration Declaration => invoiceLine?.Declaration;

	readonly ImmutableArray<ZString> customsWarehouseAuthorisationTypes = new ZString[]
	{
				CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP,
				CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
				CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2
	}
	.ToImmutableArray();

	enum ProcedureType
	{
		Undefined,
		IntoWarehouse,
		InwardProcessing,
		OutwardProcessing,
	}
}
