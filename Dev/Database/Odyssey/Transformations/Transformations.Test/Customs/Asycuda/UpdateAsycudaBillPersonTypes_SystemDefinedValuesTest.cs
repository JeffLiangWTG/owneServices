using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Asycuda;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Customs.Asycuda.Testing
{
	[TestedType(typeof(UpdateAsycudaBillPersonTypes_SystemDefinedValues))]
	sealed class UpdateAsycudaBillPersonTypes_SystemDefinedValuesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateAsycudaBillPersonTypes_SystemDefinedValues();

		protected override void PrepareTestData()
		{
			var sql = @"
DECLARE
	@GC_PK UNIQUEIDENTIFIER = NEWID(),
	@GB_PK UNIQUEIDENTIFIER = NEWID(),
	@AMA_PK UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.GlbCompany(
	GC_PK,
	GC_RN_NKCountryCode,
	GC_RX_NKLocalCurrency,
	GC_Code,
	GC_Name,
	GC_SystemCreateTimeUtc,
	GC_SystemCreateUser,
	GC_SystemLastEditTimeUtc,
	GC_SystemLastEditUser)
VALUES
	(
		@GC_PK,
		'DE',
		'EUR',
		'CO1',
		'DE company',
		'2024-04-01 07:34:56.000',
		'E',
		'2024-04-01 08:12:34.000',
		'E'
	)

INSERT INTO dbo.GlbBranch(
	GB_PK,
	GB_GC,
	GB_Code,
	GB_SystemCreateTimeUtc,
	GB_SystemCreateUser,
	GB_SystemLastEditTimeUtc,
	GB_SystemLastEditUser)
VALUES
	(
		@GB_PK,
		@GC_PK,
		'BR1',
		'2024-01-01 07:34:56.000',
		'E',
		'2024-04-01 08:12:34.000',
		'E'
	)

INSERT INTO dbo.AsycudaManifestHeader(
	AMA_PK,
	AMA_GB,
	AMA_ClusterKey,
	AMA_ApplicationCode,
	AMA_JobReference,
	AMA_RN_NKCountry,
	AMA_IsActive,
	AMA_SystemCreateTimeUtc,
	AMA_SystemCreateUser,
	AMA_SystemLastEditTimeUtc,
	AMA_SystemLastEditUser)
VALUES
	(
		@AMA_PK,
		@GB_PK,
		123,
		'VOC',
		'REF123',
		'DE',
		1,
		'2024-05-30 07:34:56.000',
		'E',
		'2024-05-30 08:12:34.000',
		'E'
	)

INSERT INTO dbo.AsycudaBill(
	ABL_PK,
	ABL_AMA,
	ABL_ClusterKey,
	ABL_ShipperLocalState,
	ABL_ShipperState,
	ABL_ConsigneeLocalState,
	ABL_ConsigneeState,
	ABL_NotifyPartyLocalState,
	ABL_NotifyPartyState,
	ABL_RN_NKShipperCountry,
	ABL_RN_NKConsigneeCountry,
	ABL_RN_NKNotifyPartyCountry,
	ABL_RX_NKOtherDeductionsCurrency,
	ABL_IsActive,
	ABL_SystemCreateTimeUtc,
	ABL_SystemCreateUser,
	ABL_SystemLastEditTimeUtc,
	ABL_SystemLastEditUser)
VALUES
	(
		@ABL_PK1,
		@AMA_PK,
		123,
		N'CA',
		N'CA',
		N'HE',
		N'HE',
		N'HE',
		N'HE',
		'US',
		'DE',
		'DE',
		'USD',
		1,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	),
	(	@ABL_PK2,
		@AMA_PK,
		123,
		N'CA',
		N'CA',
		N'HE',
		N'HE',
		N'HE',
		N'HE',
		'US',
		'DE',
		'DE',
		'USD',
		1,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	)

INSERT INTO dbo.GenCustomAddOnValue(
	XV_PK,
	XV_ParentTableCode,
	XV_ParentID,
	XV_Name,
	XV_Type,
	XV_Data,
	XV_IsRuleEnabled,
	XV_AutoVersion,
	XV_SystemCreateTimeUtc,
	XV_SystemCreateUser,
	XV_SystemLastEditTimeUtc,
	XV_SystemLastEditUser)
VALUES
	(
		NEWID(),
		'ABL',
		@ABL_PK1,
		'ShipperPersonType',
		'STR',
		@ShipperPersonType1,
		1,
		0,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	),
	(
		NEWID(),
		'ABL',
		@ABL_PK1,
		'ConsigneePersonType',
		'STR',
		@ConsigneePersonType1,
		1,
		0,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	),
	(
		NEWID(),
		'ABL',
		@ABL_PK1,
		'NotifyPartyPersonType',
		'STR',
		@NotifyPartyPersonType1,
		1,
		0,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	),
	(
		NEWID(),
		'ABL',
		@ABL_PK2,
		'ShipperPersonType',
		'STR',
		@ShipperPersonType2,
		1,
		0,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	),
	(
		NEWID(),
		'ABL',
		@ABL_PK2,
		'ConsigneePersonType',
		'STR',
		@ConsigneePersonType2,
		1,
		0,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	),
	(
		NEWID(),
		'ABL',
		@ABL_PK2,
		'NotifyPartyPersonType',
		'STR',
		@NotifyPartyPersonType2,
		1,
		0,
		'2024-05-30 07:42:00.000',
		'E',
		'2024-05-31 07:45:00.000',
		'E'
	)
";

			using var cmd = Db.Connection.Command(sql);

			cmd.AddParameter("@ABL_PK1", SqlDbType.UniqueIdentifier, ABL_PK1);
			cmd.AddParameter("@ShipperPersonType1", SqlDbType.NVarChar, ShipperPersonType1);
			cmd.AddParameter("@ConsigneePersonType1", SqlDbType.NVarChar, ConsigneePersonType1);
			cmd.AddParameter("@NotifyPartyPersonType1", SqlDbType.NVarChar, NotifyPartyPersonType1);

			cmd.AddParameter("@ABL_PK2", SqlDbType.UniqueIdentifier, ABL_PK2);
			cmd.AddParameter("@ShipperPersonType2", SqlDbType.NVarChar, ShipperPersonType2);
			cmd.AddParameter("@ConsigneePersonType2", SqlDbType.NVarChar, ConsigneePersonType2);
			cmd.AddParameter("@NotifyPartyPersonType2", SqlDbType.NVarChar, NotifyPartyPersonType2);

			_ = cmd.ExecuteNonQuery();
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("Bill 1: ShipperPersonType system-defined value created", ShipperPersonType1, GetSystemDefinedValue(ABL_PK1, "ShipperPersonType"));
			AssertEquals("Bill 1: ConsigneePersonType system-defined value created", ConsigneePersonType1, GetSystemDefinedValue(ABL_PK1, "ConsigneePersonType"));
			AssertEquals("Bill 1: NotifyPartyPersonType system-defined value created", NotifyPartyPersonType1, GetSystemDefinedValue(ABL_PK1, "NotifyPartyPersonType"));

			AssertEquals("Bill 2: ShipperPersonType system-defined value created", ShipperPersonType2, GetSystemDefinedValue(ABL_PK2, "ShipperPersonType"));
			AssertEquals("Bill 2: ConsigneePersonType system-defined value created", ConsigneePersonType2, GetSystemDefinedValue(ABL_PK2, "ConsigneePersonType"));
			AssertEquals("Bill 2: NotifyPartyPersonType system-defined value created", NotifyPartyPersonType2, GetSystemDefinedValue(ABL_PK2, "NotifyPartyPersonType"));

			AssertEquals("Bill 1: ShipperPersonType user-defined value removed", expected: false, actual: UserDefinedValueExists(ABL_PK1, "ShipperPersonType"));
			AssertEquals("Bill 1: ConsigneePersonType user-defined value removed", expected: false, actual: UserDefinedValueExists(ABL_PK1, "ConsigneePersonType"));
			AssertEquals("Bill 1: NotifyPartyPersonType user-defined value removed", expected: false, actual: UserDefinedValueExists(ABL_PK1, "NotifyPartyPersonType"));

			AssertEquals("Bill 2: ShipperPersonType user-defined value removed", expected: false, actual: UserDefinedValueExists(ABL_PK2, "ShipperPersonType"));
			AssertEquals("Bill 2: ConsigneePersonType user-defined value removed", expected: false, actual: UserDefinedValueExists(ABL_PK2, "ConsigneePersonType"));
			AssertEquals("Bill 2: NotifyPartyPersonType user-defined value removed", expected: false, actual: UserDefinedValueExists(ABL_PK2, "NotifyPartyPersonType"));
		}

		string GetSystemDefinedValue(Guid ablPk, string name) => Db.Connection.ExecuteScalar<string>(
			$"SELECT XA_Data FROM dbo.GenAddOnColumn WHERE XA_ParentTableCode = 'ABL' AND XA_ParentID = '{ablPk}' AND XA_Name = '{name}'");

		bool UserDefinedValueExists(Guid ablPk, string name) => Db.Connection.ExecuteScalar<int>(
			$"SELECT COUNT(1) FROM dbo.GenCustomAddOnValue WHERE XV_ParentTableCode = 'ABL' AND XV_ParentID = '{ablPk}' AND XV_Name = '{name}'") > 0;

		readonly Guid ABL_PK1 = Guid.NewGuid();
		const string ShipperPersonType1 = "1";
		const string ConsigneePersonType1 = "2";
		const string NotifyPartyPersonType1 = "3";
		readonly Guid ABL_PK2 = Guid.NewGuid();
		const string ShipperPersonType2 = "2";
		const string ConsigneePersonType2 = "3";
		const string NotifyPartyPersonType2 = "1";
	}
}
