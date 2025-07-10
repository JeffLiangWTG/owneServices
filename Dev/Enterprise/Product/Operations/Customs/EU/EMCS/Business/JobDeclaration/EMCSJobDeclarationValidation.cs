using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationValidation : JobDeclarationValidation
	{
		public EMCSJobDeclarationValidation(EMCSJobDeclaration parent)
			: base(parent)
		{
		}

		protected new EMCSJobDeclaration Parent => (EMCSJobDeclaration)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateSpecialInstructions();
			ValidateJourneyTime();
			ValidatePackages();
		}

		public void ValidateSpecialInstructions()
		{
			ValidateCalculatedProperty(Parent.SpecialInstructionsInfo);
		}

		protected void CheckSpecialInstructions()
		{
			if (Parent.TransportMode == Core.Constants.TransportModes.Other && Parent.SpecialInstructions.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SpecialInstructionsInfo, Res.GetString("cca2f5cf-8bb3-4814-a144-c5dffc57458c", "Special Instruction"));
			}
		}

		public void ValidateJourneyTime()
		{
			ValidateCalculatedProperty(Parent.JourneyTimeFormatPartInfo);
		}

		protected virtual void CheckJourneyTimeFormatPart()
		{
			var journeyTimeFormatPartInfo = Parent.JourneyTimeFormatPartInfo;
			ListValidation.ErrorIfInvalidCode(journeyTimeFormatPartInfo);

			var journeyTimeFormatPart = Parent.JourneyTimeFormatPart;
			var journeyTimeNumericPart = Parent.JourneyTimeNumericPart;
			if (journeyTimeNumericPart <= 0)
			{
				journeyTimeFormatPartInfo.AddMessageError(Res.GetString("b942a7fc-4d44-4923-a59d-0afe0855f443", "Journey Time Must be greater than zero"));
			}
			else
			{
				if (journeyTimeFormatPart == JourneyTimeUnitList.Codes.Hours && journeyTimeNumericPart > 24)
				{
					journeyTimeFormatPartInfo.AddMessageError(Res.GetString("5a4d7c12-d822-4602-a8d9-36c346fa250b", "Journey Time Cannot exceed 24 Hours"));
				}
				else if (journeyTimeFormatPart == JourneyTimeUnitList.Codes.Days)
				{
					var transportMode = Parent.JE_TransportMode;
					var maxDays = GetMaximumJourneyDays(transportMode);
					if (journeyTimeNumericPart > maxDays)
					{
						journeyTimeFormatPartInfo.AddMessageError(Res.GetString("94725EBD-80DD-4A50-BEEB-8E0DE2439A3C"
							, "For Transport Mode '{0}' the maximum Journey Time is '{1}' Days"
							, transportMode
							, maxDays));
					}
				}
			}
		}

		ZInt GetMaximumJourneyDays(ZString transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Other:
				case Core.Constants.TransportModes.Sea:
					return 45;
				case Core.Constants.TransportModes.Rail:
				case Core.Constants.TransportModes.Road:
				case Core.Constants.TransportModes.InlandWaterwayTransport:
					return 35;
				case Core.Constants.TransportModes.Air:
					return 20;
				case Core.Constants.TransportModes.Mail:
					return 30;
				case Core.Constants.TransportModes.FixedTransportInstallations:
					return 15;
				default:
					return 45;
			}
		}

		protected override void CheckJE_MessageType()
		{
		}

		protected override void CheckJE_DeclarantType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_DeclarantTypeInfo);
		}

		protected override void CheckJE_MessageSubType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_MessageSubTypeInfo);

			var guarantorType = Parent.ZG_GuarantorType;
			var destinationType = Parent.JE_MessageSubType;
			if (ShouldValidateDestinationTypeMustBe1WhenGuarantorIs0
				&& guarantorType == EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements
				&& destinationType != EMCSDestinationTypeList.Codes.DestinationTaxWarehouse)
			{
				Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("540a8385-7059-447a-8cf1-c75f1c8aa82c"
					, "Destination Type must be 1 when Guarantor(s) is 0."));
			}

			if (destinationType == EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown
				&& guarantorType.Contains(EMCSGuarantorTypeList.Codes.Consignee, StringComparison.OrdinalIgnoreCase))
			{
				Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("3bac49f4-bcf3-4463-9bf9-ea4965c581b6"
					, "Destination Type should not be 8 when Guarantor(s) contains a 4."));
			}

			if (destinationType == EMCSDestinationTypeList.Codes.DestinationExport
				&& !Parent.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfDelivery))
			{
				Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("CD0067EC-B50A-4339-AF38-FB642332E41F"
					, "Customs Office ({0}) is required when Destination Type = 6 - {1}."
					, EuOfficeCodesTypes.Descriptions.OfficeOfDelivery, EMCSDestinationTypeList.Descriptions.DestinationExport));
			}

			switch (Parent.ZG_SubmissionType)
			{
				case EMCSSubmissionTypeList.Codes.StandardSubmission:
					if (destinationType != EMCSDestinationTypeList.Codes.DestinationTaxWarehouse
						&& destinationType != EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee
						&& destinationType != EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee
						&& destinationType != EMCSDestinationTypeList.Codes.DestinationDirectDelivery
						&& destinationType != EMCSDestinationTypeList.Codes.DestinationExemptedConsignee
						&& destinationType != EMCSDestinationTypeList.Codes.DestinationExport
						&& destinationType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown)
					{
						Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("b7ddfd84-e147-4b04-b58d-f27715ccba7c",
							"Destination Type can be {0},{1},{2},{3},{4},{5}, or {6} when Submission Type is {7}.",
							EMCSDestinationTypeList.Codes.DestinationTaxWarehouse,
							EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee,
							EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee,
							EMCSDestinationTypeList.Codes.DestinationDirectDelivery,
							EMCSDestinationTypeList.Codes.DestinationExemptedConsignee,
							EMCSDestinationTypeList.Codes.DestinationExport,
							EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown,
							EMCSSubmissionTypeList.Codes.StandardSubmission));
					}
					break;
				case EMCSSubmissionTypeList.Codes.SubmissionForExport:
					if (destinationType != EMCSDestinationTypeList.Codes.DestinationExport)
					{
						Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("00e44348-147a-4f7f-a3a3-99a3ec2c11ce",
							"Destination Type must be {0} when Submission Type is {1}.",
							EMCSDestinationTypeList.Codes.DestinationExport,
							EMCSSubmissionTypeList.Codes.SubmissionForExport));
					}
					break;
				case EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B:
					if (destinationType != EMCSDestinationTypeList.Codes.DestinationCertifiedConsignee
						&& destinationType != EMCSDestinationTypeList.Codes.DestinationTemporaryCertifiedConsignee)
					{
						Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("4fcc0cc0-6a80-4537-b3e7-10d18df202f2",
							"Destination Type must be either {0} or {1} when Submission Type is {2}.",
							EMCSDestinationTypeList.Codes.DestinationCertifiedConsignee,
							EMCSDestinationTypeList.Codes.DestinationTemporaryCertifiedConsignee,
							EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B));
					}
					break;
				default:
					break;
			}
		}

		protected virtual ZBool ShouldValidateDestinationTypeMustBe1WhenGuarantorIs0 => true;

		protected override void CheckJE_PaymentMethod()
		{
		}

		protected override void CheckJE_TransportMode()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TransportModeInfo);

			var codes = new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.FixedTransportInstallations };
			var transportMode = Parent.JE_TransportMode;
			if (Parent.ZG_GuarantorType == EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements && codes.All(c => c != transportMode))
			{
				Parent.JE_TransportModeInfo.AddMessageError(Res.GetString("bacecc2a-a764-45fa-8de1-17bb0e45a9b6",
					"Transport Mode must be {0} or {1} when Guarantor Type is 5.",
					TransportTypeList.Codes.Sea,
					TransportTypeList.Codes.FixedTransportInstallations));
			}

			codes = new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.InlandWaterwayTransport };
			if (Parent.JE_MessageSubType == EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown
				&& codes.All(c => c != transportMode))
			{
				Parent.JE_TransportModeInfo.AddMessageError(Res.GetString("3c7b7338-fc70-4ecd-aa12-77b8486e4193",
					"Transport Mode must be {0} or {1} when Destination Type is 8.",
					TransportTypeList.Codes.Sea,
					TransportTypeList.Codes.InlandWaterwayTransport));
			}

			if (!Parent.CusContainers.Any<EMCSCusContainer>())
			{
				Parent.JE_TransportModeInfo.AddMessageError(Res.GetString("61157784-e3a4-4740-98b3-92f4aa11972b", "Transport Details are missing."));
			}
		}

		protected override void CheckJE_EntryStatus()
		{
			base.CheckJE_EntryStatus();
			ListValidation.ErrorIfInvalidCode(Parent.JE_EntryStatusInfo);
		}

		protected override void CheckJE_DateAtOrigin()
		{
			var parent = Parent;
			var dispatchTime = parent.JE_DateAtOrigin;
			var deferredSubmission = parent.ZG_DeferredSubmission;
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_DateAtOriginInfo);
			if (!dispatchTime.IsEmpty && parent.JE_DeclarantType == EMCSEntryTypeList.Codes.Consignor && parent.JE_EntryStatus.IsEmpty)
			{
				if (deferredSubmission == EMCSDeferredSubmissionList.Codes.No)
				{
					if (dispatchTime.IsInThePastDatePartOnly)
					{
						parent.JE_DateAtOriginInfo.AddMessageError(Res.GetString("bb73bb87-56bf-4f12-aee6-a0a89ebb0aa2", "Dispatch Time Cannot be earlier than Today"));
					}
					else if (dispatchTime > ZDateTime.Today.AddDays(7))
					{
						parent.JE_DateAtOriginInfo.AddMessageError(Res.GetString("3fb77e6a-feff-4d07-9b14-beb17c4150a0", "Dispatch Time Cannot be later than 7 Days from Today"));
					}
				}
				else if (IsDeferredSubmission(deferredSubmission) && dispatchTime.IsInTheFutureDatePartOnly)
				{
					parent.JE_DateAtOriginInfo.AddMessageError(Res.GetString("3733917a-0325-4a25-8140-f89b4b17e558", "Dispatch Time Cannot be later than Today with Deferred Submission."));
				}
			}
		}

		protected virtual bool IsDeferredSubmission(ZString deferredSubmission) => deferredSubmission == EMCSDeferredSubmissionList.Codes.Yes;

		#region CheckJE_OH_Supplier

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_SupplierInfo);

			if (Parent.JE_MessageSubType != EMCSDestinationTypeList.Codes.DestinationDirectDelivery
				&& Parent.ZG_GuarantorType.Contains(EMCSGuarantorTypeList.Codes.Consignor, StringComparison.OrdinalIgnoreCase)
				&& !HasCusCode(Parent.Supplier, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, true))
			{
				Parent.JE_OH_SupplierInfo.AddMessageError(Res.GetString("94A3C46E-C781-4B48-8F8D-8D498F757079",
					"{0} does not specify the Trader Excise Number when Guarantor(s) contains a 1 and Destination Type is not 4, please see Organization -> Details -> Config -> Registration Numbers / Codes.",
					Parent.JE_OH_SupplierInfo.HumanReadableName));
			}
		}

		#endregion

		#region CheckJE_OH_Importer

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			ValidateConsigneeWithDestinationType();
			ValidateConsigneeWithGuarantorType();
		}

		void ValidateConsigneeWithDestinationType()
		{
			switch (Parent.JE_MessageSubType)
			{
				case EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown:
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_OH_ImporterInfo);
						break;
					}

				case EMCSDestinationTypeList.Codes.DestinationExemptedConsignee:
					{
						ValidateCusCodeWithDestinationType(OrgCusCode.EuropeanUnionSharedCodeTypes.ConsigneeExemptNo, Res.GetString("00edeee4-f790-45c0-ad3e-d5dadb93fc27", "Consignee Exempt Number"));
						break;
					}

				case EMCSDestinationTypeList.Codes.DestinationExport:
					{
						ValidateCusCodeWithDestinationType(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Res.GetString("4bf4c6ce-69e5-4303-9ea9-5aca9e3191d2", "EORI Code"), true);
						break;
					}
			}
		}

		void ValidateConsigneeWithGuarantorType()
		{
			if (Parent.ZG_GuarantorType.Contains(EMCSGuarantorTypeList.Codes.Consignee, StringComparison.OrdinalIgnoreCase))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo);

				if (Parent.JE_MessageSubType != EMCSDestinationTypeList.Codes.DestinationDirectDelivery)
				{
					var consignee = Parent.Consignee;
					var code = consignee.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);

					if (!Regex.IsMatch(code, @"^[a-zA-Z]{2}[a-zA-Z0-9]{11}$"))
					{
						Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("474849CA-4EC4-4D85-8567-E64C51430A80",
							"{0} must have a Trader Excise Number starts with 2 alpha characters and 11 alphanumeric when Guarantor(s) contains a 4 and Destination Type is not 4, please see Organization -> Details -> Config -> Registration Numbers / Codes.",
							Parent.JE_OH_ImporterInfo.HumanReadableName));
					}
				}
			}
		}

		void ValidateCusCodeWithDestinationType(string code, string description, bool ignoreCountry = false)
		{
			var consignee = Parent.Factory.Load<OrgHeader>(Parent.JE_OH_Importer);
			if (consignee == null || !HasCusCode(consignee, code, ignoreCountry))
			{
				Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("E54462B4-F747-469B-8315-63E4DE85A3CA",
					"{0} does not specify the {1} when Destination Type is {2}, please see Organization -> Details -> Config -> Registration Numbers / Codes.",
					base.Parent.JE_OH_ImporterInfo.HumanReadableName,
					description,
					Parent.JE_MessageSubType));
			}
		}

		bool HasCusCode(OrgHeader header, string code, bool ignoreCountry = false)
		{
			return header != null && header.CustomsCodes.GetOrgCusCode(code, ignoreCountry ? null : GlbCompany.CurrentCompany.Country) != null;
		}

		#endregion

		protected override void CheckJE_OwnerRef()
		{
			base.CheckJE_OwnerRef();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OwnerRefInfo);

			var declarationsWithSameLocRefNumber = new EMCSJobDeclarationCollection(Parent.Factory, GlbCompany.CurrentCompany.PK);
			var query = new ZQuery(JobDeclarationSchema.JE_OwnerRef, Parent.JE_OwnerRef);
			query.AddToFilter(JobDeclarationSchema.JE_DeclarantType, Parent.JE_DeclarantType);
			query.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, Parent.JE_OH_Supplier);
			query.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			declarationsWithSameLocRefNumber.Load(query);
			if (declarationsWithSameLocRefNumber.Any())
			{
				Parent.JE_OwnerRefInfo.AddError(Res.GetString("B92C6FCE-0FAB-4BE3-A243-C19D640DBB03", "Another Declaration with this Reference Number and Declaration Type is already existing. Please use another Local Reference Number."));
			}
		}

		protected override void CheckJE_ScreeningStatus()
		{
		}

		void ValidatePackages()
		{
			if (Parent.EMCSPackages.Count == 0)
			{
				Parent.AddRowMessageError(Res.GetString("46469852-74e3-4db3-9683-b73133269ac0", "At least 1 package is required."));
			}
		}
	}
}
