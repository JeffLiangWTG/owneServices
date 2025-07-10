using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusStorageDocPivot : BaseCusStorageDocPivot
	{
		public CusStorageDocPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusStorageDocPivot.Schema
		{
			public const string DocumentExtension = "DocumentExtension";
			public const string DocumentSize = "DocumentSize";
			public const string MessageStatus = "MessageStatus";
		}

		protected override Customs.Business.CusStorageDocPivotLookups GetNewLookups()
		{
			return new CusStorageDocPivotLookups(this);
		}

		public new CusStorageDocPivotLookups Lookups => (CusStorageDocPivotLookups)base.Lookups;

		protected override Customs.Business.CusStorageDocPivotValidation GetNewValidation()
		{
			return new CusStorageDocPivotValidation(this);
		}

		public new CusStorageDocPivotValidation Validation => (CusStorageDocPivotValidation)base.Validation;

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				var result = new TypeLoaderCollection();

				result.Add(ObjectFactory.GetType<Integration.Customs.ES.ICusEntryHeader>());

				return result;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusStorageDocPivotLookups.AvailableEDocs), "PK", "Code", AllowOnlyTheseValues = true)]
		public override ZGuid CSD_StorageDocReference
		{
			get => base.CSD_StorageDocReference;
			set
			{
				var oldValue = CSD_StorageDocReference;
				base.CSD_StorageDocReference = value;

				if (!IsCopying && CSD_StorageDocReference != oldValue)
				{
					ResetDocument();
					CSD_DocType = DocumentExtension;
					if (CSD_Description.IsEmpty)
					{
						CSD_Description = Document != null ? Document.Description : ZString.Empty;
					}
				}
			}
		}

		public CusEntryHeader EntryHeader => Parent as CusEntryHeader;

		public virtual CusStorageDocPivotEDIMessageCollection Messages => messages ?? (messages = EntryHeader != null ? new CusStorageDocPivotEDIMessageCollection(this, EntryHeader.Messages) : new CusStorageDocPivotEDIMessageCollection(this, new EDIMessageCollection(this, Factory)));
		CusStorageDocPivotEDIMessageCollection messages;

		public GenPivotCollection MessagePivots => messagePivots ?? (messagePivots = new GenPivotCollection(this, GenPivotTypes.CusStorageDocPivotEdiMessage));
		GenPivotCollection messagePivots;

		public EDIMessage Message
		{
			get
			{
				return Messages?.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			}
		}

		public ZString DocumentSize => Document != null ? AttachmentSize(Document.FileSizeInMB) : ZString.Empty;

		internal static ZString AttachmentSize(ZDecimal lenInMB)
		{
			var len = lenInMB * 1024 * 1024; //Start with len in Bytes
			string[] sizes = { "B", "KB", "MB", "GB" };
			var order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				len /= 1024;
				++order;
			}
			return string.Format(CultureInfo.CurrentCulture, "{0:0.###}{1}", len, sizes[order]);
		}

		public ZString DocumentExtension
		{
			get
			{
				var dataType = Document?.DataType ?? ZString.Empty;
				return dataType;
			}
		}
		public ZString MessageStatus => Message?.EM_Status ?? ZString.Empty;

		public override bool ReadOnly
		{
			get => ((EntryHeader?.IsT2L ?? false) || (EntryHeader?.RequiresAESAnnexes ?? false)) && (IsAccepted || IsSentWithoutResponse);
			set => base.ReadOnly = value;
		}

		public bool IsAccepted => MessageStatus == EDIMessageStatusList.Codes.Received;

		public bool IsSentWithoutResponse => Message != null
											&& Message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
											&& Message.EM_Status != EDIMessage.Status.Rejected
											&& Message.EM_Status != EDIMessage.Status.Cancelled
											&& Message.EM_Status != EDIMessage.Status.Received
											&& Message.EM_Status != EDIMessage.Status.Failed
											&& Message.EM_Status != EDIMessage.Status.Error
											&& Message.EM_Status != MessageStatusList.Codes.FailedFromTransmission
											&& !(Message.AssumeMessageClearIfAcknowledgedAndNoResponse && Message.HasHadItsInterchangeAcknowledged);
	}
}
