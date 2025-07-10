using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ControlBehaviour
	{
		#region Name

		public string Name
		{
			get
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					name = CreateBehaviourName();
				}

				return name;
			}
		}

		string name;

		#endregion

		public abstract void UpdateBehaviour(Control control, object dataItem);

		public virtual bool UseControlDependentValues { get; }

		public virtual IEnumerable<IZType> GetControlDependentValues(object dataItem)
		{
			return Array.Empty<IZType>();
		}

		protected virtual string GetCustomBehaviourName() => null;

		string CreateBehaviourName()
		{
			var customBehaviourName = GetCustomBehaviourName();
			if (!string.IsNullOrWhiteSpace(customBehaviourName))
			{
				return customBehaviourName;
			}

			var type = GetType();
			return type.IsGenericType
				? type.GetGenericTypeDefinition().FullName
				: type.FullName;
		}
	}

	public abstract class ControlBehaviour<TControl, TDataItem> : ControlBehaviour
		where TDataItem : BusinessObject
		where TControl : Control
	{
		public override void UpdateBehaviour(Control control, object dataItem)
		{
			UpdateBehaviourCore((TControl)control, (TDataItem)dataItem);
		}

		protected abstract void UpdateBehaviourCore(TControl control, TDataItem dataItem);

		public sealed override IEnumerable<IZType> GetControlDependentValues(object dataItem)
		{
			var item = dataItem as TDataItem;

			return item is null
				? Array.Empty<IZType>()
				: GetControlDependentValuesCore(item);
		}

		protected virtual IEnumerable<IZType> GetControlDependentValuesCore(TDataItem dataItem)
		{
			return Array.Empty<IZType>();
		}
	}

	public abstract class ControlBehaviourWithCustomName<TControl, TDataItem> : ControlBehaviour<TControl, TDataItem>
		where TDataItem : BusinessObject
		where TControl : Control
	{
		protected sealed override string GetCustomBehaviourName() => GetCustomBehaviourNameCore();

		protected abstract string GetCustomBehaviourNameCore();
	}
}
