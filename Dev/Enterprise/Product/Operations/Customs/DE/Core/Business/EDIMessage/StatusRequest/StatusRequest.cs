using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class StatusRequest : EDIMessage
	{
		public new class Schema : EDIMessage.Schema
		{
			public const string Module = nameof(StatusRequest.Module);
			public const string MovementReferenceNumber = nameof(StatusRequest.MovementReferenceNumber);
			public const string Response = nameof(StatusRequest.Response);
		}
		public StatusRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
			EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			EM_MessageSubType = NctsMessageSubTypeList.Codes.StatusRequestMessage;
			EM_ReceiveTransmit = Direction.Transmit;
		}

		[BusinessObjectTestExclude]
		[MaxLength(4)]
		[ResourceStringData("139D0AF9-CFEA-4D8A-974C-86EFF1984B0C", Caption = "Module")]
		[List(nameof(Lookups) + "." + nameof(StatusRequestLookups.ModuleList))]
		public ZString Module
		{
			get
			{
				return Factory.GetValue(ref module,
					() => EM_ApplicationCode == ApplicationCodes.DECustomsAtlasSystem
						? ExportStatusRequestModuleCodeList.Codes.NCTS
						: ExportStatusRequestModuleCodeList.Codes.AES);
			}
			set
			{
				var oldValue = Module;
				if (value != oldValue)
				{
					var isNcts = value == ExportStatusRequestModuleCodeList.Codes.NCTS;
					EM_ApplicationCode = isNcts ? ApplicationCodes.DECustomsAtlasSystem : ApplicationCodes.DECustomsAesSystem;
					EM_MessageType = isNcts ? EDIMessageTypeList.Codes.NCTS : EDIMessageTypeList.Codes.AES;
					EM_MessageSubType = isNcts ? NctsMessageSubTypeList.Codes.StatusRequestMessage : ExportMessageSubTypeList.Codes.EXQ;
					ModuleInfo.RefreshBinding(oldValue);
				}
			}
		}
		CachedProperty<ZString> module;

		public ZPropertyInfo ModuleInfo => GetZPropertyInfo(Schema.Module);

		[MaxLength(18)]
		[ResourceStringData("3A8DDEE6-A87B-4E15-9148-4383FE9BFBD0", Caption = "MRN")]
		public ZString MovementReferenceNumber
		{
			get => MRNCusEntryNumberWrapper.EntryNumber;
			set
			{
				MRNCusEntryNumberWrapper.SetEntryNumber(value, MovementReferenceNumberInfo);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMovementReferenceNumber();
				}
			}
		}

		CusEntryNumberWrapper MRNCusEntryNumberWrapper => mrnCusEntryNumberWrapper ?? (mrnCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Standard.MovementReferenceNumber));
		CusEntryNumberWrapper mrnCusEntryNumberWrapper;

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(Schema.MovementReferenceNumber);

		[ReadOnly(true)]
		[ResourceStringData("9F45B2A7-6680-4352-8BA7-AC9E4CFFB3E2", Caption = "Response?")]
		public ZBool Response => EM_Status == EDIMessage.Status.Acknowledged;

		public ZPropertyInfo ResponseInfo => GetZPropertyInfo(Schema.Response);

		public IEnumerable<EDIMessage> ResponseMessages
		{
			get
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EM_ApplicationCode);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, EM_MessageType);
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EM_MessageSubType);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Direction.Receive);
				query.AddToFilter(EDIMessageSchema.EM_LinkTable, TableName);
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.Load<EDIMessage>(query);
			}
		}

		[ResourceStringData("088EF705-7654-444B-93C0-F4F9676D0AD1", Caption = "Identification")]
		[List(nameof(Lookups) + "." + nameof(StatusRequestLookups.Identifications))]
		public ZGuid Identification
		{
			get => identification;
			set
			{
				var oldValue = Identification;
				if (value != oldValue)
				{
					SetNonPersistentPropertyValue(IdentificationInfo, ref identification, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateIdentification();
					}
				}
			}
		}
		ZGuid identification;

		public ZPropertyInfo IdentificationInfo => GetZPropertyInfo(nameof(Identification));

		public OrgHeader IdentificationOrg => Factory.Load<OrgHeader>(Identification);

		[ResourceStringData("944C37F3-8F8C-4826-8F55-E94558CE2E85", Caption = "Role")]
		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(StatusRequestLookups.RoleList))]
		public ZString Role
		{
			get => role;
			set
			{
				var oldValue = Role;
				if (value != oldValue)
				{
					CheckMaximumLength(RoleInfo, value);
					SetNonPersistentPropertyValue(RoleInfo, ref role, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateRole();
					}
				}
			}
		}
		ZString role;

		public ZPropertyInfo RoleInfo => GetZPropertyInfo(nameof(Role));

		public override void Delete() => throw new NotSupportedException("delete not supported");

		public new StatusRequestLookups Lookups => (StatusRequestLookups)base.Lookups;

		protected override EDIMessageLookups GetNewLookups() => new StatusRequestLookups(this);

		public new StatusRequestValidation Validation => (StatusRequestValidation)base.Validation;

		protected override EDIMessageValidation GetNewValidation() => new StatusRequestValidation(this);

		protected override string GetMessageReferenceNumber() => DEEDIMessageSharedHelpers.GetDEMessageReferenceNumber(Factory);

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			DEEDIMessageSharedHelpers.GetInterchangeControlReferenceAndFillInPlaceHolders(this);
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = base.NoteTypesCore;
				noteTypes.Add(new PredefinedNoteType((NoResString)LogbookHelper.LogbookRegistrationNumberNoteDescription, StmNoteVisibility.INT, true, true, true, true));
				return noteTypes;
			}
		}

		#region IDocManagerSupport Members

		protected override DocManagerInfo GetDocManagerInfoCore() => new StatusRequestDocManagerInfo(this);

		#endregion
	}
}
