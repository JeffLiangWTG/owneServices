using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	sealed class ForwardingShipmentModuleCustomsFiltersProviderEntryStatusTest : TestCaseWithFactory
	{
		public void TestEntryStatusFilters()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				ZString PrepareShipment(string jobNumber, bool hasDeclaration,
					GlbBranch declBranch,
					string status1 = null, string status2 = null)
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_UniqueConsignRef = jobNumber;
					shipment.JS_CFSReference = "XXXXX";

					shipment.JS_UniqueConsignRef = "Shipment " + jobNumber;
					if (hasDeclaration)
					{
						var declaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
						declaration.JE_JS = shipment.PK;
						declaration.JE_GB = declBranch.PK;
						declaration.JE_GC = declBranch.Company.PK;
						if (status1 is not null)
						{
							var header = Factory.New<Integration.Customs.ICusEntryHeader>();
							header.CH_JE = declaration.PK;
							header.CH_EntryStatus = status1;
						}

						if (status2 is not null)
						{
							var header = Factory.New<Integration.Customs.ICusEntryHeader>();
							header.CH_JE = declaration.PK;
							header.CH_EntryStatus = status2;
						}
					}

					return shipment.JobNumber;
				}

				var current = GlbBranch.CurrentBranch;
				var otherCompany = Factory.New<GlbCompany>();
				var other = otherCompany.Branches.AddNew();
				other.GB_Code = "OTH";
				var testCases =
					new
						Dictionary<ZString, (bool InEqualsEmpty, bool InIsBlank, bool InEqualsXXX, bool InNotEqualsXXX,
							bool InIsNotBlank, bool InEqualsEmptyALL, bool InEqualsXXXALL)>()
						{
							{ PrepareShipment("1", false, null), (true, true, false, true, false, true, false) },
							{ PrepareShipment("2", true, current), (true, true, false, true, false, true, false) },
							{ PrepareShipment("3", true, current, "XXX", "XXX"), (true, false, true, false, true, false, true) },
							{ PrepareShipment("4", true, current, "XXX", ""), (true, true, true, true, true, false, false) },
							{ PrepareShipment("5", true, current, "XXX", "YYY"), (true, false, true, true, true, false, false) },
							// shipment in current company, declaration in other
							{ PrepareShipment("6", true, other), (true, true, false, true, false, true, false) },
							{ PrepareShipment("7", true, other, "XXX", "XXX"), (true, true, false, true, false, true, false) },
							{ PrepareShipment("8", true, other, "XXX", ""), (true, true, false, true, false, true, false) },
							{ PrepareShipment("9", true, other, "XXX", "YYY"), (true, true, false, true, false, true, false) },
						};

				Factory.Save();

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var entryStatusFilter = filterStripBizObj["Customs Entry Status"] as EntryStatusFilter;
				entryStatusFilter.IsActive = true;

				var comparisonOperatorList = entryStatusFilter.ComparisonOperator_List;
				AssertContainsExactElementsInAnyOrder(
					"There should be only 4 elements in CustomsEntryStatus.FilterComparationOperations.",
					new[]
					{
						ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual,
						ModuleTextFilter.ComparisonConstants.IsBlank,
						ModuleTextFilter.ComparisonConstants.IsNotBlank
					}, comparisonOperatorList.GetAllCodes());

				entryStatusFilter.Property = "";
				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				var collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertContainsExactElementsInAnyOrder("InEqualsEmpty",
					testCases.Where(e => e.Value.InEqualsEmpty).Select(e => e.Key),
					collection.Select(s => s.JobNumber));

				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
				collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertContainsExactElementsInAnyOrder("InIsBlank",
					testCases.Where(e => e.Value.InIsBlank).Select(e => e.Key), collection.Select(s => s.JobNumber));

				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertContainsExactElementsInAnyOrder("InIsNotBlank",
					testCases.Where(e => e.Value.InIsNotBlank).Select(e => e.Key), collection.Select(s => s.JobNumber));

				entryStatusFilter.Property = "XXX";
				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertContainsExactElementsInAnyOrder("InEqualsXXX",
					testCases.Where(e => e.Value.InEqualsXXX).Select(e => e.Key), collection.Select(s => s.JobNumber));

				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertContainsExactElementsInAnyOrder("InNotEqualsXXX",
					testCases.Where(e => e.Value.InNotEqualsXXX).Select(e => e.Key),
					collection.Select(s => s.JobNumber));

				entryStatusFilter.FilterType = EntryStatusFilterTypeList.Codes.All;
				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertContainsExactElementsInAnyOrder("InEqualsXXXALL",
					testCases.Where(e => e.Value.InEqualsXXXALL).Select(e => e.Key), collection.Select(s => s.JobNumber));

				entryStatusFilter.Property = "";
				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertContainsExactElementsInAnyOrder("InEqualsEmptyALL",
					testCases.Where(e => e.Value.InEqualsEmptyALL).Select(e => e.Key), collection.Select(s => s.JobNumber));
			}
		}

		FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DummyFilterStripBusinessObject();
		}
	}
}
