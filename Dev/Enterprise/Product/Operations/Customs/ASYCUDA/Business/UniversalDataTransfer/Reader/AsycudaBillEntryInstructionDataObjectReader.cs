using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaBillEntryInstructionDataObjectReader : DataObjectReader<EntryInstruction, AsycudaBill>
	{
		public AsycudaBillEntryInstructionDataObjectReader(EntryInstruction dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper)
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
			var countryRow = GetColumnIndexer(countryBO);
			SetValue(countryRow, AsycudaBillSchema.ABL_LocationInformation, dataObject.AddInfoCollection.GetZStringValue(AsycudaBillSchema.Constants.ABL_LocationInformation));
			SetValue(countryRow, AsycudaBillSchema.ABL_GoodsLocation, dataObject.LocationAtClearance);
			SetValue(countryRow, AsycudaBillSchema.ABL_ShipmentType, dataObject.Style);
			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(dataObject.AddInfoCollection, helper.GetAsycudaBillGenAddOnColumnList(countryBO), countryBO);
			FillBillIssuer(countryRow);
		}

		void FillBillIssuer(IColumnIndexer countryRow)
		{
			var billIssuerAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.HouseBillIssuingParty));
			var countryCode = helper.CountryCode;
			var targetBillIssuerCode = billIssuerAddress?.RegistrationNumberCollection?.FirstOrDefault(x => x.CountryOfIssue.GetCodeAsUpperCase() == countryCode && x.Type.GetCodeAsUpperCase() == Constants.RegistrationTypes.BillIssuer)?.Value;
			if (targetBillIssuerCode.HasValue)
			{
				SetValue(countryRow, AsycudaBillSchema.ABL_BillIssuer, targetBillIssuerCode.Value);
			}
		}
	}
}
