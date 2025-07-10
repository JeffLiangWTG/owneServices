using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration;

public class InvoiceLineCompleteCollection(JobDeclaration jobDeclaration) : EU.Business.Declaration.InvoiceLineCompleteCollection(jobDeclaration)
{
	protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var invLine = (JobComInvoiceLine)child;
		invLine.ZG_StatisticalValueManualOverride = false;
		invLine.ZG_CountryOfDestination = ZString.Empty;

		var jobDeclaration = JobDeclaration;
		var importerAddInfo = jobDeclaration.ImporterAddInfo;
		if (jobDeclaration.IsImport && !jobDeclaration.ZG_DestinationState.IsEmpty && importerAddInfo != null)
		{
			invLine.ZG_MethodOfPayment = importerAddInfo.ZO_MethodOfPayment;

			if (jobDeclaration.DestinationStateIsCanaryIsland)
			{
				invLine.ZG_MethodOfPayment2 = importerAddInfo.ZO_MethodOfPaymentCan;
			}
		}
	}
}
