using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ControlVisibilityBehaviour<TData> : ControlBehaviour<Control, TData>, IControlVisibilityBehaviour
		where TData : BusinessObject
	{
		public ControlVisibilityBehaviour(Func<TData, bool> isVisible)
		{
			this.isVisible = Argument.NotNull(isVisible, nameof(isVisible));
		}

		readonly Func<TData, bool> isVisible;

		protected override void UpdateBehaviourCore(Control control, TData dataItem)
		{
			control.Visible = IsVisible(dataItem);
		}

		public override bool UseControlDependentValues => true;

		protected override IEnumerable<IZType> GetControlDependentValuesCore(TData dataItem)
		{
			yield return (ZBool)IsVisible(dataItem);
		}

		public bool IsVisible(BusinessObject dataItem)
		{
			return dataItem != null && isVisible((TData)dataItem);
		}
	}
}
