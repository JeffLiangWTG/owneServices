using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Moq;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DutyAndTaxAmountDescriptorTest : TestCaseWithFactory
	{
		#region TestGetDescription

		public void TestGetDescriptionWithRemission()
		{
			var combinedDuty = new DutyAndTaxManager.CombinedDuty(DutyAndTaxManager.CombinedDuty.Type.Classification);
			var duty = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AdValorem, 8, CustomsUnitOfMeasureList.Codes.Kilogram, 0.07648);
			combinedDuty.Regulars.Add(duty);
			AssertEquals("Classification duty rate. The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%).", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));
			AssertEquals("The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%).", DutyAndTaxAmountDescriptor.GetDescription(duty));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RepairsRemission);
			AssertEquals("Classification duty rate. 0 (zero) for repairs remission.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));
			AssertEquals("0 (zero) for repairs remission.", DutyAndTaxAmountDescriptor.GetDescription(duty));

			parentMock.Setup(m => m.RulingConfigs).Returns(new[] { Factory.New<CACusRulingConfig>() });
			AssertEquals("Classification duty rate. The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%).", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));
			AssertEquals("The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%).", DutyAndTaxAmountDescriptor.GetDescription(duty));
		}

		public void TestGetDescription()
		{
			//AdValorem
			var testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AdValorem, 8, CustomsUnitOfMeasureList.Codes.Kilogram, 0.07648);
			AssertEquals("The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.ExciseTax, RateTypes.Codes.AdValorem, 21.2, CustomsUnitOfMeasureList.Codes.Litre, 415.52);
			AssertEquals("The normal duty paid value ($1960) is multiplied by the Ad Valorem rate (21.2%).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.GST, RateTypes.Codes.AdValorem, 0.7, CustomsUnitOfMeasureList.Codes.Gram, 16.6012);
			AssertEquals("The normal value for tax ($2371.6) is multiplied by the Ad Valorem rate (0.7%).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.DutyDeferral);
			AssertEquals("The normal value for tax ($2371.6) is multiplied by the Ad Valorem rate (0.7%).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			//Specific
			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.NoRemission);
			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.Specific, 7.77, CustomsUnitOfMeasureList.Codes.Kilogram, 0);
			AssertEquals("Undefined quantity (0KGM) is multiplied by the Specific rate ($7.77/Kilogram).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.ExciseTax, RateTypes.Codes.Specific, 0.2345, CustomsUnitOfMeasureList.Codes.Litre, 70);
			AssertEquals("The quantity of the second unit of measure (666.666LTR) is multiplied by the Specific rate (23.45¢/Litre).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.GST, RateTypes.Codes.Specific, 30.00, CustomsUnitOfMeasureList.Codes.Gram, 70);
			AssertEquals("The quantity of the third unit of measure (0.33333GRM) is multiplied by the Specific rate ($30/Gram).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.Specific, 7.77, string.Empty, 70);
			AssertEquals("The quantity of the first unit of measure (1000KGM) is multiplied by the Specific rate ($7.77/Kilogram).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CustomsUnits).Returns(ZString.Empty); //if without UOM then $100/Piece
			parentMock.Setup(m => m.CustomsQuantity).Returns((ZDecimal)600);
			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.Specific, 1000, string.Empty, 70);
			AssertEquals("The quantity of the first unit of measure (600) is multiplied by the Specific rate ($1000/Piece).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			//AcceptT, AcceptX, Free
			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AcceptT, 0, string.Empty, 0);
			AssertEquals("Accept (T), you must override.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AcceptX, 0, string.Empty, 0);
			AssertEquals("Accept (X), you must override.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.Free, 0, string.Empty, 0);
			AssertEquals("Free.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			//Remission description for duty with ad valorem rate
			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AdValorem, 8, CustomsUnitOfMeasureList.Codes.Kilogram, 0.07648);
			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RepairsRemission);
			AssertEquals("0 (zero) for repairs remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.SoftwareRemission);
			AssertEquals("0 (zero) for software remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.WarrantyRepairsRemission);
			AssertEquals("0 (zero) for warranty repairs remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RegularRemission);
			AssertEquals("0 (zero) for regular remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.OneSixtiethRemission);
			AssertEquals("The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%). The result is divided by 60 and multiplied by the monthly time limit (3).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.DutyDeferral);
			AssertEquals("0 (zero) for duty deferral (60/40) remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			//Remission description for tax with specific rate
			testTax = GetNewTax(DutyAndTaxTypes.Codes.ExciseTax, RateTypes.Codes.Specific, 0.2345, CustomsUnitOfMeasureList.Codes.Litre, 70);
			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RepairsRemission);
			AssertEquals("0 (zero) for repairs remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.WarrantyRepairsRemission);
			AssertEquals("0 (zero) for warranty repairs remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.SoftwareRemission);
			AssertEquals("0 (zero) for software remission.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RegularRemission);
			AssertEquals("The quantity of the second unit of measure (666.666LTR) is multiplied by the Specific rate (23.45¢/Litre).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.GiftsUpTo60);
			AssertEquals("The quantity of the second unit of measure (666.666LTR) is multiplied by the Specific rate (23.45¢/Litre).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.OneSixtiethRemission);
			AssertEquals("The quantity of the second unit of measure (666.666LTR) is multiplied by the Specific rate (23.45¢/Litre). The result is divided by 60 and multiplied by the monthly time limit (3).", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			//Remission description for duty with Accept T rate
			testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AcceptT, 0, string.Empty, 0);
			var remissions = new[] { CalculationMethods.Codes.RepairsRemission, CalculationMethods.Codes.WarrantyRepairsRemission, CalculationMethods.Codes.RegularRemission, CalculationMethods.Codes.OneSixtiethRemission };
			foreach (ZString remission in remissions)
			{
				parentMock.Setup(m => m.CalculationMethod).Returns(remission);
				AssertEquals("Accept (T), you must override.", DutyAndTaxAmountDescriptor.GetDescription(testTax));
			}

			//Remission description for tax with exempt rate
			testTax = GetNewTax(DutyAndTaxTypes.Codes.GST, RateTypes.Codes.Exempt, 0, string.Empty, 0);
			foreach (ZString remission in remissions)
			{
				parentMock.Setup(m => m.CalculationMethod).Returns(remission);
				AssertEquals("Exempt.", DutyAndTaxAmountDescriptor.GetDescription(testTax));
			}

			//Remission description for tax with OIC number 85-2955
			testTax = GetNewTax(DutyAndTaxTypes.Codes.GST, RateTypes.Codes.Specific, 0, string.Empty, 0);
			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RegularRemission);
			parentMock.Setup(m => m.AuthorityNumber).Returns((ZString)"85-2955");
			AssertEquals("0 (zero) for regular remission and OIC number 85-2955.", DutyAndTaxAmountDescriptor.GetDescription(testTax));

			testTax = GetNewTax(DutyAndTaxTypes.Codes.ExciseTax, RateTypes.Codes.Specific, 0, string.Empty, 0);
			AssertEquals("0 (zero) for regular remission and OIC number 85-2955.", DutyAndTaxAmountDescriptor.GetDescription(testTax));
		}

		#endregion

		#region TestGetCombinedDescription

		public void TestGetCombinedDescription()
		{
			var combinedDuty = new DutyAndTaxManager.CombinedDuty(DutyAndTaxManager.CombinedDuty.Type.Classification);
			var duty1 = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AdValorem, 8, CustomsUnitOfMeasureList.Codes.Kilogram, 0.07648);
			combinedDuty.Regulars.Add(duty1);
			AssertEquals("Classification duty rate. The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%).", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			combinedDuty = new DutyAndTaxManager.CombinedDuty(DutyAndTaxManager.CombinedDuty.Type.Tariff);
			var duty2 = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.Specific, 7.77, ZString.Empty, 70);
			combinedDuty.Regulars.Add(duty1);
			combinedDuty.Regulars.Add(duty2);
			AssertEquals("Tariff duty rate. The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%)."
						 + " The quantity of the first unit of measure (1000KGM) is multiplied by the Specific rate ($7.77/Kilogram)."
						 + " The results are added.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			combinedDuty = new DutyAndTaxManager.CombinedDuty(DutyAndTaxManager.CombinedDuty.Type.Excise);
			combinedDuty.Regulars.Add(duty1);
			combinedDuty.Regulars.Add(duty2);
			combinedDuty.Max = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.Specific, 0.0948, CustomsUnitOfMeasureList.Codes.Kilogram, 70);
			AssertEquals("Excise duty rate. The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%)."
						 + " The quantity of the first unit of measure (1000KGM) is multiplied by the Specific rate ($7.77/Kilogram)."
						 + " The results are added."
						 + " But not more than 9.48¢/Kilogram.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			combinedDuty.Min = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AdValorem, 2.2, CustomsUnitOfMeasureList.Codes.Kilogram, 70);
			AssertEquals("Excise duty rate. The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%)."
						 + " The quantity of the first unit of measure (1000KGM) is multiplied by the Specific rate ($7.77/Kilogram)."
						 + " The results are added."
						 + " But not less than 2.2% or not more than 9.48¢/Kilogram.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RepairsRemission);
			AssertEquals("Excise duty rate. 0 (zero) for repairs remission.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.WarrantyRepairsRemission);
			AssertEquals("Excise duty rate. 0 (zero) for warranty repairs remission.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.DutyDeferral);
			AssertEquals("Excise duty rate. 0 (zero) for duty deferral (60/40) remission.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.RegularRemission);
			AssertEquals("Excise duty rate. 0 (zero) for regular remission.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.OneSixtiethRemission);
			AssertEquals("Excise duty rate. The customs value (95.6¢) is multiplied by the Ad Valorem rate (8%)."
						 + " The quantity of the first unit of measure (1000KGM) is multiplied by the Specific rate ($7.77/Kilogram)."
						 + " The results are added."
						 + " But not less than 2.2% or not more than 9.48¢/Kilogram."
						 + " The result is divided by 60 and multiplied by the monthly time limit (3).", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			parentMock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.NoRemission);
			combinedDuty.Regulars.RemoveAt(0);
			combinedDuty.Max = null;
			AssertEquals("Excise duty rate. The quantity of the first unit of measure (1000KGM) is multiplied by the Specific rate ($7.77/Kilogram). But not less than 2.2%.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			//AcceptT, AcceptX, Free
			combinedDuty.Min = null;
			combinedDuty.Regulars.Clear();
			combinedDuty.Regulars.Add(GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.Free, 0, string.Empty, 0));
			AssertEquals("Excise duty rate. Free.", DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty));

			//Test for DutyAndTax.AmountDescription property
			duty1.C1_Override = true;
			AssertEquals("DutyAndTax.AmountDescription", ZString.Empty, duty1.AmountDescription);
		}

		#endregion

		public void TestNoExcetpionThrownWhenGetDescription()
		{
			var testTax = GetNewTax(DutyAndTaxTypes.Codes.CustomsDuty, RateTypes.Codes.AdValorem, 8, CustomsUnitOfMeasureList.Codes.Kilogram, 0.07648);
			testTax.Parent = null;
			AssertNoExceptionThrown(() => DutyAndTaxAmountDescriptor.GetDescription(testTax));
		}

		#region Implementation

		DutyAndTax GetNewTax(ZString dutyAndTaxType, ZString rateType, ZDecimal rate, ZString unitOfMeasure, ZDecimal amount)
		{
			var tax = taxes.AddNew();
			tax.Parent = parentMock.Object;
			tax.C1_TaxType = dutyAndTaxType;
			tax.C1_RateType = rateType;
			tax.C1_Rate = rate;
			tax.C1_UnitOfMeasure = unitOfMeasure;
			tax.C1_Amount = amount;
			return tax;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var mock = new Mock<IDutyAndTaxData>();
			mock.Setup(m => m.NormalDutyPaidValue).Returns((ZDecimal)1960);
			mock.Setup(m => m.NormalValueForTax).Returns((ZDecimal)2371.6);
			mock.Setup(m => m.CalculationMethod).Returns((ZString)CalculationMethods.Codes.NoRemission);
			mock.Setup(m => m.MonthlyTimeLimit).Returns((ZInt)3);
			mock.Setup(m => m.CustomsQuantity).Returns((ZDecimal)1000);
			mock.Setup(m => m.CustomsUnits).Returns((ZString)CustomsUnitOfMeasureList.Codes.Kilogram);
			mock.Setup(m => m.CustomsQuantity2).Returns((ZDecimal)666.666);
			mock.Setup(m => m.CustomsUnits2).Returns((ZString)CustomsUnitOfMeasureList.Codes.Litre);
			mock.Setup(m => m.CustomsQuantity3).Returns((ZDecimal)0.33333);
			mock.Setup(m => m.CustomsUnits3).Returns((ZString)CustomsUnitOfMeasureList.Codes.Gram);
			mock.Setup(m => m.CustomsValue).Returns((ZDecimal)0.956);
			mock.Setup(m => m.IsB3ValidationRequired).Returns((ZBool)true);
			mock.Setup(m => m.AuthorityNumber).Returns(ZString.Empty);
			mock.Setup(m => m.RulingConfigs).Returns(Array.Empty<CACusRulingConfig>());
			parentMock = mock;
			taxes = new DutyAndTaxCollection((JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew());
		}

		DutyAndTaxCollection taxes;
		Mock<IDutyAndTaxData> parentMock;

		#endregion
	}
}
