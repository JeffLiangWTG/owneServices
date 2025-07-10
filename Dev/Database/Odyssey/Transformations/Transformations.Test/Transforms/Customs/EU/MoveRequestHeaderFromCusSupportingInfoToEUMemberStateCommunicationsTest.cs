using System;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	[TestedType(typeof(MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunications))]
	class MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunicationsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunications();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [IX_MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunications_AsycudaManifestHeader] ON [dbo].[AsycudaManifestHeader] ([AMA_ManifestType]) INCLUDE ([AMA_ClusterKey], [AMA_PK]) WHERE ([AMA_ManifestType]='ENS') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [IX_MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunications_CusSupportingInfo] ON [dbo].[CusSupportingInfo] ([CSI_Type], [CSI_ParentTableCode]) INCLUDE ([CSI_AdditionalDescription], [CSI_Code], [CSI_Description], [CSI_PK], [CSI_ReferenceNumber], [CSI_ReferenceNumber2], [CSI_RN_NKCountryCode], [CSI_Status], [CSI_SubType], [CSI_UnitOfQuantity2]) WHERE ([CSI_Type]='RQH' AND [CSI_ParentTableCode]='AMA') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void PrepareTestData()
		{
			//create parent (AsyCudaManifestHeader)
			//valid: CSI record with CSI_Type RQH and CSI_ParentTableCode AMA and CSI_ParentID (exists)
			//valid: CSI record with CSI_Type RQH and CSI_ParentTableCode AMA and CSI_ParentID (not exists)
			//invalid: CSI record with CSI_Type RQH and CSI_ParentTableCode not AMA 
			//invalid: CSI record with CSI_Type RQI and CSI_ParentTableCode AMA and CSI_ParentID (exists)
			//invalid: CSI record with CSI_Type SUP and CSI_ParentTableCode AMA and CSI_ParentID (exists)
			//invalid: CSI record with CSI_Type RQR and CSI_ParentTableCode AMA and CSI_ParentID (exists)
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, EUMemberStateCommunicationSchema.Constants.SqlSchemaName, EUMemberStateCommunicationSchema.Constants.TableName, "Constraint_EUS_ClusterKey"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, EUMemberStateCommunicationSchema.Constants.SqlSchemaName, EUMemberStateCommunicationSchema.Constants.TableName, "Constraint_EUS_Identifier"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, EUMemberStateCommunicationSchema.Constants.SqlSchemaName, EUMemberStateCommunicationSchema.Constants.TableName, "Constraint_EUS_ParentTableCode"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, EUMemberStateCommunicationSchema.Constants.SqlSchemaName, EUMemberStateCommunicationSchema.Constants.TableName, "Constraint_EUS_Status"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, EUMemberStateCommunicationSchema.Constants.SqlSchemaName, EUMemberStateCommunicationSchema.Constants.TableName, "Constraint_EUS_SystemCreateUser"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, EUMemberStateCommunicationSchema.Constants.SqlSchemaName, EUMemberStateCommunicationSchema.Constants.TableName, "Constraint_EUS_SystemLastEditUser"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, EUMemberStateCommunicationSchema.Constants.SqlSchemaName, EUMemberStateCommunicationSchema.Constants.TableName, "Constraint_EUS_Type"))
			{
				var sql = $@"
				DECLARE	@GC_PK				UNIQUEIDENTIFIER = '{GC_PK}',
						@GB_PK				UNIQUEIDENTIFIER = '{GB_PK}',
						@AMA_PK1			UNIQUEIDENTIFIER = '{AMA_PK1}',
						@AMA_PK2			UNIQUEIDENTIFIER = '{AMA_PK2}',
						@NONAMA_PK			UNIQUEIDENTIFIER = '{NONAMA_PK}',
						@CSI_PK1			UNIQUEIDENTIFIER = '{CSI_PK1}',
						@CSI_PK2			UNIQUEIDENTIFIER = '{CSI_PK2}',
						@CSI_PK3			UNIQUEIDENTIFIER = '{CSI_PK3}',
						@CSI_PK4			UNIQUEIDENTIFIER = '{CSI_PK4}',
						@CSI_PK5			UNIQUEIDENTIFIER = '{CSI_PK5}',
						@CSI_PK6			UNIQUEIDENTIFIER = '{CSI_PK6}',
						@CSI_PK7			UNIQUEIDENTIFIER = '{CSI_PK7}',
						@CSI_PK8			UNIQUEIDENTIFIER = '{CSI_PK8}'

				DELETE FROM GlbCompany WHERE GC_PK = @GC_PK
				DELETE FROM GlbBranch WHERE GB_PK = @GB_PK
				DELETE FROM AsycudaManifestHeader WHERE AMA_PK IN(@AMA_PK1, @AMA_PK2)
				DELETE FROM CusInBondHeader WHERE BH_PK = @NONAMA_PK
				DELETE FROM CusSupportingInfo WHERE CSI_PK IN (@CSI_PK1, @CSI_PK2, @CSI_PK3, @CSI_PK4, @CSI_PK5, @CSI_PK6, @CSI_PK7, @CSI_PK8)
				DELETE FROM EUMemberStateCommunication

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemCreateUser, GC_SystemLastEditUser)
				VALUES (@GC_PK, 'DE', 'EUR', 'DDE', 'DE company', GETUTCDATE(), GETUTCDATE(), 'RT0', 'RT0')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemCreateUser, GB_SystemLastEditUser)
				VALUES (@GB_PK, @GC_PK, 'BRN', GETUTCDATE(), GETUTCDATE(), 'RT0', 'RT0')

				-- Insert new AsycudaManifestHeader record, ICS2, non ICS2
				INSERT INTO AsycudaManifestHeader (AMA_PK, AMA_ApplicationCode, AMA_ClusterKey, AMA_GB, AMA_JobReference, AMA_RN_NKCountry, AMA_ManifestType, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser)
				VALUES
					(@AMA_PK1, 'NVC', 1, @GB_PK, 'REFXXX', 'IT', 'ENS', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0'),
					(@AMA_PK2, 'NVC', 2, @GB_PK, 'REFYYY', 'IT', 'ALM', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0')

				-- Insert new CusInBondHeader record for NonAMA parent
				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser)
				VALUES (@NONAMA_PK, @GB_PK, GetUtcDate(), 'RT0', GetUtcDate(), 'RT0')

				-- Insert new CusSupportingInfo records
	
				INSERT INTO CusSupportingInfo (CSI_PK, CSI_ParentTableCode, CSI_ParentID, CSI_DataModel, CSI_Type, CSI_Code, CSI_SubType, CSI_Description, CSI_ReferenceNumber, CSI_AdditionalDescription,
					CSI_ReferenceNumber2, CSI_RN_NKCountryCode, CSI_Status, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc,
					CSI_SystemLastEditUser, CSI_Quantity, CSI_Quantity2, CSI_Quantity3, CSI_LineNo, CSI_ItemNumber, CSI_PackQty, CSI_UnitOfQuantity2)
				VALUES
				-- CSI record with CSI_Type RQH and CSI_ParentTableCode AMA (ICS2) and CSI_ParentID (exists)
					(@CSI_PK1, 'AMA', @AMA_PK1, '', 'RQH', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, 'Y'),
					(@CSI_PK8, 'AMA', @AMA_PK1, '', 'RQH', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, 'N'),
				-- CSI record with CSI_Type RQH and CSI_ParentTableCode AMA (non ICS2) and CSI_ParentID (exists)
					(@CSI_PK2, 'AMA', @AMA_PK2, '', 'RQH', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, ''),
				-- CSI record with CSI_Type RQH and CSI_ParentTableCode AMA and CSI_ParentID (not exists)
					(@CSI_PK3, 'AMA', @NONAMA_PK, '', 'RQH', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, ''),
				-- CSI record with CSI_Type RQH and CSI_ParentTableCode not AMA
					(@CSI_PK4, 'CEI', @AMA_PK1, 'XX', 'RQH', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, ''),
				-- CSI record with CSI_Type RQI and CSI_ParentTableCode AMA and CSI_ParentID (exists)
					(@CSI_PK5, 'AMA', @AMA_PK1, '', 'RQI', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, ''),
				-- CSI record with CSI_Type SUP and CSI_ParentTableCode AMA and CSI_ParentID (exists)
					(@CSI_PK6, 'AMA', @AMA_PK1, '', 'SUP', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, ''),
				-- CSI record with CSI_Type RQR and CSI_ParentTableCode AMA and CSI_ParentID (exists)
					(@CSI_PK7, 'AMA', @AMA_PK1, '', 'RQR', 'N001', 'REF', 'compl', '123', 'TEST', '123', 'DE', 'AWA', GETUTCDATE(), 'RT0', GETUTCDATE(), 'RT0', 5, 5, 5, 0, 0, 0, '')";

				TestConnection.ExecuteNonQuery(sql);
			}
		}
		Guid GC_PK = Guid.NewGuid();
		Guid GB_PK = Guid.NewGuid();
		Guid AMA_PK1 = Guid.NewGuid();
		Guid AMA_PK2 = Guid.NewGuid();
		Guid NONAMA_PK = Guid.NewGuid();
		Guid CSI_PK1 = Guid.NewGuid();
		Guid CSI_PK2 = Guid.NewGuid();
		Guid CSI_PK3 = Guid.NewGuid();
		Guid CSI_PK4 = Guid.NewGuid();
		Guid CSI_PK5 = Guid.NewGuid();
		Guid CSI_PK6 = Guid.NewGuid();
		Guid CSI_PK7 = Guid.NewGuid();
		Guid CSI_PK8 = Guid.NewGuid();

		protected override void AssertTransformationResults()
		{
			// Validate the existence of the valid RQH record
			var validRQHRecordsCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM EUMemberStateCommunication");
			AssertEquals("Total exactly two records should exist", 2, validRQHRecordsCount);

			// Validate that valid RQH records have been deleted from cussupportinginfo after migrated to EUMemberStateCommunication
			var deletedRQHRecordsCount = TestConnection.ExecuteScalar<int>($@"
				SELECT COUNT(*) FROM CusSupportingInfo
				WHERE CSI_PK IN ('{CSI_PK1}', '{CSI_PK8}')");
			AssertEquals($"Valid RQH records {CSI_PK1} have been deleted from cussupportinginfo after migrated to EUMemberStateCommunication", 0, deletedRQHRecordsCount);

			// Validate that remaining records still exist in cussupportinginfo.
			var nonRQHRecordsCount = TestConnection.ExecuteScalar<int>($@"
				SELECT COUNT(*) FROM CusSupportingInfo
				WHERE CSI_PK IN ('{CSI_PK2}', '{CSI_PK3}', '{CSI_PK4}', '{CSI_PK5}', '{CSI_PK6}', '{CSI_PK7}')");
			AssertEquals(6, nonRQHRecordsCount);

			var validRQHRecords = $@"
				SELECT *
				FROM EUMemberStateCommunication
				WHERE EUS_ParentTableCode = 'AMA' AND EUS_ParentId = '{AMA_PK1}'";

			using var command = TestConnection.Command(validRQHRecords);
			using var reader = command.ExecuteReader();
			int recordsFound = 0;
			while (reader.Read())
			{
				recordsFound++;
				var eusPK = (Guid)reader["EUS_PK"];

				// Retrieve and validate each field
				AssertEquals($"{AMA_PK1} - EUS_Identifier", "N001", reader["EUS_Identifier"]);
				AssertEquals($"{AMA_PK1} - EUS_Type", "REF", reader["EUS_Type"]);
				AssertEquals($"{AMA_PK1} - EUS_MessageElement", "compl", reader["EUS_MessageElement"]);
				AssertEquals($"{AMA_PK1} - EUS_HouseBillNumber", "123", reader["EUS_HouseBillNumber"]);
				AssertEquals($"{AMA_PK1} - EUS_ScreeningMethod", "TEST", reader["EUS_ScreeningMethod"]);
				AssertEquals($"{AMA_PK1} - EUS_TransportDocumentType", "123", reader["EUS_TransportDocumentType"]);
				AssertEquals($"{AMA_PK1} - EUS_MemberState", "DE", reader["EUS_MemberState"]);
				AssertEquals($"{AMA_PK1} - EUS_Status", "AWA", reader["EUS_Status"]);
				var includeScreeningDetails = reader.GetBoolean(reader.GetOrdinal("EUS_IncludeScreeningDetails"));
				if (eusPK == CSI_PK1)
				{
					AssertEquals($"{AMA_PK1} - EUS_IncludeScreeningDetails", true, includeScreeningDetails);
				}
				else if(eusPK == CSI_PK2)
				{
					AssertEquals($"{AMA_PK1} - EUS_IncludeScreeningDetails", false, includeScreeningDetails);
				}
			}
			AssertEquals($"Two records were not found for AMA_PK: {AMA_PK1}", 2, recordsFound);
		}
	}
}
