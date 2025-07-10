using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.MasterFiles
{
	[TestedType(typeof(PopulateDIQuantityClassification))]
	class PopulateDIQuantityClassificationTest : DataTransformationTestCase
	{
		Guid Guid1 = Guid.NewGuid();
		Guid Guid2 = Guid.NewGuid();
		Guid Guid3 = Guid.NewGuid();

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateDIQuantityClassification();

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			CreateUNDGDataItem(helper, Guid1, "", 1);
			CreateUNDGDataItem(helper, Guid2, "REG", 0);
			CreateUNDGDataItem(helper, Guid3, "LIM", 1);
		}

		void CreateUNDGDataItem(TestDbHelper testDbHelper, Guid recordPK, string quantityClassification, int? isLimitedQuantity)
		{
			testDbHelper.Insert("UNDGDataItem", new
			{
				DI_PK = recordPK,
				DI_QuantityClassification = quantityClassification,
				DI_IsLimitedQuantity = isLimitedQuantity,
				DI_AutoVersion = 2,
				DI_DG = Guid.NewGuid(),
				DI_IsCombustible = 0,
				DI_DGFlashPoint = 0.0m,
				DI_TechnicalName = "",
				DI_IMOClass = "3",
				DI_MPMarinePollutant = "",
				DI_DGVolume = 0.000m,
				DI_UnitOfVolume = "",
				DI_DGWeight = 250.000m,
				DI_UnitOfWeight = "KG",
				DI_PackageCount = 0,
				DI_F3_NKPackType = "CTN",
				DI_OverpackID = "",
				DI_HasOverpack = 0,
				DI_PackingInstructionSection = "",
				DI_IsNotOtherwiseSpecified = 0,
				DI_HazardousWasteCode = "",
				DI_SpecialPermitIssueDate = DateTime.UtcNow.Date,
				DI_SpecialPermitNumber = "",
				DI_IsSalvagePackaging = 0,
				DI_IsResidueLastContained = 0,
				DI_RadioactiveLabelCategory = "",
				DI_MaterialFormDescription = "",
				DI_IsHighwayRouteControlledQuantity = 0,
				DI_IsFissileExcepted = 0,
				DI_IsExclusiveUse = 0,
				DI_CriticalitySafetyIndex = 0.0m,
				DI_RadionuclideElement = "",
				DI_RadionuclideElementSuffix = "",
				DI_RadioactiveMaximumActivity = 0.000m,
				DI_RadioactiveMaximumActivityUnit = "",
				DI_RadioactiveTransportIndex = 0.0m,
				DI_ParentTableCode = "APA",
				DI_ParentID = Guid.NewGuid(),
				DI_ApprovalCertificateType = "",
				DI_ApprovalCertificateIDMark = "",
				DI_NECWeight = 5.2,
				DI_NECWeightUQ = "KG",
				DI_SystemCreateTimeUtc = DateTime.UtcNow,
				DI_SystemCreateUser = "~BP",
				DI_SystemLastEditTimeUtc = DateTime.UtcNow,
				DI_SystemLastEditUser = "~BP"
			});
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, TestConnection.Exists($"FROM UNDGDataItem WHERE DI_PK = '{Guid1}' AND DI_QuantityClassification = 'LIM'"));
			AssertEquals(true, TestConnection.Exists($"FROM UNDGDataItem WHERE DI_PK = '{Guid2}' AND DI_QuantityClassification = 'REG'"));
			AssertEquals(true, TestConnection.Exists($"FROM UNDGDataItem WHERE DI_PK = '{Guid3}' AND DI_QuantityClassification = 'LIM'"));
			AssertEquals(false, TestConnection.Exists($"FROM UNDGDataItem WHERE DI_PK = '{Guid1}' AND DI_QuantityClassification = 'REG'"));
		}
	}
}

