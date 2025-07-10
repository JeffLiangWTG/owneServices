using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageDec))]
	public class CusTempStorageDecTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackageType()
		{
			var storageDec = Factory.New<CusTempStorageDec>();
			AssertEquals("", storageDec.PackageType);

			var line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_PackageType = "TST";
			AssertEquals("TST", storageDec.PackageType);

			line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_PackageType = "TS2";
			AssertEquals("Multiple", storageDec.PackageType);
		}

		public void TestPackageQty()
		{
			var storageDec = Factory.New<CusTempStorageDec>();
			AssertEquals("PackageQty should be 0 because there is no line in storageDec", 0, storageDec.PackageQty);

			var line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_PackageQty = 1;
			line.TSL_PackageType = "CTN";
			AssertEquals("PackageQty should be sum of CusTempStorageLines TSL_PackageQty", 1, storageDec.PackageQty);

			var line2 = storageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 2;
			line2.TSL_PackageType = "BOX";
			AssertEquals("PackageQty should be sum of CusTempStorageLines TSL_PackageQty, even if all packages are not of same type.", 3, storageDec.PackageQty);
		}

		public void TestLineCount()
		{
			var storageDec = Factory.New<CusTempStorageDec>();
			AssertEquals("LineCount should return 0 because there is no line in storage declaration.", 0, storageDec.LineCount);

			storageDec.CusTempStorageLines.AddNew();
			storageDec.CusTempStorageLines.AddNew();
			AssertEquals("LineCount should return number of lines in storage declaration.", 2, storageDec.LineCount);
		}

		public void TestFetchStrategyGet()
		{
			var storageDec = Factory.New<CusTempStorageDec>();
			AssertType<CusTempStorageDecFetchStrategy>(storageDec.FetchStrategy);
		}

		public void TestDelete()
		{
			var storageDec = Factory.New<CusTempStorageDec>();
			var storageLine = storageDec.CusTempStorageLines.AddNew();
			var cusEntryNum = CusEntryNumber.New(storageDec, "AAA", Core.Constants.CountryCodes.EuropeanUnion);
			storageDec.Delete();
			Assert(storageLine.IsDeleted);
			Assert(cusEntryNum.IsDeleted);
		}

		public void TestMessages()
		{
			var storageDec = (CusTempStorageDec)GetNewBusinessObject(Factory);
			var message = storageDec.Messages.AddNew();
			AssertEquals(storageDec, message.EM_LinkedObject);
			AssertEquals(storageDec.TableName, message.EM_LinkTable);
			AssertEquals(storageDec.PK, message.EM_LinkUniqueID);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals(true, message.ReadOnly);
		}

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

			var fromJob = factory.New<CusTempStorageJobHeader>();
			fromJob.SJH_GB = GlbBranch.CurrentBranch.PK;
			fromJob.SJH_JobReference = "From1";
			fromJob.SJH_OH_Customer = customer.PK;
			fromJob.SJH_OA_Presenter = presenter.PK;
			fromJob.SJH_OA_Representative = representative.PK;

			var fromDec = fromJob.CusTempStorageDecs.AddNew();
			fromDec.STH_SJH = fromJob.PK;
			fromDec.STH_DeclarationType = "A";

			return fromDec;
		}
	}
}
