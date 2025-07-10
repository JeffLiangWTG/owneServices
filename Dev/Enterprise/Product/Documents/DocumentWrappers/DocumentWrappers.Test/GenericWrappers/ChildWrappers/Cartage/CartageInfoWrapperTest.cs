using Direction = Enterprise.DocumentEngineCore.DocumentSupport.DocumentDirection;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class CartageInfoWrapperTest : Base.Testing.GenericWrapperTest
	{
		public void TestOverrideDocumentDirection()
		{
			var wrapper = (CartageInfoWrapper)GetNewDocumentWrapper();
			AssertEquals("Precondition", false, wrapper.IsExportDocument);
			AssertEquals("Precondition", false, wrapper.IsImportDocument);

			wrapper.OverrideDocumentDirection(Direction.ARV);
			AssertEquals(false, wrapper.IsExportDocument);
			AssertEquals(true, wrapper.IsImportDocument);

			wrapper.OverrideDocumentDirection(Direction.DEP);
			AssertEquals(true, wrapper.IsExportDocument);
			AssertEquals(false, wrapper.IsImportDocument);
		}

		public abstract void TestWrapperMappingFull();

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CartageInfo
======================================================================
Name                                    Type
----------------------------------------------------------------------
JourneyOneDeliverToAddress              AddressWithIDocDocAddress
JourneyOnePickUpAddress                 AddressWithIDocDocAddress
JourneyTwoDeliverToAddress              AddressWithIDocDocAddress
JourneyTwoPickUpAddress                 AddressWithIDocDocAddress
AvailableDate                           DateTime
CutOffDate                              DateTime
CutOffOrAvailableDate                   DateTime
EmailSubjectNumber                      String
EquipmentType                           String
FullCartageInstructions                 String
FullHandlingInstructions                String
IsAir                                   Bool
JourneyOneDeliverToContactName          String
JourneyOneDeliverToContactPhone         String
JourneyOneDeliverToDate                 DateTime
JourneyOneDeliverToDateHeading          String
JourneyOneDeliverToHeading              String
JourneyOneDeliverToRequiredByDate       DateTime
JourneyOneDeliverToRequiredByDateHeading  String
JourneyOnePickUpContactName             String
JourneyOnePickUpContactPhone            String
JourneyOnePickUpDate                    DateTime
JourneyOnePickUpDateHeading             String
JourneyOnePickUpHeading                 String
JourneyOnePickUpReleaseNum              String
JourneyOnePickUpRequiredByDate          DateTime
JourneyOnePickUpRequiredByDateHeading   String
JourneyOnePickUpSlofRef                 String
JourneyTwoDeliverToContactName          String
JourneyTwoDeliverToContactPhone         String
JourneyTwoDeliverToDate                 DateTime
JourneyTwoDeliverToDateHeading          String
JourneyTwoDeliverToHeading              String
JourneyTwoDeliverToReleaseNum           String
JourneyTwoDeliverToRequiredByDate       DateTime
JourneyTwoDeliverToRequiredByDateHeading  String
JourneyTwoDeliverToSlofRef              String
JourneyTwoPickUpContactName             String
JourneyTwoPickUpContactPhone            String
JourneyTwoPickUpDate                    DateTime
JourneyTwoPickUpDateHeading             String
JourneyTwoPickUpHeading                 String
JourneyTwoPickUpRequiredByDate          DateTime
JourneyTwoPickUpRequiredByDateHeading   String
LegNotes                                String
PickupOrStorageCommenceDate             DateTime
PickupOrStorageCommenceDateHeading      String
PrintAsContainers                       Bool
PrintJourneyOne                         Bool
PrintJourneyTwo                         Bool
PrintTwoJourneys                        Bool
ReceivalDate                            DateTime
StorageCommenceDate                     DateTime

AddressesWithWareHousing                AddressWrapperWithIDocDocAddressCollection Collection
";
			}
		}
	}
}
