using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	sealed class DocDataObjectValueSetStrategy : IValueSetStrategy
	{
		public DocDataObjectValueSetStrategy(IDictionary<string, List<Action>> valueChangedActions)
		{
			this.valueChangedActions = valueChangedActions;
		}

		readonly IDictionary<string, List<Action>> valueChangedActions;

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			if (valueThatHasChanged == null
				|| valueChangedActions == null
				|| oldValue == null)
			{
				return;
			}

			if (valueChangedActions.TryGetValue(valueThatHasChanged.Name, out var actions))
			{
				foreach (var action in actions)
				{
					action();
				}
			}
		}
	}
}
