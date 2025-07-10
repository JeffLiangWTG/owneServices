using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class IMPJobDeclarationValidation : JobDeclarationValidation
	{
		public IMPJobDeclarationValidation(JobDeclaration declaration) : base(declaration)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateImporterType();
			ValidateCustomsBrokerCommentCode1();
			ValidateCustomsBrokerCommentCode2();
			ValidateCustomsBrokerCommentCode3();
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DateOfArrivalInfo);

			if ((Parent.JE_DeclarationPlan.Equals(ImportCustomsClearancePlanCodeList.Codes.A) || Parent.JE_DeclarationPlan.Equals(ImportCustomsClearancePlanCodeList.Codes.B)) && Parent.JE_DateOfArrival < (Parent.LastestCustomsEntryIssueDate.IsEmpty ? ZDateTime.Today : Parent.LastestCustomsEntryIssueDate))
			{
				Parent.JE_DateOfArrivalInfo.AddMessageError(Res.GetString("7204293D-59C3-42FC-839F-E37059FE7BF9", "Arrival Date at Discharge Port must be greater than or equal to Declaration Date for Declaration Plan A or B."));
			}
		}

		public void ValidateUnderbondMovementArrivalDate()
		{
			ValidateCalculatedProperty(Parent.UnderbondMovementArrivalDateInfo);
		}

		protected void CheckUnderbondMovementArrivalDate()
		{
			if (IsUnderbondPeriodRequired && Parent.UnderbondMovementArrivalDate.IsEmpty)
			{
				Parent.UnderbondMovementArrivalDateInfo.AddMessageError(Res.GetString("342ACE5E-F007-429A-9972-62B53FF07CE6", "Underbond Movement Arrival Date is mandatory for the Declaration Plan Code D or F."));
			}
			if (Parent.UnderbondMovementArrivalDate.IsValid)
			{
				if (Parent.UnderbondMovementArrivalDate > (Parent.LastestCustomsEntryIssueDate.IsEmpty ? ZDateTime.Today : Parent.LastestCustomsEntryIssueDate))
				{
					Parent.UnderbondMovementArrivalDateInfo.AddMessageError(ExpiryIssueDateMessageError);
				}
				if (Parent.UnderbondMovementArrivalDate < Parent.JE_DateOfArrival)
				{
					Parent.UnderbondMovementArrivalDateInfo.AddMessageError(ExpiryDateOfArrivalMessageError);
				}
			}
		}

		protected override void CheckJE_CarrierCode()
		{
			base.CheckJE_CarrierCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CarrierCodeInfo);
			if ((Parent.JE_DeclarationPlan == ImportCustomsClearancePlanCodeList.Codes.A || Parent.JE_DeclarationPlan == ImportCustomsClearancePlanCodeList.Codes.B) && Parent.JE_TransportMode == Core.Constants.TransportModes.Sea)
			{
				if (!Parent.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_ImportCargoManagementNumber.ToUpper() != Constants.ImportCargoManagementNumber.No))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CarrierCodeInfo);
				}
			}
		}

		public bool IsUnderbondPeriodRequired
		{
			get
			{
				return Parent.JE_DeclarationPlan == ImportCustomsClearancePlanCodeList.Codes.D || Parent.JE_DeclarationPlan == ImportCustomsClearancePlanCodeList.Codes.F;
			}
		}

		protected override void CheckJE_LocationOtherInformation()
		{
			base.CheckJE_LocationOtherInformation();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOtherInformationInfo);
			if (Parent.JE_LocationOtherInformation.SubstringSafe(0, 3) != Parent.JE_CustomsOffice)
			{
				Parent.JE_LocationOtherInformationInfo.AddMessageError(Res.GetString("39A1B9BC-8327-45C3-AA48-CF2A0F206063", "The first three characters of the bonded warehouse code is not equal to the Customs office code. The Customs this declaration is sent under should be identical with the Customs which manages the bonded warehouse."));
			}
		}

		protected override void CheckJE_OH_Forwarder()
		{
			base.CheckJE_OH_Forwarder();
			var courierCompanyID = Parent.Forwarder?.GetRegistrationNumber(Constants.IdentificationType.CourierCompanyID) ?? ZString.Empty;
			if (Parent.JE_MessageSubType == ExportTypeCodeList.Codes.E || Parent.JE_PaymentMethod == PaymentMethodCodeList.Codes._18)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ForwarderInfo);
				if (Parent.Forwarder != null)
				{
					if (courierCompanyID.IsEmpty)
					{
						Parent.JE_OH_ForwarderInfo.AddMessageError(Res.GetString("D355AEBA-2F5F-40EC-8270-4F60F14DC2CB", "There is no Courier Company ID for this organization. Please press F3 here and add a number of type 'SDC' in Config > Registration Numbers/Codes on the Organization form."));
					}
				}
			}
			if (!courierCompanyID.IsEmpty && MessageFunctions.GetRefCusCodeList(Parent.Factory, courierCompanyID, Constants.ZZ.NKCodeType.ExpressDeliveryServiceIDs) == null)
			{
				Parent.JE_OH_ForwarderInfo.AddMessageError(Res.GetString("742894D5-32A4-465C-95BB-63AA713874BB", "There is an invalid Courier Company ID for this organization. Please press F3 here and modify a number of type 'SDC' in Config > Registration Numbers/Codes on the Organization form."));
			}
		}

		public void ValidateImporterType()
		{
			ValidateCalculatedProperty(Parent.ImporterTypeInfo);
		}

		protected void CheckImporterType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ImporterTypeInfo);
		}

		public void ValidateCustomsBrokerCommentCode1()
		{
			ValidateCalculatedProperty(Parent.CustomsBrokerCommentCode1Info);
		}

		protected void CheckCustomsBrokerCommentCode1()
		{
			CheckCustomsBrokerCommentCode(Parent.CustomsBrokerCommentCode1, Parent.CustomsBrokerCommentCode1Info);
		}

		public void ValidateCustomsBrokerCommentCode2()
		{
			ValidateCalculatedProperty(Parent.CustomsBrokerCommentCode2Info);
		}

		protected void CheckCustomsBrokerCommentCode2()
		{
			CheckCustomsBrokerCommentCode(Parent.CustomsBrokerCommentCode2, Parent.CustomsBrokerCommentCode2Info);
		}

		public void ValidateCustomsBrokerCommentCode3()
		{
			ValidateCalculatedProperty(Parent.CustomsBrokerCommentCode3Info);
		}

		protected void CheckCustomsBrokerCommentCode3()
		{
			CheckCustomsBrokerCommentCode(Parent.CustomsBrokerCommentCode3, Parent.CustomsBrokerCommentCode3Info);
		}

		void CheckCustomsBrokerCommentCode(ZString customsBrokerCommentCode, ZPropertyInfo info)
		{
			ListValidation.MessageErrorIfInvalidCode(info);
			if (customsBrokerCommentCode.IsEmpty)
			{
				ZBool errorCheckError = ZBool.True;
				if (Parent.CustomsBrokerCommentCode1.IsEmpty && Parent.CustomsBrokerCommentCode2.IsEmpty && Parent.CustomsBrokerCommentCode3.IsEmpty)
				{
					errorCheckError = ZBool.False;
				}

				if (errorCheckError)
				{
					info.AddMessageError(CustomsBrokerCommentCodeError);
				}
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (IsContainerCheck)
			{
				if (Parent.CusContainers.Count == 0)
				{
					Parent.JE_TransportModeInfo.AddMessageError(ContainerYouHaveNotEntered);
				}
			}
			else
			{
				if (Parent.CusContainers.Count != 0)
				{
					Parent.JE_TransportModeInfo.AddMessageError(ContainerDoNotEntered);
				}
			}
		}

		protected override void CheckJE_MessageSubType()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MessageSubTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_MessageSubTypeInfo);
		}

		protected override void CheckJE_PaymentMethod()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_PaymentMethodInfo);
			if (Parent.JE_MessageSubType == ImportDeclarationTypeCodeList.Codes.E)
			{
				if (!PaymentMethodCodeList.IsImportTypeCodeERequired(Parent.JE_PaymentMethod))
				{
					Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("325534DB-304C-408A-8238-9F0E6ADF0393", "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'."));
				}
			}

			if (DeclarationProcedureTypeCodeList.MustHavePaymentMethod00(Parent.JE_ProcedureType) && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes._00)
			{
				Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("25A38572-8F88-497E-8C58-339F1390BFAC", "A method of payment must be '00' for the selected declaration procedure type."));
			}
			else if (DeclarationProcedureTypeCodeList.MustNotHavePaymentMethod00(Parent.JE_ProcedureType) && Parent.JE_PaymentMethod == PaymentMethodCodeList.Codes._00)
			{
				Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("68B5FD8A-4942-4E18-B160-7239DBE325F5", "A method of payment must not be '00' for the selected declaration procedure type."));
			}
			else if (DeclarationProcedureTypeCodeList.MustHavePaymentMethod01(Parent.JE_ProcedureType) && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes._01)
			{
				Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("8A5A3C30-C57A-4937-A4B1-131196A72502", "A method of payment must be '01' for the selected declaration procedure type."));
			}
		}

		protected override void CheckJE_PaidBy()
		{
			if (Parent.IsImporterInformationRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_PaidByInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_PaidByInfo);
		}

		protected override void CheckJE_GB()
		{
			base.CheckJE_GB();
			if (Parent.BrokerAddress != null)
			{
				if (Parent.BrokerAddress.CompanyName.IsEmpty)
				{
					Parent.JE_GBInfo.AddMessageError(GetMissingCompanyMessage((NoResString)"company name"));
				}
				if (Parent.BrokerAddress.OA_Phone.IsEmpty)
				{
					Parent.JE_GBInfo.AddMessageError(GetMissingCompanyMessage((NoResString)"phone number"));
				}
				if (Parent.BrokerAddress.OA_Email.IsEmpty)
				{
					Parent.JE_GBInfo.AddMessageError(GetMissingCompanyMessage((NoResString)"email"));
				}
			}
		}

		protected override void CheckJE_OA_ImporterAddress()
		{
			base.CheckJE_OA_ImporterAddress();

			if (Parent.IsImporterInformationRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_ImporterAddressInfo);
			}
		}

		public ZBool IsContainerCheck => IsContainerModeCheck && IsContainerTypeCheck;
		public ZBool IsContainerModeCheck => Parent.JE_TransportMode == TransportTypeList.Codes.Sea && (Parent.JE_ContainerPackMode == ContainerPackModeCodeList.Codes.FC || Parent.JE_ContainerPackMode == ContainerPackModeCodeList.Codes.LC);
		public ZBool IsContainerTypeCheck => !DeclarationProcedureTypeCodeList.IsCheckDigitM(Parent.JE_ProcedureType) && !ImportDeclarationTypeCodeList.IsSimpleDeclarationType(Parent.JE_MessageSubType);

		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			if (Parent.ImportTotalPackQty.IsEmpty)
			{
				if (!PackageKindCodeList.IsBulk(Parent.JE_TotalNoOfPacksPackType) && !Parent.JE_TotalNoOfPacksPackType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_TotalNoOfPacksPackTypeInfo);
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TotalNoOfPacksPackTypeInfo);
			}
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();

			if (DeclarationProcedureTypeCodeList.DoesRequireVesselOrFlightInformation(Parent.JE_ProcedureType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKPortOfArrivalInfo);
			}
		}

		protected override void CheckJE_CustomsLoadPort()
		{
			base.CheckJE_CustomsLoadPort();
			if (Parent.JE_CustomsLoadPort.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsLoadPortInfo);
			}
			else
			{
				if (Parent.DepartureCountry != null && Parent.DepartureCountryKRCCode.IsEmpty)
				{
					Parent.JE_CustomsLoadPortInfo.AddMessageError(Res.GetString("CB135E43-9F69-4B57-939B-273CA6DF0434", "You entered a code that does not correspond to the departure country KRC code."));
				}
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsLoadPortInfo, MessageErrorPortCodeInvalidMultilingual);
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();

			if (Parent.JE_TransportMode == Core.Constants.TransportModes.Sea && DeclarationProcedureTypeCodeList.DoesRequireVesselOrFlightInformation(Parent.JE_ProcedureType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo);
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();

			if (Parent.JE_TransportMode == Core.Constants.TransportModes.Air && DeclarationProcedureTypeCodeList.DoesRequireVesselOrFlightInformation(Parent.JE_ProcedureType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo);
			}
		}

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();

			if (DeclarationProcedureTypeCodeList.DoesRequireVesselOrFlightInformation(Parent.JE_ProcedureType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RN_NKTransportNationalityInfo);
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();

			if ((Parent.JE_DeclarationPlan == ImportCustomsClearancePlanCodeList.Codes.A || Parent.JE_DeclarationPlan == ImportCustomsClearancePlanCodeList.Codes.B) &&
				Parent.JE_TransportMode == Core.Constants.TransportModes.Sea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MasterBillInfo);
			}

			if (Parent.JE_DeclarationPlan == ImportCustomsClearancePlanCodeList.Codes.H)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_MasterBillInfo);
			}
		}

		protected override void CheckJE_OH_DutyPayer()
		{
			base.CheckJE_OH_DutyPayer();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_DutyPayerInfo);
			if (Parent.DutyPayer != null)
			{
				var wrapper = new OrganizationDocWrapper(Parent.DutyPayer);
				if (Parent.JE_PaidBy == PaidByCodeList.Codes.OTH && Parent.AreImporterDutyPayerTheSame)
				{
					Parent.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("3FF34FDA-186E-4DF9-84E0-CA1DCA08C92D", "You have indicated the payer type as different to importer, but they have the same business registration number."));
				}
				if (wrapper.AddressLine1.IsEmpty)
				{
					Parent.JE_OH_DutyPayerInfo.AddMessageError(NotEnteredOrgAddressMessage);
				}
				if (wrapper.CompanyName.IsEmpty)
				{
					Parent.JE_OH_DutyPayerInfo.AddMessageError(MissingCompanyNameMessage);
				}
				if (wrapper.PhoneNumber.IsEmpty)
				{
					Parent.JE_OH_DutyPayerInfo.AddMessageError(MissingPhoneNumberMessage);
				}
				if (wrapper.RepresentativeName.IsEmpty)
				{
					Parent.JE_OH_DutyPayerInfo.AddMessageError(MissingRepresentativeMessage);
				}

				if (Parent.IsRefundRequestValidationOn)
				{
					if (Parent.BankAccountNo.IsEmpty)
					{
						Parent.JE_OH_DutyPayerInfo.AddMessageError(MissingBankAccountNo);
					}
					if (Parent.BankCode.IsEmpty)
					{
						Parent.JE_OH_DutyPayerInfo.AddMessageError(MissingBankCode);
					}
					else if (!Parent.Lookups.BankTypeList.ContainsCode(Parent.BankCode))
					{
						Parent.JE_OH_DutyPayerInfo.AddMessageError(InvalidBankCode);
					}
					if (Parent.DutyPayer.GetIsIndividual())
					{
						if (wrapper.UnipassIDForIndividual.IsEmpty)
						{
							Parent.JE_OH_DutyPayerInfo.AddMessageError(JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage(Res.GetString("CDB1DAA8-A91E-4C0B-BFF8-B5F3BDEBBF3F", "Unipass ID"), IdentificationType.UnipassIDForIndividual));
						}
						if (wrapper.KoreanRegNoForResident.IsEmpty)
						{
							Parent.JE_OH_DutyPayerInfo.AddMessageError(JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage(Res.GetString("248F196C-8CD4-4AAA-8A50-E005F213A67E", "Citizen Registration Number"), IdentificationType.KoreanRegNoForResident));
						}
					}
					else
					{
						if (wrapper.UnipassIDForOrganization.IsEmpty)
						{
							Parent.JE_OH_DutyPayerInfo.AddMessageError(JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage(Res.GetString("CDB1DAA8-A91E-4C0B-BFF8-B5F3BDEBBF3F", "Unipass ID"), IdentificationType.UnipassIDForOrganization));
						}
						if (wrapper.BusinessRegNo.IsEmpty)
						{
							Parent.JE_OH_DutyPayerInfo.AddMessageError(JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage(Res.GetString("33F92CAF-AF72-42F1-9C41-A4D0EE097B16", "Business Registration Number"), IdentificationType.BusinessRegNo));
						}
					}
				}
				if (Parent.IsTaxExemptionSpecificDutyRateValidationOn)
				{
					if (Parent.DutyPayer.GetIsIndividual())
					{
						var identificationTypes = new string[] { IdentificationType.KoreanRegNoForResident, IdentificationType.PassportNo, IdentificationType.KoreanRegNoForForeigner, IdentificationType.UnipassIDForIndividual };
						var cusCodes = Parent.DutyPayer.GetRegistrationNumbers(identificationTypes);
						if (cusCodes.Length == 0)
						{
							Parent.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("28E87263-578B-4934-88DF-C50AB186947B", "There is no identification number for this organization. Press F3 here and add one of the types '01', 'PAS', '03', '05' in the organization form under Config > Registration Numbers/Codes."));
						}
					}
					else
					{
						var identificationTypes = new string[] { IdentificationType.BusinessRegNo, IdentificationType.ForeignCompanyID };
						var cusCodes = Parent.DutyPayer.GetRegistrationNumbers(identificationTypes);
						if (cusCodes.Length == 0)
						{
							Parent.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("1C88F414-A63F-44A3-8052-7840DB459515", "There is no identification number for this organization. Please press F3 here and add a number of type 'GBR' or '07' in Config > Registration Numbers/Codes on the Organization form."));
						}
					}
				}
				if (Parent.IsValidationModeSetFor934)
				{
					if (wrapper.BusinessRegNoOrIndividualID.IsEmpty)
					{
						if (wrapper.IsIndividual)
						{
							Declaration.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("D3949045-50E3-444E-BAC7-C0F2EF14BE60", "There is no Identification ID for this organization. Please press F3 here and add a number of type '01', 'PAS', '03', '05' in Config > Registration Numbers/Codes on the Organization form."));
						}
						else
						{
							if (wrapper.BuyerID.IsEmpty)
							{
								Declaration.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("422C9241-ABAF-4C4C-B2E1-D5770ED6C935", "There is no Identification ID for this organization. Please press F3 here and add a number of type 'GBR', '07' in Config > Registration Numbers/Codes on the Organization form."));
							}
						}
					}
				}

				var payerBizRegNo = Parent.DutyPayer.GetRegistrationNumber(IdentificationType.BusinessRegNo);
				if (string.IsNullOrEmpty(payerBizRegNo))
				{
					Parent.JE_OH_DutyPayerInfo.AddMessageError(GetMissingRegistrationNumberMessage(Res.GetString("33F92CAF-AF72-42F1-9C41-A4D0EE097B16", "Business Registration Number"), Constants.IdentificationType.BusinessRegNo));
				}
			}
		}

		protected override void CheckJE_TradeIndicatorWithKP()
		{
			base.CheckJE_TradeIndicatorWithKP();
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TradeIndicatorWithKPInfo);
		}

		protected override void CheckJE_GoldTrade()
		{
			base.CheckJE_GoldTrade();
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_GoldTradeInfo);
		}

		protected override void CheckJE_TradeType()
		{
			base.CheckJE_TradeType();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_TradeTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TradeTypeInfo);

			if (Declaration.JE_TradeType != ImportDealingTypeCodeList.Codes._15)
			{
				ZBool ordersCheck = false;
				foreach (CusEntryInstruction instruction in Declaration.CustomsEntryInstructions)
				{
					if (instruction.OnlineOrders.Count > 0)
					{
						ordersCheck = true;
						break;
					}
				}

				if (ordersCheck)
				{
					Declaration.JE_TradeTypeInfo.AddMessageError(OnlineOrdersDoNotEntered);
				}
			}
		}

		protected override void CheckJE_DeclarationPlan()
		{
			base.CheckJE_DeclarationPlan();
			if (!ImportDeclarationTypeCodeList.IsSimpleDeclarationType(Declaration.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_DeclarationPlanInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_DeclarationPlanInfo);
		}

		protected override void CheckJE_ProcedureType()
		{
			base.CheckJE_ProcedureType();
			if (Declaration != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_ProcedureTypeInfo);
				ListValidation.MessageErrorIfInvalidCode(Declaration.JE_ProcedureTypeInfo);
				if (Declaration.JE_CustomsLoadPort == Core.Constants.CountryCodes.KoreaSouth)
				{
					if (!DeclarationProcedureTypeCodeList.IsImportFromBondedAreaInKR(Declaration.JE_ProcedureType))
					{
						Declaration.JE_ProcedureTypeInfo.AddMessageError(Res.GetString("79DE3639-E100-4BFF-BE85-308DD82DB64C", "If Departure Country Code is 'KR' then, Declaration Procedure Type must be ('13', '29', '15', '28', '33', '36')"));
					}
				}
				if (Declaration.JE_PaymentMethod == PaymentMethodCodeList.Codes._33)
				{
					if (!DeclarationProcedureTypeCodeList.IsApplicableForPaymentPostClearance(Declaration.JE_ProcedureType))
					{
						Declaration.JE_ProcedureTypeInfo.AddMessageError(Res.GetString("068DF325-385C-4B6D-BAA6-076D5D3E01D2", "If Payment Type = '33' then, Declaration Procedure Type must be ('11', '13', '15', '16', '29', '36')"));
					}
				}
			}
		}

		protected override void CheckJE_TransshipmentPort()
		{
			base.CheckJE_TransshipmentPort();
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TransshipmentPortInfo);
		}

		protected override void CheckJE_MissedDecPenaltyRate()
		{
			base.CheckJE_MissedDecPenaltyRate();
			if (Parent.JE_MissedDecPenaltyRate < 0 || Parent.JE_MissedDecPenaltyRate > 100)
			{
				Parent.JE_MissedDecPenaltyRateInfo.AddWarning(ResString.GetMultilingualString("B9BE80EF-5317-4147-93A9-7F5636572F39", "Percentage value should be between 0 and 100."));
			}
		}

		protected override void CheckJE_TaxOffice()
		{
			base.CheckJE_TaxOffice();
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TaxOfficeInfo);
		}

		protected override void CheckJE_AuthorJobTitle()
		{
			base.CheckJE_AuthorJobTitle();
			if (Parent.IsValidationModeSetFor934)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuthorJobTitleInfo);
			}
		}

		protected override void CheckJE_AuthorName()
		{
			base.CheckJE_AuthorName();
			if (Parent.IsValidationModeSetFor934)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuthorNameInfo);
			}
		}

		protected override void CheckJE_AuthorPhone()
		{
			base.CheckJE_AuthorPhone();
			if (Parent.IsValidationModeSetFor934)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuthorPhoneInfo);
			}
		}

		protected override void CheckJE_AuditorJobTitle()
		{
			base.CheckJE_AuditorJobTitle();
			if (Parent.IsValidationModeSetFor934)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuditorJobTitleInfo);
			}
		}

		protected override void CheckJE_AuditorName()
		{
			base.CheckJE_AuditorName();
			if (Parent.IsValidationModeSetFor934)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuditorNameInfo);
			}
		}

		protected override void CheckJE_AuditorPhone()
		{
			base.CheckJE_AuditorPhone();
			if (Parent.IsValidationModeSetFor934)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuditorPhoneInfo);
			}
		}

		public static string ExpiryIssueDateMessageError => Res.GetString("F62C7DE7-BB3A-4F8F-87AA-F88075833632", "Underbond Movement Arrival Date must be less than or equal to Declaration Date.");
		public static string ExpiryDateOfArrivalMessageError => Res.GetString("C7911429-9E8B-4D83-8DC3-D8A62169781C", "Underbond Movement Arrival Date must be greater than or equal to Arrival Date At Discharge Port.");
		public static string CustomsBrokerCommentCodeError => Res.GetString("A2AD5CBC-C935-4314-8C6B-BA6052443FEB", "All three of Customs Broker comments should be entered or none should be entered. You have entered only one or two of them and left this empty.");
		public static string ContainerYouHaveNotEntered => Res.GetString("5E5F1DB3-2044-49B9-AA72-D75C279EB127", "For the entered transport mode and pack mode, you should enter at least one container.");
		public static string ContainerDoNotEntered => Res.GetString("2DB5CECD-123F-4D2C-BE68-1A290450A4C6", "Containers are not relevant for the entered transport mode, pack mode, procedure type and declaration type. Please remove containers.");
		public static string MissingBankAccountNo => Res.GetString("299B17AE-8ED6-4BD1-834D-23E717298A9F", "The refund account number is missing. Please press F3 here and enter refund account number on the tab Details > Config > Korea.");
		public static string MissingBankCode => Res.GetString("198A8941-A520-45EA-9D54-2BCFB0BC1297", "The refund bank code is missing. Please press F3 here and enter refund bank code on the tab Details > Config > Korea.");
		public static string InvalidBankCode => Res.GetString("67DE5E98-67E9-418F-B140-6A66A6660EF9", "The refund bank code is not in the list.");

		public static string OnlineOrdersDoNotEntered => Res.GetString("8C89167F-6E70-489D-A574-C9F0F5E11C0B", "Online orders are not relevant for the entered trade type. It must be 15.");
	}
}
