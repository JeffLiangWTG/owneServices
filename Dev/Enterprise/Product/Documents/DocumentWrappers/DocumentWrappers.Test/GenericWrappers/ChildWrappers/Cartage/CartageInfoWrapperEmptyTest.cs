using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CartageInfoWrapperEmpty))]
	sealed class CartageInfoWrapperEmptyTest : CartageInfoWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			TestMappings();
		}

		public override void TestWrapperMappingFull()
		{
			TestMappings();
		}

		void TestMappings()
		{
			CartageInfoWrapper wrapper = (CartageInfoWrapperEmpty)GetNewDocumentWrapper();
			AssertEquals("wrapper.EmailSubjectNumber", "", wrapper.EmailSubjectNumber);
			AssertEquals("wrapper.JourneyOnePickUpHeading", "", wrapper.JourneyOnePickUpHeading);
			AssertEquals("wrapper.JourneyOneDeliverToHeading", "", wrapper.JourneyOneDeliverToHeading);
			AssertEquals("wrapper.JourneyTwoPickUpHeading", "", wrapper.JourneyTwoPickUpHeading);
			AssertEquals("wrapper.JourneyTwoDeliverToHeading", "", wrapper.JourneyTwoDeliverToHeading);
			AssertEquals("wrapper.JourneyOnePickUpAddress", "", wrapper.JourneyOnePickUpAddress.Address);
			AssertEquals("wrapper.JourneyOneDeliverToAddress", "", wrapper.JourneyOneDeliverToAddress.Address);
			AssertEquals("wrapper.JourneyTwoPickUpAddress", "", wrapper.JourneyTwoPickUpAddress.Address);
			AssertEquals("wrapper.JourneyTwoDeliverToAddress", "", wrapper.JourneyTwoDeliverToAddress.Address);
			AssertEquals("wrapper.JourneyOnePickUpContactName", "", wrapper.JourneyOnePickUpContactName);
			AssertEquals("wrapper.JourneyOnePickUpContactPhone", "", wrapper.JourneyOnePickUpContactPhone);
			AssertEquals("wrapper.JourneyOneDeliverToContactName", "", wrapper.JourneyOneDeliverToContactName);
			AssertEquals("wrapper.JourneyOneDeliverToContactPhone", "", wrapper.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapper.JourneyTwoPickUpContactName", "", wrapper.JourneyTwoPickUpContactName);
			AssertEquals("wrapper.JourneyTwoPickUpContactPhone", "", wrapper.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapper.JourneyTwoDeliverToContactName", "", wrapper.JourneyTwoDeliverToContactName);
			AssertEquals("wrapper.JourneyTwoDeliverToContactPhone", "", wrapper.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapper.PrintAsContainers", false, wrapper.PrintAsContainers);
			AssertEquals("wrapper.PrintTwoJourneys", false, wrapper.PrintTwoJourneys);
			AssertEquals("wrapper.EquipmentType", "", wrapper.EquipmentType);
			AssertEquals("wrapper.FullHandlingInstructions", "", wrapper.FullHandlingInstructions);
			AssertEquals("wrapper.FullCartageInstructions", "", wrapper.FullCartageInstructions);
			AssertEquals("wrapper.AddressesWithWareHousing", 0, wrapper.AddressesWithWareHousing.Count);
			AssertEquals("wrapper.CartageAdvice", "", wrapper.CartageAdvice.DateAsUniqueIdentifier);
			AssertNotNull("wrapper.CurrentCompany", wrapper.CurrentCompany);
			AssertEquals("wrapper.IsAir", false, wrapper.IsAir);

			AssertEquals("wrapper.JourneyOnePickUpDate", ZDateTime.Empty, wrapper.JourneyOnePickUpDate);
			AssertEquals("wrapper.JourneyOnePickUpDateHeading", "", wrapper.JourneyOnePickUpDateHeading);
			AssertEquals("wrapper.JourneyOnePickUpRequiredByDate", ZDateTime.Empty, wrapper.JourneyOnePickUpRequiredByDate);
			AssertEquals("wrapper.JourneyOnePickUpRequiredByDateHeading", ZString.Empty, wrapper.JourneyOnePickUpRequiredByDateHeading);
			AssertEquals("wrapper.JourneyOneDeliverToDate", ZDateTime.Empty, wrapper.JourneyOneDeliverToDate);
			AssertEquals("wrapper.JourneyOneDeliverToDateHeading", "", wrapper.JourneyOneDeliverToDateHeading);
			AssertEquals("wrapper.JourneyOneDeliverToRequiredByDate", ZDateTime.Empty, wrapper.JourneyOneDeliverToRequiredByDate);
			AssertEquals("wrapper.JourneyOneDeliverToRequiredByDateHeading", ZString.Empty, wrapper.JourneyOneDeliverToRequiredByDateHeading);
			AssertEquals("wrapper.JourneyTwoPickUpDate", ZDateTime.Empty, wrapper.JourneyTwoPickUpDate);
			AssertEquals("wrapper.JourneyTwoPickUpDateHeading", "", wrapper.JourneyTwoPickUpDateHeading);
			AssertEquals("wrapper.JourneyTwoPickUpRequiredByDate", ZDateTime.Empty, wrapper.JourneyTwoPickUpRequiredByDate);
			AssertEquals("wrapper.JourneyTwoPickUpRequiredByDateHeading", ZString.Empty, wrapper.JourneyTwoPickUpRequiredByDateHeading);
			AssertEquals("wrapper.JourneyTwoDeliverToDate", ZDateTime.Empty, wrapper.JourneyTwoDeliverToDate);
			AssertEquals("wrapper.JourneyTwoDeliverToDateHeading", "", wrapper.JourneyTwoDeliverToDateHeading);
			AssertEquals("wrapper.JourneyTwoDeliverToRequiredByDate", ZDateTime.Empty, wrapper.JourneyTwoDeliverToRequiredByDate);
			AssertEquals("wrapper.JourneyTwoDeliverToRequiredByDateHeading", ZString.Empty, wrapper.JourneyTwoDeliverToRequiredByDateHeading);
			AssertEquals("wrapper.JourneyOnePickUpReleaseNum", "", wrapper.JourneyOnePickUpReleaseNum);
			AssertEquals("wrapper.JourneyOnePickUpSlofRef", "", wrapper.JourneyOnePickUpSlofRef);
			AssertEquals("wrapper.JourneyTwoDeliverToReleaseNum", "", wrapper.JourneyTwoDeliverToReleaseNum);
			AssertEquals("wrapper.JourneyTwoDeliverToSlofRef", "", wrapper.JourneyTwoDeliverToSlofRef);
			AssertEquals("wrapper.LegNotes", "", wrapper.LegNotes);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
JourneyOneDeliverToAddress : 
JourneyOnePickUpAddress : 
JourneyTwoDeliverToAddress : 
JourneyTwoPickUpAddress : 
Registry : (No Default Field Value Available on Registry)";
			}
		}

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CartageInfoWrapperEmpty(Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CartageInfoWrapperEmpty(Factory);
		}

		#endregion
	}
}
