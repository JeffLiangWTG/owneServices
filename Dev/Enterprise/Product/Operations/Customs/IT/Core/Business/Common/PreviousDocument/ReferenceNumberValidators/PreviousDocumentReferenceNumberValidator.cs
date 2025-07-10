using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class PreviousDocumentReferenceNumberValidator : IPreviousDocumentReferenceNumberValidator
{
	public PreviousDocumentReferenceNumberValidator(IPreviousDocumentReferenceNumberProvider previousDocument, PreviousDocumentFieldsInfo previousDocumentSettings)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		this.previousDocumentSettings = Argument.NotNull(previousDocumentSettings, nameof(previousDocumentSettings));
	}

	readonly IPreviousDocumentReferenceNumberProvider previousDocument;
	readonly PreviousDocumentFieldsInfo previousDocumentSettings;

	void IPreviousDocumentReferenceNumberValidator.CheckReferenceNumber()
	{
		if (previousDocumentSettings.IsReferenceNumberEditable)
		{
			CheckReferenceNumberWhenIsEditable();
		}
		else
		{
			MandatoryValidation.MessageErrorIfIsEntered(ReferenceNumberInfo);
		}
	}

	void CheckReferenceNumberWhenIsEditable()
	{
		var referenceNumber = ReferenceNumberProvider.ReferenceNumber;
		if (!referenceNumber.IsEmpty)
		{
			CheckReferenceNumberFormat();
		}
		MandatoryValidation.MessageErrorIfNotEntered(ReferenceNumberInfo);
	}

	protected virtual void CheckReferenceNumberFormat()
	{
		if (ReferenceNumberProvider.IsManualA3)
		{
			CheckReferenceNumberAgainstRegex(ReferenceNumberInfo, RegexToCheckReferenceNumberWithNoCin);
		}
		else if (!ReferenceNumberProvider.IsCIM && CheckReferenceNumberAgainstRegex(ReferenceNumberInfo, RegexToCheckReferenceNumberWithCin))
		{
			CheckCinMatchesWithCalculated();
		}
	}

	ZBool CheckReferenceNumberAgainstRegex(ZPropertyInfo referenceNumberInfo, ZString regex)
	{
		if (!Regex.IsMatch((ZString)referenceNumberInfo.Value, regex))
		{
			referenceNumberInfo.AddMessageError(ValidationCaptions.PreviousDocument.ReferenceNumberEnteredIsInvalidFormat);
			return ZBool.False;
		}
		return ZBool.True;
	}

	void CheckCinMatchesWithCalculated()
	{
		var actualCin = ReferenceNumberProvider.ReferenceNumberCin;
		var calculatedCin = ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString(ReferenceNumberProvider.ReferenceNumberWithoutCin);

		if (actualCin != calculatedCin)
		{
			ReferenceNumberInfo.AddMessageError(ValidationCaptions.PreviousDocument.CheckDigitIsIncorrect(actualCin, calculatedCin));
		}
	}

	protected ZPropertyInfo ReferenceNumberInfo => ReferenceNumberProvider.ReferenceNumberInfo;

	PreviousDocumentReferenceNumberProvider ReferenceNumberProvider => previousDocument.ReferenceNumberProvider;

	const string RegexToCheckReferenceNumberWithNoCin = "^[0-9]+$";
	const string RegexToCheckReferenceNumberWithCin = "^[0-9]+[A-Z]{1}$";
}
