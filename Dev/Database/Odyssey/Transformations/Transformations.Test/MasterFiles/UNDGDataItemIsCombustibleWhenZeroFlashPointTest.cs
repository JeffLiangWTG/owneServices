using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles
{
	[TestedType(typeof(UNDGDataItemIsCombustibleFalseWhenZeroFlashPoint))]
	class UNDGDataItemIsCombustibleWhenZeroFlashPointTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__In Old DG Items make IsCombustible false when flash point is 0.0 else true._1] ON [dbo].[UNDGDataItem] ([DI_IsCombustible], [DI_DGFlashPoint]) INCLUDE ([DI_SystemLastEditTimeUtc], [DI_SystemLastEditUser]) WHERE ([DI_IsCombustible]=(1) AND [DI_DGFlashPoint]=(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__In Old DG Items make IsCombustible false when flash point is 0.0 else true._2] ON [dbo].[UNDGDataItem] ([DI_IsCombustible], [DI_DGFlashPoint]) INCLUDE ([DI_SystemLastEditTimeUtc], [DI_SystemLastEditUser]) WHERE ([DI_IsCombustible]=(0) AND [DI_DGFlashPoint]<>(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO [UNDGDataItem] ([DI_PK], [DI_DGFlashPoint], [DI_IsCombustible], [DI_TechnicalName], [DI_IMOClass], [DI_MPMarinePollutant], [DI_OC_DGContact], [DI_DGVolume], [DI_UnitOfVolume], [DI_DGWeight], [DI_UnitOfWeight], [DI_ParentTableCode], [DI_ParentID], [DI_PackageCount], [DI_F3_NKPackType], [DI_IsLimitedQuantity], [DI_DG], [DI_HasOverpack], [DI_OverpackID], [DI_AutoVersion], [DI_HazardousWasteCode], [DI_IsNotOtherwiseSpecified], [DI_IsResidueLastContained], [DI_IsSalvagePackaging], [DI_PackingInstructionSection], [DI_SpecialPermitIssueDate], [DI_SpecialPermitNumber], [DI_IsExclusiveUse], [DI_IsFissileExcepted], [DI_IsHighwayRouteControlledQuantity], [DI_MaterialFormDescription], [DI_RadioactiveLabelCategory], [DI_RadioactiveMaximumActivity], [DI_RadioactiveMaximumActivityUnit], [DI_RadioactiveTransportIndex], [DI_RadionuclideElement], [DI_RadionuclideElementSuffix], [DI_CriticalitySafetyIndex], [DI_SystemCreateTimeUtc], [DI_SystemCreateUser], [DI_SystemLastEditTimeUtc], [DI_SystemLastEditUser])
VALUES (N'362A6F43-F877-4FA4-B88E-4DC15AD88C71', CAST(-29.0 AS Decimal(8, 1)), 0, N'', N'3', N'', NULL, CAST(0.000 AS Decimal(9, 3)), N'', CAST(0.000 AS Decimal(9, 3)), N'', N'JL', N'dc470ac8-ce2e-4671-894d-b8d432889c69', 0, N'', 0, N'6b5ad1d2-b272-430f-bb3c-de66a779a837', 0, N'', 5, N'', 0, 0, 0, N'', NULL, N'', 0, 0, 0, N'', N'', CAST(0.000 AS Decimal(9, 3)), N'', CAST(0.000 AS Decimal(9, 3)), N'', N'', CAST(0.0 AS Decimal(6, 1)), CAST(N'2022-02-15T13:44:00' AS SmallDateTime), N'E', CAST(N'2022-02-15T13:44:00' AS SmallDateTime), N'E')

INSERT INTO [UNDGDataItem] ([DI_PK], [DI_DGFlashPoint], [DI_IsCombustible], [DI_TechnicalName], [DI_IMOClass], [DI_MPMarinePollutant], [DI_OC_DGContact], [DI_DGVolume], [DI_UnitOfVolume], [DI_DGWeight], [DI_UnitOfWeight], [DI_ParentTableCode], [DI_ParentID], [DI_PackageCount], [DI_F3_NKPackType], [DI_IsLimitedQuantity], [DI_DG], [DI_HasOverpack], [DI_OverpackID], [DI_AutoVersion], [DI_HazardousWasteCode], [DI_IsNotOtherwiseSpecified], [DI_IsResidueLastContained], [DI_IsSalvagePackaging], [DI_PackingInstructionSection], [DI_SpecialPermitIssueDate], [DI_SpecialPermitNumber], [DI_IsExclusiveUse], [DI_IsFissileExcepted], [DI_IsHighwayRouteControlledQuantity], [DI_MaterialFormDescription], [DI_RadioactiveLabelCategory], [DI_RadioactiveMaximumActivity], [DI_RadioactiveMaximumActivityUnit], [DI_RadioactiveTransportIndex], [DI_RadionuclideElement], [DI_RadionuclideElementSuffix], [DI_CriticalitySafetyIndex], [DI_SystemCreateTimeUtc], [DI_SystemCreateUser], [DI_SystemLastEditTimeUtc], [DI_SystemLastEditUser])
VALUES (N'79F8A57D-B9B1-44B0-9CB2-DC3658442A8A', CAST(0.0 AS Decimal(8, 1)), 1, N'', N'3', N'', N'42994ed1-df1c-467e-8ef2-a7786ff86ff4', CAST(0.000 AS Decimal(9, 3)), N'', CAST(0.000 AS Decimal(9, 3)), N'', N'JL', N'eb7779da-6497-4d05-8f30-f0b96eae72b9', 0, N'', 0, N'5f1e5117-804d-491b-8932-e93edd0d7cb7', 0, N'', 7, N'', 0, 0, 0, N'', NULL, N'', 0, 0, 0, N'', N'', CAST(0.000 AS Decimal(9, 3)), N'', CAST(0.000 AS Decimal(9, 3)), N'', N'', CAST(0.0 AS Decimal(6, 1)), CAST(N'2022-03-08T07:31:00' AS SmallDateTime), N'E', CAST(N'2022-03-08T09:28:00' AS SmallDateTime), N'E')
");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, TestConnection.ExecuteScalar("SELECT DI_IsCombustible FROM UNDGDataItem WHERE DI_PK = '362A6F43-F877-4FA4-B88E-4DC15AD88C71'"));
			AssertEquals(false, TestConnection.ExecuteScalar("SELECT DI_IsCombustible FROM UNDGDataItem WHERE DI_PK = '79F8A57D-B9B1-44B0-9CB2-DC3658442A8A'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UNDGDataItemIsCombustibleFalseWhenZeroFlashPoint();
		}
	}
}
