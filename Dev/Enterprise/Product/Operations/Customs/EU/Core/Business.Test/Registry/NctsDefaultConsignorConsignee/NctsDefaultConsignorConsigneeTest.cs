using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(NctsDefaultConsignorConsignee))]
	class NctsDefaultConsignorConsigneeTest : RegistryBusinessObjectTemplateTestCase<NctsDefaultConsignorConsignee>
	{
		public void TestSetLeaveBlankAndValueFromRelationShip()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultConsignorConsignee = (NctsDefaultConsignorConsignee)GetNewBusinessObject();
				nctsDefaultConsignorConsignee.ValueFrom = true;
				nctsDefaultConsignorConsignee.LeaveBlank = true;
				AssertEquals("When ticking LeaveBlank, ValueFrom should be false.", false, nctsDefaultConsignorConsignee.ValueFrom);

				nctsDefaultConsignorConsignee.ValueFrom = true;
				AssertEquals("When ticking ValueFrom, LeaveBlank should be false.", false, nctsDefaultConsignorConsignee.LeaveBlank);
			});
		}

		public void TestConsignorAndConsigneeReadOnly()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultConsignorConsignee = (NctsDefaultConsignorConsignee)GetNewBusinessObject();
				nctsDefaultConsignorConsignee.ValueFrom = false;
				AssertEquals("Consignor should be ReadOnly.", true, nctsDefaultConsignorConsignee.ConsignorInfo.ReadOnly);
				AssertEquals("Consignee should be ReadOnly.", true, nctsDefaultConsignorConsignee.ConsigneeInfo.ReadOnly);

				nctsDefaultConsignorConsignee.ValueFrom = true;
				AssertEquals("Consignee should not be  ReadOnly.", false, nctsDefaultConsignorConsignee.ConsignorInfo.ReadOnly);
				AssertEquals("Consignor should not be  ReadOnly.", false, nctsDefaultConsignorConsignee.ConsigneeInfo.ReadOnly);
			});
		}

		public void TestConsignorAndConsigneedefaultValue()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultConsignorConsignee = (NctsDefaultConsignorConsignee)GetNewBusinessObject();
				nctsDefaultConsignorConsignee.LeaveBlank = true;
				nctsDefaultConsignorConsignee.ValueFrom = true;
				AssertEquals("Consignor should be true.", true, nctsDefaultConsignorConsignee.Consignor);
				AssertEquals("Consignee should be true.", true, nctsDefaultConsignorConsignee.Consignee);

				nctsDefaultConsignorConsignee.ValueFrom = false;
				AssertEquals("Consignor should be false.", false, nctsDefaultConsignorConsignee.Consignor);
				AssertEquals("Consignee should be false.", false, nctsDefaultConsignorConsignee.Consignee);
			});
		}

		public void TestConsignorAndConsigneeRelationShipWithValueFrom()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultConsignorConsignee = (NctsDefaultConsignorConsignee)GetNewBusinessObject();
				nctsDefaultConsignorConsignee.LeaveBlank = true;
				nctsDefaultConsignorConsignee.ValueFrom = true;
				nctsDefaultConsignorConsignee.Consignor = false;
				AssertEquals("ValueFrom should be still true.", true, nctsDefaultConsignorConsignee.ValueFrom);

				nctsDefaultConsignorConsignee.Consignee = false;
				AssertEquals("ValueFrom should be false.", false, nctsDefaultConsignorConsignee.ValueFrom);

				nctsDefaultConsignorConsignee.ValueFrom = true;
				nctsDefaultConsignorConsignee.Consignee = false;
				AssertEquals("ValueFrom should be still true.", true, nctsDefaultConsignorConsignee.ValueFrom);

				nctsDefaultConsignorConsignee.Consignor = false;
				AssertEquals("ValueFrom should be false.", false, nctsDefaultConsignorConsignee.ValueFrom);
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override NctsDefaultConsignorConsignee GetBusinessObjectToClone() => (NctsDefaultConsignorConsignee)GetNewBusinessObject();

		protected override NctsDefaultConsignorConsignee GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject() => new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
	}
}
