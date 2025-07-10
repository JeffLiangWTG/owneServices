using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	/// <summary>
	/// Summary description for IAutoRateAndJobInvoicing.
	/// </summary>
	public interface ICustomsJobInfo : IRatingSupporter, IJobInvoicingPlugIn, IImportExport
	{
		IJobInvoicingPlugIn TopLevelObjectForJobToReference { get; }
		GlbBranch Branch { get; }

		AutoPostingNotification AutoPostingNotification { get; }

		ZString[] GetValidAPInvoiceNumsToMatchAndValidateAgainst();

		ZGuid CreditorPK { get; }

		EntryInfoCollection Entries { get; }
	}

	public interface ICustomsJobInfoProvider
	{
		ICustomsJobInfo GetCustomsJobInfo(ZGuid companyPK);
	}
}
