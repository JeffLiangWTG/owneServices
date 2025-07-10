using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MX.Business
{
	public class JobComInvoiceLineValidation : Customs.Business.BaseJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateObservations();
		}

		protected override void CheckJI_CEI()
		{
			if (Parent.Declaration?.IsPersistent ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CEIInfo);
			}
		}

		protected override void CheckJI_RN_NKCountryOfExport()
		{
			base.CheckJI_RN_NKCountryOfExport();
			if (Parent.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_RN_NKCountryOfExportInfo);
			}
		}

		public void ValidateObservations()
		{
			ValidateCalculatedProperty(Parent.ObservationsInfo);
		}

		protected virtual void CheckObservations()
		{
			if (Parent.Observations.Split('\r', '\n').Any(a => a.Length > 120))
			{
				Parent.ObservationsInfo.AddMessageError(Res.GetString("B8AF0B7B-292E-40D2-BB85-D2B470D09ABD", "Each observation cannot have more than 120 characters."));
			}
		}
	}
}
