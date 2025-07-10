using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using static Enterprise.Customs.JP.Business.JobComInvoiceLine;
using static Enterprise.Customs.JP.Business.JPExportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business
{
	public class JobComInvoiceLineValidation : AutoJPJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public const decimal HighAmount = 201000m;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJI_Calc_Preference();
			ValidateJI_Calc_OriginCertifier();
			ValidateJI_Calc_CertificateOfOriginCertifier();
		}

		protected override void CheckJI_CEI()
		{
			if (Parent.JI_CEI.IsEmpty)
			{
				if (Parent.Declaration != null && Parent.Declaration.IsPersistent)
				{
					Parent.JI_CEIInfo.AddMessageError(Res.GetString("8228E9F3-4284-4D8F-AA8F-444269B6A359", "You have not entered the Entry Instruction."));
				}
			}
		}

		protected override void CheckJI_DutyReductionExemptionRefundCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_DutyReductionExemptionRefundCodeInfo);
		}

		protected override void CheckJI_DutyReductionAmount()
		{
			var parent = Parent;
			if (parent.IsImport && !parent.JI_DutyReductionAmount.IsEmpty && parent.JI_DutyReductionExemptionRefundCode.IsEmpty)
			{
				parent.JI_DutyReductionAmountInfo.AddMessageError(Res.GetString("8926E397-9D2B-49EB-9ABE-75B8AB595EFA", "Duty Reduction Amount should only be entered if Duty Reduction or Exemption Code is not empty."));
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				var instruction = Parent.EntryInstruction;
				var tariff = Parent.JI_Tariff;
				var tariffInfo = Parent.JI_TariffInfo;
				var declarationSubType = instruction?.CEI_Style ?? ZString.Empty;
				var valueType = instruction?.CEI_ValueType;
				var isImport = declaration.IsImport;
				var isExport = declaration.IsExport;

				if (string.IsNullOrWhiteSpace(tariff))
				{
					var errorMessage = Res.GetString("55DB3F0F-7591-4136-8A08-90F0B0E45AC4", "You have not entered Tariff.");
					if (isImport)
					{
						tariffInfo.AddMessageError(errorMessage);
					}
					else if (isExport)
					{
						switch (declarationSubType)
						{
							case JPExportDeclarationTypeList.Codes.T:
							case JPExportDeclarationTypeList.Codes.E:
							case JPExportDeclarationTypeList.Codes.N:
							case JPExportDeclarationTypeList.Codes.M:
							case JPExportDeclarationTypeList.Codes.R:
								if (ValueTypeList.Codes.L.Equals(valueType))
								{
									tariffInfo.AddMessageError(errorMessage);
								}
								break;
						}
					}
				}
				else
				{
					var invoiceAmountLocal = instruction?.InvoiceLines.Select(x => x.InvoiceHeader).Distinct().Sum(y => y.JZ_InvoiceAmountInLocalCurrency)
												?? Parent.InvoiceHeader.JZ_InvoiceAmountInLocalCurrency;
					var tariffLength = tariff.Length;
					var tariffMustBeNineCharactersLongMessage = Res.GetString("0683D5A0-0142-4006-91A7-F264737C33B4", "Tariff must be 9 characters long.");
					if ((tariffLength == 9 && Parent.UniversalTariff == null) || ((tariffLength == 4 || tariffLength == 6) && Parent.RefCusNomenclatureGroup == null))
					{
						tariffInfo.AddMessageError(Res.GetString("2B43848E-15E6-4A19-A4EB-497606BD1B02", "The entered Tariff does not exist. Please select one from the list provided."));
					}
					if (tariffLength == 9 && Parent.RefCusNomenclatureGroup != null)
					{
						tariffInfo.AddMessageError(Res.GetString("37FB353D-8257-4DFC-A00E-0EE72C3141C5", "Please select a root level tariff code."));
					}

					if (IsImport)
					{
						if (declarationSubType == JPImportDeclarationTypeList.Codes.Y && tariffLength != 6)
						{
							tariffInfo.AddMessageError(Res.GetString("5C380BD7-4376-42EB-9B6F-1BE62D90AF68", "Tariff must be 6 characters long."));
						}
						if ((declarationSubType == JPImportDeclarationTypeList.Codes.H || declarationSubType == JPImportDeclarationTypeList.Codes.N) && invoiceAmountLocal > HighAmount && tariffLength != 9)
						{
							tariffInfo.AddMessageError(tariffMustBeNineCharactersLongMessage);
						}
						if (declarationSubType != JPImportDeclarationTypeList.Codes.Y && declarationSubType != JPImportDeclarationTypeList.Codes.H && declarationSubType != JPImportDeclarationTypeList.Codes.N && tariffLength == 6)
						{
							tariffInfo.AddMessageError(tariffMustBeNineCharactersLongMessage);
						}
					}
					else if (IsExport)
					{
						if (tariffLength != 9 && declarationSubType != JPExportDeclarationTypeList.Codes.G && ValueTypeList.Codes.L.Equals(valueType))
						{
							tariffInfo.AddMessageError(tariffMustBeNineCharactersLongMessage);
						}
						else if (tariffLength != 9 && tariffLength != 4)
						{
							tariffInfo.AddMessageError(Res.GetString("9D1EB357-D92C-4926-A797-FD0CE25FA9F4", "Tariff must be 4 or 9 characters long."));
						}
					}
				}
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();

			var declaration = Parent.Declaration;
			var instruction = Parent.EntryInstruction;
			if (declaration != null && Parent.JI_Description.IsEmpty)
			{
				var targetInfo = Parent.JI_DescriptionInfo;
				var errorMessage = Res.GetString("22521A8C-B385-4D12-B40B-AAA6CD92393D", "You have not entered {0}.", targetInfo.HumanReadableName);
				var lengthOfTariff = Parent.JI_Tariff.Length;

				if (declaration.JE_MessageType == JPJobMessageTypeList.Codes.Import && instruction != null &&
						(instruction.CEI_Style == JPImportDeclarationTypeList.Codes.Y || (instruction.CEI_Style == JPImportDeclarationTypeList.Codes.H || instruction.CEI_Style == JPImportDeclarationTypeList.Codes.N) && lengthOfTariff == 6))
				{
					targetInfo.AddMessageError(errorMessage);
				}

				if (declaration.JE_MessageType == JPJobMessageTypeList.Codes.Export && (lengthOfTariff == 10 || lengthOfTariff == 4))
				{
					targetInfo.AddMessageError(errorMessage);
				}
			}
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			if (Parent.IsImport)
			{
				if (MandatoryForThisDecType && Parent.JI_Procedure.IsEmpty)
				{
					Parent.JI_ProcedureInfo.AddMessageError(OriginIDRequired);
				}
			}
		}

		protected override void CheckJI_ConcessionOrder()
		{
			base.CheckJI_ConcessionOrder();
			if (Parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_ConcessionOrderInfo);
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
			if (Declaration.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CountryOfOriginInfo);
				if (Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.Japan || Parent.JI_CountryOfOrigin == Common.Constants.CountryCodes.UnknownCountryCode)
				{
					Parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("7C2300BA-8B44-46D9-8315-CE6FFF6B3745", "Goods Origin cannot be {0} or {1}.", Core.Constants.CountryCodes.Japan, Common.Constants.CountryCodes.UnknownCountryCode));
				}
			}
		}

		public void ValidateJI_Calc_Preference()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_PreferenceInfo);
		}

		protected void CheckJI_Calc_Preference()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_Calc_PreferenceInfo);
			if (Parent.IsImport && Parent.JI_Calc_Preference.IsEmpty)
			{
				Parent.JI_Calc_PreferenceInfo.AddMessageError(Res.GetString("66C0B1E6-D3FA-441C-85EF-7EE055AAA3C4", "You have not entered the first two characters of Certificate of Origin Type."));
			}
		}

		public void ValidateJI_Calc_OriginCertifier()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_OriginCertifierInfo);
		}

		protected void CheckJI_Calc_OriginCertifier()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_Calc_OriginCertifierInfo);
			if (Parent.IsImport && Parent.JI_Calc_OriginCertifier.IsEmpty)
			{
				Parent.JI_Calc_OriginCertifierInfo.AddMessageError(Res.GetString("D81381AD-B100-4D1F-9269-AF779CB7FB1E", "You have not entered the third character of Certificate of Origin Type."));
			}
		}

		public void ValidateJI_Calc_CertificateOfOriginCertifier()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_CertificateOfOriginCertifierInfo);
		}

		protected void CheckJI_Calc_CertificateOfOriginCertifier()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_Calc_CertificateOfOriginCertifierInfo);
			if (Parent.IsImport && Parent.JI_Calc_CertificateOfOriginCertifier.IsEmpty)
			{
				Parent.JI_Calc_CertificateOfOriginCertifierInfo.AddMessageError(Res.GetString("EDDF14E2-6E7A-4D55-80B1-12515F90AABA", "You have not entered the fourth character of Certificate of Origin Type."));
			}
		}

		protected override void CheckJI_Volume()
		{
			base.CheckJI_Volume();
			MandatoryValidation.CheckNotNegative(Parent.JI_VolumeInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_VolumeInfo, Parent.JI_VolumeUQInfo);
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			MandatoryValidation.CheckNotNegative(Parent.JI_WeightInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_WeightInfo, Parent.JI_WeightUQInfo);
		}

		protected override void CheckUnitPrice()
		{
			base.CheckUnitPrice();
			MandatoryValidation.CheckNotNegative(Parent.UnitPriceInfo);
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			MandatoryValidation.CheckNotNegative(Parent.JI_InvoiceQuantityInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_InvoiceQuantityInfo, Parent.JI_InvoiceUQInfo);
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			MandatoryValidation.CheckNotNegative(Parent.JI_LinePriceInfo);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			MandatoryValidation.CheckNotNegative(Parent.JI_CustomsQuantityInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_CustomsQuantityInfo, Parent.JI_CustomsUnitQtyInfo);
		}

		protected override void CheckJI_CustomsQuantityIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_CustomsQuantityInfo, 11, 2);
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_CustomsSecondQuantityInfo, Parent.JI_CustomsSecondUnitQtyInfo);
		}

		protected override void CheckJI_CustomsSecondQuantityIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_CustomsSecondQuantityInfo, 11, 2);
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			ListValidation.ErrorIfInvalidCode(Parent.JI_CustomsUnitQtyInfo);
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			ListValidation.ErrorIfInvalidCode(Parent.JI_CustomsSecondUnitQtyInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_CustomsSecondUnitQtyInfo, Parent.JI_CustomsSecondQuantityInfo);
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JI_WeightUQInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JI_InvoiceUQInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_InvoiceUQInfo, Parent.JI_InvoiceQuantityInfo);
		}

		protected override void CheckJI_TradeControlOrderAppendix()
		{
			var invoiceLine = Parent;
			var targetInfo = invoiceLine.JI_TradeControlOrderAppendixInfo;
			if (invoiceLine.Declaration is JobDeclaration declaration)
			{
				base.CheckJI_TradeControlOrderAppendix();
				ListValidation.MessageErrorIfInvalidCode(targetInfo);

				if (declaration.IsExport)
				{
					var declarationType = invoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty;
					switch (declarationType)
					{
						case JPExportDeclarationTypeList.Codes.M:
						case JPExportDeclarationTypeList.Codes.N:
						case JPExportDeclarationTypeList.Codes.T:
							if (invoiceLine.JI_TradeControlOrderAppendix == TradeControlOrderAppendixCode._10101)
							{
								targetInfo.AddMessageError(Res.GetString("6A373DE2-A7E3-47DF-BCC3-3DA512FA1BB2", "Value cannot be selected with the current declaration type."));
							}
							break;
					}
				}
			}
		}

		protected override void CheckJI_FEFTAArticle48()
		{
			base.CheckJI_FEFTAArticle48();
			var invoiceLine = Parent;
			var targetInfo = invoiceLine.JI_FEFTAArticle48Info;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, invoiceLine.Lookups.FEFTAArticle48List);
			if (invoiceLine.IsAppendixTable1)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(targetInfo);
			}
		}

		protected override void CheckJI_DomesticConsumptionTaxExemptionCode()
		{
			var parent = Parent;
			if (parent.IsExport)
			{
				base.CheckJI_DomesticConsumptionTaxExemptionCode();
				ListValidation.MessageErrorIfInvalidCode(parent.JI_DomesticConsumptionTaxExemptionCodeInfo);
			}
		}

		protected override void CheckJI_AdvanceRulingOnClassification()
		{
			base.CheckJI_AdvanceRulingOnClassification();
			var parent = Parent;
			var advanceRulingOnClassification = parent.JI_AdvanceRulingOnClassification;
			if (!string.IsNullOrEmpty(advanceRulingOnClassification) && advanceRulingOnClassification.Length != 9)
			{
				var targetInfo = parent.JI_AdvanceRulingOnClassificationInfo;
				targetInfo.AddMessageError($"{targetInfo.HumanReadableName} must be exactly 9 characters long.");
			}
		}

		protected override void CheckJI_AdvanceRulingOnOrigin()
		{
			base.CheckJI_AdvanceRulingOnOrigin();

			var advanceRulingOnOrigin = Parent.JI_AdvanceRulingOnOrigin;
			if (!string.IsNullOrEmpty(advanceRulingOnOrigin) && advanceRulingOnOrigin.Length != 7)
			{
				var targetInfo = Parent.JI_AdvanceRulingOnOriginInfo;
				targetInfo.AddMessageError($"{targetInfo.HumanReadableName} must be exactly 7 characters long.");
			}
		}

		protected override void CheckJI_NACCSCode()
		{
			base.CheckJI_NACCSCode();
			var invoiceLine = Parent;
			if (invoiceLine.Declaration is JobDeclaration declaration && invoiceLine.EntryInstruction is CusEntryInstruction instruction)
			{
				var tariffCode = invoiceLine.JI_Tariff;
				var naccsCode = invoiceLine.JI_NACCSCode;
				var targetInfo = Parent.JI_NACCSCodeInfo;
				var valueType = instruction.CEI_ValueType;
				var decSubType = instruction.CEI_Style;
				if (declaration.IsImport)
				{
					if (!(decSubType == JPImportDeclarationTypeList.Codes.H || decSubType == JPImportDeclarationTypeList.Codes.N || decSubType == JPImportDeclarationTypeList.Codes.Y))
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo, targetInfo.HumanReadableName);
					}
					else if ((decSubType == JPImportDeclarationTypeList.Codes.H || decSubType == JPImportDeclarationTypeList.Codes.N) && tariffCode.Length == 9)
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo, targetInfo.HumanReadableName);
					}
				}
				else if (declaration.IsExport)
				{
					if (valueType == ValueTypeList.Codes.L && string.IsNullOrWhiteSpace(naccsCode))
					{
						if (decSubType != JPExportDeclarationTypeList.Codes.G)
						{
							MandatoryValidation.MessageErrorIfNotEntered(targetInfo, targetInfo.HumanReadableName);
						}
					}
					else if (valueType == ValueTypeList.Codes.S && !string.IsNullOrWhiteSpace(naccsCode))
					{
						targetInfo.AddMessageError(Res.GetString("A3FF9CFA-B171-4B0E-9C72-D5F96A3D7F0F", "NACCS Code is not required."));
					}

					if (decSubType == JPExportDeclarationTypeList.Codes.G && valueType == ValueTypeList.Codes.L && !string.IsNullOrWhiteSpace(naccsCode) && naccsCode != ExportNACCSCodeList.Codes.T)
					{
						targetInfo.AddMessageError(Res.GetString("490EB9E5-F706-4893-AB16-F3076BA16D85", "NACCS Code must be T or empty."));
					}

					if (naccsCode == ExportNACCSCodeList.Codes.T && !invoiceLine.Factory.GetCachedValue<ReturnedGoodsDeclarationTypeList>().ContainsCode(decSubType))
					{
						targetInfo.AddMessageError(Res.GetString("8909982F-C580-4546-B2A9-667DA637308E", "Value is incompatible with Declaration Type."));
					}
				}
				ListValidation.MessageErrorIfInvalidCode(targetInfo, invoiceLine.Lookups.NACCSCodeList);
			}
		}

		protected override void CheckJI_StorageType()
		{
			var invoiceLine = Parent;
			if (invoiceLine.JI_StorageTypeVisible)
			{
				if (invoiceLine.JI_StorageType.IsEmpty)
				{
					var storageTypeList = invoiceLine.Lookups.StorageTypeList;
					if (storageTypeList is StorageTypeListWhenDeclarationTypeIsA || storageTypeList is StorageTypeListWhenDeclarationTypeIsG)
					{
						invoiceLine.JI_StorageTypeInfo.AddMessageError(Res.GetString("B7148144-FDBA-415D-B0E5-2AD69FB8EE18", "Storage Type cannot be empty when Declaration Type is A or G."));
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(invoiceLine.JI_StorageTypeInfo, invoiceLine.Lookups.StorageTypeList, ResString.GetMultilingualString("5158A36B-C059-462C-835F-AF4A299F4723", "The value entered in Storage Type is not a valid list option."));
				}
			}
		}

		protected override void CheckJI_BondedDate()
		{
			base.CheckJI_BondedDate();

			var invoiceLine = Parent;
			if (invoiceLine.Declaration is JobDeclaration jobdeclaration && jobdeclaration.JE_MessageType == JPJobMessageTypeList.Codes.Import)
			{
				var iSDate = invoiceLine.JI_BondedDate;
				var iSDateInfo = invoiceLine.JI_BondedDateInfo;
				var declarationType = invoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty;

				if (!iSDate.IsEmpty && iSDate.IsInTheFutureDatePartOnly)
				{
					iSDateInfo.AddMessageError(Res.GetString("4A7195F0-EE66-49B9-AB90-A5B5BF95303A", "Import for Storage (IS) Date must be today or a past date."));
				}

				switch (declarationType)
				{
					case JPImportDeclarationTypeList.Codes.K:
					case JPImportDeclarationTypeList.Codes.D:
					case JPImportDeclarationTypeList.Codes.U:
					case JPImportDeclarationTypeList.Codes.L:
					case JPImportDeclarationTypeList.Codes.B:
					case JPImportDeclarationTypeList.Codes.E:
					case JPImportDeclarationTypeList.Codes.R:
						if (iSDate.IsEmpty)
						{
							iSDateInfo.AddMessageError(Res.GetString("A232182D-166E-4308-8DB0-A7041BF6DAE7", "You have not entered Import for Storage (IS) Date."));
						}
						break;
					case JPImportDeclarationTypeList.Codes.S:
					case JPImportDeclarationTypeList.Codes.M:
					case JPImportDeclarationTypeList.Codes.A:
						break;
					default:
						if (!iSDate.IsEmpty)
						{
							iSDateInfo.AddMessageError(Res.GetString("3EBF0907-83AE-4F4F-9FC9-2C73B4845F65", "Import for Storage (IS) Date is not required."));
						}
						break;
				}
			}
		}

		const string OriginIDRequired = "Required if you want to generate the Customs Declaration Message (IDA).";

		bool MandatoryForThisDecType
		{
			get
			{
				return Parent.EntryInstruction is CusEntryInstruction instruction
					&& instruction.CEI_Style != JPImportDeclarationTypeList.Codes.H
					&& instruction.CEI_Style != JPImportDeclarationTypeList.Codes.N
					&& instruction.CEI_Style != JPImportDeclarationTypeList.Codes.Y;
			}
		}
	}
}
