using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class ExportDeclarationMessageSendingObjectLookups : DeclarationMessageSendingObjectLookups
{
	public ExportDeclarationMessageSendingObjectLookups(BusinessObject parent) : base(parent)
	{
	}

	public new ExportDeclarationMessageSendingObject Parent => (ExportDeclarationMessageSendingObject)base.Parent;

	public override CodeDescriptionPairList MessageTypeList
	{
		get
		{
			var cacheKey = GetCacheKey(Parent.Header.Declaration.JE_MessageType, Parent.Header.Declaration.JE_MessageSubType, Parent.Header.CH_EntryStatus, !Parent.Header.MovementReferenceNumber.IsEmpty);
			return Factory.GetCachedValue($"CH.Business.ExportDeclarationMessageSendingObjectLookups.MessageTypeList|{cacheKey}", () => GetMessageTypeList(cacheKey));
		}
	}

	string GetCacheKey(ZString messageType, ZString messageSubType, ZString entryStatus, bool hasMRN)
	{
		switch (messageType)
		{
			case MessageTypeCodeList.Codes.Export:
				var cacheKey = new StringBuilder(MessageTypeCodeList.Codes.Export);
				switch (entryStatus)
				{
					case "":
						cacheKey.Append((NoResString)"|empty");
						break;
					case AdditionalCHEntryStatusList.Codes.CustomsAssessmentDecision:
					case EntryStatusList.Codes.Cancelled:
					case EntryStatusList.Codes.Clear:
						cacheKey.Append($"|{entryStatus}");
						break;
					default:
						cacheKey.Append((NoResString)"|other");
						break;
				}
				return cacheKey.Append($"|{(hasMRN ? "MRN" : "noMRN")}").ToString();
			case CHJobMessageTypeList.Codes.ExportDeclarationActivation:
				cacheKey = new StringBuilder(CHJobMessageTypeList.Codes.ExportDeclarationActivation + "|" + messageSubType);
				switch (entryStatus)
				{
					case null:
					case "":
						break;
					case SwissCustomsConstants.CustomsStatusCodes.SubmittedToTaxud:
						cacheKey.Append($"|{entryStatus}");
						break;
					default:
						cacheKey.Clear();
						break;
				}
				return cacheKey.ToString();
			default:
				return string.Empty;
		}
	}

	CodeDescriptionPairList GetMessageTypeList(string cacheKey)
	{
		var messageTypeList = new CodeDescriptionPairList();
		switch (cacheKey)
		{
			case $"{MessageTypeCodeList.Codes.Export}|{AdditionalCHEntryStatusList.Codes.CustomsAssessmentDecision}|MRN":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE069, PassarMessageTypeList.Descriptions.NE069));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC016, PassarMessageTypeList.Descriptions.NC016));
				break;
			case $"{MessageTypeCodeList.Codes.Export}|{EntryStatusList.Codes.Cancelled}|MRN":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC016, PassarMessageTypeList.Descriptions.NC016));
				break;
			case $"{MessageTypeCodeList.Codes.Export}|{EntryStatusList.Codes.Clear}|noMRN":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE013, PassarMessageTypeList.Descriptions.NE013));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE014, PassarMessageTypeList.Descriptions.NE014));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC123, PassarMessageTypeList.Descriptions.NC123));
				break;
			case $"{MessageTypeCodeList.Codes.Export}|{EntryStatusList.Codes.Clear}|MRN":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE013, PassarMessageTypeList.Descriptions.NE013));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE014, PassarMessageTypeList.Descriptions.NE014));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC123, PassarMessageTypeList.Descriptions.NC123));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC016, PassarMessageTypeList.Descriptions.NC016));
				break;
			case $"{MessageTypeCodeList.Codes.Export}|empty|noMRN":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE015, PassarMessageTypeList.Descriptions.NE015));
				break;
			case $"{MessageTypeCodeList.Codes.Export}|other|noMRN":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE013, PassarMessageTypeList.Descriptions.NE013));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE014, PassarMessageTypeList.Descriptions.NE014));
				break;
			case $"{MessageTypeCodeList.Codes.Export}|other|MRN":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE013, PassarMessageTypeList.Descriptions.NE013));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE014, PassarMessageTypeList.Descriptions.NE014));
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC016, PassarMessageTypeList.Descriptions.NC016));
				break;
			case $"{CHJobMessageTypeList.Codes.ExportDeclarationActivation}|PAS":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC123, PassarMessageTypeList.Descriptions.NC123));
				break;
			case $"{CHJobMessageTypeList.Codes.ExportDeclarationActivation}|EDC":
			case $"{CHJobMessageTypeList.Codes.ExportDeclarationActivation}|EDC|{SwissCustomsConstants.CustomsStatusCodes.SubmittedToTaxud}":
				messageTypeList.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NE130, PassarMessageTypeList.Descriptions.NE130));
				break;
		}
		return messageTypeList;
	}

	public override CodeDescriptionPairList CorrectionReasonList
	{
		get
		{
			var codeType = ZString.Empty;
			switch (Parent.MessageType)
			{
				case PassarMessageTypeList.Codes.NE013:
					codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1053;
					break;
				case PassarMessageTypeList.Codes.NE014:
					codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1054;
					break;
				case PassarMessageTypeList.Codes.NE069:
					codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1057;
					break;
			}
			return codeType.IsEmpty
				? new CodeDescriptionPairList()
				: RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, codeType, Parent.Header.EffectiveValuationDate);
		}
	}

	public CodeDescriptionPairList NextProcedureList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.NextProcedure, ZDateTime.Today);
}
