using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.EU
{
	[TestedType(typeof(MoveCusSupportingInfoFromCusInBondHeaderToCusInBondMoveHeader))]
	class MoveCusSupportingInfoFromCusInBondHeaderToCusInBondMoveHeaderTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Change CSI_ParentTableCode 'BH' to 'BM' and update CSI_ParentID with BM_PK (where BH_ApplicationCode_1] ON [dbo].[CusSupportingInfo] ([CSI_ParentID]) INCLUDE ([CSI_CSI_SupportingInfo], [CSI_ParentTableCode], [CSI_SystemCreateTimeUtc], [CSI_SystemLastEditTimeUtc], [CSI_Type]) WHERE ([CSI_Type]='SUP' AND [CSI_ParentTableCode]='BH') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Change CSI_ParentTableCode 'BH' to 'BM' and update CSI_ParentID with BM_PK (where BH_ApplicationCode_2] ON [dbo].[CusInBondHeader] ([BH_PK]) WHERE ([BH_ApplicationCode]='NC5' AND [BH_HeaderType]='D') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		protected override void AssertTransformationResults()
		{
			var sql = $"SELECT COUNT(*) FROM dbo.CusSupportingInfo WHERE CSI_ParentTableCode = 'BM'";
			AssertEquals("Only Parent for NCTS Departure Phase5 should be changed", 1, TestConnection.ExecuteScalar(sql));

			sql = $"SELECT CSI_ParentTableCode FROM dbo.CusSupportingInfo WHERE CSI_PK = '{cusSupportingInfoDepartureP5PK}'";
			AssertEquals("Parent for NCTS Departure Phase5 should be changed", "BM", TestConnection.ExecuteScalar(sql));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new MoveCusSupportingInfoFromCusInBondHeaderToCusInBondMoveHeader();

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @companyPK											UNIQUEIDENTIFIER = NEWID(),
						@branchPK											UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderArrivalP5PK							UNIQUEIDENTIFIER = '{cusInBondHeaderArrivalP5PK}',
						@cusInBondHeaderDepartureP5PK						UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureP5PK}',
						@cusInBondHeaderDepartureP4PK						UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureP4PK}',

						@cusInBondBillPK									UNIQUEIDENTIFIER = '{cusInBondBillPK}',
						@cusSupportingInfoArrivalP5PK						UNIQUEIDENTIFIER = '{cusSupportingInfoArrivalP5PK}',
						@cusSupportingInfoDepartureP5PK						UNIQUEIDENTIFIER = '{cusSupportingInfoDepartureP5PK}',
						@cusSupportingInfoDepartureP4PK						UNIQUEIDENTIFIER = '{cusSupportingInfoDepartureP4PK}',
						@cusSupportingInfoBillPK							UNIQUEIDENTIFIER = '{cusSupportingInfoBillPK}'

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser)	VALUES
					(@companyPK, 'DE', 'EUR', 'DDE', 'DE company', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@branchPK, @companyPK, 'BRN', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@cusInBondHeaderArrivalP5PK,	@branchPK, 'BH01', 'NC5', 'A', '2024-07-30', 1, GetUtcDate(), 'NLK', 'NLK'),
					(@cusInBondHeaderDepartureP5PK,	@branchPK, 'BH02', 'NC5', 'D', '2024-12-15', 1, GetUtcDate(), 'NLK', 'NLK'),
					(@cusInBondHeaderDepartureP4PK, @branchPK, 'BH04', 'NC4', 'D', '2024-03-02', 1, GetUtcDate(), 'NLK', 'NLK')

				INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SubApplicationCode, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate, BM_ValuationDate) VALUES
					(NEWID(),	@cusInBondHeaderArrivalP5PK,	'D', '2024-07-30', 'NLK', GetUtcDate(), 'NLK','2022-07-30','2022-07-30'),
					(NEWID(),	@cusInBondHeaderDepartureP5PK,	'D', '2024-11-15', 'NLK', GetUtcDate(), 'NLK','2022-07-30','2022-07-30'),
					(NEWID(),	@cusInBondHeaderDepartureP5PK,	'D', '2024-07-30', 'NLK', GetUtcDate(), 'NLK','2022-12-15','2022-12-15'),
					(NEWID(),	@cusInBondHeaderDepartureP4PK,	'D', '2024-03-02', 'NLK', GetUtcDate(), 'NLK','2023-03-02','2023-03-02')

				INSERT INTO CusInBondBill (B0_PK, B0_BH, B0_SystemCreateTimeUtc, B0_SystemCreateUser, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser) VALUES
					(@cusInBondBillPK, @cusInBondHeaderDepartureP5PK, '2024-10-10', 'NLK', GetUtcDate(), 'NLK')

				INSERT INTO CusSupportingInfo (CSI_PK, CSI_Type, CSI_ParentTableCode, CSI_ParentID, CSI_SystemCreateTimeUtc, CSI_SystemLastEditTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditUser) VALUES
					(@cusSupportingInfoArrivalP5PK,		'SUP',	'BH', @cusInBondHeaderArrivalP5PK,		'2024-10-11', '2024-10-11 01:01:00', 'NLK', 'NLK'),
					(@cusSupportingInfoDepartureP5PK,	'SUP',  'BH', @cusInBondHeaderDepartureP5PK,	'2024-10-11', '2024-10-11 01:01:00', 'NLK', 'NLK'),
					(@cusSupportingInfoDepartureP4PK,	'SUP',  'BH', @cusInBondHeaderDepartureP4PK,	'2024-10-11', '2024-10-11 01:01:00', 'NLK', 'NLK'),
					(@cusSupportingInfoBillPK,			'SUP',  'B0', @cusInBondBillPK,					'2024-10-11', '2024-10-11 01:01:00', 'NLK', 'NLK')";
			TestConnection.ExecuteNonQuery(sql);
		}
		Guid cusInBondHeaderArrivalP5PK = Guid.NewGuid();
		Guid cusInBondHeaderDepartureP5PK = Guid.NewGuid();
		Guid cusInBondHeaderDepartureP4PK = Guid.NewGuid();

		Guid cusInBondBillPK = Guid.NewGuid();

		Guid cusSupportingInfoArrivalP5PK = Guid.NewGuid();
		Guid cusSupportingInfoDepartureP5PK = Guid.NewGuid();
		Guid cusSupportingInfoDepartureP4PK = Guid.NewGuid();
		Guid cusSupportingInfoBillPK = Guid.NewGuid();
	}
}
