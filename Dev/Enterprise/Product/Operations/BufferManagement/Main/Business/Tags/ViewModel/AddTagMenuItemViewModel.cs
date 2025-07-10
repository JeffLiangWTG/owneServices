using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	public class AddTagMenuItemViewModel : ITagMenuTreeViewModel
	{
		public AddTagMenuItemViewModel(GetTagables getTagables, Type tagableType, Lazy<TagDefinitionCache> definitions, BusinessObjectFactory factory)
		{
			if (!typeof(ITagable).IsAssignableFrom(tagableType))
			{
				throw new ArgumentException("Must implement ITagable", nameof(tagableType));
			}

			this.getTagables = getTagables;
			this.definitions = definitions;
			this.factory = factory;
			this.tagableType = tagableType;

			Name = ResString.GetMultilingualString("8c83190d-6039-40bb-84a6-a06d2df25fa3", "Add Tag");
		}

		readonly Lazy<TagDefinitionCache> definitions;
		readonly GetTagables getTagables;
		readonly BusinessObjectFactory factory;
		readonly Type tagableType;

		#region ITagMenuTreeViewModel Members

		public ResourceString Name { get; set; }

		TagActionType ITagMenuTreeViewModel.ActionType
		{
			get { return TagActionType.AddTag; }
		}

		IEnumerable<MenuItemDescriptor<TagDefinition>> ITagMenuTreeViewModel.GetValidDefinitionMenuItems()
		{
			return
				from d in definitions.Value.AllDefinitionsRelevantToCurrentWorkflowManagementMode
				where d.CanUserUseTags
				where d.ValidForScope(tagableType)
				orderby d.DisplayText
				select new MenuItemDescriptor<TagDefinition>(d)
				{
					Text = d.DisplayText,
				};
		}

		IEnumerable<MenuItemDescriptor<TagMagnitude>> ITagMenuTreeViewModel.GetValidMagnitudeMenuItems(TagDefinition definition)
		{
			var list = definitions.Value.GetMagnitudes(definition);

			foreach (var magnitude in list.OrderBy(m => m.DisplayText).ToArray())
			{
				if (magnitude.IsDeleted || !magnitude.IsInDatabase)
				{
					magnitude.Delete();
					definitions.Value.RemoveMagnitude(definition, magnitude);
				}
				else
				{
					if (magnitude.TGM_IsActive)
					{
						yield return new MenuItemDescriptor<TagMagnitude>(magnitude)
						{
							Text = magnitude.DisplayText,
							ClickHandler = CreateMagnitudeEvent(magnitude.PK, definition.PK, definition.TGD_IsExclusive),
						};
					}
				}
			}

			if (definition.TGD_Code == BMConstants.WorkQueuesTagGroupCode)
			{
				yield return new MenuItemDescriptor<TagMagnitude>(null) { Text = "-" };

				var newWorkQueueMenuItem = new MenuItemDescriptor<TagMagnitude>(null)
				{
					Text = Res.GetString("4215bc10-3ffc-49bd-ae45-bba315090826", "New..."),
					ControllerID = ControllerIDs.WorkQueues,
					ShowFormForPayloadAfterExecute = true,
				};

				newWorkQueueMenuItem.ClickHandler = CreateNewWorkQueueEvent(newWorkQueueMenuItem);

				yield return newWorkQueueMenuItem;
			}
		}

		void ITagMenuTreeViewModel.ReloadAllMagnitudes()
		{
			factory.ReloadAllSafe<TagMagnitude>();
		}

		#endregion

		#region Implementation

		EventHandler<MenuItemClickHandlerEventArgs> CreateMagnitudeEvent(ZGuid magnitudePK, ZGuid definitionPK, bool isExclusive)
		{
			return (s, e) =>
			{
				OnTagMagnitudeClick(magnitudePK, definitionPK, isExclusive);
			};
		}

		EventHandler<MenuItemClickHandlerEventArgs> CreateNewWorkQueueEvent(MenuItemDescriptor<TagMagnitude> newWorkQueueMenuItem)
		{
			return (s, e) =>
			{
				var queue = factory.New<WorkQueue>();
				newWorkQueueMenuItem.Payload = queue;

				definitions.Value.AddMagnitude(queue.Definition, queue);

				OnTagMagnitudeClick(queue.PK, queue.TGM_TGD_Tag, queue.Definition.TGD_IsExclusive, shouldSave: false);
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is a viewmodel")]
		void OnTagMagnitudeClick(ZGuid magnitudePK, ZGuid definitionPK, bool isExclusive, bool shouldSave = true)
		{
			var factoryForLoading = factory.IsOwnedByCurrentThread ? factory : new BusinessObjectFactory { NameForDebugging = "AddTagMagnitudeMenuItemClick" };
			var tagables = getTagables(factoryForLoading, isForRemovingTag: false).ToArray();
			var isExclusiveTagConditionsSatisfied = !isExclusive;
			var tagsToDelete = Array.Empty<TagLink>();

			if (!isExclusiveTagConditionsSatisfied)
			{
				tagsToDelete = tagables.SelectMany(t => t.TagLinks.Cast<TagLink>()).Where(link => link.TGL_TGM_Magnitude != magnitudePK && link.TagDefinitionPk == definitionPK).ToArray();
				isExclusiveTagConditionsSatisfied = !tagsToDelete.Any();
			}

			if (!isExclusiveTagConditionsSatisfied)
			{
				var message = GetExclusiveTagViolationMessage(tagsToDelete);
				var caption = Res.GetString("bdf3b5a0-e249-4b0f-b423-81c5ba5af778", "Replace Existing Tag");

				isExclusiveTagConditionsSatisfied = Globals.Message.Show(message, caption, ZMessageBoxButtons.OKCancel, ZDialogResult.OK) == ZDialogResult.OK; // This is a menu item viewmodel
			}

			if (isExclusiveTagConditionsSatisfied)
			{
				var tagMagnitudesToDelete = (tagsToDelete.Select(t => factoryForLoading.Load<TagMagnitude>(t.Magnitude.PK)).Distinct().ToArray());
				foreach (var tag in tagMagnitudesToDelete)
				{
					if (!TagSecurity.CheckTagRemoveSecurity(tag))
					{
						return;
					}
				}

				foreach (var tag in tagMagnitudesToDelete)
				{
					BatchTagOperator.TryRemoveTag(tag, tagsToDelete.Where(t => t.Magnitude.PK == tag.PK).Select(t => t.Parent), showSecurityDialog: false);
				}

				var magnitude = factoryForLoading.Load<TagMagnitude>(magnitudePK);
				var items = tagables.Where(t => t.TagLinks.Cast<TagLink>().All(l => l.TGL_TGM_Magnitude != magnitudePK)).ToArray();

				string addTagFailureMessage;
				if (!BatchTagOperator.TryAddTag(magnitude, items, out addTagFailureMessage, false))
				{
					Globals.Message.Show(addTagFailureMessage); // This is a viewmodel
				}

				if (shouldSave)
				{
					ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(factoryForLoading);
					factoryForLoading.SaveHandlingZSaveExceptions();
				}
			}
		}

		static string GetExclusiveTagViolationMessage(IEnumerable<TagLink> tagsToDelete)
		{
			var sb = new StringBuilder();
			sb.AppendLine(Res.GetString("a47d58f1-1eb4-4dc4-a68c-1a686e51937d", "The following items already have a tag from the tag group '{0}' applied:", tagsToDelete.First().Definition.DisplayText));

			foreach (var link in tagsToDelete)
			{
				sb.AppendLine(Res.GetString("384F7C51-6CA6-4AC0-A2AC-8CD378AE06F0", "'{0}' already has the tag '{1}'", link.Parent.Description, link.Magnitude.DisplayText));
			}

			sb.Append(Res.GetString("78E21E47-05C6-462B-BFAC-0512E037C14E", "Replace existing tags?"));

			return sb.ToString();
		}

		#endregion
	}
}
