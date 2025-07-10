using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();

			var declaration = JobDeclaration;
			if (declaration.IsPersistent && declaration.IsImportLicense && declaration.IsMergeDone)
			{
				var strategy = new ImportLicenseEntryCreationStrategy(JobDeclaration);
				this.ForEach(x => strategy.GetKeyForLine(x as JobComInvoiceLine));
			}
		}

		protected override void SetDefaultForCommonInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			base.SetDefaultForCommonInvoiceLine(invoiceLine);
			if (invoiceLine is JobComInvoiceLine invLine)
			{
				if (JobDeclaration.IsImportLicense)
				{
					var drawbackImportLicense = invLine.DrawbackImportLicense;
					using (drawbackImportLicense.SuspendSettingHasChanges())
					using (drawbackImportLicense.GetValidationSuspender())
					{
						invLine.DrawbackModality = DrawbackModalityList.Codes.NoDrawback;
					}
				}
				if (JobDeclaration.IsImportExcludingLicense)
				{
					invLine.JI_PrimaryPreference = Constants.RatePreferenceType.Normal;
				}
			}
		}
	}
}
