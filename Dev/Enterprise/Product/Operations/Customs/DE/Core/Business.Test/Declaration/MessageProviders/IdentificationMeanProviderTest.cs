using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IdentificationMeanProviderTest : Customs.Business.Testing.DataProviderTestCase<IdentificationMeanProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", IdentificationMeanProvider.NewOrNull(null));
		}

		public void TestConstructor()
		{
			var provider = new IdentificationMeanProvider(IdentificationMeansList.Codes.N, "Some description");
			CombineAssertions(() =>
			{
				AssertEquals("Type", IdentificationMeansList.Codes.N, provider.Type);
				AssertEquals("Description", "Some description", provider.Description);
			});
		}

		public void TestType() => AssertEquals(IdentificationMeansList.Codes.S, dataProvider.Type);

		public void TestDescription() => AssertEquals("Description", dataProvider.Description);

		protected override void SetUp()
		{
			base.SetUp();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var identificationMeansCode = entryInstruction.IdentificationMeanCodes.AddNew();
			identificationMeansCode.CY_Code = IdentificationMeansList.Codes.S;
			identificationMeansCode.CY_Data = "Description";
			dataProvider = IdentificationMeanProvider.NewOrNull(entryInstruction.IdentificationMeanCodes.Cast<IdentificationMeansCode>().First());
		}
		IIdentificationMeans dataProvider;

		protected override IdentificationMeanProvider GetProvider() => (IdentificationMeanProvider)dataProvider;
	}
}
