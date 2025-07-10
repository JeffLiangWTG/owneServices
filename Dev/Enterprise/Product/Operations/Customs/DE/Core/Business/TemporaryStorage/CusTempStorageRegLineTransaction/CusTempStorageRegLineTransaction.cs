using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransaction : EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction, Integration.Customs.DE.ICusTempStorageRegLineTransaction
	{
		public CusTempStorageRegLineTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine RegLine => regLine ?? Factory.Load<CusTempStorageRegLine>(SRT_SRL);
		readonly CusTempStorageRegLine regLine;

		#region Properties

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

		public override ZInt SRT_PackageQty
		{
			get => base.SRT_PackageQty;
			set
			{
				if (base.SRT_PackageQty != value)
				{
					base.SRT_PackageQty = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateSRT_GrossWeight();
					}
				}
			}
		}

		public override ZDecimal SRT_GrossWeight
		{
			get => base.SRT_GrossWeight;
			set
			{
				if (base.SRT_GrossWeight != value)
				{
					base.SRT_GrossWeight = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateSRT_PackageQty();
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString SRT_TransactionType
		{
			get => base.SRT_TransactionType;
			set => base.SRT_TransactionType = value;
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsInDatabase; }
			set { base.ReadOnly = value; }
		}

		public override bool CanDelete => base.CanDelete && SRT_TransactionType != TransactionTypes.Codes.OpeningBalance;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("113d6d13-0565-44ad-8ec5-5365d9117722", "Opening Balance transaction cannot be deleted"); }
		}

		#endregion

		#region Implement

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionLookups GetNewLookups()
		{
			return new CusTempStorageRegLineTransactionLookups(this);
		}

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionValidation GetNewValidation()
		{
			return new CusTempStorageRegLineTransactionValidation(this);
		}

		#endregion
	}
}
