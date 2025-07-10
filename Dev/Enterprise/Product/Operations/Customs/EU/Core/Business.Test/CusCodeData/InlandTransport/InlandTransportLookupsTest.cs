using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class InlandTransportLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList_SEA()
		{
			AssertCY_CodeList(TransportTypeList.Codes.Sea, "10, 11");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_RAI()
		{
			AssertCY_CodeList(TransportTypeList.Codes.Rail, "20, 21");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_ROA()
		{
			AssertCY_CodeList(TransportTypeList.Codes.Road, "30, 31");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_AIR()
		{
			AssertCY_CodeList(TransportTypeList.Codes.Air, "40, 41");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_IWT()
		{
			AssertCY_CodeList(TransportTypeList.Codes.InlandWaterwayTransport, "80, 81");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_FIX()
		{
			AssertCY_CodeList(TransportTypeList.Codes.FixedTransportInstallations, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_OWN()
		{
			AssertCY_CodeList(TransportTypeList.Codes.OwnPropulsion, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_MAI()
		{
			AssertCY_CodeList(TransportTypeList.Codes.Mail, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");
		}

		[ExpectNoExceptions]
		public void TestCY_CodeList_InvalidJE_TransportModeInland()
		{
			AssertCY_CodeList(ZString.Empty, ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestTransportCountryList()
		{
			var transportCountryList = lookups.TransportCountryList;
			NUnit.Framework.Assert.That(transportCountryList, NUnit.Framework.Is.TypeOf<RefCountryCollection>(), "Type");
		}

		[ExpectNoExceptions]
		void AssertCY_CodeList(ZString transportModeInland, ZString expectedCodes)
		{
			declaration.JE_TransportModeInland = transportModeInland;
			CombineAssertions(() =>
			{
				var codeList = lookups.CY_CodeList;
				NUnit.Framework.Assert.That(codeList.CodesAsString, NUnit.Framework.Is.EqualTo(expectedCodes).Using(CustomComparers.TypeComparison), "Valid Codes");
				NUnit.Framework.Assert.That(lookups.CY_CodeList, NUnit.Framework.Is.SameAs(codeList), "Cached");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var inlandTransport = declaration.InlandTransports.AddNew();

			lookups = inlandTransport.Lookups;
		}
		InlandTransportLookups lookups;
		JobDeclaration declaration;
	}
}
