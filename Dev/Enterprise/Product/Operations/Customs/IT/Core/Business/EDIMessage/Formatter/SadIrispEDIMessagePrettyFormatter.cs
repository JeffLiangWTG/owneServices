using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

sealed class SadIrispEDIMessagePrettyFormatter : ITEDIMessagePrettyFormatter
{
	public SadIrispEDIMessagePrettyFormatter(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		LoadIrisp(originalText);
		return FormatIrispContent();
	}

	ZString FormatIrispContent()
	{
		var stringBuilder = new ZStringBuilder();
		AppendIfNotEmpty(stringBuilder, ResStrings.Filename, irisp.IdocElaborationInfo.IdocName);

		AppendSectionIfNotEmpty(stringBuilder, FormatPositiveResponseMessage());
		AppendSectionIfNotEmpty(stringBuilder, FormatNegativeResponseMessages());
		AppendSectionIfNotEmpty(stringBuilder, FormatNbPositiveResponseMessage());
		AppendSectionIfNotEmpty(stringBuilder, FormatNbNegativeResponseMessages());
		return stringBuilder.ToString();
	}

	#region Negative Response Message
	ZString FormatNegativeResponseMessages()
	{
		var isImport = (irisp.ResponseMessages.FirstOrDefault()?.MessageCode ?? ZString.Empty) == IrispConstants.MessageTypes.IM;
		return isImport ? FormatNegativeResponseMessagesForImport() : FormatNegativeResponseMessagesForExport();
	}

	ZString FormatNegativeResponseMessagesForExport()
	{
		return FormatNegativeResponseMessages(NegativeResponseMessages, new CustomsFieldDescriptionsProvider<SadExportFieldDescriptionList>(factory));
	}

	ZString FormatNegativeResponseMessagesForImport()
	{
		return FormatNegativeResponseMessages(NegativeResponseMessages, new CustomsFieldDescriptionsProvider<SadImportFieldDescriptionList>(factory));
	}

	void AppendTableRow(ZStringBuilder stringBuilder, ZString headingText, ZString valueText) => stringBuilder.Append(FormattableString.Invariant($"<tr><td><p>{headingText}:</p></td><td>{valueText}</td></tr>"));

	ZString FormatGenericNegativeResponseMessages<T>(IEnumerable<UnifiedDeclarationPositiveResponseMessage> positiveMessages, IEnumerable<UnifiedDeclarationNegativeResponseMessage> negativeMessages, CustomsFieldDescriptionsProvider<T> fieldsDescriptionProvider, ZString title)
		where T : CodeDescriptionPairList, new()
	{
		if (negativeMessages.Any())
		{
			var stringBuilder = new ZStringBuilder();
			AppendSectionHeader(stringBuilder, positiveMessages, negativeMessages, title);
			stringBuilder.Append(FormatNegativeResponseMessages(negativeMessages, fieldsDescriptionProvider));
			return stringBuilder.ToString();
		}
		return ZString.Empty;
	}
	ZString FormatNbNegativeResponseMessages() => FormatGenericNegativeResponseMessages(NbPositiveResponseMessages, NbNegativeResponseMessages, new CustomsFieldDescriptionsProvider<NbSadFieldDescriptionList>(factory), ResStrings.NbRecords);

