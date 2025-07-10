using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.ZArchitecture.Core;
using SupportingDocument = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument;

namespace Enterprise.Customs.FR.DocumentWrappers;

public class Box44AddInfoAndDocumentsHelper
{
	static ZString TitleRegimeEconomiqueInBox44 => (NoResString)"Régime Economique :" + DocumentWrapperConstants.Delimiters.CarriageReturn;

	static ZString TitleBox44SpecialMentions => (NoResString)"Mention(s) Spéciale(s): ";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
	static ZString TitleBox44SupportingDocuments => "Document(s) joint(s) : ";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
	static ZString TitleDocumentDTPInBox44 => "Disposition(s) tarifaire(s) particulière(s) : ";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
	static ZString TitleCANAInBox44 => "CANA(s) : ";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
	static ZString TitleTaxationSpecifique => "Taxations spécifiques : quantité : ";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
	static ZString TitleCodeMesurage => " - code mesurage : ";

	public static int Box44MaxLength => 593;

	public ZString GetBox44AddInfoAndDocuments(CusEntryLine entryLine, ZBool shouldHaveEconomicRegime)
	{
		var box44SeparateBoxes = GetBox44AddInfoAndDocumentsForSADH(entryLine, shouldHaveEconomicRegime);

		var result = new ZStringBuilder();
		result.Append(box44SeparateBoxes.First44box);

		if (!box44SeparateBoxes.Second44Box.IsEmpty)
		{
			result.Append(box44SeparateBoxes.Second44Box);
		}

		return result.ToStringWithNewLineBetweenAppends();
	}

	public (ZString First44box, ZString Second44Box) GetBox44AddInfoAndDocumentsForSADH(CusEntryLine entryLine, ZBool shouldHaveEconomicRegime)
	{
		var resultFirst44box = new ZStringBuilder();
		var resultSecond44Box = new ZStringBuilder();
		var newTextToAdd = new ZStringBuilder();
		var stopAddToFirstBox = false;
		var mainlineSupportingDocument = new ZStringBuilder();

		var isFirstEntryLine = entryLine.CL_LineNumber == 1;

		if (shouldHaveEconomicRegime && entryLine.Header.EntryInstruction?.SpecificRegimeAuthorisation != null)
		{
			AppendEconomicRegime(newTextToAdd, entryLine);
			stopAddToFirstBox = ShouldAssignInSecondBox(resultFirst44box, resultSecond44Box, newTextToAdd, stopAddToFirstBox);
		}

		newTextToAdd = new ZStringBuilder();
		newTextToAdd.AppendIfNotEmpty(GetFirstLineOfBox44());

		stopAddToFirstBox = ShouldAssignInSecondBox(resultFirst44box, resultSecond44Box, newTextToAdd, stopAddToFirstBox);
		newTextToAdd = new ZStringBuilder();

		var specialMentionsBuilder = new ZStringBuilder();
		AddAdditionalInfos(specialMentionsBuilder, isFirstEntryLine, entryLine);
		if (!specialMentionsBuilder.IsEmpty)
		{
			newTextToAdd.Append(TitleBox44SpecialMentions + specialMentionsBuilder.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.SemiColonAndspace));
		}

		stopAddToFirstBox = ShouldAssignInSecondBox(resultFirst44box, resultSecond44Box, newTextToAdd, stopAddToFirstBox);
		newTextToAdd = new ZStringBuilder();
		AddNationalAdditionalCode(newTextToAdd, entryLine.InvoiceLines);

		stopAddToFirstBox = ShouldAssignInSecondBox(resultFirst44box, resultSecond44Box, newTextToAdd, stopAddToFirstBox);
		newTextToAdd = new ZStringBuilder();
		if (isFirstEntryLine)
		{
			AddEntryHeaderSupportingDocumentsToFirstPage(mainlineSupportingDocument, entryLine.Header.SupportingDocuments);
		}

		AddEntryLineSupportingDocuments(mainlineSupportingDocument, entryLine.SupportingDocuments);

