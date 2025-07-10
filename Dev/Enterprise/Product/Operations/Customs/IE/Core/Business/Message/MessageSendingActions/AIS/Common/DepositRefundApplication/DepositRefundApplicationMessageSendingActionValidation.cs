using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class DepositRefundApplicationMessageSendingActionValidation : CusEntryHeaderMessageSendingActionValidation
	{
		public DepositRefundApplicationMessageSendingActionValidation(DepositRefundApplicationMessageSendingAction parent) : base(parent) { }

		public override void ValidateAll()
		{
			ValidateExportMovementReferenceNumber();
			ValidateExportDate();
			ValidateCustomsDuty();
			ValidateVat();
			ValidateOtherDuties();
			ValidateImportedGoodsDischarged();
			ValidateOutstandingBalance();
			ValidateAmountOfDepositRefund();
			ValidatePayerEori();
			ValidatePeriodForDischarge();
			ValidateRateOfYield();
		}

		public void ValidateExportMovementReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.ExportMovementReferenceNumberInfo);
		}

		protected void CheckExportMovementReferenceNumber()
		{
			if (Parent.ShouldSend)
			{
				var targetInfo = Parent.ExportMovementReferenceNumberInfo;
				MandatoryValidation.CheckEntered(targetInfo);
				if (Parent.ExportMovementReferenceNumber.Length != 18)
				{
					targetInfo.AddError(Res.GetString("9C4843E9-5CFD-4D6E-AF20-54CFCAF9F41B", "{0} should be exactly 18 characters long.", targetInfo.HumanReadableName));
				}
				if (!Parent.ExportMovementReferenceNumber.IsLettersAndNumbersOnlyOrEmpty)
				{
					targetInfo.AddError(Res.GetString("81E5956D-1556-4C29-A618-EF5CAF82A27F", "{0} should be alphanumeric only.", targetInfo.HumanReadableName));
				}
			}
		}

		public void ValidateExportDate()
		{
			ValidateCalculatedProperty(Parent.ExportDateInfo);
		}

		protected void CheckExportDate()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.ExportDateInfo);
			}
		}

		public void ValidateCustomsDuty()
		{
			ValidateCalculatedProperty(Parent.CustomsDutyInfo);
		}

		protected void CheckCustomsDuty()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckNotNegative(Parent.CustomsDutyInfo);
			}
		}

		public void ValidateVat()
		{
			ValidateCalculatedProperty(Parent.VatInfo);
		}

		protected void CheckVat()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckNotNegative(Parent.VatInfo);
			}
		}

		public void ValidateOtherDuties()
		{
			ValidateCalculatedProperty(Parent.OtherDutiesInfo);
		}

		protected void CheckOtherDuties()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckNotNegative(Parent.OtherDutiesInfo);
			}
		}
		public void ValidateImportedGoodsDischarged()
		{
			ValidateCalculatedProperty(Parent.ImportedGoodsDischargedInfo);
		}

		protected void CheckImportedGoodsDischarged()
		{
		}

		public void ValidateOutstandingBalance()
		{
			ValidateCalculatedProperty(Parent.OutstandingBalanceInfo);
		}

		protected void CheckOutstandingBalance()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckNotNegative(Parent.OutstandingBalanceInfo);
			}
		}

		public void ValidateAmountOfDepositRefund()
		{
			ValidateCalculatedProperty(Parent.AmountOfDepositRefundInfo);
		}

		protected void CheckAmountOfDepositRefund()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckNotNegative(Parent.AmountOfDepositRefundInfo);
			}
		}

		public void ValidatePayerEori()
		{
			ValidateCalculatedProperty(Parent.PayerEoriInfo);
		}

		protected void CheckPayerEori()
		{
			var parent = Parent;
			if (parent.ShouldSend)
			{
				var targetInfo = parent.PayerEoriInfo;
				if (!parent.PayerEori.IsLettersAndNumbersOnlyOrEmpty)
				{
					targetInfo.AddError(Res.GetString("C78F49FE-AF55-4451-9EE5-DF2FF15D41D2", "{0} should be alphanumeric only.", targetInfo.HumanReadableName));
				}

				CheckPayerEori_RuleBR8070();
			}
		}

		void CheckPayerEori_RuleBR8070()
		{
			var parent = Parent;

			if (parent.PayerEori != parent.EntryHeader.RepresentativeOrDeclarantEoriOfMainOffice)
			{
				parent.PayerEoriInfo.AddError(Res.GetString("E056BB60-1212-4857-B574-5A65814684FE", "[BR8070] Payer EORI for Refund must equal Declarant EORI or empty."));
			}
		}

		public void ValidatePeriodForDischarge()
		{
			ValidateCalculatedProperty(Parent.PeriodForDischargeInfo);
		}

		protected void CheckPeriodForDischarge()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckNotNegative(Parent.PeriodForDischargeInfo);
			}
		}
		public void ValidateRateOfYield()
		{
			ValidateCalculatedProperty(Parent.RateOfYieldInfo);
		}

		protected void CheckRateOfYield()
		{
		}

		new DepositRefundApplicationMessageSendingAction Parent => (DepositRefundApplicationMessageSendingAction)base.Parent;
	}
}