	ZString FormatNegativeResponseMessages<T>(IEnumerable<UnifiedDeclarationNegativeResponseMessage> responseMessages, CustomsFieldDescriptionsProvider<T> customsFieldDescriptionProvider)
		where T : CodeDescriptionPairList, new()
	{
		var stringBuilder = new ZStringBuilder();
		for (int i = 0; i < responseMessages.Count(); i++)
		{
			var irregularityRecord = responseMessages.ElementAt(i).Irregularity;
			var errorDescriptionWithoutPrefixAndColon = irregularityRecord.ErrorDescription.SubstringSafe(7).Replace(':', '-');
			var fieldDescription = customsFieldDescriptionProvider.GetDescriptionByReferenceCode(irregularityRecord.FieldSadBoxNumber);
			stringBuilder
				.Append(FormattableString.Invariant($"<h3 style='color:red;margin-left: 10pt'>{ResStrings.ErrorNumber(i + 1)}</h3>"))
				.Append(Br())
				.Append((NoResString)"<table style='margin-left: 20pt'>");

			AppendTableRow(stringBuilder, ResStrings.EntryLine, irregularityRecord.ItemNumber.ToString());
			AppendTableRow(stringBuilder, ResStrings.Field, !fieldDescription.IsEmpty ? FormattableString.Invariant($"{irregularityRecord.FieldSadBoxNumber} {fieldDescription}") : (string)irregularityRecord.FieldIdentificative);
			AppendTableRow(stringBuilder, ResStrings.Occurrence, irregularityRecord.SequentialNumberOfFieldInRecord.ToString());
			AppendTableRow(stringBuilder, ResStrings.ErrorType, irregularityRecord.ErrorType);
			AppendTableRow(stringBuilder, ResStrings.ErrorMessage, errorDescriptionWithoutPrefixAndColon);

			stringBuilder.Append((NoResString)"</table>");
		}
		return stringBuilder.ToString();
	}

	#endregion

	#region Positive Response Message

	ZString FormatPositiveResponseMessage()
	{
		var positiveResponseMessage = PositiveResponseMessages.FirstOrDefault();
		if (positiveResponseMessage != null)
		{
			(var status, var statusColor) = GetStatusAndColorStylePrettified(positiveResponseMessage);
			var stringBuilder = new ZStringBuilder();
			AppendIfNotEmpty(stringBuilder, ResStrings.Status, status, valueColorStyle: statusColor);
			AppendIfNotEmpty(stringBuilder, ResStrings.CustomsOffice, FormattableString.Invariant($"IT{irisp.Header.CustomsOfficeSectionCode}"));
			AppendIfNotEmpty(stringBuilder, ResStrings.RegistrationInfo, positiveResponseMessage.FullRegistrationInfo);
			AppendIfNotEmpty(stringBuilder, ResStrings.Mrn, positiveResponseMessage.MrnCode);
			AppendIfNotEmpty(stringBuilder, ResStrings.ClearanceCode, positiveResponseMessage.ReleaseCode);
			AppendIfNotEmpty(stringBuilder, ResStrings.A93Number, positiveResponseMessage.A93Number);
			AppendIfNotEmpty(stringBuilder, ResStrings.FirstMethodOfPayment, positiveResponseMessage.FirstPaymentMethod);
			AppendIfNotEmpty(stringBuilder, ResStrings.FirstExpiryDate, positiveResponseMessage.FirstPaymentDueDate);
			AppendIfNotEmpty(stringBuilder, ResStrings.SecondMethodOfPayment, positiveResponseMessage.SecondPaymentMethod);
			AppendIfNotEmpty(stringBuilder, ResStrings.SecondExpiryDate, positiveResponseMessage.SecondPaymentDueDate);
			AppendIfNotEmpty(stringBuilder, ResStrings.ThirdMethodOfPayment, positiveResponseMessage.ThirdPaymentMethod);
			AppendIfNotEmpty(stringBuilder, ResStrings.ThirdExpiryDate, positiveResponseMessage.ThirdPaymentDueDate);
			return stringBuilder.ToString();
		}
		return ZString.Empty;
	}

	ZStringBuilder AppendIfNotEmpty(ZStringBuilder stringBuilder, ZString headingText, ZString valueText, string valueColorStyle = "")
	{
		if (!valueText.IsEmpty)
		{
			stringBuilder
				.Append(FormatHeadingAndValue(headingText, valueText, valueColorStyle: valueColorStyle))
				.Append(Br()).Append(Br());
		}
		return stringBuilder;
	}