		if (!mainlineSupportingDocument.IsEmpty)
		{
			newTextToAdd.Append(TitleBox44SupportingDocuments + mainlineSupportingDocument.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.SemiColonAndspace));
		}

		stopAddToFirstBox = ShouldAssignInSecondBox(resultFirst44box, resultSecond44Box, newTextToAdd, stopAddToFirstBox);
		newTextToAdd = new ZStringBuilder();

		mainlineSupportingDocument = new ZStringBuilder();
		if (isFirstEntryLine)
		{
			AddEntryLineDTPSupportingDocumentsInfo(mainlineSupportingDocument, entryLine.Header.SupportingDocuments);
		}

		AddEntryLineDTPSupportingDocumentsInfo(mainlineSupportingDocument, entryLine.SupportingDocuments);
		if (!mainlineSupportingDocument.IsEmpty)
		{
			newTextToAdd.Append(TitleDocumentDTPInBox44 + mainlineSupportingDocument.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.SemiColonAndspace));
		}

		stopAddToFirstBox = ShouldAssignInSecondBox(resultFirst44box, resultSecond44Box, newTextToAdd, stopAddToFirstBox);
		newTextToAdd = new ZStringBuilder();

		if (entryLine.ThirdQuantity > 0 && !entryLine.ThirdUQ.IsEmpty)
		{
			newTextToAdd.Append(TitleTaxationSpecifique + entryLine.ThirdQuantity.Round(2) + TitleCodeMesurage + entryLine.ThirdUQ);
		}

		ShouldAssignInSecondBox(resultFirst44box, resultSecond44Box, newTextToAdd, stopAddToFirstBox);

		return (resultFirst44box.ToStringWithNewLineBetweenAppends(), resultSecond44Box.ToStringWithNewLineBetweenAppends());

		bool ShouldAssignInSecondBox(ZStringBuilder resultFirst44box, ZStringBuilder resultSecond44Box, ZStringBuilder newTextToAdd, bool stopAddToFirstBox)
		{
			if (!stopAddToFirstBox)
			{
				if (newTextToAdd.Length + resultFirst44box.Length > Box44MaxLength)
				{
					resultSecond44Box.Append(newTextToAdd);
					return true;
				}
				else
				{
					resultFirst44box.Append(newTextToAdd);
					return false;
				}
			}
			else
			{
				resultSecond44Box.Append(newTextToAdd);
				return true;
			}
		}
	}

	ZString GetFirstLineOfBox44() => ZString.Empty;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
	static void AppendEconomicRegime(ZStringBuilder result, CusEntryLine entryLine)
	{
		var economicRegimeBuilder = new ZStringBuilder();

		var entryInstruction = entryLine.Header.EntryInstruction;
		if (entryLine.Declaration is JobDeclaration declaration)
		{
			if (declaration.IsImport)
			{
				var economicAuthorization = (entryInstruction.SpecificRegimeNumber.IsEmpty ? string.Empty : "° Autorisation Économique (entrée) : " + entryInstruction.SpecificRegimeNumber)
					+ (entryInstruction.SpecificRegimeCountryCode.IsEmpty ? string.Empty : "	° Pays d’Autorisation : " + entryInstruction.SpecificRegimeCountryCode);
				economicRegimeBuilder.AppendIfNotEmpty(economicAuthorization);

				AppendIfNotEmpty(economicRegimeBuilder, "° Montant garanti : ", entryLine.SpecificRegimeGuaranteeAmount);
				AppendIfNotEmpty(economicRegimeBuilder, "° Délai d’apurement : ", entryLine.SpecificRegimeNumberDaysOfDischarge);
				AppendIfNotEmpty(economicRegimeBuilder, "Nature du perfectionnement, de la transformation ou de l’utilisation des marchandises : ", entryInstruction.SpecificRegimeNature);
				AppendIfNotEmpty(economicRegimeBuilder, "Description technique des marchandises et des produits compensateurs ou transformés et les moyens de les identifier : ", entryInstruction.SpecificRegimeDescription);
				AppendIfNotEmpty(economicRegimeBuilder, "Codes relatifs aux conditions économiques conformément à l’annexe 70 : ", entryInstruction.SpecificRegimeCondition);
				AppendIfNotEmpty(economicRegimeBuilder, "Bureau d’apurement : ", entryInstruction.SpecificRegimeOffice);
				AppendIfNotEmpty(economicRegimeBuilder, "Lieu de perfectionnement, de transformation ou d’utilisation : ", entryInstruction.SpecificRegimeLocation);
				AppendIfNotEmpty(economicRegimeBuilder, "Formalités de transfert proposées : ", entryInstruction.SpecificRegimeProcedureDescription);
			}
			else
			{
				var economicAuthorization = (entryInstruction.SpecificRegimeNumber.IsEmpty ? string.Empty : (NoResString)"° Autorisation Économique (sortie) : " + entryInstruction.SpecificRegimeNumber)
					+ (entryInstruction.SpecificRegimeCountryCode.IsEmpty ? string.Empty : (NoResString)"	° Pays d’Autorisation : " + entryInstruction.SpecificRegimeCountryCode);
				economicRegimeBuilder.AppendIfNotEmpty(economicAuthorization);
			}

			if (!economicRegimeBuilder.IsEmpty)
			{
				result.Append(TitleRegimeEconomiqueInBox44 + economicRegimeBuilder.ToStringWithNewLineBetweenAppends());
			}
		}
	}

	static void AppendIfNotEmpty(ZStringBuilder appendTo, ZString inputTitle, string inputValue)
	{
		if (!string.IsNullOrWhiteSpace(inputValue))
		{
			appendTo.Append(inputTitle + inputValue);
		}
	}

	static void AppendIfNotEmpty(ZStringBuilder appendTo, ZString inputTitle, INumericZType inputValue)
	{
		if (inputValue != null && !inputValue.IsEmpty)
		{
			appendTo.Append(inputTitle + inputValue);
		}
	}

	void AddAdditionalInfos(ZStringBuilder result, bool isFirstPage, CusEntryLine entryLine)
	{
		var additionalInfos =
			(isFirstPage ? entryLine.Header.AdditionalInfos.Union(entryLine.AdditionalInfos) : entryLine.AdditionalInfos.Except(entryLine.Header.AdditionalInfos))
			.Distinct();

		foreach (AdditionalInfo info in additionalInfos)
		{
			FormatAdditionalInformationStatement(result, info);
		}
	}

	void FormatAdditionalInformationStatement(ZStringBuilder result, AdditionalInfo info)
	{
		result.Append(info.CSI_Code + (info.CSI_Description.IsEmpty ? "" : "-" + info.CSI_Description));
	}

	void AddEntryHeaderSupportingDocumentsToFirstPage(ZStringBuilder result, IEnumerable<SupportingDocument> supportingDocuments) => ProducedDocumentsCertificatesBuilder.AppendSupportingDocumentInfo(result, supportingDocuments);

	Enterprise.DocumentWrappers.Customs.EU.ProducedDocumentsCertificatesBuilder ProducedDocumentsCertificatesBuilder => producedDocumentsCertificatesBuilder ?? (producedDocumentsCertificatesBuilder = new ProducedDocumentsCertificatesBuilder());
	Enterprise.DocumentWrappers.Customs.EU.ProducedDocumentsCertificatesBuilder producedDocumentsCertificatesBuilder;

	static void AddNationalAdditionalCode(ZStringBuilder result, Customs.Business.InvoiceLinesForEntryLineCollection invoiceLines)
	{
		var line = new ZStringBuilder();
		if (invoiceLines != null)
		{
			var nationalAdditionalCodes = invoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.FRAdditionalCodes).Distinct();

			nationalAdditionalCodes.ForEach(x => line.AppendIfNotEmpty(x));
		}
		if (!line.IsEmpty)
		{
			result.Append(TitleCANAInBox44 + line.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.SemiColonAndspace));
		}
	}

	void AddEntryLineDTPSupportingDocumentsInfo(ZStringBuilder mainLine, IEnumerable<SupportingDocument> supportingDocuments) => AppendSupportingDTPDocumentInfo(mainLine, supportingDocuments);

	void AddEntryLineSupportingDocuments(ZStringBuilder result, IEnumerable<SupportingDocument> supportingDocuments) => ProducedDocumentsCertificatesBuilder.AppendSupportingDocumentInfo(result, supportingDocuments);

	static void AppendSupportingDTPDocumentInfo(ZStringBuilder mainLine, IEnumerable<SupportingDocument> supportingDocuments)
	{
		foreach (Business.Declaration.SupportingDocument supportingDocument in supportingDocuments)
		{
			if (supportingDocument.CSI_IsDTP)
			{
				AppendSupportingDTPDocument(mainLine, supportingDocument);
			}
		}
	}

	static void AppendSupportingDTPDocument(ZStringBuilder mainLine, Business.Declaration.SupportingDocument supportingDocument) => mainLine.Append(supportingDocument.CSI_Code);
}
