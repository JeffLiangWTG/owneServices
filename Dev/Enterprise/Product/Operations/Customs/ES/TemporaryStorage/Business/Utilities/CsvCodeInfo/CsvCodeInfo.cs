using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.CusTempStorage;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;
public class CsvCodeInfo : NonPersistentBusinessObject
{
	CsvCodeInfo(TemporaryStorageHeader temporaryStorageHeader)
		: base(temporaryStorageHeader.Factory)
	{
	}

	public static class Schema
	{
		public const string CsvCodeFromUser = "CsvCodeFromUser";
		public const string ClearanceDateFromUser = "ClearanceDateFromUser";
	}

	public static CsvCodeInfo LoadNew(TemporaryStorageHeader temporaryStorageHeader)
	{
		Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
		var updateCSVClearance = new CsvCodeInfo(temporaryStorageHeader);
		updateCSVClearance.CsvCodeFromUser = temporaryStorageHeader.ClearanceNumber;
		updateCSVClearance.ClearanceDateFromUser = temporaryStorageHeader.ClearanceDate;
		return updateCSVClearance;
	}

	[ResourceStringData("fcdfab1e-9204-40cf-8e7d-8314c3d35ef2", Caption = "CSV Clearance")] 
	public ZString CsvCodeFromUser
	{
		get => csvCodeFromUser;
		set
		{
			SetNonPersistentPropertyValue(CsvCodeFromUserInfo, ref csvCodeFromUser, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateCsvCodeFromUser();
				Validation.ValidateClearanceDateFromUser();
			}
		}
	}
	ZString csvCodeFromUser;

	public ZPropertyInfo CsvCodeFromUserInfo => GetZPropertyInfo(Schema.CsvCodeFromUser);

	[ResourceStringData("cffd34d2-c3fc-4b24-a7fc-18537b52e677", Caption = "Clearance Date")]
	public ZDateTime ClearanceDateFromUser
	{
		get => clearanceDateFromUser;
		set
		{
			SetNonPersistentPropertyValue(ClearanceDateFromUserInfo, ref clearanceDateFromUser, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateClearanceDateFromUser();
			}
		}
	}
	ZDateTime clearanceDateFromUser;

	public ZPropertyInfo ClearanceDateFromUserInfo => GetZPropertyInfo(Schema.ClearanceDateFromUser);

	public CsvCodeInfoValidation Validation => new CsvCodeInfoValidation(this);

	protected override void RunPreSaveValidationCore()
	{
		Validation.ValidateAll();
		base.RunPreSaveValidationCore();
	}
}