	ZStringBuilder AppendIfNotEmpty(ZStringBuilder stringBuilder, ZString headingText, ZDate value) => AppendIfNotEmpty(stringBuilder, headingText, value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

	ZString FormatHeadingAndValue(ZString fieldHeading, ZString fieldValue, string valueColorStyle = "") => FormattableString.Invariant($"<h3 style='display:inline'>{fieldHeading}: </h3><p style='display:inline;color:{valueColorStyle ?? ""}'>{fieldValue}</<p>");

	ZString Br() => (NoResString)"<br/>";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant status color")]
	(ZString Status, ZString Color) GetStatusAndColorStylePrettified(SadPositiveResponseMessage positiveResponseMessage)
	{
		const string green = "green";
		const string red = "red";
		var color = "black";
		ZString customsStatus;
		if (positiveResponseMessage.IsCleared)
		{
			customsStatus = ResStrings.Cleared;
			color = green;
		}
		else if (positiveResponseMessage.IsUnderControl)
		{
			customsStatus = ResStrings.UnderControl;
			color = red;
		}
		else if (positiveResponseMessage.IsRegistered)
		{
			customsStatus = ResStrings.Registered;
			color = green;
		}
		else
		{
			customsStatus = positiveResponseMessage.ReleaseNotes;
		}
		return (Status: customsStatus, Color: color);
	}

	ZString FormatGenericPositiveResponseMessage(IEnumerable<UnifiedDeclarationPositiveResponseMessage> positiveMessages, IEnumerable<UnifiedDeclarationNegativeResponseMessage> negativeMessages, ZString title)
	{
		if (positiveMessages.Any() && !negativeMessages.Any())
		{
			var stringBuilder = new ZStringBuilder();
			AppendSectionHeader(stringBuilder, positiveMessages, negativeMessages, title);
			stringBuilder.Append(FormattableString.Invariant($"<h3 style='color:green'><b>{ResStrings.AllApproved}</b></h3>"));
			return stringBuilder.ToString();
		}

		return ZString.Empty;
	}
	ZString FormatNbPositiveResponseMessage() => FormatGenericPositiveResponseMessage(NbPositiveResponseMessages, NbNegativeResponseMessages, ResStrings.NbRecords);

	#endregion

	void LoadIrisp(ZString messageText)
	{
		var irispCustomsInterchangeResult = CustomsInterchange.LoadSafe<IrispTypeR>(messageText);
		if (!irispCustomsInterchangeResult.IsValid)
		{
			throw new UnableToInterpretInterchangeException(ResStrings.IrispMalformed);
		}
		irisp = irispCustomsInterchangeResult.Interchange;
	}

	void AppendSectionIfNotEmpty(ZStringBuilder stringBuilder, ZString section)
	{
		if (!section.IsEmpty)
		{
			stringBuilder.Append(section).Append(Br());
		}
	}

	void AppendSectionHeader(ZStringBuilder stringBuilder, IEnumerable<UnifiedDeclarationPositiveResponseMessage> positiveMessages, IEnumerable<UnifiedDeclarationNegativeResponseMessage> negativeMessages, ZString title)
	{
		if (PositiveResponseMessages.Any() || NegativeResponseMessages.Any())
		{
			stringBuilder.Append((NoResString)"<hr/>");
		}
		if (positiveMessages.Any() || negativeMessages.Any())
		{
			stringBuilder.Append(FormattableString.Invariant($"<h3 style='color:#5d85cc'><b>{title}</b></h3>"));
		}
	}

	IrispTypeR irisp;

	IEnumerable<SadPositiveResponseMessage> PositiveResponseMessages => positiveResponseMessages ?? (positiveResponseMessages = irisp.GetSadPositiveResponseMessages());
	IEnumerable<SadPositiveResponseMessage> positiveResponseMessages;

	IEnumerable<SadNegativeResponseMessage> NegativeResponseMessages => negativeResponseMessages ?? (negativeResponseMessages = irisp.GetSadNegativeResponseMessages());
	IEnumerable<SadNegativeResponseMessage> negativeResponseMessages;

	IEnumerable<SadNbPositiveResponseMessage> NbPositiveResponseMessages => nbPositiveResponseMessages ?? (nbPositiveResponseMessages = irisp.GetNbPositiveResponseMessages());
	IEnumerable<SadNbPositiveResponseMessage> nbPositiveResponseMessages;

	IEnumerable<SadNbNegativeResponseMessage> NbNegativeResponseMessages => nbNegativeResponseMessages ?? (nbNegativeResponseMessages = irisp.GetNbNegativeResponseMessages());
	IEnumerable<SadNbNegativeResponseMessage> nbNegativeResponseMessages;

	static class ResStrings
	{
		public static ZString Filename => Res.GetString("E068CFB0-1FCF-4356-85F0-FEE1670823B6", "Filename");
		public static ZString Status => Res.GetString("7717B61B-B55D-4210-9EC8-858B8F651AB4", "Status");
		public static ZString CustomsOffice => Res.GetString("50C2A1EB-2FD4-4499-BF66-67121D465149", "Customs Office");
		public static ZString RegistrationInfo => Res.GetString("3B3571AA-9DFA-4FDC-B53C-9CF01C6C60C5", "Reg. No");
		public static ZString Mrn => Res.GetString("EF20D6EA-EBB1-468C-B15A-758BAEE47F07", "MRN");
		public static ZString ClearanceCode => Res.GetString("8F5B653A-2CB1-4770-8DB7-9CAC165E17FC", "Clearance Code");
		public static ZString A93Number => Res.GetString("41A6ED92-1664-4888-8E7D-592B4FABD4ED", "A93 number");
		public static ZString FirstMethodOfPayment => Res.GetString("677FC403-0666-4093-B086-B8FD50453BE0", "First method of payment");
		public static ZString FirstExpiryDate => Res.GetString("9518DD2B-E4B0-4C94-B4EE-98BCE126F2E1", "First expiry date");
		public static ZString SecondMethodOfPayment => Res.GetString("3E3B2047-E2AD-430E-932B-A538A0CBA117", "Second method of payment");
		public static ZString SecondExpiryDate => Res.GetString("534C813F-BEDC-4A96-82FB-4C934FD8DE41", "Second expiry date");
		public static ZString ThirdMethodOfPayment => Res.GetString("D239A274-1D0E-4BCC-93C1-2C7108503F12", "Third method of payment");
		public static ZString ThirdExpiryDate => Res.GetString("8495AE2D-7D33-45D3-8D27-2E713E49175D", "Third expiry date");
		public static ZString IrispMalformed => Res.GetString("D45A2CD5-6983-40F2-A471-1A3EB96BC0AB", "IRISP malformed");
		public static ZString Cleared => Res.GetString("2B53947D-9869-4844-8504-1F2B861E9EA1", "Cleared");
		public static ZString UnderControl => Res.GetString("518163B6-D5ED-4F13-A5C6-EB9C47BE827B", "Under control");
		public static ZString Registered => Res.GetString("F9463562-EA35-4FD2-8BAE-DB017CCD89AD", "Registered");
		public static ZString ErrorNumber(ZInt number) => Res.GetString("5E9A27F4-651F-4122-B057-B800C6AC73C1", "Error {0}", number);
		public static ZString EntryLine => Res.GetString("B151C7D5-1646-409A-9AF6-598931D609EA", "Entry Line");
		public static ZString Field => Res.GetString("3FF514FD-6611-4C1D-A2B3-F0A9BB749228", "Field");
		public static ZString Occurrence => Res.GetString("3EAC10B4-6092-41B0-A4AD-33F62668FD34", "Occurrence");
		public static ZString ErrorType => Res.GetString("8C258006-7844-4FD9-A77F-533D008EFB2A", "Error Type");
		public static ZString ErrorMessage => Res.GetString("A3322305-041B-44AD-BA00-F2154A782C6C", "Error Message");
		public static ZString NbRecords => Res.GetString("E184F0CC-1722-4A31-B669-F9161F5DEC06", "NB Records");
		public static ZString AllApproved => Res.GetString("F5AB1F2F-DA02-4983-B952-C21102926C64", "All Approved");
	}
}
