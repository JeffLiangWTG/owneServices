using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNChannelCollection : ActiveBusinessObjectCollection<BMNCNChannel>
	{
		public BMNCNChannelCollection(BMNCNRootDiagramShape diagram)
			: base(diagram.Factory, diagram, new ZQuery(BMNCNChannelSchema.BNL_ParentTableCode, diagram.TablePrefix), BMNCNChannelSchema.BNL_ParentId)
		{
		}

		protected override void SetDefaultsForNewElementCore(BMNCNChannel newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var maxSequence = this.MaxOrDefault(x => x.BNL_Sequence);
			newElement.BNL_Sequence = maxSequence + 1;
		}

		#region Empty

		internal static BMNCNChannelCollection Empty(BusinessObjectFactory factory)
		{
			return new BMNCNChannelCollection(factory);
		}

		BMNCNChannelCollection(BusinessObjectFactory factory)
			: base(factory, ZQuery.NoResultQuery)
		{
			isEmpty = true;
			SetReadOnlyIncludingChildren(true);
		}

		protected override bool AllowNew => !isEmpty;
		readonly bool isEmpty;

		#endregion
	}
}
