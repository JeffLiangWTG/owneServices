using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class LinkFromClipboardAction : JobNetworkAction
	{
		public LinkFromClipboardAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("8E46D768-28D6-4CE3-AE92-E3886A0C3B37", "Link to Entity from Clipboard");

		protected override ResourceString GetNameCore(BMNCNShape shape) => GetNameAndEnabledness(Network, shape).Item1;

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("412528ad-6eca-4ea7-a86a-5d2977934243", "Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)");

		protected override string IconName => (NoResString)"Link"; // resource name

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckShapeIsNormalDiagramOrChild(shape);

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape) => GetNameAndEnabledness(Network, shape).Item2;

		#endregion

		#region Execution

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			var jobInClipboard = GetJobHeadersFromClipboard(Network, shape.Factory).Single();
			LinkEntity(shape, Network, jobInClipboard.EntityToLink);
		}

		#endregion

		#region Implementation

		Tuple<ResourceString, NetworkActionAccessibility> GetNameAndEnabledness(IJobNetwork network, BMNCNShape shape)
		{
			var jobsInClipboard = GetJobHeadersFromClipboard(network, shape.Factory);

			if (jobsInClipboard.Length > 1)
			{
				var explanation = Res.GetString("baf9d3da-1f8c-46ea-b97d-ee8693d57017", "More than one hyperlink has been copied to clipboard. This action can only be performed with a single hyperlink.");
				return Tuple.Create(GetDefaultNameCore(), new NetworkActionAccessibility(shape, explanation));
			}
			else if (jobsInClipboard.Length == 0)
			{
				var explanation = Res.GetString("e169caa8-3096-4a9e-bafb-01e3450af41e", "There is no hyperlink in the clipboard. If a hyperlink has been copied to the clipboard then this shape can be linked to the job.");
				return Tuple.Create(GetDefaultNameCore(), new NetworkActionAccessibility(shape, explanation));
			}
			else
			{
				var jobInClipboard = jobsInClipboard.Single();

				if (jobInClipboard.IsValid)
				{
					var name = ResString.GetMultilingualString("a678148a-f0be-4f51-b3ac-545d388e9021", "Link to {0} (from clipboard)", jobInClipboard.Name);

					if (shape.IsDiagram && jobInClipboard.EntityToLink is BMNCNShape)
					{
						var explanation = Res.GetString("d1911ed7-c723-486f-99ad-8505dca84b03", "Only jobs can be linked to the diagram surface.");
						return Tuple.Create(name, new NetworkActionAccessibility(shape, explanation));
					}

					if (jobInClipboard.EntityToLink != shape.ProcessHeader)
					{
						return Tuple.Create(name, NetworkActionAccessibility.Allowed);
					}
					else
					{
						var explanation = Res.GetString("112812FF-2407-436E-A047-5E6A1A410811", "The hyperlink in the clipboard points to the same job or workflow which is already linked to this shape.");
						return Tuple.Create(name, new NetworkActionAccessibility(shape, explanation));
					}
				}
				else
				{
					var explanation = Res.GetString("14fb20a8-1141-4886-9d36-2d49b7ccdfc5", "The hyperlink in the clipboard cannot be linked to this shape.");
					return Tuple.Create(GetDefaultNameCore(), new NetworkActionAccessibility(shape, explanation));
				}
			}
		}

		static WorkflowClipboardResult[] GetJobHeadersFromClipboard(IJobNetwork network, BusinessObjectFactory factory) => network.GetJobHeadersFromClipboard(factory).ToArray();

		static void LinkEntity(BMNCNShape parent, IJobNetwork network, BusinessObject entityToLink)
		{
			network.LinkEntity(parent, entityToLink);
		}

		#endregion
	}
}
