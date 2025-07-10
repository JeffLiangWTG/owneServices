using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class ManifestCommonResponseMessageProcessor<T, TResponseProvider> : ESCommonResponseMessageProcessor<T, TResponseProvider>
		where T : BusinessObject
	{
		protected ManifestCommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected void CreateOrUpdateCusEntryNumber(AsycudaBill bill, ZString entryType, ZString entryNum, ZString entryLineReference, string entryStatus, ZDateTime issueDate)
		{
			var existingEntryNumber = bill.CustomsEntryNumbers.OfType<ABLEntryNum>().FirstOrDefault(x =>
				x.CE_EntryType == entryType
				&& (string.IsNullOrEmpty(x.CE_EntryLineReference) || x.CE_EntryLineReference == entryLineReference));

			var cusEntryNumber = existingEntryNumber ?? bill.CustomsEntryNumbers.AddNew();

			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_EntryStatus = entryStatus;
			cusEntryNumber.CE_IssueDate = issueDate;
			cusEntryNumber.CE_EntryLineReference = entryLineReference;
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
		}
	}
}
