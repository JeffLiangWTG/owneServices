using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusEntryInstructionValidation : AutoJPCusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent) { }

		public CusEntryInstruction CusEntryInstruction => Parent as CusEntryInstruction;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJP_CustomsNotes();
			ValidateJP_OwnersNotes();
			ValidateJP_BrokersNotes();
			ValidateJP_MarksAndNumbers();
			ValidateTradeTypeChar();
			ValidateApprovalCertificateInfos();
			ValidateNoBlanks();
			ValidateExportControlNumber();
			ValidateCEI_CustomsWeight();
			ValidateCEI_CustomsVolume();
			ValidateCEI_BillNumber();
			ValidateReceiptMode();
			ValidateFinalDestination();
			ValidateMoveInNotice();
		}

		public void ValidateMoveInNotice()
		{
			ValidateCalculatedProperty(CusEntryInstruction.MoveInNoticeInfo);
		}

		protected void CheckCEI_BillNumber()
		{
			var cusEntryInstruction = CusEntryInstruction;
			var declaration = cusEntryInstruction.JobDeclaration;
			if (declaration != null && declaration.IsExportAndAir)
			{
				var billNumber = cusEntryInstruction.CEI_BillNumber;
				if (billNumber.IsEmpty)
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(cusEntryInstruction.CEI_BillNumberInfo);
				}
				else if ((!billNumber.StartsWith(declaration.JE_HouseBill) && !billNumber.StartsWith(declaration.JE_MasterBill)) || !new Regex(@"^/{1}\d{2}$").IsMatch(billNumber.SubstringSafe(billNumber.Length - 3)))
				{
					cusEntryInstruction.CEI_BillNumberInfo.AddMessageError(Res.GetString("543B7E0B-9C5D-4860-9B00-8CA8ABFE0619", "AWB Number is not in the correct format. It should start with Master Bill or House Bill, and end with an extension number (/NN for export) if applicable."));
				}
			}
		}

		protected void CheckMoveInNotice()
		{
			var entryInstruction = CusEntryInstruction;
			var moveInNotice = entryInstruction.MoveInNotice;
			if (!moveInNotice.IsEmpty && !entryInstruction.RequestMoveInNotice && !new Regex("^[A-Z]{3}[A-Z0-9]*$").IsMatch(moveInNotice))
			{
				entryInstruction.MoveInNoticeInfo.AddMessageError(Res.GetString("18C0C7B1-1086-4E97-8251-BDA708D0D368", "Only capital letters and digits are allowed with the first 3 characters must be capitalized."));
			}
		}

		public void ValidateExportControlNumber()
		{
			((IValidationInternals)this).Validate(CusEntryInstruction.ExportControlNumberInfo, CheckExportControlNumber);
		}

		public void ValidateCEI_BillNumber()
		{
			ValidateCalculatedProperty(CusEntryInstruction.CEI_BillNumberInfo);
		}

		public void CheckExportControlNumber()
		{
			var parent = CusEntryInstruction;
			var info = parent.ExportControlNumberInfo;
			ValidateCalculatedProperty(info);
			ValidateRCRExportControlNumber(info);
		}

		public void ValidateCEI_CustomsWeight()
		{
			((IValidationInternals)this).Validate(CusEntryInstruction.CEI_CustomsWeightInfo, CheckCEI_CustomsWeight);
		}

		public void ValidateCEI_CustomsVolume()
		{
			((IValidationInternals)this).Validate(CusEntryInstruction.CEI_CustomsVolumeInfo, CheckCEI_CustomsVolume);
		}

		public void ValidateReceiptMode()
		{
			((IValidationInternals)this).Validate(CusEntryInstruction.ReceiptModeInfo, CheckReceiptMode);
		}

		public void ValidateFinalDestination()
		{
			((IValidationInternals)this).Validate(CusEntryInstruction.FinalDestinationInfo, CheckFinalDestination);
		}

		void ValidateNoBlanks()
		{
			if (string.IsNullOrWhiteSpace(Parent.CEI_Style) && string.IsNullOrWhiteSpace(Parent.CEI_Description))
			{
				Parent.AddRowMessageError(Res.GetString("59651F2A-F0E4-4F97-AC4D-308629CEF004", "Please enter a unique Entry Instruction Name or Description."));
			}
		}

		public void ValidateApprovalCertificateInfos()
		{
			var entryInstruction = CusEntryInstruction;
			var declaration = entryInstruction.JobDeclaration;
			if (declaration != null)
			{
				var messageType = declaration.JE_MessageType;
				var approvalCertificateInfoCollection = entryInstruction.ApprovalCertificateInfos;

				if (messageType == JPJobMessageTypeList.Codes.Export)
				{
					var codesMaxOne = new string[] { ApprovalCertificateInfoCodes.ITNO, ApprovalCertificateInfoCodes.MOTS, ApprovalCertificateInfoCodes.HFNN };
					foreach (var codeMaxOne in codesMaxOne)
					{
						entryInstruction.ClearRowNotificationsContaining(codeMaxOne);
						var certainCodeCount = approvalCertificateInfoCollection.Where(x => x.CSI_Code == codeMaxOne).Count();
						if (certainCodeCount > 1)
						{
							entryInstruction.AddRowMessageError(Res.GetString("51B1C948-C1FC-4C15-8D24-F7B004091036", "The maximum row count for {0} is 1. You have entered {1} rows.", codeMaxOne, certainCodeCount));
						}
					}
				}
			}
		}

		protected override void CheckCEI_ValueType()
		{
			base.CheckCEI_ValueType();

			var parent = CusEntryInstruction;
			var info = parent.CEI_ValueTypeInfo;
			ListValidation.MessageErrorIfInvalidCode(info);

			var declaration = parent.JobDeclaration;
			if (declaration != null)
			{
				if (!(declaration.IsImport && parent.CEI_Style == JPImportDeclarationTypeList.Codes.Y))
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
				else if (!parent.CEI_ValueType.IsEmpty)
				{
					info.AddMessageError(Res.GetString("D4C59EEE-76BC-4CD2-8354-C956D1724EEB", "Value Type must be empty when Declaration Type is Y."));
				}
			}

			var entryHeader = parent.EntryHeader;
			if (entryHeader != null)
			{
				if (parent.CEI_ValueType == ValueTypeList.Codes.L && parent.IsOnlyContainSmallValueGoods)
				{
					info.AddMessageError(Res.GetString("007F942C-9167-4777-8B06-EA1031734CEC", "Large Value is input but there are only small value goods."));
				}

				if (parent.CEI_ValueType == ValueTypeList.Codes.S && entryHeader.AllEntryLines.Count > 1)
				{
					info.AddMessageError(Res.GetString("414F0AD1-F104-49B6-972D-421E9AFAC380", "Small Value is input but there are multiple lines to be declared."));
				}
			}
		}

		protected void CheckCEI_CustomsWeight()
		{
			var targetInfo = CusEntryInstruction.CEI_CustomsWeightInfo;
			if (CusEntryInstruction.CEI_CustomsWeight > CusEntryInstruction.CustomsWeightMaxValue)
			{
				if (CusEntryInstruction.CEI_GrossWeightUnit != Core.Constants.Weight.Tonnes)
				{
					targetInfo.AddMessageError(Res.GetString("B4F9E63A-0A88-401A-91B1-2572777B90F8", $"The maximum Customs Weight allowed by NACCS is {CusEntryInstruction.CustomsWeightMaxValue}. Please consider using T as the Gross Weight Unit."));
				}
				else
				{
					targetInfo.AddMessageError(Res.GetString("7457208A-5B08-4573-B917-2CCEF577A909", $"The maximum value allowed by NACCS is {CusEntryInstruction.CustomsWeightMaxValue}."));
				}
			}
		}

		protected void CheckCEI_CustomsVolume()
		{
			CheckUpperlimit(CusEntryInstruction.CEI_CustomsVolumeInfo);
		}

		void CheckUpperlimit(ZPropertyInfo targetInfo)
		{
			if ((ZDecimal)targetInfo.Value >= 1000000m)
			{
				targetInfo.AddMessageError(Res.GetString("13F84397-9C7A-4E0D-A82A-4D93996F7CFC", "Value exceeds the upper limit."));
			}
		}

		protected override void CheckCEI_DateForDuty()
		{
			base.CheckCEI_DateForDuty();
			var entryInstruction = CusEntryInstruction;
			var date = entryInstruction.CEI_DateForDuty;
			if (date.IsValid)
			{
				var declaration = entryInstruction.JobDeclaration;
				var checkOnSendingMessage = false;
				if (declaration != null)
				{
					checkOnSendingMessage = declaration.IsEDASendingInProgress
					|| declaration.IsEDA01SendingInProgress
					|| declaration.IsIDASendingInProgress
					|| declaration.IsIDA01SendingInProgress;
				}

				if (date.IsInThePastDatePartOnly && ((entryInstruction.EntryHeader?.EntryNumber.IsEmpty ?? true) || checkOnSendingMessage))
				{
					entryInstruction.CEI_DateForDutyInfo.AddMessageError(Res.GetString("D4C1BFF4-4B69-42C0-9F8E-D26F19F74788", "{0} must be today or a future date.", entryInstruction.CEI_DateForDutyInfo.HumanReadableName));
				}

				var today = ZDateTime.Today;
				var lastDayOfThisWeek = today.AddDays(DayOfWeek.Saturday - today.DayOfWeek);
				var exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsRate)
					.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date)
					.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date)
					.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);

				if (date > lastDayOfThisWeek && Parent.InvoiceLines.Any(x => x.JI_RX_NKLinePriceCurr != Core.Constants.CurrencyCodes.Japan && !x.Factory.ExistsInDatabase(ZZRefExchangeRateSchema.Constants.TableName, exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, x.JI_RX_NKLinePriceCurr))))
				{
					entryInstruction.CEI_DateForDutyInfo.AddMessageError(Res.GetString("3B9DFDD6-FA43-45F4-97C3-CC1A84C2F5E3", "Please select a date for which the exchange rate has been published."));
				}

				if (declaration != null && date > declaration.JE_DateAtOrigin)
				{
					entryInstruction.CEI_DateForDutyInfo.AddMessageError(Res.GetString("CCB7DDCB-F311-4CB2-9C67-394906165DF7", "Scheduled Declaration Date must be equal to or earlier than Date of Departure."));
				}
			}
		}

		public void ValidateJP_CustomsNotes()
		{
			ValidateCalculatedProperty(CusEntryInstruction.JP_CustomsNotesInfo);
		}

		protected void CheckJP_CustomsNotes()
		{
			CheckMaxLengthForMessage(CusEntryInstruction.JP_CustomsNotesInfo);
		}

		public void ValidateJP_OwnersNotes()
		{
			ValidateCalculatedProperty(CusEntryInstruction.JP_OwnersNotesInfo);
		}

		protected void CheckJP_OwnersNotes()
		{
			CheckMaxLengthForMessage(CusEntryInstruction.JP_OwnersNotesInfo);
		}

		public void ValidateJP_BrokersNotes()
		{
			ValidateCalculatedProperty(CusEntryInstruction.JP_BrokersNotesInfo);
		}

		protected void CheckJP_BrokersNotes()
		{
			CheckMaxLengthForMessage(CusEntryInstruction.JP_BrokersNotesInfo);
		}

		public void ValidateJP_MarksAndNumbers()
		{
			ValidateCalculatedProperty(CusEntryInstruction.JP_MarksAndNumbersInfo);
		}

		protected void CheckJP_MarksAndNumbers()
		{
			var cusEntryInstruction = CusEntryInstruction;
			var targetInfo = cusEntryInstruction.JP_MarksAndNumbersInfo;
			var declaration = cusEntryInstruction.JobDeclaration;

			if (targetInfo.Value.IsEmpty && declaration != null && declaration.IsSea && !cusEntryInstruction.IsMailedCargo &&
				(declaration.IsExport || !cusEntryInstruction.IsBondedImportDeclarationType))
			{
				targetInfo.AddWarning(ValidationConstants.SpecialMandatoryErrorMessage(targetInfo.HumanReadableName));
			}
			CheckMaxLengthForMessage(targetInfo);
		}

		void CheckMaxLengthForMessage(ZPropertyInfo info)
		{
			if (JPMessageUtils.ConvertStringToMessage(info.Value.ToString().Replace("\r\n", " ")).Length > info.MaxLength)
			{
				info.AddMessageError(Res.GetString("24B74FBE-A9C0-4E12-80FA-5024DA6753A3", "The input text exceeded the max length. It will be truncated in the sent message."));
			}
		}

		public void ValidateTradeTypeChar()
		{
			ValidateTradeTypeFirstChar();
			ValidateTradeTypeSecondChar();
			ValidateTradeTypeThirdChar();
		}

		public void ValidateTradeTypeFirstChar()
		{
			ValidateCalculatedProperty(CusEntryInstruction.TradeTypeFirstCharInfo);
		}

		protected void CheckTradeTypeFirstChar()
		{
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.TradeTypeFirstCharInfo, ResString.GetMultilingualString("8E1DE4ED-B869-4C20-92DC-0A0FC0D38890", "The selected value is invalid."));

			if (NumberOfEnteredTradeTypeCharIsIncorrect && CusEntryInstruction.TradeTypeFirstChar.IsEmpty)
			{
				CusEntryInstruction.TradeTypeFirstCharInfo.AddMessageError(ResString.GetMultilingualString("8E1DE4ED-B869-4C20-92DC-0A0FC0D31234", "You have not entered first char of Trade Type."));
			}
		}

		public void ValidateTradeTypeSecondChar()
		{
			ValidateCalculatedProperty(CusEntryInstruction.TradeTypeSecondCharInfo);
		}

		protected void CheckTradeTypeSecondChar()
		{
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.TradeTypeSecondCharInfo, ResString.GetMultilingualString("780AF00C-BA11-4048-8D51-0F3CCC58DC75", "The selected value is invalid."));

			if (NumberOfEnteredTradeTypeCharIsIncorrect && CusEntryInstruction.TradeTypeSecondChar.IsEmpty)
			{
				CusEntryInstruction.TradeTypeSecondCharInfo.AddMessageError(ResString.GetMultilingualString("8E1DE4ED-B869-4C20-92DC-0A0FC0D39876", "You have not entered second char of Trade Type."));
			}
		}

		public void ValidateTradeTypeThirdChar()
		{
			ValidateCalculatedProperty(CusEntryInstruction.TradeTypeThirdCharInfo);
		}

		protected void CheckTradeTypeThirdChar()
		{
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.TradeTypeThirdCharInfo, ResString.GetMultilingualString("332BDAE9-7E05-4461-BD71-2AE92A1F31F1", "The selected value is invalid."));

			var shouldMandatoryValidate = true;

			var declarationType = CusEntryInstruction.CEI_Style;
			if (CusEntryInstruction.JobDeclaration.IsImport)
			{
				switch (declarationType)
				{
					case JPImportDeclarationTypeList.Codes.K:
					case JPImportDeclarationTypeList.Codes.D:
					case JPImportDeclarationTypeList.Codes.U:
					case JPImportDeclarationTypeList.Codes.L:
					case JPImportDeclarationTypeList.Codes.B:
					case JPImportDeclarationTypeList.Codes.E:
					case JPImportDeclarationTypeList.Codes.R:
						shouldMandatoryValidate = false;
						break;
				}

				if (CusEntryInstruction.TradeTypeSecondChar == TradeTypeSecondChar.Codes.G)
				{
					shouldMandatoryValidate = false;
				}
			}

			if (NumberOfEnteredTradeTypeCharIsIncorrect && CusEntryInstruction.TradeTypeThirdChar.IsEmpty && shouldMandatoryValidate)
			{
				CusEntryInstruction.TradeTypeThirdCharInfo.AddMessageError(ResString.GetMultilingualString("8E1DE4ED-B869-4C20-92DC-0A0FC0D31357", "You have not entered third char of Trade Type."));
			}
		}

		protected override void CheckCEI_AnimalQuarantineCertificateType()
		{
			base.CheckCEI_AnimalQuarantineCertificateType();
			var cusEntryInstruction = CusEntryInstruction;
			ValidateCertificateTypes(cusEntryInstruction.IsImport, cusEntryInstruction.CEI_AnimalQuarantineCertificateTypeInfo);
		}

		protected override void CheckCEI_BeforePermitApplicationReason()
		{
			base.CheckCEI_BeforePermitApplicationReason();
			var cusEntryInstruction = CusEntryInstruction;
			if (cusEntryInstruction.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(cusEntryInstruction.CEI_BeforePermitApplicationReasonInfo);
			}
		}

		protected override void CheckCEI_BondedLocationCode()
		{
			base.CheckCEI_BondedLocationCode();
			var cusEntryInstruction = CusEntryInstruction;
			if (cusEntryInstruction.IsImport)
			{
				var declaration = cusEntryInstruction.JobDeclaration;
				var style = cusEntryInstruction.CEI_Style;
				var factory = Parent.Factory;

				if (factory.GetCachedValue<BondedImportDeclarationTypeList>().ContainsCode(style) || JPImportDeclarationTypeList.Codes.R == style)
				{
					if (!cusEntryInstruction.CEI_BondedLocationCode.IsEmpty)
					{
						cusEntryInstruction.CEI_BondedLocationCodeInfo.AddMessageError(Res.GetString("50B59256-3990-4276-9E09-AF437C8BA353", "{0} is not required.", CusEntryInstruction.CEI_BondedLocationCodeInfo.HumanReadableName));
					}
				}
				else
				{
					var code = declaration.GetBondedLocationCode();
					if (!code.IsEmpty && code != cusEntryInstruction.CEI_BondedLocationCode)
					{
						cusEntryInstruction.CEI_BondedLocationCodeInfo.AddMessageError(Res.GetString("6296AFD0-802A-4664-BA62-2E785899A92B", "The Bonded Warehouse organization CCP location code does not match the value entered."));
					}
				}

				if (factory.GetCachedValue<InbondDeclarationTypeList>().ContainsCode(style))
				{
					MandatoryValidation.MessageErrorIfNotEntered(cusEntryInstruction.CEI_BondedLocationCodeInfo);
				}

				if (!CusEntryInstruction.IsUsingBasketBondedLocationCode)
				{
					ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.CEI_BondedLocationCodeInfo, ResString.GetMultilingualString("1347BDA2-E16C-495D-B247-A1076F22B67C", "The entered {0} is invalid", CusEntryInstruction.CEI_BondedLocationCodeInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckCEI_CommercialValueType()
		{
			base.CheckCEI_CommercialValueType();
			var cusEntryInstruction = CusEntryInstruction;
			if (cusEntryInstruction.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(cusEntryInstruction.CEI_CommercialValueTypeInfo);
			}

			if (!cusEntryInstruction.CEI_CommercialValueType.IsEmpty)
			{
				var declaration = cusEntryInstruction.JobDeclaration;
				var style = cusEntryInstruction.CEI_Style;
				if (declaration != null && (style == JPImportDeclarationTypeList.Codes.H || style == JPImportDeclarationTypeList.Codes.N)
										&& declaration.IsImport && declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(c => c.JI_Tariff.KeepNumericCharacters().Length == 6))
				{
					var message = Res.GetString("0719F756-67E8-4801-B80D-1726D35948C5", "{0} must be empty when the {1} is H or N, and a 6-character Tariff Code is entered.", CusEntryInstruction.CEI_CommercialValueTypeInfo.HumanReadableName, cusEntryInstruction.CEI_StyleInfo.HumanReadableName);
					cusEntryInstruction.CEI_CommercialValueTypeInfo.AddMessageError(message);
				}
			}
		}

		protected override void CheckCEI_CommonControlNumber()
		{
			base.CheckCEI_CommonControlNumber();
			var cusEntryInstruction = CusEntryInstruction;
			if (cusEntryInstruction.IsImport && !cusEntryInstruction.CEI_CommonControlNumber.IsEmpty)
			{
				if (NumberOfHouseBills > 1)
				{
					cusEntryInstruction.CEI_CommonControlNumberInfo.AddMessageError(Res.GetString("1B30AED6-0334-4055-972E-416375994E6B", "Please do not enter {0} when there are multiple B/L.", CusEntryInstruction.CEI_CommonControlNumberInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckCEI_ContentInspectionResult()
		{
			base.CheckCEI_ContentInspectionResult();
			var cusEntryInstruction = CusEntryInstruction;
			if (cusEntryInstruction.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(cusEntryInstruction.CEI_ContentInspectionResultInfo);
			}
		}

		protected override void CheckCEI_CustomsOfficeDepartmentForSpecialDeclarations()
		{
			base.CheckCEI_CustomsOfficeDepartmentForSpecialDeclarations();
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.CEI_CustomsOfficeDepartmentForSpecialDeclarationsInfo, ResString.GetMultilingualString("05532DDD-B6F1-48C6-9621-C2355D59A959", "The entered customs office department does not exist."));
		}

		protected override void CheckCEI_CustomsOfficeForSpecialDeclarations()
		{
			base.CheckCEI_CustomsOfficeForSpecialDeclarations();
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.CEI_CustomsOfficeForSpecialDeclarationsInfo, ResString.GetMultilingualString("709AF55F-CF9C-47A4-8B37-365868229C5D", "The entered customs office does not exist."));
		}

		protected override void CheckCEI_DutyDrawback()
		{
			base.CheckCEI_DutyDrawback();
			var cusEntryInstruction = CusEntryInstruction;
			var info = cusEntryInstruction.CEI_DutyDrawbackInfo;
			if (!Parent.CEI_DutyDrawback.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(info);
			}

			var declaration = cusEntryInstruction.JobDeclaration;
			if (declaration != null && Parent.CEI_DutyDrawback == YesNoList.Codes.Yes)
			{
				var declarationType = cusEntryInstruction.CEI_Style;
				if (declarationType != JPImportDeclarationTypeList.TakeoverDeclarationTypeList.Codes.H && declarationType != JPImportDeclarationTypeList.TakeoverDeclarationTypeList.Codes.N && declarationType != JPImportDeclarationTypeList.Codes.Y)
				{
					info.AddMessageError(Res.GetString("0E039E12-ED7C-49D7-BC26-ADA1A0E6D420", "Cannot claim tax return for this type of declaration."));
				}
			}
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			var cusEntryInstruction = CusEntryInstruction;
			var declaration = cusEntryInstruction.JobDeclaration;
			var targetInfo = cusEntryInstruction.CEI_StyleInfo;
			var declarationType = cusEntryInstruction.CEI_Style;

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (declaration.IsForMarineProductsExport && declarationType != JPImportDeclarationTypeList.Codes.E)
			{
				targetInfo.AddMessageError(
						Res.GetString("E24D9B0D-40AB-480D-A868-F9FD7F1FBE45", "Declaration Type must be E when Customs Depot is 洋上."));
			}

			if (cusEntryInstruction.IsImport && declarationType == JPImportDeclarationTypeList.Codes.Y)
			{
				if (!declaration.JE_TransportMode.IsEmpty && declaration.JE_TransportMode != Core.Constants.TransportModes.Air)
				{
					targetInfo.AddMessageError(
						Res.GetString("59C2736A-56C2-4E8D-A4FF-FF290448C465", "Declaration Type \"Y\" must be only selected for AIR freight jobs"));
				}

				var invoiceLineCount = cusEntryInstruction.InvoiceLines.Length;
				if (invoiceLineCount > 1)
				{
					targetInfo.AddMessageError(
						Res.GetString("B8E30A5D-96E0-4810-984B-6DA108CB660F", "It is required to include one and only one invoice line when the Declaration Type is Y. You have included {0} invoice lines.", invoiceLineCount));
				}
			}

			if (cusEntryInstruction.IsExport)
			{
				var certificateTypes = new string[] { ApprovalCertificateInfoCodes.AEOH, ApprovalCertificateInfoCodes.AEOU };
				if (declarationType == JPExportDeclarationTypeList.Codes.N && cusEntryInstruction.ApprovalCertificateInfos.Cast<ApprovalCertificateInfo>().All(x => !certificateTypes.Contains(x.CSI_Code.ToString())))
				{
					targetInfo.AddMessageError(Res.GetString("82DCACD5-5DB6-4B01-B005-53DF12C40654", "Please add AEOU or AEOH as certificate."));
				}
			}
		}

		#region DeclaredCargoType

		protected override void CheckCEI_DeclarationCargoType()
		{
			base.CheckCEI_DeclarationCargoType();
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.CEI_DeclarationCargoTypeInfo);
		}

		#endregion

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			var cusEntryInstruction = CusEntryInstruction;
			ListValidation.MessageErrorIfInvalidCode(cusEntryInstruction.CEI_SubStyleInfo, cusEntryInstruction.Lookups.EntrySubStyleList);
		}

		protected override void CheckCEI_FoodHygieneCertificateType()
		{
			base.CheckCEI_FoodHygieneCertificateType();
			var cusEntryInstruction = CusEntryInstruction;
			ValidateCertificateTypes(cusEntryInstruction.IsImport, cusEntryInstruction.CEI_FoodHygieneCertificateTypeInfo);
		}

		protected override void CheckCEI_PlantProtectionCertificateType()
		{
			base.CheckCEI_PlantProtectionCertificateType();
			var cusEntryInstruction = CusEntryInstruction;
			ValidateCertificateTypes(cusEntryInstruction.IsImport, cusEntryInstruction.CEI_PlantProtectionCertificateTypeInfo);
		}

		protected override void CheckCEI_PreInspectedCargoType()
		{
			base.CheckCEI_PreInspectedCargoType();
			var cusEntryInstruction = CusEntryInstruction;
			if (cusEntryInstruction.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCode(cusEntryInstruction.CEI_PreInspectedCargoTypeInfo);
			}
		}

		protected override void CheckCEI_TradeControlOrder()
		{
			base.CheckCEI_TradeControlOrder();
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.CEI_TradeControlOrderInfo, ResString.GetMultilingualString("637CCBDF-5EF0-4DA7-B20A-6FD70F3D0294", "The value entered is invalid. Please select a value from the list."));
		}

		protected override void CheckCEI_BondedLocationName()
		{
			base.CheckCEI_BondedLocationName();
			var cusEntryInstruction = CusEntryInstruction;
			if (cusEntryInstruction.IsUsingBasketBondedLocationCode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(cusEntryInstruction.CEI_BondedLocationNameInfo);
			}
		}

		protected override void CheckCEI_CargoQuantityUnit()
		{
			base.CheckCEI_CargoQuantity();
			var parent = CusEntryInstruction;
			if (!parent.IsAir)
			{
				if (Parent.CEI_CargoQuantity > 0 && Parent.CEI_CargoQuantityUnit.IsEmpty)
				{
					Parent.CEI_CargoQuantityUnitInfo.AddMessageError(Res.GetString("DAD5D2DE-749A-A6B2-A63A-4CAA5056B3C9", "You have not entered Cargo Quantity Unit."));
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.CEI_CargoQuantityUnitInfo, CusEntryInstruction.JobDeclaration?.Lookups.PackingUnitTypesList ?? new CodeDescriptionPairList());
				}
			}
		}

		protected override void CheckCEI_GrossWeight()
		{
			base.CheckCEI_GrossWeight();
			var cusEntryInstruction = CusEntryInstruction;
			var targetInfo = cusEntryInstruction.CEI_GrossWeightInfo;
			MandatoryValidation.CheckNotNegative(targetInfo);
			CheckGrossWeightOrCargoQuantityMandatory(cusEntryInstruction, targetInfo);
		}

		protected override void CheckCEI_CargoQuantity()
		{
			base.CheckCEI_CargoQuantity();
			var cusEntryInstruction = CusEntryInstruction;
			var targetInfo = cusEntryInstruction.CEI_CargoQuantityInfo;
			MandatoryValidation.CheckNotNegative(targetInfo);
			CheckGrossWeightOrCargoQuantityMandatory(cusEntryInstruction, targetInfo);
		}

		void CheckGrossWeightOrCargoQuantityMandatory(CusEntryInstruction cusEntryInstruction, ZPropertyInfo targetInfo)
		{
			if (cusEntryInstruction.IsECR || (cusEntryInstruction.IsImport && cusEntryInstruction.IsAir && !cusEntryInstruction.IsBondedImportDeclarationType && (cusEntryInstruction.IsMailedCargo || cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_BondedDate.IsEmpty))))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckCEI_GrossWeightUnit()
		{
			base.CheckCEI_GrossWeightUnit();
			var cusEntryInstruction = CusEntryInstruction;
			var targetInfo = cusEntryInstruction.CEI_GrossWeightUnitInfo;
			if (cusEntryInstruction.CEI_GrossWeight > 0 && cusEntryInstruction.CEI_GrossWeightUnit.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("4C6FECCA-F285-46B3-B7D4-DC50D22171A6", "You have not entered Gross Weight Unit."));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckCEI_DeclarationCondition()
		{
			base.CheckCEI_DeclarationCondition();
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.CEI_DeclarationConditionInfo);
		}

		protected override void CheckCEI_ApprovalCertificateCategory()
		{
			base.CheckCEI_ApprovalCertificateCategory();
			var cusEntryInstruction = CusEntryInstruction;
			var targetInfo = cusEntryInstruction.CEI_ApprovalCertificateCategoryInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			var approvalCertificateCategory = cusEntryInstruction.CEI_ApprovalCertificateCategory;
			var invoiceLines = cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>();
			var isAppendixTable1 = invoiceLines.Any(invoiceLine => invoiceLine.IsAppendixTable1);
			var isAppendixTable2 = invoiceLines.Any(invoiceLine => invoiceLine.IsAppendixTable2);
			CheckApprovalCertificateCategoryAppendixTable1(targetInfo, approvalCertificateCategory, isAppendixTable1);
			CheckApprovalCertificateCategoryAppendixTable2(targetInfo, approvalCertificateCategory, isAppendixTable2);
			CheckApprovalCertificateCategoryEnteredExportNumber(targetInfo, approvalCertificateCategory, cusEntryInstruction.ApprovalCertificateInfos.Cast<ApprovalCertificateInfo>());
		}

		protected override void CheckCEI_SpecialCargoCode()
		{
			base.CheckCEI_SpecialCargoCode();
			ListValidation.MessageErrorIfInvalidCode(CusEntryInstruction.CEI_SpecialCargoCodeInfo);
		}

		protected override void CheckCEI_Volume()
		{
			base.CheckCEI_Volume();
			if (CusEntryInstruction.IsECR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(CusEntryInstruction.CEI_VolumeInfo);
			}
		}

		protected override void CheckCEI_VolumeUnit()
		{
			base.CheckCEI_VolumeUnit();
			if (Parent.CEI_Volume > 0 && Parent.CEI_VolumeUnit.IsEmpty)
			{
				Parent.CEI_VolumeUnitInfo.AddMessageError(Res.GetString("532D062D-DF65-885E-5A38-1936CBE1019B", "You have not entered Volume Unit."));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CEI_VolumeUnitInfo, CusEntryInstruction.Lookups.VolumeUnitList);
			}
		}

		protected override void CheckCEI_ContainerCount()
		{
			base.CheckCEI_ContainerCount();
			var info = CusEntryInstruction.CEI_ContainerCountInfo;
			MandatoryValidation.CheckNotNegative(info);

			var linkInvoiceLines = CusEntryInstruction.InvoiceLines;
			if (linkInvoiceLines.Length > 0)
			{
				var actualContainerCount = linkInvoiceLines.SelectMany(invoiceLine => invoiceLine.ContainersPivot.Select(c => c.C2_CO)).Where(c => c.IsValid).Distinct().Count();
				if (actualContainerCount > 0 && actualContainerCount != CusEntryInstruction.CEI_ContainerCount)
				{
					var invoiceLinesAbbreviation = Res.GetString("BDDC2744-6B08-4F0D-92A8-8609BAD40F54", "Inv. Lines");
					info.AddWarning(Res.GetString("4CE09561-9540-4463-A4C8-07298FA3AFF3", "The entered container count is different from the actual number of containers associated with this entry instruction through {0} > Containers, which is {1}", invoiceLinesAbbreviation, actualContainerCount));
				}
			}
		}

		protected override void CheckCEI_GoodsDescription()
		{
			base.CheckCEI_GoodsDescription();
			if (CusEntryInstruction.IsECR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(CusEntryInstruction.CEI_GoodsDescriptionInfo);
			}
		}

		protected override void ValidateAgainstLinkedEntryHeadersCore()
		{
			base.ValidateAgainstLinkedEntryHeadersCore();

			var parent = Parent;
			if (parent.EntryHeader?.MergedLines.Count > 99)
			{
				parent.AddRowMessageError(Res.GetString("60ACCCB3-FCEC-470C-9D85-5AD5C66328B0", "The number of entry lines merged from invoice lines exceeds the upper limit of a declaration message. Please confirm Misc. > Merged By, and Tariff, Custom Quantity and Additional Details of invoice lines. Please then Generate Entries (Merge) again to reflect changes."));
			}
		}

		void CheckApprovalCertificateCategoryAppendixTable1(ZPropertyInfo targetInfo, ZString approvalCertificateCategory, bool isAppendixTable1)
		{
			switch (approvalCertificateCategory)
			{
				case ApprovalCertificateCategoryCodeList.Codes.FE:
					if (!isAppendixTable1)
					{
						targetInfo.AddMessageError(Res.GetString("BBFAC334-5932-43BD-AE46-D2AF4A850135", "There is no invoice line that is for goods listed in trade control order appendix table 1."));
					}
					break;
				case ApprovalCertificateCategoryCodeList.Codes.FT:
					break;
				default:
					if (isAppendixTable1)
					{
						targetInfo.AddMessageError(Res.GetString("08C275F6-5506-4040-A930-6E6EE618BAEA", "There are invoice lines that are for goods listed in trade control order appendix table 1."));
					}
					break;
			}
		}

		void CheckApprovalCertificateCategoryAppendixTable2(ZPropertyInfo targetInfo, ZString approvalCertificateCategory, bool isAppendixTable2)
		{
			switch (approvalCertificateCategory)
			{
				case ApprovalCertificateCategoryCodeList.Codes.FE:
				case ApprovalCertificateCategoryCodeList.Codes.FT:
					break;
				case ApprovalCertificateCategoryCodeList.Codes.E1:
				case ApprovalCertificateCategoryCodeList.Codes.E2:
					if (!isAppendixTable2)
					{
						targetInfo.AddMessageError(Res.GetString("F0810F15-997C-4C2D-8204-6B99BBFBE6B3", "There is no invoice line that is for goods listed in trade control order appendix table 2."));
					}
					break;
				default:
					if (isAppendixTable2)
					{
						targetInfo.AddMessageError(Res.GetString("E42628C3-B30C-430E-B15A-379069732DEE", "There are invoice lines that are for goods listed in trade control order appendix table 2."));
					}
					break;
			}
		}

		void CheckApprovalCertificateCategoryEnteredExportNumber(ZPropertyInfo targetInfo, ZString approvalCertificateCategory, IEnumerable<ApprovalCertificateInfo> approvalCertificateInfos)
		{
			switch (approvalCertificateCategory)
			{
				case ApprovalCertificateCategoryCodeList.Codes.FE:
				case ApprovalCertificateCategoryCodeList.Codes.FT:
					if (!approvalCertificateInfos.Any(approvalCertificateCategory => ExportPermitNumber.Contains(approvalCertificateCategory.CSI_Code)))
					{
						targetInfo.AddMessageError(Res.GetString("114C2F02-9BEA-4825-925C-38F88FC4B460", "You have not entered an export permit number."));
					}
					break;
				case ApprovalCertificateCategoryCodeList.Codes.E1:
				case ApprovalCertificateCategoryCodeList.Codes.E2:
					if (!approvalCertificateInfos.Any(approvalCertificateCategory => ExportApprovalNumber.Contains(approvalCertificateCategory.CSI_Code)))
					{
						targetInfo.AddMessageError(Res.GetString("31CF69F9-541A-4763-B728-C0E5B5DF544D", "You have not entered an export approval number."));
					}
					break;
			}
		}

		void ValidateCertificateTypes(bool isimport, ZPropertyInfo info)
		{
			if (isimport)
			{
				ListValidation.MessageErrorIfInvalidCode(info);
				if (!info.Value.IsEmpty && NumberOfHouseBills > 1)
				{
					info.AddMessageError(Res.GetString("C2473784-0F7C-4C05-8D49-64D09B61F48A", "{0} should not be entered when there are more than one house bills.", info.HumanReadableName));
				}
			}
		}

		bool NumberOfEnteredTradeTypeCharIsIncorrect => CusEntryInstruction.TradeTypeFirstChar.IsEmpty != CusEntryInstruction.TradeTypeSecondChar.IsEmpty ||
																CusEntryInstruction.TradeTypeSecondChar.IsEmpty != CusEntryInstruction.TradeTypeThirdChar.IsEmpty;

		int NumberOfHouseBills => CusEntryInstruction.JobDeclaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill).Length;

		ZString[] ExportPermitNumber => new ZString[] {
			ApprovalCertificateInfoCodes.FENJ,
			ApprovalCertificateInfoCodes.FENO,
			ApprovalCertificateInfoCodes.FTNO
		};

		ZString[] ExportApprovalNumber => new ZString[] {
			ApprovalCertificateInfoCodes.ELNJ,
			ApprovalCertificateInfoCodes.ELNO
		};
	}
}
