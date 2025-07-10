using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class CHNCTSValidationHelperTest : TestCaseWithFactory
{
	public void TestCheckMRNorGDRN() => CombineAssertions(() =>
	{
		BusinessObject.IsGDRNAllowed = true;

		BusinessObject.MRN = ValidGDRN;
		AssertNoMessageErrors($"Valid GDRN: {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = InvalidGDRN;
		AssertHasMessageErrorContaining($"Invalid GDRN: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNStructure);

		BusinessObject.MRN = ValidMRN;
		AssertNoMessageErrors($"Valid MRN: {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = InvalidMRN;
		AssertHasMessageErrorContaining($"Invalid MRN: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongMRNStructure);

		BusinessObject.MRN = ZString.Empty;
		AssertNoMessageErrors("Empty", BusinessObject.MRNInfo);

		BusinessObject.MRN = "X";
		AssertHasMessageErrorContaining("Check for index-out-of-range errors", BusinessObject.MRNInfo, MessageWrongMRNStructure);

		BusinessObject.MRN = Replace(Replace(Replace(ValidMRN, 2, "DE"), 16, "N"), 17, "9");
		AssertHasMessageErrorContaining($"DE/N validated as MRN, not as GDRN: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongMRNCheckDigit);
		AssertNoMessageErrorContaining($"DE/N validated as MRN, not as GDRN: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNCheckDigit);
		BusinessObject.MRN = Replace(Replace(Replace(ValidMRN, 2, "CH"), 16, "N"), 17, "9");
		AssertHasMessageErrorContaining($"CH/N validated as GDRN, not as MRN: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNCheckDigit);
	});

	public void TestCheckMRN() => CombineAssertions(() =>
	{
		BusinessObject.IsGDRNAllowed = false;

		BusinessObject.MRN = ValidMRN;
		AssertNoMessageErrors($"Valid MRN: {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidMRN, 0, "21"));
		AssertHasMessageError($"Year below 22: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongMRNYear);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidMRN, 0, "22"));
		AssertNoMessageErrors($"Year at minmum (22): {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidMRN, 0, "99"));
		AssertNoMessageErrors($"Year at maximum (99): {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidMRN, 0, "2A"));
		AssertHasMessageErrorContaining($"Non-numeric year: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongMRNStructure);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidMRN, 2, "XX"));
		AssertHasMessageError($"Invalid country: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongMRNCountry);

		BusinessObject.MRN = Replace(ValidMRN, 17, "9");
		AssertHasMessageErrorContaining($"Invalid check digit: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongMRNCheckDigit);

		BusinessObject.MRN = ZString.Empty;
		AssertNoMessageErrors("Empty", BusinessObject.MRNInfo);

		assertValidSuffixes(false);
		assertValidSuffixes(true);

		void assertValidSuffixes(bool isGdrnAllowed)
		{
			BusinessObject.IsGDRNAllowed = isGdrnAllowed;
			foreach (var suffix in Enumerable.Range('0', 10).Concat(Enumerable.Range('A', 26)).Select(c => ((char)c).ToString()))
			{
				BusinessObject.MRN = SetCheckDigit(Replace(ValidMRN, 16, suffix));
				AssertNoMessageErrors($"GDRNAllowed={isGdrnAllowed} Invalid suffix ({suffix}): {BusinessObject.MRN}", BusinessObject.MRNInfo);
			}
		}
	});

	public void TestCheckGDRN() => CombineAssertions(() =>
	{
		BusinessObject.IsGDRNAllowed = true;

		BusinessObject.MRN = ValidGDRN;
		AssertNoMessageErrors($"Valid GDRN: {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 0, "21"));
		AssertHasMessageError($"Year below 22: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNYear);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 0, "22"));
		AssertNoMessageErrors($"Year at minmum: {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 0, "99"));
		AssertNoMessageErrors($"Year at minmum: {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 0, "2A"));
		AssertHasMessageErrorContaining($"Non-numeric year: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNStructure);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 2, "DE"));
		AssertNoMessageErrors($"Invalid country: {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 4, "00"));
		AssertHasMessageError($"Month below mimimum 01: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNMonth);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 4, "01"));
		AssertNoMessageErrors($"Month at miminum (01): {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 4, "12"));
		AssertNoMessageErrors($"Month at maximum (12): {BusinessObject.MRN}", BusinessObject.MRNInfo);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 4, "13"));
		AssertHasMessageError($"Month above maximum 12: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNMonth);

		BusinessObject.MRN = SetCheckDigit(Replace(ValidGDRN, 4, "1A"));
		AssertHasMessageError($"Month not numeric: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNMonth);

		BusinessObject.MRN = Replace(ValidGDRN, 17, "9");
		AssertHasMessageErrorContaining($"Invalid check digit: {BusinessObject.MRN}", BusinessObject.MRNInfo, MessageWrongGDRNCheckDigit);

		BusinessObject.MRN = ZString.Empty;
		AssertNoMessageErrors("Empty", BusinessObject.MRNInfo);
	});

	static string Replace(string input, int start, string replacement)
	{
		return input.Substring(0, start) + replacement + input.Substring(start + replacement.Length);
	}

	static string SetCheckDigit(string value)
	{
		value = value.Remove(value.Length - 1, 1);
		var factor = 1;
		var sum = 0;
		foreach (var ch in value)
		{
			int n;
			if (ch >= '0' && ch <= '9')
			{
				n = ch - '0';
			}
			else if (ch == 'A')
			{
				n = 10;
			}
			else if (ch >= 'B' && ch <= 'K')
			{
				n = ch - 'B' + 12;
			}
			else if (ch >= 'L' && ch <= 'U')
			{
				n = ch - 'L' + 23;
			}
			else if (ch >= 'V' && ch <= 'Z')
			{
				n = ch - 'V' + 34;
			}
			else
			{
				throw new Exception($"Unknown character: '{ch}'");
			}
			sum += n * factor;
			factor *= 2;
		}
		sum %= 11;
		return value + (char)((sum == 10 ? 0 : sum) + '0');
	}

	public void TestCheckMRN_NotificationType()
	{
		BusinessObject.IsGDRNAllowed = false;

		BusinessObject.NotificationType = null;
		BusinessObject.MRN = InvalidMRN;
		AssertHasMessageErrorContaining("NotificationType=default", BusinessObject.MRNInfo, MessageWrongMRNStructure);

		BusinessObject.NotificationType = CargoWise.EntityFramework.NotificationType.Error;
		BusinessObject.MRN = InvalidMRN;
		AssertHasErrorContaining("NotificationType=Error", BusinessObject.MRNInfo, MessageWrongMRNStructure);
	}

	public void TestCheckGDRNorMRN_NotificationType()
	{
		BusinessObject.IsGDRNAllowed = true;

		BusinessObject.NotificationType = null;
		BusinessObject.MRN = InvalidMRN;
		AssertHasMessageErrorContaining("NotificationType=default", BusinessObject.MRNInfo, MessageWrongMRNStructure);

		BusinessObject.NotificationType = CargoWise.EntityFramework.NotificationType.Error;
		BusinessObject.MRN = InvalidMRN;
		AssertHasErrorContaining("NotificationType=Error", BusinessObject.MRNInfo, MessageWrongMRNStructure);
	}

	const string ValidMRN = "23IT123456789012J1";
	const string InvalidMRN = "23IT12-456789012J4";
	const string ValidGDRN = "23CH063456789012N8";
	const string InvalidGDRN = "23CH06-456789012N0";

	const string MessageWrongMRNStructure = "[NS30006]:  a MRN structure is required";
	const string MessageWrongGDRNStructure = "[NS30118]:  a MRN structure is required";
	const string MessageWrongMRNCountry = "[NS30006]: MRN does not contain a valid country/region code";
	const string MessageWrongMRNCheckDigit = "[NS30006]: MRN does not have a valid last digit.";
	const string MessageWrongGDRNCheckDigit = "[NS30118]: MRN does not have a valid last digit.";
	const string MessageWrongMRNYear = "[NS30006]: The year (the first two digits) must be at least 22.";
	const string MessageWrongGDRNYear = "[NS30118]: The year (the first two digits) must be at least 22.";
	const string MessageWrongGDRNMonth = "[NS30118]: The month (the two digits after the country) must be between 01 and 12.";

	BusinessObjectForTesting BusinessObject => businessObject ?? (businessObject = new BusinessObjectForTesting(Factory));
	BusinessObjectForTesting businessObject;

	class BusinessObjectForTesting : NonPersistentBusinessObject
	{
		internal BusinessObjectForTesting(BusinessObjectFactory factory) : base(factory) { }

		public ZString MRN
		{
			get => mrn;
			set
			{
				mrn = value;
				Validation.ValidateMRN();
			}
		}
		ZString mrn;

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(nameof(MRN));

		public ZBool IsGDRNAllowed { get; set; }

		public INotificationType NotificationType { get; set; }

		BusinessObjectValidationForTesting Validation => new BusinessObjectValidationForTesting(this);
	}

	class BusinessObjectValidationForTesting : ZValidation
	{
		internal BusinessObjectValidationForTesting(BusinessObjectForTesting parent) : base(parent)
		{
			Parent = parent;
		}
		BusinessObjectForTesting Parent { get; }

		public override Type AutoValidationType => typeof(BusinessObjectForTesting);

		public override void ValidateAll()
		{
		}

		public void ValidateMRN()
		{
			((IValidationInternals)this).Validate(Parent.MRNInfo, new RunValidationInvoker(CheckMRN));
		}

		protected void CheckMRN()
		{
			if (Parent.IsGDRNAllowed)
			{
				if (Parent.NotificationType == null)
				{
					CHNCTSValidationHelper.CheckGDRNorMRN(Parent.Factory, Parent.MRNInfo);
				}
				else
				{
					CHNCTSValidationHelper.CheckGDRNorMRN(Parent.Factory, Parent.MRNInfo, Parent.NotificationType);
				}
			}
			else
			{
				if (Parent.NotificationType == null)
				{
					CHNCTSValidationHelper.CheckMRN(Parent.Factory, Parent.MRNInfo);
				}
				else
				{
					CHNCTSValidationHelper.CheckMRN(Parent.Factory, Parent.MRNInfo, Parent.NotificationType);
				}
			}
		}
	}
}
