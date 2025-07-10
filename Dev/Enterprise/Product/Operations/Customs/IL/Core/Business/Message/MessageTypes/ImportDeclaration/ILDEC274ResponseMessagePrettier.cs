using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.NS3;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IL.Business
{
	public class ILDEC274ResponseMessagePrettier : ILEDIMessagePrettierBase<DfNg2754Msg10004ImportDeclarationResponse>
	{
		public ILDEC274ResponseMessagePrettier(MessageDataObject<DfNg2754Msg10004ImportDeclarationResponse> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData.Response,
				b => b
					.WithResponseSection(
						(p, sb) =>
						{
							var dictionary = new Dictionary<ZString, ZString>()
							{
								{ PrettiedCaptions.Common.Status, GetStatus(p?.Status?.NameCode?.Value) },
								{ PrettiedCaptions.Declaration.StatusDate, p?.Status?.EffectiveDateTime },
								{ PrettiedCaptions.Declaration.AcceptanceDate, p?.Declaration?.AcceptanceDateTime },
								{ PrettiedCaptions.Declaration.DeclarationNumber, p?.Declaration?.Id?.Value },
								{ PrettiedCaptions.Declaration.Version, DeclarationVersionIDConverter.ToEntryHeaderVersionID(p?.Declaration?.DmExtensions?.VersionId?.Value).ToString() },
								{ PrettiedCaptions.Declaration.ExternalId, p?.Declaration?.DmExtensions?.ExternalDeclarationId?.Value },
							};
							sb.Append(ToBaseInformationPart(dictionary));
						})
					.WithSectionOf<ResponseError>(
						p => p?.Error?.Count(s => s.ValidationCode?.ListVersionId == "1") > 0,
						PrettiedCaptions.Errors.ErrorsSection,
						p => () => p.Error?.Where(s => s.ValidationCode?.ListVersionId == "1"),
						b1 => b1.WithSection(
							(error, sb) =>
							{
								var dictionary = new Dictionary<ZString, ZString>()
								{
									{ PrettiedCaptions.Errors.ValidationCode, error.ValidationCode?.Value },
									{ PrettiedCaptions.Errors.Description, error.ValidationCode?.Name },
								};
								sb.Append(ToBaseInformationPart(dictionary));

								if (error.Pointer != null && error.Pointer.Count > 0)
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
										),
										new TableColumn<ResponseErrorPointer>(
											PrettiedCaptions.Pointers.NaturalKey,
											s => s.DmExtensions?.NaturalKey?.Value
										)
									};

									sb.Append(ToPointTablePart(PrettiedCaptions.Pointers.PointersSection, error.Pointer, columns));
								}
							})
						)
					.WithSectionOf<ResponseError>(
						p => p?.Error?.Count(s => s.ValidationCode?.ListVersionId == "2" || s.ValidationCode?.ListVersionId == "3") > 0,
						PrettiedCaptions.Constraints.ConstraintsSection,
						p => () => p.Error?.Where(s => s.ValidationCode?.ListVersionId == "2" || s.ValidationCode?.ListVersionId == "3"),
						b1 => b1.WithSection(
							(error, sb) =>
							{
								var dictionary = new Dictionary<ZString, ZString>()
								{
									{ PrettiedCaptions.Constraints.ValidationCode, error.ValidationCode?.Value },
									{ PrettiedCaptions.Constraints.Description, error.ValidationCode?.Name },
									{ PrettiedCaptions.Constraints.ConstraintId, error.DmExtensions?.ConstraintId.ToString() },
									{ PrettiedCaptions.Constraints.ConstraintType, error.DmExtensions?.ConstraintType.ToString() },
									{ PrettiedCaptions.Constraints.ConstraintStatus, error.DmExtensions?.ConstraintStatus.ToString() },
								};
								sb.Append(ToBaseInformationPart(dictionary));

								if (error.Pointer != null && error.Pointer.Count > 0)
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
										),
									};

									sb.Append(ToPointTablePart(PrettiedCaptions.Pointers.PointersSection, error.Pointer, columns));
								}
							})
						)
					.WithSectionOf<ResponseError>(
						p => p?.Error?.Count(s => s.ValidationCode?.ListVersionId == "4" || s.ValidationCode?.ListVersionId == "5") > 0,
						PrettiedCaptions.Warnings.WarningsSection,
						p => () => p.Error?.Where(s => s.ValidationCode?.ListVersionId == "4" || s.ValidationCode?.ListVersionId == "5"),
						b1 => b1.WithSection(
							(error, sb) =>
							{
								var dictionary = new Dictionary<ZString, ZString>()
								{
									{ PrettiedCaptions.Errors.ValidationCode, error.ValidationCode?.Value },
									{ PrettiedCaptions.Errors.Description, error.ValidationCode?.Name },
								};
								sb.Append(ToBaseInformationPart(dictionary));

								if (error.Pointer != null && error.Pointer.Count > 0)
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
										),
										new TableColumn<ResponseErrorPointer>(
											PrettiedCaptions.Pointers.NaturalKey,
											s => s.DmExtensions?.NaturalKey?.Value
										)
									};

									sb.Append(ToPointTablePart(PrettiedCaptions.Pointers.PointersSection, error.Pointer, columns));
								}
							})
						)
			)
			.Build();

		string GetStatus(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return string.Empty;
			}

			var customsStatuses = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus);

			if (customsStatuses.ContainsCode(code))
			{
				return $"{code} - {customsStatuses.GetDescriptionFromCode(code)}";
			}

			return $"{code} - UNKNOWN";
		}
	}
}
