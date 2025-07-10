using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaBillEntryInstructionDataObjectWriter<TCountry> : DataObjectWriter<TCountry, EntryInstruction>
		where TCountry : AsycudaBill
	{
		public AsycudaBillEntryInstructionDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper linkHelper)
			: base(manager)
		{
			this.linkHelper = linkHelper;
		}

		readonly AsycudaManifestHeaderDataObjectWriterHelper linkHelper;

		protected override EntryInstruction PopulateDataObject(TCountry countrySource)
		{
			var entryInstruction = CreateEntryInstruction(countrySource);
			PopulateBillIssuer(entryInstruction, countrySource);
			PopulateAddInfosData(entryInstruction, countrySource);

			return entryInstruction;
		}

		protected virtual EntryInstruction CreateEntryInstruction(TCountry countrySource)
		{
			CodeDescriptionPair35Char GetLocationAtClearance()
			{
				if (countrySource.Lookups.Locations is CodeDescriptionPairList list)
				{
					return ListHelper.GetWithDescription<CodeDescriptionPair35Char>(countrySource.ABL_GoodsLocation, list);
				}
				else
				{
					return new CodeDescriptionPair35Char() { Code = countrySource.ABL_GoodsLocation };
				}
			}

			var entryInstruction = new EntryInstruction()
			{
				Link = linkHelper.AllocateEntryInstructionLink(countrySource.PK),
				LocationAtClearance = GetLocationAtClearance(),
				Style = countrySource.ABL_ShipmentType,
			};

			return entryInstruction;
		}

		void PopulateBillIssuer(EntryInstruction entryInstruction, TCountry countrySource)
		{
			if (entryInstruction.OrganizationAddressCollection == null)
			{
				entryInstruction.OrganizationAddressCollection = new List<OrganizationAddress>();
			}

			var orgAddress = new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = nameof(DocAddressType.HouseBillIssuingParty),
			};
			if (orgAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()))
			{
				orgAddress.RegistrationNumberCollection.Add(new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = Constants.RegistrationTypes.BillIssuer,
						Description = Constants.CodeTypeDescription.BillIssuerDescription,
					},
					CountryOfIssue = Country.New(countrySource.Header?.Country),
					Value = countrySource.ABL_BillIssuer,
				});
			}
			entryInstruction.OrganizationAddressCollection.Add(orgAddress);
		}

		void PopulateAddInfosData(EntryInstruction entryInstruction, TCountry countrySource)
		{
			entryInstruction.AddInfoCollection = new List<AddInfo>();
			entryInstruction.AddInfoCollection.Add(new AddInfo { Key = AsycudaBillSchema.Constants.ABL_LocationInformation, Value = countrySource.ABL_LocationInformation });

			foreach (var pair in linkHelper.GetBillCountryEntryInstructionAdditionalAddInfos(countrySource))
			{
				if (!pair.Value.IsEmpty)
				{
					entryInstruction.AddInfoCollection.Add(AddInfo.New(pair.Key, pair.Value));
				}
			}

			entryInstruction.AddInfoCollection.WriteGenAddOnColumnIntoAddInfoCollection(linkHelper.GetAsycudaBillGenAddOnColumnList(countrySource), countrySource);
		}
	}
}
