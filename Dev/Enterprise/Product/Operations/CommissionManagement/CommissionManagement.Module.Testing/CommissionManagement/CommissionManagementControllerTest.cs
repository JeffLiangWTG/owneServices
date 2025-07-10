using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing
{
	[TestedType(typeof(CommissionManagementController))]
	internal class CommissionManagementControllerTest : ZControllerBasherTest
	{
		#region Standard Overrides

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Commission;
		}

		#endregion

		#region ShowViewForm

		public override void TestViewForm()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			Factory.Save();

			var jobLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var invoiceLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			invoiceLine.VCL_GroupingSourceID = invoice.PK;
			invoiceLine.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			var jobGrouping = GetNewGrouping(Factory, new[] { jobLine });
			var invoiceGrouping = GetNewGrouping(Factory, new[] { invoiceLine });
			var invalidGrouping = GetNewGrouping(Factory, new[] { Factory.NewWithValidTestData<ViewCommissionLine>() });

			var deletedJobLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			deletedJobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			deletedJobLine.VCL_GroupingSourceID = ZGuid.NewZGuid();
			var deletedJobGrouping = GetNewGrouping(Factory, new[] { deletedJobLine });

			var controller = ZControllerFactory.Create(ControllerIDs.Commission);
			using (var jobForm = controller.ShowViewForm(jobGrouping))
			{
				AssertNotNull(jobForm);
				AssertEquals(ControllerIDs.JobManagement, jobForm.ControllerID);
			}

			using (var invoiceForm = controller.ShowViewForm(invoiceGrouping))
			{
				AssertNotNull(invoiceForm);
				AssertEquals(ControllerIDs.ARInvoice, invoiceForm.ControllerID);
			}

			using (var form = controller.ShowViewForm(invalidGrouping))
			{
				AssertNull(form);
			}

			using (var form = controller.ShowViewForm(deletedJobGrouping))
			{
				AssertNull(form);
				AssertEquals("Message should show when user clicks job with missing parent.", "This job cannot be opened as the corresponding Job Header has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region ShowEditForm

		public override void TestEditForm()
		{
			AssertNotNull("EditForm should not be null", Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()));

			var deletedJobLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			deletedJobLine.VCL_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			deletedJobLine.VCL_GroupingSourceID = ZGuid.NewZGuid();
			var deletedJobGrouping = GetNewGrouping(Factory, new[] { deletedJobLine });

			using (var form = ZControllerFactory.Create(ControllerIDs.Commission).ShowViewForm(deletedJobGrouping))
			{
				AssertNull(form);
				AssertEquals("Message should show when user clicks job with missing parent.", "This job cannot be opened as the corresponding Job Header has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var party = Factory.NewWithValidTestData<OrgHeader>();
			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			var commissionLine = commissionHeader.Lines.AddNew();
			commissionLine.FillWithValidTestData();
			Factory.Save();

			var viewCommissionLine = Factory.Load<ViewCommissionLine>(commissionLine.PK);
			return GetNewGrouping(Factory, new[] { viewCommissionLine });
		}

		#endregion

		#region Implementation

		static ViewCommissionLineGrouping GetNewGrouping(BusinessObjectFactory factory, IEnumerable<ViewCommissionLine> commissionLines)
		{
			var grouping = new ViewCommissionLineGrouping(factory);
			grouping.Init(commissionLines);
			return grouping;
		}

		#endregion
	}
}
