using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.IE.Business.Constants;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants.ProcedureCodes;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override ZBool ShouldValidateDutyRule => !(Parent.JI_PrimaryPreference.IsEmpty && Parent.EntryInstruction is CusEntryInstruction instruction &&
			instruction.IsH2 && Declaration is JobDeclaration declaration && declaration.IsUCC5);

		protected override void CheckJI_Tariff_NoPackage()
		{
			if (Declaration is JobDeclaration dec)
			{
				CheckJI_Tariff_NoPackage_Import(dec);
			}
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			var parent = Parent;
			if (parent.JI_LinePrice == ZDecimal.Zero && parent.EntryInstruction is CusEntryInstruction instruction && EntrySubStyleList.EntrySubStyleListBR8072.Contains(instruction.CEI_SubStyle) && ImportDeclarationTypeList.h1H3H4H5.Contains(instruction.CEI_Style))
			{
				parent.JI_LinePriceInfo.AddMessageError(Res.GetString("5503AFFE-293A-42E6-B515-8AE3C0444A9C", "[BR8072] Price is required when Declaration is H1, H3, H4, or H5, and Additional Declaration Type is A or D"));
			}
		}

		protected override bool IsTariffMandatory => false;

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			var parent = Parent;
			if (parent.JI_Tariff.IsEmpty && parent.EntryInstruction is CusEntryInstruction instruction && ImportDeclarationTypeList.DeclarationTypeListCD0104.Contains(instruction.CEI_Style))
			{
				parent.JI_TariffInfo.AddMessageError(Res.GetString("FF65D790-277B-4DFE-BEC3-BBAC29880848", "[CD0104] Tariff is required when Declaration is H1, H2, H3, H4, H5, or H6"));
			}

			if (!parent.JI_Tariff.StartsWith(Tariff03) && parent.AdditionalProcedureCodesIncludingConcession.Any(UniversalReferenceConstants.ProcedureCodes.additionalProcedureCodesForBR1115.Contains))
			{
				parent.JI_TariffInfo.AddMessageError(Res.GetString("E737A6A7-6FCF-408B-BD5B-9F2B4BF98C2C", "[BR1115] Tariff Code must start with 03 when Additional Procedure F21 or F22 is declared."));
			}
		}

		protected override void CheckJI_ZZF_NKTaxType()
		{
			base.CheckJI_ZZF_NKTaxType();
			if (Parent.EntryInstruction is CusEntryInstruction instruction
				&& instruction.CEI_Style.ToString() is ImportDeclarationTypeList.Codes.H1 or ImportDeclarationTypeList.Codes.H5
				&& !Parent.JI_Calc_AdditionalProcedureCodes.Contains(UniversalReferenceConstants.ProcedureCodes.Concession.F05)
			)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ZZF_NKTaxTypeInfo);
			}
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();

			if (Parent.EntryInstruction is CusEntryInstruction instruction)
			{
				CheckPrimaryPreference_BR4170(instruction);
				CheckPrimaryPreference_BR4181(instruction);
				CheckPrimaryPreference_BR5150(instruction);
				CheckPrimaryPreference_BR5160(instruction);
				CheckPrimaryPreferenceNotEmpty(instruction);
			}

			CheckPrimaryPreference_BR4175();
			CheckPrimaryPreference_BR4176();
			CheckPrimaryPreference_BR5157();
			CheckPrimaryPreference_BR5158();
			CheckPrimaryPreference_BR5159();
		}
		void CheckPrimaryPreference_BR5150(CusEntryInstruction instruction)
		{
			var parent = Parent;
			ZBool isContainSpecificDocumentType = new ZBool(instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._N954) ||
															instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._N864) ||
															instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._U162) ||
															instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._U163) ||
															instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._U168) ||
															instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._U169) ||
															instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._U170) ||
															instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._U171));

			if (parent.PreferenceCode == Constants.PreferenceCode.Code3 &&
				parent.ZG_CountryOfSupply == Core.Constants.CountryCodes.Andorra &&
				!isContainSpecificDocumentType)
			{
				parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("3C02D9D5-E075-4DD4-AA55-236A5207AB4B", "[BR5150] Please entry a Supporting Document of type 'N954' or 'N864' or 'U162' or 'U163' or 'U168' or 'U169' or 'U170' or 'U171' under Supporting Documents."));
			}
		}

		void CheckPrimaryPreferenceNotEmpty(CusEntryInstruction instruction)
		{
			if (instruction.CEI_Style == ImportDeclarationTypeList.Codes.H1 || instruction.CEI_Style == ImportDeclarationTypeList.Codes.H5)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PrimaryPreferenceInfo);
			}
		}

		#region BR5158

		void CheckPrimaryPreference_BR5158()
		{
			var parent = Parent;
			if (parent.JI_PrimaryPreference.StartsWith(PrimaryPreference3) &&
				parent.ZG_CountryOfSupply.EqualsIgnoringCase(Core.Constants.CountryCodes.Japan) &&
				!HasSupportingDocumentForBR5158)
			{
				parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("BB5ABA14-55D6-43CF-B1EF-7EEA3C72E205", "[BR5158] A supporting document of type U110, U111, U112 is required on the entry instruction when there is at least an invoice line where preference starts with 3 and country of preferential origin is JP."));
			}
		}

		bool HasSupportingDocumentForBR5158
		{
			get
			{
				var parent = Parent;
				return parent.Factory.GetValue(ref hasSupportingDocumentForBR5158Cached,
					() => (parent.EntryInstruction is CusEntryInstruction instruction && ContainsSupportingDocumentForBR5158(instruction.SupportingDocuments))
					|| (parent.InvoiceHeader is JobComInvoiceHeader invoiceHeader && ContainsSupportingDocumentForBR5158(invoiceHeader.SupportingDocuments)));
			}
		}
		CachedProperty<bool> hasSupportingDocumentForBR5158Cached;

		static bool ContainsSupportingDocumentForBR5158(SupportingDocumentCollection supportingDocuments)
		{
			return supportingDocuments.Cast<SupportingDocument>().Any(doc => IsValidForBR5158(doc.CSI_Code));
		}

		static bool IsValidForBR5158(ZString supportingDocumentCode)
		{
			switch (supportingDocumentCode)
			{
				case Constants.SupportingDocumentCodes._U110:
				case Constants.SupportingDocumentCodes._U111:
				case Constants.SupportingDocumentCodes._U112:
					return true;
				default:
					return false;
			}
		}

		#endregion

		#region BR5157

		void CheckPrimaryPreference_BR5157()
		{
			var parent = Parent;
			if (parent.JI_PrimaryPreference.StartsWith(PrimaryPreference3) &&
				parent.ZG_CountryOfSupply.EqualsIgnoringCase(Core.Constants.CountryCodes.UnitedKingdom) &&
				!HasSupportingDocumentForBR5157)
			{
				parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("525E096C-E516-4BC1-ADAC-27FE37541E43", "[BR5157] A supporting document of type U116, U117, or U118 is required on the entry instruction when there is at least an invoice line with preference starting with 3 and the country of preferential origin is GB."));
			}
		}

		bool HasSupportingDocumentForBR5157
		{
			get
			{
				var parent = Parent;
				return parent.Factory.GetValue(ref hasSupportingDocumentForBR5157Cached,
					() => (parent.EntryInstruction is CusEntryInstruction instruction && ContainsSupportingDocumentForBR5157(instruction.SupportingDocuments))
					|| (parent.InvoiceHeader is JobComInvoiceHeader invoiceHeader && ContainsSupportingDocumentForBR5157(invoiceHeader.SupportingDocuments)));
			}
		}
		CachedProperty<bool> hasSupportingDocumentForBR5157Cached;

		static bool ContainsSupportingDocumentForBR5157(SupportingDocumentCollection supportingDocuments)
		{
			return supportingDocuments.Cast<SupportingDocument>().Any(doc => IsValidForBR5157(doc.CSI_Code));
		}

		static bool IsValidForBR5157(ZString supportingDocumentCode)
		{
			switch (supportingDocumentCode)
			{
				case Constants.SupportingDocumentCodes._U116:
				case Constants.SupportingDocumentCodes._U117:
				case Constants.SupportingDocumentCodes._U118:
					return true;
				default:
					return false;
			}
		}

		#endregion

		#region BR4170

		void CheckPrimaryPreference_BR4170(CusEntryInstruction instruction)
		{
			if (!instruction.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H1) && IsValidForBR4170(Parent.JI_PrimaryPreference.Right(2)))
			{
				Parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("2D737B62-6B35-46FA-9015-2F2812BA3980", "[BR4170] Quota request preferences where the last two digits of the code is '20', '25' or '28' are only allowed when Declaration Type is 'H1'."));
			}
		}

		static bool IsValidForBR4170(ZString lastTowCodesOfPrimaryPreference)
		{
			switch (lastTowCodesOfPrimaryPreference)
			{
				case UniversalReferenceConstants.PrimaryPreference.LastTwoCodes._20:
				case UniversalReferenceConstants.PrimaryPreference.LastTwoCodes._25:
				case UniversalReferenceConstants.PrimaryPreference.LastTwoCodes._28:
					return true;
				default:
					return false;
			}
		}

		#endregion

		#region BR4175

		void CheckPrimaryPreference_BR4175()
		{
			var parent = Parent;
			if (IsPrimaryPreferenceValid(parent.JI_PrimaryPreference) && HasSupportingDocumentForBR4175())
			{
				parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("B2FB6795-0F54-4742-9438-01D507E03073", "[BR4175] Please enter a Supporting Document Type of type 'U164' or 'U165' or ('U165' and 'U167' jointly) under Supporting Documents."));
			}
		}

		bool HasSupportingDocumentForBR4175()
		{
			var parent = Parent;

			var allTransportDocument = parent.EntryInstruction?.SupportingDocuments.Cast<SupportingDocument>();

			if (parent.InvoiceHeader is JobComInvoiceHeader invoiceHeader)
			{
				var allTransportDocumentInvoiceHeader = parent.InvoiceHeader.SupportingDocuments.Cast<SupportingDocument>();
				allTransportDocument = allTransportDocument?.Union(allTransportDocumentInvoiceHeader) ?? allTransportDocumentInvoiceHeader;
			}

			return allTransportDocument != null && allTransportDocument.Any(doc => doc.CSI_Code == Constants.SupportingDocumentCodes._C100) && allTransportDocument.All(doc => doc.CSI_Code != Constants.SupportingDocumentCodes._U164 && doc.CSI_Code != Constants.SupportingDocumentCodes._U165);
		}

		#endregion

		#region BR4176

		void CheckPrimaryPreference_BR4176()
		{
			var parent = Parent;
			if (IsPrimaryPreferenceValid(parent.JI_PrimaryPreference))
			{
				var invoice = parent.InvoiceHeader;
				var instruction = parent.EntryInstruction;
				if (!invoice.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._C100) &&
					instruction?.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._C100) == false)
				{
					if (!invoice.HasU164OrU166OrN865SupportingDocument &&
						instruction?.HasU164OrU166OrN865SupportingDocument == false)
					{
						parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("6b1024b1-70c5-40b7-b816-bb2e8dbacb9c", "[BR4176] Please enter a Supporting Document of type 'U164' or 'U166' or 'N865' under Supporting Documents when Preference is '200' or '218' or '220' or '225' or '250' and there is no Supporting Document of type 'C100' declared on the Invoice Header or Entry Instruction."));
					}

					if (invoice.HasU165OrU167SupportingDocument ||
						instruction?.HasU165OrU167SupportingDocument == true)
					{
						parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("01474884-c564-4443-a51f-ec06f864db8e", "[BR4176] The Supporting Document of 'U165' or 'U167' under Supporting Documents / Invoice Header is not allowed when Preference is '200' or '218' or '220' or '225' or '250' and there is no Supporting Document of type 'C100' declared on the Invoice Header or Entry Instruction."));
					}
				}
			}
		}

		#endregion

		static bool IsPrimaryPreferenceValid(ZString primaryPreference)
		{
			switch (primaryPreference)
			{
				case UniversalReferenceConstants.PrimaryPreference.Codes.GSPRateWithoutConditionsOfLimits:
				case UniversalReferenceConstants.PrimaryPreference.Codes.TariffSuspensionSubjectToCertificateUnderGSP:
				case UniversalReferenceConstants.PrimaryPreference.Codes.TariffQuotaUnderGSP:
				case UniversalReferenceConstants.PrimaryPreference.Codes.TariffQuotaSubjectToCertificateUnderGSP:
				case UniversalReferenceConstants.PrimaryPreference.Codes.ApplicationOfGSPRatesSubjectToCertificate:
					return true;
				default:
					return false;
			}
		}

		void CheckPrimaryPreference_BR4181(CusEntryInstruction instruction)
		{
			if (instruction.CEI_Style == ImportDeclarationTypeList.Codes.H5 &&
				Parent.JI_PrimaryPreference != UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty)
			{
				Parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("3941CECD-3698-4BCA-A9C2-3149BAB27D76", "[BR4181] Preference must be 100 when Declaration is H5."));
			}
		}

		void CheckPrimaryPreference_BR5159()
		{
			var parent = Parent;
			if (parent.JI_PrimaryPreference.StartsWith(PrimaryPreference3)
				&& parent.ZG_CountryOfSupply == Core.Constants.CountryCodes.Pakistan
				&& !HasSupportingDocumentForBR5159)
			{
				parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("6CF788B4-8CAA-4CEA-9365-F7575C311CF7", "[BR5159] A supporting document of type U164 or U165 is required on the entry instruction when there is at least an invoice line where preference starts with 3 and country of preferential origin is PK."));
			}
		}

		bool HasSupportingDocumentForBR5159
		{
			get
			{
				var parent = Parent;
				return parent.Factory.GetValue(ref hasSupportingDocumentForBR5159Cached,
					() => (parent.EntryInstruction is CusEntryInstruction instruction && ContainsSupportingDocumentForBR5159(instruction.SupportingDocuments))
					|| (parent.InvoiceHeader is JobComInvoiceHeader invoiceHeader && ContainsSupportingDocumentForBR5159(invoiceHeader.SupportingDocuments)));
			}
		}
		CachedProperty<bool> hasSupportingDocumentForBR5159Cached;

		static bool ContainsSupportingDocumentForBR5159(SupportingDocumentCollection supportingDocuments)
		{
			return supportingDocuments.Cast<SupportingDocument>().Any(doc => IsValidForBR5159(doc.CSI_Code));
		}

		static bool IsValidForBR5159(ZString supportingDocumentCode)
		{
			switch (supportingDocumentCode)
			{
				case Constants.SupportingDocumentCodes._U164:
				case Constants.SupportingDocumentCodes._U165:
					return true;
				default:
					return false;
			}
		}

		void CheckPrimaryPreference_BR5160(CusEntryInstruction instruction)
		{
			if (Parent.JI_PrimaryPreference == UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota &&
				Parent.ZG_CountryOfSupply == Core.Constants.CountryCodes.KoreaSouth &&
				!instruction.HasSupportingDocumentWithCode(Constants.SupportingDocumentCodes._U059))
			{
				Parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("42f31c93-5a10-4b82-a5e2-a3f8dcf8ea7e", "[BR5160] A supporting document of type U059 is required on the entry instruction when there is at least an invoice line where preference is 320 and the country of preferential origin is KR."));
			}
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();

			var parent = Parent;
			var propertyInfo = parent.JI_ProcedureInfo;
			var entryInstruction = parent.EntryInstruction;
			var procedure = parent.JI_Procedure;
			if (!procedure.IsEmpty)
			{
				if (IsPreviousProcedureCodesForBR8078(parent.RequestedPreviousProcedure) && !parent.HasChargeType(AISChargeCodeList.Codes._2X))
				{
					propertyInfo.AddMessageError(Res.GetString("0272261C-2B52-4A42-8A73-2D8843812A62", "[BR8078] Please enter Charge '2X' under Invoice Header > Invoice Charges when Requested Procedure is '6121' or '7121'."));
				}

				if (entryInstruction != null)
				{
					var additionalProcedures = parent.AdditionalProcedureCodesIncludingConcession;

					if (entryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.H1)
					{
						CheckProcedureCodesBR600000(propertyInfo, parent.AdditionalProcedureCodesIncludingConcession);
						CheckProcedureBR3399(parent, parent.AdditionalProcedureCodesIncludingConcession, propertyInfo);
					}
					CheckProcedureCodesBR600009(propertyInfo, additionalProcedures, entryInstruction);

					var calcConcession = parent.JI_Calc_Concession;
					CheckProcedureCodesBR11106(x => x.Contains(calcConcession), entryInstruction, propertyInfo);
					CheckProcedureCodesBR11107(x => x.Contains(calcConcession), entryInstruction, propertyInfo);

					if (parent.JI_Calc_PreviousProcedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._71 && parent.CusProcedure is Universal.RefCusProcedure cusProcedure)
					{
						if (cusProcedure.IsIntoWarehouse())
						{
							var fromWarehouse = entryInstruction.Warehouse;
							if (fromWarehouse?.GetRegoCodeOfThisAddress(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Ireland).IsEmpty ?? true)
							{
								parent.JI_ProcedureInfo.AddMessageError(Res.GetString("A15663AB-92FA-4925-99A8-2CC6600EEE6B", "[BR8062] Please enter a EORI Number for From Warehouse."));
							}
						}

						if (cusProcedure.IsOutOfWarehouse())
						{
							var toWarehouse = entryInstruction.Warehouse2;
							if (toWarehouse?.GetRegoCodeOfThisAddress(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Ireland).IsEmpty ?? true)
							{
								parent.JI_ProcedureInfo.AddMessageError(Res.GetString("A2038A84-F346-449C-962B-B2414FE3801B", "[BR8062] Please enter a EORI Number for To Warehouse."));
							}
						}
					}
				}

				if (parent.IsBR2001PreviousProcedureCodeUsed && !parent.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader)
				{
					propertyInfo.AddMessageError(Res.GetString("D2D37249-C4FB-487E-996E-1486594DA6FC", "[BR2001] Please enter an MRN number under 'Previous Documents' if that MRN number applies to all invoice lines of the entry. Alternatively, enter an MRN number under 'Invoice Lines > Previous Documents' if that MRN number applies specifically to this invoice line."));
				}
			}

			CheckRequestedProcedure();

			CheckProcedureCodesBR599999(entryInstruction, parent, propertyInfo);
			CheckProcedureCodesBR1126();
		}

		void CheckProcedureCodesBR1126()
		{
			var parent = Parent;
			if (parent.EntryInstruction is CusEntryInstruction instruction && instruction.IsH5 && !IsTradeWithSpecialFiscalTerritories(Parent))
			{
				parent.JI_ProcedureInfo.AddMessageError(Message_BR1126);
			}
		}

		bool IsPreviousProcedureCodesForBR8078(ZString requestedPreviousProcedure)
		{
			switch (requestedPreviousProcedure)
			{
				case ProcedureCode._6121:
				case ProcedureCode._7121:
					return true;
				default:
					return false;
			}
		}

		protected override void CheckJI_ConcessionOrder()
		{
			base.CheckJI_ConcessionOrder();
			CheckConcessionOrder_BR8010();
			CheckConcessionOrder_BR8011();
		}

		void CheckConcessionOrder_BR8010()
		{
			var parent = Parent;
			if (!parent.JI_ConcessionOrder.IsEmpty && parent.EntryInstruction is CusEntryInstruction instruction &&
				(instruction.CEI_Style == ImportDeclarationTypeList.Codes.I1 || (instruction.CEI_Style == ImportDeclarationTypeList.Codes.H1 && instruction.CEI_SubStyle == EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic)))
			{
				parent.JI_ConcessionOrderInfo.AddMessageError(Res.GetString("1655E61E-398B-4F1F-8C4A-037C8CA3ADE2", "[BR8010] Quota cannot be entered when Declaration Type is 'I1', or when Declaration Type is 'H1' and Sub Type is 'Z'."));
			}
		}

		void CheckConcessionOrder_BR8011()
		{
			var parent = Parent;

			if (!parent.JI_ConcessionOrder.IsEmpty && parent.IsBR8011PreferenceUsed() && parent.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				var br80111Key = parent.GetBR8011Key();
				if (entryInstruction.BR8011KeyCounts[br80111Key] > 1)
				{
					parent.JI_ConcessionOrderInfo.AddMessageError(Res.GetString("7C9009FD-2135-4FAF-83FF-95877FEA9B39", "[BR8011] The combination of Tariff + Quota + Country of Origin (if Preference starts with 1) + Country of Preferential Origin (if Preference starts with 2, 3, 4, or 5) must be unique across all invoice lines."));
				}
			}
		}

		protected override void CheckAdditionalProcedureCodesAsString()
		{
			base.CheckAdditionalProcedureCodesAsString();
			var parent = Parent;
			var procedure = parent.JI_FormattedProcedure;
			var entryInstruction = parent.EntryInstruction;
			var propertyInfo = parent.AdditionalProcedureCodesAsStringInfo;
			if (!procedure.IsEmpty && !parent.AdditionalProcedureCodesAsString.IsEmpty)
			{
				var additionalProcedures = parent.AdditionalProcedureCodesIncludingConcession;

				if (additionalProcedures.Contains(ProcedureF48) || additionalProcedures.Contains(ProcedureF49))
				{
					if (additionalProcedures.Contains(ProcedureC08))
					{
						propertyInfo.AddMessageError(Res.GetString(
							"026874F3-D253-4112-83E3-41FA437481BC",
							"[BR1119] Additional Procedure F48 or F49 cannot be used when Additional Procedure C08 is used."
						));
					}

					if (additionalProcedures.Contains(ProcedureF49)
						&& !parent.SupportingDocuments.Cast<SupportingDocument>()
							.Concat(parent.EntryInstruction.SupportingDocuments.Cast<SupportingDocument>())
							.Any(x => x.CSI_Code == Constants.SupportingDocumentCodes._1A06 && !x.CSI_ReferenceNumber.IsEmpty)
					)
					{
						propertyInfo.AddMessageError(Res.GetString(
							"D883E9F0-AC2B-4948-A97B-0BE43504452B",
							"[BR600008] If [11 10 001 000] Additional Procedure contains 'F49', then  [12 03 002 000] Supporting Document type must have '1A06' and [12 03 001 000] Supporting Document Reference Number should have the authorization number."
						));
					}
				}

				if (entryInstruction != null)
				{
					if (entryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.H1)
					{
						CheckProcedureCodesBR600000(propertyInfo, additionalProcedures);
						CheckProcedureBR3399(parent, additionalProcedures, propertyInfo);
					}

					CheckProcedureCodesBR600009(propertyInfo, additionalProcedures, entryInstruction);
					CheckProcedureCodesBR11106(x => x.Intersect(additionalProcedures).Any(), entryInstruction, propertyInfo);
					CheckProcedureCodesBR11107(x => x.Intersect(additionalProcedures).Any(), entryInstruction, propertyInfo);
				}
			}
			else if (entryInstruction is CusEntryInstruction instruction
				&& PerformEmptyCheckForAdditionalProcedure(instruction.CEI_Style)
				&& !parent.AdditionalProcedureCodesIncludingConcession.Any()
			)
			{
				propertyInfo.AddMessageError(Res.GetString("6741F117-C9EA-4CB4-857F-024C427D335D", "Additional Procedure code is required when declaration is H1, H2, H3, H4, H5, H6."));
			}

			CheckProcedureCodesBR1117(propertyInfo);
			CheckProcedureCodesBR599999(entryInstruction, parent, propertyInfo);
		}

		void CheckProcedureCodesBR1117(ZPropertyInfo targetInfo)
		{
			if (Parent.EntryInstruction is CusEntryInstruction instruction
				&& instruction.IsUCC6
				&& instruction.IsH5 && instruction.JobDeclaration is JobDeclaration dec && dec.IsCoJob
				&& IsTradeWithSpecialFiscalTerritories(Parent)
				&& !IsAdditionalProcedureCodesContainsF15(Parent.AdditionalProcedureCodesIncludingConcession)
			)
			{
				targetInfo.AddMessageError(Message_BR1117);
			}
		}

		bool PerformEmptyCheckForAdditionalProcedure(ZString cei_style)
		{
			return cei_style == ImportDeclarationTypeList.Codes.H1
				|| cei_style == ImportDeclarationTypeList.Codes.H2
				|| cei_style == ImportDeclarationTypeList.Codes.H3
				|| cei_style == ImportDeclarationTypeList.Codes.H4
				|| cei_style == ImportDeclarationTypeList.Codes.H5
				|| cei_style == ImportDeclarationTypeList.Codes.H6;
		}

		void CheckProcedureCodesBR599999(CusEntryInstruction entryInstruction, JobComInvoiceLine line, ZPropertyInfo propertyInfo)
		{
			if (entryInstruction != null)
			{
				var currentLineAdditionalProcedures = line.AdditionalProcedureCodesIncludingConcession;
				if ((!currentLineAdditionalProcedures.Contains(ProcedureF48) && entryInstruction.IsProcedureF48)
					|| (!currentLineAdditionalProcedures.Contains(ProcedureF49) && entryInstruction.IsProcedureF49))
				{
					propertyInfo.AddMessageError(Res.GetString("F6B5EBDF-6263-4217-AD44-4D2B8D552339", "[BR599999] If Additional Procedure F48 or F49 is declared, then the same code must be declared in all the declaration items."));
				}
			}
		}

		void CheckProcedureCodesBR600009(ZPropertyInfo propertyInfo, ISet<ZString> additionalProcedures, CusEntryInstruction instruction)
		{
			var isFR5Present = instruction.FiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor) || Parent.FiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor);
			if (!isFR5Present && additionalProcedures.Contains(ProcedureF48))
			{
				propertyInfo.AddMessageError(Res.GetString("9C248C94-26C8-4759-9875-7769C7736E4C", "[BR600009] Please enter a Fiscal Reference where Code is 'FR5' under Entry Instruction> Fiscal References."));
			}

			if (isFR5Present && !additionalProcedures.Contains(ProcedureF48))
			{
				propertyInfo.AddMessageError(Res.GetString("E178D9C2-BBA1-4802-ADCE-80D1DF1E5DA2", "[BR600009] Please enter an Additional Procedure Code where Code is 'F48'."));
			}
		}

		void CheckProcedureCodesBR11106(Func<ISet<ZString>, bool> predicate, CusEntryInstruction entryInstruction, ZPropertyInfo propertyInfo)
		{
			if (predicate(UniversalReferenceConstants.ProcedureCodes.BR11106MutuallyExclusiveCodes) && entryInstruction.IsBR11106MutuallyExclusiveCodesSubsetOfAdditionalProcedures)
			{
				propertyInfo.AddMessageError(Res.GetString("14A64251-9765-46BA-B6FA-A6D94852C7CE", "[BR11106] The combination of additional procedures C07 and C08 is not allowed, even in different invoice lines."));
			}
		}

		void CheckProcedureCodesBR11107(Func<ISet<ZString>, bool> predicate, CusEntryInstruction entryInstruction, ZPropertyInfo propertyInfo)
		{
			if (predicate(UniversalReferenceConstants.ProcedureCodes.BR11107MutuallyExclusiveCodes) && entryInstruction.IsBR11107MutuallyExclusiveCodesSubsetOfAdditionalProcedures)
			{
				propertyInfo.AddMessageError(Res.GetString("394290B2-0BF8-4165-9780-E4D027BB1DB8", "[BR11107] The combination of additional procedures C07 and 1C1 is not allowed, even in different invoice lines."));
			}
		}

		void CheckProcedureCodesBR600000(ZPropertyInfo propertyInfo, ISet<ZString> additionalProcedures)
		{
			if (additionalProcedures.Contains(ProcedureC07) && (additionalProcedures.Count > 2 || additionalProcedures.Any(x => x != ProcedureF48 && x != ProcedureC07)))
			{
				propertyInfo.AddMessageError(Res.GetString("88A28A6D-8539-428E-A134-DEF961AE4F3D", "[BR600000] In H1, if C07 is declared as an Additional Procedure, then only F48 or none can also be included as Additional Procedure."));
			}
		}

		protected override void CheckJI_ValuationCode()
		{
			base.CheckJI_ValuationCode();
			CheckJI_ValuationCode_BR4130();
		}

		void CheckJI_ValuationCode_BR4130()
		{
			var parent = Parent;
			if (parent?.EntryInstruction is CusEntryInstruction instruction && parent.JI_ValuationCode.IsEmpty && IsInstructionForBR4130(instruction))
			{
				parent.JI_ValuationCodeInfo.AddMessageError(Res.GetString("aac51ff7-4f02-4e36-8243-6febf152611b", "[BR4130] Valuation Method is required when Declaration Type is H1, H4 or H5."));
			}
		}

		static void CheckProcedureBR3399(JobComInvoiceLine invoiceLine, ISet<ZString> additionalProcedures, ZPropertyInfo propertyInfo)
		{
			if (!invoiceLine.JI_Tariff.In(new ZString[] { "3303001000", "3303009000" }) && additionalProcedures.Contains(ProcedureF48) && !additionalProcedures.Contains(ProcedureC07))
			{
				propertyInfo.AddMessageError(Res.GetString("EBA8BAD7-008F-4379-B232-C0ED3DA99CD2", "[BR3399] When Declaration Type is 'H1', Additional Procedure 'F48' can only be entered if another Additional Procedure 'C07' is declared or Tariff is '3303001000' or '3303009000'."));
			}
		}

		static bool IsInstructionForBR4130(CusEntryInstruction instruction)
		{
			switch (instruction.CEI_Style)
			{
				case ImportDeclarationTypeList.Codes.H1:
				case ImportDeclarationTypeList.Codes.H4:
				case ImportDeclarationTypeList.Codes.H5:
					return true;
				default:
					return false;
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();

			var parent = Parent;
			if (RefCountryHelper.IsCountryPartOfEuropeanUnion(parent.Factory, parent.JI_CountryOfOrigin))
			{
				parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("FAB636D8-290E-48B0-9924-A4A34FEACB7E", "For countries within the European Union the Country of Origin must be code 'EU'"));
			}
		}

		protected override void CheckJI_CountryOfOriginMandatoryValidation()
		{
			base.CheckJI_CountryOfOriginMandatoryValidation();
			var parent = Parent;
			if (parent.EntryInstruction is CusEntryInstruction instruction)
			{
				var countryOfOrigin = parent.JI_CountryOfOrigin;
				var targetInfo = parent.JI_CountryOfOriginInfo;
				if (countryOfOrigin.IsEmpty && ImportDeclarationTypeList.DeclarationTypeListCD0103.Contains(instruction.CEI_Style.ToUpperInvariant()))
				{
					targetInfo.AddMessageError(Res.GetString("7F2CCCF3-26C3-4452-827E-C8F4CFC75A13", "[CD0103] Country of Origin is required when Declaration is H1, H2, H3, H4, H5, or I1"));
				}

				if (countryOfOrigin == Core.Constants.CountryCodes.Turkey && !instruction.HasN018SupportingDocument && !parent.InvoiceHeader.HasN018SupportingDocument && preferenceToCheck_BR5152.Contains(parent.JI_PrimaryPreference))
				{
					targetInfo.AddMessageError(Res.GetString("4E30FAD8-E404-4F31-B7E5-C7A965A31A61", "A Supporting document of type N018 is required on the entry instruction, invoice header when there is at least an invoice line where preference is one of 400, 410, 415, 418, 420, 423, 425, 428, 440, 450, and country of origin is TR."));
				}

				if (instruction.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H5) && !RefCountryHelper.IsCountryPartOfEuropeanUnion(parent.Factory, countryOfOrigin) && countryOfOrigin != Core.Constants.CountryCodes.EuropeanUnion)
				{
					targetInfo.AddMessageError(Res.GetString("31BE8659-3DDC-4F56-8BE8-0FA11FE3A50E", "[BR5151] Country of Origin must be 'EU' when Declaration is H5."));
				}
			}
			CheckRuleCD5151(parent);
		}

		void CheckRuleCD5151(JobComInvoiceLine invoiceLine)
		{
			var parent = Parent;
			if (ValidationDecider is IIEInvoiceLineValidationDecider decider && decider.IsRuleCD5151ActiveForJI_CountryOfOrigin && invoiceLine.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				var style = entryInstruction.CEI_Style.ToUpperInvariant();
				if (RequiresCountryOfSupply(style))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.JI_CountryOfOriginInfo);
				}

				if (style == ImportDeclarationTypeList.Codes.I1)
				{
					if (parent.JI_CountryOfOrigin.IsEmpty && IsPrimaryRefWeCareAbout(invoiceLine.JI_PrimaryPreference))
					{
						parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("3CFFE031-1C94-42EC-99C7-99AA2E003D16", "[CD5151] Country of Origin is required when the first digit of Preference is '1', '4' or '5' and is not equal to Pref. Orig."));
					}
					else if (!invoiceLine.ZG_CountryOfSupply.IsEmpty && !parent.JI_CountryOfOrigin.EqualsIgnoringCase(invoiceLine.ZG_CountryOfSupply))
					{
						parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("26C3D0C6-C9C7-4B81-BC44-3E5502A75C75", "[CD5151] Country of Origin must be equal to Pref. Orig. for Import and Declaration Type of 'I1'."));
					}
				}
			}
		}

		bool IsPrimaryRefWeCareAbout(ZString primaryRef)
		{
			return primaryRef.StartsWith("1")
			|| primaryRef.StartsWith("4")
			|| primaryRef.StartsWith("5");
		}

		bool RequiresCountryOfSupply(string style)
		{
			return style == ImportDeclarationTypeList.Codes.H1
				|| style == ImportDeclarationTypeList.Codes.H2
				|| style == ImportDeclarationTypeList.Codes.H3
				|| style == ImportDeclarationTypeList.Codes.H4
				|| style == ImportDeclarationTypeList.Codes.H5;
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();

			var parent = Parent;
			if (parent.Declaration is JobDeclaration declaration && declaration.IsUCC5 && parent.JI_Description.Length > JobComInvoiceLine.JI_DescriptionMaxLength_AISUCC5)
			{
				parent.JI_DescriptionInfo.AddWarning(Res.GetString("09109F65-548C-48DB-AF5A-E63B096C8376", "Goods description can have up to {0} alpha numeric characters.", JobComInvoiceLine.JI_DescriptionMaxLength_AISUCC5));
			}
		}

		static readonly HashSet<string> preferenceToCheck_BR5152 = new HashSet<string> { "400", "410", "415", "418", "420", "423", "425", "428", "440", "450" };

		const string ProcedureC07 = UniversalReferenceConstants.ProcedureCodes.Concession.C07;
		const string ProcedureC08 = UniversalReferenceConstants.ProcedureCodes.Concession.C08;
		const string ProcedureF48 = UniversalReferenceConstants.ProcedureCodes.Concession.F48;
		const string ProcedureF49 = UniversalReferenceConstants.ProcedureCodes.Concession.F49;

		const string Tariff03 = "03";
		const string PrimaryPreference3 = "3";

		#region BR1126 & BR1117(Trade With Special Fiscal Territories)

		static readonly Lazy<ImmutableHashSet<string>> tradeWithSpecialFiscalTerritoriesRequestedProceduresLazy = new Lazy<ImmutableHashSet<string>>(() => ImmutableHashSet.Create(
				ProcedureCode._40,
				ProcedureCode._42,
				ProcedureCode._61,
				ProcedureCode._63,
				ProcedureCode._95,
				ProcedureCode._96
		));
		static ISet<string> TradeWithSpecialFiscalTerritoriesRequestedProcedures => tradeWithSpecialFiscalTerritoriesRequestedProceduresLazy.Value;

		static bool IsTradeWithSpecialFiscalTerritories(JobComInvoiceLine invoiceLine) => TradeWithSpecialFiscalTerritoriesRequestedProcedures.Contains(invoiceLine.JI_Calc_RequestedProcedure);

		static bool IsAdditionalProcedureCodesContainsF15(ISet<ZString> additionalProcedureCodes) => additionalProcedureCodes.Contains(Concession.F15);

		static readonly Lazy<string> tradeWithSpecialFiscalTerritories_RequestedProcedureTextLazy = new Lazy<string>(() =>
		{
			var requestedProcedures = TradeWithSpecialFiscalTerritoriesRequestedProcedures.OrderBy(code => code).ToArray();
			return $"'{string.Join("', '", requestedProcedures, 0, requestedProcedures.Length - 1)}' or '{requestedProcedures.Last()}'";
		});
		static string TradeWithSpecialFiscalTerritories_RequestedProcedureText => tradeWithSpecialFiscalTerritories_RequestedProcedureTextLazy.Value;

		static string Message_BR1126 => Res.GetString(
			"1AE96B72-1306-49E5-8C1B-E2E2F4E64A6F",
			"[BR1126] Requested Procedure must be {0} when Declaration is {1}",
			TradeWithSpecialFiscalTerritories_RequestedProcedureText,
			ImportDeclarationTypeList.Codes.H5
		);

		static string Message_BR1117 => Res.GetString(
			"A28510B0-21C2-4127-BDFF-AA11B65C1759",
			"[BR1117] If Requested Procedure is {0}, an 'F15' Additional Procedure must be declared.",
			TradeWithSpecialFiscalTerritories_RequestedProcedureText,
			Concession.F15
		);

		#endregion

		#region Requested Procedure

		void CheckRequestedProcedure()
		{
			ValidateBR1030();
			ValidateBR1106();
			ValidateBR1107_BR1108();
			ValidateBR1110();
			ValidateBR1112();
			ValidateBR3401();
			ValidateBR2312();
		}

		readonly Dictionary<ZString, ZString[]> authCodesByProcedureCode = new Dictionary<ZString, ZString[]>
		{
			{ ProcedureCode._44, new ZString[] { AuthorizationUsage.Codes.EUS } },
			{ ProcedureCode._51, new ZString[] { AuthorizationUsage.Codes.IPO } },
			{ ProcedureCode._53, new ZString[] { AuthorizationUsage.Codes.TEA } },
			{ ProcedureCode._71, new ZString[] { AuthorizationUsage.Codes.CW1, AuthorizationUsage.Codes.CW2, AuthorizationUsage.Codes.CWP } }
		};

		void ValidateBR1030()
		{
			var parent = Parent;
			var procedureCode = parent.JI_Calc_RequestedProcedure;
			if (authCodesByProcedureCode.TryGetValue(procedureCode, out var acceptableAuthCodes) &&
				parent.EntryInstruction is CusEntryInstruction entryInstruction &&
				!entryInstruction.CusAuthorizationUsages.Any(usage => acceptableAuthCodes.Contains(usage.AGC_Code)))
			{
				var codes = authCodesByProcedureCode[procedureCode].Select(code => $"'{code}'").ToArray();

				var quotedCodeList = codes.Length < 2
					? codes[0]
					: new ZStringBuilder().Append(string.Join(", ", codes.Take(codes.Length - 1))).Append((NoResString)" or ").Append(codes.Last()).ToString();
				parent.JI_ProcedureInfo.AddMessageError(Res.GetString
				(
					"628493FE-9771-4B4F-B46F-33139EA7B37C",
					"[BR1030] If Requested Procedure is '{0}', then please enter a {1} Authorization under the Entry Instructions > Authorizations tab.",
					procedureCode,
					quotedCodeList
				));
			}
		}

		void ValidateBR1106()
		{
			var parent = Parent;
			var targetInfo = parent.JI_ProcedureInfo;
			if (!parent.JI_Calc_RequestedProcedure.IsEmpty &&
				!parent.IsCustomsWarehousingProcedure76Or77 &&
				parent.Declaration is JobDeclaration declaration)
			{
				GetDocumentsForValidation(out var supportingDocumentsToBeValidated, out var additionalInfosToBeValidated);

				if (declaration.IsUCC5 && !supportingDocumentsToBeValidated.Any(addInfo => IsValidForBR1106(addInfo.CSI_Code)))
				{
					targetInfo.AddMessageError(br1106ErrorMessageForSupportingDocument);
				}
				else if (declaration.IsUCC6 && !additionalInfosToBeValidated.Any(addInfo => addInfo.IsATransportDocument))
				{
					targetInfo.AddMessageError(br1106ErrorMessageForTransportDocument);
				}
			}
		}

		static string br1106ErrorMessageForSupportingDocument => Res.GetString("7A9FF3CB-AFA7-45D2-8329-E69C3AC8AE9E", "[BR1106] If Requested Procedure is not '76' nor '77', then please enter at least one Transport Document under the Supporting Documents tab.");
		static string br1106ErrorMessageForTransportDocument => Res.GetString("7089CE52-BFF2-4E1B-9B17-5B5953150BF5", "[BR1106] If Requested Procedure is not '76' nor '77', then please enter at least one Transport Document under the Additional Documents tab.");

		internal bool IsValidForBR1106(ZString transportDocumentCode)
		{
			switch (transportDocumentCode)
			{
				case Constants.TransportDocumentCodes._N235:
				case Constants.TransportDocumentCodes._N271:
				case Constants.TransportDocumentCodes._N703:
				case Constants.TransportDocumentCodes._N704:
				case Constants.TransportDocumentCodes._N705:
				case Constants.TransportDocumentCodes._N710:
				case Constants.TransportDocumentCodes._N714:
				case Constants.TransportDocumentCodes._N720:
				case Constants.TransportDocumentCodes._N722:
				case Constants.TransportDocumentCodes._N730:
				case Constants.TransportDocumentCodes._N740:
				case Constants.TransportDocumentCodes._N741:
				case Constants.TransportDocumentCodes._N750:
				case Constants.TransportDocumentCodes._N760:
				case Constants.TransportDocumentCodes._N785:
				case Constants.TransportDocumentCodes._N787:
				case Constants.TransportDocumentCodes._N952:
				case Constants.TransportDocumentCodes._N955:
					return true;
				default:
					return false;
			}
		}

		void ValidateBR1107_BR1108()
		{
			var parent = Parent;
			var targetInfo = parent.JI_ProcedureInfo;
			var declaration = parent.Declaration;
			if (declaration.JE_TransportMode == TransportTypeList.Codes.Sea)
			{
				var procedure = parent.JI_Calc_RequestedProcedure;
				if (!procedure.IsEmpty && procedure != ProcedureCode._76 && procedure != ProcedureCode._77)
				{
					GetDocumentsForValidation(out var supportingDocumentsToBeValidated, out var additionalInfosToBeValidated);

					if (declaration.IsUCC5)
					{
						var supportingDocs = supportingDocumentsToBeValidated
							.Select(doc => doc.CSI_Code.ToUpperInvariant())
							.Where(IsValidForBR1107AndBR1108)
							.Take(3)
							.ToArray();

						if (supportingDocs.Length == 0 || supportingDocs.Length > 2 || supportingDocs.Length != supportingDocs.Distinct().Count())
						{
							targetInfo.AddMessageError(br1107ErrorMessageForSupportingDocument);
						}

						if (supportingDocs.Length == 2 && (!supportingDocs.Contains(Constants.TransportDocumentCodes._N704) || supportingDocs.Length != supportingDocs.Distinct().Count()))
						{
							targetInfo.AddMessageError(br1108ErrorMessageForSupportingDocument);
						}
					}
					else
					{
						var transportDocs = additionalInfosToBeValidated
							.Where(addInfo => addInfo.IsATransportDocument)
							.Select(doc => doc.CSI_Code.ToUpperInvariant())
							.Where(IsValidForBR1107AndBR1108)
							.Take(3)
							.ToArray();

						if (transportDocs.Length == 0 || transportDocs.Length > 2 || transportDocs.Length != transportDocs.Distinct().Count())
						{
							targetInfo.AddMessageError(br1107ErrorMessageForTransportDocument);
						}

						if (transportDocs.Length == 2 && (!transportDocs.Contains(Constants.TransportDocumentCodes._N704) || transportDocs.Length != transportDocs.Distinct().Count()))
						{
							targetInfo.AddMessageError(br1108ErrorMessageForTransportDocument);
						}
					}
				}
			}
		}
		static string br1107ErrorMessageForSupportingDocument => Res.GetString("AF4657F5-DE37-4F29-9021-2D45B1DF2EDD", "[BR1107] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then please uniquely enter one or two 'N703', 'N704', 'N705', 'N714', or 'N730' Supporting Documents under the Supporting Documents tab.");
		static string br1107ErrorMessageForTransportDocument => Res.GetString("6C670225-8F8D-4B9E-930C-A8CCC6DD251F", "[BR1107] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then please uniquely enter one or two 'N703', 'N704', 'N705', 'N714', or 'N730' Transport Documents under the Additional Documents tab.");
		static string br1108ErrorMessageForSupportingDocument => Res.GetString("B5E3CF93-2C3E-485C-8510-F0DD3EE6E08D", "[BR1108] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then the only allowable combinations of two Supporting Documents under the Supporting Documents tab are 'N704', with 'N703', 'N705', 'N714', or 'N730'.");
		static string br1108ErrorMessageForTransportDocument => Res.GetString("AC4DC72B-97AF-4B03-B729-E95CD5D62CAF", "[BR1108] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then the only allowable combinations of two Transport Documents under the Additional Documents tab are 'N704', with 'N703', 'N705', 'N714', or 'N730'.");

		bool IsValidForBR1107AndBR1108(ZString procedureCode)
		{
			switch (procedureCode)
			{
				case Constants.TransportDocumentCodes._N703:
				case Constants.TransportDocumentCodes._N704:
				case Constants.TransportDocumentCodes._N705:
				case Constants.TransportDocumentCodes._N714:
				case Constants.TransportDocumentCodes._N730:
					return true;
				default:
					return false;
			}
		}

		void ValidateBR1110()
		{
			var parent = Parent;
			var declaration = Declaration;
			if ((declaration?.IsAir ?? false) && !(parent.JI_Calc_RequestedProcedure.IsEmpty || parent.IsCustomsWarehousingProcedure76Or77))
			{
				GetDocumentsForValidation(out var supportingDocumentsToBeValidated, out var additionalInfosToBeValidated);

				if (declaration?.IsUCC5 ?? false)
				{
					var supportingDocuments = supportingDocumentsToBeValidated
						.Select(doc => doc.CSI_Code.ToUpperInvariant())
						.Where(IsValidForBR1110)
						.Take(3)
						.ToArray();
					if (supportingDocuments.Length == 0 || supportingDocuments.Length > 2 || supportingDocuments.Length != supportingDocuments.Distinct().Count())
					{
						AddBR1110MessageError();
					}
				}
				else
				{
					var transportDocuments = additionalInfosToBeValidated
						.Where(addInfo => addInfo.IsATransportDocument)
						.Select(doc => doc.CSI_Code.ToUpperInvariant())
						.Where(IsValidForBR1110)
						.Take(3)
						.ToArray();
					if (transportDocuments.Length == 0 || transportDocuments.Length > 2 || transportDocuments.Length != transportDocuments.Distinct().Count())
					{
						AddBR1110MessageError();
					}
				}
			}

			bool IsValidForBR1110(ZString procedureCode) => Constants.TransportDocumentCodes.DocumentTypesForBR1110Validation.Contains(procedureCode);

			string TargetTab() => Declaration is JobDeclaration declaration && declaration.IsUCC5 ? Res.GetString("D1C4484A-066C-49A0-A0B0-B5BC36ECBCF4", "Supporting Documents") : Res.GetString("4D83B5C8-85F7-4A06-91DE-FCB9AB1FDC81", "Additional Documents");

			void AddBR1110MessageError() => parent.JI_ProcedureInfo.AddMessageError(Res.GetString("0CD06A8A-CA82-43CC-80CA-F91E2040F8A9", "[BR1110] If Requested Procedure is not '76' nor '77', and Transport Mode is 'AIR', then please uniquely enter one or two 'N703', 'N740', or 'N741' Transport Documents under the {0} tab.", TargetTab()));
		}

		void GetDocumentsForValidation(out IEnumerable<SupportingDocument> supportingDocuments, out IEnumerable<AdditionalInfo> additionalInfos)
		{
			if (Parent.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				supportingDocuments = entryInstruction.SupportingDocumentsAtAnyLevel;
				additionalInfos = entryInstruction.AdditionalInfosAtAnyLevel;
			}
			else if (Parent.InvoiceHeader is JobComInvoiceHeader invoiceHeader)
			{
				supportingDocuments = invoiceHeader.SupportingDocumentsAtAnyLevel;
				additionalInfos = invoiceHeader.AdditionalInfosAtAnyLevel;
			}
			else
			{
				supportingDocuments = Parent.SupportingDocuments.Cast<SupportingDocument>();
				additionalInfos = Parent.AdditionalInfos.Cast<AdditionalInfo>();
			}
		}

		void ValidateBR1112()
		{
			var parent = Parent;
			var procedure = parent.JI_Calc_RequestedProcedure;
			if (Declaration.IsAir && !procedure.IsEmpty && !parent.IsCustomsWarehousingProcedure76Or77)
			{
				var transportDocuments = parent.AdditionalInfos.Where(addInfo => addInfo.IsATransportDocument &&
				(addInfo.CSI_Code.EqualsIgnoringCase(TransportDocumentCodes._N703) || addInfo.CSI_Code.EqualsIgnoringCase(TransportDocumentCodes._N740) || addInfo.CSI_Code.EqualsIgnoringCase(TransportDocumentCodes._N741)))
				.Take(3).Select(addinfo => addinfo.CSI_Code).OrderBy(x => x).ToArray();
				if (transportDocuments.Length == 2)
				{
					var combinationIsAllowable = (transportDocuments[0].EqualsIgnoringCase(TransportDocumentCodes._N703) || transportDocuments[0].EqualsIgnoringCase(TransportDocumentCodes._N740)) && transportDocuments[1].EqualsIgnoringCase(TransportDocumentCodes._N741);
					if (!combinationIsAllowable)
					{
						Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("682093B8-7AA0-4E1C-B0AD-93A1DA9F20C6", "[BR1112] When Requested Procedure is not '76' nor '77', and Transport Mode is 'AIR', then the only allowable combinations of two Transport Documents under the Additional Documents tab are 'N741' together with 'N703' or 'N740'."));
					}
				}
			}
		}

		void ValidateBR3401()
		{
			var parent = Parent;
			var procedure = parent.JI_Calc_RequestedProcedure;
			var info = parent.JI_ProcedureInfo;

			if (procedure == ProcedureCodes.ProcedureCode._42 || procedure == ProcedureCodes.ProcedureCode._63)
			{
				var fiscalReferences = parent.FiscalReferences.Cast<CusFiscalReference>();
				var isValidFiscalReferencesCombination = fiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer)
					&& fiscalReferences.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR1_Importer || x.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
				if (!isValidFiscalReferencesCombination)
				{
					info.AddMessageError(Res.GetString("CD2A04D3-C65F-4622-97A1-26BFFBD8828A", "[BR3401] If the Requested Procedure is coded as '42' or '63', please ensure that the information stipulated by Article 143 (2) of Directive 2006/112/EC is entered in the 'Fiscal References' tab, by inputting an FR2 along with either an FR1 or an FR3."));
				}
			}
		}

		void ValidateBR2312()
		{
			var parent = Parent;
			if (parent.JI_Calc_RequestedProcedure.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration)
				|| parent.EntryInstruction is CusEntryInstruction instruction && instruction.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.I1)
			)
			{
				var addInfos = parent.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.IsAnAdditionalReference);
				var numberOf1D94 = addInfos.Count(x => x.CSI_Code.EqualsIgnoringCase(Constants.AdditionalInformationCodes._1D94));
				var numberOf1D95 = addInfos.Count(x => x.CSI_Code.EqualsIgnoringCase(Constants.AdditionalInformationCodes._1D95));

				if (numberOf1D94 != numberOf1D95 || numberOf1D94 > 1)
				{
					parent.JI_ProcedureInfo.AddMessageError(Res.GetString(
						"9B494D45-A78E-46F8-A51C-99AE0F8D1754",
						"[BR2312] If the Dataset is 'I1' or the Requested Procedure is '71', the Additional Documents tab must include both '1D94' and '1D95' Additional References, or none of them."
					));
				}
			}
		}

		#endregion
	}
}
