using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(NctsDefaultTraderAtDestination))]
	sealed class NctsDefaultTraderAtDestinationTest : RegistryBusinessObjectTemplateTestCase<NctsDefaultTraderAtDestination>
	{
		public void TestSetLeaveBlankWipeOutTraderAtDestinationCode()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultTraderAtDestination = (NctsDefaultTraderAtDestination)GetNewBusinessObject();
				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.BrettsGuid;
				nctsDefaultTraderAtDestination.LeaveBlank = true;
				AssertEquals("When ticking LeaveBlank, TraderAtDestination", ZGuid.Empty, nctsDefaultTraderAtDestination.TraderAtDestination);

				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.BrettsGuid;
				nctsDefaultTraderAtDestination.LeaveBlank = false;
				AssertEquals("When unticking LeaveBlank, TraderAtDestination", ZGuid.BrettsGuid, nctsDefaultTraderAtDestination.TraderAtDestination);
			});
		}

		public void TestTraderAtDestinationCodeReadOnly()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultTraderAtDestination = (NctsDefaultTraderAtDestination)GetNewBusinessObject();
				nctsDefaultTraderAtDestination.LeaveBlank = false;
				AssertEquals("TraderAtDestinationInfo ReadOnly", false, nctsDefaultTraderAtDestination.TraderAtDestinationInfo.ReadOnly);

				nctsDefaultTraderAtDestination.LeaveBlank = true;
				AssertEquals("TraderAtDestinationInfo ReadOnly", true, nctsDefaultTraderAtDestination.TraderAtDestinationInfo.ReadOnly);
			});
		}

		public void TestTraderAtDestinationOrganization()
		{
			var nctsDefaultTraderAtDestination = (NctsDefaultTraderAtDestination)GetNewBusinessObject();
			nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.Empty;
			AssertNull("When TraderAtDestinationOrganization is empty, TraderAtDestinationOrganization", nctsDefaultTraderAtDestination.TraderAtDestinationOrganization);

			nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.Invalid;
			AssertNull("When TraderAtDestination is invalid, TraderAtDestinationOrganization", nctsDefaultTraderAtDestination.TraderAtDestinationOrganization);

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			nctsDefaultTraderAtDestination.TraderAtDestination = organization.PK;
			AssertSame("When TraderAtDestination is valid, TraderAtDestinationOrganization", organization, nctsDefaultTraderAtDestination.TraderAtDestinationOrganization);
		}

		public void TestTraderAtDestinationCollection()
		{
			var nctsDefaultTraderAtDestination = (NctsDefaultTraderAtDestination)GetNewBusinessObject();
			AssertType<OrgHeaderCollection>("TraderAtDestinationCollection type", nctsDefaultTraderAtDestination.TraderAtDestinationCollection);
		}

		public void TestTraderAtDestinationMandatoryValidation()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultTraderAtDestination = (NctsDefaultTraderAtDestination)GetNewBusinessObject();
				nctsDefaultTraderAtDestination.LeaveBlank = false;
				nctsDefaultTraderAtDestination.ValidateTraderAtDestination();
				AssertHasErrorContaining("When LeaveBlank is not ticked and Trader at Destination is empty", nctsDefaultTraderAtDestination.TraderAtDestinationInfo, MandatoryValidation.MustBeEntered);

				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.BrettsGuid;
				AssertNoErrorContaining("When LeaveBlank is not ticked and Trader at Destination is not empty", nctsDefaultTraderAtDestination.TraderAtDestinationInfo, MandatoryValidation.MustBeEntered);

				nctsDefaultTraderAtDestination.LeaveBlank = true;
				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.Empty;
				AssertNoErrorContaining("When LeaveBlank is ticked and Trader at Destination is empty", nctsDefaultTraderAtDestination.TraderAtDestinationInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestTraderAtDestinationListValidation()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultTraderAtDestination = (NctsDefaultTraderAtDestination)GetNewBusinessObject();
				nctsDefaultTraderAtDestination.LeaveBlank = false;
				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.Empty;
				AssertNoErrorContaining("When LeaveBlank is not ticked and Trader at Destination is empty", nctsDefaultTraderAtDestination.TraderAtDestinationInfo, ListValidation.InvalidCodeError);

				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.BrettsGuid;
				AssertHasErrorContaining("When LeaveBlank is not ticked and Trader at Destination is invalid", nctsDefaultTraderAtDestination.TraderAtDestinationInfo, ListValidation.InvalidCodeError);

				var organization = Factory.NewWithValidTestData<OrgHeader>();
				nctsDefaultTraderAtDestination.TraderAtDestination = organization.PK;
				AssertNoErrorContaining("When LeaveBlank is not ticked and Trader at Destination is valid", nctsDefaultTraderAtDestination.TraderAtDestinationInfo, ListValidation.InvalidCodeError);

				nctsDefaultTraderAtDestination.LeaveBlank = true;
				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.BrettsGuid;
				AssertNoErrorContaining("[EDGE-CASE] When LeaveBlank is ticked and Trader at Destination is invalid", nctsDefaultTraderAtDestination.TraderAtDestinationInfo, ListValidation.InvalidCodeError);
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override NctsDefaultTraderAtDestination GetBusinessObjectToClone() => (NctsDefaultTraderAtDestination)GetNewBusinessObject();

		protected override NctsDefaultTraderAtDestination GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject() => new NctsDefaultTraderAtDestination(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
	}
}
