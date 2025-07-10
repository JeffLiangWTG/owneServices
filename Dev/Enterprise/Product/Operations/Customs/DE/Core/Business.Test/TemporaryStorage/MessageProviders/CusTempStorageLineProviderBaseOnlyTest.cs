using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLineProvider))]
	sealed class CusTempStorageLineProviderBaseOnlyTest : CusTempStorageLineProviderAbstractTest<CusTempStorageLineProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CusTempStorageLineProvider(null));
		}

		public void TestEquals()
		{
			var testWrapped = TempStorageLineWrapped;
			var otherWrapped = GetTempStorageLineWrapped();
			Assert(!testWrapped.Equals(null));
			Assert(testWrapped.Equals(otherWrapped));

			Assert(!(testWrapped == null));
			Assert(testWrapped == otherWrapped);

			Assert(testWrapped != null);
			Assert(!(testWrapped != otherWrapped));

			var modifiers = new List<Action<CusTempStorageLine>>
			{
				l => l.TSL_LineNo = 535,
				l => l.TSL_ReferenceNumber = "11DE11111111111116",
				l => l.TSL_ReferenceNumberLine = 979,
				l => l.TSL_ReferenceNumber2 = "ATB150000010320006001",
				l => l.TSL_ReferenceNumber2Line = 588,
				l => l.TSL_UnionStatus = DEUnionStatusList.Codes.C,
				l => l.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB,
				l => l.TSL_OwnerReferenceNumber = "789013",
				l => l.TSL_GoodsDescription = "GOODS DESCRIPTION 1",
				l => l.TSL_GoodsType = DEGoodsTypeList.Codes.A,
				l => l.TSL_GrossWeight = 35.874m,
				l => l.TSL_RN_NKDepartureCountry = Core.Constants.CountryCodes.Afghanistan,
				l => l.TSL_DestinationPlace = "DESTINATION PLACE 1",
				l => l.TSL_LocationOfGoods = "LOC04",
				l => l.TSL_IsFTZ = false,
				l => l.TSL_CustodianIdentifier = "DE1234567891",
				l => l.TSL_CustodianIdentifierBranchNo = "0002",
				l => l.TSL_GoodsOwnerIdentifier = "DE0987654322",
				l => l.TSL_GoodsOwnerIdentifierBranchNo = "0003",
				l => l.TSL_PackageType = "VT",
				l => l.TSL_PackageQty = 934
			};

			foreach (var modifier in modifiers)
			{
				var line = GetTempStorageLineToTest();
				line.TSL_LineNo = 534;
				line.TSL_ReferenceNumber = "11DE11111111111115";
				line.TSL_ReferenceNumberLine = 978;
				line.TSL_ReferenceNumber2 = "ATB150000010320006000";
				line.TSL_ReferenceNumber2Line = 587;
				line.TSL_UnionStatus = DEUnionStatusList.Codes.X;
				line.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ZZZ;
				line.TSL_OwnerReferenceNumber = "789012";
				line.TSL_GoodsDescription = "GOODS DESCRIPTION";
				line.TSL_GoodsType = DEGoodsTypeList.Codes.E;
				line.TSL_GrossWeight = 35.873m;
				line.TSL_RN_NKDepartureCountry = Core.Constants.CountryCodes.FishFromForeignBoats;
				line.TSL_DestinationPlace = "DESTINATION PLACE";
				line.TSL_LocationOfGoods = "LOC03";
				line.TSL_IsFTZ = true;
				line.TSL_CustodianIdentifier = "DE1234567890";
				line.TSL_CustodianIdentifierBranchNo = "0001";
				line.TSL_GoodsOwnerIdentifier = "DE0987654321";
				line.TSL_GoodsOwnerIdentifierBranchNo = "0002";
				line.TSL_PackageType = "VS";
				line.TSL_PackageQty = 937;

				var test1 = new CusTempStorageLineProvider(line);
				var test2 = new CusTempStorageLineProvider(line);
				Assert(test1 == test2);

				modifier.Invoke(line);
				var test3 = new CusTempStorageLineProvider(line);
				Assert(test1 != test3);
			}
		}

		public void TestLineNumber()
		{
			TempStorageLine.TSL_LineNo = 534;
			AssertEquals(534, TempStorageLineWrapped.LineNumber);
		}

		public void TestReferenceNumber()
		{
			TempStorageLine.TSL_ReferenceNumber = "11DE11111111111116";
			AssertEquals("11DE11111111111116", TempStorageLineWrapped.ReferenceNumber);
		}

		public void TestReferenceNumberLine()
		{
			TempStorageLine.TSL_ReferenceNumberLine = 978;
			AssertEquals(978, TempStorageLineWrapped.ReferenceNumberLine);
		}

		public void TestReferenceNumber2()
		{
			TempStorageLine.TSL_ReferenceNumber2 = "ATB150000010320006001";
			AssertEquals("ATB150000010320006001", TempStorageLineWrapped.ReferenceNumber2);
		}

		public void TestReferenceNumber2Line()
		{
			TempStorageLine.TSL_ReferenceNumber2Line = 587;
			AssertEquals(587, TempStorageLineWrapped.ReferenceNumber2Line);
		}

		public void TestUnionStatus()
		{
			TempStorageLine.TSL_UnionStatus = DEUnionStatusList.Codes.X;
			AssertEquals(DEUnionStatusList.Codes.X, TempStorageLineWrapped.UnionStatus);
		}

		public void TestOwnerReferenceType()
		{
			TempStorageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ZZZ;
			AssertEquals(OwnerReferenceTypeList.Codes.ZZZ, TempStorageLineWrapped.OwnerReferenceType);
		}

		public void TestOwnerReferenceNumber()
		{
			TempStorageLine.TSL_OwnerReferenceNumber = "789012";
			AssertEquals("789012", TempStorageLineWrapped.OwnerReferenceNumber);
		}

		public void TestGoodsDescription()
		{
			TempStorageLine.TSL_GoodsDescription = "GOODS DESCRIPTION";
			AssertEquals("GOODS DESCRIPTION", TempStorageLineWrapped.GoodsDescription);
		}

		public void TestGoodsType()
		{
			TempStorageLine.TSL_GoodsType = DEGoodsTypeList.Codes.E;
			AssertEquals(DEGoodsTypeList.Codes.E, TempStorageLineWrapped.GoodsType);
		}

		public void TestGrossWeight_Round()
		{
			TempStorageLine.TSL_GrossWeight = 35.8731m;
			AssertEquals(35.873m, TempStorageLineWrapped.GrossWeight);
		}

		public void TestGrossWeight_TrimSingleTrailingZero()
		{
			TempStorageLine.TSL_GrossWeight = 35.870m;
			AssertEquals("35.87", TempStorageLineWrapped.GrossWeight.ToString());
		}

		public void TestGrossWeight_TrimMultipleTrailingZero()
		{
			TempStorageLine.TSL_GrossWeight = 35.800m;
			AssertEquals("35.8", TempStorageLineWrapped.GrossWeight.ToString());
		}

		public void TestGrossWeight_TrimToInteger()
		{
			TempStorageLine.TSL_GrossWeight = 35.000m;
			AssertEquals("35", TempStorageLineWrapped.GrossWeight.ToString());
		}

		public void TestDepartureCountry()
		{
			TempStorageLine.TSL_RN_NKDepartureCountry = Core.Constants.CountryCodes.FishFromForeignBoats;
			AssertEquals(Core.Constants.CountryCodes.FishFromForeignBoats, TempStorageLineWrapped.DepartureCountry);
		}

		public void TestDestinationPlace()
		{
			TempStorageLine.TSL_DestinationPlace = "DESTINATION PLACE";
			AssertEquals("DESTINATION PLACE", TempStorageLineWrapped.DestinationPlace);
		}

		public void TestLocationOfGoods()
		{
			TempStorageLine.TSL_LocationOfGoods = "LOC03";
			AssertEquals("LOC03", TempStorageLineWrapped.LocationOfGoods);
		}

		public void TestIsFTZ()
		{
			TempStorageLine.TSL_IsFTZ = true;
			Assert(TempStorageLineWrapped.IsFTZ);
		}

		public void TestCustodianEoriNumber()
		{
			TempStorageLine.TSL_CustodianIdentifier = "DE1234567890";
			AssertEquals("DE1234567890", TempStorageLineWrapped.CustodianEoriNumber);
		}

		public void TestCustodianEoriBranch()
		{
			TempStorageLine.TSL_CustodianIdentifierBranchNo = "0001";
			AssertEquals("0001", TempStorageLineWrapped.CustodianEoriBranch);
		}

		public void TestDisposalEntitledTraderEoriNumber()
		{
			TempStorageLine.TSL_GoodsOwnerIdentifier = "DE0987654321";
			AssertEquals("DE0987654321", TempStorageLineWrapped.DisposalEntitledTraderEoriNumber);
		}

		public void TestDisposalEntitledTraderEoriBranch()
		{
			TempStorageLine.TSL_GoodsOwnerIdentifierBranchNo = "0002";
			AssertEquals("0002", TempStorageLineWrapped.DisposalEntitledTraderEoriBranch);
		}

		public void TestPackageType()
		{
			TempStorageLine.TSL_PackageType = "VS";
			AssertEquals("VS", TempStorageLineWrapped.PackageType);
		}

		public void TestPackageQuantity()
		{
			TempStorageLine.TSL_PackageQty = 937;
			AssertEquals(937, TempStorageLineWrapped.PackageQuantity);
		}

		protected override CusTempStorageLine GetTempStorageLineToTest() => Factory.New<CUSPRLCusTempStorageDec>().CusTempStorageLines.AddNew();

		protected override CusTempStorageLineProvider GetTempStorageLineWrapped() => new CusTempStorageLineProvider(TempStorageLine);
	}
}
