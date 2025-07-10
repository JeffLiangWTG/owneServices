using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BF;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CancellationMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public CancellationMessageSendingObject(CusEntryHeader entry)
			: base(entry, ElectronicDocumentTypeList.Codes._5BF)
		{
		}

		public new class Schema : JobDeclarationMiscMessageSendingObjectCore.Schema
		{
			public const string CancellationReason = "CancellationReason";
			public const int CancellationReasonMaxLength = 500;
		}

		[MaxLength(Schema.CancellationReasonMaxLength)]
		[ResourceStringData("6AF21E59-A3E3-40DB-9E91-FDC5757F4B4B", Caption = "Cancellation Reason")]
		public ZString CancellationReason
		{
			get => cancellationReason;
			set
			{
				CheckMaximumLength(CancellationReasonInfo, value);
				SetNonPersistentPropertyValue(CancellationReasonInfo, ref cancellationReason, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCancellationReason();
				}
			}
		}
		ZString cancellationReason;

		public ZPropertyInfo CancellationReasonInfo => GetZPropertyInfo(Schema.CancellationReason);

		public ZDateTime DateOfApplication { get; set; }

		public void Populate(EDIMessage message)
		{
			if (message.EM_MessageType != ElectronicDocumentTypeList.Codes._5BF)
			{
				throw new ArgumentException("expected to receive an EDIMessage of '5BF'");
			}

			Declaration declaration = null;

			using (var reader = message.GetEM_MessageTextReader())
			{
				declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Declaration>(reader);
			}
			using (GetValidationSuspender())
			{
				CancellationReason = declaration.Reason.Value;
				if (DateTime.TryParseExact(declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					DateOfApplication = new ZDateTime(dt);
				}

				HasChanges = false;
			}
		}

		public new CancellationMessageSendingObjectValidation Validation => (CancellationMessageSendingObjectValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new CancellationMessageSendingObjectValidation(this);
	}
}
