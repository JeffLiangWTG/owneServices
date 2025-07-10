using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class ValueSetStrategyHelperForAuthorisedEconomicOperator
	{
		public static void PerformAeoSupportingDocumentDefaulting(ZPropertyInfo zPropertyInfoThatHasChanged, Eu.JobDeclaration declaration)
		{
			if (GBCustomsDataRegistry.Instance.IncludeY02xSupportingDocumentsForAeoOrganisations.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), declaration.RegistryBranchPK, Guid.Empty))
			{
				bool shouldDeleteSupportingDocCosNewPartyIsBeingSetToEmpty = zPropertyInfoThatHasChanged.Value.IsEmpty;
				bool shouldAmendSupportingDocument = shouldDeleteSupportingDocCosNewPartyIsBeingSetToEmpty;
				string documentCodeToCreate = "";
				OrgHeader orgHeader = null;

				switch (zPropertyInfoThatHasChanged.Name)
				{
					case JobDeclaration.Schema.JE_OH_Importer:
						shouldAmendSupportingDocument = true;
						documentCodeToCreate = "Y023";
						orgHeader = declaration.Importer;
						break;
					case JobDeclaration.Schema.JE_OH_Supplier:
						shouldAmendSupportingDocument = true;
						documentCodeToCreate = "Y022";
						orgHeader = declaration.Supplier;
						break;
					case JobDeclaration.Schema.JE_OH_ShippingLine:
						shouldAmendSupportingDocument = true;
						documentCodeToCreate = "Y028";
						orgHeader = declaration.ShippingLine;
						break;
					case JobDeclaration.Schema.JE_OA_DeclarantAddress:
						shouldAmendSupportingDocument = true;
						documentCodeToCreate = "Y024";
						orgHeader = declaration.Declarant?.Header;
						break;
					case JobDocAddress.Schema.E2_OA_Address:
						shouldAmendSupportingDocument = zPropertyInfoThatHasChanged.HumanReadableName.Contains("Warehouse"); // is there a better way to respond to JobDocAddress setters? 
						documentCodeToCreate = "Y027";
						orgHeader = declaration.WarehouseAddress?.Header;
						break;
				}

				if (shouldAmendSupportingDocument)
				{
					CreateOrDeleteSupportingDocumentForAeo(documentCodeToCreate, orgHeader, declaration, shouldDeleteSupportingDocCosNewPartyIsBeingSetToEmpty);
				}
			}
		}

		static void CreateOrDeleteSupportingDocumentForAeo(string documentCodeToCreate, OrgHeader orgHeader, Eu.JobDeclaration declaration, bool shouldDeleteSupportingDocCosNewPartyIsBeingSetToEmpty)
		{
			if (orgHeader != null && !shouldDeleteSupportingDocCosNewPartyIsBeingSetToEmpty)
			{
				// Setting an org to a non-blank value....
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator);
				var orgCusCodes = (OrgCusCode[])orgHeader.CustomsCodes.Find(query);
				if (orgCusCodes != null && orgCusCodes.Length > 0)
				{
					ZString aeoCertificate = orgCusCodes[0].OK_RN_NKCodeCountry + orgCusCodes[0].OK_CustomsRegNo;
					if (orgCusCodes[0].OK_RN_NKCodeCountry == Constants.CountryCodes.Japan)
					{
						aeoCertificate = orgCusCodes[0].OK_CustomsRegNo;
						documentCodeToCreate = "Y031";
					}
					if (!aeoCertificate.IsEmpty)
					{
						MeasuresToTaxAndDocsHelper.FindExistingOrAddNewSupportingDocument(documentCodeToCreate, declaration, out var suppDoc);
						suppDoc.CSI_Actions = "P";//PartIfPaperReturnToTrader
						suppDoc.CSI_Availability = "J"; //PaperHeldByAuthorisedTrader
						suppDoc.CSI_ReferenceNumber = aeoCertificate.Left(SupportingDocument.Schema.ReferenceNumberMaxLength);
					}
				}
			}
			else
			{
				// Removing an org from a dec....
				MeasuresToTaxAndDocsHelper.FindExistingSupportingDocument(documentCodeToCreate, declaration, out var sd);
				sd?.Delete();
			}
		}
	}
}
