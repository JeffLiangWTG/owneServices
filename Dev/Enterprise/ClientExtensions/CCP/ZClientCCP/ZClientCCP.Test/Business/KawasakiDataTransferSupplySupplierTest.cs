using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.ZClientCCP.Business.Testing
{
	[TestedType(typeof(KawasakiDataTransferSupplySupplier))]
	public class KawasakiDataTransferSupplySupplierTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSupplierListIsFilled()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, SQLComparisonOperator.Equal, true);
			OrgHeaderCollection supplierList = new OrgHeaderCollection(Factory, filter);
			AssertEquals("Supplier List is not valid", supplierList.Count, TransferBusinessObject.SupplierList.Count);
		}

		public void TestPropertiesForDataTransferAreNotEmpty()
		{
			AssertEquals("Dialog Filter should not be empty", Filter, TransferBusinessObject.DialogFilter);
			AssertEquals("Dialog Heading should not be empty", "Kawasaki", TransferBusinessObject.FormHeading);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			return new KawasakiDataTransferSupplySupplier(jobDec, Filter, "Kawasaki");
		}

		#region SetUp
		protected override void SetUp()
		{
			//			Factory = new BusinessObjectFactory();
			Declaration = Factory.New<BaseJobDeclaration>();
			TransferBusinessObject = new KawasakiDataTransferSupplySupplier(Declaration, "Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*", "Kawasaki");
		}

		KawasakiDataTransferSupplySupplier TransferBusinessObject;
		BaseJobDeclaration Declaration;
		//		private BusinessObjectFactory Factory;
		const string Filter = "Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
		#endregion
	}
}
