using System;
using System.Collections.Generic;
using CargoWise.Customs.IL.MessageDefinitions.DOC.RES_276.D_NG_2716_MSG22001_AddAttachmentResponse;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC276ResponseMessagePrettier : ILEDIMessagePrettierBase<DNg2716Msg22001AddAttachmentResponse>
	{
		public ILDOC276ResponseMessagePrettier(MessageDataObject<DNg2716Msg22001AddAttachmentResponse> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData,
				builder =>
				builder.WithResponseSection((attachment, sb) =>
				{
					var dictionary = new Dictionary<ZString, ZString>()
					{
						{ PrettiedCaptions.Attachment.DocNumber, attachment.ExternalAttachmentId },
						{ PrettiedCaptions.Attachment.CustomsDocID, attachment.ResponseContentHeader.ApplicationId.ToString() },
						{ PrettiedCaptions.Common.Status, attachment.ResponseContentHeader.ApplicationId > 0 ? PrettiedCaptions.Common.Success : PrettiedCaptions.Common.Failed }
					};

					sb.Append(ToBaseInformationPart(dictionary));
				})
				.WithExceptionsSection(
					attachment => attachment.ResponseContentHeader.ApplicationId == 0 && attachment.ResponseContentHeader?.Exception.Count > 0,
					attachment => attachment.ResponseContentHeader.Exception,
					ex => ex.ExceptionLevel,
					ex => ex.ExeptionType,
					ex => ex.ExeptionDescription,
					ex => ex.ExceptionParms,
					new Dictionary<string, Func<CargoWise.Customs.IL.MessageDefinitions.DOC.RES_276.Customs.Exception, string>>
					{
						{  PrettiedCaptions.Attachment.EnglishDescription, ex => ex.EnglishDescription },
					},
					new string[]
					{
						PrettiedCaptions.Exceptions.ExceptionLevel,
						PrettiedCaptions.Exceptions.ExceptionType,
						PrettiedCaptions.Exceptions.ExceptionParams,
						PrettiedCaptions.Exceptions.ExceptionDescription,
						PrettiedCaptions.Attachment.EnglishDescription
					}
				)
			)
			.Build();
	}
}
