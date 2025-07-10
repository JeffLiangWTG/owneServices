using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class TagProvider
	{
		#region Applicable Tags

		public static IEnumerable<TagMagnitude> GetApplicableTags(this ITagable tagable, bool addFetchHints = true)
		{
			var exclusiveDefinitions = new HashSet<ZGuid>();
			var magnitudes = new HashSet<TagMagnitude>();
			var visitedTagables = new HashSet<ITagable>();

			while (tagable != null)
			{
				var parentTagable = tagable.Parent;

				if (addFetchHints && parentTagable != null)
				{
					tagable.Factory.AddFetchHint(TagLinkSchema.Instance, tagable.CreateTagLinksQuery());
					tagable.Factory.AddFetchHint(TagLinkSchema.Instance, parentTagable.CreateTagLinksQuery());
				}

				var links = tagable.TagLinks.Cast<TagLink>()
					.Where(l => l.Magnitude?.Definition != null);

				foreach (var link in links)
				{
					var magnitude = link.Magnitude;

					if (magnitude.Definition.TGD_IsExclusive)
					{
						if (exclusiveDefinitions.Add(magnitude.TGM_TGD_Tag))
						{
							magnitudes.Add(magnitude);
						}
					}

					if (!exclusiveDefinitions.Contains(magnitude.TGM_TGD_Tag))
					{
						magnitudes.Add(magnitude);
					}
				}

				visitedTagables.Add(tagable);

				if (visitedTagables.Contains(parentTagable))
				{
					break;
				}

				tagable = parentTagable;
			}

			return magnitudes;
		}

		#endregion

		#region TagCache

		public static IReadOnlyList<TagLink> GetApplicableTagLinks(this ProcessHeader header)
		{
			var cache = header.Factory.GetCachedValue<TagCache>();
			return cache.GetApplicableTags(header);
		}

		public static IReadOnlyDictionary<ZGuid, IReadOnlyList<TagLink>> PopulateTagCache(BusinessObjectFactory factory, IEnumerable<ProcessHeader> headers)
		{
			return factory.GetCachedValue<TagCache>().Populate(factory, headers);
		}

		class TagCache
		{
			readonly Dictionary<ZGuid, IReadOnlyList<TagLink>> applicableTagsByHeader = new Dictionary<ZGuid, IReadOnlyList<TagLink>>();

			internal IReadOnlyList<TagLink> GetApplicableTags(ProcessHeader header)
			{
				if (applicableTagsByHeader.TryGetValue(header.PK, out IReadOnlyList<TagLink> tags))
				{
					return tags;
				}
				else
				{
					Populate(header.Factory, new[] { header });
					return applicableTagsByHeader[header.PK];
				}
			}

			internal IReadOnlyDictionary<ZGuid, IReadOnlyList<TagLink>> Populate(BusinessObjectFactory factory, IEnumerable<ProcessHeader> headers)
			{
				var jobHeaderPKs = headers.Select(h => h.FH_FH_ParentHeader).Distinct().Where(guid => guid.IsValid).ToArray();
				var jobHeaders = factory.Load<ProcessJobHeader>(new ZQuery(ProcessHeaderSchema.PK, jobHeaderPKs)); // Pre-loading all of the ProcessJobHeader's since we know with absolute certainty they will be used soon.

				var tagLinks = factory.Load<TagLink>(new ZQuery(TagLinkSchema.TGL_ParentId, headers.Select(h => h.PK).Concat(jobHeaderPKs).ToArray()));
				var magnitudes = factory.Load<TagMagnitude>(new ZQuery(TagMagnitudeSchema.PK, tagLinks.Select(t => t.TGL_TGM_Magnitude).Distinct().ToArray()));
				var definitions = factory.Load<TagDefinition>(new ZQuery(TagDefinitionSchema.PK, magnitudes.Select(t => t.TGM_TGD_Tag).Distinct().ToArray()));

				var exclusiveDefinitions = definitions.Where(d => d.TGD_IsExclusive).Select(d => d.PK).ToHashSet();
				var exclusiveMagnitudes = magnitudes.Where(m => exclusiveDefinitions.Contains(m.TGM_TGD_Tag)).ToDictionary(d => d.PK, d => d.TGM_TGD_Tag);

				var tagLinksByParent = tagLinks.ToLookup(h => h.TGL_ParentId);
				foreach (var header in headers)
				{
					var excludedDefinitions = new HashSet<ZGuid>();
					foreach (var tagLink in tagLinksByParent[header.PK])
					{
						if (exclusiveMagnitudes.TryGetValue(tagLink.TGL_TGM_Magnitude, out var definitionPK))
						{
							excludedDefinitions.Add(definitionPK);
						}
					}
					var inheritedJobHeaderTags = tagLinksByParent[header.FH_FH_ParentHeader].Where(l => !excludedDefinitions.Contains(l.TagDefinitionPk));
					this.applicableTagsByHeader[header.PK] = tagLinksByParent[header.PK].Concat(inheritedJobHeaderTags).ToList().AsReadOnly();
				}

				foreach (var jobHeader in jobHeaders)
				{
					applicableTagsByHeader[jobHeader.PK] = tagLinksByParent[jobHeader.PK].ToList().AsReadOnly();
				}

				return new ReadOnlyDictionaryWrapper<ZGuid, IReadOnlyList<TagLink>>(() => applicableTagsByHeader);
			}
		}

		#endregion

		#region Tag Definitions

		public static TagDefinitionCache GetAllTagDefinitions(BusinessObjectFactory factory)
		{
			return new TagDefinitionCache(factory);
		}

		#endregion

		#region Visual Styles

		public static Color[] GetOrderedColors(TagDefinitionCache definitions, ImmutableHashSet<ZGuid> magnitudePKs)
		{
			return GetOrderedTagMagnitudesWithColor(definitions.AllMagnitudes, magnitudePKs)
				.Select(magnitude => magnitude.GetColor())
				.ToArray();
		}

		public static IEnumerable<TagMagnitude> GetOrderedTagMagnitudesWithColor(IEnumerable<TagMagnitude> tagMagnitudes, IEnumerable<ZGuid> magnitudePKs)
		{
			return tagMagnitudes
				.Where(magnitude => magnitudePKs.Contains(magnitude.PK))
				.Where(magnitude => magnitude.GetColor() != Color.Empty)
				.OrderByDescending(magnitude => magnitude.VisualStylePriority)
				.ThenBy(magnitude => magnitude.DisplayText);
		}

		public static Color? GetBackgroundColor(TagDefinitionCache definitions, ImmutableHashSet<ZGuid> magnitudePKs)
		{
			return GetTagMagnitudeToApplyBackgroundColor(definitions.AllMagnitudes, magnitudePKs)?.GetColor();
		}

		public static TagMagnitude GetTagMagnitudeToApplyBackgroundColor(IEnumerable<TagMagnitude> tagMagnitudes, IEnumerable<ZGuid> magnitudePKs)
		{
			return tagMagnitudes
				.Where(magnitude => magnitudePKs.Contains(magnitude.PK) && magnitude.ApplyColorToBackground)
				.OrderBy(x => x.DisplayText)
				.MaxBySafe(x => x.VisualStylePriority);
		}

		public static Color? GetBorderColor(TagDefinitionCache definitions, ImmutableHashSet<ZGuid> magnitudePKs)
		{
			var prioritisedMagnitude = definitions.AllMagnitudes
				.Where(magnitude => magnitudePKs.Contains(magnitude.PK) && magnitude.ApplyColorToBorder)
				.OrderBy(x => x.DisplayText)
				.MaxBySafe(x => x.VisualStylePriority);

			return prioritisedMagnitude != null ? prioritisedMagnitude.GetColor() : null;
		}

		public static SizedButtonBorderStyle GetBorderStyle(TagDefinitionCache definitions, ImmutableHashSet<ZGuid> magnitudePKs)
		{
			var prioritisedMagnitude = definitions.AllMagnitudes
				.Where(magnitude => magnitudePKs.Contains(magnitude.PK) && !string.IsNullOrEmpty(magnitude.BorderStyle))
				.OrderBy(x => x.DisplayText)
				.MaxBySafe(x => x.VisualStylePriority);

			return prioritisedMagnitude != null ? SizedButtonBorderStyle.FromCode(prioritisedMagnitude.BorderStyle) : null;
		}

		#endregion

		#region CCPM

		public static TagDefinition GetCCPMReleaseTagGroup(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<TagDefinition>(new ZQuery(TagDefinitionSchema.TGD_Code, BMConstants.CCPMReleaseRulesTagGroupCode));
		}

		public static TagMagnitude GetCCPMReadyToReleaseTag(TagDefinition tagGroup)
		{
			return tagGroup.Magnitudes.Single(t => t.TGM_Code == BMConstants.ReadyToReleaseTagCode);
		}

		public static TagMagnitude GetCCPMReleaseBlockedTag(TagDefinition tagGroup)
		{
			return tagGroup.Magnitudes.Single(t => t.TGM_Code == BMConstants.ReleaseBlockedTagCode);
		}

		public static ZGuid GetCCPMReadyToReleaseTagMagnitudePK()
		{
			if (CCPMReadyToReleaseTagMagnitudePK == null)
			{
				var query = new ZDBOnlyQuery(typeof(TagMagnitude));
				query.AddToFilter(TagMagnitudeSchema.TGM_Code, BMConstants.ReadyToReleaseTagCode);
				var tagDefinitionSubQuery = new ZDBOnlySubQuery(typeof(TagDefinition), TagMagnitudeSchema.TGM_TGD_Tag);
				tagDefinitionSubQuery.AddToFilter(TagDefinitionSchema.TGD_Code, BMConstants.CCPMReleaseRulesTagGroupCode);

				query.AddSubQuery(tagDefinitionSubQuery, JoinCondition.And);

				var factory = new BusinessObjectFactory { NameForDebugging = "TagProvider.GetCCPMReadyToReleaseTagMagnitudePK" };
				var magnitude = factory.LoadTop1<TagMagnitude>(query)
					?? throw new InvalidOperationException("System-defined CCPM tags were not found");

				CCPMReadyToReleaseTagMagnitudePK = magnitude.PK;
			}

			return CCPMReadyToReleaseTagMagnitudePK.Value;
		}

		[SuppressThreadStaticFieldMessage]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "We really just want to cache this for all threads. Race conditions aren't a big deal.")]
		static ZGuid? CCPMReadyToReleaseTagMagnitudePK;

#if DEBUG
		public static void ResetCCPMReadyToReleaseTagMagnitudePK_ForTest()
		{
			CCPMReadyToReleaseTagMagnitudePK = null;
		}
#endif

		#endregion

		#region Work Queues

		public static TagDefinition GetWorkQueuesTagGroup(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<TagDefinition>(new ZQuery(TagDefinitionSchema.TGD_Code, BMConstants.WorkQueuesTagGroupCode));
		}

		#endregion
	}
}
