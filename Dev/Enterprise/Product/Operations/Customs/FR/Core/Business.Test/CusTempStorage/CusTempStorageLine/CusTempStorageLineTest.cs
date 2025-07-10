using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageLine))]
	public abstract class CusTempStorageLineTest : CusTempStorageLineTestCase<CusTempStorageLine>
	{
		protected override CusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory) =>
			(CusTempStorageLine)GetNewBusinessObject();

		protected override Type GetLookupType() => typeof(CusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CusTempStorageLineValidation);

		protected override Type GetLineItemsType() => typeof(CusTempStorageLineItemCollection<CusTempStorageLineItem>);

		public void TestReadOnly_TSL_GrossWeightUQ()
		{
			var line = GetNewCusTempStorageLine(Factory);
			AssertEquals("TSL_GrossWeightUQ should be read only.", true, line.TSL_GrossWeightUQInfo.ReadOnly);
		}

		public override void TestDecType()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory, GetAppCode());
			var storageDec = storageHeader.CusTempStorageDec;
			var storageLine = (CusTempStorageLine)Factory.New(GetExpectedBusinessObjectType());
			storageLine.TSL_STH = storageDec.PK;

			AssertType(GetDecType(), storageLine.Dec);
		}

		protected abstract ZString GetAppCode();
	}
}
