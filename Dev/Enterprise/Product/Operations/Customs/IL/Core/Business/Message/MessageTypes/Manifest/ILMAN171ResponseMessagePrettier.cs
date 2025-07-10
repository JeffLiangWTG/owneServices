using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.NS3;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN171ResponseMessagePrettier : ILEDIMessagePrettierBase<MnMsg4SendManifestFeedBackMessage>
	{
		public ILMAN171ResponseMessagePrettier(MessageDataObject<MnMsg4SendManifestFeedBackMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
		{
			var factory = Factory;

			return BuildMessageInterpretation()
				.OfSingle(
					() => MessageData.ResponseContentHeader,
					b => b
						.WithExceptionsSection(
							p => p.Exception?.Count > 0,
							p => p.Exception,
							ex => ex.ExceptionLevel,
							ex => ex.ExeptionType,
							ex => ex.ExeptionDescription,
							ex => ex.ExceptionParms
						)
				)
				.OfSingle(
					() => MessageData.Response,
					b => b
					.WithResponseSection(
						(p, sb) =>
						{
							var dictionary = new Dictionary<ZString, ZString>()
							{
								{ PrettiedCaptions.Manifest.IssuingDate, p.IssueDateTime },
								{ PrettiedCaptions.Manifest.FunctionalReference, p.FunctionalReferenceId.Value }
							};

							sb.Append(ToBaseInformationPart(dictionary));
						})
					.WithSectionOf<ResponseStatus>(
						p => p.Status != null && p.Status.Count > 0,
						PrettiedCaptions.Manifest.StatusSection,
						p => () => p.Status,
						b1 => b1.WithSection(
							(status, sb) =>
							{
								var dictionary = new Dictionary<ZString, ZString>()
									{
										{ PrettiedCaptions.Common.Status, factory.GetCodeDescriptionFromRefCusCodeListCombinedCode(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, status.NameCode.Value) },
										{ PrettiedCaptions.Manifest.StatusDate, status.EffectiveDateTime },
									};
								sb.Append(ToBaseInformationPart(dictionary));

								if (status.Pointer != null && status.Pointer.Count > 0)
								{
									var columns = new[]
									{
										new TableColumn<ResponseStatusPointer>(
											PrettiedCaptions.Manifest.StatusPointersSequenceNumeric,
											s => s.SequenceNumeric?.ToString()
										),
										new TableColumn<ResponseStatusPointer>(
											PrettiedCaptions.Manifest.StatusPointersDocumentSectionCode,
											s => s.DocumentSectionCode?.Value
										),
										new TableColumn<ResponseStatusPointer>(
											PrettiedCaptions.Manifest.StatusPointersTagID,
											s => s.TagId?.Value
										)
									};

									sb.Append(ToPointTablePart(PrettiedCaptions.Manifest.StatusPointersSection, status.Pointer, columns));
								}
							}))
					.WithSectionOf<ResponseError>(
						p => p.Error != null && p.Error.Count > 0,
						PrettiedCaptions.Errors.ErrorsSection,
						p => () => p.Error,
						b1 => b1.WithSection(
							(err, sb) =>
							{
								var dictionary = new Dictionary<ZString, ZString>()
										{
											{ PrettiedCaptions.Manifest.ValidationCode, err.ValidationCode?.Value },
											{ PrettiedCaptions.Manifest.ValidationName, err.ValidationCode?.Name },
											{ PrettiedCaptions.Manifest.ValidationListName, err.ValidationCode?.ListName },
										};
								sb.Append(ToBaseInformationPart(dictionary));

								var codeTypeAttributesTable = GenerateCodeTypeAttributesTable(err.ValidationCode);
								if (!codeTypeAttributesTable.IsEmpty)
								{
									sb.Append("<br>");
									sb.Append(codeTypeAttributesTable);
									sb.Append("<br>");
								}

								if (err.Pointer != null && err.Pointer.Count > 0)
								{
									var columns = new[]
									{
										new TableColumn<ResponseErrorPointer>(
											PrettiedCaptions.Pointers.SequenceNumeric,
											s => s.SequenceNumeric?.ToString()
										),
										new TableColumn<ResponseErrorPointer>(
											PrettiedCaptions.Pointers.DocumentSectionCode,
											s => s.DocumentSectionCode?.Value
										),
										new TableColumn<ResponseErrorPointer>(
											PrettiedCaptions.Pointers.TagID,
											s => s.TagId?.Value
										)
									};

									sb.Append(ToPointTablePart(PrettiedCaptions.Manifest.ErrorPointersSection, err.Pointer, columns));
								}
							}))
				)
				.Build();
		}

		protected ZString GenerateCodeTypeAttributesTable(CodeType codeType)
		{
			var result = ZString.Empty;
			if (codeType == null)
			{
				return result;
			}

			var myCols = new List<(string header, string value)>();

			if (!string.IsNullOrEmpty(codeType.ListId))
			{
				myCols.Add((PrettiedCaptions.Manifest.ValidationListId, codeType.ListId));
			}

			if (codeType.ListAgencyIdValueSpecified)
			{
				myCols.Add((PrettiedCaptions.Manifest.ValidationListAgencyId, codeType.ListAgencyId.Value.ToString()));
			}

			if (!string.IsNullOrEmpty(codeType.ListAgencyName))
			{
				myCols.Add((PrettiedCaptions.Manifest.ValidationListAgencyName, codeType.ListAgencyName));
			}

			if (!string.IsNullOrEmpty(codeType.ListVersionId))
			{
				myCols.Add((PrettiedCaptions.Manifest.ValidationListVersionId, codeType.ListVersionId));
			}

			if (!string.IsNullOrEmpty(codeType.LanguageId))
			{
				myCols.Add((PrettiedCaptions.Manifest.ValidationLanguageId, codeType.LanguageId));
			}

			if (!string.IsNullOrEmpty(codeType.ListUri))
			{
				myCols.Add((PrettiedCaptions.Manifest.ValidationListURI, codeType.ListUri));
			}

			if (!string.IsNullOrEmpty(codeType.ListSchemeUri))
			{
				myCols.Add((PrettiedCaptions.Manifest.ValidationListSchemeUri, codeType.ListSchemeUri));
			}

			if (myCols.Count > 0)
			{
				var creator = GetHtmlTableCreator();

				creator.WriteRowWithFormatting(myCols.Select(col => new CellWithFormatting(col.header, true)).ToArray());

				creator.WriteRowWithFormatting(myCols.Select(col => new CellWithFormatting(col.value)).ToArray());

				return creator.ToHtml();
			}
			return result;
		}
	}
}
