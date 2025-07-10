using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderEntryInstructionDataObjectWriter<T> : DataObjectWriter<T, EntryInstruction>
		where T : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderEntryInstructionDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = helper;
		}

		readonly AsycudaManifestHeaderDataObjectWriterHelper helper;

		protected override EntryInstruction PopulateDataObject(T headerSource)
		{
			var entryInstruction = new EntryInstruction(writeManager.WriterStrategy)
			{
				Link = helper.AllocateEntryInstructionLink(headerSource.PK),
				Style = headerSource.AMA_ManifestType,
				DateAtCustomsOffice = headerSource.AMA_DateAtCustomsOffice,
				FirstArrival = ListHelper.GetWithName(headerSource.AMA_RL_NKPortOfFirstArrival, headerSource.Factory.GetRefUNLOCOList())
			};
			entryInstruction.AddOrgAddress(writeManager, headerSource.ShippingAgent, DocAddressType.ControllingAgent);
			if (!headerSource.AMA_CarrierCode.IsEmpty)
			{
				PopulateCarrierCode(entryInstruction, headerSource);
			}
			PopulateAddInfosData(entryInstruction, headerSource);
			return entryInstruction;
		}

		void PopulateCarrierCode(EntryInstruction entryInstruction, T headerSource)
		{
			if (entryInstruction.OrganizationAddressCollection == null)
			{
				entryInstruction.OrganizationAddressCollection = new List<OrganizationAddress>();
			}

			var orgAddress = new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = nameof(DocAddressType.Carrier),
			};
			if (orgAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()))
			{
				orgAddress.RegistrationNumberCollection.Add(new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = OrgCusCode.CodeTypes.CarrierCode,
						Description = "Carrier Code",
					},
					CountryOfIssue = Country.New(headerSource.Country),
					Value = headerSource.AMA_CarrierCode,
				});
			}
			entryInstruction.OrganizationAddressCollection.Add(orgAddress);
		}

		void PopulateAddInfosData(EntryInstruction entryInstruction, T headerSource)
		{
			entryInstruction.AddInfoCollection = new List<AddInfo>();
			entryInstruction.AddInfoCollection.Add(new AddInfo { Key = AsycudaManifestHeaderEntryInstructionDataObjectReader.AHC_Nature, Value = headerSource.AMA_Nature });

			entryInstruction.AddInfoCollection.WriteGenAddOnColumnIntoAddInfoCollection(helper.GetAsycudaManifestHeaderGenAddOnColumnList(headerSource), headerSource);
		}
	}
}
