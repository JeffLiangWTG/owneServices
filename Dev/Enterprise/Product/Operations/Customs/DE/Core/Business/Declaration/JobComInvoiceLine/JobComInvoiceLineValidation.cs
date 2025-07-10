using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateOutwardMRN();
			ValidateOutwardDecisiveDate();
			ValidateJI_CustomsValue();

			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				CheckSpecialCasesExists();
				CheckAirFreightCostsExists();
				CheckInwardProcessingProducts();
			}
		}

		public void ValidateJI_CustomsValue()
		{
			ValidateCalculatedProperty(Parent.JI_CustomsValueInfo);
		}

		protected virtual void CheckJI_CustomsValue()
		{
		}

		public virtual void CheckSpecialCasesExists()
		{
		}

		public virtual void CheckAirFreightCostsExists()
		{
		}

		public virtual void CheckInwardProcessingProducts()
		{
		}

		public void ValidateOutwardMRN()
		{
			ValidateCalculatedProperty(Parent.OutwardMRNInfo);
		}

		protected virtual void CheckOutwardMRN()
		{
		}

		public void ValidateOutwardDecisiveDate()
		{
			ValidateCalculatedProperty(Parent.OutwardDecisiveDateInfo);
		}

		protected virtual void CheckOutwardDecisiveDate()
		{
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			var info = Parent.JI_CustomsSecondQuantityInfo;
			MandatoryValidation.CheckNotNegative(info);

			var customsSecondUnitQty = Parent.JI_CustomsSecondUnitQty;
			var customsSecondQty = Parent.JI_CustomsSecondQuantity;
			if (Parent.Factory.IsIntegerRequiredUnitOfQuantity(customsSecondUnitQty) && !customsSecondQty.IsInteger)
			{
				info.AddMessageError(Res.GetString("7EAC794F-21ED-4FE7-BF60-C4C7266AC443", "Only integer values are allowed for this Supplementary Quantity Unit"));
			}

			CheckJI_CustomsSecondQuantity_Mandatory();
		}

		protected virtual void CheckJI_CustomsSecondQuantity_Mandatory()
		{
			var parent = Parent;
			if (!parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JI_CustomsSecondQuantityInfo, JI_CustomsSecondQuantityMandatoryMessageError);
			}
		}

		protected override INotificationType ValidRatesNotificationSeverity => CargoWise.EntityFramework.NotificationType.Warning;

		protected virtual void CheckPreviousDocumentProcedureCode()
		{
		}

		protected override void CheckJI_CEI()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CEIInfo);
		}

		protected override void CheckJI_ZZF_NKTaxType()
		{
			// no validation, TaxType is not used in exports/imports
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();
			if (!Parent.JI_CustomsThirdQuantity.IsInteger && Parent.Factory.IsIntegerRequiredUnitOfQuantity(Parent.JI_CustomsThirdUnitQty))
			{
				Parent.JI_CustomsThirdQuantityInfo.AddMessageError(Res.GetString("C5293D47-1DB7-45E0-94EA-E921EEC76631", "Only integer values are allowed for this Third Qty Unit"));
			}
		}

		protected override bool ShouldCheckNo7NNNSupplementaryCodeForMeursingIsNotApplicable => false;

		protected void CheckJI_ProcedureForImportAndWarehouseAdjustment()
		{
			base.CheckJI_Procedure();

			var parent = Parent;
			var cpcInfo = parent.JI_ProcedureInfo;

			if (parent.ProcedureNeedSpecialRateCharge && !parent.Charges.Cast<InvoiceLineCharge>().Any(x => ImportChargeCodeList.IsSpecificRate(x.J7_ChargeType)))
			{
				cpcInfo.AddMessageError(Res.GetString("D44F85A7-A53E-46A9-8957-22C2F086BF73", "This CPC requires a Charge Code of Type SRC or SRN or SRS."));
			}
		}

		protected CusEntryInstruction instruction => Parent.EntryInstruction;

		protected string JI_CustomsSecondQuantityMandatoryMessageError => Res.GetString("46269ADE-C3A6-4994-A54B-8A625C3723B3", "You have not entered a valid Supp. Qty");
	}
}
