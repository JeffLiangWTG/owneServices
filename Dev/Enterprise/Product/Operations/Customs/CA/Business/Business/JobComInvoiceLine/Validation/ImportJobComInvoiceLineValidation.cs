using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.CA.Business
{
	public class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		CADeclarationValidator DeclarationValidator
		{
			get { return Parent.Declaration?.DeclarationValidator; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckDutyAndTaxMatchToProduct();
			ValidateCA_SIMADumpingDesc();
		}

		#region CheckDutyAndTaxMatchToProduct

		void CheckDutyAndTaxMatchToProduct()
		{
			if (Parent.Pivot != null)
			{
				Parent.ClearRowNotificationsContaining("Duty & Tax (Tax Types:");

				var registryValue = CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.Value;
				if (registryValue != ProductAuditActions.Codes.NoAction)
				{
					var notMatchToProductTypes = new HashSet<ZString>();

					var lineDutyAndTaxs = Parent.DutiesAndTaxes;
					var pivotDutyAndTaxs = Parent.Pivot.DutiesAndTaxes;
					foreach (var dutyAndTax in pivotDutyAndTaxs)
					{
						if (!lineDutyAndTaxs.Any(x => x.C1_TaxType == dutyAndTax.C1_TaxType && x.C1_ExemptCode == dutyAndTax.C1_ExemptCode))
						{
							notMatchToProductTypes.Add(dutyAndTax.C1_TaxType);
						}
					}

					if (notMatchToProductTypes.Count > 0)
					{
						var message = string.Format(DutyAndTaxDoesNotMatchProductCodeFile, string.Join(", ", notMatchToProductTypes));
						if (registryValue == ProductAuditActions.Codes.AddMessageErrorValidation)
						{
							Parent.AddRowMessageError(message);
						}
						else if (registryValue == ProductAuditActions.Codes.AddWarningValidation)
						{
							Parent.AddRowWarning(message);
						}
					}
				}
			}
		}

		static string DutyAndTaxDoesNotMatchProductCodeFile => Res.GetString("BAEBC52C-5D6D-4040-BA74-DDE23BC1D95B", "Duty & Tax (Tax Types: {0}) does not match product code file.");

		#endregion

		#region CA_ImportReasonCodeTC (ACROSS)

		public void ValidateCA_ImportReasonCodeTC()
		{
			ValidateCalculatedProperty(Parent.CA_ImportReasonCodeTCInfo);
		}

		protected void CheckCA_ImportReasonCodeTC()
		{
			if (IsTCValidationRequiredForThisLine(Parent))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ImportReasonCodeTCInfo, ImportReasonCodeTCResString);
			}
		}

		static string ImportReasonCodeTCResString => Res.GetString("663D2896-235A-466F-8061-6A74BD84AE72", "Import Reason Code TC");

		internal static bool IsTCValidationRequiredForThisLine(JobComInvoiceLine parentLine)
		{
			return parentLine.Declaration is JobDeclaration declaration && declaration.IsOGD && declaration.CA_OGDTC && declaration.DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && parentLine.IsHSCodeContainsOGDTC;
		}

		#endregion

		#region CA_ImportReasonCodeNR (ACROSS)

		public void ValidateCA_ImportReasonCodeNR()
		{
			ValidateCalculatedProperty(Parent.CA_ImportReasonCodeNRInfo);
		}

		protected void CheckCA_ImportReasonCodeNR()
		{
			if (IsNRValidationRequiredForThisLine)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ImportReasonCodeNRInfo, ImportReasonCodeNRResString);
			}
		}

		static string ImportReasonCodeNRResString => Res.GetString("494CD288-BF1D-4F3A-B104-3224AA166BA5", "Import Reason Code NR");

		bool IsNRValidationRequiredForThisLine
		{
			get
			{
				return Parent.Declaration is JobDeclaration declaration && declaration.IsOGD && declaration.CA_OGDNR && declaration.DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && Parent.IsHSCodeContainsOGDNRCan;
			}
		}

		#endregion

		#region CA_ImportReasonCodeSITT (ACROSS)

		public void ValidateCA_ImportReasonCodeSITT()
		{
			ValidateCalculatedProperty(Parent.CA_ImportReasonCodeSITTInfo);
		}

		protected void CheckCA_ImportReasonCodeSITT()
		{
			if (IsSITTValidationRequiredForThisLine)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_ImportReasonCodeSITTInfo);
				if (Parent.SITTCertificationNumbers.Count == 0)
				{
					if (Parent.CA_ImportReasonCodeSITT.IsEmpty)
					{
						Parent.CA_ImportReasonCodeSITTInfo.AddMessageError(Res.GetString("6138bdbd-c023-41ed-91f7-93eb1cb406e7", "At least one Certification Number must be entered if Import Reason Code is blank"));
					}
					else if (Parent.CA_ImportReasonCodeSITT != ImportReasonCodes.Codes.Testing)
					{
						Parent.CA_ImportReasonCodeSITTInfo.AddMessageError(Res.GetString("035a96f7-74a3-4fed-83c0-1a974f1a99af", "Import Reason Code must be 05 (Testing) if no Certification Nos are entered"));
					}
				}
			}
		}

		bool IsSITTValidationRequiredForThisLine
		{
			get
			{
				return Parent.Declaration is JobDeclaration declaration && declaration.IsOGD && declaration.CA_OGDIC && declaration.DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && !Parent.IsHSCodeContainsOGDTC && !Parent.IsHSCodeContainsOGDNRCan && !Parent.IsHSCodeContainsOGDCFIA &&
					(!Parent.CA_Model.IsEmpty ||
					!Parent.CA_ModelNumber.IsEmpty ||
					!Parent.JI_BrandName.IsEmpty);
			}
		}

		#endregion

		#region CA_BrandNameTC (ACROSS)

		public void ValidateCA_BrandNameTC()
		{
			ValidateCalculatedProperty(Parent.CA_BrandNameTCInfo);
		}

		protected void CheckCA_BrandNameTC()
		{
			if (Parent.CA_BrandNameTC.IsEmpty && IsTCValidationRequiredForThisLine(Parent))
			{
				Parent.CA_BrandNameTCInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(BrandNameTCResString));
			}
		}

		static string BrandNameTCResString => Res.GetString("A84DABA5-7760-4BDD-937F-5860A3200C33", "Brand Name TC");

		#endregion

		#region CA_BrandNameNR (ACROSS)

		public void ValidateCA_BrandNameNR()
		{
			ValidateCalculatedProperty(Parent.CA_BrandNameNRInfo);
		}

		protected void CheckCA_BrandNameNR()
		{
			if (Parent.CA_BrandNameNR.IsEmpty && IsNRValidationRequiredForThisLine)
			{
				Parent.CA_BrandNameNRInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(BrandNameNRResString));
			}
		}

		static string BrandNameNRResString => Res.GetString("EC7C21B0-EBBE-4E8D-882B-9B1261D72144", "Brand Name NR");

		#endregion

		#region CA_BrandNameSITT (ACROSS)

		public void ValidateCA_BrandNameSITT()
		{
			ValidateCalculatedProperty(Parent.CA_BrandNameSITTInfo);
		}

		protected void CheckCA_BrandNameSITT()
		{
			if (Parent.CA_BrandNameSITT.IsEmpty && IsSITTValidationRequiredForThisLine)
			{
				Parent.CA_BrandNameSITTInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(BrandNameSITTResString));
			}
		}

		static string BrandNameSITTResString => Res.GetString("2BB11A04-3298-496F-BDC5-EE0ABFB857F4", "Brand Name SITT");

		#endregion

		#region CA_TypeSizeTC (ACROSS)

		public void ValidateCA_TypeSizeTC()
		{
			ValidateCalculatedProperty(Parent.CA_TypeSizeTCInfo);
		}

		protected void CheckCA_TypeSizeTC()
		{
			if (Parent.CA_TypeSizeTC.IsEmpty && IsTCValidationRequiredForThisLine(Parent))
			{
				Parent.CA_TypeSizeTCInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TypeSizeTCResString));
			}
		}

		static string TypeSizeTCResString => Res.GetString("BB7D0A9C-3BA2-4419-B238-EAB40F936C51", "Type Size TC");

		#endregion

		#region CA_TypeSizeNR (ACROSS)

		public void ValidateCA_TypeSizeNR()
		{
			ValidateCalculatedProperty(Parent.CA_TypeSizeNRInfo);
		}

		protected void CheckCA_TypeSizeNR()
		{
			if (Parent.CA_TypeSizeNR.IsEmpty && IsNRValidationRequiredForThisLine)
			{
				Parent.CA_TypeSizeNRInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TypeSizeNRResString));
			}
		}

		static string TypeSizeNRResString => Res.GetString("8B206908-9B8B-4E09-B3B9-73D5C0DC130E", "Type Size NR");

		#endregion

		#region CA_ModelNR (ACROSS)

		public void ValidateCA_ModelNR()
		{
			ValidateCalculatedProperty(Parent.CA_ModelNRInfo);
		}

		protected void CheckCA_ModelNR()
		{
			if (Parent.CA_ModelNR.IsEmpty && IsNRValidationRequiredForThisLine)
			{
				Parent.CA_ModelNRInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ModelNRResString));
			}
		}

		static string ModelNRResString => Res.GetString("E33477C8-8E69-450F-90F7-3AC8772B7C31", "Model NR");

		#endregion

		#region CA_ModelSITT (ACROSS)

		public void ValidateCA_ModelSITT()
		{
			ValidateCalculatedProperty(Parent.CA_ModelSITTInfo);
		}

		protected void CheckCA_ModelSITT()
		{
			if (Parent.CA_ModelSITT.IsEmpty && IsSITTValidationRequiredForThisLine)
			{
				Parent.CA_ModelSITTInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ModelSITTResString));
			}
		}

		static string ModelSITTResString => Res.GetString("0D4128F5-7C41-4446-BAA4-C7B66DA81364", "Model SITT");

		#endregion

		#region CA_ModelNumberNR (ACROSS)

		public void ValidateCA_ModelNumberNR()
		{
			ValidateCalculatedProperty(Parent.CA_ModelNumberNRInfo);
		}

		protected void CheckCA_ModelNumberNR()
		{
			if (Parent.CA_ModelNumberNR.IsEmpty && IsNRValidationRequiredForThisLine)
			{
				Parent.CA_ModelNumberNRInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ModelNumberNRResString));
			}
		}

		static string ModelNumberNRResString => Res.GetString("EACFFD77-B6B7-4EF7-9A31-0986ACAC306E", "Model Number NR");

		#endregion

		#region CA_ModelNumberSITT (ACROSS)

		public void ValidateCA_ModelNumberSITT()
		{
			ValidateCalculatedProperty(Parent.CA_ModelNumberSITTInfo);
		}

		protected void CheckCA_ModelNumberSITT()
		{
			if (Parent.CA_ModelNumberSITT.IsEmpty && IsSITTValidationRequiredForThisLine)
			{
				Parent.CA_ModelNumberSITTInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ModelNumberSITTResString));
			}
		}

		static string ModelNumberSITTResString => Res.GetString("3DFAD7D3-1170-4B03-B46E-85F42E3DB79B", "Model Number SITT");

		#endregion

		#region CA_OGDStatus

		public void ValidateCA_OGDStatus()
		{
			ValidateCalculatedProperty(Parent.CA_OGDStatusInfo);
		}

		protected void CheckCA_OGDStatus()
		{
			if (Parent.Declaration is JobDeclaration declaration && declaration.DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS)
				&& (declaration.IsOGD && declaration.JE_GB.IsValid && !string.IsNullOrEmpty(CACustomsDataRegistry.Instance.AIRSValidationKey.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty))))
			{
				switch (Parent.CA_OGDStatus)
				{
					case AVSStatusList.Codes.NotImport:
						Parent.CA_OGDStatusInfo.AddMessageError(Res.GetString("0ad39646-fa64-45e9-9548-97f0e88bfb6f", "CFIA reports entry refused, goods may not be imported into Canada"));
						break;
					case AVSStatusList.Codes.NotValidated:
					case AVSStatusList.Codes.Error:
					case AVSStatusList.Codes.Rejected:
					case AVSStatusList.Codes.Unknown:
						Parent.CA_OGDStatusInfo.AddMessageError(Res.GetString("194d6637-7b78-47ec-a407-b6a3fc6b9410", "CFIA validation has not been completed on this line"));
						break;
					case AVSStatusList.Codes.InspectionRequired:
					case AVSStatusList.Codes.ReviewRequired:
						Parent.CA_OGDStatusInfo.AddMessageError(Res.GetString("bf702a28-6b52-40d1-a67d-1b2d252829f3", "OGD Inspection/Review may be required"));
						break;
				}
			}
		}

		#endregion

		#region CheckJI_Tariff (common, ACROSS)

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			new TariffValidator(Parent.Factory).Validate(Parent.JI_TariffInfo, true, 10, Parent.EffectiveDateForDutyRate, Parent.IsImport);

			var declaration = Parent.Declaration;
			if (declaration != null && !declaration.IsIID && (declaration.DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) || declaration.IsLVS))
			{
				var notificationType = declaration.IsLVS ? NotificationType.Warning : NotificationType.MessageError;

				if ((!declaration.CA_OGDTC || Parent.CA_ImportReasonCode.IsEmpty) && Parent.IsHSCodeContainsOGDTC)
				{
					Parent.JI_TariffInfo.AddNotification(notificationType, Res.GetString("700f8a93-55c8-4b5c-a4be-269173687cc0", "Transport Canada details are required for this HS code."));
				}

				if ((!declaration.CA_OGDCFIA || Parent.IsOGDCFIABlank) && Parent.IsHSCodeContainsOGDCFIA)
				{
					Parent.JI_TariffInfo.AddNotification(notificationType, Res.GetString("1F3888BC-C536-4D1A-A428-BFBD5EF0AEB2", "CFIA details are required for this HS code."));
				}

				if ((!declaration.CA_OGDNR || Parent.CA_ImportReasonCode.IsEmpty) && Parent.IsHSCodeContainsOGDNRCan)
				{
					Parent.JI_TariffInfo.AddNotification(notificationType, Res.GetString("6FC6D923-3DD5-4F22-ADDE-1B35FE8CDFB7", "Natural Resources Canada details are required for this HS code."));
				}
			}

			if (Parent.IsSIMADutyRequired && Parent.CA_AuthorityNumber.IsEmpty && !Parent.DutiesAndTaxes.Any(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType)))
			{
				Parent.JI_TariffInfo.AddMessageError(Res.GetString("7899AC08-1B56-40D3-8C59-C6C94F65F6F9", "Classification Requires SIMA – please add SIMA Code under the Duty & Tax tab, if there is no row with SIMA and no Special Authority Number entered."));
			}

			Parent.AddInfoValidation.ValidateCA_TreatmentCode();
		}

		#endregion

		#region CheckJI_Description (common)

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo, Res.GetString("f005b92d-3cee-41ff-9937-d0aafda2c969", "Goods Description"));

			if (Parent.JI_Description.Length > 256 && (Parent.Declaration?.IsIID ?? false))
			{
				Parent.JI_DescriptionInfo.AddMessageError(GoodsDescriptionUpTo256Chars);
			}
		}

		internal static string GoodsDescriptionUpTo256Chars
		{
			get
			{
				return Res.GetString("0bafc77e-c0e7-4d0a-be53-d3742a33cbb1", "The goods description is up to 256 chars.");
			}
		}

		#endregion

		#region CheckJI_CountryOfOrigin (common)

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			Parent.Validation.MatchToProductValidation(Parent.JI_CountryOfOriginInfo, () => Parent.Pivot?.CCA_RN_NKOrigin ?? ZString.Empty);
			var header = Parent.InvoiceHeader;
			if (header != null && header.JZ_RN_NKDefaultOrigin.IsEmpty)
			{
				var ecccHeader = Parent.ECCCPGAHeader;
				if (ecccHeader != null && ecccHeader.CA_WENProgramInd == YesNoList.Codes.Yes)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo, CountryOfOriginResString);
				}
				if (header.IsAttachedToPersistentLVXDeclaration)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo, CountryOfOriginResString);
				}
				else if (header.IsAttachedToPersistentLVSDeclaration)
				{
					if (Parent.JI_CountryOfOrigin.IsEmpty &&
						TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(Parent.EffectiveTreatmentCode))
					{
						Parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("f6ed1892-9c4f-4302-b57b-316577ce2a74",
							"Country/Region of Origin must be specified for LVS when tariff treatment code other than 02 and 10."));
					}
					ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo, CountryOfOriginResString);
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
			}
			Parent.AddInfoValidation.ValidateCA_TreatmentCode();
		}

		static string CountryOfOriginResString => Res.GetString("906F5817-7D88-4312-B956-4D87CB57CFDD", "Country/Region of Origin");

		#endregion

		#region CheckJI_CustomsQuantity (ACROSS)

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			var parent = Parent;
			if (parent.IsWarrantyRepairLine)
			{
				if (parent.JI_CustomsQuantity != 0m)
				{
					parent.JI_CustomsQuantityInfo.AddMessageError(ValueShouldBeZero);
				}
			}
			else if (parent.JI_CustomsQuantity.IsEmpty && !parent.JI_CustomsUnitQty.IsEmpty && (DeclarationValidator?.IsValidationRequired(ValidateForMessageType.ACROSS) ?? false))
			{
				parent.JI_CustomsQuantityInfo.AddMessageError(MandatoryValidation.ValueCannotBeZeroMessage(CustomsQtyResString));
			}

			var tCPGAHeader = parent.TCPGAHeader;
			if (parent.JI_CustomsQuantity > 1 && parent.JI_CustomsUnitQty == CustomsUnitOfMeasureList.Codes.Number && tCPGAHeader != null && tCPGAHeader.IsVPREnabledExceptPIG)
			{
				parent.JI_CustomsQuantityInfo.AddWarning(SeparateLineShouldBeCreated);
			}
		}

		internal static string ValueShouldBeZero => Res.GetString("6b545116-1964-4790-946a-e21f3700cb7a", "The value should be zero.");
		internal static string SeparateLineShouldBeCreated => Res.GetString("53056B01-E507-46F9-9629-B87E3F19C9F5", "For lines with Transport Canada, separate invoices lines should be created for each VIN.");
		static string CustomsQtyResString => Res.GetString("2AF5C3F3-C876-41F7-8624-40C9171C06ED", "Customs Qty");
		static string CustomsSecondQtyResString => Res.GetString("7379194C-A7D7-47A6-8D58-E454AF27FF0C", "Customs Second Qty");
		static string CustomsThirdQtyResString => Res.GetString("07522FA4-4275-42CE-BF65-21A5B2E26865", "Customs Third Qty");

		#endregion

		#region CheckJI_CustomsUnitQty (validate if entered, common, ACROSS)

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			if (Parent.JI_CustomsUnitQty.IsEmpty)
			{
				if (DoesRequireUQ && (DeclarationValidator?.IsValidationRequired(ValidateForMessageType.ACROSS) ?? false) && (Parent.JI_InvoiceUQ.IsEmpty || !Parent.Factory.GetCachedValue<CustomsUnitOfMeasureList>().ContainsCode(Parent.JI_InvoiceUQ)))
				{
					Parent.JI_CustomsUnitQtyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(CustomsUnitQtyResString));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsUnitQtyInfo, Lookups.CustomsUQList);
			}
		}

		bool DoesRequireUQ => Parent.JI_CustomsQuantity > 0 && Parent.Tariff != null && !Parent.Tariff.TariffUnits.IsEmpty && !Parent.JI_CustomsUnitQty_ReadOnly;

		static string CustomsUnitQtyResString => Res.GetString("38F3C3D4-9335-47F9-A9A7-CDB2D941C7F1", "Customs UQ");

		#endregion

		#region CheckJI_InvoiceQuantity & CheckJI_InvoiceUQ (ACROSS)

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			var parent = Parent;
			if (parent.JI_InvoiceQuantity.IsEmpty)
			{
				if (parent.JI_CustomsUnitQty.IsEmpty && (DeclarationValidator?.IsValidationRequired(ValidateForMessageType.ACROSS) ?? false) && !parent.IsLuxuryTaxInvoiceLine)
				{
					parent.JI_InvoiceQuantityInfo.AddMessageError(InvoiceQtyRequired);
				}
				else if (PGAInvoiceLineValidator.IsJI_InvoiceQuantityRequired)
				{
					parent.JI_InvoiceQuantityInfo.AddMessageError(InvoiceQtyRequiredForGAC);
				}

				if (DeclarationValidator is CADeclarationValidator declValidator && declValidator.IsIID && declValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && !parent.IsLuxuryTaxInvoiceLine)
				{
					parent.JI_InvoiceQuantityInfo.AddMessageError(InvoiceQtyRequiredForServiceIID);
				}

				if (!parent.JI_InvoiceUQ.IsEmpty)
				{
					parent.JI_InvoiceQuantityInfo.AddMessageError(InvoiceQuantityRequiredForInvoiceUQ);
				}
			}
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			if (Parent.JI_NetWeight.IsEmpty && (PGAInvoiceLineValidator.IsJI_NetWeightAndUQRequiredByCNSC || PGAInvoiceLineValidator.IsJI_NetWeightAndUQRequiredByGAC))
			{
				Parent.JI_NetWeightInfo.AddMessageError(MandatoryValidation.ValueCannotBeZeroMessage(NetWeightResString));
			}
		}

		static string NetWeightResString => Res.GetString("5BD3924C-F6A1-4571-8954-D8DF4232FA45", "Net Weight");

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_NetWeightUQInfo, Parent.Lookups.WeightUQList);
			if (PGAInvoiceLineValidator.IsJI_NetWeightAndUQRequiredByGAC)
			{
				var uq = Parent.JI_NetWeightUQ;
				if (uq.IsEmpty)
				{
					Parent.JI_NetWeightUQInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(NetWeightUQResString));
				}
				else
				{
					if (uq != Core.Constants.Weight.Kilograms && uq != Core.Constants.Weight.Tonnes)
					{
						Parent.JI_NetWeightUQInfo.AddMessageError(Res.GetString("BC1DB008-2C73-4832-8B77-5FE7F21884A5", "Requires unit KG or T since PGA GAC has LPCO with type 2006."));
					}
				}
			}
		}

		static string NetWeightUQResString => Res.GetString("8C1F47E3-3C46-4185-9CB7-8436777E3A09", "Net Weight UQ");

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_WeightUQInfo, Parent.Lookups.WeightUQList);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			var parent = Parent;
			var invoiceUQ = parent.JI_InvoiceUQ;
			var isInvoiceUQEmpty = invoiceUQ.IsEmpty;
			if (isInvoiceUQEmpty)
			{
				if (parent.JI_CustomsUnitQty.IsEmpty && !parent.IsLuxuryTaxInvoiceLine &&
					DeclarationValidator is CADeclarationValidator declarationValidator && declarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
				{
					parent.JI_InvoiceUQInfo.AddMessageError(InvoiceQtyRequired);
				}

				if (DeclarationValidator is CADeclarationValidator declValidator && declValidator.IsIID && declValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && !parent.IsLuxuryTaxInvoiceLine)
				{
					parent.JI_InvoiceUQInfo.AddMessageError(InvoiceQtyRequiredForServiceIID);
				}

				if (!parent.JI_InvoiceQuantity.IsEmpty)
				{
					parent.JI_InvoiceUQInfo.AddMessageError(InvoiceUQRequiredForInvoiceQuantity);
				}
			}
			else
			{
				var isIID = parent.Declaration?.IsIID ?? false;
				if (!Lookups.InvoiceUQList.ContainsCode(invoiceUQ) &&
					(isIID || !parent.Factory.GetCachedValue<CustomsUnitOfMeasureList>().ContainsCode(invoiceUQ)))
				{
					parent.JI_InvoiceUQInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}

				if (parent.TCPGAHeader is TCPGAHeader tcPGAHeader
					&& tcPGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes)
				{
					if (isIID)
					{
						if (invoiceUQ != IIDUnitOfCountCodeList.Codes.Each)
						{
							parent.JI_InvoiceUQInfo.AddMessageError(Res.GetString("94E65DF1-FA00-4ED9-80FF-E60412BDE232", "EA should be selected for {0} of PGA {1}", TCPGADepartmentCodes.Descriptions.TPR, PGACodes.Descriptions.TC));
						}
					}
					else if (invoiceUQ != CustomsUnitOfMeasureList.Codes.Number
						&& invoiceUQ != CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(CustomsUnitOfMeasureList.Codes.Number, parent.Factory))
					{
						parent.JI_InvoiceUQInfo.AddMessageError(Res.GetString("a5258035-e020-4b7b-bae9-1705de0a319a", "NO or NMB should be selected for {0} of PGA {1}", TCPGADepartmentCodes.Descriptions.TPR, PGACodes.Descriptions.TC));
					}
				}
			}

			if (parent.ECCCPGAHeader is ECCCPGAHeader ecccPGAHeader
						&& ecccPGAHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				if (invoiceUQ != IIDUnitOfCountCodeList.Codes.Each)
				{
					parent.JI_InvoiceUQInfo.AddMessageError(Res.GetString("986901DA-75E3-4259-89FB-7EFD492B0050", "EA should be selected for {0} of PGA {1}", ECCCPGADepartmentCodes.Descriptions.VEE, PGACodes.Descriptions.ECCC));
				}
			}

			parent.Validation.ValidateJI_CustomsUnitQty();
		}

		internal static string InvoiceQtyRequired
		{
			get { return Res.GetString("EB18787D-1143-4C34-A3E0-B3C33AEB50C7", "The Invoice Qty and Units are required for Release message, when no Customs Units apply."); }
		}

		internal static string InvoiceQtyRequiredForGAC
		{
			get { return Res.GetString("EE882901-46C6-43F1-A97F-3E1B670F96CC", "GAC reporting requires Invoice Quantity."); }
		}

		internal static string InvoiceQuantityRequiredForInvoiceUQ
		{
			get { return Res.GetString("2237F74D-E5CB-4D15-8F7C-1447DDBF8E2A", "The Invoice Quantity is required, when Invoice Unit is entered."); }
		}

		internal static string InvoiceQtyRequiredForServiceIID
		{
			get { return Res.GetString("F28CD7E5-EA93-4757-96BE-54287C99165A", "The Invoice Qty and Unit are required for Release Option Service IID"); }
		}

		internal static string InvoiceUQRequiredForInvoiceQuantity
		{
			get { return Res.GetString("4CCDA480-16C0-487D-8B77-A1713B5D2CBD", "The Invoice Unit is required, when Invoice Quantity is entered."); }
		}

		#endregion

		#region CheckJI_PreviousEntryNumber

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();
			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsOutwardBondedWarehousingEnabled && declaration.IsExWarehouseEntry
				&& declaration.IsBondedWarehousingFieldValidationRequired && Parent.JI_PreviousEntryNumber.IsEmpty)
			{
				Parent.JI_PreviousEntryNumberInfo.AddMessageError(Res.GetString("ea48860d-4fce-42e5-864a-1b528433ad19", "Previous Tran. Number is required."));
			}
		}

		#endregion

		#region CheckJI_PreviousEntryLineNumber

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();
			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsOutwardBondedWarehousingEnabled && declaration.IsExWarehouseEntry
				&& Parent.JI_PreviousEntryLineNumber.IsEmpty)
			{
				Parent.JI_PreviousEntryLineNumberInfo.AddMessageError(Res.GetString("3c474ec5-cc19-49f3-accb-45dbaf5c65e6", "Previous Tran. Line Number is required."));
			}
		}

		#endregion

		#region CheckJI_StateOrRegionOfOrigin
		protected override void CheckJI_StateOrRegionOfOrigin()
		{
			base.CheckJI_StateOrRegionOfOrigin();
			var parent = Parent;
			if (parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.UnitedStates)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JI_StateOrRegionOfOriginInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(parent.JI_StateOrRegionOfOriginInfo);
			}
		}
		#endregion

		#region CheckJI_HazMatCode

		protected override void CheckJI_HazMatCode()
		{
			base.CheckJI_HazMatCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_HazMatCodeInfo, Parent.Lookups.UNDGs);
		}

		PGAInvoiceLineValidator PGAInvoiceLineValidator
		{
			get => fPGAInvoiceLineValidator ?? (fPGAInvoiceLineValidator = new PGAInvoiceLineValidator(Parent));
		}
		PGAInvoiceLineValidator fPGAInvoiceLineValidator;
		#endregion

		#region CheckJI_OA_ManufacturerAddress

		protected override void CheckJI_OA_ManufacturerAddress()
		{
			base.CheckJI_OA_ManufacturerAddress();

			var parent = Parent;
			if (parent.JI_OA_ManufacturerAddress.IsEmpty)
			{
				if (PGAInvoiceLineValidator.IsJI_OA_ManufacturerAddressRequired)
				{
					parent.JI_OA_ManufacturerAddressInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerResString));
				}
			}
			else
			{
				if (parent.ManufacturerAddress is OrgAddress manufacturerAddress)
				{
					if (PGAInvoiceLineValidator.IsWMIOfManufacturerRequired)
					{
						var manufacturer = manufacturerAddress.Header;
						if (manufacturer != null)
						{
							var wmiCode = manufacturer.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.WorldManufacturerIdentifier, Core.Constants.CountryCodes.Canada);
							if (!CanadianCustomsCodeValidator.GetWorldManufacturerIdentifierError(wmiCode).IsEmpty)
							{
								parent.JI_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("34462683-b146-4543-b6f6-26cad28bd667", "Manufacturer WMI code is not found or is invalid.  WMI codes can be added under the Config > Registration Number tab of an organization."));
							}
						}
					}

					var invoice = parent.InvoiceHeader;
					if (invoice != null && invoice.JobDeclaration.IsIID)
					{
						var requiredMessageAdded = false;
						if (PGAInvoiceLineValidator.IsJI_OA_ManufacturerAddressPhoneAndEmailRequired)
						{
							OrganisationValidation.ValidateCAPContactOrAddressPhoneAndEmailRequired(parent.JI_OA_ManufacturerAddressInfo, manufacturerAddress);
							requiredMessageAdded = true;
						}
						if (!requiredMessageAdded && PGAInvoiceLineValidator.IsAddressPhoneOrEmailRecommended)
						{
							OrganisationValidation.ValidateCAPContactOrAddressPhoneAndEmailRecommened(parent.JI_OA_ManufacturerAddressInfo, parent.ManufacturerAddress);
						}

						CAAddressValidator.ValidateMandatory(manufacturerAddress, Parent.JI_OA_ManufacturerAddressInfo, ManufacturerResString);
					}
				}
			}
		}

		static string ManufacturerResString => Res.GetString("1341FE57-9875-4A20-AD9A-2EE88070495B", "Manufacturer");

		#endregion

		#region CheckJI_BrandName

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();

			if (Parent.JI_BrandName.IsEmpty && PGAInvoiceLineValidator.IsJI_BrandNameRequired)
			{
				Parent.JI_BrandNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(BrandNameResString));
			}

			var header = Parent.HCPGAHeader;
			if (header != null && header.IsProgramEnabled(HCPGADepartmentCodes.Codes.CPR))
			{
				if (Parent.JI_BrandName.IsEmpty)
				{
					Parent.JI_BrandNameInfo.AddWarning(CPR_BrandNameShouldNotBeEmptyMessage);
				}
			}

			if (Parent.Validation is ImportJobComInvoiceLineValidation lineValidation)
			{
				lineValidation.ValidateCA_BrandNameTC();
				lineValidation.ValidateCA_BrandNameNR();
				lineValidation.ValidateCA_BrandNameSITT();
			}
		}

		internal static string CPR_BrandNameShouldNotBeEmptyMessage
		{
			get { return Res.GetString("56ea6c4e-0f46-46b5-a691-c5fcb89408cd", "It is strongly recommended that the Brand Name of the commodity being imported be provided. While not required, this information may help to facilitate communication in case of a referral."); }
		}

		static string BrandNameResString => Res.GetString("AB192F38-3F57-4A3D-B240-3EFFF34EB8BD", "Brand Name");

		#endregion

		#region CheckJI_Model

		protected override void CheckJI_Model()
		{
			base.CheckJI_Model();

			if (Parent.JI_Model.IsEmpty && PGAInvoiceLineValidator.IsJI_ModelRequired)
			{
				Parent.JI_ModelInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ModelResString));
			}
		}

		static string ModelResString => Res.GetString("C00DCAA5-564C-4642-AEBE-83AB27445389", "Model");

		#endregion

		#region CheckJI_OA_ConsigneeAddress

		protected override void CheckJI_OA_ConsigneeAddress()
		{
			base.CheckJI_OA_ConsigneeAddress();

			var parent = Parent;
			if (parent.JI_OA_ConsigneeAddress.IsEmpty)
			{
				if (parent.JI_OA_ConsigneeAddress_ZAddress.OrgHeader != null)
				{
					parent.JI_OA_ConsigneeAddressInfo.AddError(Res.GetString("5188ba11-d418-497e-a7a3-8c9e64a68fd9", "Please enter a consignee address."));
				}
			}
			else
			{
				var cfiaPGAHeader = parent.CFIAPGAHeader;
				var cnscPGAHeader = parent.CNSCPGAHeader;
				var ecccPGAHeader = parent.ECCCPGAHeader;

				if (parent.ConsigneeAddress is OrgAddress consigneeAddress)
				{
					if (consigneeAddress.OA_RN_NKCountryCode != Constants.CountryCodes.Canada)
					{
						if (cfiaPGAHeader != null)
						{
							parent.JI_OA_ConsigneeAddressInfo.AddMessageError(Res.GetString("b0b46b72-18d3-4326-b763-2cfa5bf42b04", "Please select an Canada address."));
						}
						else
						{
							parent.JI_OA_ConsigneeAddressInfo.AddWarning(ConsigneeAddrIsNotCA);
						}
					}
					if (cfiaPGAHeader != null || cnscPGAHeader != null || (ecccPGAHeader != null && ecccPGAHeader.CA_WRMProgramInd == YesNoList.Codes.Yes))
					{
						OrganisationValidation.ValidateCAPContactOrAddressPhone(parent.JI_OA_ConsigneeAddressInfo, parent.ConsigneeAddress);
					}

					if (parent.Declaration is JobDeclaration declaration && declaration.IsIID)
					{
						CAAddressValidator.ValidateMandatory(consigneeAddress, Parent.JI_OA_ConsigneeAddressInfo, Res.GetString("26211620-EEFF-4B0C-B8B5-15DA77172059", "Consignee"));
					}
				}
			}
		}

		internal static string ConsigneeAddrIsNotCA => Res.GetString("5483CC48-37A2-465C-81E4-9A66FF899149", "Consignee address is not in Canada");

		#endregion

		#region CheckJI_CustomsSecondQuantity

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (Parent.IsWarrantyRepairLine)
			{
				if (Parent.JI_CustomsSecondQuantity != 0m)
				{
					Parent.JI_CustomsSecondQuantityInfo.AddMessageError(ImportJobComInvoiceLineValidation.ValueShouldBeZero);
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsSecondQuantityInfo);
				RunValidationOnDutyLinesQuantity();
			}
			if (Parent.JI_CustomsSecondQuantity.IsEmpty && !Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				Parent.JI_CustomsSecondQuantityInfo.AddMessageError(MandatoryValidation.ValueCannotBeZeroMessage(CustomsSecondQtyResString));
			}
		}

		void RunValidationOnDutyLinesQuantity()
		{
			foreach (var line in Parent.DutiesAndTaxes)
			{
				var dutyLineValidation = line.Validation as DutyAndTaxValidation;
				if (dutyLineValidation != null)
				{
					dutyLineValidation.ValidateQuantity();
				}
			}
		}

		#endregion

		#region CheckJI_CustomsSecondUnitQty

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsSecondUnitQtyInfo, Parent.Lookups.CustomsUQList);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.JI_CustomsSecondUnitQtyInfo, Parent.JI_CustomsSecondQuantityInfo);
			RunValidationOnDutyLinesQuantity();
			var excTax = Parent.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax && EXSCodeList.Contains(x.C1_Code));
			if (excTax != null && !Parent.HasABVInUnits)
			{
				Parent.JI_CustomsSecondUnitQtyInfo.AddMessageError(EXSTaxShouldHaveAtLeastOneABVUQ);
			}
		}

		static readonly List<String> EXSCodeList = new List<string> { "E10", "E11", "E12", "E31", "E33", "E34", "E35", "E36" };
		static string EXSTaxShouldHaveAtLeastOneABVUQ => Res.GetString("0EB5BA1F-CB46-4E5B-8FC3-FAF2BDDA9C10", "Alcohol By Volume (ABV) is required. At least one of the Customs QTY UQ must be ABV.");

		#endregion

		#region CheckJI_CustomsThirdQuantity

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();
			if (Parent.IsWarrantyRepairLine)
			{
				if (Parent.JI_CustomsThirdQuantity != 0m)
				{
					Parent.JI_CustomsThirdQuantityInfo.AddMessageError(ImportJobComInvoiceLineValidation.ValueShouldBeZero);
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsThirdQuantityInfo);
				RunValidationOnDutyLinesQuantity();
			}
			if (Parent.JI_CustomsThirdQuantity.IsEmpty && !Parent.JI_CustomsThirdUnitQty.IsEmpty)
			{
				Parent.JI_CustomsThirdQuantityInfo.AddMessageError(MandatoryValidation.ValueCannotBeZeroMessage(CustomsThirdQtyResString));
			}
		}

		#endregion

		#region CheckJI_CustomsThirdUnitQty

		protected override void CheckJI_CustomsThirdUnitQty()
		{
			base.CheckJI_CustomsThirdUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsThirdUnitQtyInfo, Parent.Lookups.CustomsUQList);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.JI_CustomsThirdUnitQtyInfo, Parent.JI_CustomsThirdQuantityInfo);
			RunValidationOnDutyLinesQuantity();
			var excTax = Parent.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax && EXSCodeList.Contains(x.C1_Code));
			if (excTax != null && !Parent.HasABVInUnits)
			{
				Parent.JI_CustomsThirdUnitQtyInfo.AddMessageError(EXSTaxShouldHaveAtLeastOneABVUQ);
			}
		}

		#endregion

		#region CheckCA_SIMADumpingDesc

		protected void CheckCA_SIMADumpingDesc()
		{
			if (Parent.CA_SIMADumpingDesc.IsEmpty && Parent.DutiesAndTaxes.Find(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD || x.C1_TaxType == DutyAndTaxTypes.Codes.CVD).Any())
			{
				Parent.CA_SIMADumpingDescInfo.AddMessageError(SimaMeasureIsMissingMessage);
			}
		}
		internal static string SimaMeasureIsMissingMessage => Res.GetString("3b3a3f2b-1989-4008-ab95-98471a796e9d", "SIMA Measure is missing – re-link to tariff number to re-instate.");

		public void ValidateCA_SIMADumpingDesc()
		{
			ValidateCalculatedProperty(Parent.CA_SIMADumpingDescInfo);
		}

		#endregion
	}
}
