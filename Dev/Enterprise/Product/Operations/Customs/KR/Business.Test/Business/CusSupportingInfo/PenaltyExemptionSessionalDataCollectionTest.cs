using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
  [TestedType(typeof(PenaltyExemptionSessionalDataCollection))]
  sealed class PenaltyExemptionSessionalDataCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PenaltyExemptionSessionalData>
	{
		public void TestConstructors()
		{
			PenaltyExemptionSessionalDataCollection collection = null;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			AssertNoExceptionThrown(() => collection = new PenaltyExemptionSessionalDataCollection(instruction, amendmentSessionalData.PK));
			var penaltyExemptionSessionalData = collection.AddNew();
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, penaltyExemptionSessionalData.CSI_ParentTableCode);
			AssertEquals(CusSupportingInfoTypeList.Codes._5UA, penaltyExemptionSessionalData.CSI_Type);
			AssertEquals(false, penaltyExemptionSessionalData.CSI_CSI_SupportingInfo.IsEmpty);
		}
		protected override CusSupportingInfoCollection<PenaltyExemptionSessionalData> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			return new PenaltyExemptionSessionalDataCollection(instruction, amendmentSessionalData.PK);
		}
	}
}
