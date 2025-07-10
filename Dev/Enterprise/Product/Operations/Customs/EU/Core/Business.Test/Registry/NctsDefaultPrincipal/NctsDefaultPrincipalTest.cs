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
	[TestedType(typeof(NctsDefaultPrincipal))]
	class NctsDefaultPrincipalTest : RegistryBusinessObjectTemplateTestCase<NctsDefaultPrincipal>
	{
		public void TestSetLeaveBlankWipeOutPrincipalCode()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultPrincipal = (NctsDefaultPrincipal)GetNewBusinessObject();
				nctsDefaultPrincipal.Principal = ZGuid.BrettsGuid;
				nctsDefaultPrincipal.LeaveBlank = true;
				AssertEquals("When ticking LeaveBlank, Principal", ZGuid.Empty, nctsDefaultPrincipal.Principal);

				nctsDefaultPrincipal.Principal = ZGuid.BrettsGuid;
				nctsDefaultPrincipal.LeaveBlank = false;
				AssertEquals("When unticking LeaveBlank, Principal", ZGuid.BrettsGuid, nctsDefaultPrincipal.Principal);
			});
		}

		public void TestPrincipalCodeReadOnly()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultPrincipal = (NctsDefaultPrincipal)GetNewBusinessObject();
				nctsDefaultPrincipal.LeaveBlank = false;
				AssertEquals("PrincipalInfo ReadOnly", false, nctsDefaultPrincipal.PrincipalInfo.ReadOnly);

				nctsDefaultPrincipal.LeaveBlank = true;
				AssertEquals("PrincipalInfo ReadOnly", true, nctsDefaultPrincipal.PrincipalInfo.ReadOnly);
			});
		}

		public void TestPrincipalOrganization()
		{
			var nctsDefaultPrincipal = (NctsDefaultPrincipal)GetNewBusinessObject();
			nctsDefaultPrincipal.Principal = ZGuid.Empty;
			AssertNull("When PrincipalOrganization is empty, PrincipalOrganization", nctsDefaultPrincipal.PrincipalOrganization);

			nctsDefaultPrincipal.Principal = ZGuid.Invalid;
			AssertNull("When Principal is invalid, PrincipalOrganization", nctsDefaultPrincipal.PrincipalOrganization);

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			nctsDefaultPrincipal.Principal = organization.PK;
			AssertSame("When Principal is valid, PrincipalOrganization", organization, nctsDefaultPrincipal.PrincipalOrganization);
		}

		public void TestPrincipalCollection()
		{
			var nctsDefaultPrincipal = (NctsDefaultPrincipal)GetNewBusinessObject();
			AssertType<OrgHeaderCollection>("PrincipalCollection type", nctsDefaultPrincipal.PrincipalCollection);
		}

		public void TestPrincipalMandatoryValidation()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultPrincipal = (NctsDefaultPrincipal)GetNewBusinessObject();
				nctsDefaultPrincipal.LeaveBlank = false;
				nctsDefaultPrincipal.ValidatePrincipal();
				AssertHasErrorContaining("When LeaveBlank is not ticked and principal is empty", nctsDefaultPrincipal.PrincipalInfo, MandatoryValidation.MustBeEntered);

				nctsDefaultPrincipal.Principal = ZGuid.BrettsGuid;
				AssertNoErrorContaining("When LeaveBlank is not ticked and principal is not empty", nctsDefaultPrincipal.PrincipalInfo, MandatoryValidation.MustBeEntered);

				nctsDefaultPrincipal.LeaveBlank = true;
				nctsDefaultPrincipal.Principal = ZGuid.Empty;
				AssertNoErrorContaining("When LeaveBlank is ticked and principal is empty", nctsDefaultPrincipal.PrincipalInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestPrincipalListValidation()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultPrincipal = (NctsDefaultPrincipal)GetNewBusinessObject();
				nctsDefaultPrincipal.LeaveBlank = false;
				nctsDefaultPrincipal.Principal = ZGuid.Empty;
				AssertNoErrorContaining("When LeaveBlank is not ticked and principal is empty", nctsDefaultPrincipal.PrincipalInfo, ListValidation.InvalidCodeError);

				nctsDefaultPrincipal.Principal = ZGuid.BrettsGuid;
				AssertHasErrorContaining("When LeaveBlank is not ticked and principal is invalid", nctsDefaultPrincipal.PrincipalInfo, ListValidation.InvalidCodeError);

				var organization = Factory.NewWithValidTestData<OrgHeader>();
				nctsDefaultPrincipal.Principal = organization.PK;
				AssertNoErrorContaining("When LeaveBlank is not ticked and principal is valid", nctsDefaultPrincipal.PrincipalInfo, ListValidation.InvalidCodeError);

				nctsDefaultPrincipal.LeaveBlank = true;
				nctsDefaultPrincipal.Principal = ZGuid.BrettsGuid;
				AssertNoErrorContaining("[EDGE-CASE] When LeaveBlank is ticked and principal is invalid", nctsDefaultPrincipal.PrincipalInfo, ListValidation.InvalidCodeError);
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override NctsDefaultPrincipal GetBusinessObjectToClone() => (NctsDefaultPrincipal)GetNewBusinessObject();

		protected override NctsDefaultPrincipal GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject() => new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
	}
}
