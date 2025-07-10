using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class StmMenuItemTest : TestCaseWithFactory
	{
		[StressTestAttribute]
		public void TestAllMenuItemPathsDoNotHaveLeadingOrTrailingSlashes()
		{
			CombineAssertions(delegate()
			{
				foreach (var menuItem in GetAllMenuItems())
				{
					if (menuItem.SU_MenuType != Core.Constants.StmMenuItemTypes.OperationalActions)
					{
						AssertEquals(
							string.Format("Menu Item: [{0}] should not have leading forward slashes.", GetMenuItemDetails(menuItem)),
							false,
							menuItem.SU_MenuPath.StartsWith("/"));

						AssertEquals(
							string.Format("Menu Item: [{0}] should not have leading backward slashes.", GetMenuItemDetails(menuItem)),
							false,
							menuItem.SU_MenuPath.StartsWith("\\"));

						AssertEquals(
							string.Format("Menu Item: [{0}] should not have trailing forward slashes.", GetMenuItemDetails(menuItem)),
							false,
							menuItem.SU_MenuPath.EndsWith("/"));

						AssertEquals(
							string.Format("Menu Item: [{0}] should not have trailing backward slashes.", GetMenuItemDetails(menuItem)),
							false,
							menuItem.SU_MenuPath.EndsWith("\\"));
					}
				}
			});
		}

		[StressTestAttribute]
		public void TestNoLegacyDocumentsHaveDocBuilderConfigurations()
		{
			var results = new List<string>();

			foreach (var menuItem in GetAllLegacyDocuments())
			{
				string legacyMenuItemDetails = GetMenuItemDetails(menuItem);
				if (LegacyDocBuilderDocumentList.Contains(legacyMenuItemDetails))
				{
					continue;
				}
				foreach (StmMenuTemplatePivot templatePivot in menuItem.Documents)
				{
					if (templatePivot.Template.IsSystemDocBuilderStyle)
					{
						results.Add(string.Format("BusinessContext: [{0}] Name: [{1}] Path: [{2}] Filter: [{3}]", menuItem.SU_BusinessContext, menuItem.SU_MenuName, menuItem.SU_MenuPath, menuItem.SU_FilterList));
						break;
					}
				}
			}

			Assert("No documents under the 'Legacy Documents' menu path should be DocBuilder documents." + new ZStringBuilder(results).ToStringWithNewLineBetweenAppends(), results.Count == 0);
		}

		[StressTestAttribute]
		public void TestAllLegacyDocumentsHaveUseDocBuilderFreightDocsFilter()
		{
			CombineAssertions(delegate()
			{
				var freightFilter = "\"<UseDocBuilderFreightDocs>\"!=\"Y\"";
				var warehouseFilter = "\"<UseDocBuilderWarehouseDocsOnly>\"!=\"Y\"";
				var organizationFilter = "\"<UseDocBuilderOrganizationDocs>\"!=\"Y\"";
				var linerAndAgencyFilter = "\"<UseDocBuilderLinerAndAgencyDocs>\"!=\"Y\"";

				foreach (var menuItem in GetAllLegacyDocumentsForFreightExcludingAccountingLegacyDocuments())
				{
					var errorMessage = string.Format(
						"The menu item [{0}] is a legacy document, but does not have [{1}] or [{2}] or [{3}] or [{4}] in the filter.",
						GetMenuItemDetails(menuItem),
						freightFilter, warehouseFilter, organizationFilter, linerAndAgencyFilter);

					var menuFilter = menuItem.SU_FilterList.Replace(" ", "");
					var containsFreightFilter = menuFilter.Contains(freightFilter);
					var containsWarehouseFilter = menuFilter.Contains(warehouseFilter);
					var containOrganizationFilter = menuFilter.Contains(organizationFilter);
					var containLinerAndAgencyFilter = menuFilter.Contains(linerAndAgencyFilter);
					AssertEquals(errorMessage, true, containsFreightFilter || containsWarehouseFilter || containOrganizationFilter || containLinerAndAgencyFilter);
				}
			});
		}

		[StressTestAttribute]
		public void TestAllNonLegacyDocumentsDoNotHaveUseDocBuilderFreightDocsFilter()
		{
			CombineAssertions(delegate()
			{
				var freightFilter = "<UseDocBuilderFreightDocs>";
				var warehouseFilter = "<UseDocBuilderWarehouseDocsOnly>";
				var organizationFilter = "\"<UseDocBuilderOrganizationDocs>\"!=\"Y\"";
				var linerAndAgencyFilter = "\"<UseDocBuilderLinerAndAgencyDocs>\"!=\"Y\"";

				foreach (var menuItem in GetAllNonLegacyDocuments())
				{
					if (!ShouldExclude(menuItem))
					{
						var errorMessage = string.Format(
							"The menu item [{0}] is a production document, but contains [{1}] or [{2}] or [{3}] or [{4}] in the filter.",
							GetMenuItemDetails(menuItem),
							freightFilter, warehouseFilter, organizationFilter, linerAndAgencyFilter);

						var menuFilter = menuItem.SU_FilterList.Replace(" ", "");
						var containsFreightFilter = menuFilter.Contains(freightFilter);
						var containsWarehouseFilter = menuFilter.Contains(warehouseFilter);
						var containOrganizationFilter = menuFilter.Contains(organizationFilter);
						var containLinerAndAgencyFilter = menuFilter.Contains(linerAndAgencyFilter);
						AssertEquals(errorMessage, false, containsFreightFilter || containsWarehouseFilter || containOrganizationFilter || containLinerAndAgencyFilter);
					}
				}
			});
		}

		bool ShouldExclude(StmMenuItem menuItem)
		{
			return
				menuItem.SU_MenuName.Equals("Charge Sheet") &&
				menuItem.SU_BusinessContext.Equals("Consol") &&
				menuItem.SU_MenuPath.Equals("Arrival/Shipment Docs") &&
				menuItem.SU_ContactType.Equals("CNE");
		}

		[StressTestAttribute]
		public void TestMenuItemsThatUseDocBuilderFreightDocsFilterIsUsedOnlyToMarkAsBeingLegacy()
		{
			var filter = "\"<UseDocBuilderFreightDocs>\"!=\"Y\"";
			var menuItems = GetAllMenuItems();

			CombineAssertions(delegate()
			{
				foreach (var menuItem in menuItems)
				{
					if (menuItem.SU_FilterList.Contains("<UseDocBuilderFreightDocs>"))
					{
						var errorMessage = string.Format(
							"The menu item [{0}] uses <UseDocBuilderFreightDocs>, but does not use it as [{1}].",
							GetMenuItemDetails(menuItem),
							filter);

						AssertContains(errorMessage, filter, menuItem.SU_FilterList.Replace(" ", ""));
					}
				}
			});
		}

		[StressTestAttribute]
		public void TestAllLegacyMenuItemsHaveCorrespondingRealMenuItem()
		{
			CombineAssertions(delegate()
			{
				foreach (var menuItem in GetAllMenuItems())
				{
					if (menuItem.SU_MenuPath.StartsWith(Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments))
					{
						string legacyMenuItemDetails = GetMenuItemDetails(menuItem);
						var query = new ZQuery();

						if (RenamedDocumentsList.ContainsKey(legacyMenuItemDetails))
						{
							ZString[] newMenuItemDetails = RenamedDocumentsList[legacyMenuItemDetails].Split(':');

							query.AddToFilter(StmMenuItemSchema.SU_MenuName, newMenuItemDetails[0].Trim());
							query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, newMenuItemDetails[1].Trim());
							query.AddToFilter(StmMenuItemSchema.SU_MenuPath, newMenuItemDetails[2].Trim());
							query.AddToFilter(StmMenuItemSchema.SU_ContactType, newMenuItemDetails[3].Trim());
						}
						else
						{
							var productionMenuItemPath = menuItem.SU_MenuPath.Replace(Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments, "").TrimStart('/', '\\', ' ');

							query.AddToFilter(StmMenuItemSchema.SU_MenuName, menuItem.SU_MenuName);
							query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, menuItem.SU_BusinessContext);
							query.AddToFilter(StmMenuItemSchema.SU_MenuPath, productionMenuItemPath);
							query.AddToFilter(StmMenuItemSchema.SU_ContactType, menuItem.SU_ContactType);
						}

						var productionMenuItem = Factory.Load<StmMenuItem>(query);

						if (productionMenuItem == null || productionMenuItem.Length < 1)
						{
							Fail(string.Format("The menu item [{0}] is a legacy version, but doesn't have a corresponding parent menu item.",
								legacyMenuItemDetails));
						}
					}
				}
			});

			Assert(true);
		}

		#region Legacy DocBuilder Document List

		List<ZString> legacyDocBuilderDocumentList;
		List<ZString> LegacyDocBuilderDocumentList
		{
			get
			{
				if (legacyDocBuilderDocumentList == null)
				{
					legacyDocBuilderDocumentList = new List<ZString>();
					legacyDocBuilderDocumentList.Add("AWB Security Declaration : Consol : Legacy Documents/AWB : SHP");
				}
				return legacyDocBuilderDocumentList;
			}
		}
		#endregion

		#region Renamed Document List

		Dictionary<ZString, ZString> RenamedDocumentsList
		{
			get
			{
				if (renamedDocumentsList == null)
				{
					renamedDocumentsList = new Dictionary<ZString, ZString>();

					renamedDocumentsList.Add("Cartage Advice : ContainerLeg : Legacy Documents : TRN", "Leg Cartage Advice : ContainerLeg :  : TRN");
					renamedDocumentsList.Add("Cartage Advice With Receipt : ContainerLeg : Legacy Documents : TRN", "Leg Cartage Advice With Receipt : ContainerLeg :  : TRN");
					renamedDocumentsList.Add("Cartage Leg Cartage Advice : JobCartageRunSheet : Legacy Documents : TRN", "Leg Cartage Advice : JobCartageRunSheet :  : TRN");
					renamedDocumentsList.Add("Cartage Leg Cartage Advice With Receipt : JobCartageRunSheet : Legacy Documents : TRN", "Leg Cartage Advice With Receipt : JobCartageRunSheet :  : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice : Cartage : Legacy Documents/Cartage Advice : TRN", "Multi-Container Cartage Advice : Cartage : Cartage Advice : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice : Consol : Legacy Documents/Arrival : TRN", "Multi-Container Cartage Advice : Consol : Arrival : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice : Consol : Legacy Documents/Departure : TRN", "Multi-Container Cartage Advice : Consol : Departure : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice : Customs : Legacy Documents/Cartage : TRN", "Multi-Container Cartage Advice : Customs : Cartage : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice : Shipment : Legacy Documents/Arrival/Cartage Advice : TRN", "Multi-Container Cartage Advice : Shipment : Arrival/Cartage Advice : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice : Shipment : Legacy Documents/Departure/Cartage Advice : TRN", "Multi-Container Cartage Advice : Shipment : Departure/Cartage Advice : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice With Receipt : Cartage : Legacy Documents/Cartage Advice : TRN", "Multi-Container Cartage Advice w Receipt : Cartage : Cartage Advice : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice With Receipt : Consol : Legacy Documents/Arrival : TRN", "Multi-Container Cartage Advice w Receipt : Consol : Arrival : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice With Receipt : Consol : Legacy Documents/Departure : TRN", "Multi-Container Cartage Advice w Receipt : Consol : Departure : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice With Receipt : Customs : Legacy Documents/Cartage : TRN", "Multi-Container Cartage Advice w Receipt : Customs : Cartage : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice With Receipt : Shipment : Legacy Documents/Arrival/Cartage Advice : TRN", "Multi-Container Cartage Advice w Receipt : Shipment : Arrival/Cartage Advice : TRN");
					renamedDocumentsList.Add("Combined Cartage Advice With Receipt : Shipment : Legacy Documents/Departure/Cartage Advice : TRN", "Multi-Container Cartage Advice w Receipt : Shipment : Departure/Cartage Advice : TRN");
					renamedDocumentsList.Add("Invoice Detail : ARInvoice : Legacy Documents : 3PL", "Periodic Invoice Detail: ARInvoice :  : 3PL");
					renamedDocumentsList.Add("Invoice && Invoice Detail : ARInvoice : Legacy Documents : 3PL", "Periodic Invoice && Invoice Detail : ARInvoice :  : 3PL");
					renamedDocumentsList.Add("Hazardous Cargo Label : Shipment : Legacy Documents/AWB : NCT", "Hazardous Cargo Label : Shipment : Legacy Documents/AWB : NCT");
					renamedDocumentsList.Add("AWB Security Declaration : Consol : Legacy Documents/AWB : SHP", "AWB Security Declaration : Consol : Legacy Documents/AWB : SHP");
					renamedDocumentsList.Add("CMR International Consignment Note : Consol : Legacy Documents/Departure : NCT", "CMR Consignment Note : Consol : Departure : NCT");
				}

				return renamedDocumentsList;
			}
		}

		Dictionary<ZString, ZString> renamedDocumentsList;

		#endregion

		[StressTestAttribute]
		public void TestNoDevelpmentVersionMenuPathsUsed()
		{
			var menuItems = GetAllMenuItems();

			CombineAssertions(delegate()
			{
				foreach (var menuItem in menuItems)
				{
					var errorMessage = string.Format(
						"The menu item [{0}] is a development version. Please move to Parent folder instead, and move the old menu item to the Legacy Documents folder.",
						GetMenuItemDetails(menuItem));

					AssertNotContains(errorMessage, "development version", menuItem.SU_MenuPath.ToLower());
				}
			});
		}

		[StressTestAttribute]
		public void TestMenuPathsDoNotHaveLeadingOrTrailingSlashes()
		{
			var query = new ZQuery(StmMenuItemSchema.SU_IsSystemDefined, true);
			var menuItems = Factory.Load<StmMenuItem>(query);

			CombineAssertions(delegate()
			{
				foreach (var menuItem in menuItems)
				{
					var errorMessage = string.Format(
						"Menu item [{0}] - Menu path should not start or end with '/'.",
						GetMenuItemDetails(menuItem));

					AssertEquals(errorMessage, menuItem.SU_MenuPath.Trim('/', ' '), menuItem.SU_MenuPath);
				}
			});
		}

		#region Implementation

		StmMenuItemBase[] GetAllMenuItems()
		{
			var query = new ZQuery();

			return Factory.Load<StmMenuItemBase>(query);
		}

		StmMenuItemBase[] GetAllLegacyDocuments()
		{
			var query = new ZQuery(
				StmMenuItemSchema.SU_MenuPath,
				SQLComparisonOperator.StartsWith,
				Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments);

			return Factory.Load<StmMenuItemBase>(query);
		}

		StmMenuItemBase[] GetAllLegacyDocumentsForFreightExcludingAccountingLegacyDocuments()
		{
			StmMenuItemBase[] menus = GetAllLegacyDocuments();
			return menus.Where(x => x.SU_BusinessContext != "APInvoice" &&
												 x.SU_BusinessContext != "APTransaction" &&
												 x.SU_BusinessContext != "CBDirectPayment" &&
												 x.SU_BusinessContext != "PaymentApproval" &&
												 x.SU_BusinessContext != "Statement" &&
												 x.SU_BusinessContext != "StatementSummary").ToArray<StmMenuItemBase>();
		}

		StmMenuItemBase[] GetAllNonLegacyDocuments()
		{
			var query = new ZQuery(
				StmMenuItemSchema.SU_MenuPath,
				SQLComparisonOperator.DoesNotStartWith,
				Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments);

			return Factory.Load<StmMenuItemBase>(query);
		}

		string GetMenuItemDetails(StmMenuItem menuItem)
		{
			return string.Format("{0} : {1} : {2} : {3}",
				menuItem.SU_MenuName,
				menuItem.SU_BusinessContext,
				menuItem.SU_MenuPath,
				menuItem.SU_ContactType);
		}

		#endregion

	}
}
