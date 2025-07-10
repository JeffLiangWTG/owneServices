using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using DocumentNames = Enterprise.DocumentEngine.Constants.DocumentNames;

namespace Enterprise.DocumentWrappers.Testing
{
	public class DocumentMenuTestClass : TestCaseWithFactory
	{
		[StressTest]
		public void TestAllSystemMenusArePublishedForDocumentXML()
		{
			StmMenuItem[] menuItems = Factory.Load<StmMenuItem>(new DocumentZQuery());
			StringBuilder incorrectMenus = new StringBuilder();

			foreach (StmMenuItem item in menuItems)
			{
				var result =
					(from exclude in UnPublishedSystemDocuments
						 where exclude.Key == item.SU_MenuName
						 && exclude.Value == item.SU_BusinessContext
						 select exclude).Count();

				if (item.SU_IsSystemDefined && !item.SU_IsPublished && result == 0)
				{
					incorrectMenus.Append(item.SU_MenuName + " Business Context: " + item.SU_BusinessContext + System.Environment.NewLine);
				}
			}

			if (incorrectMenus.Length > 0)
			{
				incorrectMenus.Insert(0, System.Environment.NewLine + "The following menu items are system defined but not published:" + System.Environment.NewLine);
				Fail(incorrectMenus.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		List<KeyValuePair<ZString, ZString>> UnPublishedSystemDocuments
		{
			get
			{
				if (unPublishedSystemDocuments == null)
				{
					unPublishedSystemDocuments = new List<KeyValuePair<ZString, ZString>>();
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("DocBuilder Invoice", nameof(BusinessContext.Customs)));
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("Opportunity Pipeline", "RepSalesMgrReports"));
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("Opportunity Pipeline With Trade Profile", "RepSalesMgrReports"));
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("Opportunity Value Analysis", "RepSalesMgrReports"));
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("Opportunity Weighted Value Report", "RepSalesMgrReports"));
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("Organization - Similar Organizations Report (Legacy)", "RepMasterDataReports"));
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("Organization - US IRS 1099-MISC and 1099-NEC Forms 2020", "RepMasterDataReports"));
					unPublishedSystemDocuments.Add(new KeyValuePair<ZString, ZString>("Organization - US IRS 1099-MISC and 1099-NEC Forms 2021", "RepMasterDataReports"));
				}

				return unPublishedSystemDocuments;
			}
		}
		List<KeyValuePair<ZString, ZString>> unPublishedSystemDocuments;

		public void TestCartageAdviceMenuSetUps()
		{
			StringBuilder pivotNameErrors = new StringBuilder();
			StringBuilder docDirectionErrors = new StringBuilder();

			DynamicBusinessObjectCollection businessObjCollection = new DynamicBusinessObjectCollection(Factory);
			ZString sqlString = "Select " + StmMenuItem.Schema.SU_MenuName + ", " + StmMenuItem.Schema.SU_BusinessContext + ", " + StmMenuItem.Schema.SU_MenuPath + ", " + StmMenuItem.Schema.SU_DocumentDirection + ", " + StmMenuTemplatePivot.Schema.SI_DocumentTitle
				+ " from " + StmMenuItemSchema.Constants.SqlSchemaName + "." + StmMenuItemSchema.Constants.TableName
				+ " inner join " + StmMenuTemplatePivotSchema.Constants.SqlSchemaName + "." + StmMenuTemplatePivotSchema.Constants.TableName + " on " + StmMenuItem.Schema.PK + " = " + StmMenuTemplatePivot.Schema.SI_SU
				+ " inner join " + StmTemplateSchema.Constants.SqlSchemaName + "." + StmTemplateSchema.Constants.TableName + " on " + StmMenuTemplatePivot.Schema.SI_SO + " = " + StmTemplate.Schema.PK
				+ " where " + StmTemplate.Schema.SO_Name + " = @CartageAdvice"
				+ " and " + StmMenuItemSchema.Constants.SU_MenuType + " = @MenuType";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add(ZSqlParameter.New("@CartageAdvice", "Cartage Advice", StmTemplateSchema.SO_Name));
			@params.Add(ZSqlParameter.New("@MenuType", Core.Constants.StmMenuItemTypes.Documents, StmMenuItemSchema.SU_MenuType));

			businessObjCollection.Load(sqlString, @params);

			foreach (BusinessObject currentBisObj in businessObjCollection)
			{
				ZString menuName = new ZString(currentBisObj[StmMenuItem.Schema.SU_MenuName]);
				ZString businessContext = new ZString(currentBisObj[StmMenuItem.Schema.SU_BusinessContext]);
				ZString docDirection = new ZString(currentBisObj[StmMenuItem.Schema.SU_DocumentDirection]);
				ZString pivotName = new ZString(currentBisObj[StmMenuTemplatePivot.Schema.SI_DocumentTitle]).ToUpper();
				ZString menuPath = new ZString(currentBisObj[StmMenuItem.Schema.SU_MenuPath]);

				ZBool isCorrectPivotName = pivotName == DocumentNames.BookingCartageAdvice
					|| pivotName == DocumentNames.BookingCartageAdviceWithReceipt
					|| pivotName == DocumentNames.AgencyCartageAdvice
					|| pivotName == DocumentNames.AgencyCartageAdviceWithReceipt
					|| pivotName == DocumentNames.CFSCartageAdvice
					|| pivotName == DocumentNames.CFSCartageAdviceWithReceipt
					|| pivotName == DocumentNames.CFSShipmentCartageAdvice
					|| pivotName == DocumentNames.CFSShipmentCartageAdviceWithReceipt
					|| pivotName == DocumentNames.ShipmentCartageAdvice
					|| pivotName == DocumentNames.ShipmentCartageAdviceWithReceipt
					|| pivotName == DocumentNames.ConfirmationCartageAdvice
					|| pivotName == DocumentNames.ConfirmationCartageAdviceWithReceipt
					|| pivotName == DocumentNames.CustomsDeclarationCartageAdvice
					|| pivotName == DocumentNames.CustomsDeclarationCartageAdviceWithReceipt
					|| pivotName == DocumentNames.LocalCartageAdvice
					|| pivotName == DocumentNames.LocalCartageAdviceWithReceipt
					|| pivotName == DocumentNames.ContainerLegCartageAdvice
					|| pivotName == DocumentNames.ContainerLegCartageAdviceWithReceipt
					|| pivotName == DocumentNames.ConsolCartageAdvice
					|| pivotName == DocumentNames.ConsolCartageAdviceWithReceipt
					|| pivotName == DocumentNames.CartageLegCartageAdvice
					|| pivotName == DocumentNames.CartageLegCartageAdvice + " WITH RECEIPT" //Add when i have permission DocumentNames.CartageLegCartageAdviceWithReceipt.ToString()
					|| pivotName == DocumentNames.LocalMultiContainerCartageAdvice;

				if (!isCorrectPivotName)
				{
					pivotNameErrors.Append(menuName + System.Environment.NewLine);
				}

				if (pivotName == DocumentNames.BookingCartageAdvice || pivotName == DocumentNames.BookingCartageAdviceWithReceipt)
				{
					if (docDirection != nameof(DocumentDirection.DEP))
					{
						docDirectionErrors.Append(menuName + " on the Booking Form needs a document direction of 'DEP'" + System.Environment.NewLine);
					}
				}
				else if (menuPath.StartsWith("Import"))
				{
					if (docDirection != nameof(DocumentDirection.ARV))
					{
						docDirectionErrors.Append(menuName + " (" + businessContext + ") needs a document direction of 'ARV'" + System.Environment.NewLine);
					}
				}
				else if (menuPath.StartsWith("Export"))
				{
					if (docDirection != nameof(DocumentDirection.DEP))
					{
						docDirectionErrors.Append(menuName + " (" + businessContext + ") needs a document direction of 'DEP'" + System.Environment.NewLine);
					}
				}
			}

			if (pivotNameErrors.Length > 0 || docDirectionErrors.Length > 0)
			{
				if (pivotNameErrors.Length > 0)
				{
					pivotNameErrors.Insert(0, "The following menus have an incorrect pivot name:");
					pivotNameErrors.Append("It should be either:"
						+ DocumentNames.AgencyCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.AgencyCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.ConfirmationCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.ConfirmationCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.BookingCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.BookingCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.CFSCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.CFSCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.ShipmentCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.ShipmentCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.CustomsDeclarationCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.CustomsDeclarationCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.LocalCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.LocalCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.ContainerLegCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.ContainerLegCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.ConsolCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.ConsolCartageAdviceWithReceipt + " or " + System.Environment.NewLine
						+ DocumentNames.CartageLegCartageAdvice + " or " + System.Environment.NewLine
						+ DocumentNames.CartageLegCartageAdvice + " WITH RECEIPT" + " or " + System.Environment.NewLine //Add when i have permission DocumentNames.CartageLegCartageAdviceWithReceipt.ToString()
						+ DocumentNames.LocalMultiContainerCartageAdvice + System.Environment.NewLine);
				}

				if (docDirectionErrors.Length > 0)
				{
					docDirectionErrors.Insert(0, System.Environment.NewLine + "The following Cartage Advice menu items have incorrect document directions:" + System.Environment.NewLine);
				}

				Fail(pivotNameErrors.ToString() + docDirectionErrors.ToString());
			}
			else
			{
				Assert(true);
			}
		}

