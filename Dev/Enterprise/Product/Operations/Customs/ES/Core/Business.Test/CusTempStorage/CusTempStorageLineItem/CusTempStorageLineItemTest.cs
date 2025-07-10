using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLineItem))]
	public class CusTempStorageLineItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject(factory);
		}

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var fromJob = CusTempStorageJobHeader.New(factory);
			fromJob.SJH_GB = GlbBranch.CurrentBranch.PK;
			fromJob.SJH_JobReference = "From1";
			fromJob.SJH_OH_Customer = customer.PK;
			fromJob.SJH_OA_Presenter = presenter.PK;
			fromJob.SJH_OA_Representative = representative.PK;

			var fromDec = fromJob.CusTempStorageDec;

			var storageLine = fromDec.CusTempStorageLines.FirstOrDefault() as CusTempStorageLine;
			storageLine.TSL_LineNo = 1;
			storageLine.TSL_ReferenceNumberLine = 1;

			return storageLine.CusTempStorageLineItems.AddNew();
		}

		public void TestSetDefaultValues()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var lineItem = (CusTempStorageLineItem)GetNewBusinessObject(Factory);
				AssertEquals(Core.Constants.CurrencyCodes.France, lineItem.TSI_RX_NKCurrency);
			}
		}

		public void TestTSI_NetWeightUQ()
		{
			var lineItem = (CusTempStorageLineItem)GetNewBusinessObject(Factory);
			AssertHasCustomAttribute<ListAttribute>(lineItem.GetType(), CusTempStorageLineItem.Schema.TSI_NetWeightUQ, false, x => x.ListDataSourceMember == "Lookups.WeightUQList");
		}

		public void TestLookups()
		{
			var lineItem = (CusTempStorageLineItem)GetNewBusinessObject(Factory);
			AssertType<CusTempStorageLineItemLookups>(lineItem.Lookups);
		}
	}
}
