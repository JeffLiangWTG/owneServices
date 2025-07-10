using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(UpdateCD_TaxRateDescForCusUSClassification))]
	class UpdateCD_TaxRateDescForCusUSClassificationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var message = "Classification{0} should execute DataTransformation, {1}";
			AssertCusUSClassification(string.Format(message, "1", "0.153389 -> 0.1533902"), string.Format(message, "1", "15.3389c/L -> 15.33902c/L"), Classification1, 0.1533902m, "15.33902c/L");
			AssertCusUSClassification(string.Format(message, "2", "0.2826619 -> 0.2826641"), string.Format(message, "2", "28.26619c/WL -> 28.26641c/WL"), Classification2, 0.2826641m, "28.26641c/WL");
			AssertCusUSClassification(string.Format(message, "3", "0.4147469 -> 0.4147501"), string.Format(message, "3", "41.47469c/WL -> 41.47501c/WL"), Classification3, 0.4147501m, "41.47501c/WL");
			AssertCusUSClassification(string.Format(message, "4", "0.8321355 -> 0.832142"), string.Format(message, "4", "83.21355c/WL -> 83.2142c/WL"), Classification4, 0.832142m, "83.2142c/WL");
			AssertCusUSClassification(string.Format(message, "5", "0.898178 -> 0.898185"), string.Format(message, "5", "89.8178c/WL -> 89.8185c/WL"), Classification5, 0.898185m, "89.8185c/WL");
			AssertCusUSClassification(string.Format(message, "6", "0.871761 -> 0.8717678"), string.Format(message, "6", "87.1761c/WL -> 87.17678c/WL"), Classification6, 0.8717678m, "87.17678c/WL");
			AssertCusUSClassification(string.Format(message, "7", "3.566322 -> 3.5663227"), string.Format(message, "7", "$3.566322/PFL -> $3.5663227/PFL"), Classification7, 3.5663227m, "$3.5663227/PFL");
			AssertCusUSClassification(string.Format(message, "8", "3.566322 -> 3.5663227"), string.Format(message, "8", "$3.566322/L -> $3.5663227/L"), Classification8, 3.5663227m, "$3.5663227/L");
			AssertCusUSClassification("Classification9 should not execute DataTransformation", "Classification9 should not execute DataTransformation", Classification9, 0m, "Space");
		}

		void AssertCusUSClassification(string taxRateMessage, string taxRateDescMessage, Guid classificationId, decimal expectedTaxRate, string expectedTaxRateDesc)
		{
			var actualTaxRate = Db.Connection.ExecuteScalar($"SELECT CD_TaxRate FROM dbo.CusUSClassification WHERE CD_PK = '{classificationId}'");
			var actualTaxRateDesc = Db.Connection.ExecuteScalar($"SELECT CD_TaxRateDesc FROM dbo.CusUSClassification WHERE CD_PK = '{classificationId}'");
			AssertEquals(taxRateMessage, expectedTaxRate, actualTaxRate);
			AssertEquals(taxRateDescMessage, expectedTaxRateDesc, actualTaxRateDesc);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCD_TaxRateDescForCusUSClassification();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			Classification1 = CreateClassification("TEST0001", 0.153389m, "15.3389c/L");
			Classification2 = CreateClassification("TEST0002", 0.2826619m, "28.26619c/WL");
			Classification3 = CreateClassification("TEST0003", 0.4147469m, "41.47469c/WL");
			Classification4 = CreateClassification("TEST0004", 0.8321355m, "83.21355c/WL");
			Classification5 = CreateClassification("TEST0005", 0.898178m, "89.8178c/WL");
			Classification6 = CreateClassification("TEST0006", 0.871761m, "87.1761c/WL");
			Classification7 = CreateClassification("TEST0007", 0.3566322m, "$3.566322/PFL");
			Classification8 = CreateClassification("TEST0009", 0.3566322m, "$3.566322/L");
			Classification9 = CreateClassification("TEST0010", 0m, "Space");

			Guid CreateClassification(string partNum, decimal taxRate, string taxRateDesc)
			{
				var part1 = testDataCreator.CreateOrgSupplierPart(partNum);
				var pivot1 = testDataCreator.CreateCusClassPartPivot(part1, "US");
				return testDataCreator.CreateCusUSClassification(pivot1, taxRate, taxRateDesc);
			}
		}
		Guid Classification1;
		Guid Classification2;
		Guid Classification3;
		Guid Classification4;
		Guid Classification5;
		Guid Classification6;
		Guid Classification7;
		Guid Classification8;
		Guid Classification9;

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update CD_TaxRateDesc For CusUSClassification_1] ON [dbo].[CusUSClassification] ([CD_TaxRateDesc]) INCLUDE ([CD_SystemLastEditTimeUtc], [CD_SystemLastEditUser]) WHERE ([CD_TaxRateDesc] IN ('15.3389c/L', '28.26619c/WL', '41.47469c/WL', '83.21355c/WL', '89.8178c/WL', '87.1761c/WL', '$3.566322/PFL', '$3.566322/L')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};
	}
}
