using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNLevelingRuleCollection : ActiveBusinessObjectCollection<BMNCNLevelingRule>
	{
		public BMNCNLevelingRuleCollection(BMNCNShape diagramShape)
			: base(diagramShape.Factory, diagramShape, new ZQuery(), BMNCNLevelingRuleSchema.BNR_BNS_Diagram)
		{
		}

		#region Empty

		internal static BMNCNLevelingRuleCollection Empty(BusinessObjectFactory factory)
		{
			return new BMNCNLevelingRuleCollection(factory);
		}

		BMNCNLevelingRuleCollection(BusinessObjectFactory factory)
			: base(factory, ZQuery.NoResultQuery)
		{
			isEmpty = true;
			SetReadOnlyIncludingChildren(true);
		}

		protected override bool AllowNew => !isEmpty;
		readonly bool isEmpty;

		protected override void SetDefaultsForNewElementCore(BMNCNLevelingRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (isEmpty)
			{
				throw new InvalidOperationException("Cannot add a leveling rule when the parent diagram doesn't support them. Make sure this collection was created for a scaled diagram.");
			}
		}

		#endregion
	}
}
