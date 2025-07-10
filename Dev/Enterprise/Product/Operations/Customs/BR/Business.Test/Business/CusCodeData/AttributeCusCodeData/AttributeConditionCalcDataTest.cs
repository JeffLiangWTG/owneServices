using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;

namespace Enterprise.Customs.BR.Business.Testing
{
	class AttributeConditionCalcDataTest : TestCaseWithFactory
	{
		public void TestUnitOfMeasureValueList()
		{
			var calcData = new AttributeConditionCalcData(AnswerDataTypes.Boolean, "true") as IUniversalRateCalcData;
			AssertEquals(1m, calcData.UnitOfMeasureValueList["Answer"]);

			calcData = new AttributeConditionCalcData(AnswerDataTypes.Boolean, "false");
			AssertEquals(0m, calcData.UnitOfMeasureValueList["Answer"]);

			calcData = new AttributeConditionCalcData(AnswerDataTypes.String, "true");
			AssertEquals(0m, calcData.UnitOfMeasureValueList["Answer"]);

			calcData = new AttributeConditionCalcData(AnswerDataTypes.Number, "123.123");
			AssertEquals(123.123m, calcData.UnitOfMeasureValueList["Answer"]);

			calcData = new AttributeConditionCalcData(AnswerDataTypes.List, "1");
			AssertEquals(1m, calcData.UnitOfMeasureValueList["Answer"]);

			calcData = new AttributeConditionCalcData(AnswerDataTypes.String, "err");
			AssertEquals(0m, calcData.UnitOfMeasureValueList["Answer"]);
		}
	}
}
