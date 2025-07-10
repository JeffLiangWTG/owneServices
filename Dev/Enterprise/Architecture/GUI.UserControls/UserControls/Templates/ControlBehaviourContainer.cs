using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ControlBehaviourContainer<T> : IControlBehaviourContainer where T : BusinessObject
	{
		public ControlBehaviourContainer(ControlBehaviour controlBehaviour, Func<T, ZPropertyInfo>[] dependencies)
		{
			ControlBehaviour = Argument.NotNull(controlBehaviour, nameof(controlBehaviour));
			this.dependencies = dependencies;
		}

		readonly Func<T, ZPropertyInfo>[] dependencies;

		public ControlBehaviour ControlBehaviour { get; }

		public IReadOnlyCollection<ZPropertyInfo> GetDependencies(BusinessObject businessObject)
		{
			if (businessObject == null)
			{
				return Array.Empty<ZPropertyInfo>();
			}

			var dataItem = (T)businessObject;
			return dependencies?.Select(d => d(dataItem)).WhereNotNull().ToArray() ?? Array.Empty<ZPropertyInfo>();
		}

		public void UpdateControlBehaviour(Control control, BusinessObject dataItem)
		{
			ControlBehaviour.UpdateBehaviour(control, dataItem);
			currentHashCode = GetNewHashFromValues(dataItem);
		}

		public bool IsRefreshRequired(BusinessObject dataItem, Control control)
		{
			var newHashCode = GetNewHashFromValues(dataItem);
			return newHashCode != currentHashCode;
		}

		IEnumerable<IZType> GetDependentValuesFromDependencies(BusinessObject businessObject)
		{
			var behaviourDependencies = GetDependencies(businessObject);
			var dependentValues = behaviourDependencies.Select(d => d.Value).WhereNotNull().ToArray();
			return dependentValues;
		}

		int GetNewHashFromValues(BusinessObject dataItem)
		{
			IEnumerable<IZType> dependentValues = ControlBehaviour.UseControlDependentValues
				? ControlBehaviour.GetControlDependentValues(dataItem)
				: GetDependentValuesFromDependencies(dataItem);

			return string.Join("_", dependentValues).GetHashCode();
		}

		int currentHashCode;
	}
}
