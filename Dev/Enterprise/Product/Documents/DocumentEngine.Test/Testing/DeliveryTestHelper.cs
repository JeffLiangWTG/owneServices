using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	static class DeliveryTestHelper
	{
		const string TestEmail = "unit.test@cargowise.com";

		#region Deliver Documents

		internal static StmPrintJob[] DeliverDocument(DocumentCommand documentCommand)
		{
			return DeliverDocument(documentCommand, Enterprise.Core.Constants.Languages.English);
		}

		internal static StmPrintJob[] DeliverDocument(DocumentCommand documentCommand, ZString languageCode)
		{
			return DeliverDocumentViaEmail(documentCommand, null, languageCode, TestEmail, OrgConstants.AttachmentType.XLS);
		}

		internal static StmPrintJob[] DeliverDocumentViaEmail(DocumentCommand documentCommand, UserControlProviderList userFieldList, ZString languageCode, ZString email, ZString attachmentType)
		{
			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Language = languageCode;
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = email;
			recipient.AttachmentType = attachmentType;

			return DeliverDocument(documentCommand, userFieldList, deliveryInstructions);
		}

		internal static StmPrintJob[] DeliverDocument(DocumentCommand documentCommand, UserControlProviderList userFieldList, DeliveryInstructions deliveryInstructions)
		{
			var group = deliveryInstructions.DeliveryGroups[0];
			using (var printSet = new DocumentPrintSet(documentCommand, userFieldList))
			{
				printSet.Run(deliveryInstructions);
			}

			return GetPrintJobs(group);
		}

		#endregion

		#region Deliver Reports

		internal static StmPrintJob[] DeliverReport(ReportCommand reportCommand, string attachmentType = OrgConstants.AttachmentType.XLS)
		{
			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = TestEmail;
			recipient.AttachmentType = attachmentType;

			return DeliverReport(reportCommand, deliveryInstructions);
		}

		internal static StmPrintJob[] DeliverReport(ReportCommand reportCommand, DeliveryInstructions deliveryInstructions)
		{
			using (var printSet = new ReportPrintSet(reportCommand))
			{
				printSet.Run(deliveryInstructions);
			}

			return GetPrintJobs(deliveryInstructions.DeliveryGroups[0]);
		}

		#endregion

		internal static StmPrintJob[] GetPrintJobs(StmDeliveryGroup deliveryGroup)
		{
			var query = new ZQuery(StmPrintJobSchema.SP_SB_DeliveryGroup, deliveryGroup.PK);
			var result = deliveryGroup.Factory.Load<StmPrintJob>(query);

			return result;
		}
	}
}
