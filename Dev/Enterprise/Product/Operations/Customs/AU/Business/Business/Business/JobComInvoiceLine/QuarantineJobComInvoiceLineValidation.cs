using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineJobComInvoiceLineValidation : ExportJobComInvoiceLineValidation
	{
		public QuarantineJobComInvoiceLineValidation(JobComInvoiceLine line)
			: base(line)
		{
		}

		ZBool IsNEXDOCSActive => Parent.QuarantineExDocLine.QuarantineExDocHeader.IsNEXDOCSActive;

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRFPNumbers();
		}

		#endregion

		#region CheckJI_Tariff

		protected override void CheckJI_Tariff()
		{
			if (Parent.InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit)
			{
				base.CheckJI_Tariff();
			}
		}

		#endregion

		#region CheckJI_LinePrice

		protected override void CheckJI_LinePrice()
		{
			if (!Parent.Declaration.IsAQISCertificateRequest)
			{
				if (Parent.JI_LinePrice.IsEmpty && Parent.InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit)
				{
					Parent.JI_LinePriceInfo.AddMessageError("When Quarantine is to obtain a customs EDN a price is required.");
				}
				else if (!IsNEXDOCSActive)
				{
					base.CheckJI_LinePrice();
				}
			}
		}

		#endregion

		#region ValidateRFPNumbers

		public void ValidateRFPNumbers()
		{
			if (!Parent.IsValidationSuspended)
			{
				const string messageError = "At least one RFP Number should be entered for Certificate Request message.";
				if (Parent.Declaration.IsAQISCertificateRequest && Parent.RFPNumbers.Count == 0)
				{
					if (!Parent.RowMessageErrors.HasMessageErrors())
					{
						Parent.AddRowMessageError(messageError);
					}
				}
				else
				{
					Parent.RemoveRowNotification(new Notification(CargoWise.EntityFramework.NotificationType.MessageError, messageError));
				}
			}
		}

		#endregion

		#region No Validation for Quarantine

		protected override void CheckJI_Weight()
		{
			if (IsNEXDOCSActive)
			{
				Parent.JI_WeightInfo.AddAllNotificationsFrom(Parent.QuarantineExDocLine.QL_GrossMetricWeightInfo);
			}
		}

		protected override void CheckJI_Description()
		{
			//No Validation for Quarantine
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			//No Validation for Quaratine
		}

		#endregion
	}
}
