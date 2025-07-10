using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaBillEntryHeaderDataObjectReader : DataObjectReader<EntryHeader, AsycudaBill>
	{
		public AsycudaBillEntryHeaderDataObjectReader(EntryHeader dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper)
			: base(dataObject, logger, factory)
		{
			this.bill = Argument.NotNull(bill, "bill");
			this.helper = Argument.NotNull(helper, "helper");
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestDataObjectReaderHelper helper;

		protected override AsycudaBill GetExistingBusinessObject()
		{
			return bill;
		}

		protected sealed override void PopulateBusinessObject(AsycudaBill countryBO)
		{
			FillEntryNumbers(countryBO);
		}

		void FillEntryNumbers(AsycudaBill countryBO)
		{
			if (dataObject.EntryNumberCollection != null)
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, countryBO.PK);
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, helper.CountryCode);
				query.FetchOnlyFromLocalCache = !countryBO.IsInDatabase;
				var existingNumbers = new List<CusEntryNumber>(factory.Load<CusEntryNumber>(query));
				foreach (var entryNumberDataObject in dataObject.EntryNumberCollection)
				{
					var entryNumber = new CustomsEntryNumberDataObjectReader<AsycudaBill>(entryNumberDataObject, logger, factory, countryBO, helper.CountryCode).ReadIntoBusinessObject();
					existingNumbers.Remove(entryNumber);
				}
				existingNumbers.DeleteAll();
			}
		}
	}
}
