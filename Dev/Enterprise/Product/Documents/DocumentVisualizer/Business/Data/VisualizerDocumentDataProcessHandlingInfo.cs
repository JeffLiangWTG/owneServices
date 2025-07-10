using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.DocumentVisualizer.Business
{
	public class VisualizerDocumentDataProcessHandlingInfo : ProcessHandlingInfo
	{
		public VisualizerDocumentDataProcessHandlingInfo(VisualizerDocumentData parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly VisualizerDocumentData parent;

		protected override bool IsEventExcludedFromCascadingOrPropagation(ZString eventCode)
		{
			switch (eventCode)
			{
				case Events.DocumentDeliveredCode:
				case Events.DocumentSentCode:
				case Events.MessageSentCode:
				case Events.MessageWithdrawCancelRequestCode:
					return false;

				default:
					return base.IsEventExcludedFromCascadingOrPropagation(eventCode);
			}
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			var target = parent.Parent as IStmALogParent;
			if (target != null)
			{
				yield return new PropagationLink(target, Enumerable.Empty<BusinessObject>(), "Document Data");
			}
		}
	}
}