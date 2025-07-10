using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business
{
	public class JobDeclarationValidation : AutoJPJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected BusinessObjectFactory Factory => Parent.Factory;

		IEnumerable<CusEntryInstruction> CustomsEntryInstructions => Parent.CustomsEntryInstructions.Cast<CusEntryInstruction>();

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateInspectionWitnessCode();
			ValidateExternalBrokerCode();
			ValidateAirCargoAgentNACCSCode();
			ValidateForwarderCode();
			ValidatePortOfLoadingIATACode();
			ValidateFinalDestinationIATACode();
		}

		public void ValidateForwarderCode()
		{
			ValidateCalculatedProperty(Parent.ForwarderCodeInfo);
		}

		protected void CheckForwarderCode()
		{
			var parent = Parent;
			var forwarderCode = Parent.ForwarderCode;
			if (!forwarderCode.IsEmpty && forwarderCode.Length != 5)
			{
				parent.ForwarderCodeInfo.AddMessageError(ResString.GetMultilingualString("13A298EB-EDE2-4CF5-ABD6-71002BC2A287", "NACCS User Code should be exactly 5 characters long."));
			}
		}

		public void ValidateAirCargoAgentNACCSCode()
		{
			ValidateCalculatedProperty(Parent.AirCargoAgentNACCSCodeInfo);
		}

		protected void CheckAirCargoAgentNACCSCode()
		{
			var parent = Parent;
			var airCargoAgentCode = Parent.AirCargoAgentNACCSCode;
			if (!airCargoAgentCode.IsEmpty && airCargoAgentCode.Length != 5)
			{
				parent.AirCargoAgentNACCSCodeInfo.AddMessageError(ResString.GetMultilingualString("60996B9F-60D4-4637-872F-1D226D1913A2", "NACCS User Code should be exactly 5 characters long."));
			}
		}

		public void ValidateAirCargoAgentLocationCode()
		{
			ValidateCalculatedProperty(Parent.AirCargoAgentLocationCodeInfo);
		}

		protected void CheckAirCargoAgentLocationCode()
		{
			var parent = Parent;
			CustomsRegistrationNumberValidation.ValidateCustomsCode(NotificationType.MessageError, OrgCusCode.JapanCodeTypes.AAL, parent.AirCargoAgentLocationCode, parent.AirCargoAgentLocationCodeInfo);
		}

		public void ValidatePortOfLoadingIATACode()
		{
			ValidateCalculatedProperty(Parent.PortOfLoadingIATACodeInfo);
		}

		public void ValidateFinalDestinationIATACode()
		{
			ValidateCalculatedProperty(Parent.FinalDestinationIATACodeInfo);
		}

		protected void CheckPortOfLoadingIATACode()
		{
			if (Parent.IsAir)
			{
				ValidationHelper.CheckIATACode(Parent.Factory, Parent.PortOfLoadingIATACodeInfo, Parent.JE_RL_NKPortOfLoadingInfo.HumanReadableName);
			}
		}

		protected void CheckFinalDestinationIATACode()
		{
			if (Parent.IsAir)
			{
				ValidationHelper.CheckIATACode(Parent.Factory, Parent.FinalDestinationIATACodeInfo, Parent.JE_RL_NKFinalDestinationInfo.HumanReadableName);
			}
		}

		internal void ValidateAdditionalEntryNumber(ZPropertyInfo info, ZString type, ZString number)
		{
			if (type == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG && number.Length > 16)
			{
				info.AddMessageError(Res.GetString("99FEC591-5739-4CF2-9414-5121AB4FB95F", "The booking number's maximum length is 16."));
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			if (Parent.IsImport)
			{
				var importerDocumentaryAddress = Parent.ImporterDocumentaryAddress;
				if ((importerDocumentaryAddress == null || importerDocumentaryAddress.IsEmpty) &&
						CustomsEntryInstructions.Any(c => c.CEI_Style == JPImportDeclarationTypeList.Codes.H
							|| c.CEI_Style == JPImportDeclarationTypeList.Codes.J
							|| c.CEI_Style == JPImportDeclarationTypeList.Codes.R))
				{
					var importer = Parent.Importer;
					if (importer != null)
					{
						var hasLPC = importer.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.JapanCodeTypes.LPC, Core.Constants.CountryCodes.Japan).Any();
						var hasCIE = importer.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.JapanCodeTypes.CIE, Core.Constants.CountryCodes.Japan).Any();
						if (!hasLPC && !hasCIE)
						{
							Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("8C8C806E-35A7-44B5-A433-B3D0FABA2047", "The selected importer must have a legal person code or importer/exporter code."));
						}
					}
				}
			}
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();

			if (Parent.IsImport)
			{
				var importerDocumentaryAddress = Parent.ImporterDocumentaryAddress;
				if ((importerDocumentaryAddress == null || importerDocumentaryAddress.IsEmpty) && Parent.Supplier != null &&
					!Parent.Supplier.RequiredDocuments.Cast<JobRequiredDocument>().Any(x => x.EQ_DocType == Core.Constants.RefDocTypes.PowerOfAttorneyCustoms &&
																						x.EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.Japan))
				{
					Parent.JE_OH_SupplierInfo.AddMessageError(Res.GetString("5144A782-F5F4-49EA-A487-740A6A8EA127", "The selected supplier should have POC in Japan."));
				}
			}
		}

		protected override void CheckJE_ACP_POA()
		{
			base.CheckJE_ACP_POA();
			if (Parent.Representative != null)
			{
				if (Parent.JE_ACP_POA.IsEmpty)
				{
					if (!Parent.HasDesignatedCustomsCodes(Parent.Representative, new List<ZString> { OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS }))
					{
						Parent.JE_ACP_POAInfo.AddMessageError(Res.GetString("584C6F45-499B-4A0A-BA04-18E43A0EF93F", "You have not entered an ACP Power of Attorney."));
					}
				}
				else
				{
					var trackingRecord = Parent.GetDocumentTrackingRecord(Parent.Supplier);
					if (trackingRecord != null && trackingRecord.EQ_OH_DocumentOwner == Parent.Representative.OA_OH && trackingRecord.EQ_DocNumber != Parent.JE_ACP_POA)
					{
						Parent.JE_ACP_POAInfo.AddWarning(Res.GetString("42ADE77B-AEF8-469F-80C6-24A801B3ECE3", "The entered ACP Power of Attorney is different from the one configured in the Supplier organization: {0}", trackingRecord.EQ_DocNumber));
					}
				}
			}
		}

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();
			if (Parent.Representative != null)
			{
				if (Parent.JE_ACP_POA.IsEmpty && !Parent.HasDesignatedCustomsCodes(Parent.Representative, new List<ZString> { OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS }))
				{
					Parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("10CF42A3-98E4-419C-8774-FAD9C436D46D", "The selected Attorney for Customs Procedures does not have a Customs Importer/Exporter Code. To add one, click on the organization field and press F3 to visit the organization, go to Details > Config > Registration Numbers/Codes, and add a new registration with the entered address as the Premises Address, JP as the Country/Region of Issue, and CIE/LPC/JAS as the Type."));
				}
			}
		}

		protected override void CheckJE_ValuationDate()
		{
			base.CheckJE_ValuationDate();

			var invoiceHeader = Parent.Invoices.FirstOrDefault();
			if (Parent.JE_ValuationDate.IsEmpty && (invoiceHeader == null || invoiceHeader.JZ_ValuationDateOverride.IsEmpty))
			{
				Parent.JE_ValuationDateInfo.AddMessageError(Res.GetString("A15D2CB7-97F2-499F-BA96-B94AA21B6BE3", "You have not entered Valuation Date."));
			}
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();
			if (Parent.JE_TransportMode == Core.Constants.TransportModes.Sea && Parent.JE_HouseBill.Length < 5)
			{
				Parent.JE_HouseBillInfo.AddMessageError(Res.GetString("59C2736A-56C2-4E8D-A4FF-FF290448C000", "House Bills should have at least 5 characters."));
			}
			else if (Parent.JE_TransportMode == Core.Constants.TransportModes.Air && Parent.JE_HouseBill.Length > 20)
			{
				Parent.JE_HouseBillInfo.AddMessageError(Res.GetString("59C2736A-56C2-4E8D-A4FF-FF290448C001", "HAWB should have less than or equal to 20 characters."));
			}
			if (Parent.IsExport && Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_HouseBillInfo);
			}
		}

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();

			if (string.IsNullOrWhiteSpace(Parent.JE_GS_NKCusAgent))
			{
				Parent.JE_GS_NKCusAgentInfo.AddMessageError(Res.GetString("941543E7-3762-48F9-A901-CE9E12BEFEC7", "You have not entered a Customs Broker."));
			}

			var cusAgent = Parent.CusAgent;
			if (cusAgent != null)
			{
				var certificates = cusAgent.Certificates.Where(x => x.XZ_Type == CertificateTypePairList.Codes.BR1 && x.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Japan);
				var hasExpiredCertificates = certificates.Any(x => x.XZ_ExpiryOrDueDate < ZDateTime.Now);
				var hasAboutToExpiredCertificates = certificates.Any(x => x.XZ_ExpiryOrDueDate < ZDateTime.Now.AddDays(30));

				if (hasExpiredCertificates)
				{
					Parent.JE_GS_NKCusAgentInfo.AddMessageError(Res.GetString("59C2736A-56C2-4E8D-1234-FF290448C465", "The selected Broker's certificate has expired. Press F3 to visit the Staff and go to Human Resources > Certificate, ID and Training to update the JP-BRK certificate."));
				}
				else if (hasAboutToExpiredCertificates)
				{
					Parent.JE_GS_NKCusAgentInfo.AddMessageError(Res.GetString("59C2736A-56C2-4E8D-1230-FF290448C465", "The selected Broker's certificate will expire in 30 days. Please remember to renew the certificate."));
				}
			}
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo, ResString.GetMultilingualString("59C2736A-56C2-4E8D-A4FF-FF290448C999", "The entered customs office does not exist."));
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			var parent = Parent;
			var targetInfo = parent.JE_RL_NKPortOfLoadingInfo;
			var portOfLoading = parent.JE_RL_NKPortOfLoading;

			if (!parent.ShouldShowDescriptionForUnknownPortName)
			{
				base.CheckJE_RL_NKPortOfLoading();
			}

			if (parent.IsForMarineProductsExport && portOfLoading != "ZZZ")
			{
				targetInfo.AddMessageError(Res.GetString("145FB855-CD4A-4969-B7E3-4A350C5B8507", "Port of Loading must be ZZZ when Customs Depot is 洋上."));
			}

			if (parent.IsImport && (portOfLoading.StartsWith(Core.Constants.CountryCodes.Japan) || portOfLoading.StartsWith(Common.Constants.CountryCodes.UnknownCountryCode)))
			{
				targetInfo.AddMessageError(Res.GetString("FF7B7448-8FD7-4779-8816-4A9AE49CFF32", "Port of Loading cannot be located in JP or ZY."));
			}

			if (parent.IsExport && portOfLoading.IsEmpty)
			{
				targetInfo.AddWarning(ValidationConstants.SpecialMandatoryErrorMessage(targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJE_DefermentAccountNumber()
		{
			base.CheckJE_DefermentAccountNumber();

			var info = Parent.JE_DefermentAccountNumberInfo;

			switch (Parent.JE_PaymentMethod)
			{
				case PaymentMethodCodeList.Codes.RealtimeAccount:
				case PaymentMethodCodeList.Codes.RealtimeAccountNotForLumpSumPayment:
				case PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeduction:
				case PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeduction:
				case PaymentMethodCodeList.Codes.TaxDeadlineExtension:
				case PaymentMethodCodeList.Codes.RealtimeAccountAll:
				case PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeductionOrTaxDeadlineExtension:
				case PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeductionOrTaxDeadlineExtension:
					if (Parent.JE_DefermentAccountNumber.IsEmpty)
					{
						info.AddMessageError(Res.GetString("81FE1A94-A1FE-4CC0-87D7-92A88FDC725F", "Bank account number is required when Payment Method is {0}.", Parent.JE_PaymentMethod));
					}
					break;
				default:
					break;
			}
			switch (Parent.JE_PaymentMethod)
			{
				case PaymentMethodCodeList.Codes.DP:
					if (!Parent.JE_DefermentAccountNumber.IsEmpty)
					{
						info.AddMessageError(Res.GetString("75C9A2E3-397E-4D90-BA59-5765ADAC3DFA", "Bank Account Number is not required when Payment Method is empty."));
					}
					break;
				case PaymentMethodCodeList.Codes.DirectPaymentNotforLumpSumPayment:
				case PaymentMethodCodeList.Codes.MPN:
				case PaymentMethodCodeList.Codes.MPNNotForLumpSumPayment:
					if (!Parent.JE_DefermentAccountNumber.IsEmpty)
					{
						info.AddMessageError(Res.GetString("639BA0B8-31F3-4A7A-94B2-6B3662D37B77", "Bank Account Number is not required when Payment Method is {0}.", Parent.JE_PaymentMethod));
					}
					break;
				default:
					break;
			}

			var leviedDeclarationTypeList = Factory.GetCachedValue<LeviedDeclarationTypeList>();
			var customsEntryInstructions = CustomsEntryInstructions;
			var leviedDeclarationType = customsEntryInstructions.Select(c => c.CEI_Style).FirstOrDefault(style => leviedDeclarationTypeList.ContainsCode(style));
			if (!leviedDeclarationType.IsEmpty && !Parent.JE_DefermentAccountNumber.IsEmpty)
			{
				info.AddMessageError(Res.GetString("623839C2-55E4-4B0E-9C92-24F060E62C4E", "Bank account number must be empty for Declaration Type {0}.", leviedDeclarationType));
			}

			if (customsEntryInstructions.Any(x => x.CEI_BeforePermitApplicationReason == BeforePermitApplicationReasonList.Codes.X9) && Parent.JE_DefermentAccountNumber.IsEmpty)
			{
				info.AddMessageError(Res.GetString("EE5663E5-11C7-40D5-8082-98BDE847BF16", "Bank account number cannot be empty when Before Permit Application Reason is {0}.", BeforePermitApplicationReasonList.Codes.X9));
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();

			var info = Parent.JE_PaymentMethodInfo;

			if (Parent.JE_PaymentDeadlineExtension.IsEmpty && !Parent.JE_PaymentMethod.IsEmpty && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.MPN && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.MPNNotForLumpSumPayment && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.DirectPaymentNotforLumpSumPayment && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.RealtimeAccount && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.RealtimeAccountNotForLumpSumPayment && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeduction && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeduction)
			{
				info.AddMessageError(Res.GetString("CC21F3A8-0E70-4AD6-A184-FA4B4A1C4F95", "Payment Method must be one of empty, M, W, X, R, Y, E, S when Payment Deadline Extension is empty."));
			}

			if ((Parent.JE_PaymentDeadlineExtension == PaymentDeadlineExtensionCodeList.Codes.Comprehensive || Parent.JE_PaymentDeadlineExtension == PaymentDeadlineExtensionCodeList.Codes.Individual || Parent.JE_PaymentDeadlineExtension == PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndIndividual || Parent.JE_PaymentDeadlineExtension == PaymentDeadlineExtensionCodeList.Codes.Special) && !Parent.JE_PaymentMethod.IsEmpty && Parent.JE_PaymentMethod != PaymentMethodCodeList.Codes.MPN)
			{
				info.AddMessageError(Res.GetString("D68B9B3A-893A-4940-A2A0-9F405AE9FD66", "Payment Method must be empty or M when Payment Deadline Extension is {0}.", Parent.JE_PaymentDeadlineExtension));
			}
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();
			if (Parent.IsImport && Parent.Bills.Count == 0)
			{
				Parent.JE_MasterBillInfo.AddMessageError(Res.GetString("2E76FDCA-22C4-4EE4-A556-1CC06F9EBAE4", @"There is not any bill added under ""Packing - Bills""."));
			}
		}

		protected override void CheckJE_CustomsOfficeDepartment()
		{
			base.CheckJE_CustomsOfficeDepartment();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeDepartmentInfo, Parent.Lookups.CustomsOfficeDepartmentsList, ResString.GetMultilingualString("59C2736A-56C2-4E8D-A4FF-FF290448C666", "The entered customs office department does not exist."));
		}

		protected override void CheckJE_NACCSCredential()
		{
			base.CheckJE_NACCSCredential();
			if (!Parent.JE_GS_NKCusAgent.IsEmpty)
			{
				if (Parent.Lookups.NACCSCredentialsList.Count == 0)
				{
					Parent.JE_NACCSCredentialInfo.AddMessageError(ResString.GetMultilingualString("E26E6315-809F-4072-94B1-8BA275F51352", "The Broker you have selected does not have a valid credential. To add a valid credential, press F3 to visit the Staff screen, navigate to the Credentials tab, and add a CUS – Customs code."));
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_NACCSCredentialInfo);
				}
			}
		}

		protected override void CheckJE_CarrierCode()
		{
			base.CheckJE_CarrierCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CarrierCodeInfo);
		}

		protected override void CheckJE_DateAtOrigin()
		{
			base.CheckJE_DateAtOrigin();
			var parent = Parent;
			var checkOnSendingMessage = parent.IsEDASendingInProgress
				|| parent.IsEDA01SendingInProgress
				|| parent.IsECRSendingInProgress;
			if (parent.IsSea && parent.IsExport && (parent.JE_EntryStatus != EntryStatusList.Codes.CLR || checkOnSendingMessage) && parent.JE_DateAtOrigin.IsInThePastDatePartOnly)
			{
				parent.JE_DateAtOriginInfo.AddMessageError(Res.GetString("D9B43E1D-7D4E-46A1-8F6B-475D81FCA6DB", "Date of Departure must be today or a future date."));
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			var parent = Parent;
			var date = parent.JE_ExportDate;
			var targetInfo = parent.JE_ExportDateInfo;

			if (parent.IsForMarineProductsExport)
			{
				if (!date.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("B87D9B46-7A65-4C1D-BEE5-1B7E59C61FFE", "Date of Departure must be empty when Bonded Location Code is 洋上."));
				}
			}
			else
			{
				if (date.IsEmpty)
				{
					if (parent.IsExport && parent.IsSea && CustomsEntryInstructions.Any(c => !c.IsMailedCargo))
					{
						targetInfo.AddWarning(ValidationConstants.SpecialMandatoryErrorMessage(targetInfo.HumanReadableName));
					}
					else
					{
						targetInfo.AddMessageError(Res.GetString("E1FCA91D-AC92-471B-A27D-B7F3F8A881F5", "You have not entered Date of Departure."));
					}
				}
				else if (date.IsInThePastDatePartOnly)
				{
					targetInfo.AddMessageError(Res.GetString("D0AD30CA-F7B5-4AB0-BD6C-1C9CAA5F399D", "Please enter Date of Departure after the system date."));
				}
			}
		}

		protected override void CheckJE_RadioCallSign()
		{
			base.CheckJE_RadioCallSign();
			var parent = Parent;
			if (parent.IsSea && !parent.IsBasketRadioCallSign)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_RadioCallSignInfo);
			}
		}

		public void ValidateInspectionWitnessCode()
		{
			ValidateCalculatedProperty(Parent.InspectionWitnessCodeInfo);
		}

		protected void CheckInspectionWitnessCode()
		{
			var parent = Parent;
			var inspectionWitnessCode = Parent.InspectionWitnessCode;
			if (!inspectionWitnessCode.IsEmpty && inspectionWitnessCode.Length != 5)
			{
				Parent.InspectionWitnessCodeInfo.AddMessageError(ResString.GetMultilingualString("BE2BBF6E-FE96-4E8B-810C-17428489B8C9", "NACCS User Code should be exactly 5 characters long."));
			}
		}

		public void ValidateExternalBrokerCode()
		{
			ValidateCalculatedProperty(Parent.ExternalBrokerCodeInfo);
		}

		protected void CheckExternalBrokerCode()
		{
			var parent = Parent;
			var externalBrokerCode = Parent.ExternalBrokerCode;
			if (!externalBrokerCode.IsEmpty && externalBrokerCode.Length != 5)
			{
				parent.ExternalBrokerCodeInfo.AddMessageError(ResString.GetMultilingualString("13A298EB-EDE2-4CF5-ABD6-71002BC2A287", "NACCS User Code should be exactly 5 characters long."));
			}
		}

		#region Payment Deadline Extension

		protected override void CheckJE_PaymentDeadlineExtension()
		{
			base.CheckJE_PaymentDeadlineExtension();
			var info = Parent.JE_PaymentDeadlineExtensionInfo;

			void AddMessageError(IEnumerable<ZString> validCodes, ZString declarationType)
			{
				info.AddMessageError(Res.GetString("EB4937E1-8B78-4919-A3FA-2C10D31ADB4A", "The entered value is invalid for Declaration Type {0}. The valid values are {1}.", declarationType, validCodes.ToStringWithSeparator()));
			}

			if (!Parent.JE_PaymentDeadlineExtension.IsEmpty)
			{
				CustomsEntryInstructions.Select(c => c.CEI_Style).Distinct().ForEach(declarationType =>
				{
					switch (declarationType)
					{
						case JPImportDeclarationTypeList.Codes.C:
						case JPImportDeclarationTypeList.Codes.K:
						case JPImportDeclarationTypeList.Codes.U:
						case JPImportDeclarationTypeList.Codes.B:
							if (!NormalDeclarationExtendedCodes.Contains(Parent.JE_PaymentDeadlineExtension))
							{
								AddMessageError(NormalDeclarationExtendedCodes, declarationType);
							}
							break;
						case JPImportDeclarationTypeList.Codes.J:
						case JPImportDeclarationTypeList.Codes.P:
						case JPImportDeclarationTypeList.Codes.R:
						case JPImportDeclarationTypeList.Codes.T:
						case JPImportDeclarationTypeList.Codes.V:
							if (!SpecialDeclarationExtendedCodes.Contains(Parent.JE_PaymentDeadlineExtension))
							{
								AddMessageError(SpecialDeclarationExtendedCodes, declarationType);
							}
							break;
						case JPImportDeclarationTypeList.Codes.Y:
							if (!NormalDeclarationExtendedCodes.Except(PartiallyExtendedCodes).Contains(Parent.JE_PaymentDeadlineExtension))
							{
								AddMessageError(NormalDeclarationExtendedCodes.Except(PartiallyExtendedCodes), declarationType);
							}
							break;
						case JPImportDeclarationTypeList.Codes.F:
						case JPImportDeclarationTypeList.Codes.D:
						case JPImportDeclarationTypeList.Codes.L:
						case JPImportDeclarationTypeList.Codes.E:
							Parent.JE_PaymentDeadlineExtensionInfo.AddMessageError(MandatoryValidation.DoNotEnterMessage(Parent.JE_PaymentDeadlineExtensionInfo.HumanReadableName));
							break;
						default:
							break;
					}
				});
			}
		}

		protected override void CheckJE_ReceiptMode()
		{
			var parent = Parent;
			if (parent.IsExport && parent.IsSea)
			{
				base.CheckJE_ReceiptMode();
				ListValidation.MessageErrorIfInvalidCode(parent.JE_ReceiptModeInfo);
				if (parent.IsECR)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.JE_ReceiptModeInfo);
				}
			}
		}

		protected override void CheckJE_DeliveryMode()
		{
			if(Parent.IsExport && Parent.IsSea)
			{
				base.CheckJE_DeliveryMode();
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_DeliveryModeInfo);
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			var parent = Parent;
			var targetInfo = parent.JE_RL_NKFinalDestinationInfo;
			if (parent.IsExport && targetInfo.Value.IsEmpty)
			{
				targetInfo.AddWarning(ValidationConstants.SpecialMandatoryErrorMessage(targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			var parent = Parent;
			var targetInfo = parent.JE_VesselNameInfo;
			if (targetInfo.Value.IsEmpty && parent.IsSea &&
				CustomsEntryInstructions.Any(c => !c.IsMailedCargo && (c.IsExport || !c.IsBondedImportDeclarationType)))
			{
				targetInfo.AddWarning(ValidationConstants.SpecialMandatoryErrorMessage(targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			var parent = Parent;
			CheckJE_RL_NKOriginOrJE_RL_NKPortOfArrivalMandatory(parent, parent.JE_RL_NKOriginInfo);
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			var parent = Parent;
			CheckJE_RL_NKOriginOrJE_RL_NKPortOfArrivalMandatory(parent, parent.JE_RL_NKPortOfArrivalInfo);
		}

		void CheckJE_RL_NKOriginOrJE_RL_NKPortOfArrivalMandatory(JobDeclaration parent, ZPropertyInfo targetInfo)
		{
			if (parent.IsImport)
			{
				var customsEntryInstructions = parent.CustomsEntryInstructions.Cast<CusEntryInstruction>();
				if (parent.IsSea && customsEntryInstructions.Any(c => !c.IsBondedImportDeclarationType) && targetInfo.Value.IsEmpty)
				{
					targetInfo.AddWarning(ValidationConstants.SpecialMandatoryErrorMessage(targetInfo.HumanReadableName));
				}
				else if (parent.IsAir && customsEntryInstructions.Any(c => !c.IsBondedImportDeclarationType && (c.IsMailedCargo || c.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_BondedDate.IsEmpty))))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (Parent.IsAir)
			{
				if (!new Regex(@"^[A-Z]{2}\d{3}[A-Z0-9]?$").IsMatch(Parent.JE_VoyageFlightNo))
				{
					Parent.JE_VoyageFlightNoInfo.AddMessageError(Res.GetString("3FE347F1-D5B1-4604-83FE-9A8864465434", "The flight number is incorrect. It should consist of two uppercase letters, followed by three digits, and optionally, an additional uppercase letter or digit."));
				}
			}
		}

		ZString[] SpecialDeclarationExtendedCodes => Factory.GetCachedValue<PaymentDeadlineExtensionCodeList.SpecialDeclarationExtended>().GetAllCodesZString();

		ZString[] PartiallyExtendedCodes => Factory.GetCachedValue<PaymentDeadlineExtensionCodeList.PartiallyExtended>().GetAllCodesZString();

		ZString[] NormalDeclarationExtendedCodes => Factory.GetCachedValue<PaymentDeadlineExtensionCodeList.NormalDeclarationExtended>().GetAllCodesZString();

		#endregion
	}
}
