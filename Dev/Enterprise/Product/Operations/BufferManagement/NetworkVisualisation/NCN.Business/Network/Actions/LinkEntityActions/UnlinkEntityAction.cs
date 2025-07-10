using System;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class UnlinkEntityAction : JobNetworkAction
	{
		public UnlinkEntityAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => DefaultName;

		protected override ResourceString GetNameCore(BMNCNShape shape) => GetNameAndDescription(shape).Item1;

		protected override ResourceString GetDefaultDescriptionCore() => DefaultDescription;

		protected override ResourceString GetDescriptionCore(BMNCNShape shape) => GetNameAndDescription(shape).Item2;

		protected override string IconName => (NoResString)"Link"; // resource name

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckShapeIsNormalDiagramOrChild(shape)
			.UnionIfAllowed(() => NetworkViewModel.SelectedEntities.Count() <= 1 ? BMNetworkActionAccessibilityHelper.CheckCanUnlinkEntity(shape) : null);

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckCanUnlinkEntity(shape, needsNotification: false);
		}

		#endregion

		#region Execution

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			Network.UnlinkEntity(shape);
		}

		#endregion

		#region Implementation

		static Tuple<ResourceString, ResourceString> GetNameAndDescription(BMNCNShape shape)
		{
			if (shape.LinkedEntity != null)
			{
				switch (shape.BNS_RelatedEntityTableCode)
				{
					case ProcessHeaderSchema.Constants.Prefix:
						return Tuple.Create(ResString.GetMultilingualString("26b9290d-e582-4a34-8a06-01bcf35382f8", "Un-link this Workflow"), ResString.GetMultilingualString("9017a360-b5b2-4368-8c02-f77399d057ae", "Un-links the shape from its currently-linked workflow."));

					case BMNCNShapeSchema.Constants.Prefix:
						return Tuple.Create(ResString.GetMultilingualString("70b4a7d6-86ab-42ff-b16e-5bfadf992117", "Un-link this Diagram"), ResString.GetMultilingualString("8110c3fa-4bc5-459c-810c-984ef377f6b5", "Un-links the shape from its currently-linked diagram."));

					default:
						throw new InvalidOperationException("Invalid BNS_RelatedEntityTableCode: " + shape.BNS_RelatedEntityTableCode);
				}
			}
			else
			{
				return Tuple.Create(DefaultName, DefaultDescription);
			}
		}

		static ResourceString DefaultName => ResString.GetMultilingualString("0BA00434-9A01-442C-A952-678E9F8030E9", "Un-link Entity");

		static ResourceString DefaultDescription => ResString.GetMultilingualString("6C7C5011-588B-4AD7-AF3D-B46250B86F67", "Un-links the shape from its currently-linked entity.");

		#endregion
	}
}
