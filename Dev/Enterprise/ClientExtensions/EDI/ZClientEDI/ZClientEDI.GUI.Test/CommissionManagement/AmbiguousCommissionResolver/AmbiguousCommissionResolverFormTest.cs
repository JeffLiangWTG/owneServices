using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(AmbiguousCommissionResolverForm))]
	class AmbiguousCommissionResolverFormTest : ZFormBasherTest
	{
		#region Find

		public void TestFind_WithFilterErrors()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				filterBizObj.IsResolvingFiltered = true;
				filterBizObj.InvoicePkToResolve = ZGuid.Empty;
				filterBizObj.RunPreSaveValidation();
				AssertEquals("Precondition", true, filterBizObj.HasErrors);

				form.FindButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Errors...", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Invoice: Please enter an Invoice.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("ZFormModaliser.LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
				});
			}
		}

		public void TestFind_ReturningNoResults()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				filterBizObj.IsResolvingUnresolved = true;
				filterBizObj.RunPreSaveValidation();
				AssertEquals("Precondition", false, filterBizObj.HasErrors);

				form.FindButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Find Result", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "No ambiguous commissions found.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("ZFormModaliser.LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
				});
			}
		}

		public void TestResolveItemsAreLoadedOnFormShow()
		{
			var opportunityAgreement = CreateOrgCommissionAgreement();
			var unresolvedCommission = CreateAccAmbiguousCommission(opportunityAgreement);
			unresolvedCommission.AC0_CA0_SelectedAgreement = ZGuid.Empty;
			Factory.Save();

			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				AssertContainsExactElementsInAnyOrder(unresolvedCommission, resolver.ResolveItems.Select(x => x.AmbiguousCommission));
			}
		}

		#endregion

		#region Resolve

		public void TestResolve()
		{
			var opportunityAgreement = CreateOrgCommissionAgreement();
			var unresolvedCommission = CreateAccAmbiguousCommission(opportunityAgreement);
			unresolvedCommission.AC0_CA0_SelectedAgreement = ZGuid.Empty;
			Factory.Save();

			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				AssertEquals("Precondition", 1, resolver.ResolveItemCollection.Count);
				resolver.ResolveItemCollection[0].SelectedAgreementPk = opportunityAgreement.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ResolveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Resolve Commissions", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Are you sure you want to resolve 1 ambiguous commissions?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("ZFormModaliser.LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ResolveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertType("ZFormModaliser.LastFormShownForTest", typeof(ProgressForm), ZFormModaliser.LastFormShownForTest);
				});
			}
		}

		public void TestResolve_WithoutAnyResolutions()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				AssertEquals("Precondition", 0, resolver.ResolveItemCollection.Count);

				form.ResolveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cannot Resolve", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "No commissions have been provided with a resolution.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("ZFormModaliser.LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
				});
			}
		}

		public void TestResolve_ConcurrencyException()
		{
			var opportunityAgreement1 = CreateOrgCommissionAgreement();
			var opportunityAgreement2 = CreateOrgCommissionAgreement();
			var unresolvedCommission = CreateAccAmbiguousCommission(null);
			unresolvedCommission.AC0_CA0_SelectedAgreement = ZGuid.Empty;
			Factory.Save();

			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				AssertEquals("Precondition", 1, resolver.ResolveItemCollection.Count);
				resolver.ResolveItemCollection[0].SelectedAgreementPk = opportunityAgreement1.PK;

				var otherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(otherFactory))
				{
					var unresolveCommissionInOtherFactory = otherFactory.Load<AccAmbiguousCommission>(unresolvedCommission.PK);
					unresolveCommissionInOtherFactory.AC0_CA0_SelectedAgreement = opportunityAgreement2.PK;
					otherFactory.Save();
				}

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ResolveButton_Exposed.PerformClick();

				AssertEquals("LastMessage.Caption", "WARNING", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(0, resolver.ResolveItemCollection.Count);
			}
		}

		#endregion

		#region Invoice Filters

		public void TestInvoiceFiltersVisibility()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);

			filterBizObj.CompanyPk = GlbCompany.CurrentCompany.PK;
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				AssertEquals(true, form.InvoicePkToResolveGuidFindBox_Exposed.Visible);
				AssertEquals(false, form.InvoiceNumberToResolveTextBox_Exposed.Visible);

				filterBizObj.CompanyPk = Factory.New<GlbCompany>().PK;
				AssertEquals(false, form.InvoicePkToResolveGuidFindBox_Exposed.Visible);
				AssertEquals(true, form.InvoiceNumberToResolveTextBox_Exposed.Visible);
			}
		}

		#endregion

		#region PossibleOverallItemsGrid

		public void TestPossibleOverallItemsGrid_ContextMenu()
		{
			var opportunityAgreement = CreateOrgCommissionAgreement();
			var unresolvedCommission = CreateAccAmbiguousCommission(opportunityAgreement);
			unresolvedCommission.AC0_CA0_SelectedAgreement = ZGuid.Empty;
			Factory.Save();

			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();

				AssertEquals("Precondition", 1, resolver.ResolveItemCollection.Count);
				AssertEquals("Precondition", 1, resolver.ResolveItemCollection[0].MatchingOverallItems.Count);

				var viewAgreementMenuItem = form.PossibleOverallItemsGrid_Exposed.ContextMenu.MenuItems[0];
				AssertEquals("&View Commission Agreement", viewAgreementMenuItem.Text);
			}

			using (var form = new AmbiguousCommissionResolverFormForTest(resolver))
			{
				form.Show();
				var viewAgreementMenuItem = form.PossibleOverallItemsGrid_Exposed.ContextMenu.MenuItems[0];

				var formsCreated = new List<ZForm>();
				EventHandler formCreatedHandler = (sender, e) =>
				{
					formsCreated.Add((ZForm)sender);
				};
				ZForm.FormCreated += formCreatedHandler;

				try
				{
					UnitTestUserNotification.Instance.ClearMessages();
					viewAgreementMenuItem.PerformClick();
					var oppForm = formsCreated.FirstOrDefault(f => f is OpportunityForm);

					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertNotNull(oppForm);
					AssertEquals(ControllerIDs.Opportunity, oppForm.ControllerID);
				}
				finally
				{
					formsCreated.ForEach(f => f.Dispose());
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		#endregion

		#region Form Captions

		public void TestFormVerb()
		{
			using (var form = new AmbiguousCommissionResolverForm())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			var resolver = new AmbiguousCommissionResolver(filterBizObj);
			return new AmbiguousCommissionResolverForm(resolver);
		}

		#endregion

		#region Implement

		AccChargeCode NewChargeCodeWithDefaultItem(ZString product, ZString service, ZString subModule)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();
			result.AC_IsCommissionable = true;
			result.AC_DefaultCommissionProduct = product;
			result.AC_DefaultCommissionService = service;
			result.AC_DefaultCommissionSubModule = subModule;
			return result;
		}

		TestObjectCreator TestObjectCreator;
		AccChargeCode ChargeCode;
		OrgHeader Customer;
		OrgOpportunity Opportunity;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			ChargeCode = NewChargeCodeWithDefaultItem("ALL", "ALL", "ALL");
			Customer = Factory.NewWithValidTestData<OrgHeader>();
			Opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, Customer);
		}

		AccAmbiguousCommission CreateAccAmbiguousCommission(OrgCommissionAgreement agreement)
		{
			var commission = Factory.NewWithValidTestData<AccAmbiguousCommission>();
			TestObjectCreator.CreateARInvoiceLine(commission.Source as ARInvoice, null, ChargeCode, null, 1, "", 1);

			commission.AC0_CA0_SelectedAgreement = agreement?.PK ?? ZGuid.Empty;
			commission.AC0_CommissionStream = agreement?.CA0_CommissionStream ?? "WBP";
			commission.Source.AH_OH = Customer.PK;

			return commission;
		}

		OrgCommissionAgreement CreateOrgCommissionAgreement()
		{
			var agreement = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(Opportunity, Customer, "ALL", "ALL", "ALL");
			agreement.CA0_CommissionStream = "WBP";
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement, "ADL", 10);
			return agreement;
		}

		#endregion
	}

	#region Classes

	class AmbiguousCommissionResolverFormForTest : AmbiguousCommissionResolverForm
	{
		public AmbiguousCommissionResolverFormForTest(AmbiguousCommissionResolver resolver)
			: base(resolver)
		{
		}

		public ZTextBox InvoiceNumberToResolveTextBox_Exposed
		{
			get { return InvoiceNumberToResolveTextBox; }
		}

		public ZGuidFindBox InvoicePkToResolveGuidFindBox_Exposed
		{
			get { return InvoicePkToResolveGuidFindBox; }
		}

		public ZButton FindButton_Exposed
		{
			get { return FindButton; }
		}

		public ZButton ResolveButton_Exposed
		{
			get { return ResolveButton; }
		}

		public ZGrid PossibleOverallItemsGrid_Exposed
		{
			get { return PossibleOverallItemsGrid; }
		}
	}

	#endregion
}
