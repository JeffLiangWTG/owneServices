namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	#region SuppressResourceStringsCheckRegion

	public static class UniversalEventMessageProcessorConstants
	{
		public static class ContextType
		{
			public const string IsTest = "IsTest";
			public const string OrganizationReference = "OrganizationReference";
			public const string NoticeDocumentTypeCode = "NoticeDocumentTypeCode";
			public const string CurrentReponseSequence = "CurrentReponseSequence";
			public const string TotalResponsesCount = "TotalResponsesCount";
			public const string InterchangeNumber = "InterchangeNumber";
			public const string MessageNumber = "MessageNumber";
			public const string SentRawMessage = "SentRawMessage";
			public const string ResponseRawMessage = "ResponseRawMessage";

			public static class RelatedDocument
			{
				public const string Name = "RelatedDocument";
				public static class SubContextType
				{
					public const string DocumentType = "DocumentType";
					public const string DocumentNumber = "DocumentNumber";
				}
			}

			public static class InspectionPGA
			{
				public const string Name = "InspectionPGA";
				public static class SubContextType
				{
					public const string InspectionPort = "InspectionPort";
					public const string InspectionWarehouse = "InspectionWarehouse";
					public const string InspectionLocationOther = "InspectionLocationOther";

					public const string Type = "Type";
					public const string Port = "Port";
					public const string Code = "Code";

					public static class PGAContact
					{
						public const string Name = "PGAContact";
						public static class SubContextType
						{
							public const string PGAContactPhone = "PGAContactPhone";
							public const string PGAContactEmail = "PGAContactEmail";
							public const string PGAContactFax = "PGAContactFax";
						}
					}
				}
			}

			public static class Status
			{
				public const string Name = "Status";
			}

			public const string NoticeRecipientType = "NoticeRecipientType";
			public const string NoticeRecipientReferenceNumber = "NoticeRecipientReferenceNumber";

			public static class CloseMessageHouseBills
			{
				public const string Name = "CloseMessageHouseBills";
				public static class SubContextType
				{
					public const string DocumentID = "DocumentID";
					public const string DocumentType = "DocumentType";
				}
			}

			public static class RequestingPGA
			{
				public const string Name = "RequestingPGA";
				public static class SubContextType
				{
					public const string RequestingReviewComments = "RequestingReviewComments";
					public const string RequestingSpecialInstructions = "RequestingSpecialInstructions";
					public const string RequestingErrorDescription = "RequestingErrorDescription";
				}
			}

			public static class ErrorDetail
			{
				public const string Name = "ErrorDetail";
				public static class SubContextType
				{
					public const string ErrorDescription = "ErrorDescription";
					public const string ErrorLocation = "ErrorLocation";
				}
			}

			public static class ContainerDetails
			{
				public const string Name = "ContainerDetails";
				public static class SubContextType
				{
					public const string ContainerNumber = "ContainerNumber";
				}
			}

			public const string RawMessage = "RawMessage";
		}

		public static class XmlBoolValues
		{
			public const string True = "Y";
			public const string False = "N";
		}

		public static class NoticeRecipientType
		{
			public const string WarehouseOperator = "Warehouse Operator";
		}
	}

	#endregion

}
