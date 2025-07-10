using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
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
			ValidateSecondaryCsvCodeFromUser();
			ValidateThirdCsvCodeFromUser();
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
				parent.CsvCodeFromUserInfo.AddError(ResString.GetMultilingualString("7F6C32ED-5CF8-4488-9403-00C532E3D63C", "{0} format is incorrect. Please fill with a correct CSV Clearance Code", code));
			}
		}

		public void ValidateSecondaryCsvCodeFromUser()
		{
			ValidateCalculatedProperty(parent.SecondaryCsvCodeFromUserInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void CheckSecondaryCsvCodeFromUser()
		{
			var code = parent.SecondaryCsvCodeFromUser;
			if (!CSVCodeIsValidOrEmpty(code))
			{
				parent.SecondaryCsvCodeFromUserInfo.AddError(ResString.GetMultilingualString("DF4BB310-E52C-4FCD-9E9C-7DBCC64C0747", "{0} format is incorrect. Please fill with a correct {1} Code", code, CalculateCsvTypeName(parent.entryHeader)));
			}
		}

		public void ValidateThirdCsvCodeFromUser()
		{
			ValidateCalculatedProperty(parent.ThirdCsvCodeFromUserInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void CheckThirdCsvCodeFromUser()
		{
			var code = parent.ThirdCsvCodeFromUser;
			if (!CSVCodeIsValidOrEmpty(code))
			{
				parent.ThirdCsvCodeFromUserInfo.AddError(ResString.GetMultilingualString("F94806D2-F4C3-4610-B20D-FB202C878BED", "{0} format is incorrect. Please fill with a correct CSV Exit Certificate Code", code));
			}
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
				parent.ClearanceDateFromUserInfo.AddError(ResString.GetMultilingualString("79AC7394-6188-4A6A-B8BD-D1092CF9A385", "Clearance Date is invalid. Please fill with a correct date"));
			}

			if (date.IsEmpty && !parent.CsvCodeFromUser.IsEmpty)
			{
				parent.ClearanceDateFromUserInfo.AddError(ResString.GetMultilingualString("E6895B32-04FE-4A20-B030-92900090EA46", "Clearance Date is mandatory when CSV Clearance is entered"));
			}
		}

		public static bool CSVCodeIsValidOrEmpty(ZString csvCode) => Regex.IsMatch(csvCode, @"^[a-zA-Z0-9]+$") || csvCode.IsEmpty;

		ZString CalculateCsvTypeName(CusEntryHeader entryHeader) => entryHeader.IsImport ? ResString.GetMultilingualString("B21551A5-6C80-4D43-A43F-294D67441F32", "CSV Import Certificate") : ResString.GetMultilingualString("16E385B0-3817-4180-B52B-C4062330A879", "CSV T2L");
	}
}
