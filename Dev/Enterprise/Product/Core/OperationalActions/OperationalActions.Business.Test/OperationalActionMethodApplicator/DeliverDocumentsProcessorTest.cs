using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.DocumentEngine.DocumentMenu.Testing.DocumentCommandTest.DocDummyBusinessObject;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	class DeliverDocumentsProcessorTest : TestCaseWithFactory
	{
		public void TestProcessDocumentHasCorrectEmailRecipientsWhenSetUpJobSpecificRecipientsAndDifferentDeliveryLanguageButOrganizationAddressIsEmpty()
		{
			DocumentDelivery.JDC_EmailToRecipientsAsString = "j26@email.com";
			DocumentDelivery.JDC_CarbonCopyRecipientsAsString = "j26cc@email.com";
			Factory.Save();

			Processor.ProcessDocument();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, SupporterParent.PK));
			var recipients = Factory.Load<StmPrintJobCopyRecipient>(new ZQuery(StmPrintJobCopyRecipientSchema.SPR_SP, printJob.PK));
			AssertEquals(2, recipients.Length);
			Assert(recipients.All(r => r.SPR_EmailAddress == "j26@email.com" || r.SPR_EmailAddress == "j26cc@email.com"));
		}

		public void TestProcessDocumentOnlyHasCCRecipient()
		{
			DocumentDelivery.JDC_CarbonCopyRecipientsAsString = "j26cc@email.com";
			Factory.Save();

			Processor.ProcessDocument();

			var expectedLog = "WARNING: There is no Email Recipient found when delivering document 'Arrival Notice'\r\n('DummyBizo')";
			AssertMultilineASCIIEquals("Should warning if only has CCRecipient without Recipient", expectedLog, string.Join("\n", ActionLog.messages.ToArray()));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "A Fantastic Place";

			SupporterParent = Factory.NewWithValidTestData<DummyBusinessObjectWithDocumentSupportToTestProcessor>();
			SupporterParent.Z0_Guid = org.PK;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Arrival Notice";
			documentCommand.SU_ContactType = ContactType.Consignee.Code;
			documentCommand.SU_MenuPath = "Arrival/Arrival Notice";
			documentCommand.SU_PreventAutoDelivery = false;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test1",
@"{A}-[#Config]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(DataContext.Shipment);
			var menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = "Test Document";
			menuTemplatePivot.SI_SU = documentCommand.PK;
			menuTemplatePivot.SI_SO = template.PK;

			DocumentDelivery = Factory.New<JobDocumentDelivery>();
			DocumentDelivery.JDC_ParentID = SupporterParent.PK;
			DocumentDelivery.JDC_SU_MenuItem = documentCommand.PK;
			DocumentDelivery.JDC_DeliveryMethod = "EML";
			DocumentDelivery.JDC_OH = org.PK;

			var action = Factory.New<OperationalAction>();
			var pivot1 = action.DocumentPivots.AddNew();
			var actionSupportable = new MockOperationalActionSupportable();
			action.Context = new OperationalActionContext(actionSupportable.OperationalActionSupporter, "Module Name");
			pivot1.SF_SU_Outward = documentCommand.PK;

			var runner = new OperationalActionRunner(action, typeof(DummyBusinessObjectWithDocumentSupportToTestProcessor), new SelectedRecords(SupporterParent.PK));
			runner.Printer = Factory.NewWithValidTestData<StmPrintQueue>().PK;
			runner.DocumentPrintLanguage = "ZH-CN";
			Factory.Save();

			Processor = new DeliverDocumentsProcessor(runner);
			ActionLog = new DummyOperationalActionLog();
			Processor.InitializeProcessor(ActionLog, new ZGuid[] { SupporterParent.PK }, typeof(DummyBusinessObjectWithDocumentSupportToTestProcessor));
		}

		class DummyBusinessObjectWithDocumentSupportToTestProcessor : DummyBusinessObjectWithDocumentSupport, ISupportJobDocumentRecipient
		{
			public DummyBusinessObjectWithDocumentSupportToTestProcessor(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get
				{
					return new SupporterToTestProcessor(this);
				}
			}

			IEnumerable<(MultilingualString organisationType, IOrgHeader orgHeader)> ISupportJobDocumentRecipient.SuggestedOrganisations
			{
				get
				{
					return new List<(MultilingualString organisationType, IOrgHeader orgHeader)>
					{
						((NoResString)"Nothing", Factory.Load<OrgHeader>(Z0_Guid))
					};
				}
			}

			public class SupporterToTestProcessor : Supporter
			{
				public SupporterToTestProcessor(DummyBusinessObjectWithDocumentSupport parent) : base(parent)
				{
				}

				protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
				{
					return new DocumentWrapper[] { new DummyWrapper() };
				}

				public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
				{
					var org = Factory.Load<OrgHeader>(((DummyBusinessObjectWithDocumentSupportToTestProcessor)BusinessObject).Z0_Guid);
					return new OrgHeaderContact(org, null);
				}
			}
		}

		DeliverDocumentsProcessor Processor;
		DummyBusinessObjectWithDocumentSupportToTestProcessor SupporterParent;
		JobDocumentDelivery DocumentDelivery;
		DummyOperationalActionLog ActionLog;
		#endregion
	}
}
