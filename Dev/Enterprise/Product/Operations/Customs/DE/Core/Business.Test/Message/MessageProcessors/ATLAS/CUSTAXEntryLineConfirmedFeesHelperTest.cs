using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business
{
	sealed class CUSTAXEntryLineConfirmedFeesHelperTest : TestCaseWithFactory
	{
		public void TestDeleteEntryLineFees_DeletesFees()
		{
			CUSTAXEntryLineConfirmedFeesHelper.DeleteEntryLineConfirmedFees(baseCusEntryLine);

			CombineAssertions(() =>
			{
				AssertEquals(0, baseCusEntryLine.ConfirmedFees.Count);
				AssertEquals("Fee with CF_Source='CUS' should be deleted", true, fee1.IsDeleted);
				AssertEquals("Fee with CF_Source='CW1' shouldn't be deleted", false, fee2.IsDeleted);
			});
		}

		public void TestCreateEntryLineConfirmedFeesIfLineIsFinal_CreatesFees()
		{
			CUSTAXEntryLineConfirmedFeesHelper.CreateEntryLineConfirmedFeesIfLineIsFinal(baseCusEntryLine, lineMock.Object);
			var mainFee = baseCusEntryLine.ConfirmedFees[1] as CusEntryLineFee;
			var childFee = baseCusEntryLine.ConfirmedFees[2] as CusEntryLineFee;

			CombineAssertions(() =>
			{
				AssertEquals("New fee should be created", 3, baseCusEntryLine.ConfirmedFees.Count);

				AssertEquals("Line 1 - CF_BaseValue", 500m, mainFee.CF_BaseValue);
				AssertEquals("Line 1 - CF_MethodOfCalculation", "1 00", mainFee.CF_MethodOfCalculation);
				AssertEquals("Line 1 - CF_MethodOfPayment", ZString.Empty, mainFee.CF_MethodOfPayment);
				AssertEquals("Line 1 - CF_ChargeAmount", 1000m, mainFee.CF_ChargeAmount);
				AssertEquals("Line 1 - NationalCodeType", "C1234", mainFee.NationalFeeTypeCode);
				AssertEquals("Line 1 - Rate", 10.7m, mainFee.CF_Rate);
				AssertEquals("Line 1 - IsLandedCostOnly = false", false, mainFee.CF_IsLandedCostOnly);

				AssertEquals("Line 2 - CF_MethodOfCalculation", "3 21", childFee.CF_MethodOfCalculation);
				AssertEquals("Line 2 - CF_MethodOfPayment", ZString.Empty, childFee.CF_MethodOfPayment);
				AssertEquals("Line 2 - NationalCodeType", "C1234", childFee.NationalFeeTypeCode);
				AssertEquals("Line 2 - Rate", 10.06m, childFee.CF_Rate);
				AssertEquals("Line 2 - CF_BaseValue", 0m, childFee.CF_BaseValue);
				AssertEquals("Line 2 - CF_ChargeAmount", 0m, childFee.CF_ChargeAmount);
				AssertEquals("Line 2 - IsLandedCostOnly = true", true, childFee.CF_IsLandedCostOnly);
			});
		}

		public void TestCreateEntryLineConfirmedFeesIfLineIsFinal_TrimsChargeType()
		{
			CUSTAXEntryLineConfirmedFeesHelper.CreateEntryLineConfirmedFeesIfLineIsFinal(baseCusEntryLine, lineMock.Object);

			var newFee = baseCusEntryLine.ConfirmedFees.Last() as CusEntryLineFee;
			AssertEquals("C12", newFee.CF_ChargeType);
		}

		public void TestCreateEntryLineConfirmedFeesIfLineIsFinal_InvalidCompletionFlag()
		{
			baseCusEntryLine.ConfirmedFees.RemoveAndDeleteAll();
			lineMock.Setup(e => e.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._3);
			CUSTAXEntryLineConfirmedFeesHelper.CreateEntryLineConfirmedFeesIfLineIsFinal(baseCusEntryLine, lineMock.Object);
			AssertEquals(0, baseCusEntryLine.ConfirmedFees.Count);
		}

		public void TestRequiresProcessingOfEntryLineConfirmedFees()
		{
			var completionFlagsRequiringProcessing = new[]
			{
				ImportCompletionFlagList.Codes._1, ImportCompletionFlagList.Codes._2, ImportCompletionFlagList.Codes._3,
				ImportCompletionFlagList.Codes._4, ImportCompletionFlagList.Codes._5, ImportCompletionFlagList.Codes._6
			}.ToHashSet();

			CombineAssertions(() =>
			{
				foreach (var completionFlag in new ImportCompletionFlagList().GetAllCodes())
				{
					lineMock.Setup(e => e.LineCompletionFlag).Returns(completionFlag);
					AssertEquals($"CompletionFlag {completionFlag}",
						completionFlagsRequiringProcessing.Contains(completionFlag),
						lineMock.Object.RequiresProcessingOfEntryLineConfirmedFees());
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			baseCusEntryLine = Factory.New<CusEntryLine>();
			baseCusEntryLine.CL_LineNumber = 20;

			fee1 = baseCusEntryLine.ConfirmedFees.AddNew() as CusEntryLineFee;

			fee2 = baseCusEntryLine.Fees.AddNew();
			fee2.CF_Source = "CW1";
			var dutyRate1 = MockDutyRate("00", "1", 10.7m);
			var dutyRate2 = MockDutyRate("21", "3", 10.06m);

			dutyLine = new Mock<ICUSTAXLineDuty>();
			dutyLine.Setup(e => e.ChargeAmount).Returns(1000.0m);
			dutyLine.Setup(e => e.ChargeType).Returns("C1234");
			dutyLine.Setup(e => e.BaseValue).Returns(500m);
			dutyLine.Setup(e => e.MethodOfCalculation).Returns("ANYCal");
			dutyLine.Setup(e => e.MethodOfPayment).Returns("PAY");
			dutyLine.Setup(e => e.DutyRates).Returns(new[] { dutyRate1.Object, dutyRate2.Object });

			lineMock = new Mock<ICUSTAXLine>();
			lineMock.Setup(e => e.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._4);
			lineMock.Setup(e => e.LineNumber).Returns("20");
			lineMock.Setup(e => e.Duties).Returns(new List<ICUSTAXLineDuty>()
			{
				dutyLine.Object,
			});

			static Mock<ICUSTAXLineDutyRate> MockDutyRate(string assessmentScale, string criteriaType, decimal rate)
			{
				var dutyRate1 = new Mock<ICUSTAXLineDutyRate>();
				dutyRate1.Setup(r => r.AssessmentScale).Returns(assessmentScale);
				dutyRate1.Setup(r => r.CriteriaType).Returns(criteriaType);
				dutyRate1.Setup(r => r.Rate).Returns(rate);
				return dutyRate1;
			}
		}

		CusEntryLine baseCusEntryLine;
		Mock<ICUSTAXLine> lineMock;
		Mock<ICUSTAXLineDuty> dutyLine;
		CusEntryLineFee fee1;
		CusEntryLineFee fee2;
	}
}












