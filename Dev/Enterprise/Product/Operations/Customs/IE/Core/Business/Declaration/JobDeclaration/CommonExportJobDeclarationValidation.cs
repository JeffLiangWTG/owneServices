using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class CommonExportJobDeclarationValidation : JobDeclarationValidation
	{
		protected CommonExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			NoAmendCheckForOffices();
		}

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();

			var parent = Parent;
			var targetInfo = parent.JE_ContainerModeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (parent.ContainersRequired && parent.CusContainers.Count == 0)
			{
				targetInfo.AddMessageError(Res.GetString("A68FC4C1-1668-4223-A11A-82D6659B3402", "At least one Container must be entered."));
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();

			var parent = Parent;
			if (parent.JE_TransportMode == TransportTypeList.Codes.Air && parent.ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._40)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_VoyageFlightNoInfo);
			}
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			if (Parent.CustomsEntryInstructions.Any(instruction => instruction.IsCentralisedClearance) && Parent.CustomsOffices.GetPresentationOffice() == null)
			{
				Parent.JE_CustomsOfficeInfo.AddMessageError(Res.GetString("819126A7-465E-4CAB-89C7-441F51571868", "Customs office of Presentation is required for centralized clearance (denoted via Authorization Type CCL)."));
			}
		}

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();
			var targetInfo = Parent.JE_OA_RepresentativeInfo;
			if (Parent.JE_OA_Representative.IsValid)
			{
				CheckNoAmendingOnEntryStatus(Parent.OriginalRepresentative, targetInfo);
				var representativeId = Parent.Representative.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true);
				if (representativeId.IsEmpty)
				{
					targetInfo.AddMessageError(GetEORINumberIsRequiredMessage(targetInfo.HumanReadableName));
				}
				else
				{
					var declarantId = Parent.Declarant.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true);
					if (!declarantId.IsEmpty && representativeId == declarantId && Parent.JE_DeclarantType != RepresentationTypeList.Codes._2Direct)
					{
						targetInfo.AddMessageError(Res.GetString("54E3523F-263F-4D2B-896C-EBBEA80C38E8", "The EORI of the {0} must be different to the {1}", targetInfo.HumanReadableName, Parent.JE_OA_DeclarantAddressInfo.HumanReadableName));
					}
				}
			}
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			if (Parent.JE_OA_DeclarantAddress.IsValid)
			{
				var targetInfo = Parent.JE_OA_DeclarantAddressInfo;
				CheckNoAmendingOnEntryStatus(Parent.OriginalDeclarant, targetInfo);
				var declarantId = Parent.Declarant.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true);
				if (declarantId.IsEmpty)
				{
					targetInfo.AddMessageError(GetEORINumberIsRequiredMessage(targetInfo.HumanReadableName));
				}
				else if (Parent.Company is GlbCompany company && GlbCompanyWrapper.Get(company) is IIEGlbCompanyWrapper wrapper && wrapper.GetGlbExternalPassword() is GlbCompanyCredential companyCredential)
				{
					var messageSenderEORI = companyCredential.GP_MailBoxID;
					if (declarantId != messageSenderEORI)
					{
						targetInfo.AddMessageError(Res.GetString("{EDF645EB-2487-4C7D-8361-F93BF79E35DD}", "Declarant's EORI '{0}' is different to Company ({1})'s Message Sender EORI '{2}'", declarantId, company.CompanyName, messageSenderEORI));
					}
				}
			}
		}

		static string GetEORINumberIsRequiredMessage(string identifier) => Res.GetString("337EEA02-F305-4F4E-A860-4F0C01C90CF1", "An EORI number is required for {0}", identifier);

		protected void CheckCarrierHasEORI()
		{
			var carrierId = Parent.ShippingLine.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true);
			if (carrierId.IsEmpty)
			{
				var targetInfo = Parent.JE_OH_ShippingLineInfo;
				targetInfo.AddMessageError(GetEORINumberIsRequiredMessage(targetInfo.Description));
			}
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			// no need to check as we already checked that it's export
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			var officeOfExit = Parent.CustomsOffices.GetOfficeOfExit()?.CY_Data;
			if (!officeOfExit.HasValue)
			{
				Parent.JE_MessageTypeInfo.AddMessageError(Res.GetString("AF10CCAD-13BE-485D-BDBF-435EE9F34DDC", "At least one office of type {0} is required in the Customs Offices grid.", EuOfficeCodesTypes.Codes.OfficeOfExit));
			}
		}

		protected override void CheckJE_AircraftRegistration()
		{
			base.CheckJE_AircraftRegistration();

			var parent = Parent;
			if (parent.JE_TransportMode == TransportTypeList.Codes.Air && parent.ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._41)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_AircraftRegistrationInfo);
			}
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			EU.Business.Declaration.JobDeclarationUCC6ExportValidation.CheckJE_VesselName(Parent);
		}

		protected override void CheckJE_LloydsIMO()
		{
			base.CheckJE_LloydsIMO();

			if (Parent.ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._10)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LloydsIMOInfo);
			}
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			if (Parent.JE_EntryStyle == EntryStyleListExport.Codes.ExportNormal)
			{
				var target = Parent.JE_RL_NKPortOfArrivalInfo;
				if (Parent.JE_RL_NKPortOfArrival.IsEmpty)
				{
					target.AddMessageError(Res.GetString("1A7EB75E-5F28-4517-AE62-66F1AA4E2EA3", "Values are required in the Country of Routing of Consignment (Itinerary). Port of Discharge is automatically added to the list of countries specified in the itinerary. Additional countries should be entered on the Misc. Tab in the Itinerary Countries Grid."));
				}
			}
			base.CheckJE_RL_NKPortOfArrival();
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			if (Parent.JE_EntryStyle == EntryStyleListExport.Codes.ExportNormal)
			{
				var target = Parent.JE_OH_ShippingLineInfo;
				if (Parent.JE_OH_ShippingLine.IsEmpty)
				{
					target.AddMessageError(MandatoryValidation.MustBeEnteredMessage(target.Description));
				}
				else
				{
					var carrier = Parent.Factory.Load<OrgHeader>(Parent.JE_OH_ShippingLine);
					if (carrier.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, true).IsEmpty)
					{
						target.AddMessageError(Res.GetString("8F488268-4573-4D08-A329-E2C79F304AA8", "Carrier must have an EORI."));
					}
				}
			}
			base.CheckJE_OH_ShippingLine();
		}

		#region Inland Transport(ID, typeofID, Nationalities)

		protected override void CheckJE_TransportIDInland()
		{
			var parent = Parent;
			var info = parent.JE_TransportIDInlandInfo;
			if (Parent.IsRoadInland)
			{
				CheckTransportIDInlandForRoad(info);
			}
			else
			{
				AddErrorDependentOnOtherPropertyEntered(info, parent.JE_TransportMeansInfo);
			}
		}

		protected override void CheckJE_RN_NKTransportNationalityInland()
		{
			base.CheckJE_RN_NKTransportNationalityInland();
			var parent = Parent;
			var info = parent.JE_RN_NKTransportNationalityInlandInfo;
			var dependingOnPropertyInfo = parent.JE_TransportIDInlandInfo;
			AddErrorDependentOnOtherPropertyEntered(info, dependingOnPropertyInfo);
		}

		protected override void CheckJE_Trailer1RegNo()
		{
			base.CheckJE_Trailer1RegNo();
			if (Parent.IsRoadInland)
			{
				CheckTransportIDInlandForRoad(Parent.JE_Trailer1RegNoInfo);
			}
		}
		protected override void CheckJE_RN_NKTrailer1Nationality()
		{
			base.CheckJE_RN_NKTrailer1Nationality();
			if (Parent.IsRoadInland)
			{
				AddErrorDependentOnOtherPropertyEntered(Parent.JE_RN_NKTrailer1NationalityInfo, Parent.JE_Trailer1RegNoInfo);
			}
		}

		protected override void CheckJE_Trailer2RegNo()
		{
			base.CheckJE_Trailer2RegNo();
			if (Parent.IsRoadInland)
			{
				CheckTransportIDInlandForRoad(Parent.JE_Trailer2RegNoInfo);
			}
		}

		protected override void CheckJE_RN_NKTrailer2Nationality()
		{
			base.CheckJE_RN_NKTrailer2Nationality();
			if (Parent.IsRoadInland)
			{
				AddErrorDependentOnOtherPropertyEntered(Parent.JE_RN_NKTrailer2NationalityInfo, Parent.JE_Trailer2RegNoInfo);
			}
		}

		void AddErrorDependentOnOtherPropertyEntered(ZPropertyInfo info, ZPropertyInfo dependingOnPropertyInfo)
		{
			if (info.Value.IsEmpty && !dependingOnPropertyInfo.Value.IsEmpty)
			{
				info.AddMessageError(Res.GetString("29E5C339-6963-42CA-BACD-DEF3B698214B", "{0} is required when {1} is entered.", info.HumanReadableName, dependingOnPropertyInfo.HumanReadableName));
			}
			else if (!info.Value.IsEmpty && dependingOnPropertyInfo.Value.IsEmpty)
			{
				info.AddMessageError(Res.GetString("91FD31C7-B29A-4405-9ECE-F1F712C1B6CA", "Please do not enter {0} when {1} does not have a value", info.HumanReadableName, dependingOnPropertyInfo.HumanReadableName));
			}
		}

		protected void CheckTransportIDInlandForRoad(ZPropertyInfo targetInfo)
		{
			var transportMeansInfo = Parent.JE_TransportMeansInfo;
			var transportIDInlandInfo = Parent.JE_TransportIDInlandInfo;
			var trailer1RegNoInfo = Parent.JE_Trailer1RegNoInfo;
			var trailer2RegNoInfo = Parent.JE_Trailer2RegNoInfo;
			if (transportMeansInfo.Value.IsEmpty && !targetInfo.Value.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("74979137-AC91-45F3-9D61-6197E7E86899", "Please do not enter {0} when {1} does not have a value", targetInfo.HumanReadableName, transportMeansInfo.HumanReadableName));
			}
			else if (!transportMeansInfo.Value.IsEmpty && transportIDInlandInfo.Value.IsEmpty && trailer1RegNoInfo.Value.IsEmpty && trailer2RegNoInfo.Value.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("55B65CAE-511A-443F-BE84-3E27B12EE4D8", "At least one of {0} and the Trailer IDs is required.", transportIDInlandInfo.HumanReadableName));
			}
		}

		#endregion

		#region No-Amend check

		void NoAmendCheckForOffices()
		{
			Parent.RemoveRowMessageError(OfficeShouldNotBeDeletedOrChangedMesssage, containing: true);

			if (Parent.IsAmendmentValidationMode)
			{
				var messageBuilder = new ZStringBuilder();
				var sentExitOffice = Parent.OriginalExitOffice;
				if (!sentExitOffice.IsEmpty && sentExitOffice != Parent.OfficeOfExitCustomsOffice)
				{
					messageBuilder.Append(EuOfficeCodesTypes.Codes.OfficeOfExit + "-" + sentExitOffice);
				}

				var sentPresentationOffice = Parent.OriginalPresentationOffice;
				if (!sentPresentationOffice.IsEmpty && sentPresentationOffice != Parent.PresentationCustomsOffice)
				{
					messageBuilder.Append(EuOfficeCodesTypes.Codes.OfficeOfPresentation + "-" + sentPresentationOffice);
				}

				var sentSupervisingOffice = Parent.OriginalSupervisingOffice;
				if (!sentSupervisingOffice.IsEmpty && sentSupervisingOffice != Parent.SupervisingCustomsOffice)
				{
					messageBuilder.Append(EuOfficeCodesTypes.Codes.SupervisingOffice + "-" + sentSupervisingOffice);
				}

				if (!messageBuilder.IsEmpty)
				{
					Parent.AddRowMessageError(Res.GetString(
						"8CDD60BC-A70C-43EC-8D6C-53C5010AB511",
						"Customs Offices: {0}: {1}.",
						messageBuilder.ToStringWithDelimiterBetweenAppends(","),
						OfficeShouldNotBeDeletedOrChangedMesssage
					));
				}
			}
		}

		string OfficeShouldNotBeDeletedOrChangedMesssage => Res.GetString("2B5532D7-B600-4E04-B8A8-857526B310AD",
			"These Customs Office(s) have been declared to Customs with the Declaration and therefore may not be deleted or changed"
		);

		#endregion
	}
}
