using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	class DeltaIEJobDeclarationValueSetStrategy(JobDeclaration declaration) : JobDeclarationValueSetStrategy(declaration)
	{
		protected override ZString GetDeclarantType(OrgHeader orgHeader)
		{
			var result = RepresentationTypeList.Codes.IND;

			if (Declaration.ImporterDocumentaryAddress.E2_OA_Address == Declaration.JE_OA_DeclarantAddress)
			{
				result = Declaration.JE_OA_Representative.IsEmpty ? RepresentationTypeList.Codes.SEL : RepresentationTypeList.Codes.DIR;
			}

			return result;
		}

		protected override void DefaultDeclarantChanged()
		{
			base.DefaultDeclarantChanged();

			SetBox14Representation(Declaration.Declarant?.Header);
		}

		protected override void DefaultRepresentativeChanged()
		{
			base.DefaultRepresentativeChanged();

			SetBox14Representation(Declaration.Representative?.Header);
			DefaultJE_CustomsProfile();
		}

		protected override void DefaultImporterChanged()
		{
			base.DefaultImporterChanged();

			var shouldUseDirectRepresentation = !EUOrgImpAddInfo.Get(Declaration.Importer, Declaration.CountryCode)?.ZO_Box14UseIndirectRepresentation ?? false;

			if (shouldUseDirectRepresentation
				&& Declaration.IsUCC6AndIsImport
				&& Declaration.JE_OA_Representative.IsEmpty
				&& Declaration.ImporterDocumentaryAddress.Address != null)
			{
				Declaration.JE_OA_Representative = Declaration.JE_OA_DeclarantAddress;
				Declaration.JE_OA_DeclarantAddress = Declaration.ImporterDocumentaryAddress.E2_OA_Address;
			}
		}

		protected override void DefaultZG_AgreedCodePlace()
		{
			Declaration.ZG_AgreedPlaceCode = ZString.Empty;
		}

		protected override void DefaultJE_DeltaMode()
		{
			Declaration.JE_DeltaMode = ZString.Empty;
		}

		protected override void DefaultJE_DeclarationLanguage()
		{
			Declaration.JE_DeclarationLanguage = FRCustomsLanguageList.Codes.FR;
		}

		protected override void DefaultJE_CustomsProfile()
		{
			Declaration.JE_CustomsProfile = GetProfileDefaultValue();
		}

		ZString GetProfileDefaultValue()
		{
			var declaration = Declaration;
			var factory = declaration.Factory;
			List<OrgCusAccount> accounts = null;
			switch (declaration.JE_DeclarantType)
			{
				case RepresentationTypeList.Codes.SEL:
				case RepresentationTypeList.Codes.IND:
					accounts = RetrieveAndFilterAccounts(factory, declaration.Declarant?.Header?.PK ?? ZGuid.Empty);
					break;
				case RepresentationTypeList.Codes.DIR:
					accounts = RetrieveAndFilterAccounts(factory, declaration.Declarant?.Header?.PK ?? ZGuid.Empty);
					if (!accounts.Any())
					{
						accounts = RetrieveAndFilterAccounts(factory, declaration.Representative?.Header?.PK ?? ZGuid.Empty);
					}
					break;
			}

			return accounts != null && accounts.Count == 1 ? accounts.First().CZ_Account : ZString.Empty;
		}

		List<OrgCusAccount> RetrieveAndFilterAccounts(BusinessObjectFactory factory, ZGuid orgPK)
		{
			var result = new List<OrgCusAccount>();
			if (!orgPK.IsEmpty)
			{
				var accounts = OrgCusAccount.Loader.LoadByCodeAndCountry(factory, orgPK, OrgCusAccountCodeList.Codes.DEC, Core.Constants.CountryCodes.France);
				if (accounts.Any())
				{
					var accountsDCC = accounts.Where(account => account.CZ_Type.Equals(OrgCusAccountDeltaIETypeList.Codes.DCC));
					if (accountsDCC.Any())
					{
						result.AddRange(accountsDCC);
					}
					else
					{
						var accountsDCN = accounts.Where(account => account.CZ_Type.Equals(OrgCusAccountDeltaIETypeList.Codes.DCN));
						if (accountsDCN.Any())
						{
							result.AddRange(accountsDCN);
						}
						else
						{
							var accountsHDN = accounts.Where(account => account.CZ_Type.Equals(OrgCusAccountDeltaIETypeList.Codes.HDN));
							if (accountsHDN.Any())
							{
								result.AddRange(accountsHDN);
							}
						}
					}
				}
			}
			return result;
		}

		protected override void DefaultJE_CustomsGuaranteeNumberCore()
		{
			if (Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(inv => inv.HasGuaranteeConsumingProcedure))
			{
				base.DefaultJE_CustomsGuaranteeNumberCore();
			}
			else
			{
				Declaration.JE_CustomsGuaranteeNumber = ZString.Empty;
			}
		}

		void DefaultBypassCodeAndReason()
		{
			foreach (var instruction in Declaration.CustomsEntryInstructions)
			{
				instruction.ZG_BypassCode = ZString.Empty;
				instruction.ZG_BypassReason = ZString.Empty;
			}
		}

		void DefaultIsHighValueOvrd()
		{
			Declaration.ZG_IsHighValueOvrd = false;
		}

		protected override void SetDefaultOrSwapSubStyleIfNecessarily(CusEntryInstruction entryInstruction, ZDateTime dateInterestedIn)
		{
			entryInstruction.CEI_SubStyle = !dateInterestedIn.IsEmpty && dateInterestedIn <= ZDateTime.Today ? EntrySubstyleCodePairList.Codes.A : EntrySubstyleCodePairList.Codes.D;
		}

		protected override List<ZGuid> GetGuaranteeSourcePriorityList()
		{
			var declaration = Declaration;
			var organisations = new List<ZGuid>();

			if (declaration.IsImport)
			{
				if (declaration.JE_DeclarantType == RepresentationTypeList.Codes.DIR)
				{
					if (declaration.JE_OH_Importer is { IsValid: true } importerPk)
					{
						organisations.Add(importerPk);
					}
				}
				else if (declaration.JE_DeclarantType == RepresentationTypeList.Codes.IND || declaration.JE_DeclarantType == RepresentationTypeList.Codes.SEL)
				{
					if (declaration.Declarant?.Header.PK is { IsValid: true } declarantPk)
					{
						organisations.Add(declarantPk);
					}
				}
			}
			return organisations;
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_DeclarantType:
					Declaration.VATDeferStrategy.OnDeclarantTypeChanged();
					DefaultJE_CustomsGuaranteeNumber();
					break;
				case JobDeclaration.Schema.JE_MessageType:
					PopulateAdditionalInfo();
					break;
				case JobDeclaration.Schema.JE_ApplicationCode:
					PopulateAdditionalInfo();
					DefaultBypassCodeAndReason();
					DefaultIsHighValueOvrd();
					break;
				case JobDeclaration.Schema.ChargePaymentOrDestinationID:
					TogglePortCodeAdditionalReference();
					break;
			}
		}

		void PopulateAdditionalInfo()
		{
			var declaration = Declaration;
			if (declaration.IsImport)
			{
				var hasMandatoryInfo = DeltaIEDeclarationValidationHelper.HasMandatoryAdditionalInfos(declaration.AdditionalInfos);
				if (!hasMandatoryInfo)
				{
					var additionalInfo = declaration.AdditionalInfos.AddNew();
					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FretCargo;
				}
				if (FRCustomsDataRegistry.DeltaIFallbackIsActive)
				{
					var hasFallbackProcedureReferenceInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration.AdditionalInfos);
					if (!hasFallbackProcedureReferenceInfo)
					{
						var additionalInfo = declaration.AdditionalInfos.AddNew();
						additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.FallbackProcedure;
						var fallbackSettings = (FallbackSettings)FRCustomsDataRegistry.Instance.DeltaIMode.Value;
						additionalInfo.CSI_ReferenceNumber = fallbackSettings.InvocationReason;
					}
					var hasFallbackProcedureInformationInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration.AdditionalInfos);
					if (!hasFallbackProcedureInformationInfo)
					{
						var additionalInfo = declaration.AdditionalInfos.AddNew();
						additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FallbackProcedure;
					}
				}
			}
		}

		void TogglePortCodeAdditionalReference()
		{
			var portCodeAdditionalReference = DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos);

			if (declaration.ChargePaymentOrDestinationID.IsEmpty && portCodeAdditionalReference != null)
			{
				declaration.AdditionalInfos.RemoveAndDelete(portCodeAdditionalReference);
			}
			else if (!declaration.ChargePaymentOrDestinationID.IsEmpty)
			{
				if (portCodeAdditionalReference == null)
				{
					portCodeAdditionalReference = declaration.AdditionalInfos.AddNew();
					portCodeAdditionalReference.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					portCodeAdditionalReference.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.PortCode;
				}
				portCodeAdditionalReference.CSI_ReferenceNumber = declaration.ChargePaymentOrDestinationID;
			}
		}

		public void SetRepresentationMode(OrgHeader orgHeader) => SetBox14Representation(orgHeader);
	}
}
