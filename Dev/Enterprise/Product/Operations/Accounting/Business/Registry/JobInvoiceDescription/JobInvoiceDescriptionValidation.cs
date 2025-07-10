using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobInvoiceDescriptionValidation : JobConfigurationSelectorValidation
	{
		public JobInvoiceDescriptionValidation(JobInvoiceDescription parent) : base(parent)
		{
		}

		protected new JobInvoiceDescription Parent
		{
			get { return (JobInvoiceDescription)base.Parent; }
		}

		protected override string DuplicateJobParametersError
		{
			get { return Res.GetString("8f0eda6c-e2eb-4fc7-87f6-a361958ba3d1", "At least one more record already sets invoice description for the same Job parameters."); }
		}

		public void ValidateInvoiceDescription()
		{
			MandatoryValidation.CheckEntered(Parent.InvoiceDescriptionInfo, (IMultilingualString)ResString.GetMultilingualString("c0b94034-139d-4ed6-b7c5-f891d6083609", "Invoice Description"));
			BusinessObject.CheckMaximumLength(Parent.InvoiceDescriptionInfo, (ZString)Parent.InvoiceDescriptionInfo.Value);
		}
	}
}

