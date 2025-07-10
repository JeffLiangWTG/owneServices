using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportAddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
	{
		public ImportAddInfoCusEntryInstructionValidation(EU.Business.Declaration.AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		protected CusEntryInstruction EntryInstruction => (CusEntryInstruction)Parent.Parent;

		protected override void CheckZG_RateOfYield()
		{
			CheckBR8F0003();
		}

		void CheckBR8F0003()
		{
			var parent = EntryInstruction;
			if (parent.ZG_RateOfYield.IsEmpty &&
				((parent.IsH1 && parent.IsProcedureCode44) ||
				(parent.IsH4 && parent.IsInwardProcessingProcedure51)) &&
				parent.HasAuthorisationForSpecialProcedure)
			{
				parent.ZG_RateOfYieldInfo.AddMessageError(Res.GetString("05CF18E0-F28B-49C4-88DE-EAF0C7641680", "[BR8F0003] Rate of Yield is required."));
			}
		}

		protected override void CheckZG_BillOfDischargeIsNecessary()
		{
			CheckBR8F0002();
		}

		void CheckBR8F0002()
		{
			var parent = EntryInstruction;

			if (!parent.ZG_BillOfDischargeIsNecessary &&
				((parent.IsH1 && parent.IsProcedureCode44) ||
				(parent.IsH4 && parent.IsInwardProcessingProcedure51)) &&
				 parent.HasAuthorisationForSpecialProcedure)
			{
				parent.ZG_BillOfDischargeIsNecessaryInfo.AddMessageError(Res.GetString("9609BD06-0933-46FA-BA47-D9AC7F747D6B", "[BR8F0002] Bill of Discharge is necessary."));
			}
		}

		protected override void CheckZG_PeriodForDischarge()
		{
			CheckBR8074();
			CheckBR8F0011();
		}

		void CheckBR8F0011()
		{
			var parent = EntryInstruction;

			bool IsStyleAndProcedureMatchRule() =>
				(parent.IsH1 && parent.IsProcedureCode44)
				|| (parent.IsH3 && parent.IsProcedureCode53)
				|| (parent.IsH4 && parent.IsInwardProcessingProcedure51);
			if (parent.ZG_PeriodForDischarge == 0 && IsStyleAndProcedureMatchRule() && parent.HasAuthorisationForSpecialProcedure)
			{
				parent.ZG_PeriodForDischargeInfo.AddMessageError(Res.GetString("E83E40B8-6A36-4241-8356-47DAD0B6E552", "[BR8F0011] Period for Discharge > Period (Month) is required."));
			}
		}

		void CheckBR8074()
		{
			var parent = EntryInstruction;
			var periodForDischarge = parent.ZG_PeriodForDischarge;
			if (periodForDischarge > 6)
			{
				var targetPropertyInfo = parent.ZG_PeriodForDischargeInfo;
				var humanReadableName = targetPropertyInfo.HumanReadableName;
				if (parent.IsProcedureCode44 || parent.IsInwardProcessingProcedure51)
				{
					targetPropertyInfo.AddMessageError(Res.GetString("21BABDC3-AB3F-4F2F-A659-8A67AA1D3B4E", "[BR8074] If Requested Procedure is 51 or 44, {0} cannot be greater than 6.", humanReadableName));
				}
				if (periodForDischarge > 24)
				{
					targetPropertyInfo.AddMessageError(Res.GetString("A7A6CFF6-BD75-403A-8AD6-A917E6563644", "[BR8074] {0} cannot be greater than 24.", humanReadableName));
				}
			}
		}

		protected override void CheckZG_ProcessingProcedureCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ProcessingProcedureCodeInfo);

			CheckBR8F0004();
		}

		void CheckBR8F0004()
		{
			var parent = Parent;
			var entryInstruction = EntryInstruction;

			if (entryInstruction.IsH4 && entryInstruction.IsInwardProcessingProcedure51
				&& parent.ZG_ProcessingProcedureCode.IsEmpty
				&& entryInstruction.AdditionalInfos.HasAuthorisationForSpecialProcedure())
			{
				parent.ZG_ProcessingProcedureCodeInfo.AddMessageError(Res.GetString("44513DF9-DB7D-4E6D-9905-76DD8C11718B", "[BR8F0004] Processing Procedures > Procedure Code is required."));
			}
		}

		protected override void CheckZG_Article86_3_UCC()
		{
			base.CheckZG_Article86_3_UCC();
			CheckBR8F0005();
		}

		void CheckBR8F0005()
		{
			var entryInstruction = EntryInstruction;
			if (entryInstruction.ZG_Article86_3_UCC.IsEmpty
				&& entryInstruction.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H4) && entryInstruction.IsInwardProcessingProcedure51
				&& entryInstruction.AdditionalInfos.HasAuthorisationForSpecialProcedure())
			{
				Parent.ZG_Article86_3_UCCInfo.AddMessageError(Res.GetString("66750003-6B86-4CA6-BAA2-9C64F890C636", "[BR8F0005] Calculate the Amount of Import Duty in Accordance with Article 86(3) of the Code is required when Dataset is 'H4' and Requested Procedure is '51'."));
			}
		}

		protected override void CheckZG_IdOfGoodCode()
		{
			base.CheckZG_IdOfGoodCode();
			CheckBR8F0013();
		}

		void CheckBR8F0013()
		{
			var parent = Parent;
			if (parent.ZG_IdOfGoodCode.IsEmpty && EntryInstruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired)
			{
				parent.ZG_IdOfGoodCodeInfo.AddMessageError(Res.GetString("4C51A6D4-5650-41F6-B44F-05DAF0CF0020", "[BR8F00013] Identification of Goods > Code is required."));
			}
		}
	}
}
