using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class InternalCustomizableControlBehaviour<TControl, TDataItem> : ControlBehaviour<TControl, TDataItem>
		where TControl : Control
		where TDataItem : BusinessObject
	{
		public InternalCustomizableControlBehaviour(Action<TControl, TDataItem> updateControlBehaviourAction)
		{
			this.updateControlBehaviourAction = Argument.NotNull(updateControlBehaviourAction, nameof(updateControlBehaviourAction));
		}

		public InternalCustomizableControlBehaviour(string behaviourName, Action<TControl, TDataItem> updateControlBehaviourAction)
			: this(updateControlBehaviourAction)
		{
			customBehaviourName = Argument.NotNullOrEmpty(behaviourName, nameof(behaviourName));
		}

		protected override void UpdateBehaviourCore(TControl control, TDataItem dataItem)
		{
			updateControlBehaviourAction(control, dataItem);
		}

		protected override string GetCustomBehaviourName() => customBehaviourName;

		readonly Action<TControl, TDataItem> updateControlBehaviourAction;
		readonly string customBehaviourName;
	}
}
