using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF409;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TypeOfDutyProvider))]
	sealed class TypeOfDutyProviderTest : TestCase
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NationalCode", "Test_National_Code", provider.NationalCode);
				AssertEquals("UnionCode", "Test_Union_Code", provider.UnionCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TypeOfDutyProvider(
				new TypeOfDutyRf409Type
				{
					NationalCode = "Test_National_Code",
					UnionCode = "Test_Union_Code",
				});
		}

		TypeOfDutyProvider provider;
	}
}
