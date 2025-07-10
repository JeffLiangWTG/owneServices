using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderCustomsLinkedObjectAdapter : ISadCustomsLinkedObjectAdapter
	, ISingleWindowCustomsLinkedObjectAdapter
{
	public NctsHeaderCustomsLinkedObjectAdapter(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		CheckNctsHeaderIsDepartureJob(nctsHeader);
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
	}

	readonly NctsHeader nctsHeader;
	readonly NctsDepartureMovementHeader movementHeader;

	#region ISadCustomsLinkedObjectAdapter Members

	ZString ISadCustomsLinkedObjectAdapter.EntryCustomsStatus => movementHeader.BM_CustomsStatus;

	ISadCustomsStatusProvider ISadCustomsLinkedObjectAdapter.StatusProvider => StatusProvider;

	ZBool ISadCustomsLinkedObjectAdapter.IsImport => false;

	ZBool ISadCustomsLinkedObjectAdapter.IsExport => true;

	ZBool ISadCustomsLinkedObjectAdapter.IsEntryRegisteredOrNbRejected
	{
		get
		{
			var customsStatus = movementHeader.BM_CustomsStatus;
			return customsStatus == NctsTransitStatusList.Codes.DeclarationMrnAllocated || customsStatus == NctsTransitStatusList.Codes.NbRejected;
		}
	}

	IEnumerable<ISadCustomsLineLinkedObjectAdapter> ISadCustomsLinkedObjectAdapter.CustomsLines => customsLines ?? (customsLines = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Select(x => new NctsDepartureCargoDescCustomsLineLinkedObjectAdapter(x)));
	IEnumerable<ISadCustomsLineLinkedObjectAdapter> customsLines;

	ISingleWindowRequestDataProvider ISadCustomsLinkedObjectAdapter.SingleWindowRequestDataProvider => nctsHeader;

	ZString ISadCustomsLinkedObjectAdapter.Mrn => nctsHeader.MovementReferenceNumber;

	CusEntryNumber ISadCustomsLinkedObjectAdapter.IrildesCusEntryNum => nctsHeader.EntryNumbersProvider.Irildes;

	CusEntryNumber ISadCustomsLinkedObjectAdapter.IvistoCusEntryNum => nctsHeader.EntryNumbersProvider.Ivisto;

	IEnumerable<CusEntryNumber> ISadCustomsLinkedObjectAdapter.GetEntryNumbers()
	{
		var entryNumberQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, nctsHeader.CountryCode);
		entryNumberQuery.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
		return nctsHeader.Factory.Load<CusEntryNumber>(entryNumberQuery);
	}

	CusEntryNumber ISadCustomsLinkedObjectAdapter.GetNewCusEntryNumber()
	{
		var entryNumber = nctsHeader.Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = nctsHeader.PK;
		entryNumber.CE_ParentTable = nctsHeader.TableName;
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_RN_NKCountryCode = nctsHeader.CountryCode;
		return entryNumber;
	}

	void ISadCustomsLinkedObjectAdapter.InsertOrUpdateA93Numbers(ISadPositiveResponseMessageA93EntryPayments entryPayments)
	{
		var registerCode = nctsHeader.EntryNumbersProvider.RegistrationInfoWrapper.RegisterIncludingSeries;
		var a93Number = entryPayments.A93Number;

		InsertOrUpdatePayInfo(a93Number, entryPayments.HasA93FirstPayment, entryPayments.FirstPaymentMethod, entryPayments.FirstPaymentDueDate, registerCode);
		InsertOrUpdatePayInfo(a93Number, entryPayments.HasA93SecondPayment, entryPayments.SecondPaymentMethod, entryPayments.SecondPaymentDueDate, registerCode);
		InsertOrUpdatePayInfo(a93Number, entryPayments.HasA93ThirdPayment, entryPayments.ThirdPaymentMethod, entryPayments.ThirdPaymentDueDate, registerCode);
	}

	void InsertOrUpdatePayInfo(ZString number, ZBool hasValidData, ZString methodOfPayment, ZDate paymentDueDate, ZString registry)
	{
		if (hasValidData)
		{
			var movementHeader = nctsHeader.MovementHeader;
			var totalAmount = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Sum(x => x.Fees.GetTotalAmount(methodOfPayment));
			movementHeader.PayInfoCollection.InsertOrUpdatePayInfo(x => x.BPI_MethodOfPayment == methodOfPayment, totalAmount, registry, paymentDueDate, number, methodOfPayment);
		}
	}

	ZBool ISadCustomsLinkedObjectAdapter.IsIncomingMessageAlreadyLinked(ZString incomingMessageType) => nctsHeader.Messages.GetLastMessageByType(incomingMessageType) != null;

	void ISadCustomsLinkedObjectAdapter.SetEntryCustomsStatus(ZString entryCustomsStatus) => movementHeader.BM_CustomsStatus = entryCustomsStatus;

	void ISadCustomsLinkedObjectAdapter.SetEntryReleaseDate(ZDateTime releaseDateTime) { }

	void ISadCustomsLinkedObjectAdapter.UpdatePendingGuaranteeTransactions(ZString transactionsNewStatus)
	{
		var sentMessage = nctsHeader.Messages.GetLastSuccessfullySentIdoc();
		if (sentMessage != null)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactions(nctsHeader.Factory, sentMessage, NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.Italy, status: transactionsNewStatus);
		}
	}

	void ISadCustomsLinkedObjectAdapter.WriteOffGuarantee(ZString applicationId, ZDate transactionDate)
	{
		var guaranteeTransactionProcessor = new NctsHeaderGuaranteeTransactionProcessor(nctsHeader);
		guaranteeTransactionProcessor.AddNewWriteOffTransaction(applicationId, transactionDate);
	}

	#endregion

	#region ICustomsLinkedObjectAdapter Members

	ZGuid ICustomsLinkedObjectAdapter.PK => nctsHeader.PK;

	void ICustomsLinkedObjectAdapter.AddMessage(EDIMessage message)
	{
		Argument.NotNull(message, nameof(message));
		nctsHeader.Messages.Add(message);
	}

	EDIMessage ICustomsLinkedObjectAdapter.GetLastSuccessfullySentMessage() => nctsHeader.Messages.GetLastSuccessfullySentIdoc();

	void ICustomsLinkedObjectAdapter.GenerateDocuments()
	{
		if (IsEntryCleared)
		{
			const string italianCultureCode = "IT-IT";
			new NctsTadEdocSaver(nctsHeader, new NctsTadEdocSaverOptions(language: italianCultureCode)).RenderTadAndStoreInEdocs(nctsHeader);
		}
	}

	ZString ICustomsLinkedObjectAdapter.EntryReferenceNumber => nctsHeader.BH_JobReference;

	ZString ICustomsLinkedObjectAdapter.JobReferenceNumber => nctsHeader.BH_JobReference;

	BusinessObjectFactory ICustomsLinkedObjectAdapter.Factory => nctsHeader.Factory;

	ZString ICustomsLinkedObjectAdapter.CustomsProfile => nctsHeader.BH_CustomsProfile;

	#endregion

	#region ISingleWindowCustomsLinkedObjectAdapter Members

	DocManagerInfo ISingleWindowCustomsLinkedObjectAdapter.DocManagerInfo => nctsHeader.DocManagerInfo;

	void ISingleWindowCustomsLinkedObjectAdapter.AddLog(Event eventType, ZDateTime eventDate, KeyValuePair<string, string>[] eventAttributes) => nctsHeader.Logs.AddNew(eventType, eventDate.ToOffset(), eventAttributes);

	void ISingleWindowCustomsLinkedObjectAdapter.SetEntryCustomsChannel(ZString entryCustomsChannel) => movementHeader.BM_ControlChannel = entryCustomsChannel;

	void ISingleWindowCustomsLinkedObjectAdapter.SetEntryAsCleared(ZDateTime releaseDateTime) => movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

	void ISingleWindowCustomsLinkedObjectAdapter.InsertOrUpdateReleaseCode(ZString releaseCode, ZDateTime releaseDate) => nctsHeader.EntryNumbersProvider.InsertOrUpdateReleaseCode(releaseCode, releaseDate);

	ZBool ISingleWindowCustomsLinkedObjectAdapter.IsEntryCleared => IsEntryCleared;

	#endregion

	#region ICustomsStatusLinkedObjectAdapter

	ZString ICustomsStatusLinkedObjectAdapter.MessageStatus => nctsHeader.EffectiveMessageStatus;

	ZString ICustomsStatusLinkedObjectAdapter.AwaitingMessageStatus => StatusProvider.AwaitingMessageStatus;

	void ICustomsStatusLinkedObjectAdapter.SetMessageStatus(ZString status)
	{
		nctsHeader.EffectiveMessageStatus = status;
		EmptyEntryDateIfIsInErrorMessageStatus(status);
	}

	#endregion

	#region Implementation

	ZBool IsEntryCleared => movementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

	void CheckNctsHeaderIsDepartureJob(NctsHeader nctsHeader)
	{
		if (!nctsHeader.IsDepartureMovement)
		{
			throw new NotSupportedException($"{nameof(nctsHeader)} is not a departure job");
		}
	}

	void EmptyEntryDateIfIsInErrorMessageStatus(ZString status)
	{
		if (status == (this as ISadCustomsLinkedObjectAdapter).StatusProvider.ErrorMessageStatus)
		{
			movementHeader.BM_EntryDate = ZDateTime.Empty;
		}
	}

	ISadCustomsStatusProvider StatusProvider => statusProvider ?? (statusProvider = new NctsHeaderCustomsStatusProvider());
	ISadCustomsStatusProvider statusProvider;

	#endregion
}
