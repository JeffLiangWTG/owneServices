using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Rating
{
	[TestedType(typeof(TruncateQuoteScopeTable))]
	class TruncateQuoteScopeTableTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var disableConstraints = @"
        ALTER TABLE dbo.QuoteScope NOCHECK CONSTRAINT ALL;
    ";
			TestConnection.ExecuteNonQuery(disableConstraints);
			var createPreparedData = @"
    INSERT INTO dbo.QuoteScope (
        QS_PK, QS_AircraftType, QS_ContainerMode, QS_ContractNumber, QS_COS_OpportunityScope, QS_DeliveryAddressPostCode, QS_Destination, 
        QS_FirstLoad, QS_FirstRouteSetLoadPort, QS_Frequency, QS_FrequencyUnit, QS_HBLDeliveryMode, QS_IDNumber, QS_IncoTerm, 
        QS_LastDischarge, QS_LastRouteSetDischargePort, QS_MatchContainerRateClass, QS_OA_DeliveryAddress, QS_OA_PickupAddress, 
        QS_OH_Consignee, QS_OH_Consignor, QS_OH_ControllingCustomer, QS_OH_Supplier, QS_OH_TransportProvider, QS_Origin, QS_PaymentTerm, 
        QS_PickupAddressPostCode, QS_PlannedDischarge, QS_PlannedLoad, QS_PL_NKCarrierServiceLevel, QS_ProductCode, QS_Quantity, 
        QS_RateDestination, QS_RateOrigin, QS_RC_ContainerType, QS_RH_NKCommodityCode, QS_RS_NKServiceLevel, QS_TH_RatingHeader, QS_TransitTime,
        QS_TransportMode, QS_Transshipment, QS_Volume, QS_VolumeUnit, QS_Weight, QS_WeightUnit, QS_SystemCreateTimeUtc, QS_SystemLastEditTimeUtc
    )
    VALUES 
    (
        NEWID(),'CAO', 'ALL', '1', NEWID(), '12345', 'US', 
        'HKHKG', 'NZAKL', 1, 'DAYS', 'DOOR/DOOR', 1, 'FOB', 
        'USLAX', 'USLAX', 0, NEWID(), NEWID(), 
        NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), 'US', 'PPD',
        '54321', 'USLAX', 'USLAX', 'FCL', 'FWD', 10, 
        'AUSYD', 'USLAX', NEWID(), 'CODE', 'STD', NEWID(), '3', 
        'AIR', 'LAX', 100.0, 'M3', 200.0, 'KG', GETDATE(), GETDATE()
    ),
    (
        NEWID(),'CAO', 'ALL', '2', NEWID(), '67890', 'UK', 
        'LHR', 'FRA', 2, 'WEEKS', 'DOOR/DOOR', 2, 'FOB', 
        'GBLHR', 'DEBER', 1, NEWID(), NEWID(), 
        NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), 'UK', 'PPD',
        '98765', 'GBLHR', 'GBLHR', 'FCL', 'AIR', 20, 
        'GBLHR', 'GBLHR', NEWID(), 'CODE', 'STD', NEWID(), '5', 
        'AIR', 'FRA', 500.0, 'M3', 1500.0, 'KG', GETDATE(), GETDATE()
    );
";

			TestConnection.ExecuteNonQuery(createPreparedData);

			var count = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.QuoteScope");
			AssertEquals(2, count);
		}

		protected override void AssertTransformationResults()
		{
			var count = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.QuoteScope");
			AssertEquals(0, count);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new TruncateQuoteScopeTable();
		}
	}
}
