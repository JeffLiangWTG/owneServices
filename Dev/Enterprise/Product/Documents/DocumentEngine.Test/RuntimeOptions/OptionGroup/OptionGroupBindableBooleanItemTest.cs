using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class OptionGroupBindableBooleanItemTest : IBindableBooleanItemTest
	{
		#region IBindableBooleanItemTest Members

		protected override IBindableBooleanItem GetNewBusinessObject()
		{
			ZBoolDescriptionPair pair = new ZBoolDescriptionPair(testText, false);
			pair.PK = testPK;
			return new OptionGroupBindableBooleanItem(pair);
		}

		public override void TestText()
		{
			AssertEquals(testText, TestBizObj.Text);
		}

		protected override void AssertBoolValueMatchesConcreteFieldValue(ZBool value)
		{
			AssertEquals(value, ((OptionGroupBindableBooleanItem)TestBizObj).BoolValue);
		}

		#endregion

		#region TestIdentifier

		public void TestIdentifier()
		{
			AssertEquals("Identifier should be equal to ZBoolDescriptionPair PK", testPK, TestBizObj.Identifier);
		}

		#endregion

		#region Implementation

		const string testText = "foo";
		readonly ZGuid testPK = new ZGuid(new Guid("583641F6-6EF5-4cc6-BFEE-30D4376B0A42"));

		#endregion
	}
}
