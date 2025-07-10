using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer
{
	public class AsycudaBillDataObjectReader : ASYCUDA.Business.UniversalDataTransfer.AsycudaBillDataObjectReader
	{
		public AsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override void PopulateBillForSpecificRules(ASYCUDA.Business.AsycudaBill bill)
		{
			base.PopulateBillForSpecificRules(bill);

			var manifestHeader = header as AsycudaManifestHeader;
			if (dataObject.EntryNumberCollection?.Find(x => x.Type.Code.GetValueOrDefault().EqualsIgnoringCase(Constants.CustomsEntryType.SAD)) is EntryNumber entryNumberDataObject && manifestHeader != null && manifestHeader.ShowExportGeneralManifest)
			{
				var entryNumber = CusEntryNumber.LoadOrCreate(bill, Constants.CustomsEntryType.SAD, helper.CountryCode);
				var entryNumberRow = GetColumnIndexer(entryNumber);
				SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryNum, entryNumberDataObject.Number);
				SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryLineReference, entryNumberDataObject.EntryLineReference);
				SetValue(entryNumberRow, CusEntryNumSchema.CE_Category, entryNumberDataObject.Category);
				SetValue(entryNumberRow, CusEntryNumSchema.CE_IssueDate, entryNumberDataObject.IssueDate);
			}
		}
	}
}
