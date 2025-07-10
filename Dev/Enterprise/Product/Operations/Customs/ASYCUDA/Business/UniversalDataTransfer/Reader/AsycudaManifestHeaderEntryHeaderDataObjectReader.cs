using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderEntryHeaderDataObjectReader : DataObjectReader<EntryHeader, AsycudaManifestHeader>
	{
		public AsycudaManifestHeaderEntryHeaderDataObjectReader(EntryHeader dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper)
			: base(dataObject, logger, factory)
		{
			this.header = Argument.NotNull(header, "header");
			this.helper = Argument.NotNull(helper, "helper");
		}
		readonly AsycudaManifestHeader header;
		readonly AsycudaManifestDataObjectReaderHelper helper;

		protected override AsycudaManifestHeader GetExistingBusinessObject()
		{
			return header;
		}

		protected sealed override void PopulateBusinessObject(AsycudaManifestHeader headerBO)
		{
			var countryRow = GetColumnIndexer(headerBO);
			var countryCode = helper.CountryCode;
			SetValue(countryRow, AsycudaManifestHeaderSchema.AMA_RN_NKCountry, countryCode);
			FillEntryNumbers(headerBO, countryCode);
		}

		void FillEntryNumbers(AsycudaManifestHeader headerBO, ZString countryCode)
		{
			if (dataObject.EntryNumberCollection != null)
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, headerBO.PK);
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
				query.FetchOnlyFromLocalCache = !headerBO.IsInDatabase;
				var existingNumbers = new List<CusEntryNumber>(factory.Load<CusEntryNumber>(query));
				foreach (var entryNumberDataObject in dataObject.EntryNumberCollection)
				{
					var entryNumber = new CustomsEntryNumberDataObjectReader<AsycudaManifestHeader>(entryNumberDataObject, logger, factory, headerBO, countryCode).ReadIntoBusinessObject();
					existingNumbers.Remove(entryNumber);
				}
				existingNumbers.DeleteAll();
			}
		}
	}
}
