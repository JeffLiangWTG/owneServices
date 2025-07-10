using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaBillEntryHeaderDataObjectWriter<TBill> : DataObjectWriter<TBill, EntryHeader>
		where TBill : AsycudaBill
	{
		public AsycudaBillEntryHeaderDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper linkHelper)
			: base(manager)
		{
			this.linkHelper = linkHelper;
		}

		readonly AsycudaManifestHeaderDataObjectWriterHelper linkHelper;

		protected override EntryHeader PopulateDataObject(TBill countrySource)
		{
			var entryHeader = new EntryHeader(writeManager.WriterStrategy)
			{
				Type = ListHelper.GetWithDescription<EntryType>(countrySource.CountryCode, countrySource.Header?.Lookups.Countries),
				EntryStatus = ListHelper.GetWithDescription<EntryStatus>(countrySource.ABL_BillStatus, countrySource.Lookups.CustomsStatusList),
				EntryInstructionLink = linkHelper.AllocateEntryInstructionLink(countrySource.PK),
			};
			entryHeader.CustomsReferenceCollection = new List<CustomsReference>();
			entryHeader.CustomsReferenceCollection.Add(new CustomsReference
			{
				Type = new CodeDescriptionPair { Code = Constants.CustomsReferenceType.ABL_SenderReferenceType },
				Reference = countrySource.ABL_SenderReference,
			});
			PopulateCusEntryNumber(entryHeader, countrySource);
			return entryHeader;
		}

		void PopulateCusEntryNumber(EntryHeader entryHeader, TBill countrySource)
		{
			if (countrySource.CustomsEntryNumbers.Count > 0)
			{
				entryHeader.EntryNumberCollection = new List<EntryNumber>();
				foreach (ABLEntryNum entryNumber in countrySource.CustomsEntryNumbers)
				{
					entryHeader.EntryNumberCollection.Add(new EntryNumber
					{
						Type = ListHelper.GetWithDescription<EntryType>(entryNumber.CE_EntryType, countrySource.Lookups.CustomsEntryNumberTypes),
						Number = entryNumber.CE_EntryNum,
						EntryStatus = new EntryStatus { Code = entryNumber.CE_EntryStatus },
						IssueDate = entryNumber.CE_IssueDate,
					});
				}
			}
		}
	}
}
