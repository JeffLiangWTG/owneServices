using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Service
{
	public class SectionService : ISectionService
	{
		readonly ReadOnlyBusinessObjectFactory Factory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = nameof(SectionService), RefreshEnabled = false };

		public ISection Get(Guid sectionPK)
		{
			var section = LoadSection(sectionPK);

			if (section == null || !section.Component.FC_IsActive)
			{
				return null;
			}

			return SectionFactory.Create(section);
		}

		public IEnumerable<AcceptabilityBandResultDTO> GetTimeRecording(Guid sectionPK)
		{
			var releaseGroupPK = ZGuid.Empty;

			if (!IsNewBoard(sectionPK))
			{
				var section = LoadSection(sectionPK, refreshChannels: false);

				if (section == null)
				{
					return null;
				}

				releaseGroupPK = section.SectionConfiguration.ApplicableReleaseGroupPK;
			}
			else
			{
				releaseGroupPK = GetReleaseGroupForNewBoard(sectionPK);
			}

			var results = BoardAcceptabilityBandCalculator.Calculate(Factory, sectionPK, releaseGroupPK, BMSRegistry.Instance.AcceptabilityBandForTimeRecording.Value);
			var dtos = results.Select(BoardSectionAcceptabilityBandResultToDTO);
			return dtos;
		}

		public static AcceptabilityBandResultDTO BoardSectionAcceptabilityBandResultToDTO(BoardSectionAcceptabilityBandResult bandResult) => new AcceptabilityBandResultDTO()
		{
			PK = bandResult.Result.BandPK.ToGuid(),
			Label = bandResult.Result.AggregatedLabel,
			Value = bandResult.Result.Value,
			CalculationDurationInSeconds = Convert.ToInt32(bandResult.Result.CalculationDuration.TotalSeconds),
			LastCalculatedAt = bandResult.Result.AccurateAsOfTimeUtc.ToDateTime(),
		};

#pragma warning disable CW1107
		bool IsNewBoard(Guid sectionPK)
		{
			using (var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.BMBoardConfiguration WHERE BMB_PK = @PK"))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, sectionPK);
				var value = cmd.ExecuteScalar();
				return value != DBNull.Value && (int)value == 1;
			}
		}

		Guid GetReleaseGroupForNewBoard(Guid boardPK)
		{
			using (var cmd = Db.Connection.Command("SELECT BMB_GG_ReleaseGroup FROM dbo.BMBoardConfiguration WHERE BMB_PK = @PK"))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, boardPK);
				var value = cmd.ExecuteScalar();
				return value != DBNull.Value ? (Guid)value : Guid.Empty;
			}
		}

#pragma warning restore CW1107

		BMBoardSection LoadSection(Guid sectionPK, bool refreshChannels = true)
		{
			var section = Factory.Load<BMBoardSection>(sectionPK);

			if (section == null)
			{
				return null;
			}

			AddFetchHints(section);

			if (refreshChannels)
			{
				DefaultChannelsProvider.RefreshChannels(section.SectionConfiguration, Factory);
			}

			return section;
		}

		void AddFetchHints(BMBoardSection section)
		{
			var componentPKs = section.AllShownComponentPKs;

			Factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.PK, componentPKs));
			Factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.FC_FC_ParentComponent, componentPKs));
		}

		public IEnumerable<FilterStripDTO> GetFilterStrips(Guid sectionPK)
		{
			var section = Factory.Load<BMBoardSection>(sectionPK);
			section.SectionConfiguration.SetContext(BufferManagementBusinessContext.IgnoreDefaultFilters);
			var workflowFilter = section.WorkflowSectionFilter;
			var tasksFilter = section.TaskSectionFilter;

			var parameterFactory = new ParameterNameFactory(); // Using a shared parameter name factory to guarantee we don't get collisions.
			var taskDataQuery = ((IFilterPart)tasksFilter).ParameterisedSql(parameterFactory);
			var workflowDataQuery = ((IFilterPart)workflowFilter).ParameterisedSql(parameterFactory);

			return new List<FilterStripDTO>
			{
				GetFilterStripDTOFromQuery(FilterType.Tasks, taskDataQuery),
				GetFilterStripDTOFromQuery(FilterType.Workflows, workflowDataQuery)
			};
		}

		static FilterStripDTO GetFilterStripDTOFromQuery(FilterType type, ZNonPersistentDataQuery taskDataQuery)
		{
			return new FilterStripDTO
			{
				Type = type,
				Filter = taskDataQuery.ParameterisedQueryText,
				Parameters = taskDataQuery.Parameters.Select(p => new CargoWise.PAVE.Common.DTO.SqlParameter { Name = p.ParameterName, Type = p.SchemaColumn.ColumnType.ToString(), Value = p.ParameterValueTextSql }).ToList()
			};
		}
	}
}
