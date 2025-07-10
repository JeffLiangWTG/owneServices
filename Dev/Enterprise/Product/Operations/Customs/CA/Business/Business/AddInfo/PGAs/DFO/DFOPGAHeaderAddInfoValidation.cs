//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDFOPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoDFOPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class DFOPGAHeaderAddInfoValidation : AutoDFOPGAHeaderAddInfoValidation
	{
		public DFOPGAHeaderAddInfoValidation(AutoDFOPGAHeaderAddInfo parent) : base(parent)
		{
		}

		DFOPGAHeader PGAHeader => Parent?.Parent as DFOPGAHeader;

		bool IsParentGeneticModification => Parent.CA_ABIProgramInd == YesNoList.Codes.Yes && Parent.CA_HasGeneticModification;

		protected override void CheckCA_GeneticModificationDescription()
		{
			base.CheckCA_GeneticModificationDescription();
			MandatoryValidationOnABIProgram(Parent.CA_GeneticModificationDescriptionInfo);
		}

		protected override void CheckCA_GenusOrSpecies()
		{
			base.CheckCA_GenusOrSpecies();
			MandatoryValidationOnABIProgram(Parent.CA_GenusOrSpeciesInfo);
			MandatoryValidationOnAISProgram(Parent.CA_GenusOrSpeciesInfo);
			MandatoryValidationOTTPProgram(Parent.CA_GenusOrSpeciesInfo);
		}

		protected override void CheckCA_Direction()
		{
			base.CheckCA_Direction();

			if (PGAHeader != null && PGAHeader.HasAsianCarpSpeciesOrQuaggaOrZebraMussels)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_DirectionInfo, Parent.Lookups.DirectionList);
			}
		}

		void MandatoryValidationOnABIProgram(ZPropertyInfo info)
		{
			if (Parent.CA_ABIProgramInd == YesNoList.Codes.Yes && Parent.CA_HasGeneticModification)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		void MandatoryValidationOnAISProgram(ZPropertyInfo info)
		{
			if (Parent.CA_AISProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		void MandatoryValidationOTTPProgram(ZPropertyInfo info)
		{
			if (Parent.CA_TTPProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		protected override void CheckCA_Category()
		{
			base.CheckCA_Category();

			if (IsParentGeneticModification)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryInfo, Parent.Lookups.CategoryList);
			}
		}

		protected override void CheckCA_Count()
		{
			base.CheckCA_Count();

			if (IsParentGeneticModification)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CA_CountInfo);
			}
		}

		protected override void CheckCA_SpeciesCode()
		{
			base.CheckCA_SpeciesCode();

			if (!Parent.CA_SpeciesCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_SpeciesCodeInfo, Parent.Lookups.ScientificNames);
			}
		}

		protected override void CheckCA_Commission()
		{
			base.CheckCA_Commission();

			if (PGAHeader.CA_TTPProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_CommissionInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CA_CommissionInfo, Parent.Lookups.CommissionList);
		}

		protected override void CheckCA_CommonNameCode()
		{
			base.CheckCA_CommonNameCode();

			if (!Parent.CA_CommonNameCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_CommonNameCodeInfo, Parent.Lookups.CommonNameCodes);
			}
		}

		#region Life Stage

		protected override void CheckCA_LifeStagePropagate()
		{
			base.CheckCA_LifeStagePropagate();

			ValidateLifeStageOnABIProgram(Parent.CA_LifeStagePropagateInfo);
			ValidateLifeStageOnAISProgram(Parent.CA_LifeStagePropagateInfo);
			ValidateLifeStageOnTTPProgram(Parent.CA_LifeStagePropagateInfo);
		}

		protected override void CheckCA_LifeStageAdult()
		{
			base.CheckCA_LifeStageAdult();

			ValidateLifeStageOnABIProgram(Parent.CA_LifeStageAdultInfo);
			ValidateLifeStageOnAISProgram(Parent.CA_LifeStageAdultInfo);
			ValidateLifeStageOnTTPProgram(Parent.CA_LifeStageAdultInfo);
		}

		protected override void CheckCA_LifeStageDead()
		{
			base.CheckCA_LifeStageDead();

			ValidateLifeStageOnAISProgram(Parent.CA_LifeStageDeadInfo);
			ValidateLifeStageOnTTPProgram(Parent.CA_LifeStageDeadInfo);
		}

		protected override void CheckCA_LifeStageEmbryo()
		{
			base.CheckCA_LifeStageEmbryo();

			ValidateLifeStageOnABIProgram(Parent.CA_LifeStageEmbryoInfo);
			ValidateLifeStageOnAISProgram(Parent.CA_LifeStageEmbryoInfo);
			ValidateLifeStageOnTTPProgram(Parent.CA_LifeStageEmbryoInfo);
		}

		protected override void CheckCA_LifeStageJuvenile()
		{
			base.CheckCA_LifeStageJuvenile();

			ValidateLifeStageOnABIProgram(Parent.CA_LifeStageJuvenileInfo);
			ValidateLifeStageOnAISProgram(Parent.CA_LifeStageJuvenileInfo);
			ValidateLifeStageOnTTPProgram(Parent.CA_LifeStageJuvenileInfo);
		}

		protected override void CheckCA_LifeStageLive()
		{
			base.CheckCA_LifeStageLive();

			ValidateLifeStageOnAISProgram(Parent.CA_LifeStageLiveInfo);
		}

		void ValidateLifeStageOnABIProgram(ZPropertyInfo info)
		{
			if (IsParentGeneticModification)
			{
				var hasOneOption = Parent.CA_LifeStagePropagate
					|| Parent.CA_LifeStageEmbryo
					|| Parent.CA_LifeStageJuvenile
					|| Parent.CA_LifeStageAdult;

				if (!hasOneOption)
				{
					var message = Res.GetString("5e37c41c-8ef7-49bb-8a06-74cb25293cf3",
						@"If the commodity being imported has been genetically modified or genetically engineered, the life stage must be provided.");

					info.AddMessageError(message);
				}
			}
		}

		void ValidateLifeStageOnAISProgram(ZPropertyInfo info)
		{
			if (PGAHeader != null && PGAHeader.HasAsianCarpSpeciesOrQuaggaOrZebraMussels)
			{
				var hasOneOption = Parent.CA_LifeStagePropagate
					|| Parent.CA_LifeStageEmbryo
					|| Parent.CA_LifeStageJuvenile
					|| Parent.CA_LifeStageAdult
					|| Parent.CA_LifeStageLive
					|| Parent.CA_LifeStageDead;

				if (!hasOneOption)
				{
					var message = Res.GetString("ba2cf335-893b-4c7b-9821-7d688c7d03ea",
						@"The life stage must be provided in this field if the commodity being imported is non-eviscerated Asian Carp, Zebra or Quagga Mussels as prohibited under the AIS Regulations.");

					info.AddMessageError(message);
				}
			}
		}

		void ValidateLifeStageOnTTPProgram(ZPropertyInfo info)
		{
			if (Parent.CA_TTPProgramInd == YesNoList.Codes.Yes)
			{
				var hasOneOption = Parent.CA_LifeStagePropagate
					|| Parent.CA_LifeStageEmbryo
					|| Parent.CA_LifeStageJuvenile
					|| Parent.CA_LifeStageAdult
					|| Parent.CA_LifeStageDead;

				if (!hasOneOption)
				{
					var message = Res.GetString("815aeaf2-146f-40f8-bcda-2632a2a78611",
						@"The life stage of the commodity being imported must be provided for each commodity line with one of the life stage codes.");

					info.AddMessageError(message);
				}
			}
		}

		#endregion

		#region Sex

		protected override void CheckCA_SexFemale()
		{
			base.CheckCA_SexFemale();
			ValidateSex(Parent.CA_SexFemaleInfo);
		}

		protected override void CheckCA_SexHermaphrodite()
		{
			base.CheckCA_SexHermaphrodite();
			ValidateSex(Parent.CA_SexHermaphroditeInfo);
		}

		protected override void CheckCA_SexMale()
		{
			base.CheckCA_SexMale();
			ValidateSex(Parent.CA_SexMaleInfo);
		}

		protected override void CheckCA_SexOther()
		{
			base.CheckCA_SexOther();
			ValidateSex(Parent.CA_SexOtherInfo);
		}

		protected override void CheckCA_SexSterile()
		{
			base.CheckCA_SexSterile();
			ValidateSex(Parent.CA_SexSterileInfo);
		}

		void ValidateSex(ZPropertyInfo info)
		{
			var hasOneOption = Parent.CA_SexMale
				|| Parent.CA_SexFemale
				|| Parent.CA_SexOther
				|| Parent.CA_SexSterile
				|| Parent.CA_SexHermaphrodite;

			if (!hasOneOption)
			{
				if (IsParentGeneticModification)
				{
					var message = Res.GetString("31e5f970-6c15-4c17-8ba4-1104ccf25154",
						@"If the commodity being imported has been genetically modified or genetically engineered, the sex of the commodity must be provided.");

					info.AddMessageError(message);
				}

				if (PGAHeader != null && PGAHeader.HasAsianCarpSpeciesOrQuaggaOrZebraMussels)
				{
					var message = Res.GetString("0a0400d8-137f-4bf6-88c9-5477153f3689",
						@"The sex of the commodity must be provided in this field if the commodity being imported is non-eviscerated Asian Carp, Zebra or Quagga Mussels as prohibited under the AIS Regulations.");

					info.AddMessageError(message);
				}
			}
		}

		#endregion

		#region Intended Use

		protected override void CheckCA_IUF()
		{
			base.CheckCA_IUF();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IUFInfo);
		}

		protected override void CheckCA_IUA()
		{
			base.CheckCA_IUA();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IUAInfo);
		}

		protected override void CheckCA_IURAD()
		{
			base.CheckCA_IURAD();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IURADInfo);
		}

		protected override void CheckCA_IUSCP()
		{
			base.CheckCA_IUSCP();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IUSCPInfo);
		}

		protected override void CheckCA_IUEA()
		{
			base.CheckCA_IUEA();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IUEAInfo);
		}

		protected override void CheckCA_IUO()
		{
			base.CheckCA_IUO();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IUOInfo);
		}

		protected override void CheckCA_IUE()
		{
			base.CheckCA_IUE();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IUEInfo);
			ValidateIntendedUseCodeOnAISProgram(Parent.CA_IUEInfo);
		}

		protected override void CheckCA_IUOTH()
		{
			base.CheckCA_IUOTH();
			ValidateIntendedUseCodeOnABIProgram(Parent.CA_IUOTHInfo);
		}

		protected override void CheckCA_IUS()
		{
			base.CheckCA_IUS();
			ValidateIntendedUseCodeOnAISProgram(Parent.CA_IUSInfo);
		}

		protected override void CheckCA_IUAIS()
		{
			base.CheckCA_IUAIS();
			ValidateIntendedUseCodeOnAISProgram(Parent.CA_IUAISInfo);
		}

		void ValidateIntendedUseCodeOnABIProgram(ZPropertyInfo info)
		{
			if (IsParentGeneticModification)
			{
				var hasOneOption = Parent.CA_IUF
					|| Parent.CA_IUA
					|| Parent.CA_IURAD
					|| Parent.CA_IUSCP
					|| Parent.CA_IUEA
					|| Parent.CA_IUO
					|| Parent.CA_IUE
					|| Parent.CA_IUOTH;

				if (!hasOneOption)
				{
					var message = Res.GetString("7cf3a5c9-a0e1-4207-80a4-c31198efcce2",
						@"If the commodity being imported has been genetically modified or genetically engineered, the coded description of how a product will be used within Canada must be provided.");

					info.AddMessageError(message);
				}
			}
		}

		void ValidateIntendedUseCodeOnAISProgram(ZPropertyInfo info)
		{
			if (PGAHeader != null && PGAHeader.HasAsianCarpSpeciesOrQuaggaOrZebraMussels)
			{
				var hasOneOption = Parent.CA_IUS
					|| Parent.CA_IUAIS
					|| Parent.CA_IUE;

				if (!hasOneOption)
				{
					var message = Res.GetString("0d69dead-4150-4fef-bb62-414ac8242d48",
						@"If the declaration includes goods prohibited for importation under the Aquatic Invasive Species Regulations, then one of the Intended End Use codes must be provided indicating the purpose of the importation.");

					info.AddMessageError(message);
				}
			}
		}

		#endregion

		#region CheckCA_OA_HarvestingParty
		protected override void CheckCA_OA_HarvestingParty()
		{
			base.CheckCA_OA_HarvestingParty();

			var parent = Parent;
			var pgaHeader = PGAHeader;
			if (!parent.CA_OA_HarvestingParty.IsEmpty)
			{
				OrganisationValidation.ValidateCAPContactOrAddressPhone(parent.CA_OA_HarvestingPartyInfo, pgaHeader.HarvestingParty);

				if (pgaHeader.InvoiceLine is JobComInvoiceLine invoiceLine &&
					invoiceLine.Declaration is JobDeclaration declaration &&
					declaration.IsIID &&
					pgaHeader.HarvestingParty is OrgAddress orgAddress)
				{
					CAAddressValidator.ValidateMandatory(orgAddress, parent.CA_OA_HarvestingPartyInfo, Res.GetString("0B505BCF-1DE4-48B8-ADFE-915CF4B9FCC3", "Harvesting Party"));
				}
			}
		}
		#endregion

		protected override void CheckCA_OA_Processor()
		{
			base.CheckCA_OA_Processor();

			var parent = Parent;
			var pgaHeader = PGAHeader;
			if (parent.CA_OA_Processor.IsValid &&
				pgaHeader.InvoiceLine is JobComInvoiceLine invoiceLine &&
				invoiceLine.Declaration is JobDeclaration declaration &&
				declaration.IsIID &&
				pgaHeader.Processor is OrgAddress orgAddress)
			{
				CAAddressValidator.ValidateMandatory(orgAddress, parent.CA_OA_ProcessorInfo, Res.GetString("85974E1E-96AA-405F-868A-D127ED0DE6C9", "Processor"));
			}
		}
	}
}
