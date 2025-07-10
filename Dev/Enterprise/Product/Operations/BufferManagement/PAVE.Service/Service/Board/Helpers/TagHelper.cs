using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Models = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Service
{
	internal static class TagHelper
	{
		#region TaskToTags

		internal static Dictionary<Guid, IEnumerable<Models.Tag>> GetTagsByTasks(this IEnumerable<Models.Task> tasks, ITagRepository tagRepository)
		{
			var tagsPerTaskPk = tasks.ToDictionary(task => task.PK, task => tagRepository.GetByTaskPK(task.PK));

#if NETFRAMEWORK
			var allTags = tagsPerTaskPk.Values.SelectMany(t => t).DistinctBy(t => t.PK)
				.WhereNotNull();
#else
			var allTags = IEnumerableExtensions.DistinctBy(tagsPerTaskPk.Values.SelectMany(t => t), t => t.PK)
				.WhereNotNull();
#endif

			foreach (var tag in allTags)
			{
				var taskPKs = tagsPerTaskPk.Where(kv => kv.Value.Contains(tag))
				.Select(kv => kv.Key)
				.Distinct()
				.ToList();

				tag.TaskPKs = taskPKs;
			}

			return tagsPerTaskPk;
		}

		#endregion

		#region Model To DTO

		internal static IEnumerable<TagDTO> ToTagDTOs(this Dictionary<Guid, IEnumerable<Models.Tag>> tagsPerTaskPks)
		{
#if NETFRAMEWORK
			return tagsPerTaskPks.SelectMany(t => t.Value).DistinctBy(tag => tag.PK)
#else
			return IEnumerableExtensions.DistinctBy(tagsPerTaskPks.SelectMany(t => t.Value), tag => tag.PK)
#endif
				.WhereNotNull()
				.Where(tag => !string.IsNullOrEmpty(tag.Color))
				.Select(tag => ToTagDTO(tag))
				.ToArray();
		}

		#region ToTagDTO

		static TagDTO ToTagDTO(Models.Tag tag)
		{
			return new TagDTO()
			{
				PK = tag.PK,
				Code = tag.Code,
				Description = tag.Description,
				Color = tag.Color,
				ColorApplicationStyle = GetTagColorApplicationStyle(tag),
				BorderStyle = GetBorderStyle(tag),
				BorderSize = GetBorderSize(tag),
			};
		}

		static TagBorderStyle? GetBorderStyle(Models.Tag tag)
		{
			if (tag.ApplyColorToBackground || !tag.ApplyColorToBorder || tag.BorderStyle == ZString.Empty)
			{
				return null;
			}

			return SizedButtonBorderStyle.FromCodeToTagBorderStyle(tag.BorderStyle);
		}

		static TagBorderSize? GetBorderSize(Models.Tag tag)
		{
			if (tag.ApplyColorToBackground || !tag.ApplyColorToBorder || tag.BorderStyle == ZString.Empty)
			{
				return null;
			}

			var borderStyle = SizedButtonBorderStyle.FromCode(tag.BorderStyle);

			return borderStyle.IsLarge ? TagBorderSize.Large : TagBorderSize.Small;
		}

		static TagColorApplicationStyle GetTagColorApplicationStyle(Models.Tag tag)
		{
			if (tag.ApplyColorToBackground)
			{
				return TagColorApplicationStyle.ApplyToBackground;
			}

			if (tag.ApplyColorToBorder && tag.BorderStyle != ZString.Empty)
			{
				return TagColorApplicationStyle.ApplyToBorder;
			}

			return TagColorApplicationStyle.None;
		}

		#endregion

		#endregion

		#region Order

		internal static IEnumerable<Guid> GetOrderedTagPKs(this Dictionary<Guid, IEnumerable<Models.Tag>> tagsPerTaskPKs, Guid taskPK)
		{
			if (!tagsPerTaskPKs.ContainsKey(taskPK))
			{
				return Enumerable.Empty<Guid>();
			}

			var tags = tagsPerTaskPKs[taskPK];
			var orderedTags = tags.Where(t => t.Color != null).OrderByDescending(t => t.VisualStylePriority).ThenBy(t => t.DisplayText).ToList();

			if (orderedTags.Count(t => t.ApplyColorToBackground) > 1)
			{
				//Swap tags: if the first tag color is not applied to the background, WAVE Client need the first tag to be the one applied to background if any
				var tagsToApplyBackgroundColor = orderedTags.Where(t => t.ApplyColorToBackground).OrderBy(t => t.DisplayText).MaxBySafe(t => t.VisualStylePriority);
				var firstTag = orderedTags.First(tagMagnitude => tagMagnitude.ApplyColorToBackground);

				if (firstTag != tagsToApplyBackgroundColor)
				{
					var firstTagMagnitudeIndex = orderedTags.IndexOf(firstTag);
					var tagMagnitudeToApplyBackgroundColorIndex = orderedTags.IndexOf(tagsToApplyBackgroundColor);

					orderedTags[firstTagMagnitudeIndex] = tagsToApplyBackgroundColor;
					orderedTags[tagMagnitudeToApplyBackgroundColorIndex] = firstTag;
				}
			}

			return orderedTags
					.Select(tag => tag.PK)
					.ToArray();
		}

		#endregion

		#region BusinessObject to Model

		internal static Dictionary<Guid, IEnumerable<Models.Tag>> ToTagsByTaskPK(this IEnumerable<ProcessTask> tasks)
		{
			var tags = tasks.ToDictionary(task => task.PK.ToGuid(), task =>
				task.GetApplicableTags().Select(t => t.ToModel()));

			return tags;
		}

		#endregion
	}
}
