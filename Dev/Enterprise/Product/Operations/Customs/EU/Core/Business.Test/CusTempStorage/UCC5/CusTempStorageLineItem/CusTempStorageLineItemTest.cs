using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLineItem))]
	public class CusTempStorageLineItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var storageJob = factory.New<CusTempStorageJobHeader>();
			storageJob.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJob.SJH_JobReference = "From1";
			storageJob.SJH_OH_Customer = customer.PK;
			storageJob.SJH_OA_Presenter = presenter.PK;
			storageJob.SJH_OA_Representative = representative.PK;

			var storageDec = storageJob.CusTempStorageDecs.AddNew();
			storageDec.STH_SJH = storageJob.PK;
			storageDec.STH_DeclarationType = "A";

			var storageLine = storageDec.CusTempStorageLines.AddNew();
			storageLine.TSL_STH = storageDec.PK;
			storageLine.TSL_LineNo = 1;
			storageLine.TSL_ReferenceNumberLine = 1;

			var storageItem = factory.New<CusTempStorageLineItem>();
			storageItem.TSI_TSL = storageLine.PK;
			storageItem.TSI_NetWeight = 30m;
			storageItem.TSI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			storageItem.TSI_CommodityCode = "comm";

			return storageItem;
		}
	}
}