#region CFS Label ReportName Tests
		public void TestCFSTranshipmentLabelDocumentNamesForCFSShipment()
		{
			StmMenuItem menu = AssertMenuNotNull("Transhipment Label", nameof(BusinessContext.CFSShipmentReceival));
			AssertPivotDocumentTitleCorrect(DocBaseWrapper.TranshipmentLabel, menu);
		}

		public void TestCFSTranshipmentLabelDocumentNamesForCFSLoadListConsol()
		{
			StmMenuItem menu = AssertMenuNotNull("Transhipment Label", nameof(BusinessContext.CFSLoadList));
			AssertPivotDocumentTitleCorrect(DocBaseWrapper.TranshipmentLabel, menu);
		}

		public void TestCFSImportLabelDocumentNameForCFSShipment()
		{
			StmMenuItem menu = AssertMenuNotNull("Import Label", nameof(BusinessContext.CFSShipmentReceival));
			AssertPivotDocumentTitleCorrect("IMPORT", menu);
		}

		public void TestCFSImportLabelDocumentNameForCFSLoadListConsol()
		{
			StmMenuItem menu = AssertMenuNotNull("Import Label", nameof(BusinessContext.CFSLoadList));
			AssertPivotDocumentTitleCorrect("IMPORT", menu);
		}

		public void TestCFSOnForwardingLabelDocumentNameForCFSShipment()
		{
			StmMenuItem menu = AssertMenuNotNull("On Forwarding Label", nameof(BusinessContext.CFSShipmentReceival));
			AssertPivotDocumentTitleCorrect(DocBaseWrapper.OnForwardingLabel, menu);
		}

		public void TestCFSOnForwardingLabelDocumentNameForCFSLoadListConsol()
		{
			StmMenuItem menu = AssertMenuNotNull("On Forwarding Label", nameof(BusinessContext.CFSLoadList));
			AssertPivotDocumentTitleCorrect(DocBaseWrapper.OnForwardingLabel, menu);
		}

