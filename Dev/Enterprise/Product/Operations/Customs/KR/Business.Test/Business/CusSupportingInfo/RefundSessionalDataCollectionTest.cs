using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
  [TestedType(typeof(RefundSessionalDataCollection))]
  sealed class RefundSessionalDataCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<RefundSessionalData>
	{
		public void TestConstructors()
		{
			RefundSessionalDataCollection collection = null;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			AssertNoExceptionThrown(() => collection = new RefundSessionalDataCollection(instruction, amendmentSessionalData.PK));
			var refundSessionalData = collection.AddNew();
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, refundSessionalData.CSI_ParentTableCode);
			AssertEquals(CusSupportingInfoTypeList.Codes._5UL, refundSessionalData.CSI_Type);
			AssertEquals(false, refundSessionalData.CSI_CSI_SupportingInfo.IsEmpty);

			AssertNoExceptionThrown(() => collection = new RefundSessionalDataCollection(instruction));
			refundSessionalData = collection.AddNew();
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, refundSessionalData.CSI_ParentTableCode);
			AssertEquals(CusSupportingInfoTypeList.Codes._5UL, refundSessionalData.CSI_Type);
			AssertEquals(true, refundSessionalData.CSI_CSI_SupportingInfo.IsEmpty);
		}
		protected override CusSupportingInfoCollection<RefundSessionalData> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			return new RefundSessionalDataCollection(instruction, amendmentSessionalData.PK);
		}
	}
}
