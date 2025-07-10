using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsHeader : EU.NCTS.Business.NctsHeader
		, Integration.Customs.IENCTS.ICusInBondHeader
		, IMessageAttachee
		, IRelatedJob
		, ISupportingDocObject
		, Integration.Customs.EU.NCTS.INctsHeaderWithAdditionalMessagingValidation
	{
		public NctsHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments => (ICommonPreviousDocumentCollection<CommonPreviousDocument>)base.PreviousDocuments;

		protected override ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments() => new CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

		public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

		public new NctsHeaderValidation Validation => (NctsHeaderValidation)base.Validation;

		protected override CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderValidation(this);

		public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

		public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;
		protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

		public new INctsBillCollection<NctsBill> Bills => (INctsBillCollection<NctsBill>)base.Bills;
		protected override INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection() => new NctsBillCollection<NctsBill>(this);
		protected override Type BillTypeCore => typeof(NctsBill);

		public string GetLRNAndSetIfNeeded()
		{
			var lrn = string.Empty;
			if (MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				lrn = new IE.Business.LRNGenerator(Factory, Branch).GetLRNAndSetIfNeeded((ZPropertyInfoString)movementHeader.BM_PaperlessInbondNumInfo, () => false); // TODO: Need to check for syntax error for NCTS just like in AES
			}
			return lrn;
		}

		protected override ZString GetPermitReferenceCore() => MovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty;

		protected override IList<PermitRecord> GetPermitRecordsCore()
		{
			var permitRecords = base.GetPermitRecordsCore();

			var guaranteesNoLongerInDeclaration = GetGuaranteesNoLongerInDeclaration();
			foreach (var guarantee in guaranteesNoLongerInDeclaration)
			{
				var permitRecord = new PermitRecord();
				permitRecord.PermitHeader = guarantee;
				permitRecord.Quantity = 0;
				permitRecord.Value = 0;
				permitRecords.Add(permitRecord);
			}

			return permitRecords;
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(CommonPreviousDocument);
			return result;
		}

		IE.Business.CusGuaranteeHeader[] GetGuaranteesNoLongerInDeclaration()
		{
			var guaranteeHeaderQuery = new ZDBOnlyQuery(typeof(IE.Business.CusGuaranteeHeader));
			guaranteeHeaderQuery.AddToFilter(ZArchitecture.Schema.CusPermitHeaderSchema.CPH_Number, SQLComparisonOperator.NotEqual, IsPhase5Departure ? MovementHeader.Guarantees.Select(x => x.PW_BondNumber) : Guarantees.Select(x => x.PW_BondNumber));

			var transactionQuery = new ZDBOnlySubQuery(typeof(SharedCusPermitLineTransaction), ZArchitecture.Schema.CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			transactionQuery.AddToFilter(ZArchitecture.Schema.CusPermitLineTransactionSchema.CPL_Reference, GetPermitReference());
			guaranteeHeaderQuery.AddSubQuery(transactionQuery, JoinCondition.And);
			return Factory.Load<IE.Business.CusGuaranteeHeader>(guaranteeHeaderQuery);
		}

		protected override ZString DefaultApplicationCode => CusInBondApplicationCodeList.Codes.NCTS5;

		public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;
		protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new NctsGuaranteeCollection<NctsGuarantee>(this);

		public SupportingDocSendingObject GetSupportingDocSendingObject() => new DocumentSendingObject(this);

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch => Branch;
		GlbStaff IMessageAttachee.CustomsAgent => null; // TODO: should return Customs Agent
		IRelatedJob IMessageAttachee.RelatedJob => this;
		ZString IMessageAttachee.LogicalStatus { get => EffectiveMessageStatus; set => EffectiveMessageStatus = value; }
		ZString IMessageAttachee.EntryStatus { get => CommonMovementHeader.BM_CustomsStatus; set => CommonMovementHeader.BM_CustomsStatus = value; }
		IEnumerable<EDIMessage> IMessageAttachee.Messages => Messages.Cast<EDIMessage>();

		#endregion

		protected override EU.NCTS.Business.NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);

		protected override bool IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesCore() => IsPhase5Departure && Configuration.ValidationRuleConfiguration.IsRuleR0520Active && !MovementReferenceNumber.IsEmpty && !MessageStaticHelper.CanAmendGuarantees(this);

		protected override bool IsConditionR0520_UserShouldNotSaveAmendmentsCore() => !MovementReferenceNumber.IsEmpty && base.IsConditionR0520_UserShouldNotSaveAmendmentsCore();

		protected override bool IsUnloadingAllowedOrCompleteCore => base.IsUnloadingAllowedOrCompleteCore || UnloadingPermissionReceived;

		bool UnloadingPermissionReceived => Messages.GetLastMessage(
				applicationCode: EDIMessage.ApplicationCodes.IECustomsNCTS,
				messageType: NCTSIncomingMessageTypeList.Codes.IE043,
				tRXorRCV: EDIMessage.Direction.Receive,
				status: EDIMessage.Status.ProcessedOK
			) != null;

		public string[] GetAdditionalValidationErrorMessages()
		{
			var credentialError = Company.ValidateROSCredential();
			return string.IsNullOrEmpty(credentialError) ? [] : [credentialError];
		}
	}
}
