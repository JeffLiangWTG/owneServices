using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using StaffCertType = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes;

namespace Enterprise.Customs.CN.Business
{
	public class JobDeclarationValidation : AutoCNJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		internal IValidationModeProvider ValidationModeProvider => Parent;

		protected override string CannotChangeMessageTypeErrorText => Res.GetString("639b9c57-ac16-40e2-a669-095a0e6eca54", "You may not change the shipment type because messages have been sent or Declaration Unified Number has been set.");

		protected override void CheckJE_GB()
		{
			base.CheckJE_GB();

			var parentDec = Parent;
			ValidationHelper.CheckOrganizationHasValidRegNumbers(parentDec.Declarant, parentDec.CIQRequires || parentDec.IsImport, parentDec.JE_GBInfo, ValidationModeProvider.GetNotificationType(), true);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			Parent.JE_CustomsOfficeInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			CustomsOfficeValidation.CheckCustomsOfficeIsValid(Parent.JE_CustomsOfficeInfo, Parent.DateOfValuation, ValidationModeProvider);
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();
			var declaration = Parent;
			if (declaration.IsSea || declaration.IsRail || declaration.IsAir || declaration.IsRoad || declaration.IsPost)
			{
				declaration.JE_MasterBillInfo.AddNotificationIfNotEntered(declaration.JE_MasterBillInfo.HumanReadableName, ValidationModeProvider);

				if (declaration.IsRoad && !(declaration.JE_MasterBill.Length == 13 && declaration.JE_MasterBill.IsLettersAndNumbersOnlyOrEmpty))
				{
					declaration.JE_MasterBillInfo.AddNotification(Res.GetString("6C9F9496-C2F4-4273-91F1-E0ACBC9E66A7", "Transportation Batch Number should be 13 alphanumeric characters"), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJE_ShipmentIncoTerm()
		{
			base.CheckJE_ShipmentIncoTerm();
			Parent.JE_ShipmentIncoTermInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			if (Parent.IsImport)
			{
				Parent.JE_RL_NKOriginInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			if (Parent.IsExport)
			{
				Parent.JE_RL_NKFinalDestinationInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		readonly ZInt voyageWarningLimitLength = 6;

		internal static string VoyageBeyondWarningLimitLength => Res.GetString("ABB15A32-42A8-4D86-9040-34CB43168828", "Voyage exceeds the limit of 6 characters (including spaces). Please check the manifest.");

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (Parent.IsAir || Parent.IsSea)
			{
				Parent.JE_VoyageFlightNoInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}

			if (Parent.IsSea && Parent.JE_VoyageFlightNo.Length > voyageWarningLimitLength)
			{
				Parent.JE_VoyageFlightNoInfo.AddWarning(VoyageBeyondWarningLimitLength);
			}
		}

		readonly ZInt vesselWarningLimitLength = 19;

		internal static string VesselBeyondWarningLimitLength => Res.GetString("5649BEF9-80B3-4A88-8E03-219152934A2A", "Vessel exceeds the limit of 19 characters (including spaces). Please check the manifest.");

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (Parent.IsSea)
			{
				Parent.JE_VesselNameInfo.AddNotificationIfNotEntered(ValidationModeProvider);
				ListValidation.WarnIfInvalidCode(Parent.JE_VesselNameInfo);
				if (Parent.JE_VesselName.Length > vesselWarningLimitLength)
				{
					Parent.JE_VesselNameInfo.AddWarning(VesselBeyondWarningLimitLength);
				}
			}
		}

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			if (Parent.IsCustomsTransit)
			{
				if (Parent.IsExport && Parent.IsTransshipment && Parent.IsSea)
				{
					Parent.JE_TransportModeInlandInfo.AddNotificationIfNotEntered(ValidationModeProvider);
				}
				else
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JE_TransportModeInlandInfo);
				}
			}
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			Parent.JE_MessageSubTypeInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if ((Parent.WillGenerateExitingEntry && (Parent.TransportDataHelper.IsRail || Parent.TransportDataHelper.IsMail))
				|| (Parent.IsImport && Parent.CIQRequires))
			{
				Parent.JE_ExportDateInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			if (Parent.WillGenerateEnteringEntry)
			{
				Parent.JE_DateOfArrivalInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			if (Parent.WillGenerateEnteringEntry || Parent.CIQRequires)
			{
				Parent.JE_LocationOfGoodsInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			var declaration = Parent;
			if (!declaration.TransportDataHelper.IsLoadingInChina && declaration.JE_MessageSubType == DecTypeList.Codes.Both)
			{
				Parent.JE_RL_NKPortOfLoadingInfo.AddNotification(Res.GetString("ab36f422-7b5f-4028-b907-7fa7b3c6b7d6", "Port of Loading should be a port in China."), ValidationModeProvider);
			}
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			var declaration = Parent;
			if (!declaration.TransportDataHelper.IsDischargeInChina && declaration.JE_MessageSubType == DecTypeList.Codes.Both)
			{
				Parent.JE_RL_NKPortOfArrivalInfo.AddNotification(Res.GetString("29bf3a4c-c8f9-407b-a963-d86d3bbdbc61", "Port of Discharge should be a port in China."), ValidationModeProvider);
			}
		}

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();
			var targetInfo = Parent.JE_GS_NKCusAgentInfo;

			MandatoryValidation.WarnIfNotEntered(targetInfo);

			if (Parent.CusAgent != null)
			{
				if (Parent.OperatorCardID.IsEmpty)
				{
					targetInfo.AddWarning(Res.GetString("605536D6-A902-4DD3-906D-1C05DA5F22B7", "Broker should have an effective e-port operator card ID (CNO)."));
				}

				var nameOnBrk = Parent.NameOnBrokerCertificate;
				if (!Parent.BrokerCertificateNumber.IsEmpty && !nameOnBrk.IsEmpty && nameOnBrk.IsWesternEuropeanOrEmpty)
				{
					targetInfo.AddWarning(ShouldHaveChineseNameMessage(StaffCertType.BRK));
				}

				var nameOnCno = Parent.NameOnOperatorCard;
				if (!Parent.OperatorCardID.IsEmpty && !nameOnCno.IsEmpty && nameOnCno.IsWesternEuropeanOrEmpty)
				{
					targetInfo.AddWarning(ShouldHaveChineseNameMessage(StaffCertType.CNO));
				}
			}
		}

		ZString ShouldHaveChineseNameMessage(string certType)
		{
			return Res.GetString("1994AADC-C9B1-4E6A-BD71-B87AAC0AB0DC", "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate {0}/CN.", certType);
		}

		#region AddInfo Properties

		protected override void CheckJE_LicenseInvolved()
		{
			base.CheckJE_LicenseInvolved();

			if (Parent.IsTwoStepDeclaration && !Parent.JE_LicenseInvolved)
			{
				var invoiceLines = Parent.InvoiceLines.Cast<JobComInvoiceLine>();
				if (invoiceLines.Any(x => x.CusSupportingDocuments.Any()) ||
					invoiceLines.Any(x => x.CIQProductQualifications.Any()))
				{
					Parent.JE_LicenseInvolvedInfo.AddNotification(Res.GetString("4DA52401-D5CC-4C41-AC19-890A80B2587C", "License Involved should be ticked when any supporting documents or product qualifications entered on any Invoice Line."), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJE_InspectionInvolved()
		{
			base.CheckJE_InspectionInvolved();

			if (Parent.IsTwoStepDeclaration && !Parent.JE_InspectionInvolved)
			{
				if (Parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_CIQRequires))
				{
					Parent.JE_InspectionInvolvedInfo.AddNotification(Res.GetString("3CA7089E-11D1-45DD-A98D-3F9E19B06BAF", "Inspection & Quarantine Involved should be ticked when the Entry Instruction requires CIQ."), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJE_TaxInvolved()
		{
			base.CheckJE_TaxInvolved();

			if (Parent.IsTwoStepDeclaration && !Parent.JE_TaxInvolved)
			{
				if (Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_DutyMode != DutyModeList.Codes._3))
				{
					Parent.JE_TaxInvolvedInfo.AddNotification(Res.GetString("4769CC7F-9269-45E6-B3C8-80DDDCB1C5DA", "Tax Involved should be ticked when any duty mode on any Invoice Line is not {0} - {1}.", DutyModeList.Codes._3, DutyModeList.Descriptions._3), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJE_CNPortOfOrigin()
		{
			base.CheckJE_CNPortOfOrigin();
			if (Parent.WillGenerateEnteringEntry)
			{
				Parent.JE_CNPortOfOriginInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
			else
			{
				Parent.JE_CNPortOfOriginInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
			}
		}

		protected override void CheckJE_CNPortOfDestination()
		{
			base.CheckJE_CNPortOfDestination();
			if (Parent.WillGenerateExitingEntry)
			{
				Parent.JE_CNPortOfDestinationInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
			else
			{
				Parent.JE_CNPortOfDestinationInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
			}
		}

		protected override void CheckJE_CNTransportMode()
		{
			base.CheckJE_CNTransportMode();

			var declaration = Parent;
			declaration.JE_CNTransportModeInfo.AddNotificationIfInvalidCode(ValidationModeProvider);

			if (!declaration.TransportDataHelper.IsCrossBorder && declaration.JE_CNTransportMode.IsEmpty)
			{
				declaration.JE_CNTransportModeInfo.AddNotification(Res.GetString("ef87e9ea-ddeb-488e-a8c2-e18e646d9b36", "Transport Flow is required for a domestic transport."), ValidationModeProvider);
			}
			if (declaration.TransportDataHelper.IsCrossBorder && declaration.JE_MessageSubType != DecTypeList.Codes.Both && !declaration.JE_CNTransportMode.IsEmpty)
			{
				declaration.JE_CNTransportModeInfo.AddNotification(Res.GetString("050455dc-08e1-437d-9211-201636e8817d", "Transport Flow is not required for a cross-border transport."), ValidationModeProvider);
			}
		}

		protected override void CheckJE_OfficeOfEntryExit()
		{
			base.CheckJE_OfficeOfEntryExit();

			var declaration = Parent;
			declaration.JE_OfficeOfEntryExitInfo.AddNotificationIfNotEntered(ValidationModeProvider);

			if (!declaration.JE_OfficeOfEntryExit.IsEmpty
				&& CNRefCusCodeListLoader.GetCustomsOffice(declaration.Factory, declaration.JE_OfficeOfEntryExit, declaration.DateOfValuation) == null)
			{
				declaration.JE_OfficeOfEntryExitInfo.AddNotification(ListValidation.InvalidCodeMessageError.ToString(), ValidationModeProvider);
			}
		}

		protected override void CheckJE_CNLastPortBeforeEntry()
		{
			base.CheckJE_CNLastPortBeforeEntry();
			var declaration = Parent;
			if (declaration.IsImport)
			{
				declaration.JE_CNLastPortBeforeEntryInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
		}

		protected override void CheckJE_CIQOfficeOfEntryExit()
		{
			base.CheckJE_CIQOfficeOfEntryExit();
			Parent.JE_CIQOfficeOfEntryExitInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
		}

		protected override void CheckJE_RN_NKCountryOfTrade()
		{
			base.CheckJE_RN_NKCountryOfTrade();

			var declaration = Parent;
			var targetInfo = declaration.JE_RN_NKCountryOfTradeInfo;
			targetInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

			void CheckCountryOfTradeShouldMatchOrganization(CNJobDocAddress docAddress, ZString propertyDescription)
			{
				if (docAddress != null
					&& (docAddress.OverseasPartyCodeType == OrgCusCode.ChinaCodeTypes.MMR || docAddress.OverseasPartyCodeType == OrgCusCode.ChinaCodeTypes.SMR)
					&& docAddress.E2_RN_NKCountryCode != declaration.JE_RN_NKCountryOfTrade
				)
				{
					targetInfo.AddNotification(Res.GetString("39E03CC7-F6D5-4A4F-8555-251A75610368", "Country of Trade should match the Country Code of {0}.", propertyDescription), ValidationModeProvider);
				}
			}

			if (declaration.IsImport)
			{
				CheckCountryOfTradeShouldMatchOrganization(declaration.SupplierDocumentaryAddress, Res.GetString("D6B20B36-88D2-4F6A-8EC3-DA93553450A7", "Supplier"));
			}
			else if (declaration.IsExport)
			{
				CheckCountryOfTradeShouldMatchOrganization(declaration.ImporterDocumentaryAddress, Res.GetString("C847F9D1-405A-49CE-9859-53813C519760", "Importer"));
			}
		}

		protected override void CheckJE_ClearanceMode()
		{
			base.CheckJE_ClearanceMode();
			var declaration = Parent;
			if (!declaration.ClearanceModeReadOnly)
			{
				declaration.JE_ClearanceModeInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}

			if (declaration.IsTwoStepDeclaration)
			{
				if (declaration.CustomsEntryInstructions.Count > 1)
				{
					declaration.JE_ClearanceModeInfo.AddError(Res.GetString("67967CF6-03E1-4D43-A6BA-A7DFCD947503", "Only one Entry Instruction is allowed for two-step declaration clearance mode."));
				}

				var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();
				var documentCodesSupportsTSD = CNRefCusCodeListTypes.GetSupportingDocumentsSupportsTSD(declaration.Factory, declaration.DateOfValuation);

				if (invoiceLines.SelectMany(l => l.CusSupportingDocuments.Cast<CusSupportingDocument>().Select(d => d.CSI_Code))
					.Any(c => !documentCodesSupportsTSD.ContainsCode(c)))
				{
					declaration.JE_ClearanceModeInfo.AddNotification(Res.GetString("1837118D-E668-4C5C-905F-47EDAB366F3E", "There are some Supporting Documents on Invoice Lines are not supported in two-step declaration clearance mode."), ValidationModeProvider);
				}

				if (declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_CIQRequires))
				{
					if (invoiceLines.SelectMany(l => l.CIQProductQualifications.Cast<CIQProductQualification>().Select(d => d.CSI_Code)).Any(c => !ProductQualificationCodeList.IsProductQualificationCodeSupportTSD(c)))
					{
						declaration.JE_ClearanceModeInfo.AddNotification(Res.GetString("4A8B1E20-B5D8-49E4-B853-2F8682D02BE2", "There are some Product Qualifications on Invoice Lines are not supported in two-step declaration clearance mode."), ValidationModeProvider);
					}
				}

				var tariffCodeSet = invoiceLines.Where(l => l.UniversalTariff != null && l.UniversalTariff.DoesNotSupportTSD())
					.Select(l => l.UniversalTariff.ZZ1_TariffCode.SubstringSafe(0, 6))
					.ToHashSet();

				if (tariffCodeSet.Any())
				{
					var limitShowCount = 10;
					var tariffCodeStringNotSupportTSD = tariffCodeSet.Count > limitShowCount ? string.Join(",", tariffCodeSet.Take(limitShowCount).OrderBy(v => v).Append("...")) : string.Join(",", tariffCodeSet.OrderBy(v => v));
					declaration.JE_ClearanceModeInfo.AddNotification(Res.GetString("EEE55316-BAF5-4910-A902-88B7614B9836", @"There are some Tariffs on Invoice Lines are not supported in two-step declaration clearance mode.
(e.g. {0})", tariffCodeStringNotSupportTSD), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJE_VoyageInland()
		{
			base.CheckJE_VoyageInland();
			var declaration = Parent;
			if (declaration.IsExport && declaration.IsTransshipment && declaration.IsSea && declaration.IsInlandWaterwayTransport)
			{
				declaration.JE_VoyageInlandInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		protected override void CheckJE_VesselInland()
		{
			base.CheckJE_VesselInland();
			var declaration = Parent;
			if (declaration.IsCustomsTransit)
			{
				if (declaration.IsImport)
				{
					if (declaration.IsInlandRoadTransport && declaration.JE_VesselInland.IsEmpty)
					{
						declaration.JE_VesselInlandInfo.AddWarning(VehicleRoadMessage);
					}
				}
				else if (declaration.IsExport)
				{
					if (declaration.IsDeclaringInAdvance)
					{
						if (declaration.IsInlandRoadTransport && declaration.JE_VesselInland.IsEmpty)
						{
							declaration.JE_VesselInlandInfo.AddWarning(VehicleRoadMessage);
						}
					}
					else if (declaration.IsTransshipment)
					{
						if (declaration.IsSea)
						{
							if (declaration.IsInlandWaterwayTransport)
							{
								declaration.JE_VesselInlandInfo.AddNotificationIfNotEntered(VesselInlandMessage, ValidationModeProvider);
							}
							else if (declaration.IsInlandCarNumberApplicable)
							{
								declaration.JE_VesselInlandInfo.AddNotificationIfNotEntered(CarNumberMessage, ValidationModeProvider);
							}
						}
					}
				}
			}
		}

		protected override void CheckJE_TransitMode()
		{
			base.CheckJE_TransitMode();

			var declaration = Parent;
			declaration.JE_TransitModeInfo.AddNotificationIfInvalidCode(ValidationModeProvider);

			if (declaration.WillGenerateBothEntries && declaration.IsCustomsTransit)
			{
				declaration.JE_TransitModeInfo.AddNotification(Res.GetString("8B93C7F7-FDAB-4D56-AC66-F9FA327EA9B6", "Transit Mode is not available for Declaration Type 'BTH'."), ValidationModeProvider);
			}

			if (declaration.IsCustomsTransit && !declaration.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Any(
				x => x.CE_EntryType == AdditionalReferenceNumberTypes.Codes.DTDPreNumber || x.CE_EntryType == AdditionalReferenceNumberTypes.Codes.GoodsCarriedListNo))
			{
				if (declaration.IsImport)
				{
					if (!(declaration.IsTransshipment && (declaration.IsSea || declaration.IsRail || declaration.IsAir)))
					{
						declaration.JE_TransitModeInfo.AddNotification(DTDOrGclRequiredMessage, declaration);
					}
				}
				else if (declaration.IsExport)
				{
					if (!(declaration.IsSea && declaration.IsTransshipment) && declaration.CustomsEntryInstructions.Count <= 1)
					{
						declaration.JE_TransitModeInfo.AddNotification(DTDOrGclRequiredMessage, declaration);
					}
				}
			}
		}

		public static string DTDOrGclRequiredMessage => Res.GetString("BE4DF9B7-BF21-41DD-9E0E-DADE2A39B38C", "Reference Number DTD or GCL is required for Customs Transit declaration.");
		public static string VehicleRoadMessage => Res.GetString("3F504590-595D-44D3-B321-7ED9404A6F8F", "Please enter the car number of the vehicle if the goods transited by road in Guangdong province.");
		public static string VesselInlandMessage => Res.GetString("48BFB702-8427-41D8-923B-2C9994092260", "Vessel Inland");
		public static string CarNumberMessage => Res.GetString("1EF43A0B-2718-4321-97D2-3338788DCFBC", "Car Number");

		#endregion

		new JobDeclaration Parent => (JobDeclaration)base.Parent;
	}
}