#endregion

#region MY North/WestPort Document Menu Test
		public void TestShipmentNorthPortDeliveryOrderMenu()
		{
			AssertMenuFilter("NorthPort Delivery Order", nameof(BusinessContext.Shipment), MYDOFilter);
		}

		public void TestConsolNorthPortDeliveryOrderMenu()
		{
			AssertMenuFilter("NorthPort Delivery Order", nameof(BusinessContext.Consol), MYDOFilter);
		}

		public void TestShipmentNorthPortPKDPDeliveryOrderMenu()
		{
			AssertMenuFilter("NorthPort PKDP Delivery Order", nameof(BusinessContext.Shipment), MYDOFilter);
		}

		public void TestConsolNorthPortPKDPDeliveryOrderMenu()
		{
			AssertMenuFilter("NorthPort PKDP Delivery Order", nameof(BusinessContext.Consol), MYDOFilter);
		}

		public void TestShipmentWestPortDeliveryOrderMenu()
		{
			AssertMenuFilter("WestPort Delivery Order", nameof(BusinessContext.Shipment), MYDOFilter);
		}

		public void TestConsolWestPortDeliveryOrderMenu()
		{
			AssertMenuFilter("WestPort Delivery Order", nameof(BusinessContext.Consol), MYDOFilter);
		}

		public void TestShipmentNorthWestPortDeliveryOrderFollowOnMenu()
		{
			AssertMenuFilter("North/WestPort Document Continuation", nameof(BusinessContext.Shipment), "CTY=MY");
		}

		public void TestConsolNorthWestPortDeliveryOrderFollowOnMenu()
		{
			AssertMenuFilter("North/WestPort Document Continuation", nameof(BusinessContext.Consol), MYDOFilter);
		}

		public void TestShipmentContainerShippingNoteMenu()
		{
			AssertMenuFilter("NorthPort Container Shipping Note", nameof(BusinessContext.Shipment), "CTY=MY");
		}
#endregion

