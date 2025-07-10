using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDesc))]
	sealed class NctsDepartureCargoDescTest : EU.NCTS.Business.Testing.NctsDepartureCargoDescAbstractTest<NctsHeader>
	{
		public void TestPreviousDocuments()
		{
			var nctsDepartureCargoDesc = GetNewBusinessObject(Factory);
			AssertType<EU.NCTS.Business.NctsPreviousDocumentCollection<NctsPreviousDocument>>(nctsDepartureCargoDesc.PreviousDocuments);
		}

		public void TestValidationType()
		{
			var nctsDepartureCargoDesc = GetNewBusinessObject(Factory);
			AssertType<NctsDepartureCargoDescPhase5Validation>(nctsDepartureCargoDesc.Validation);
		}

		public void TestPackageType()
		{
			var nctsDepartureCargoDesc = GetNewBusinessObject(Factory);
			AssertType<NctsPackage>(nctsDepartureCargoDesc.Packages.AddNew());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override ZString CountryCode => Core.Constants.CountryCodes.Ireland;

		public static NctsDepartureCargoDesc GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var bill = NctsBillTest.GetNewBusinessObject(factory);
			return bill.GoodsItems.AddNew();
		}
	}
}
