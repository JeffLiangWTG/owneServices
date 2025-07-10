using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class DeclarationOfIntentValueObject
{
	public DeclarationOfIntentValueObject(ZString doiNumber)
	{
		DOINumber = doiNumber;
	}

	public ZString DOINumber { get; }

	public ZBool IsPlaceholder => DOINumber == ValidPlaceholder;

	public ZDateTime IssueDate
	{
		get
		{
			if (!issueDate.HasValue)
			{
				issueDate = LoadIssueDate();
			}
			return issueDate.Value;
		}
	}
	ZDateTime? issueDate;

	ZDateTime LoadIssueDate()
	{
		var effectiveDateTimePart = DOINumber.Left(DateTimePartFormat.Length);
		var parseOk = ZDateTime.TryParseExact(effectiveDateTimePart, out var dateOfIssue, DateTimePartFormat);
		return parseOk ? dateOfIssue : ZDateTime.Empty;
	}

	public ZBool IsValid(ZBool considerPlaceholderValidValue)
	{
		ZBool isValid;
		if (considerPlaceholderValidValue && IsPlaceholder)
		{
			isValid = ZBool.True;
		}
		else
		{
			var dateTimePart = DOINumber.Left(DateTimePartFormat.Length);
			var decimalPart = DOINumber.SubstringSafe(DateTimePartFormat.Length);

			isValid = dateTimePart.Length == DateTimePartFormat.Length
				&& decimalPart.Length == DecimalPartLength
				&& !IssueDate.IsEmpty
				&& decimalPart.IsNumbersOnlyOrEmpty;
		}
		return isValid;
	}

	const string ValidPlaceholder = "X";
	const string DateTimePartFormat = "yyMMddHHmmss";
	const int DecimalPartLength = 11;
}
