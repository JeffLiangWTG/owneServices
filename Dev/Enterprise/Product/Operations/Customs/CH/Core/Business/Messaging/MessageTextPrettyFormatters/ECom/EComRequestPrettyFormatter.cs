using System;
using System.Collections.Specialized;
using System.Net;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class EComRequestPrettyFormatter : IMessagePrettyFormatter
{
	public EComRequestPrettyFormatter(BusinessObjectFactory factory, IEdecComplaintRequestDetail requestDetail, bool isTransmitMessage)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.requestDetail = requestDetail;
		this.isTransmitMessage = isTransmitMessage;
	}

	readonly BusinessObjectFactory factory;
	readonly IEdecComplaintRequestDetail requestDetail;
	readonly bool isTransmitMessage;

	public ZString GetFormattedText()
	{
		if (requestDetail is null)
		{
			return FormattableString.Invariant($"<h2>{MessageIsEmpty}</h2>");
		}

		var correctionReason = CorrectionReasons.GetDescriptionFromCode(requestDetail.CorrectionReason);

		var htmlBuilder = new ZStringBuilder();
		htmlBuilder.Append($"<h2>{(isTransmitMessage ? RequestTitle : RequestByCustomsTitle)}</h2>");
		htmlBuilder.Append($"<p>{CorrectionReasonLabel}: {WebUtility.HtmlEncode(requestDetail.CorrectionReason)} - {correctionReason}</p>");
		htmlBuilder.Append($"<p>{AttachedDeclarationLabel}: {(requestDetail.AttachedDeclaration ? YesLabel : NoLabel)}</p>");

		if (!isTransmitMessage)
		{
			htmlBuilder.Append($"<h3>{WebUtility.HtmlEncode(CustomsTitle)}</h3>");
			var customsTable = new HtmlTableCreator(new NameValueCollection { { (NoResString)"border", "0" } });
			customsTable.WriteRow(CustomsOfficerLabel + ":", requestDetail.CustomsOfficer);

			if (!requestDetail.AppealText.IsNullOrEmpty())
			{
				customsTable.WriteRow(ReminderLabel + ":", requestDetail.AppealText);
			}

			if (!requestDetail.PaperCorrespondence.IsNullOrEmpty())
			{
				customsTable.WriteRow(PaperCorrespondenceLabel + ":", requestDetail.PaperCorrespondence);
			}

			htmlBuilder.Append(customsTable.ToHtml());
		}

		var eComplaintFields = EComplaintFields;

		htmlBuilder.Append($"<h3>{EComLinesTitle}</h3>");
		var ecomLinesTable = new HtmlTableCreator(ColumnTitles);
		foreach (var complaint in requestDetail.Complaints)
		{
			var fieldName = eComplaintFields.GetDescriptionFromCode(complaint.FieldName);
			ecomLinesTable.WriteRow(GetLocationLabel(complaint.Location), complaint.TraderItemID, fieldName, complaint.Remark);
		}

		htmlBuilder.Append(ecomLinesTable.ToHtml());
		return htmlBuilder.ToString();
	}

	CodeDescriptionPairList CorrectionReasons => RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CorrectionReason, ZDateTime.Today);

	CodeDescriptionPairList EComplaintFields => RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.EComplaintFields, ZDateTime.Today);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	string GetLocationLabel(string location)
	{
		switch (location)
		{
			case "Header":
				return HeaderLabel;
			case "Line":
				return LineLabel;
		}
		return string.Empty;
	}

	#region Labels

	static string RequestTitle => Res.GetString("311F6EF6-D072-4961-8111-BDDF35A15D69", "eCom Request");
	static string RequestByCustomsTitle => Res.GetString("17BBACC3-B1AD-4A2A-8007-2311B9740920", "eCom Request by Customs");
	static string CustomsTitle => Res.GetString("2C30F818-3292-4EA6-B0C8-3DD0B7BF158A", "Customs");
	static string EComLinesTitle => Res.GetString("07E531DD-5BBA-4833-B8B0-803E045D61F9", "eCom Lines");
	static string CorrectionReasonLabel => Res.GetString("0D3D0422-462C-4FB8-A141-7C3F8A797B74", "Correction Reason");
	static string AttachedDeclarationLabel => Res.GetString("7578EC05-04DB-4CD5-8990-00A758005982", "Attached Declaration");
	static string CustomsOfficerLabel => Res.GetString("EC53957B-D032-4FCB-85C2-6AB155015DED", "Customs Officer");
	static string ReminderLabel => Res.GetString("8FBA4DB0-8D4C-4738-8B59-364DE0D18740", "Reminder");
	static string PaperCorrespondenceLabel => Res.GetString("B0F182C6-17D8-4DC7-9C65-E5CFF50F0D7B", "Paper Correspondence");

	string[] ColumnTitles => new[] { LocationLabel, EntryLineLabel, FieldNameLabel, RemarkLabel };
	static string LocationLabel => Res.GetString("44274EFD-FBF6-424D-810F-6DFD5B175A98", "Location");
	static string EntryLineLabel => Res.GetString("3DC7BAED-ECAE-4B98-B134-93469BBC9EBB", "Entry Line #");
	static string FieldNameLabel => Res.GetString("B89DC529-25E4-43A3-AD93-7ADE020123EB", "Field Name");
	static string RemarkLabel => Res.GetString("7E0F0CA7-C704-4511-9F1E-76E41175DB2A", "Remark");

	static string YesLabel => Res.GetString("12EDF763-CCFA-4B2E-9939-D8D8CD24E207", "Yes");
	static string NoLabel => Res.GetString("ED813A2D-9C19-4AFC-A616-3B0F1D30CD39", "No");

	static string HeaderLabel => Res.GetString("31881B3E-16D3-4B34-936C-BEF8129939FC", "Header");
	static string LineLabel => Res.GetString("2D2EEEC9-B89F-427A-A36C-49EED5A656CA", "Line");

	static string MessageIsEmpty => Res.GetString("837FAE4E-BAC6-468C-918C-A046F4CDBDC8", "Message is empty");

	#endregion
}
