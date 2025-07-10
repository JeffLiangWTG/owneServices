using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	public class DocWrapperContextManagerTest : TestCaseWithFactory
	{
		public void TestSetContextValuesTemporarily()
		{
			var manager = Factory.GetDocWrapperContextManager();
			var managerAsContext = (IDocWrapperContext)manager;
			AssertExceptionThrown(typeof(ArgumentNullException), () => manager.SetContextValuesTemporarily(DocumentDirection.DEP, null));

			var contactType = new Mock<ContactTypeForTest>();

			contactType.Setup(m => m.ToString()).Returns("XXX");

			using (manager.SetContextValuesTemporarily(DocumentDirection.DEP, contactType.Object))
			{
				AssertEquals(ZGuid.Empty, managerAsContext.BrandedOrganisationPK);
				AssertEquals(ZGuid.Empty, managerAsContext.ContactOrganisationPK);
				AssertEquals(ZGuid.Empty, managerAsContext.MenuItemPK);
				AssertEquals("XXX", managerAsContext.DocumentContactTypeCode);
				AssertEquals(ZString.Empty, managerAsContext.DocumentDeliveryMode);
				AssertEquals("DEP", managerAsContext.DocumentDirection);
				AssertEquals(ZString.Empty, managerAsContext.MenuTitle);
				AssertEquals(ZString.Empty, managerAsContext.ReportName);
			}

			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.BrandedOrganisationPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.ContactOrganisationPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.DocumentContactTypeCode; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.DocumentDeliveryMode; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.DocumentDirection; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.MenuItemPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.MenuTitle; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate { var value = managerAsContext.ReportName; });

			contactType.VerifyAll();
		}

		public void TestSuspendContextSetCheck()
		{
			var manager = Factory.GetDocWrapperContextManager();

			var managerAsContext = (IDocWrapperContext)manager;
			using (manager.SuspendContextSetCheck())
			{
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.BrandedOrganisationPK; });
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.ContactOrganisationPK; });
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.DocumentContactTypeCode; });
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.DocumentDeliveryMode; });
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.DocumentDirection; });
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.MenuItemPK; });
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.MenuTitle; });
				AssertNoExceptionThrown(delegate
				{ var value = managerAsContext.ReportName; });
			}

			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.BrandedOrganisationPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.ContactOrganisationPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.DocumentContactTypeCode; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.DocumentDeliveryMode; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.DocumentDirection; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.MenuItemPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.MenuTitle; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.ReportName; });
		}

		public void TestClearMenuContextValuesTemporarily()
		{
			var manager = Factory.GetDocWrapperContextManager();
			var brandedOrganisationPK = ZGuid.NewZGuid();
			var contactOrganisationPK = ZGuid.NewZGuid();
			var menuItemPK = ZGuid.NewZGuid();
			manager.UpdateDocWrapperContextFromReportConstants(new Dictionary<string, object>
			{
				{ Constants.TemplateDefined.DocumentDirection, "IMP" },
				{ Constants.TemplateDefined.MenuItemPK, menuItemPK },
				{ Constants.TemplateDefined.MenuTitle, "Menu Title" },
				{ Constants.TemplateDefined.ContactType, "CNE" },
				{ Constants.TemplateDefined.BrandedOrganisationPK, brandedOrganisationPK },
				{ Constants.TemplateDefined.ContactOrganisationPK, contactOrganisationPK } });

			using (manager.ClearMenuContextValuesTemporarily())
			{
				var managerAsContext = (IDocWrapperContext)manager;
				AssertNullOrEmpty(managerAsContext.DocumentDirection);
				AssertNullOrEmpty(managerAsContext.MenuTitle);
				AssertNullOrEmpty(managerAsContext.DocumentContactTypeCode);
				Assert(managerAsContext.MenuItemPK.IsEmpty);
				Assert(managerAsContext.BrandedOrganisationPK.IsEmpty);
				Assert(managerAsContext.ContactOrganisationPK.IsEmpty);
			}

			CombineAssertions(delegate
			{
				var managerAsContext = (IDocWrapperContext)manager;
				AssertEquals("BrandedOrganisationPK", brandedOrganisationPK, managerAsContext.BrandedOrganisationPK);
				AssertEquals("ContactOrganisationPK", contactOrganisationPK, managerAsContext.ContactOrganisationPK);
				AssertEquals("DocumentContactTypeCode", "CNE", managerAsContext.DocumentContactTypeCode);
				AssertEquals("DocumentDirection", "IMP", managerAsContext.DocumentDirection);
				AssertEquals("MenuItemPK", menuItemPK, managerAsContext.MenuItemPK);
				AssertEquals("MenuTitle", "Menu Title", managerAsContext.MenuTitle);
			});
		}

		public void TestStraightUpdateFromReportObjectIsFineToo()
		{
			ZGuid brandedOrganisationPK = ZGuid.NewZGuid();
			ZGuid contactOrganisationPK = ZGuid.NewZGuid();
			ZString documentContactTypeCode = "CNE";
			ZString documentDeliveryMode = "DLV";
			ZString documentDirection = "IMP";
			ZGuid menuItemPK = ZGuid.NewZGuid();
			ZString menuTitle = "Menu Title";
			ZString reportName = "Report Name";

			var manager = Factory.GetDocWrapperContextManager();

			manager.UpdateDocWrapperContextFromReportConstants(new Dictionary<string, object>
			{
				{ Constants.TemplateDefined.DocumentDirection, documentDirection },
				{ Constants.TemplateDefined.MenuItemPK, menuItemPK },
				{ Constants.TemplateDefined.MenuTitle, menuTitle },
				{ Constants.TemplateDefined.ContactType, documentContactTypeCode },
				{ Constants.TemplateDefined.BrandedOrganisationPK, brandedOrganisationPK },
				{ Constants.TemplateDefined.ContactOrganisationPK, contactOrganisationPK },
				{ Constants.TemplateDefined.DeliveryMode, documentDeliveryMode },
				{ Constants.TemplateDefined.ReportName, reportName },
			});

			var managerAsContext = (IDocWrapperContext)manager;
			CombineAssertions(delegate
			{
				AssertEquals("BrandedOrganisationPK", brandedOrganisationPK, managerAsContext.BrandedOrganisationPK);
				AssertEquals("ContactOrganisationPK", contactOrganisationPK, managerAsContext.ContactOrganisationPK);
				AssertEquals("DocumentContactTypeCode", documentContactTypeCode, managerAsContext.DocumentContactTypeCode);
				AssertEquals("DocumentDeliveryMode", documentDeliveryMode, managerAsContext.DocumentDeliveryMode);
				AssertEquals("DocumentDirection", documentDirection, managerAsContext.DocumentDirection);
				AssertEquals("MenuItemPK", menuItemPK, managerAsContext.MenuItemPK);
				AssertEquals("MenuTitle", menuTitle, managerAsContext.MenuTitle);
				AssertEquals("ReportName", reportName, managerAsContext.ReportName);
			});
		}

		public void TestAccessingContextManagerBeforeItsBeenSetBlowsUp()
		{
			var manager = Factory.GetDocWrapperContextManager();

			var managerAsContext = (IDocWrapperContext)manager;
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.BrandedOrganisationPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.ContactOrganisationPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.DocumentContactTypeCode; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.DocumentDeliveryMode; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.DocumentDirection; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.MenuItemPK; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.MenuTitle; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.ReportName; });
		}

		public void TestAccessingContextManagerAfterItsBeenSetDoesNotBlowup()
		{
			var manager = Factory.GetDocWrapperContextManager();
			manager.SetupDocWrapperContextFromDocumentPack("IMP", ZGuid.NewZGuid(), "Second Menu Title", "CNR", null, null);

			AssertNoExceptionThrown(delegate
			{ manager.UpdateDocWrapperContextFromReportConstants(null); });

			var managerAsContext = (IDocWrapperContext)manager;
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.BrandedOrganisationPK; });
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.ContactOrganisationPK; });
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.DocumentContactTypeCode; });
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.DocumentDeliveryMode; });
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.DocumentDirection; });
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.MenuItemPK; });
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.MenuTitle; });
			AssertNoExceptionThrown(delegate
			{ var value = managerAsContext.ReportName; });
		}

		public void TestValuesUpdatedReturnRightValues()
		{
			ZGuid brandedOrganisationPK = ZGuid.NewZGuid();
			ZGuid contactOrganisationPK = ZGuid.NewZGuid();
			ZString documentContactTypeCode = "CNE";
			ZString documentDeliveryMode = "DLV";
			ZString documentDirection = "IMP";
			ZGuid menuItemPK = ZGuid.NewZGuid();
			ZString menuTitle = "Menu Title";
			ZString reportName = "Report Name";

			var manager = Factory.GetDocWrapperContextManager();
			manager.SetupDocWrapperContextFromDocumentPack(documentDirection, menuItemPK, menuTitle, documentContactTypeCode, new MockOrgHeader(brandedOrganisationPK), new MockOrgHeader(contactOrganisationPK));

			manager.UpdateDocWrapperContextFromReportConstants(new Dictionary<string, object>
			{
				{ Constants.TemplateDefined.DeliveryMode, documentDeliveryMode },
				{ Constants.TemplateDefined.ReportName, reportName },
			});

			var managerAsContext = (IDocWrapperContext)manager;
			CombineAssertions(delegate
			{
				AssertEquals("BrandedOrganisationPK", brandedOrganisationPK, managerAsContext.BrandedOrganisationPK);
				AssertEquals("ContactOrganisationPK", contactOrganisationPK, managerAsContext.ContactOrganisationPK);
				AssertEquals("DocumentContactTypeCode", documentContactTypeCode, managerAsContext.DocumentContactTypeCode);
				AssertEquals("DocumentDeliveryMode", documentDeliveryMode, managerAsContext.DocumentDeliveryMode);
				AssertEquals("DocumentDirection", documentDirection, managerAsContext.DocumentDirection);
				AssertEquals("MenuItemPK", menuItemPK, managerAsContext.MenuItemPK);
				AssertEquals("MenuTitle", menuTitle, managerAsContext.MenuTitle);
				AssertEquals("ReportName", reportName, managerAsContext.ReportName);
			});
		}

		public void TestWeCanAccessOnlyMenuLevelDataBeforeTemplateContantsLoadedIn()
		{
			ZGuid brandedOrganisationPK = ZGuid.NewZGuid();
			ZGuid contactOrganisationPK = ZGuid.NewZGuid();
			ZString documentContactTypeCode = "CNE";
			ZString documentDirection = "IMP";
			ZGuid menuItemPK = ZGuid.NewZGuid();
			ZString menuTitle = "Menu Title";

			var manager = Factory.GetDocWrapperContextManager();
			manager.SetupDocWrapperContextFromDocumentPack(documentDirection, menuItemPK, menuTitle, documentContactTypeCode, new MockOrgHeader(brandedOrganisationPK), new MockOrgHeader(contactOrganisationPK));

			var managerAsContext = (IDocWrapperContext)manager;
			CombineAssertions(delegate
			{
				AssertEquals("DocumentDirection", documentDirection, managerAsContext.DocumentDirection);
				AssertEquals("MenuItemPK", menuItemPK, managerAsContext.MenuItemPK);
				AssertEquals("MenuTitle", menuTitle, managerAsContext.MenuTitle);
				AssertEquals("DocumentContactTypeCode", documentContactTypeCode, managerAsContext.DocumentContactTypeCode);
				AssertEquals("BrandedOrganisationPK", brandedOrganisationPK, managerAsContext.BrandedOrganisationPK);
				AssertEquals("ContactOrganisationPK", contactOrganisationPK, managerAsContext.ContactOrganisationPK);
			});

			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.DocumentDeliveryMode; });
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ var value = managerAsContext.ReportName; });
		}

		class MockOrgHeader : IOrganisationData
		{
			internal MockOrgHeader(ZGuid pK)
			{
				this.PK = pK;
			}

			public ZGuid PK
			{
				get;
				private set;
			}

			#region IOrganisationData Members Left Blowing Exceptions as they are not required here.

			ZString IOrganisationData.Address1
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Address2
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.City
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Code
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Email
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Fax
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.FullName
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Mobile
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Phone
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Postcode
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.State
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.UNLOCO
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			ZString IOrganisationData.Web
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			#endregion
		}

		public abstract class ContactTypeForTest : IContactType
		{
			public abstract ContactBrandingType BrandingType { get; }

			public override string ToString()
			{
				return string.Empty;
			}
		}
	}
}
