using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class EXPJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public EXPJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJI_Calc_FOB();
			ValidateCertificateOfOriginIssueStatus();
			ValidatePRA_ReferenceNumber();
			ValidatePRA_DateOfIssue();
			ValidatePRA_DateOfExpiry();
		}

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();
			if (!Parent.JI_BrandName.IsEmpty)
			{
				if (Parent.JI_BrandName != Parent.JI_BrandName.KeepAlphanumericCharacters())
				{
					Parent.JI_BrandNameInfo.AddMessageError(Res.GetString("109BB3FF-064A-458F-BE3A-F02E920ADF74", "Brand Name should only contain alphanumeric characters."));
				}
			}
		}

		protected override void CheckJI_Model()
		{
			base.CheckJI_Model();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ModelInfo);
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo);
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_NetWeightInfo);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (!Parent.JI_CustomsUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_CustomsQuantityInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsQuantityInfo);
			}
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();

			var transactionType = Parent.Declaration?.JE_ExportGoodsType ?? CargoWise.Types.ZString.Empty;

			if (ExportDealingTypeCodeList.IsImportedToReExported(transactionType))
			{
				if (ExportDealingTypeCodeList.IsImportDeclarationNumberOptional(transactionType))
				{
					if (Parent.JI_PreviousEntryLineNumber > 0)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryNumberInfo);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryNumberInfo);
				}
			}
			else if (!Parent.JI_PreviousEntryNumber.IsEmpty)
			{
				Parent.JI_PreviousEntryNumberInfo.AddMessageError(importDeclarationDetailsNotRelevant);
			}
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();

			var transactionType = Parent.Declaration?.JE_ExportGoodsType ?? CargoWise.Types.ZString.Empty;

			if (ExportDealingTypeCodeList.IsImportedToReExported(transactionType))
			{
				if (ExportDealingTypeCodeList.IsImportDeclarationNumberOptional(transactionType))
				{
					if (Parent.JI_PreviousEntryNumber.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_PreviousEntryLineNumberInfo);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsZero(Parent.JI_PreviousEntryLineNumberInfo);
						MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_PreviousEntryLineNumberInfo);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryLineNumberInfo);
				}
			}
			else if (!Parent.JI_PreviousEntryLineNumber.IsEmpty)
			{
				Parent.JI_PreviousEntryLineNumberInfo.AddMessageError(importDeclarationDetailsNotRelevant);
			}
		}

		public void ValidateJI_Calc_FOB()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_FOBInfo);
		}

		protected void CheckJI_Calc_FOB()
		{
			if (Parent.JI_Calc_FOB < 0)
			{
				Parent.JI_Calc_FOBInfo.AddMessageError(Res.GetString("1A048890-6DC8-4D32-9180-C28146C84E7B", "The FOB value is calculated as a negative value. Please check Line Price, Currency and its Exchange Rate and deduction charges."));
			}
		}

		public void ValidateCertificateOfOriginIssueStatus()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginIssueStatusInfo);
			Parent.AddAllNotificationsFromCertificateOfOriginCSI_Code();
		}

		protected void CheckCertificateOfOriginIssueStatus()
		{
			var declaration = Parent.Declaration;
			if (declaration != null && !declaration.IsDeclarationProcedureTypeE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CertificateOfOriginIssueStatusInfo);
			}

			if (!Parent.Declaration.JE_GoodsDestination.IsEmpty)
			{
				var destinationCountry = Parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, declaration.JE_GoodsDestination);
				if (destinationCountry != null && Parent.CertificateOfOriginIssueStatus == CertificateOfOriginIssuedCodeList.Codes.Y)
				{
					if (Parent.JI_CountryOfOrigin != Core.Constants.CountryCodes.KoreaSouth || !Parent.Lookups.ExportFTAList.Any())
					{
						Parent.CertificateOfOriginIssueStatusInfo.AddMessageError(Res.GetString("2445992A-CA8B-4E3B-B2B2-4A8CA566A9DA", "It can be ‘Y’ only for a destination country with which Country of Origin Korea has an FTA relationship."));
					}
				}
			}
		}

		protected override void CheckUnitPrice()
		{
			base.CheckUnitPrice();
			CompareValidation.MessageErrorIfNumberNegative(Parent.UnitPriceInfo);
		}
		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			CompareValidation.MessageErrorIfNumberNegative(Parent.JI_LinePriceInfo);
		}

		protected override void CheckJI_PrimaryPreference()
		{
			if (!Parent.Declaration.JE_GoodsDestination.IsEmpty && Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.KoreaSouth && Parent.CertificateOfOriginIssueStatus == CertificateOfOriginIssuedCodeList.Codes.Y)
			{
				var collection = Parent.Lookups.ExportFTAList;

				if (collection.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_CodeType == Constants.ZZ.NKCodeType.EXFTA))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PrimaryPreferenceInfo);
				}
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_PrimaryPreferenceInfo);
		}

		protected override void CheckJI_LineNo()
		{
			base.CheckJI_LineNo();
			var declaration = Parent.Declaration;
			if (declaration.IsDeclarationProcedureTypeB)
			{
				if (Parent.ContainersPivot.Count == 0)
				{
					Parent.JI_LineNoInfo.AddMessageError(Res.GetString("7F9D633D-B47B-4813-A9F0-4087F87AD719", "If the 'Declaration Type' is 'B', at least one container must be checked."));
				}
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			if (Parent.JI_LinePrice != 0 && !Parent.HasHSRequiringInvQuantityInCustomsUQ)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_InvoiceQuantityInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_InvoiceQuantityInfo);
			}

			if (Parent.VehicleNumbers.Count > 0)
			{
				if (Parent.JI_InvoiceQuantity != Parent.VehicleNumbers.Count)
				{
					Parent.JI_InvoiceQuantityInfo.AddMessageError(Res.GetString("03CC028F-6AF2-4CC3-9F05-035734A7C660", "If there are vehicles in the invoice line, The quantity must be equal to vehicles count."));
				}
			}
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();

			if (Parent.JI_LinePrice != 0 && !Parent.HasHSRequiringInvQuantityInCustomsUQ)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InvoiceUQInfo);
			}
		}

		public void ValidatePRA_ReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.PRA_ReferenceNumberInfo);
		}

		protected void CheckPRA_ReferenceNumber()
		{
			var propertyInfo = Parent.PRA_ReferenceNumberInfo;
			if (!Parent.IsPreapprovalMandatory)
			{
				MandatoryValidation.MessageErrorIfIsEntered(propertyInfo);
			}
			else if (Parent.PRA_ReferenceNumber.IsEmpty)
			{
				propertyInfo.AddMessageError(PRAReferenceNumberError);
			}

			var currentEntryLine = Parent.CusEntryLine;
			if (!Parent.PRA_ReferenceNumber.IsEmpty && currentEntryLine != null)
			{
				foreach (var invoiceLine in Parent.InvoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.CusEntryLine != currentEntryLine))
				{
					if (Parent.PRA_ReferenceNumber == invoiceLine.PRA_ReferenceNumber)
					{
						Parent.PRA_ReferenceNumberInfo.AddMessageError(Res.GetString("E69C32CC-0723-4929-BAB8-00826CC34832", "The same Steel Pre-Approval number cannot be applied to different entry lines. You need to get another pre-approval number from the Korean Steel Association."));
						break;
					}
				}
			}
		}

		public void ValidatePRA_DateOfIssue()
		{
			ValidateCalculatedProperty(Parent.PRA_DateOfIssueInfo);
		}

		protected void CheckPRA_DateOfIssue()
		{
			var propertyInfo = Parent.PRA_DateOfIssueInfo;
			if (Parent.IsPreapprovalMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				if (!Parent.PRA_DateOfIssue.IsEmpty)
				{
					if (Parent.PRA_DateOfIssue > Parent.PRA_DateOfExpiry)
					{
						propertyInfo.AddMessageError(IssueDateMessageError);
					}
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(propertyInfo);
			}
		}

		public void ValidatePRA_DateOfExpiry()
		{
			ValidateCalculatedProperty(Parent.PRA_DateOfExpiryInfo);
		}

		protected void CheckPRA_DateOfExpiry()
		{
			var propertyInfo = Parent.PRA_DateOfExpiryInfo;
			if (Parent.IsPreapprovalMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				if (!Parent.PRA_DateOfExpiry.IsEmpty)
				{
					if (Parent.PRA_DateOfIssue > Parent.PRA_DateOfExpiry)
					{
						propertyInfo.AddMessageError(IssueDateMessageError);
					}
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(propertyInfo);
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			if (Parent.UniversalTariff != null)
			{
				var exportOGA = Parent.UniversalTariff.Conditions
					.Where(x =>
						x.ZX1_IsExport && x.ConditionType == Constants.ZZ.RefCusConditionType.OGA
						&& x.ZX1_StartDate <= Parent.EffectiveAssessmentDate
						&& x.ZX1_EndDate >= Parent.EffectiveAssessmentDate)
					.Select(x =>
						x.ConditionValues
						.FirstOrDefault(y => y.ValueType == Constants.ZZ.RefCusConditionValueType.OGARegulationNumber).ZX3_Value)
					.ToHashSet();
				var regulations = Parent.GAApprovalDataCollection.Select(x => x.CSI_Procedure).ToHashSet();
				exportOGA.ExceptWith(regulations);
				if (exportOGA.Count > 0)
				{
					var regulation = exportOGA.Count > 1 ? (NoResString)"regulations" : (NoResString)"regulation";
					Parent.JI_TariffInfo.AddMessageError(string.Format(TariffInfoRegulationError, regulation, string.Join(", ", exportOGA)));
				}
			}
		}

		protected override void CheckJI_PackType()
		{
			base.CheckJI_PackType();
			if (Parent.JI_NoOfPacks > 0)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_PackTypeInfo);
			}
			else if (!Parent.JI_PackType.IsEmpty)
			{
				Parent.JI_PackTypeInfo.AddMessageError(Res.GetString("4FD9F371-24D2-451C-8B00-B737FBCF3CB1", "Pack type should not be entered if Pack Qty is zero."));
			}
		}

		protected override void CheckJI_NoOfPacks()
		{
			base.CheckJI_NoOfPacks();
			if (Parent.JI_PackType.IsEmpty && Parent.JI_NoOfPacks != 0)
			{
				Parent.JI_NoOfPacksInfo.AddMessageError(Res.GetString("A5465CED-1CCD-434C-9B3D-9934D6304A61", "Pack Qty should be zero When Pack Type is not entered."));
			}
			CompareValidation.MessageErrorIfNumberNegative(Parent.JI_NoOfPacksInfo);
		}

		protected override void CheckJI_SkipManifestReport()
		{
			base.CheckJI_SkipManifestReport();
			if (Parent.JI_Tariff.StartsWith(AutomatedShippingTargetCode))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_SkipManifestReportInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_SkipManifestReportInfo);
			}
		}

		protected override void CheckJI_COOLabelLocation()
		{
			base.CheckJI_COOLabelLocation();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_COOLabelLocationInfo);
		}

		const string AutomatedShippingTargetCode = "8609";

		public static string PRAReferenceNumberError => Res.GetString("5A342538-CCD7-4E95-B8E4-8C21DA951A9A", "The entered tariff requires a pre-approval from the Korean Steel Association.");
		public static string IssueDateMessageError => Res.GetString("C98080C9-FE39-4E24-9277-99AA301B9FE7", "A From date cannot be later than a To date.");
		public static string TariffInfoRegulationError => Res.GetString("FB16BB27-C142-4886-85FB-39D50AD445ED", "This tariff has OGA requirements for the {0} {1} but the details are not in Other Details > Approval Document.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "Baseline")]
		readonly string importDeclarationDetailsNotRelevant = Res.GetString("51F33265-6716-402F-93F5-469607984D19", "For the selected transaction type, an import declaration number/entry line number is not relevant.");
	}
}
