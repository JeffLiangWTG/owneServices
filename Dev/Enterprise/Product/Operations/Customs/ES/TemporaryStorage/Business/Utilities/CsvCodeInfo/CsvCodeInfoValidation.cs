using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;
public class CsvCodeInfoValidation : ZValidation
{
	public CsvCodeInfoValidation(CsvCodeInfo parent)
		: base(parent)
	{
		this.parent = parent;
	}
	readonly CsvCodeInfo parent;

	public override Type AutoValidationType => typeof(CsvCodeInfoValidation);

	public override void ValidateAll()
	{
		ValidateCsvCodeFromUser();
		ValidateClearanceDateFromUser();
	}

	public void ValidateCsvCodeFromUser()
	{
		ValidateCalculatedProperty(parent.CsvCodeFromUserInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckCsvCodeFromUser()
	{
		var code = parent.CsvCodeFromUser;
		if (!CSVCodeIsValidOrEmpty(code))
		{
			parent.CsvCodeFromUserInfo.AddError(ResString.GetMultilingualString("454C4429-8163-4DFA-B26A-54CEA1B09FB9", "{0} format is incorrect. Please fill with a correct CSV Clearance Code", code));
		}

		bool CSVCodeIsValidOrEmpty(ZString csvCode) => Regex.IsMatch(csvCode, @"^[a-zA-Z0-9]+$") || csvCode.IsEmpty;
	}

	public void ValidateClearanceDateFromUser()
	{
		ValidateCalculatedProperty(parent.ClearanceDateFromUserInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckClearanceDateFromUser()
	{
		var date = parent.ClearanceDateFromUser;
		if (!date.IsEmpty && !date.IsValid)
		{
			parent.ClearanceDateFromUserInfo.AddError(ResString.GetMultilingualString("8212DA98-AC8B-4556-A72A-5AEB9F2F32B5", "Clearance Date is invalid. Please fill with a correct date"));
		}

		if (date.IsEmpty && !parent.CsvCodeFromUser.IsEmpty)
		{
			parent.ClearanceDateFromUserInfo.AddError(ResString.GetMultilingualString("BD84E236-30F9-47F8-A02C-B6D6307C5C46", "Clearance Date is mandatory when CSV Clearance is entered"));
		}
	}
}

