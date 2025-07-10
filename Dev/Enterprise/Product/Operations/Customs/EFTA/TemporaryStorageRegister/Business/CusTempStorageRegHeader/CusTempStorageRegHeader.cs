using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BatchProcessor;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

[SystemDefinedValues]
[SingleObjectAroundARow]
[CodeProperty(CusTempStorageRegHeader.Schema.SRH_Reference), DescriptionProperty(CusTempStorageRegHeader.Schema.SRH_Reference)]
public class CusTempStorageRegHeader : AutoCusTempStorageRegHeader, ICusTempStorageRegHeader, IEDocsProvider
{
	public CusTempStorageRegHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Type Decider

	[ThreadSafe]
	public static readonly CusTempStorageRegHeaderTypeDecider TypeDecider = new();

	#endregion

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader|SRH_Reference", Caption = "TSD Number")]
	public override ZString SRH_Reference { get => base.SRH_Reference; set => base.SRH_Reference = value; }

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader|SRH_PreviousReferenceType", Caption = "Previous Reference Type")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegHeaderLookups.PreviousReferenceTypeList))]
	public override ZString SRH_PreviousReferenceType { get => base.SRH_PreviousReferenceType; set => base.SRH_PreviousReferenceType = value; }

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader|SRH_InternalReference", Caption = "Job Reference")]
	public override ZString SRH_InternalReference { get => base.SRH_InternalReference; set => base.SRH_InternalReference = value; }

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader|SRH_Status", Caption = "Status")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegHeaderLookups.StatusList))]
	public override ZString SRH_Status { get => base.SRH_Status; set => base.SRH_Status = value; }

	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegHeaderLookups.CustomsOfficeList))]
	[ResourceStringData("66672940-e8cb-4638-b97a-3036524c3c88", Caption = "Customs Office")]
	public override ZString SRH_CustomsOffice { get => base.SRH_CustomsOffice; set => base.SRH_CustomsOffice = value; }

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader|SRH_ArrivalDate", Caption = "Arrival Date")]
	public override ZDate SRH_ArrivalDate { get => base.SRH_ArrivalDate; set => base.SRH_ArrivalDate = value; }

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader|SRH_PresentationDate", Caption = "Presentation Date")]
	public override ZDateTime SRH_PresentationDate { get => base.SRH_PresentationDate; set => base.SRH_PresentationDate = value; }

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader|SRH_PreviousReference", Caption = "Previous Reference Number")]
	public override ZString SRH_PreviousReference { get => base.SRH_PreviousReference; set => base.SRH_PreviousReference = value; }

	public Type GetStorageRegLineType() => GetStorageRegLineTypeCore();

	protected virtual Type GetStorageRegLineTypeCore() => typeof(CusTempStorageRegLine);

	protected override ZString HumanReadableNameCore => Res.GetString("713B0B1D-AC14-46F4-B297-70A319E7DDB9", "SumA Register {0}", SRH_Reference);

	public virtual ICusTempStorageRegPremises Premises
	{
		get
		{
			if (premises is null)
			{
				premises = GetPremisesByLinkedPremisesId(SRH_SRP_Premises);
				premises?.SetReadOnlyIncludingChildren(true);
			}
			return premises;
		}
	}
	CusTempStorageRegPremises premises;

	protected virtual CusTempStorageRegPremises GetPremisesByLinkedPremisesId(ZGuid premisesId) => Factory.Load<CusTempStorageRegPremises>(premisesId);

	[ChildEditable(true)]
	public CusTempStorageRegLineCollection CusTempStorageRegLines
	{
		get
		{
			if (cusTempStorageRegLines is null)
			{
				cusTempStorageRegLines = CreateNewCusTempStorageRegLines();
				RegisterEditableChildObject(cusTempStorageRegLines);
			}
			return cusTempStorageRegLines;
		}
	}
	CusTempStorageRegLineCollection cusTempStorageRegLines;

	ICusTempStorageRegLineCollection<ICusTempStorageRegLine> ICusTempStorageRegHeader.CusTempStorageRegLines => CusTempStorageRegLines;

	protected virtual CusTempStorageRegLineCollection CreateNewCusTempStorageRegLines() => new CusTempStorageRegLineCollection<CusTempStorageRegLine>(this);

	#region Mutex

	public ZGlobalMutex Mutex => mutex ??= new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "SRH" + base.PK.ToString());
	ZGlobalMutex mutex;

	public bool LockMutex() => Mutex.Lock();

	#endregion

	#region IEDocsProvider Members

	public EDocsProviderSupporter GetEDocsProviderSupporter() => new(this);

	public DocumentSupporter DocumentSupporter => documentSupporter ??= GetNewDocumentSupporter();
	CusTempStorageRegHeaderDocumentSupporter documentSupporter;

	protected virtual CusTempStorageRegHeaderDocumentSupporter GetNewDocumentSupporter() => new(this);

	public DocManagerInfo DocManagerInfo => docManagerInfo ??= GetNewDocManagerInfo();
	DocManagerInfo docManagerInfo;

	protected virtual DocManagerInfo GetNewDocManagerInfo() => new(this, Core.Constants.DocManagerCodes.TempStorageRegHeader);

	#endregion

	[ResourceStringData("DFD313D0-CA23-4037-BA60-9323687F936C", Caption = "Remaining Packages Qty")]
	public virtual ZInt RemainingPackagesQty => CusTempStorageRegLines.Sum(x => x.SRL_PackagesRemaining);
	public virtual ZPropertyInfo RemainingPackagesQtyInfo => GetZPropertyInfo(nameof(RemainingPackagesQty));

	#region Transaction

	public void AddNewRegisterTransaction(LoggingInformation logger,
		ZInt lineNumber,
		ZString reference,
		ZString referenceType,
		ZString internalReference,
		ZString internalReferenceType,
		ZDecimal grossWeight,
		ZInt packageQty,
		ZString comments,
		bool checkExistingManualAdjustmentTransactions = false,
		Action<ZString> notifierWhenInsufficient = null,
		Action<ZString> notifierWhenManualAdjusted = null)
	{
		var registerLine = CusTempStorageRegLines.FirstOrDefault(x => x.SRL_LineNumber == lineNumber);

		if (checkExistingManualAdjustmentTransactions || registerLine is null)
		{
			return;
		}

		if (IsNotBusting(registerLine, packageQty, notifierWhenInsufficient))
		{
			var transactionInLine = registerLine.CusTempStorageRegLineTransactions.AddNew();
			transactionInLine.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transactionInLine.SRT_ReferenceType = referenceType;
			transactionInLine.SRT_Reference = reference;
			transactionInLine.SRT_InternalReferenceType = internalReferenceType;
			transactionInLine.SRT_InternalReferenceNumber = internalReference;
			transactionInLine.SRT_GrossWeight = grossWeight;
			transactionInLine.SRT_PackageQty = packageQty;
			transactionInLine.SRT_Comments = comments;

			logger.Log(Res.GetString("CA3E65C8-E0C3-462E-AC78-267B2FC60F85", "Creation of a transaction for register {0}, Line Number {1}: Package Quantity {2}, Gross Weight {3}.", SRH_Reference, lineNumber, packageQty, grossWeight));
		}
	}

	bool IsNotBusting(ICusTempStorageRegLine line, ZInt packagesToAdd, Action<ZString> notifierWhenInsufficient = null)
	{
		var expectedPackagesRemaining = line.SRL_PackagesRemaining + packagesToAdd;
		if (expectedPackagesRemaining < 0)
		{
			notifierWhenInsufficient?.Invoke(SRH_Reference);
		}
		return expectedPackagesRemaining >= 0;
	}

	#endregion

	public override void OnSaving()
	{
		base.OnSaving();
		if (!IsInDatabase && SRH_InternalReference.IsEmpty && Premises is CusTempStorageRegPremises cusTempStorageRegPremises)
		{
			SRH_InternalReference = cusTempStorageRegPremises.NumberProvider.ActiveWrapper?.StmNums.GenerateNextCustomsNumber(Factory) ?? ZString.Empty;
		}
	}
}
