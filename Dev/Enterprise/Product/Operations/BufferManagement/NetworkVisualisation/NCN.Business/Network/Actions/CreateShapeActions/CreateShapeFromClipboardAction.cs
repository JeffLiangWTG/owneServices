using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Network.NetworkEntity;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateShapeFromClipboardAction : CreateShapeAction
	{
		public CreateShapeFromClipboardAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		WorkflowClipboardResult[] GetClipboardResults()
		{
			// Cache the clipboard result to prevent race conditions (The clipboard is not very thread safe).
			// We're assuming the menu is reconstructed every time it opens.
			return results ?? (results = Network.GetJobHeadersFromClipboard(Network.DiagramShape.Factory).ToArray());
		}
		WorkflowClipboardResult[] results;

		protected override ResourceString GetNameCore(BMNCNShape shape)
		{
			var clipboardResults = GetClipboardResults();
			if (clipboardResults.Length > 1 || ShapeStateContainer.GetShapeCount() > 0)
			{
				var shapeCounts = clipboardResults.Any(ValidHyperlink) ? clipboardResults.Count(ValidHyperlink) : ShapeStateContainer.GetShapeCount();
				return ResString.GetMultilingualString("3f539f8c-8763-49fe-8614-8e6194e77957", "Shapes for {0} copied items", shapeCounts);
			}
			else if (clipboardResults.Length == 0)
			{
				return ResString.GetMultilingualString("d5d02394-8f30-4f72-be6c-949a398db165", "Shapes for hyperlinks in clipboard (clipboard is empty)");
			}
			else
			{
				var result = clipboardResults[0];
				if (result.IsValid)
				{
					return ResString.GetMultilingualString("f7dd24b8-2d80-4d1b-946c-ac87fd154fac", "Shape for [{0}] (from clipboard)", result.Name);
				}
				else
				{
					return ResString.GetMultilingualString("535f821c-ab9c-46ff-b804-4ebef8817ff9", "The link in the clipboard could not be identified.");
				}
			}
		}

		protected override ResourceString GetDescriptionCore(BMNCNShape shape)
		{
			return ResString.GetMultilingualString("20dd8325-8b57-4c75-848d-61aa3c3e172e", "Create a new shape for the hyperlink currently in the clipboard");
		}

		protected override INetworkActionResult CreateShape(BMNCNShape shape)
		{
			var successfullyLinkedEntities = new List<ShapeNetworkEntity>();
			var stringBuilder = new StringBuilder();

			foreach (var clipboardEntry in GetClipboardResults().Where(ValidHyperlink))
			{
				var entityToLink = clipboardEntry.EntityToLink;
				var validationResult = JobNetworkEntityRelationshipValidator.GetLinkFailureMessageForOwner(entityToLink, Network.Entities.GetInstance(shape), Network.DiagramEntity, Network.Entities.ShapeEntities);
			
				if (validationResult.IsValid)
				{
					var result = new CreateShapeAction(NetworkViewModel).ExecuteForEntityWithoutAccessCheck(shape);
					var newShape = ((IProposedNetworkEntity)result).AsShape();
					
					var linkSuccessful = Network.LinkEntity(newShape, entityToLink);
					if (!linkSuccessful)
					{
						var linkErrorMesseage = Res.GetString("dc9e62f6-43eb-4a03-841a-c28d9f9b871b", "The entity {0} could not be linked to a shape. Ensure the entity is valid and try again.", clipboardEntry.Name);
						stringBuilder.AppendLine(linkErrorMesseage);
					}

					newShape.Name = clipboardEntry.Name;

					if (newShape.BNS_RelatedEntityID.IsValid)
					{
						successfullyLinkedEntities.Add(Network.Entities.GetInstance(newShape));
					}
					else
					{
						ErrorReporter.ReportOnce("1299e874-4304-4ead-bb67-dd9262de2e15", string.Format(CultureInfo.InvariantCulture, "An entity [{0}] passed GetLinkFailureForOwner, but somehow failed to link.", clipboardEntry.Name));
					}
				}
				else
				{
					stringBuilder.AppendLine(clipboardEntry.Name);
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\t{0}{1}", validationResult.FailureReason, System.Environment.NewLine);
				}
			}

			if (stringBuilder.Length > 0)
			{
				var caption = Res.GetString("680b5e53-29b0-416d-aeaf-fc74731e36eb", "Items in the clipboard could not be added to the diagram:");
				UserInteractionImplementor.HasUserConfirmed(caption + System.Environment.NewLine + stringBuilder.ToString(), caption);
			}

			if (successfullyLinkedEntities.Count == 1)
			{
				return successfullyLinkedEntities.First();
			}

			if (successfullyLinkedEntities.Count > 0)
			{
				return new EntityCollection(successfullyLinkedEntities);
			}
			else
			{
				return null;
			}
		}

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entityToExecute)
		{
			if (AnyThingInClipBoardByHyperLink(entityToExecute.AsShape()).IsAllowed || ShapeStateContainer.GetShapeCount() == 0)
			{
				return base.ExecuteForEntityCore(entityToExecute);
			}
			else
			{
				return Network.PasteShapeFromClipBoard(NetworkViewModel);
			}
		}

		static bool ValidHyperlink(WorkflowClipboardResult result)
		{
			return result.IsValid && !result.EntityToLink.IsDeleted;
		}

		INetworkActionAccessibility AnyThingInClipBoardByHyperLink(BMNCNShape shape)
		{
			var clipboardResults = GetClipboardResults();
			return new NetworkActionAccessibility(
				clipboardResults.Any(result => result.IsValid),
				shape,
				() => Res.GetString("3CBC3068-E5D5-49EE-B348-15089014C095", "The clipboard should contain shapes."));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			var result = AnyThingInClipBoardByHyperLink(shape);

			if (result.IsAllowed || ShapeStateContainer.GetShapeCount() == 0)
			{
				return result;
			}
			else
			{
				return NetworkActionAccessibility.Allowed;
			}
		}
	}
}
