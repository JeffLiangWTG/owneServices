using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLine))]
	public class CusTempStorageLineTest : CusTempStorageLineTestCase<CusTempStorageLine>
	{
		public void TestCusTempStorageLineItem()
		{
			var storageLine = GetNewCusTempStorageLine();
			AssertNotNull(storageLine.CusTempStorageLineItem);
			AssertEquals(1, storageLine.CusTempStorageLineItems.Count);
		}

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var storageLine = GetNewCusTempStorageLine();
			storageLine.Delete();
		}

		protected override CusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var storageHeader = CusTempStorageJobHeader.New(factory);
			var storageDec = CusTempStorageDec.New(storageHeader);
			var storageLine = storageDec.CusTempStorageLines.AddNew();

			return storageLine;
		}

		protected override Type GetLookupType() => typeof(CusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CusTempStorageLineValidation);

		protected override Type GetLineItemsType() => typeof(CusTempStorageLineItemCollection<CusTempStorageLineItem>);

		protected override Type GetDecType() => typeof(CusTempStorageDec);
	}
}
