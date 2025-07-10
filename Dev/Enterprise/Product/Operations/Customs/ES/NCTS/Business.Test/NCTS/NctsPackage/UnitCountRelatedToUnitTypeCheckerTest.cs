using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class UnitCountRelatedToUnitTypeCheckerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception when InvoiceLinePackagePivot is null", () => new UnitCountRelatedToUnitTypeChecker(null));
			AssertNoExceptionThrown("No exception when InvoiceLinePackagePivot is valid", () => new UnitCountRelatedToUnitTypeChecker(Factory.New<NctsPackage>()));

			var packagePivotWithNullInfo = Factory.New<NCTSPackageForTest>();
			AssertExceptionThrown<ArgumentNullException>("Exception when UnitCountInfo is null", () => new UnitCountRelatedToUnitTypeChecker(packagePivotWithNullInfo));
		}
	}

	class NCTSPackageForTest : NctsPackage, ICheckablePackage
	{
		public NCTSPackageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ZPropertyInfo ICheckablePackage.UnitCountInfo => null;

		ZString ICheckablePackage.UnitType => throw new NotImplementedException();

		ZString ICheckablePackage.MarksAndNumbers => throw new NotImplementedException();

		ZLong ICheckablePackage.UnitCount => throw new NotImplementedException();

		IEnumerable<ICheckablePackage> ICheckablePackage.GetRelatedEntryPackages()
		{
			throw new NotImplementedException();
		}
	}
}
