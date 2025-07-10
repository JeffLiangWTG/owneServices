using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ExportPrevDocN337RefNumberValidationStrategy : IPreviousDocumentRefNumberValidationStrategy
{
	public Ucc6ExportPrevDocN337RefNumberValidationStrategy(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	readonly BusinessObjectFactory factory;

	void IPreviousDocumentRefNumberValidationStrategy.CheckReferenceNumber(ZPropertyInfoString numberPropertyInfo)
	{
		Argument.NotNull(numberPropertyInfo, nameof(numberPropertyInfo));

		var referenceNumber = numberPropertyInfo.Value;
		var referenceNumberParts = referenceNumber.Split('-');
		if (!HasRequiredNumberOfParts(referenceNumberParts, out var errorMessage))
		{
			numberPropertyInfo.AddMessageError(errorMessage);
			return;
		}

		new ProcedureCodeValidator(numberPropertyInfo).Validate(referenceNumberParts[0]);
		new RegistrationNumberValidator(numberPropertyInfo).Validate(referenceNumberParts[1]);
		new YearOfIssueValidator(numberPropertyInfo).Validate(referenceNumberParts[2]);
		new CustomsOfficeCodeValidator(factory, numberPropertyInfo).Validate(referenceNumberParts[3]);
	}

	#region Implementation

	bool HasRequiredNumberOfParts(ZString[] referenceNumberParts, out string errorMessage)
	{
		errorMessage = string.Empty;
		var numberOfParts = referenceNumberParts.Length;
		if (numberOfParts == 4)
		{
			return true;
		}

		errorMessage = numberOfParts < 4
			? ValidationCaptions.InvoiceLinePreviousDocuments.N337ReferenceNumberHasLessParts
			: ValidationCaptions.InvoiceLinePreviousDocuments.N337ReferenceNumberHasMoreParts;

		return false;
	}

	#endregion

	#region ProcedureCodeValidator

	sealed class ProcedureCodeValidator
	{
		public ProcedureCodeValidator(ZPropertyInfo targetPropertyInfo)
		{
			this.targetPropertyInfo = Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));
		}

		readonly ZPropertyInfo targetPropertyInfo;

		public void Validate(ZString procedureCode)
		{
			if (!procedureCode.IsEmpty && procedureCode.In(allowedProcedureCodes))
			{
				return;
			}

			var allowedProcedureCodesString = string.Join(", ", allowedProcedureCodes);
			targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.N337InvalidProcedureCode(allowedProcedureCodesString));
		}

		readonly ImmutableArray<ZString> allowedProcedureCodes = new ZString[]
		{
			PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione,
			PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazioneInProceduraDiFallback,
			PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazioneConSdoganamentoTelematico,
			PreviousDocumentProcedureList.Codes.Registro5DiTempImportazione,
			PreviousDocumentProcedureList.Codes.Registro5DiTempImportazioneInProceduraDiFallback,
			PreviousDocumentProcedureList.Codes.Registro5DiTempImportazioneConSdoganamentoTelematico,
			PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDeposito,
			PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDepositoInProceduraDiFallback,
			PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDepositoConSdoganamentoTelematico,
			PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaPf,
			PreviousDocumentProcedureList.Codes.AltriDocumenti,
		}.ToImmutableArray();
	}

	#endregion

	#region RegistrationNumberValidator

	sealed class RegistrationNumberValidator
	{
		public RegistrationNumberValidator(ZPropertyInfo targetPropertyInfo)
		{
			this.targetPropertyInfo = Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));
		}

		readonly ZPropertyInfo targetPropertyInfo;

		public void Validate(ZString registrationNumber)
		{
			var referenceNumberLength = registrationNumber.Length;
			if (referenceNumberLength == 0 || referenceNumberLength > 8)
			{
				targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.N337ReferenceNumberSecondPartMustBeOf8Digits);
			}

			if (referenceNumberLength != 0)
			{
				if (registrationNumber.ToString().Any(char.IsWhiteSpace))
				{
					targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.N337ReferenceNumberSecondPartMustNotContainWhiteSpaces);
				}

				var regex = new Regex(@"^([0-9])*$");
				if (!regex.IsMatch(registrationNumber))
				{
					targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.N337ReferenceNumberSecondPartMustContainAllNumbers);
				}
			}
		}
	}

	#endregion

	#region YearOfIssueValidator

	sealed class YearOfIssueValidator
	{
		public YearOfIssueValidator(ZPropertyInfo targetPropertyInfo)
		{
			this.targetPropertyInfo = Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));
		}

		readonly ZPropertyInfo targetPropertyInfo;

		public void Validate(ZString yearOfIssue)
		{
			var yearRegex = new Regex(@"^(19|[2-9][0-9])\d{2}$");
			if (!yearRegex.IsMatch(yearOfIssue) || IsYearInTheFuture(yearOfIssue))
			{
				targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.N337ReferenceNumberYearOfIssuingMustBeSpecified);
			}
		}

		bool IsYearInTheFuture(ZString referenceNumberPart)
		{
			var year = int.Parse(referenceNumberPart);
			return year > ZDateTime.Now.Year;
		}
	}

	#endregion

	#region CustomsOfficeCodeValidator

	sealed class CustomsOfficeCodeValidator
	{
		public CustomsOfficeCodeValidator(BusinessObjectFactory factory, ZPropertyInfo targetPropertyInfo)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.targetPropertyInfo = Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));
		}

		readonly BusinessObjectFactory factory;
		readonly ZPropertyInfo targetPropertyInfo;

		public void Validate(ZString customsOfficeCode)
		{
			if (customsOfficeCode.IsEmpty || !IsBelongingToItalianCustomsOffices(customsOfficeCode))
			{
				targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.N337ReferenceNumberItalianCustomsOfficeRequired);
			}
		}

		bool IsBelongingToItalianCustomsOffices(ZString customsOfficeCode)
		{
			var cusOffices = GetAllItalianCustomsOffices();
			return cusOffices.Cast<ZZRefCusCodeListCombined>().Any(c => GetCodeWithoutCountryIfPresent(c.ZZD_Code, c.ZZD_CountryOrGrouping) == customsOfficeCode);
		}

		ZString GetCodeWithoutCountryIfPresent(ZString customsOfficeCode, ZString countryOrGrouping)
		{
			return customsOfficeCode.StartsWith(countryOrGrouping)
				? customsOfficeCode.Remove(0, countryOrGrouping.Length)
				: customsOfficeCode;
		}

		CustomsOfficeCodeCollection GetAllItalianCustomsOffices()
		{
			var customOfficeCodesCollection = CustomsOfficeCodeCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Italy, ZDateTime.Today);
			if (!customOfficeCodesCollection.IsLoaded && !customOfficeCodesCollection.IsLoading)
			{
				customOfficeCodesCollection.Load();
			}
			return customOfficeCodesCollection;
		}
	}

	#endregion
}
