using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using EntryHeader = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderEntryHeaderDataObjectWriter<T> : DataObjectWriter<T, EntryHeader>
		where T : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderEntryHeaderDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper linkHelper)
			: base(manager)
		{
			this.linkHelper = linkHelper;
		}

		readonly AsycudaManifestHeaderDataObjectWriterHelper linkHelper;

		protected override EntryHeader PopulateDataObject(T source)
		{
			var entryHeader = new EntryHeader(writeManager.WriterStrategy)
			{
				Type = ListHelper.GetWithDescription<EntryType>(source.AMA_RN_NKCountry, source.Lookups.Countries),
				EntryInstructionLink = linkHelper.AllocateEntryInstructionLink(source.PK),
			};

			PopulateRegistrationNumberDetails(source, entryHeader);
			return entryHeader;
		}

		void PopulateRegistrationNumberDetails(T source, EntryHeader entryHeader)
		{
			if (!source.RegistrationDate.IsEmpty)
			{
				entryHeader.EntryNumberCollection = new List<EntryNumber>();
				entryHeader.EntryNumberCollection.Add(new EntryNumber()
				{
					Type = new EntryType
					{
						Code = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration,
						Description = Constants.CodeTypeDescription.AsycudaRegistrationDescription,
					},
					Number = source.RegistrationNumber,
					IssueDate = source.RegistrationDate,
					EntryStatus = ListHelper.GetWithDescription<EntryStatus>(source.RegistrationStatus, source.Lookups.RegistrationStatusList)
				});
			}
		}
	}
}
