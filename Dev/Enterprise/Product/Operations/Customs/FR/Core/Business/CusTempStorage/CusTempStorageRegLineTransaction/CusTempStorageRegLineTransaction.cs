using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransaction : EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction, Integration.Customs.FR.ICusTempStorageRegLineTransaction
	{
		public CusTempStorageRegLineTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction|SRT_GrossWeight", Caption = "Gross Weight")]
		public override ZDecimal SRT_GrossWeight
		{
			get => base.SRT_GrossWeight;
			set => base.SRT_GrossWeight = value;
		}

		[ReadOnly(true)]
		public override ZString SRT_TransactionType
		{
			get => base.SRT_TransactionType;
			set => base.SRT_TransactionType = value;
		}

		[ReadOnly(true)]
		public override ZDateTime SRT_SystemCreateTimeUtc
		{
			get => base.SRT_SystemCreateTimeUtc;
			set => base.SRT_SystemCreateTimeUtc = value;
		}

		[ReadOnly(true)]
		public override ZString SRT_SystemCreateUser
		{
			get => base.SRT_SystemCreateUser;
			set => base.SRT_SystemCreateUser = value;
		}

		[ResourceStringData("Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction|SRT_Reference", Caption = "TSD Number")]
		public override ZString SRT_Reference
		{
			get => base.SRT_Reference;
			set => base.SRT_Reference = value;
		}

		[ResourceStringData("Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction|SRT_InternalReferenceNumber", Caption = "Job Reference")]
		public override ZString SRT_InternalReferenceNumber
		{
			get => base.SRT_InternalReferenceNumber;
			set => base.SRT_InternalReferenceNumber = value;
		}

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsInDatabase;
			set => base.ReadOnly = value;
		}

		public override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine RegLine => regLine ?? Factory.Load<CusTempStorageRegLine>(SRT_SRL);
		readonly CusTempStorageRegLine regLine;

		public new CusTempStorageRegLineTransactionLookups Lookups => (CusTempStorageRegLineTransactionLookups)base.Lookups;

		public new CusTempStorageRegLineTransactionValidation Validation => (CusTempStorageRegLineTransactionValidation)base.Validation;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionLookups GetNewLookups() => new CusTempStorageRegLineTransactionLookups(this);

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionValidation GetNewValidation() => new CusTempStorageRegLineTransactionValidation(this);
	}
}
