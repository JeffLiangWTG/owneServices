using System;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class AcceptBufferAction : JobNetworkAction
	{
		public AcceptBufferAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Overrides

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return new NetworkActionAccessibility(entity.IsBufferShape,
					shape,
					() => Res.GetString("0E14A08F-B6D4-4290-8826-BC6BF042A83A", "The shape should be a buffer."));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(!shape.Active,
					shape,
					() => Res.GetString("05C11237-5AAF-456E-8310-1C19AA308B58", "The buffer should not be accepted yet."));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			if (!shape.IsBufferShape)
			{
				throw new ArgumentException("shape must be a buffer", nameof(shape));
			}

			BufferCreator.SetIsBufferedFlag(Network);

			shape.Active = ZBool.True;
			Network.Refresh(RefreshType.RedrawDiagram);
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("e8beea37-98a8-4b2b-8fe8-06ca1678d15e", "Accept buffer");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("c3a47c50-9262-4e4c-8829-5e1659c57bc9", "Makes this buffer active in the diagram");
		}

		protected override string IconName => (NoResString)"Approve"; // resource name

		#endregion
	}
}
