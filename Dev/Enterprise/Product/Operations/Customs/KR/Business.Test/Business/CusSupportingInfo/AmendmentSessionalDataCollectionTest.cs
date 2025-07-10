using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(AmendmentSessionalDataCollection))]
	sealed class AmendmentSessionalDataCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<AmendmentSessionalData>
	{
		public void TestConstructors()
		{
			AmendmentSessionalDataCollection collection = null;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertNoExceptionThrown(() => collection = new AmendmentSessionalDataCollection(instruction));
			var amendmentSessionalData = collection.AddNew();
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, amendmentSessionalData.CSI_ParentTableCode);
			AssertEquals(CusSupportingInfoTypeList.Codes._5FE, amendmentSessionalData.CSI_Type);
		}
		protected override CusSupportingInfoCollection<AmendmentSessionalData> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			return new AmendmentSessionalDataCollection(instruction);
		}
	}
}
