using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class LEXJobDeclarationValidation : JobDeclarationValidation
	{
		public LEXJobDeclarationValidation(JobDeclaration declaration) : base(declaration)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();
			Parent.StevedoreCompany.MarkParentAsNeedingValidation = true;
			ValidateStevedoreCompanyAddress();
		}

		protected override void CheckJE_MessageSubType()
		{
			MandatoryValidation.CheckEntered(Parent.JE_MessageSubTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JE_MessageSubTypeInfo);
			CheckTransportMeans();
			CheckStevedores();
		}

		void CheckTransportMeans()
		{
			if (Parent.IsLocalExportToSeaVessel)
			{
				if (Parent.TransportMeans.Count == 0)
				{
					Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("0D9386D2-D937-4AC1-8686-ADD5A21CBAEF", "If Declaration Type is '07',’09’ or '17', ‘Other Transport Mean’ must be entered."));
				}
			}
			else
			{
				if (Parent.TransportMeans.Count > 0)
				{
					Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("D5D619AF-DDC8-4003-BEF7-443444FA48B6", "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered."));
				}
			}
		}

		void CheckStevedores()
		{
			if (Parent.IsLocalExportToSeaVessel)
			{
				if (Parent.Persons.Count == 0)
				{
					Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("102A7936-2E68-47FC-AF4A-89E553385C5D", "Stevedores must be entered if Declaration Type is '07', '09' or '17'."));
				}
			}
			else
			{
				if (Parent.Persons.Count > 0)
				{
					Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("67788899-1577-4290-931B-F5E609F6CADF", "Stevedores must not be entered if Declaration Type is not '07', '09' or '17'."));
				}
			}
		}

		protected override void CheckJE_SubLocationOfGoods()
		{
			base.CheckJE_SubLocationOfGoods();
			if (LocalExportTransactionNatureCodeList.Is5DP(Parent.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SubLocationOfGoodsInfo);
			}
		}

		protected override void CheckJE_LocationOtherInformation()
		{
			base.CheckJE_LocationOtherInformation();
			if (LocalExportTransactionNatureCodeList.Is5DQ(Parent.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_LocationOtherInformationInfo);
			}
			else if (LocalExportTransactionNatureCodeList.Is5DP(Parent.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOtherInformationInfo);
				if (!Parent.JE_LocationOtherInformation.IsEmpty && Parent.JE_LocationOtherInformation.Substring(0, 3) != Parent.JE_CustomsOffice)
				{
					Parent.JE_LocationOtherInformationInfo.AddMessageError(Res.GetString("C635F25D-A286-432D-91E3-D9702AC6C703", "Bonded Area Code's Customs Office and Declaration Customs Office must be the same."));
				}
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOtherInformationInfo);
		}

		protected override void CheckJE_EntryDate()
		{
			base.CheckJE_EntryDate();
			if (LocalExportTransactionNatureCodeList.Is5DP(Parent.JE_MessageSubType))
			{
				if (Parent.JE_IsBlanketDeclaration == "Y")
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_EntryDateInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_EntryDateInfo);
				}
			}
			else if (Parent.JE_MessageSubType == LocalExportTransactionNatureCodeList.Codes._08)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_EntryDateInfo);
			}
		}

		protected override void CheckJE_ExportGoodsType()
		{
			base.CheckJE_ExportGoodsType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ExportGoodsTypeInfo);
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (Parent.IsLocalExportToSeaVessel)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_VesselNameInfo);
				var vessel = Parent.Vessel;
				if (vessel != null && vessel.RV_RadioCallSign.IsEmpty)
				{
					Parent.JE_VesselNameInfo.AddMessageError(Res.GetString("06579DBB-5A7F-49EC-BDCF-C297D3E3AC39", "Radio call sign must be entered on the selected vessel."));
				}
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (Parent.IsLocalExportToAirplane)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo);
			}
		}

		protected override void CheckMRNJ3_ReferenceNumber()
		{
			if (Parent.IsLocalExportToSeaVessel)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.MRNJ3_ReferenceNumberInfo);
			}
		}

		public override void ValidateStevedoreCompanyAddress()
		{
			ValidateCalculatedProperty(Parent.StevedoreCompanyAddressInfo);
		}

		protected void CheckStevedoreCompanyAddress()
		{
			if (Parent.IsLocalExportToSeaVessel)
			{
				if (Parent.Persons.Count > 0 && Parent.StevedoreCompanyAddress.IsEmpty)
				{
					Parent.StevedoreCompanyAddressInfo.AddMessageError(Res.GetString("A99186CB-D7AB-4F4D-9ABC-8E846C9BB9DA", "Stevedore must be entered."));
					Parent.MarkAsNeedingValidation();
				}
			}
		}

		protected override void CheckJE_OH_Exporter()
		{
			base.CheckJE_OH_Exporter();
			if (LocalExportTransactionNatureCodeList.Is5DP(Parent.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_OH_ExporterInfo);
			}
			else if (LocalExportTransactionNatureCodeList.Is5DQ(Parent.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ExporterInfo);
				if (Parent.Exporter != null)
				{
					var businessRegNo = Parent.Exporter.GetRegistrationNumber(Constants.IdentificationType.BusinessRegNo);
					if (businessRegNo.IsEmpty)
					{
						Parent.JE_OH_ExporterInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Business Registration Number", Constants.IdentificationType.BusinessRegNo));
					}
				}
			}
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_SupplierInfo);
			if (Parent.Supplier != null)
			{
				if (Parent.Supplier.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
				{
					Parent.JE_OH_SupplierInfo.AddMessageError(GetErrorMessageAboutToEnteredIndividualOrganization((NoResString)"Supplier"));
				}
				else
				{
					if (Parent.Supplier.GetRegistrationNumber(IdentificationType.BusinessRegNo).IsEmpty)
					{
						Parent.JE_OH_SupplierInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Business Registration Number", Constants.IdentificationType.BusinessRegNo));
					}
					if (Parent.Supplier.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization).IsEmpty)
					{
						Parent.JE_OH_SupplierInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Unipass ID", Constants.IdentificationType.UnipassIDForOrganization));
					}
				}
			}
		}

		public static string GetErrorMessageAboutToEnteredIndividualOrganization(string organization)
		{
			return Res.GetString("F908D1DA-83A8-4286-9CF5-10D6F6D817ED", "The {0} must not be an individual.", organization);
		}

		protected override void CheckJE_CustomsDivision()
		{
			base.CheckJE_CustomsDivision();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_CustomsDivisionInfo);
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_CustomsDivisionInfo);
		}

		protected override void CheckJE_NoOfCrew()
		{
			base.CheckJE_NoOfCrew();
			if (LocalExportTransactionNatureCodeList.IsSea(Declaration.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_NoOfCrewInfo);
			}
		}

		protected override void CheckJE_VoyageDuration()
		{
			base.CheckJE_VoyageDuration();
			if (LocalExportTransactionNatureCodeList.IsSea(Declaration.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_VoyageDurationInfo);
			}
		}

		protected override void CheckJE_MRNType()
		{
			base.CheckJE_MRNType();
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_MRNTypeInfo);
		}

		protected override bool IsJE_ContainerPackModeMandatory => false;
	}
}
