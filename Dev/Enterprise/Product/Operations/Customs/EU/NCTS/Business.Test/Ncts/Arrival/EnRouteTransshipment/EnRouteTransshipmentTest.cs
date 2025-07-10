using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(EnRouteTransshipment))]
	sealed class EnRouteTransshipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICusInBondContainerTypeSupporter()
		{
			var supporter = transhipment as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(NctsContainer), supporter.ContainerType);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusInBondEventTypeList.Codes.Transshipment, transhipment.BN_Type);
		}

		public void TestLookups()
		{
			AssertType<EnRouteTransshipmentLookups>(transhipment.Lookups);
		}

		public void TestValidation()
		{
			AssertType<EnRouteTransshipmentValidation>(transhipment.Validation);
		}

		public void TestContainers()
		{
			CombineAssertions(() =>
			{
				var transshipmentContainer = transhipment.Containers;
				AssertSame("Cached", transshipmentContainer, transhipment.Containers);
				AssertEquals("Register Editable", true, transhipment.IsRegisteredEditableChildObject(transshipmentContainer));
			});
		}

		public void TestContainerType()
		{
			AssertType(((ICusInBondContainerTypeSupporter)transhipment).ContainerType, transhipment.Containers.AddNew());
		}

		public void TestIsInNCTS()
		{
			CombineAssertions(() =>
			{
				transhipment.IsInNCTS = true;
				AssertEquals("Is In Ncts", "SUB", transhipment.BN_CustomsStatus);
				transhipment.IsInNCTS = false;
				AssertEquals("Is Not Ncts", ZString.Empty, transhipment.BN_CustomsStatus);
			});
		}

		public void TestContainersNumbers()
		{
			for (var i = 1; i < 3; i++)
			{
				var container = transhipment.Containers.AddNew();
				container.BC_ContainerNum = "CONT" + i;
			}
			transhipment.Containers.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { "CONT1", "CONT2" }, transhipment.ContainersNumbers);
		}

		public void TestBN_EventCountryCode_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(transhipment.BN_EventCountryCodeInfo, multipleResourceKey: null, "Event Country/Region", "Event Ctry./Rgn.");
		}

		public void TestBN_EndorsementCountryCode_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(transhipment.BN_EndorsementCountryCodeInfo, multipleResourceKey: null, "Country/Region Reported", "Ctry./Rgn. Reported");
		}

		protected override BusinessObject GetNewBusinessObject() => transhipment;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header.EnRouteTransshipments.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			transhipment = header.EnRouteTransshipments.AddNew();
		}
		EnRouteTransshipment transhipment;
	}
}
