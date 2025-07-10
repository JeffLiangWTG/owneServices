using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class RemoveTagMenuItemViewModel : ITagMenuTreeViewModel
	{
		public RemoveTagMenuItemViewModel(GetTagables getTagables, Lazy<TagDefinitionCache> definitions, BusinessObjectFactory factory)
		{
			this.getTagables = getTagables;
			this.definitions = definitions;
			this.factory = factory;

			Name = ResString.GetMultilingualString("73f43ffc-e342-461a-b230-b4469a9b1b69", "Remove Tag");
		}

		readonly Lazy<TagDefinitionCache> definitions;
		readonly GetTagables getTagables;
		readonly BusinessObjectFactory factory;

		HashSet<ZGuid> validMagnitudes;

		#region ITagMenuTreeViewModel Members

		public ResourceString Name { get; set; }

		TagActionType ITagMenuTreeViewModel.ActionType
		{
			get { return TagActionType.RemoveTag; }
		}

		IEnumerable<MenuItemDescriptor<TagDefinition>> ITagMenuTreeViewModel.GetValidDefinitionMenuItems()
		{
			UpdateValidMagnitude();

			return
				from definition in definitions.Value.AllDefinitionsRelevantToCurrentWorkflowManagementMode
				where definition.CanUserUseTags
				let magnitudes = definitions.Value.GetMagnitudes(definition)
				where magnitudes.Any(m => validMagnitudes.Contains(m.PK))
				orderby definition.DisplayText
				select new MenuItemDescriptor<TagDefinition>(definition)
				{
					Text = definition.DisplayText,
				};
		}

		IEnumerable<MenuItemDescriptor<TagMagnitude>> ITagMenuTreeViewModel.GetValidMagnitudeMenuItems(TagDefinition definition)
		{
			return
				from m in definitions.Value.GetMagnitudes(definition)
				where validMagnitudes.Contains(m.PK)
				orderby m.DisplayText
				select new MenuItemDescriptor<TagMagnitude>(m)
				{
					Text = m.DisplayText,
					ClickHandler = CreateMagnitudeEvent(m.PK),
				};
		}

		void ITagMenuTreeViewModel.ReloadAllMagnitudes()
		{
			factory.ReloadAllSafe<TagMagnitude>();
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is a viewmodel")]
		EventHandler<MenuItemClickHandlerEventArgs> CreateMagnitudeEvent(ZGuid magnitudePK)
		{
			return (o, s) =>
			{
				var factoryForRemovingTag = factory.IsOwnedByCurrentThread ? factory : new BusinessObjectFactory { NameForDebugging = "RemoveTagMagnitudeMenuItemClick" };
				var tagables = getTagables(factoryForRemovingTag, isForRemovingTag: true).ToArray();
				string failureMessage;

				if (!BatchTagOperator.TryRemoveTag(factoryForRemovingTag.Load<ITagMagnitude>(magnitudePK), tagables, out failureMessage, false))
				{
					Globals.Message.Show(failureMessage); // This is a viewmodel
				}
				else
				{
					ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(factoryForRemovingTag);
					factoryForRemovingTag.SaveHandlingZSaveExceptions();
				}
			};
		}

		void UpdateValidMagnitude()
		{
			validMagnitudes = new HashSet<ZGuid>(getTagables(factory, isForRemovingTag: true).SelectMany(t => t.TagLinks.Cast<TagLink>()).Select(t => t.TGL_TGM_Magnitude));
		}

		#endregion
	}
}
