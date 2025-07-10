using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Model = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Service
{
	public class BoardService : IBoardService
	{
		readonly ReadOnlyBusinessObjectFactory factory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = nameof(BoardService), RefreshEnabled = false };

		public BoardConfigurationDTO GetConfiguration(Guid boardPK)
		{
			var board = factory.Load<BMBoard>(boardPK);

			if (board == null)
			{
				return null;
			}

			var sectionsPKsToLoad = board.Sections
				.Select(s => s.PK.ToGuid());
			var sectionsFromDB = LoadSectionFromDBs(sectionsPKsToLoad);

			if (sectionsFromDB == null)
			{
				return null;
			}

			var sectionsDTO = sectionsFromDB.Select(s => SectionFactory.CreateConfiguration(s));

			ReportUsage(board.ToModel());

			return new BoardConfigurationDTO()
			{
				RefreshIntervalMinutes = BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.Value,
				Sections = sectionsDTO.ToArray()
			};
		}

		static void TriggerLicenceUsage()
		{
			using (var licensing = new VisualBoardLicensedComponent())
			{
				licensing.Login();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludePKInErrorReporterKey", Justification = "Baseline")]
		public void ReportUsage(Model.Board board)
		{
			TriggerLicenceUsage();
			try
			{
				var collector = ObjectFactory.New<IPAVEUsageCollector>();
				var eventData = new Dictionary<string, string>()
				{
					{ "PK", board.PK.ToString() },
					{ (NoResString)"Name", board.Name },
					{ "FromWEB", "YES" }
				};

				var sectionTypeCount = board.Sections
					.GroupBy(s => s.SectionType)
					.Select(group => new KeyValuePair<string, string>(group.Key, group.Count().ToString()));

				eventData.AddRange(sectionTypeCount);

				var componentTypeCount = board.Sections
					.Where(s => s.SectionType == "CMP" && s.ComponentType != null)
					.GroupBy(s => s.ComponentType)
					.Select(group => new KeyValuePair<string, string>(group.Key, group.Count().ToString()));

				eventData.AddRange(componentTypeCount);

				collector.ReportVisualBoardOpen(eventData.ToDictionary(kv => kv.Key, kv => kv.Value));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce($"Error to ReportVisualBoardOpen Board: PK:{board.PK} Name:{board.Name}", ex);
			}
		}

		IEnumerable<BMBoardSection> LoadSectionFromDBs(IEnumerable<Guid> sectionPKs)
		{
			if (sectionPKs == null)
			{
				return null;
			}

			var sections = factory.Load<BMBoardSection>(new ZQuery(BMBoardSectionSchema.PK, sectionPKs))
				.ToArray();

			if (sections == null || !sections.Any())
			{
				return null;
			}

			AddFetchHints(sections);

			var activeComponentSections = sections.Where(section => section.Component == null || section.Component.FC_IsActive);

			RefreshChannels(activeComponentSections);

			return activeComponentSections;
		}

		void AddFetchHints(IEnumerable<BMBoardSection> sections)
		{
			var sectionPKs = sections.Select(s => s.PK).Distinct();

			factory.AddFetchHint(BMBoardSectionChannelSchema.Instance, new ZQuery(BMBoardSectionChannelSchema.MSC_MS_Section, sectionPKs));

			var sectionsConfiguration = sections.Where(s => s.SectionConfiguration != null).Select(section => section.SectionConfiguration).ToArray();
			var componentPKs = sections
				.Select(section => section.MS_FC_Component)
				.Concat(sectionsConfiguration
					.SelectMany(sectionConfiguration => sectionConfiguration.AdditionalComponents.Select(c => c.BSA_FC_Component)))
				.Distinct();

			factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.PK, componentPKs));
			factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.FC_FC_ParentComponent, componentPKs));

			var staffPKs = new List<ZGuid>();

			foreach (var section in sectionsConfiguration)
			{
				staffPKs.AddRange(section.Channels
					.WhereNotNull()
					.Where(channel => channel.MSC_ChannelType == ChannelTypeList.Codes.Resource)
					.Select(channel => channel.EntityPK));
			}

			var query = new ZQuery(GlbStaffSchema.PK, staffPKs);
			factory.AddFetchHint(GlbStaffSchema.Instance, query);
		}

		void RefreshChannels(IEnumerable<BMBoardSection> sections)
		{
			foreach (var section in sections.Where(section => section.SectionConfiguration != null))
			{
				DefaultChannelsProvider.RefreshChannels(section.SectionConfiguration, factory);
			}
		}
	}
}
