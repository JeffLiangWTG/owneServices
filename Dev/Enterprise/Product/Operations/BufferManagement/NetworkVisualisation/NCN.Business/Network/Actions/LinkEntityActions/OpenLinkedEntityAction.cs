using System;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class OpenLinkedEntityAction : JobNetworkAction
	{
		public OpenLinkedEntityAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
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
			.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckCanUnlinkEntity(shape));

		#endregion

		#region Execution

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			Network.OpenLinkedEntity(shape);
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
						return Tuple.Create(ResString.GetMultilingualString("1b7ec146-24a9-43a3-b3f3-9e63f89eb643", "Open Linked Workflow"), ResString.GetMultilingualString("0283e613-9f5a-4e6e-98ab-5a8de00c3dcf", "Open the linked workflow in its job's form."));

					case BMNCNShapeSchema.Constants.Prefix:
						return Tuple.Create(ResString.GetMultilingualString("44da66bd-ef04-468d-bd50-9cf85456fca5", "Open Linked Diagram"), ResString.GetMultilingualString("562d23e9-99b6-4024-b3f5-26b031207932", "Open the linked diagram in its own form."));

					default:
						throw new InvalidOperationException("Invalid BNS_RelatedEntityTableCode: " + shape.BNS_RelatedEntityTableCode);
				}
			}
			else
			{
				return Tuple.Create(DefaultName, DefaultDescription);
			}
		}

		static ResourceString DefaultName => ResString.GetMultilingualString("650743B7-4548-427F-BEE4-5FF48A6CC1EC", "Open Linked Entity");

		static ResourceString DefaultDescription => ResString.GetMultilingualString("BECD35A4-5725-4E70-AF62-ABAD17E9DDB6", "Open the linked entity in its own form.");

		#endregion
	}
}
