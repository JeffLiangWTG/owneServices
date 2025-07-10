using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using WiseTech.Business.Core;
using WiseTech.Business.Taxonomy;

namespace Enterprise.BufferManagement.Service
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Stateless localized strings")]
	public class AcceptabilityBandsService : IAcceptabilityBandService
	{
		readonly ReadOnlyBusinessObjectFactory Factory = new() { NameForDebugging = nameof(AcceptabilityBandsService), RefreshEnabled = false };

		public static class BusinessMessages
		{
			static string CanonicalPrefix => $"{WiseTech.Business.Taxonomy.TaxonomyCommon.WiseTechCanonicalPrefix}.acceptability-bands";

			public static BusinessMessage AcceptabilityBandNotFound => BusinessMessage.Build(
				TaxonomicClassification.RecordNotFound,
				englishText: "Acceptability Band not found.",
				localizationKey: Guid.Parse("794F77B6-75D8-4044-A16E-B9DD94D3AD95"),
				canonicalPrefix: CanonicalPrefix,
				nameof(AcceptabilityBandNotFound)
			);

			public static BusinessMessage AcceptabilityBandNotSupported => BusinessMessage.BuildError(
				englishText: "Acceptability Bands of this type do not support workflow enumeration.",
				localizationKey: Guid.Parse("794F77B6-75D8-4044-A16E-B9DD94D3AD95"),
				canonicalPrefix: CanonicalPrefix,
				nameof(AcceptabilityBandNotSupported)
			);

			//TODO: use a more specific error message
			public static BusinessMessage AcceptabilityBandMiscellaneousError => BusinessMessage.BuildError(
				englishText: "Acceptability Band Error.",
				localizationKey: Guid.Parse("379B62A5-1209-442E-BA0F-94208F36F6A0"),
				canonicalPrefix: CanonicalPrefix,
				nameof(AcceptabilityBandMiscellaneousError)
			);

			public static BusinessMessage NoAcceptabilityBandsPresentOnBoard => BusinessMessage.BuildError(
				englishText: "This Board's configuration does not contain any Acceptability Bands.",
				localizationKey: Guid.Parse("FEA12986-74D5-47CD-9EB9-82CDD4414702"),
				canonicalPrefix: CanonicalPrefix,
				nameof(NoAcceptabilityBandsPresentOnBoard)
			);

			public static BusinessMessage ErrorLoadingBoardConfigurationInfo => BusinessMessage.BuildError(
				englishText: "There was an error loading this Board's configuration info",
				localizationKey: Guid.Parse("41A711C0-EDDC-4982-B9CC-27B9379B7C28"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ErrorLoadingBoardConfigurationInfo)
			);

			public static BusinessMessage AcceptabilityBandNotInBoardConfiguration => BusinessMessage.BuildError(
				englishText: "The Acceptability Band being queried is not configured on this Board",
				localizationKey: Guid.Parse("41A711C0-EDDC-4982-B9CC-27B9379B7C28"),
				canonicalPrefix: CanonicalPrefix,
				nameof(AcceptabilityBandNotInBoardConfiguration)
			);

			public static BusinessMessage AcceptabilityBandTimeout => BusinessMessage.Build(
				TaxonomicClassification.TimeoutError,
				englishText: "Acceptability band calculation timed out",
				localizationKey: Guid.Parse("7A463806-ECE6-4912-9E04-A05D46EBF765"),
				canonicalPrefix: CanonicalPrefix,
				nameof(AcceptabilityBandTimeout)
			);
		}

		public bool TryGetWorkflowsMatchingAcceptabilityBand(Guid boardId, Guid acceptabilityBandId, out BoardHealthWorkflowDto[] acceptabilityBandWorkflowResultDto, out BusinessResponse businessResponse)
		{
			string GetJobType(string databaseCode)
			{
				//TODO: Move JobParentType to shared? and use it here
				return databaseCode switch
				{
					"WKI" => "WorkItem",
					"IM" => "IncidentMain",
					"INC" => "IncidentRequest",
					"WKP" => "Project",
					_ => "",
				};
			}

		acceptabilityBandWorkflowResultDto = null;

			if (!TryGetConfig(boardId, acceptabilityBandId, out var config, out businessResponse))
			{
				return false;
			}

			var boardConfig = config.Value;
			var ab = boardConfig.ab;

			List<Guid> componentIds = null;
			var releaseGroupId = Guid.Empty;

			if (boardConfig.acceptabilityBandConfiguration.FilterByComponents)
			{
				componentIds = [.. boardConfig.componentIds.Select(c => c.ToGuid())];
			}

			if (boardConfig.acceptabilityBandConfiguration.FilterByReleaseGroup)
			{
				releaseGroupId = boardConfig.ReleaseGroupId;
			}

			var results = BoardAcceptabilityBandMatchingWorkflowsCalculator.GetMatchingWorkflows(Factory, ab, componentIds, releaseGroupId);

			var query = new ZQuery { AllowTableValuedParameters = true }.AddToFilter(ProcessHeaderSchema.PK, results.MatchingWorkflows);
			var workflows = Factory.Load<ProcessHeader>(query);
			var parentHeaderPks = workflows.Select(w => w.FH_FH_ParentHeader).ToHashSet();

			Factory.AddFetchHint(ProcessHeaderSchema.Instance, new ZQuery(ProcessHeaderSchema.PK, parentHeaderPks));

			acceptabilityBandWorkflowResultDto = workflows
				.OrderByDescending(wf => wf.FH_SystemLastEditTimeUtc)
				.Select(wf => new BoardHealthWorkflowDto
				{
					WorkflowId = wf.PK.ToGuid(),
					WorkflowTitle = wf.FH_CompletionStatement,
					JobId = wf.FH_ParentId.ToGuid(),
					JobCode = wf.JobHeader.FH_JobCode,
					JobType = GetJobType(wf.FH_ParentTableCode.ToString()),
					JobTitle = wf.JobHeader.FH_JobDescription,
				})
				.ToArray();

			return true;
		}

		public bool TryGetAcceptabilityBand(Guid boardId, Guid acceptabilityBandId, out AcceptabilityBandResultDto acceptabilityBandResultDto, out BusinessResponse businessResponse)
		{
			acceptabilityBandResultDto = null;

			if (!TryGetConfig(boardId, acceptabilityBandId, out var config, out businessResponse))
			{
				return false;
			}

			var boardConfig = config.Value;
			var abConfig = boardConfig.acceptabilityBandConfiguration;
			var ab = boardConfig.ab;

			if (!abConfig.OverrideBoundaryValues)
			{
				abConfig.Boundaries = new AcceptabilityBandBoundariesDto
				{
					CautionLowerBoundary = ab.BAB_CautionLowerBound,
					GoodLowerBoundary = ab.BAB_GoodLowerBound,
					ExcellentLowerBoundary = ab.BAB_ExcellentLowerBound,
					ExcellentUpperBoundary = ab.BAB_ExcellentUpperBound,
					GoodUpperBoundary = ab.BAB_GoodUpperBound,
					CautionUpperBoundary = ab.BAB_CautionUpperBound
				};
			}

			//TODO: remove the NewBoardAcceptabilityBand create a better one for the new board
			var boardBand = new NewBoardAcceptabilityBand(
					abConfig.AcceptabilityBandId,
					abConfig.Name,
					abConfig.DisplayName,
					0,
					false,
					false,
					abConfig.FilterByReleaseGroup,
					abConfig.FilterByComponents,
					abConfig.Boundaries.CautionLowerBoundary,
					abConfig.Boundaries.GoodLowerBoundary,
					abConfig.Boundaries.ExcellentLowerBoundary,
					abConfig.Boundaries.ExcellentUpperBoundary,
					abConfig.Boundaries.GoodUpperBoundary,
					abConfig.Boundaries.CautionUpperBoundary,
					int.MaxValue);

			var result = BoardAcceptabilityBandCalculator.Calculate(
				Factory,
				boardConfig.componentIds,
				boardConfig.ReleaseGroupId,
				boardBand,
				ab);

			if (result?.Result == null)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.AcceptabilityBandMiscellaneousError);
				return false;
			}

			if (result.Result.Status == CargoWise.PAVE.Common.DTO.ComponentAcceptabilityStatus.Timeout)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.AcceptabilityBandTimeout);
				return false;
			}

			acceptabilityBandResultDto = new AcceptabilityBandResultDto
			{
				AcceptabilityBandId = result.SectionBand.AcceptabilityBandPK.ToGuid(),
				DisplayName = abConfig.DisplayName,
				Result = result.Result.Value,
				DisplayUnits = abConfig.DisplayUnits,
				CalculationDurationInSeconds = Convert.ToInt32(result.Result.CalculationDuration.TotalSeconds),
				CalculatedAtUtc = result.Result.AccurateAsOfTimeUtc.ToDateTime(),
				Boundaries = new AcceptabilityBandBoundariesDto
				{
					CautionLowerBoundary = result.SectionBand.BoundaryValues.CautionMin,
					GoodLowerBoundary = result.SectionBand.BoundaryValues.GoodMin,
					ExcellentLowerBoundary = result.SectionBand.BoundaryValues.ExcellentMin,
					ExcellentUpperBoundary = result.SectionBand.BoundaryValues.ExcellentMax,
					GoodUpperBoundary = result.SectionBand.BoundaryValues.GoodMax,
					CautionUpperBoundary = result.SectionBand.BoundaryValues.CautionMax
				},
			};

			return true;
		}

		bool TryGetConfig(
			Guid boardId,
			Guid acceptabilityBandId,
			out (Guid ReleaseGroupId, List<ZGuid> componentIds, BMComponentAcceptabilityBand ab, AcceptabilityBandConfigurationDto acceptabilityBandConfiguration)? boardAbConfig,
			out BusinessResponse businessResponse)
		{
			businessResponse = null;
			boardAbConfig = null;
			var (releaseGroupPK, boardInfo) = GetBoardReleaseGrounpAndInfo(boardId) ?? (Guid.Empty, null);

			if (boardInfo == null)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorLoadingBoardConfigurationInfo);
				return false;
			}

			var data = JObject.Parse(boardInfo);
			var acceptabilityBandConfiguration = data["acceptabilityBands"]?.ToObject<AcceptabilityBandConfigurationDto[]>();

			if (acceptabilityBandConfiguration == null)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.NoAcceptabilityBandsPresentOnBoard);
				return false;
			}

			var boardConfig = data["components"]?.ToObject<BoardConfigEntity[]>();
			if (boardConfig == null)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorLoadingBoardConfigurationInfo);
				return false;
			}

			var configuration = acceptabilityBandConfiguration.FirstOrDefault(a => a.AcceptabilityBandId == acceptabilityBandId);

			if (configuration == default(AcceptabilityBandConfigurationDto))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.AcceptabilityBandNotInBoardConfiguration);
				return false;
			}

			var acceptabilityBand = Factory.LoadTop1<BMComponentAcceptabilityBand>(new ZQuery(BMComponentAcceptabilityBandSchema.PK, new ZGuid(acceptabilityBandId)));

			if (acceptabilityBand == null)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.AcceptabilityBandNotFound);
				return false;
			}

			if (!acceptabilityBand.UseFilterStripsExclusively)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.AcceptabilityBandNotSupported);
				return false;
			}

			var componentIds = boardConfig.Select(c => new ZGuid(c.Id)).ToList();

			boardAbConfig = (releaseGroupPK, componentIds, acceptabilityBand, configuration);

			return true;
		}

#pragma warning disable CW1107
		//TODO: cached it
		(Guid ReleaseGroupPK, string boardInfo)? GetBoardReleaseGrounpAndInfo(Guid boardId)
		{
			using var cmd = Db.Connection.Command(
		@"SELECT BMB_GG_ReleaseGroup, BMB_BoardInfo
FROM dbo.BMBoardConfiguration
WHERE BMB_PK = @PK");
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, boardId);
			using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow & CommandBehavior.SequentialAccess);

			if (!reader.Read())
			{
				return null;
			}

			var releaseGroupPK = reader.GetGuid(0);
			var boardInfo = reader.GetString(1);

			return (releaseGroupPK, boardInfo);
		}
#pragma warning restore CW1107
	}
}