#region Shipment Document Direction Tests

		public void TestDirectionForImportShipmentDocos()
		{
			StringBuilder errors = new StringBuilder();

			DynamicBusinessObjectCollection businessObjCollection = new DynamicBusinessObjectCollection(Factory);
			ZString sqlString = "Select " + StmMenuItem.Schema.SU_MenuName + ", " + StmMenuItem.Schema.SU_DocumentDirection
				+ " from " + StmMenuItemSchema.Constants.SqlSchemaName + "." + StmMenuItemSchema.Constants.TableName
				+ " where " + StmMenuItem.Schema.SU_BusinessContext + " = @Shipment"
				+ " and " + StmMenuItem.Schema.SU_MenuPath + " = @Import"
				+ " and " + StmMenuItemSchema.Constants.SU_MenuType + " = @MenuType";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add(ZSqlParameter.New("@Shipment", "Shipment", StmMenuItemSchema.SU_BusinessContext));
			@params.Add(ZSqlParameter.New("@Import", "Import/", StmMenuItemSchema.SU_MenuPath));
			@params.Add(ZSqlParameter.New("@MenuType", Core.Constants.StmMenuItemTypes.Documents, StmMenuItemSchema.SU_MenuType));

			businessObjCollection.Load(sqlString, @params);

			foreach (BusinessObject currentBisObj in businessObjCollection)
			{
				ZString menuName = new ZString(currentBisObj[StmMenuItem.Schema.SU_MenuName]);
				ZString docDirection = new ZString(currentBisObj[StmMenuItem.Schema.SU_DocumentDirection]);

				if (docDirection != nameof(DocumentDirection.ARV))
				{
					errors.Append(menuName + System.Environment.NewLine);
				}
			}

			if (errors.Length > 0)
			{
				errors.Insert(0, "The following Import Shipment docos do not have the correct document direction of 'ARV':" + System.Environment.NewLine);
				Fail(errors.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDirectionForExportShipmentDocos()
		{
			StringBuilder errors = new StringBuilder();

			DynamicBusinessObjectCollection businessObjCollection = new DynamicBusinessObjectCollection(Factory);
			ZString sqlString = "Select " + StmMenuItem.Schema.SU_MenuName + ", " + StmMenuItem.Schema.SU_DocumentDirection
				+ " from " + StmMenuItemSchema.Constants.SqlSchemaName + "." + StmMenuItemSchema.Constants.TableName
				+ " where " + StmMenuItem.Schema.SU_BusinessContext + " = @Shipment"
				+ " and " + StmMenuItem.Schema.SU_MenuPath + " = @Export"
				+ " and " + StmMenuItemSchema.Constants.SU_MenuType + " = @MenuType";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add(ZSqlParameter.New("@Shipment", "Shipment", StmMenuItemSchema.SU_BusinessContext));
			@params.Add(ZSqlParameter.New("@Export", "Export/", StmMenuItemSchema.SU_MenuPath));
			@params.Add(ZSqlParameter.New("@MenuType", Core.Constants.StmMenuItemTypes.Documents, StmMenuItemSchema.SU_MenuType));

			businessObjCollection.Load(sqlString, @params);

			foreach (BusinessObject currentBisObj in businessObjCollection)
			{
				ZString menuName = new ZString(currentBisObj[StmMenuItem.Schema.SU_MenuName]);
				ZString docDirection = new ZString(currentBisObj[StmMenuItem.Schema.SU_DocumentDirection]);

				if (docDirection != nameof(DocumentDirection.DEP))
				{
					errors.Append(menuName + System.Environment.NewLine);
				}
			}

			if (errors.Length > 0)
			{
				errors.Insert(0, "The following Export Shipment docos do not have the correct document direction of 'DEP':" + System.Environment.NewLine);
				Fail(errors.ToString());
			}
			else
			{
				Assert(true);
			}
		}

#endregion

#region IMO Document Title

		public void TestIMODocumentTitle()
		{
			//If you need to change the document title then change the IMO properties in DocIMOShipment and DocContainer.
			StmMenuItem consolIMOMenu = AssertMenuNotNull("Multimodal Dangerous Goods Form", nameof(BusinessContext.Consol));
			AssertPivotDocumentTitleCorrect(DocBaseWrapper.ConsolIMO, consolIMOMenu);
			AssertMenuFilter("Multimodal Dangerous Goods Form", nameof(BusinessContext.Consol), "CTY=AU");

			StmMenuItem shipmentIMOMenu = AssertMenuNotNull("Multimodal Dangerous Goods Form (AU)", nameof(BusinessContext.Shipment));
			AssertPivotDocumentTitleCorrect(DocBaseWrapper.ShipmentIMO, shipmentIMOMenu);
			AssertMenuFilter("Multimodal Dangerous Goods Form (AU)", nameof(BusinessContext.Shipment), "CTY=AU");

			StmMenuItem cFSIMOMenu = AssertMenuNotNull("Multimodal Dangerous Goods Form", nameof(BusinessContext.CFSLoadList));
			AssertPivotDocumentTitleCorrect(DocBaseWrapper.CFSIMO, cFSIMOMenu);
			AssertMenuFilter("Multimodal Dangerous Goods Form", nameof(BusinessContext.CFSLoadList), "CTY=AU");
		}

#endregion

#region Bill Of Lading Tests

		public void TestStandardBillOfLadingPivotDocumentNameAndPrintSetting()
		{
			ZStringBuilder incorrectTemplates = new ZStringBuilder();

			ZQuery filter = new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			filter = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, menuItem.PK);
			StmMenuTemplatePivotCollection collection = new StmMenuTemplatePivotCollection(Factory, filter);
			collection.Load();

			while (collection.Count > 0)
			{
				filter = new ZQuery(StmMenuTemplatePivotSchema.SI_MenuTemplateFilter, collection[0].SI_MenuTemplateFilter);
				StmMenuTemplatePivot[] menuTemplates = (StmMenuTemplatePivot[])collection.Find(filter);
				//AssertEquals("MenuTemplate.Length " + Collection[0].SI_MenuTemplateFilter, 4, MenuTemplates.Length);

				bool hasCopy = false;
				bool hasFaxCopy = false;
				bool hasEmailCopy = false;
				bool hasOriginal = false;
				foreach (StmMenuTemplatePivot menuTemplate in menuTemplates)
				{
					switch (menuTemplate.SI_DocumentTitle)
					{
						case "COPY":
							hasCopy = (menuTemplate.SI_PrintCopyType == Core.Constants.ContactNotifyModes.Print);
							break;
						case "FAX COPY":
							hasFaxCopy = (menuTemplate.SI_PrintCopyType == Core.Constants.ContactNotifyModes.Fax);
							break;
						case "EMAIL COPY":
							hasEmailCopy = (menuTemplate.SI_PrintCopyType == Core.Constants.ContactNotifyModes.Email);
							break;
						case "ORIGINAL":
							hasOriginal = (menuTemplate.SI_PrintCopyType == Core.Constants.ContactNotifyModes.Print);
							break;
					}

					collection.Remove(menuTemplate);
				}

				bool isCorrect = (hasCopy && hasFaxCopy && hasEmailCopy && hasOriginal);

				if (!isCorrect)
				{
					string missing =
						(hasCopy ? "" : " 'COPY'") +
						(hasFaxCopy ? "" : " 'FAX COPY'") +
						(hasEmailCopy ? "" : " 'EMAIL COPY'") +
						(hasOriginal ? "" : " 'ORIGINAL'");

					incorrectTemplates.Append(menuTemplates[0].SI_MenuTemplateFilter + " - missing" + missing + System.Environment.NewLine);
				}
			}

			if (incorrectTemplates.Length > 0)
			{
				AssertEquals("The following menu templates don't have thier Title and/or Type properly set:", "", incorrectTemplates.ToString());
			}
			else
			{
				Assert(true);
			}
		}

#endregion

		protected StmMenuItem AssertMenuNotNull(ZString menuName, ZString businessContext)
		{
			DocumentZQuery filter = new DocumentZQuery(businessContext, menuName);
			StmMenuItem menu = Factory.LoadTop1<StmMenuItem>(filter);
			AssertNotNull(menu);
			return menu;
		}

		protected void AssertPivotDocumentTitleCorrect(ZString expectedReportName, StmMenuItem menu)
		{
			ZQuery pivotFilter = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, menu.PK);
			StmMenuTemplatePivot[] pivot = (StmMenuTemplatePivot[])Factory.Load(typeof(StmMenuTemplatePivot), pivotFilter);
			AssertEquals(1, pivot.Length);
			AssertEquals(expectedReportName, pivot[0].SI_DocumentTitle);
			//			AssertEquals(Core.Constants.DataContext.CFSLabels.ToString(), Pivot[0].Template.SO_DataContext);
		}

		protected void AssertMenuFilter(ZString menuName, ZString businessContext, ZString filterValue)
		{
			StmMenuItem menu = AssertMenuNotNull(menuName, businessContext);
			AssertEquals("Menu filter string", filterValue, menu.SU_FilterList);
		}
		const string MYDOFilter = "MYDO=Y";
	}
}
