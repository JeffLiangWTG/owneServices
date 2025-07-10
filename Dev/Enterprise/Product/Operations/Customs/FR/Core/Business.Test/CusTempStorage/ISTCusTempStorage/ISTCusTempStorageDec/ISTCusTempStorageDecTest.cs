using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(ISTCusTempStorageDec))]
	public class ISTCusTempStorageDecTest : CusTempStorageDecTest
	{
		[TestDate(2020, 06, 11)]
		public void TestSetDefaultValues()
		{
			var storage = GetCusTempStorageDecForTesting();
			AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, storage.STH_DeclarationType);
		}

		public void TestGetTemporaryStorageRegisterTransactionData()
		{
			var register1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			register1.SRH_Reference = "DDT1";

			var register2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			register2.SRH_Reference = "DDT2";

			var storage1 = (ISTCusTempStorageDec)GetCusTempStorageDecForTesting();
			var header1 = CusTempStorageJobHeader.New(Factory);

			header1.DDTNumber = "DDT1";
			header1.SJH_JobReference = "JobReference";
			header1.SJH_ReferenceNumber = "ReferenceNumber";
			header1.SJH_PreviousReferenceType = FRConstants.TemporaryStorage.AppCodeIST;
			header1.SJH_PreviousReferenceNumber = "DDT2";
			storage1.STH_SJH = header1.PK;
			var storageLine = storage1.CusTempStorageLines.AddNew();
			storageLine.TSL_GrossWeight = 10000;
			storageLine.TSL_GrossWeightUQ = "G";
			storageLine.TSL_PackageQty = 4;
			var storageLine2 = storage1.CusTempStorageLines.AddNew();
			storageLine2.TSL_GrossWeight = 20;
			storageLine2.TSL_GrossWeightUQ = "KG";
			storageLine2.TSL_PackageQty = 5;

			var header2 = CusTempStorageJobHeader.New(Factory);
			header2.DDTNumber = "DDT2";
			header2.SJH_JobReference = "DDT2";

			var transaction = storage1.GetTemporaryStorageRegisterTransactionData().ToList();
			AssertEquals(2, transaction.Count);

			CombineAssertions("Transaction values", () =>
			{
				AssertEquals("PreviousRegisterHeader", register2, transaction.ElementAt(0).PreviousRegisterHeader);
				AssertEquals("InternalReferenceNumber", "JobReference", transaction.ElementAt(0).InternalReferenceNumber);
				AssertEquals("CustomsReferenceNumber", "DDT1", transaction.ElementAt(0).CustomsReferenceNumber);
				AssertEquals("PackageQuantity", 4, transaction.ElementAt(0).PackageQuantity);
				AssertEquals("ReferenceType", TempStorageTransactionRefTypeList.Codes.IstHeader, transaction.ElementAt(0).ReferenceType);
				AssertEquals("GrossMass", 10m, transaction.ElementAt(0).GrossMass);
				AssertEquals("Comments", "", transaction.ElementAt(0).Comments);
			});
		}

		public override void TestValidation()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			AssertType<ISTCusTempStorageDecValidation>(storageDec.Validation);
		}

		protected override CusTempStorageDec GetCusTempStorageDecForTesting()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();
			var result = ISTCusTempStorageDec.Load(header);
			result.CusTempStorageLines.RemoveAndDeleteAll();
			return result;
		}

		protected override ZString GetAppCode() => FRConstants.TemporaryStorage.AppCodeIST;
	}
}
