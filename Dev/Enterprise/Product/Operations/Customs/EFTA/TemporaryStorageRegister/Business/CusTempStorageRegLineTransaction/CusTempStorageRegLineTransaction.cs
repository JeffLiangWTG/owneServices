using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

[SingleObjectAroundARow]
[DependentBusinessObject(typeof(CusTempStorageRegLine), nameof(CusTempStorageRegLine.CusTempStorageRegLineTransactions))]
public class CusTempStorageRegLineTransaction : AutoCusTempStorageRegLineTransaction, ICusTempStorageRegLineTransaction
{
	public CusTempStorageRegLineTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusTempStorageRegLineTransaction.Schema
	{
		public const string TransactionTypeDescription = nameof(CusTempStorageRegLineTransaction.TransactionTypeDescription);
	}

	#region Type Decider

	[ThreadSafe]
	public static readonly CusTempStorageRegLineTransactionTypeDecider TypeDecider = new();

	#endregion

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
	}

	[RelatedBusinessObject(nameof(RegLine))]
	public override ZGuid SRT_SRL
	{
		get { return base.SRT_SRL; }
		set { base.SRT_SRL = value; }
	}

	ICusTempStorageRegLine ICusTempStorageRegLineTransaction.RegLine => RegLine;

	public virtual CusTempStorageRegLine RegLine => regLine ??= Factory.Load<CusTempStorageRegLine>(SRT_SRL);
	CusTempStorageRegLine regLine;

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_TransactionType", Caption = "Transaction Type")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineTransactionLookups.TransactionTypeList))]
	public override ZString SRT_TransactionType
	{
		get => base.SRT_TransactionType;
		set => base.SRT_TransactionType = value;
	}

	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineTransactionLookups.TransactionStatusList))]
	public override ZString SRT_TransactionStatus
	{
		get => base.SRT_TransactionStatus;
		set => base.SRT_TransactionStatus = value;
	}

	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineTransactionLookups.InternalReferenceTypeList))]
	public override ZString SRT_InternalReferenceType
	{
		get => base.SRT_InternalReferenceType;
		set => base.SRT_InternalReferenceType = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_ReferenceType", Caption = "Reference Type")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineTransactionLookups.ReferenceTypeList))]
	public override ZString SRT_ReferenceType
	{
		get => base.SRT_ReferenceType;
		set => base.SRT_ReferenceType = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_PackageQty", Caption = "Package Quantity", ShortCaption = "Package Qty.")]
	public override ZInt SRT_PackageQty
	{
		get => base.SRT_PackageQty;
		set
		{
			var oldValue = SRT_PackageQty;
			base.SRT_PackageQty = value;
			if (!IsCopying && oldValue != SRT_PackageQty)
			{
				RegLine?.UpdatePackagesRemaining();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_Reference", Caption = "Reference Number")]
	public override ZString SRT_Reference
	{
		get => base.SRT_Reference;
		set => base.SRT_Reference = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_Comments", Caption = "Comments")]
	public override ZString SRT_Comments
	{
		get => base.SRT_Comments;
		set => base.SRT_Comments = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_SystemCreateTimeUtc", Caption = "Create Time")]
	public override ZDateTime SRT_SystemCreateTimeUtc
	{
		get => base.SRT_SystemCreateTimeUtc;
		set => base.SRT_SystemCreateTimeUtc = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_SystemCreateUser", Caption = "Create User")]
	public override ZString SRT_SystemCreateUser
	{
		get => base.SRT_SystemCreateUser;
		set => base.SRT_SystemCreateUser = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|TransactionTypeDescription", Caption = "Transaction Type Description")]
	public ZString TransactionTypeDescription => Lookups.TransactionTypeList.GetDescriptionFromCode(SRT_TransactionType);

	public ZPropertyInfo TransactionTypeDescriptionInfo => GetZPropertyInfo(Schema.TransactionTypeDescription);

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|PhysicalInOutDate", Caption = "Physical In/Out Date")]
	public ZDateTime PhysicalInOutDate
	{
		get => SRT_PhysicalInOutDate.ToZDateTime();
		set => SRT_PhysicalInOutDate = value.ToOffset();
	}

	public ZWrappedPropertyInfo PhysicalInOutDateInfo => GetWrappedZPropertyInfo(nameof(PhysicalInOutDate), x => SRT_PhysicalInOutDateInfo);

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|TransactionDate", Caption = "Transaction Date")]
	public ZDateTime TransactionDate
	{
		get => SRT_TransactionDate.ToZDateTime();
		set => SRT_TransactionDate = value.ToOffset();
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction|SRT_BondAmount", Caption = "Goods Value")]
	public override ZDecimal SRT_BondAmount
	{
		get => base.SRT_BondAmount;
		set => base.SRT_BondAmount = value;
	}

	public ZWrappedPropertyInfo TransactionDateInfo => GetWrappedZPropertyInfo(nameof(TransactionDate), x => SRT_TransactionDateInfo);
}
