using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageRegLineTransaction : EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction, Integration.Customs.ES.ICusTempStorageRegLineTransaction
{
	public CusTempStorageRegLineTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Override Properties

	[ReadOnly(true)]
	public override ZString SRT_TransactionType { get => base.SRT_TransactionType; set => base.SRT_TransactionType = value; }

	[ReadOnly(true)]
	public override ZDateTime SRT_SystemCreateTimeUtc { get => base.SRT_SystemCreateTimeUtc; set => base.SRT_SystemCreateTimeUtc = value; }

	[ReadOnly(true)]
	public override ZString SRT_SystemCreateUser { get => base.SRT_SystemCreateUser; set => base.SRT_SystemCreateUser = value; }

	[ReadOnly(true)]
	public override ZDecimal SRT_BondAmount { get => base.SRT_BondAmount; set => base.SRT_BondAmount = value; }

	[ReadOnly(true)]
	public override ZString SRT_TransactionStatus { get => base.SRT_TransactionStatus; set => base.SRT_TransactionStatus = value; }

	[ResourceStringData("B9737D87-8EF6-4A33-A10D-A40B48E746ED", Caption = "Gross Weight in KGs", MediumCaption = "Gross Weight in KGs", ShortCaption = "Gross Weight in KGs", FullDescription = "Gross Weight in Kilograms")]
	public override ZDecimal SRT_GrossWeight
	{
		get => base.SRT_GrossWeight;
		set
		{
			base.SRT_GrossWeight = value;
			if (!base.IsValidationSuspended)
			{
				Validation.ValidateSRT_BondAmount();
			}
		}
	}

	[ResourceStringData("D0C24B33-D646-4EE8-8E1B-F68EDCBAD8FB", Caption = "Internal Reference Type", MediumCaption = "Int. Ref. Type", ShortCaption = "Int. Type", FullDescription = "The Internal Reference Type")]
	public override ZString SRT_InternalReferenceType { get => base.SRT_InternalReferenceType; set => base.SRT_InternalReferenceType = value; }

	[ResourceStringData("5D1D75AE-D2B5-42C3-B407-F263EAF18448", Caption = "Internal Reference No.", MediumCaption = "Internal Ref. No.", ShortCaption = "Int. Ref. No.", FullDescription = "Internal Reference Number")]
	public override ZString SRT_InternalReferenceNumber { get => base.SRT_InternalReferenceNumber; set => base.SRT_InternalReferenceNumber = value; }

	#endregion

	public override bool ReadOnly
	{
		get => base.ReadOnly || IsInDatabase;
		set => base.ReadOnly = value;
	}

	public new CusTempStorageRegLineTransactionLookups Lookups => (CusTempStorageRegLineTransactionLookups)base.Lookups;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionLookups GetNewLookups() => new CusTempStorageRegLineTransactionLookups(this);

	public new CusTempStorageRegLineTransactionValidation Validation => (CusTempStorageRegLineTransactionValidation)base.Validation;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionValidation GetNewValidation() => new CusTempStorageRegLineTransactionValidation(this);

	public override void OnSaved(bool saveSucceeded)
	{
		base.OnSaved(saveSucceeded);

		if (saveSucceeded && !base.IsValidationSuspended)
		{
			Validation.ValidateSRT_GrossWeight();
			Validation.ValidateSRT_PackageQty();
			Validation.ValidateSRT_BondAmount();
		}
	}
}
