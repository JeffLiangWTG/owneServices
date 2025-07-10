using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class StandardMessageInstructions : IMessageInstructions
	{
		public StandardMessageInstructions(IStandardTemplate template, IMacroScope scope, IMacroEvaluationContext context, string documentName)
		{
			Argument.NotNull(template, nameof(template));
			this.template = template;
			this.scope = scope;
			this.context = context;
			this.DocumentName = documentName;
			this.lazyAmendmentOptions = new Lazy<ICodeDescriptionPairList>(() => CreateCodeDescriptionPairList(template.GetMessageAmendmentOptions()));
			this.lazyWidthdrawalOptions = new Lazy<ICodeDescriptionPairList>(() => CreateCodeDescriptionPairList(template.GetMessageCancellationOptions()));
		}

		readonly IStandardTemplate template;
		readonly IMacroScope scope;
		readonly IMacroEvaluationContext context;

		public string DocumentName { get; }
		public string TranslatedDocumentName
		{
			get
			{
				if (documentNameCaptions == null)
				{
					documentNameCaptions = new Dictionary<string, string>();
				}

				if (documentNameCaptions.TryGetValue(Res.CurrentLanguage, out var translatedDocumentName))
				{
					return translatedDocumentName;
				}

				translatedDocumentName = template.GetTranslatedDocumentName();
				translatedDocumentName = string.IsNullOrEmpty(translatedDocumentName) ? DocumentName : translatedDocumentName;
				documentNameCaptions[Res.CurrentLanguage] = translatedDocumentName;

				return translatedDocumentName;
			}
		}
		IDictionary<string, string> documentNameCaptions;

		public string DataContext => dataContext ?? (dataContext = template.DataContext);
		string dataContext;

		public string Recipient
		{
			get
			{
				if (recipientCaptions == null)
				{
					recipientCaptions = new Dictionary<string, string>();
				}

				if (recipientCaptions.TryGetValue(Res.CurrentLanguage, out var recipient))
				{
					return recipient;
				}

				recipient = template.GetMessageRecipient();
				recipientCaptions[Res.CurrentLanguage] = recipient;
				return recipient;
			}
		}
		IDictionary<string, string> recipientCaptions;

		public string EHubClientID => eHubClientID ?? (eHubClientID = template.GetEHubClientID());
		string eHubClientID;

		public string DirectXTClientID => directXTClientID ?? (directXTClientID = template.GetDirectXTClientID());
		string directXTClientID;

		public string XmlNamespace => xmlNamespace ?? (xmlNamespace = template.GetXmlNamespace());
		string xmlNamespace;

		public bool AllowSendMessage => allowSendMessage ?? (allowSendMessage = template.GetAllowSendMessage(scope, context)).Value;
		bool? allowSendMessage;

		public bool AllowSendMessageAmendment => allowSendMessageAmendment ?? (allowSendMessageAmendment = template.GetAllowSendMessageAmendment(scope)).Value;
		bool? allowSendMessageAmendment;

		public bool AllowSendMessageWithdrawal => allowSendMessageWithdrawal ?? (allowSendMessageWithdrawal = template.GetAllowSendMessageWithdrawal(scope)).Value;
		bool? allowSendMessageWithdrawal;

		public bool AllowResetToOriginal => allowResetToOriginal ?? (allowResetToOriginal = template.GetAllowResetToOriginal()).Value;
		bool? allowResetToOriginal;

		public bool OrderLogsByLocalTime => orderLogsByLocalTime ?? (orderLogsByLocalTime = template.OrderLogsByLocalTime()).Value;
		bool? orderLogsByLocalTime;

		public bool RequireMessageAmendmentReason => requireMessageAmendmentReason ?? (requireMessageAmendmentReason = template.GetRequireMessageAmendmentReason()).Value;
		bool? requireMessageAmendmentReason;

		public ICodeDescriptionPairList AmendmentOptions => lazyAmendmentOptions.Value;
		readonly Lazy<ICodeDescriptionPairList> lazyAmendmentOptions;

		public ICodeDescriptionPairList WidthdrawalOptions => lazyWidthdrawalOptions.Value;
		readonly Lazy<ICodeDescriptionPairList> lazyWidthdrawalOptions;

		ICodeDescriptionPairList CreateCodeDescriptionPairList(object[] untypedList)
		{
			if (untypedList == null)
			{
				return null;
			}

			var result = new CodeDescriptionPairList();

			foreach (var element in untypedList)
			{
				var pair = element as object[];

				if (pair == null
					|| pair.Length < 2)
				{
					continue;
				}

				result.AddPairIfNotExist(
					Convert.ToString(pair[0], CultureInfo.InvariantCulture),
					Convert.ToString(pair[1], CultureInfo.InvariantCulture));
			}

			return result.Count > 0
				? result
				: null;
		}
	}
}
