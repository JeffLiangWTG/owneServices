using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateCEI_InwardProcessingDescription();
			ValidateGrossWeight();
			ValidateForRowNotifications();
			ValidateReimportCountries();
			ValidateIdentificationMeans();
			ValidateProducts();
			ValidateDV1Details();
			ValidateCustomsOffices();
			ValidateCusAuthorizationUsages();
			ValidateLinkedInvoiceHeaders();
			ValidateRequiredFieldsForOutOfWarehouseWarehousing();
			ValidateSupportingDocumentsCount();
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();

			var parent = Parent;
			var info = parent.CEI_StyleInfo;
			var declaration = parent.JobDeclaration;
			if (declaration != null)
			{
				if (declaration.IsExport)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
					ValidatePartyConstellationToCheckIfSupplierNeeded();
					ValidateEconomicallyOutwardProcessing();
					ValidateCEI_Style000902();
					ValidateCEI_StyleReimportCountryNotEmpty();
					ValidateTransportInformation();
				}
				else if (declaration.IsImport)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);

					var errorMessage = CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, parent, true, true, true);
					if (!errorMessage.IsEmpty)
					{
						info.AddMessageError(errorMessage);
					}
				}
			}

			void ValidateEconomicallyOutwardProcessing()
			{
				if (parent.Style1stDigitIs1() && declaration.JE_EntryStyle != EntryStyleListExport.Codes.ExportNormal)
				{
					info.AddMessageError(Res.GetString("3D4CBC15-B620-452F-A708-95F6EE684E95", "The selected Type (Procedure) is only allowed for Entry Style of Type ‘EX’."));
				}
			}

			void ValidateCEI_Style000902()
			{
				if (parent.CEI_Style == ExportDeclarationTypeProcedureList.Codes._000902)
				{
					var hasRequiredAdditionalDocument =
						parent.Invoices
						.SelectMany(inv => inv.AdditionalInfos.Cast<AdditionalInfo>())
						.Any(ai => ai.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalInformation && ai.CSI_Code == UniversalReferenceConstants.AdditionalInfoCodes.X0000);

					if (!hasRequiredAdditionalDocument)
					{
						info.AddMessageError(Res.GetString("dc64192f-16f8-4ce8-a933-6c24892a5028", "Type (Procedure) \"000902\" requires an Additional Document of Kind \"INF\" and Full Type \"X0000\" on Invoice Header."));
					}
				}
			}

			void ValidateCEI_StyleReimportCountryNotEmpty()
			{
				if (parent.EnabledOutwardProcessing && parent.ReimportCountryCodes.Count == 0)
				{
					info.AddMessageError(Res.GetString("AB9FACAD-D056-4E38-AEFC-97B0A4168ABE", "You have not entered a Reimport Country/Region."));
				}
			}

			void ValidateTransportInformation()
			{
				if (parent.Style1stDigitIs0() && declaration.JE_TransportModeInland.IsEmpty)
				{
					info.AddMessageError(Res.GetString("9FFF2968-01C2-4CF6-81FB-0A96729E8082", "You have not entered Inland Transport Information."));
				}
			}
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			var parent = Parent;
			var declaration = parent.JobDeclaration;
			if (declaration != null)
			{
				var info = parent.CEI_SubStyleInfo;

				var style = parent.CEI_Style;
				var subStyle = parent.CEI_SubStyle;
				if (declaration.IsExport)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
					if ((declaration.IsFixedInstallation || declaration.IsFixedInstallationInland) && subStyle != ExportDeclarationTypeTimeList.Codes._20)
					{
						info.AddMessageError(Res.GetString("81805307-AFDB-4929-AEDD-904EBCFD669E", "Mode of Transport 'FIX' requires Type (Time) to be '20'."));
					}
				}
				else if (declaration.IsImport && style != ImportDeclarationTypeList.Codes.AVABR)
				{
					if (!info.ReadOnly)
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
					if (StyleAndSubStyleCombinationRequiringSpecificPreviousProcedureCodeDictionary.TryGetValue(style, out string[] subStyles)
						&& subStyles.Contains(subStyle.ToString())
						&& !ValidPreviousProcedureCodesForSpecificStyleAndSubStyleCombinations.Contains(parent.PreviousDocumentMaster.CSI_Procedure))
					{
						info.AddMessageError(Res.GetString("C51933CB-44E5-4787-B9B4-486B5F4D64B8", "If this is a Premature Input please add a Previous Document of one of the following types: ATNEU, AT-AV, AT-ZL, ESUMA, T1, T2, ATA, VER321, VO, TIR or OHNE."));
					}
				}
			}
		}

		protected override void CheckCEI_DateForDuty()
		{
			base.CheckCEI_DateForDuty();

			var parent = Parent;
			var jobDeclaration = parent.JobDeclaration;
			if (jobDeclaration != null && jobDeclaration.IsExport)
			{
				var dateForDuty = parent.CEI_DateForDuty;
				var targetInfo = parent.CEI_DateForDutyInfo;
				if (dateForDuty.IsEmpty)
				{
					if (parent.SubStyle1stDigitIs1() || parent.SubStyle1stDigitIs2())
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
					}
				}
				else if (dateForDuty.IsValid)
				{
					var referenceDate = new ZDate(ZDate.Today.Year, ZDate.Today.Month, 1);
					if (ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(parent.CEI_SubStyle) && (dateForDuty < referenceDate.AddMonths(-1) || dateForDuty >= referenceDate))
					{
						targetInfo.AddMessageError(Res.GetString("FD093E24-9306-45AC-947F-7D9F49F445D5", "The Decisive Date must be in previous month of current date if Type(Time) is ‘20’."));
					}
					else if (dateForDuty.IsInTheFutureDatePartOnly)
					{
						targetInfo.AddMessageError(Res.GetString("c0b74978-7ef9-4948-80b5-cd04a88fc7e4", "The Decisive Date must not be in the future."));
					}
				}
			}
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			var jobDeclaration = Parent.JobDeclaration;
			if (jobDeclaration != null && jobDeclaration.IsImport)
			{
				var loader = new RefCusProcedure.Loader(Parent.Factory);
				if (loader.LoadTop1FromCodeAndCountry(Parent.CEI_Style, ZString.Empty, jobDeclaration.GetDefaultDataGroupingCode(), ZDateTime.Now) != null)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_OA_Warehouse2Info);
				}
			}
		}

		protected override void CheckCEI_Procedure()
		{
			base.CheckCEI_Procedure();
			var parent = Parent;
			var jobDeclaration = parent.JobDeclaration;
			if (jobDeclaration != null)
			{
				if (jobDeclaration.IsImport && parent.CEI_Style != ImportDeclarationTypeList.Codes.AVABR)
				{
					var cpc = parent.CEI_Procedure;
					var cpcInfo = parent.CEI_ProcedureInfo;

					MandatoryValidation.MessageErrorIfNotEntered(cpcInfo);
					if ((!cpc.IsEmpty)
						&& (!cpc.StartsWith(CustomsProcedureCodeList.Import.ProcedureCode._49, StringComparison.OrdinalIgnoreCase))
						&& (parent.JobDeclaration.JE_EntryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory)
						&& (ImportDeclarationTypeList.IsForImportFromSpecialTerritoryEntryStyle(parent.CEI_Style)))
					{
						cpcInfo.AddMessageError(Res.GetString("98765CE0-488D-4D1C-ACF8-04DED3766876", "The Customs Procedure Code must start with '{0}' for this Entry Style.", CustomsProcedureCodeList.Import.ProcedureCode._49));
					}
				}
			}
		}

		public void ValidateReimportCountries()
		{
			if (Parent.EnabledOutwardProcessing && !Parent.ReimportCountryCodes.Any())
			{
				Parent.AddRowMessageError(Res.GetString("49251fec-aa8c-4d62-ae66-4fbd83e13565", "Please enter at least one Re-Import Country/Region!"));
			}
		}

		public void ValidateIdentificationMeans()
		{
			if (Parent.EnabledOutwardProcessing && !Parent.IdentificationMeanCodes.Any())
			{
				Parent.AddRowMessageError(Res.GetString("efa08426-5921-4442-af25-914005920c0b", "Please enter at least one Type of Identification Means."));
			}
		}

		public void ValidateProducts()
		{
			if (Parent.EnabledOutwardProcessing && !Parent.Products.Any())
			{
				Parent.AddRowMessageError(Res.GetString("8cb1dd3d-740e-4da8-9ae7-b192fff79be3", "Please enter a commodity code and a goods description!"));
			}
		}

		public void ValidatePartyConstellationToCheckIfSupplierNeeded()
		{
			var instruction = Parent;
			var declaration = instruction.JobDeclaration;

			instruction.RemoveRowMessageError(SupplierRequiredForSelectedCombinationOfTypeAndPartyConstellation);

			var supplierRequired = declaration.IsExport && instruction.Constellation1stDigitIs1();
			if (supplierRequired && declaration.JE_OH_Supplier == ZGuid.Empty)
			{
				instruction.AddRowMessageError(SupplierRequiredForSelectedCombinationOfTypeAndPartyConstellation);
			}
		}

		public void ValidateDV1Details()
		{
			var instruction = Parent;
			if (instruction.JobDeclaration.ZG_IsHighValueOvrd)
			{
				var pivots = instruction.DV1DetailsPivots;
				if (pivots.Any())
				{
					if (!pivots.Any(x => x.IsForEntryInstruction))
					{
						instruction.AddRowMessageError(Res.GetString("E539C541-FE0C-415E-8952-741FCD25DD7B", "D.V.1 Details must linked to the Entry Instructions"));
					}
				}
				else
				{
					instruction.AddRowMessageError(Res.GetString("E8D0A2EA-EE84-47F2-A75D-342FC9668CC0", "At least one record with D.V.1. Details must exist."));
				}
			}
		}

		public void ValidateCEI_InwardProcessingDescription()
		{
			ValidateCalculatedProperty(Parent.CEI_InwardProcessingDescriptionInfo);
		}

		protected void CheckCEI_InwardProcessingDescription()
		{
			var instruction = Parent;
			if (instruction.EnabledInwardProcessing
				&& instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_InwardProcessingDescriptionInfo);
			}
		}

		protected override ZBool AllRelatedInvoicesMustHaveSameCurrency => ZBool.True;

		protected override ZBool AllRelatedInvoicesMustHaveSameAgreedPlace => ZBool.True;

		protected override ZBool AllRelatedInvoicesMustHaveSameTransactionNature => !Parent.JobDeclaration.IsExport;

		new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		[ThreadSafe]
		static readonly ImmutableDictionary<string, string[]> StyleAndSubStyleCombinationRequiringSpecificPreviousProcedureCodeDictionary = new Dictionary<string, string[]>
		{
			{ ImportDeclarationTypeList.Codes.EZA, new[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB } },
			{ ImportDeclarationTypeList.Codes.EZL, new[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA } },
			{ ImportDeclarationTypeList.Codes.EAV, new[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA } },
			{ ImportDeclarationTypeList.Codes.VAV, new[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
			{ ImportDeclarationTypeList.Codes.VZA, new[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
			{ ImportDeclarationTypeList.Codes.VZL, new[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
		}.ToImmutableDictionary();

		[ThreadSafe]
		static readonly ImmutableHashSet<string> ValidPreviousProcedureCodesForSpecificStyleAndSubStyleCombinations = new HashSet<string>(new[]
		{
			PreviousProcedureList.Codes._ATNEU,
			PreviousProcedureList.Codes._ATAV,
			PreviousProcedureList.Codes._ATZL,
			PreviousProcedureList.Codes._ESUMA,
			PreviousProcedureList.Codes._T1,
			PreviousProcedureList.Codes._T2,
			PreviousProcedureList.Codes._ATA,
			PreviousProcedureList.Codes._VER321,
			PreviousProcedureList.Codes._VO,
			PreviousProcedureList.Codes._TIR,
			PreviousProcedureList.Codes._OHNE
		}).ToImmutableHashSet();

		void ValidateGrossWeight()
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsExport && Parent.InvoiceLines.Any() && !Parent.InvoiceLines.Any(x => !x.JI_Weight.IsEmpty))
			{
				Parent.AddRowMessageError(Res.GetString("EA665AD3-D152-4AF3-BBC5-B114F62CFFB2", "At least one Line for selected Entry Instruction must have a Gross Weight entered."));
			}
		}

		void ValidateForRowNotifications()
		{
			var parent = Parent;
			var declaration = parent.JobDeclaration;
			if (declaration != null)
			{
				if (declaration.IsImport && parent.CEI_Style != ImportDeclarationTypeList.Codes.AVABR)
				{
					ValidateFiscalReferences();
					ValidatePreviousDocuments();
					ValidateATLASSender();
				}
				else if (declaration.IsExport)
				{
					ValidatePreviousDocumentOfTypeN830Exists();
					ValidatePreviousDocumentOfType9DFEExists();
					ValidatePreviousDocumentOfTypeN955Exists();
				}
			}

			void ValidateFiscalReferences()
			{
				var fiscalReferences = parent.FiscalReferences.Cast<CusFiscalReference>().ToArray();

				if (parent.InvoiceLines.Any(x => x.JI_Procedure.StartsWith(CustomsProcedureCodeList.Import.ProcedureCode._42, StringComparison.OrdinalIgnoreCase)
					|| x.JI_Procedure.StartsWith(CustomsProcedureCodeList.Import.ProcedureCode._63, StringComparison.OrdinalIgnoreCase)))
				{
					if (!fiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR2)
						|| !(fiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR1)
							 || fiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR3)))
					{
						parent.AddRowMessageError(Res.GetString("9CB2E698-84E9-4421-AB43-2AFE4BF9CBD4", "A Fiscal Reference record of Type ('FR1' or 'FR3') and a Fiscal Reference record of Type 'FR2' are mandatory if CPC starts with '42' or '63'."));
					}
				}

				bool hasCombinationOfC07AndF48 = false;
				foreach (var invoiceLine in parent.InvoiceLines.Cast<JobComInvoiceLine>())
				{
					var concessions = invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Select(x => x.CY_Code.SubstringSafe(x.CY_Code.Length - 3)).ToList();
					concessions.Add(invoiceLine.JI_Procedure.SubstringSafe(invoiceLine.JI_Procedure.Length - 3));
					if (concessions.Contains(UniversalReferenceConstants.Concessions.C07) && concessions.Contains(UniversalReferenceConstants.Concessions.F48))
					{
						hasCombinationOfC07AndF48 = true;
						break;
					}
				}
				if (hasCombinationOfC07AndF48 && !fiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5))
				{
					parent.AddRowMessageError(Res.GetString("8C8E77E8-B7E9-4E4F-A5DA-BC3E992AEFC3", "A Fiscal Reference record of Type ('FR5' – IOSS No.) is mandatory for CPC combination Concession 'C07' and 'F48'."));
				}
			}

			void ValidatePreviousDocuments()
			{
				var previousDocuments = parent.PreviousDocuments;
				if (previousDocuments.Count == 0)
				{
					parent.AddRowMessageError(Res.GetString("50A64207-43EB-4ACD-A9A6-E69C2F946782", "This Entry Instruction has no previous documents. Without a previous document the entry may be rejected."));
				}
				else if (parent.PreviousDocumentMaster.CSI_Procedure == PreviousProcedureList.Codes._ATNEU)
				{
					if (previousDocuments.Cast<PreviousDocument>().Sum(x => x.CSI_Quantity) != parent.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Sum(y => y.PackQty)))
					{
						parent.AddRowWarning(Res.GetString("A03ECBBB-F782-4535-B72D-C326F7473B76", "The total Package Quantity captured in Previous Procedures doesn't match the total Packages of Invoice Lines."));
					}
				}
			}

			void ValidateATLASSender()
			{
				if (declaration.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin)
				{
					var declarationType = declaration.JE_DeclarantType;
					var declarationTypeIsSELOrIND = declarationType == RepresentationTypeList.Codes._1Self || declarationType == RepresentationTypeList.Codes._3Indirect;
					if (declarationTypeIsSELOrIND || declarationType == RepresentationTypeList.Codes._2Direct)
					{
						(var sender, var bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
						if (sender.EoriNumber.IsEmpty() || sender.EoriBranchSuffix.IsEmpty() || bin.IsEmpty)
						{
							if (declarationTypeIsSELOrIND)
							{
								parent.AddRowMessageError(Res.GetString("7898F6A0-3E3E-47F8-A792-E473ACEDFC0B", @"ATLAS-Sender: [14] Declarant must have the Registration Numbers/Codes of type 'EOR', 'EBS' and 'API'. Otherwise please set EORI Number, EORI Branch Suffix and Participant Identification Number in the Registry under Customs\Germany\ATLAS."));
							}
							else
							{
								parent.AddRowMessageError(Res.GetString("EF96861D-337C-4560-A214-A3497BD614B3", @"ATLAS-Sender: [14] Representative must have the Registration Numbers/Codes of type 'EOR', 'EBS' and 'API'. Otherwise please set EORI Number, EORI Branch Suffix and Participant Identification Number in the Registry under Customs\Germany\ATLAS."));
							}
						}
					}
				}
			}

			void ValidatePreviousDocumentOfTypeN830Exists()
			{
				var subStyle = parent.CEI_SubStyle;
				if (ExportDeclarationTypeTimeList.Is10(subStyle) && parent.Style1stDigitIs1() &&
					parent.InvoiceLines.Any() && !parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes.N830)))
				{
					parent.AddRowMessageError(GetRequiredPreviousDocumentMessage(UniversalReferenceConstants.SupportingDocumentTypes.N830, (NoResString)"Line"));
				}
				else if (ExportDeclarationTypeTimeList.Is11Or12(subStyle) && parent.Invoices.Any() && !parent.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes.N830)))
				{
					parent.AddRowMessageError(GetRequiredPreviousDocumentMessage(UniversalReferenceConstants.SupportingDocumentTypes.N830, (NoResString)"Header"));
				}
			}

			void ValidatePreviousDocumentOfType9DFEExists()
			{
				if (ExportDeclarationTypeTimeList.Is10(parent.CEI_SubStyle) && (parent.Style1stDigitIs0() || parent.Style1stDigitIs2()) &&
					parent.Invoices.Any() && !parent.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DFE)))
				{
					parent.AddRowMessageError(GetRequiredPreviousDocumentMessage(UniversalReferenceConstants.SupportingDocumentTypes._9DFE, (NoResString)"Header"));
				}
			}

			void ValidatePreviousDocumentOfTypeN955Exists()
			{
				if (ExportDeclarationTypeTimeList.Is13(parent.CEI_SubStyle) && ExportDeclarationTypeProcedureList.Is000000(parent.CEI_Style) &&
					parent.Invoices.Any() && !parent.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes.N955)))
				{
					parent.AddRowMessageError(GetRequiredPreviousDocumentMessage(UniversalReferenceConstants.SupportingDocumentTypes.N955, (NoResString)"Header"));
				}
			}

			string GetRequiredPreviousDocumentMessage(string type, string level)
			{
				return Res.GetString("F17354F0-B882-44DD-81DC-027258C449D8", "For the selected Type (Time + Procedure) you must enter the Previous Document Type {0} on Invoice {1} Level.", type, level);
			}
		}

		void ValidateCustomsOffices()
		{
			var parent = Parent;
			var declaration = parent.JobDeclaration;
			if (declaration != null && declaration.IsExport)
			{
				var customsOffices = declaration.CustomsOffices.Cast<DEOfficeCode>().ToArray();
				ValidateOfficeOfPresentation();
				ValidateSupplementaryDeclarationOffice();
				ValidateOfficeOfExit();
				ValidateActualExitOffice();

				void ValidateOfficeOfPresentation()
				{
					if (parent.Style4thDigitIs4() && !ExistCustomsOffice(EuOfficeCodesTypes.Codes.OfficeOfPresentation))
					{
						parent.AddRowMessageError(Res.GetString("5D3D700C-AD23-4838-ADB9-BA8388956217", "You have not entered an Office of Presentation in the Customs Offices grid on Declaration Tab."));
					}
				}

				void ValidateSupplementaryDeclarationOffice()
				{
					if (parent.Style5thDigitIs1() && !ExistCustomsOffice(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice))
					{
						parent.AddRowMessageError(Res.GetString("873D98FC-FABD-49AA-A2CD-867776B7DD2C", "You have not entered an Office of Supplement in the Customs Offices grid on Declaration Tab."));
					}
				}

				void ValidateOfficeOfExit()
				{
					if (parent.SubStyle1stDigitIs0() && !ExistCustomsOffice(EuOfficeCodesTypes.Codes.OfficeOfExit))
					{
						parent.AddRowMessageError(Res.GetString("AFBE7105-1B76-413B-A581-C00201370A67", "You have not entered an Office of Exit in the Customs Offices grid on Declaration Tab."));
					}
				}

				void ValidateActualExitOffice()
				{
					if (parent.SubStyle1stDigitIs1() && !ExistCustomsOffice(EuOfficeCodesTypes.Codes.ActualExitOffice))
					{
						parent.AddRowMessageError(Res.GetString("A1627633-7E12-46F0-A80F-F75FC1F2609E", "You have not entered an Actual Office of Exit in the Customs Offices grid on Declaration Tab."));
					}
				}

				ZBool ExistCustomsOffice(ZString officeCodesType) => customsOffices.Any(x => x.CY_Code == officeCodesType && !x.CY_Data.IsEmpty);
			}
		}

		void ValidateCusAuthorizationUsages()
		{
			var parent = Parent;
			var declaration = parent.JobDeclaration;
			if (declaration != null)
			{
				if (declaration.IsImport)
				{
					var errorMessages = CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(parent);
					errorMessages.ForEach(x => parent.AddRowMessageError(x));
				}
				else if (declaration.IsExport)
				{
					var errorMessages = CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(parent);
					errorMessages.ForEach(x => parent.AddRowMessageError(x));
				}
			}
		}

		void ValidateLinkedInvoiceHeaders()
		{
			var parent = Parent;
			if (parent.JobDeclaration.IsImport)
			{
				var invoices = parent.Invoices;
				if (invoices.Count() > 1)
				{
					if ((from JobComInvoiceHeader x in invoices select x.JZ_RX_NKInvoice_Currency).Distinct().Count() > 1)
					{
						parent.AddRowError(GetErrorMessage((NoResString)"Currency Codes"));
					}
					if ((from JobComInvoiceHeader x in invoices select x.JZ_IncoTerm).Distinct().Count() > 1)
					{
						parent.AddRowError(GetErrorMessage((NoResString)"Incoterm Codes"));
					}
					if ((from JobComInvoiceHeader x in invoices select x.JZ_IncoTermPlace).Distinct().Count() > 1)
					{
						parent.AddRowError(GetErrorMessage((NoResString)"Agreed Places"));
					}
					if ((from JobComInvoiceHeader x in invoices select x.ZG_AgreedPlaceCode).Distinct().Count() > 1)
					{
						parent.AddRowError(GetErrorMessage((NoResString)"Incoterm Keys"));
					}
					if ((from JobComInvoiceHeader x in invoices select x.JZ_ValuationCode).Distinct().Count() > 1)
					{
						parent.AddRowError(GetErrorMessage((NoResString)"Natures of Transaction"));
					}
					if (!invoices.Cast<JobComInvoiceHeader>().Select(x => x.JZ_OA_BuyerAddress).AllSame())
					{
						parent.AddRowError(GetErrorMessage((NoResString)"Buyer"));
					}
					if (!invoices.Cast<JobComInvoiceHeader>().Select(x => x.JZ_OA_SellerAddress).AllSame())
					{
						parent.AddRowError(GetErrorMessage((NoResString)"Seller"));
					}
				}
			}

			string GetErrorMessage(string name)
			{
				return Res.GetString("1A33768B-ACDC-490B-B935-56DFEA63F9EA", "Invoice Headers with different {0} are linked to this Entry Instruction.", name);
			}
		}

		void ValidateRequiredFieldsForOutOfWarehouseWarehousing()
		{
			var parent = Parent;
			var entry = parent.EntryHeader;
			if (!parent.CEI_OA_Warehouse.IsEmpty && entry != null && entry.IsExport && entry.IsOutOfWarehouseWarehousing)
			{
				var errors = entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
				if (!errors.IsEmpty)
				{
					parent.AddRowMessageError(errors);
				}

				var warehouseTransactionStatus = entry.CH_WarehouseTransactionStatus;
				if (warehouseTransactionStatus != WarehouseTransactionStatusList.Codes.OutwardCreatedPending && warehouseTransactionStatus != WarehouseTransactionStatusList.Codes.OutwardUpdatedPending)
				{
					entry.EntryInstruction.AddRowMessageError(
						Res.GetString(
							"2274f5b5-2dc3-4900-b147-e50751c31b0e",
							"Warehouse transaction status of Outward Created Pending or Outward Update Pending is required to send this {0}",
							entry.EntryInstruction.CEI_Description));
				}
			}
		}

		void ValidateSupportingDocumentsCount()
		{
			var parent = Parent;
			if (parent.JobDeclaration.IsImport)
			{
				var totalSupDocsCount = parent.Invoices.SelectMany(i => i.SupportingDocuments.Cast<SupportingDocument>()).Count();

				if (totalSupDocsCount > 20)
				{
					parent.AddRowMessageError(Res.GetString("5E2414C8-0872-4780-A349-B344877F5548", "The maximum number (20) of allowed Supporting Documents per Entry Instruction has been exceeded."));
				}
			}
		}

		string SupplierRequiredForSelectedCombinationOfTypeAndPartyConstellation => Res.GetString("F46A8009-63B8-4EB1-9966-7F52D95F0175", "For this Party Constellation a Supplier must be entered on the Declaration Tab.");
	}
}
