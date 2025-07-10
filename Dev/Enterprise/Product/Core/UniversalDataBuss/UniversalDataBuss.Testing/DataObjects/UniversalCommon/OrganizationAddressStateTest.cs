using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(OrganizationAddressState))]
	class OrganizationAddressStateTest : DataObjectTestCase<OrganizationAddressState>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ "Code", Math.Max(OrgAddressSchema.OA_State.MaxLength, JobDocAddressSchema.E2_State.MaxLength) },
				{ "Description", 80 }
			};
		}

		public void TestNew()
		{
			var orgAddressState1 = OrganizationAddressState.New("012", (ZString stateCode) => stateCode + "_Desc");
			AssertEquals("012", orgAddressState1.Code);
			AssertEquals("012_Desc", orgAddressState1.Description);

			var orgAddressState2 = OrganizationAddressState.New("", (ZString stateCode) => stateCode + "_Desc");
			AssertEquals("", orgAddressState2.Code);
			AssertEquals(false, orgAddressState2.Description.HasValue);

			var orgAddressState3 = OrganizationAddressState.New(null, (ZString stateCode) => stateCode + "_Desc");
			AssertNull(orgAddressState3);
		}

		public void TestOperators()
		{
			OrganizationAddressState orgAddressState = null;
			AssertNull(orgAddressState);
			AssertEquals(null, orgAddressState);
			AssertEquals(null, (string)orgAddressState);
			AssertEquals(false, ((ZString?)orgAddressState).HasValue);
			AssertEquals("", ((ZString?)orgAddressState).GetValueOrDefault());

			orgAddressState = (ZString?)null;
			AssertNull(orgAddressState);
			AssertEquals(null, orgAddressState);
			AssertEquals(null, (string)orgAddressState);
			AssertEquals(false, ((ZString?)orgAddressState).HasValue);
			AssertEquals("", ((ZString?)orgAddressState).GetValueOrDefault());

			orgAddressState = new OrganizationAddressState();
			AssertEquals(null, (string)orgAddressState);
			AssertEquals(false, ((ZString?)orgAddressState).HasValue);
			AssertEquals(null, (ZString?)orgAddressState);
			AssertEquals("", ((ZString?)orgAddressState).GetValueOrDefault());

			orgAddressState = "012";
			AssertEquals("012", (string)orgAddressState);
			AssertEquals("012", orgAddressState.ToString());
			AssertEquals(true, ((ZString?)orgAddressState).HasValue);
			AssertEquals("012", ((ZString?)orgAddressState).Value);
			AssertEquals("012", ((ZString?)orgAddressState).GetValueOrDefault());

			ZString? zstringValue = new ZString?("012");
			Assert(orgAddressState == zstringValue);
			Assert(zstringValue == orgAddressState);
			Assert(orgAddressState.Equals(zstringValue));

			string stringValue = "012";
			Assert(orgAddressState == stringValue);
			Assert(stringValue == orgAddressState);
			Assert(orgAddressState.Equals(stringValue));

			var orgAddressStateValue = new OrganizationAddressState() { Code = "012" };
			Assert(orgAddressState == orgAddressStateValue);
			Assert(orgAddressState.Equals(orgAddressStateValue));
		}
	}
}
