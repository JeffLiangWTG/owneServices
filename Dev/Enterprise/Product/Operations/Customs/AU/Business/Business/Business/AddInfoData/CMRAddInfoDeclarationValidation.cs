using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAddInfoDeclarationValidation : AUAddInfoValidation
	{
		public CMRAddInfoDeclarationValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_CustShipNo_Hidden()
		{
			base.CheckZA_CustShipNo_Hidden();
			JobDeclaration.Validation.ValidateJE_VesselName();
		}

		protected override void CheckZA_EDITransmitDate()
		{
			base.CheckZA_EDITransmitDate();
			var jobDeclaration = JobDeclaration;
			if (jobDeclaration.IsQueuedEntryLodgementsEnabled && !jobDeclaration.JE_EDITransmitDateInfo.ReadOnly)
			{
				if (jobDeclaration.JE_EDITransmitDate < ZDateTime.Today)
				{
					AddInfo.ZA_EDITransmitDateInfo.AddMessageError("You cannot Submit to Customs if your EDI Transmit Date is in the past.");
				}
				JobDeclaration.Validation.ValidateJE_ExportDate();
			}
		}

		protected override void CheckZA_NilReturnInd_Hidden()
		{
			base.CheckZA_NilReturnInd_Hidden();
			var jobDeclaration = JobDeclaration;
			if (jobDeclaration.NilReturnInd && !jobDeclaration.SettlementTypeSelected)
			{
				AddInfo.ZA_NilReturnInd_HiddenInfo.AddMessageError("Nil Return Indicator may only be set when the Settlement Type is selected.");
			}
			if (jobDeclaration.NilReturnInd && jobDeclaration.InvoiceLines.Count > 0)
			{
				AddInfo.ZA_NilReturnInd_HiddenInfo.AddMessageError("A Nil Return declaration cannot contain any invoice lines.");
			}
		}

		protected override void CheckZA_SettlementPeriodType_Hidden()
		{
			base.CheckZA_SettlementPeriodType_Hidden();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_SettlementPeriodType_HiddenInfo);
			ValidateZA_NilReturnInd_Hidden();
			ValidateZA_SettlementPeriodStartDate_Hidden();
		}

		protected override void CheckZA_SettlementPeriodStartDate_Hidden()
		{
			base.CheckZA_SettlementPeriodStartDate_Hidden();

			var parent = Parent;
			var startDate = parent.ZA_SettlementPeriodStartDate_Hidden;
			var startDateIsValid = startDate.IsValid;
			switch (parent.ZA_SettlementPeriodType_Hidden)
			{
				case SettlementPeriodTypeList.Codes.Weekly:
				case SettlementPeriodTypeList.Codes.WeeklyLegacy:
					if (!startDateIsValid)
					{
						parent.ZA_SettlementPeriodStartDate_HiddenInfo.AddMessageError("Settlement Period Start Date is required if Settlement Type is selected.");
					}
					break;
				case SettlementPeriodTypeList.Codes.Monthly:
					if (!startDateIsValid || startDate.Day != 1)
					{
						parent.ZA_SettlementPeriodStartDate_HiddenInfo.AddMessageError("Settlement Period Start Date must be the 1st day of a month if Monthly Settlement Type is selected.");
					}
					break;
				case SettlementPeriodTypeList.Codes.Quarterly:
					if (!startDateIsValid || startDate.Day != 1 || !startDate.Month.In(1, 4, 7, 10))
					{
						parent.ZA_SettlementPeriodStartDate_HiddenInfo.AddMessageError("Settlement Period Start Date must be the 1st day of January, April, July or October if Quarterly Settlement Type is selected.");
					}
					break;
			}
		}

		protected override void CheckZA_SettlementPeriodEndDate_Hidden()
		{
			base.CheckZA_SettlementPeriodEndDate_Hidden();
			var parent = Parent;
			if (!parent.ZA_SettlementPeriodEndDate_HiddenInfo.ReadOnly)
			{
				if (parent.ZA_SettlementPeriodStartDate_Hidden > parent.ZA_SettlementPeriodEndDate_Hidden)
				{
					parent.ZA_SettlementPeriodEndDate_HiddenInfo.AddMessageError("Settlement Period End Date cannot be earlier than the Settlement Period Start Date.");
				}
			}
		}

		protected override void CheckZA_HART_Hidden()
		{
			base.CheckZA_HART_Hidden();
			var jobDeclaration = JobDeclaration;

			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_HART_HiddenInfo, AddInfo.Lookups.ZA_HART_List);
			if (!jobDeclaration.JE_AmberStatement.IsEmpty && (AddInfo.ZA_HART_Hidden.IsEmpty && !jobDeclaration.InvoiceLines.HasAnAmberReason))
			{
				AddInfo.ZA_HART_HiddenInfo.AddMessageError("There is no amber reason type for Header or Line and thus, the amber statement is not required.");
			}

			((JobDeclarationValidation)jobDeclaration.Validation).ValidateJE_AmberStatement();
		}

		protected override void CheckZA_FPUP_Hidden()
		{
			base.CheckZA_FPUP_Hidden();
			if (!AddInfo.ZA_FPUP_Hidden.IsEmpty && AddInfo.ZA_FPUP_Hidden.Length != 9)
			{
				AddInfo.ZA_FPUP_HiddenInfo.AddMessageError("First Paid Under Protest must be 9 characters long.");
			}
			else if (!AddInfo.ZA_FPUP_Hidden.IsEmpty && AddInfo.JobDeclaration != null && !AddInfo.JobDeclaration.JE_PaidUnderProtestStatement.IsEmpty)
			{
				AddInfo.ZA_FPUP_HiddenInfo.AddMessageError("First Paid Under Protest is not allowed when the Paid Under Protest Statement is entered.");
			}
		}

		protected override void CheckZA_UPE_Hidden()
		{
			base.CheckZA_UPE_Hidden();
			if (!AddInfo.ZA_UPE_Hidden.IsEmpty && AddInfo.ZA_UPE_Hidden.Length != 9)
			{
				AddInfo.ZA_UPE_HiddenInfo.AddMessageError("Unaccompanied Personal Effects Id must be 9 characters long.");
			}
		}

		protected override void CheckZA_AQISInspectLocation_Hidden()
		{
			base.CheckZA_AQISInspectLocation_Hidden();
			var declaration = JobDeclaration;
			if (declaration != null && !declaration.IsValidationSuspended)
			{
				declaration.DeliveryAddressPostCodeMessages(out var postCodeError, out _);
				if (!postCodeError.IsEmpty)
				{
					AddInfo.ZA_AQISInspectLocation_HiddenInfo.AddMessageError(postCodeError);
				}
				declaration.ValidateAllCPDecQuestions();
				declaration.ImporterDeliveryAddress.Validation.ValidateAll();
			}
		}
	}
}
