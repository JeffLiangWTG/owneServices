using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks.Testing
{
	class AsycudaIncomingGmdMessageTestScenario
	{
		public AsycudaIncomingGmdMessageTestScenario(JobDeclaration dec, string scenarioType = null)
		{
			Declaration_Reference = dec.ActiveEntryHeaders[0].CH_BGMReference;
			Type = scenarioType;

			if (scenarioType == null || scenarioType == SUCCESS_TYPE_NO_IDENTATION_BODY)
			{
				Expected_Interchange_Status = EDIInterchange.Status.Received;

				Expected_EmailSubject = "Asycuda World Brokerage File – " + Declaration_Reference;
				Expected_EmailAttachmentFileName = Declaration_Reference + "_" + dec.JE_AgentsReference + "_Asycuda.xml";

				Expected_EmailBody = "Dear Client,\r\n"
									+ "Enclosed please find the mapped ASYCUDA World Brokerage file.\r\n"
									+ "\r\n"
									+ "Regards,\r\n"
									+ "WiseTech Global";
			}
			else
			{
				Expected_Interchange_Status = EDIInterchange.Status.Failed;

				switch (scenarioType)
				{
					case FAILURE_TYPE_EMPTY_BODY:
						Failure_Message = "The body of the interchange is empty.";
						break;
					case FAILURE_TYPE_NO_DEC_REF:
						Failure_Message = "Unable to extract the declaration reference from the Asycuda message.";
						break;
					case FAILURE_TYPE_DEC_NOT_FOUND:
						Failure_Message = "Declaration not found for declaration reference An_Invalid_Declaration_Reference.";
						break;
				}
			}
		}

		public ZString Declaration_Reference { get; set; }
		public ZString Expected_AsycudaMessageText { get; set; }
		public GlbStaff User_ToReceiveEmail { get; set; }
		public ZString Expected_EmailSubject { get; set; }
		public ZString Expected_EmailAttachmentFileName { get; set; }
		public ZString Expected_EmailBody { get; set; }
		public string MessageNo { get; set; }
		public ZGuid InterchangePK { get; set; }
		public ZString Expected_Interchange_Status { get; set; }
		public string Type { get; set; }
		public ZString Failure_Message { get; set; }

		public const string FAILURE_TYPE_EMPTY_BODY = "EMPTY_BODY";
		public const string FAILURE_TYPE_NO_DEC_REF = "NO_DEC_REF";
		public const string FAILURE_TYPE_DEC_NOT_FOUND = "DEC_NOT_FOUND";
		public const string SUCCESS_TYPE_NO_IDENTATION_BODY = "NO_IDENTATION_BODY";
	}
}
