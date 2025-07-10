using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public static class ProcessHeaderLinkViewModelHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "We are in a view model showing a message back to the user")]
		public static void RemoveLinks(IReadOnlyCollection<ProcessHeaderLink> selectedLinks, ProcessHeaderLinkCollection collection)
		{
			if (selectedLinks.Count > 0)
			{
				var linkMessage = selectedLinks.Count > 1 ? Res.GetString("f6229f57-07db-4d79-bbc9-aaa5b864107c", "Some of the selected links have") : Res.GetString("978833f4-54e9-4f31-878e-ae996ff78e67", "The selected link has");
				var linksWithApprovedArrows = selectedLinks.Select(l => Tuple.Create(l, l.ApprovedArrow)).ToArray();

				if (linksWithApprovedArrows.Any(l => l.Item2 != null && l.Item2.IsBuffered))
				{
					var message = Res.GetString("1608f959-cb45-4b6e-aff3-ea112c3457d3", "{0} an approved arrow on a project plan and has an attached buffer. Please un-approve this diagram and remove this buffer prior to removing this link", linkMessage);
					Globals.Message.Show(message, Res.GetString("936df4b5-6b51-4d37-b2f2-a9cc5d05631b", "Cannot Remove"), ZMessageBoxButtons.OK, ZMessageBoxIcon.Exclamation, ZDialogResult.OK); // We are in a view model showing a message back to the user
				}
				else
				{
					var message = linksWithApprovedArrows.Any(l => l.Item2 != null)
						? Res.GetString("8a0d27c5-a607-431c-8f04-1fe03a317c88", @"{0} an approved arrow on a project plan. The arrow will be decoupled. 
Decoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", linkMessage)
						: Res.GetString("ac63ea6e-df24-4269-bf3d-3a18ae635d89", "Are you sure you want to remove the selected links?");

					var dialogResult = Globals.Message.Show(message, Res.GetString("b00cda86-823e-4513-ad8d-59fa481f600d", "Confirm remove..."), ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Warning); // We are in a view model showing a message back to the user
					if (dialogResult == ZDialogResult.OK)
					{
						RemoveAndDecoupleLinks(linksWithApprovedArrows, collection);
					}
				}
			}
		}

		static void RemoveAndDecoupleLinks(Tuple<ProcessHeaderLink, IBMNCNAttachment>[] linksWithApprovedArrows, ProcessHeaderLinkCollection collection)
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(collection.Factory))
			{
				foreach (var linkAndArrow in linksWithApprovedArrows)
				{
					var approvedArrow = linkAndArrow.Item2;
					if (approvedArrow != null)
					{
						approvedArrow.Decouple();
					}
					if (!linkAndArrow.Item1.IsDeleted)
					{
						collection.Delete(linkAndArrow.Item1);
					}
				}

				foreach (var existingLink in collection)
				{
					existingLink.Validation.ValidateAll();
				}
			}
		}
	}
}
