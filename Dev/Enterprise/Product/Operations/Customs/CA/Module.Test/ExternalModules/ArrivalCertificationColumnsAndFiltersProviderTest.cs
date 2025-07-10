using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class ArrivalCertificationColumnsAndFiltersProviderTest : ColumnsAndFiltersProviderTest
	{
		public void TestArrivalCertificationReleaseStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				new CACFSShipmentModuleColumnsAndFiltersProvider().AddFilters(filters, Factory);

				var filter = (ModuleTextFilter)filters["Arrival Certification Status"];
				AssertNotNull("Arrival Certification Status filter", filter);
				AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
				filter.IsActive = true;

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "NST";
				var shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionNotContains(shipment4, shipments);
				AssertCollectionNotContains(shipment5, shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "REJ";
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionContains(shipment2, shipments);
				AssertCollectionContains(shipment3, shipments);
				AssertCollectionNotContains(shipment4, shipments);
				AssertCollectionNotContains(shipment5, shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "SNT";
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionContains(shipment4, shipments);
				AssertCollectionContains(shipment5, shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = "NST";
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionContains(shipment2, shipments);
				AssertCollectionContains(shipment3, shipments);
				AssertCollectionContains(shipment4, shipments);
				AssertCollectionContains(shipment5, shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = "REJ";
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionContains(shipment4, shipments);
				AssertCollectionContains(shipment5, shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = "SNT";
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionContains(shipment1, shipments);
				AssertCollectionContains(shipment2, shipments);
				AssertCollectionContains(shipment3, shipments);
				AssertCollectionNotContains(shipment4, shipments);
				AssertCollectionNotContains(shipment5, shipments);
			}
		}

		public void TestArrivalCertificationReleaseDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				new CACFSShipmentModuleColumnsAndFiltersProvider().AddFilters(filters, Factory);

				var filter = (ModuleDateFilter)filters["Arrival Certification Date"];
				AssertNotNull("Arrival Certification Date filter", filter);
				AssertEquals(FilterCategories.Dates, filter.Category);

				filter.PropertySearch = ModuleDateFilter.HasDateEntered;
				var shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionContains(shipment4, shipments);
				AssertCollectionContains(shipment5, shipments);

				filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionContains(shipment1, shipments);
				AssertCollectionContains(shipment2, shipments);
				AssertCollectionContains(shipment3, shipments);
				AssertCollectionNotContains(shipment4, shipments);
				AssertCollectionNotContains(shipment5, shipments);

				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = ZDateTime.UtcNow.AddDays(8);
				filter.Property2 = ZDateTime.Empty;
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionNotContains(shipment4, shipments);
				AssertCollectionContains(shipment5, shipments);

				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = ZDateTime.Empty;
				filter.Property2 = ZDateTime.UtcNow.AddDays(8);
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionContains(shipment4, shipments);
				AssertCollectionNotContains(shipment5, shipments);

				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = ZDateTime.UtcNow;
				filter.Property2 = ZDateTime.UtcNow.AddDays(3);
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionNotContains(shipment4, shipments);
				AssertCollectionNotContains(shipment5, shipments);

				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = ZDateTime.UtcNow;
				filter.Property2 = ZDateTime.UtcNow.AddDays(15);
				shipments = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, shipments);
				AssertCollectionNotContains(shipment2, shipments);
				AssertCollectionNotContains(shipment3, shipments);
				AssertCollectionContains(shipment4, shipments);
				AssertCollectionContains(shipment5, shipments);
			}
		}

		[TestDate(2014, 12, 11)]
		protected override void SetUp()
		{
			base.SetUp();
			var now = ZDateTime.UtcNow;
			shipment1 = Factory.NewWithValidTestData<CFSShipment>();

			shipment2 = Factory.NewWithValidTestData<CFSShipment>();
			AddArrivalCertificationMessages(shipment2, EDIMessage.Status.Rejected, now);

			shipment3 = Factory.NewWithValidTestData<CFSShipment>();
			AddArrivalCertificationMessages(shipment3, EDIMessage.Status.Rejected, now.AddDays(5));
			AddArrivalCertificationMessages(shipment3, EDIMessage.Status.Sent, now);

			shipment4 = Factory.NewWithValidTestData<CFSShipment>();
			AddArrivalCertificationMessages(shipment4, EDIMessage.Status.Rejected, now);
			AddArrivalCertificationMessages(shipment4, EDIMessage.Status.Sent, now.AddDays(5));

			shipment5 = Factory.NewWithValidTestData<CFSShipment>();
			AddArrivalCertificationMessages(shipment5, EDIMessage.Status.Rejected, now);
			AddArrivalCertificationMessages(shipment5, EDIMessage.Status.Sent, now.AddDays(10));

			Factory.Save();

			filters = new ModuleFilterCollection();
		}

		static void AddArrivalCertificationMessages(CFSShipment shipment, ZString status, ZDateTime createTime)
		{
			var message = shipment.Messages.AddNew(typeof(RNSRequestMessage));
			message.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = status;
			message.EM_SystemCreateTimeUtc = createTime;
		}

		CFSShipment shipment1;
		CFSShipment shipment2;
		CFSShipment shipment3;
		CFSShipment shipment4;
		CFSShipment shipment5;
		ModuleFilterCollection filters;
	}
}
