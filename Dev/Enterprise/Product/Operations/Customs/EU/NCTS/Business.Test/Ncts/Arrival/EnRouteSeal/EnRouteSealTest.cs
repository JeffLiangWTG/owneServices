using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(EnRouteSeal))]
	public class EnRouteSealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICusInBondContainerTypeSupporter()
		{
			var supporter = enRouteSeal as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(SealContainer), supporter.ContainerType);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusInBondEventTypeList.Codes.Seal, enRouteSeal.BN_Type);
		}

		public void TestBN_EventCountryCode()
		{
			AssertEquals("Caption is set in the business layer so that validation and UI can use it without setting it twice.", "Event Country", enRouteSeal.BN_EventCountryCodeInfo.HumanReadableName);
		}

		public void TestLookups()
		{
			AssertType<EnRouteSealLookups>(enRouteSeal.Lookups);
		}

		public void TestValidation()
		{
			AssertType<EnRouteSealValidation>(enRouteSeal.Validation);
		}

		public void TestSealContainers()
		{
			CombineAssertions(() =>
			{
				var sealContainers = enRouteSeal.SealContainers;
				AssertEquals("Registered Editable", true, enRouteSeal.IsRegisteredEditableChildObject(sealContainers));
				AssertSame("Cached", sealContainers, enRouteSeal.SealContainers);
			});
		}

		public void TestCusContainerType()
		{
			AssertType(((ICusInBondContainerTypeSupporter)enRouteSeal).ContainerType, enRouteSeal.SealContainers.AddNew());
		}

		public void TestSealContainersSealNumbers()
		{
			for (var i = 1; i < 3; i++)
			{
				var container = enRouteSeal.SealContainers.AddNew();
				container.BC_Seal1 = "SEAL" + i;
			}
			AssertContainsExactElementsInAnyOrder(new[] { "SEAL1", "SEAL2" }, enRouteSeal.SealContainersSealNumbers);
		}

		public void TestSealCollectionFilter()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.EnRouteTransshipments.AddNew();
			AssertEquals("A filter should be in place to load only the seal types of rows. Should not load a transshipment row created above", 0, header.EnRouteSeals.Count);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => enRouteSeal;

		protected override BusinessObject GetNewBusinessObject() => enRouteSeal;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			enRouteSeal = header.EnRouteSeals.AddNew();
		}
		EnRouteSeal enRouteSeal;
	}
}
