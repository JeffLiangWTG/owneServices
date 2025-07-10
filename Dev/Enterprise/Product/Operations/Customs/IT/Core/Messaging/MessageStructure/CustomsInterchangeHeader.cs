using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure;

public class CustomsInterchangeHeader
{
	public ZString InnerText { get; protected set; }
	public ZString AuthorizedUserCode { get; protected set; }
	public ZString FileName { get; protected set; }
	public ZString CustomsOfficeSectionCode { get; protected set; }
	public ZString CountryCode { get; protected set; }
	public ZString TaxCodeOrVATRegistrationNumber { get; protected set; }
	public ZInt ProgressiveSeatAuthorizedAccount { get; protected set; }
	public ZInt NumberOfRecordsInTheFile { get; protected set; }
	public ZString TransmissionEnvironment { get; protected set; }
	public ZBool IsProductionTransmissionEnvironment => TransmissionEnvironment == Constants.ProductionTransmissionEnvironment;

	static class Constants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Italian text, not a code smell")]
		public const string ProductionTransmissionEnvironment = "INVIO IN AMBIENTE REALE";
	}

	protected internal void Load(ZString line, bool ignoreTrasmissionEnviroment = false)
	{
		InnerText = line;
		AuthorizedUserCode = line.SubStringAndTrim(0, 4);
		FileName = line.SubStringAndTrim(16, 12);
		CustomsOfficeSectionCode = line.SubStringAndTrim(40, 6);
		CountryCode = line.SubStringAndTrim(48, 2);
		TaxCodeOrVATRegistrationNumber = line.SubStringAndTrim(50, 16);
		ProgressiveSeatAuthorizedAccount = ZInt.Parse(line.SubStringAndTrim(66, 3));
		NumberOfRecordsInTheFile = ZInt.Parse(line.SubStringAndTrim(70, 5));
		if (!ignoreTrasmissionEnviroment)
		{
			TransmissionEnvironment = line.SubStringAndTrim(79, 30);
		}
	}

	public static CustomsInterchangeHeader NewForEhubSending(ZString authorizedUserCode, ZString filename, ZString customsOfficeSectionCode, ZString taxCodeOrVATRegistrationNumber, ZInt progressiveSeatAuthorizedAccount, ZInt numberOfRecordsInTheFile)
	{
		var newCustomsInterchangeHeader = new CustomsInterchangeHeader()
		{
			AuthorizedUserCode = authorizedUserCode,
			FileName = filename,
			CustomsOfficeSectionCode = customsOfficeSectionCode,
			TaxCodeOrVATRegistrationNumber = taxCodeOrVATRegistrationNumber,
			ProgressiveSeatAuthorizedAccount = progressiveSeatAuthorizedAccount,
			NumberOfRecordsInTheFile = numberOfRecordsInTheFile,
			InnerText = new ZStringBuilder()
				.Append(authorizedUserCode.PadRight(4))
				.Append(GetSpaces(12))
				.Append(filename)
				.Append(GetSpaces(12))
				.Append(customsOfficeSectionCode.PadRight(6))
				.Append(GetSpaces(4))
				.Append(taxCodeOrVATRegistrationNumber.PadRight(16))
				.Append(progressiveSeatAuthorizedAccount.ToString("D3", CultureInfo.InvariantCulture))
				.Append(GetSpaces(1))
				.Append(numberOfRecordsInTheFile.ToString("D5", CultureInfo.InvariantCulture))
				.ToString(),
		};

		return newCustomsInterchangeHeader;
	}

	public static CustomsInterchangeHeader NewFromText(ZString textContent, bool ignoreTrasmissionEnviroment = false)
	{
		Argument.NotNullOrEmpty(textContent, nameof(textContent));

		var interchangeHeader = new CustomsInterchangeHeader();
		interchangeHeader.Load(textContent, ignoreTrasmissionEnviroment);
		return interchangeHeader;
	}

	public static bool MatchCustomsMessageFileNames(string sentFileName, string receivedFileName)
	{
		var fileNameMatch = false;
		if (sentFileName.Length == FileNameLength && receivedFileName.Length == FileNameLength)
		{
			fileNameMatch = RemoveMessageTypeChar(sentFileName) == RemoveMessageTypeChar(receivedFileName);
		}
		return fileNameMatch;
	}

	static string RemoveMessageTypeChar(string fileName) => fileName.Remove(FileNameMessageTypeCharIndex, 1);
	static string GetSpaces(int count) => new string(' ', count);

	const int FileNameLength = 12;
	const int FileNameMessageTypeCharIndex = 9;
}
