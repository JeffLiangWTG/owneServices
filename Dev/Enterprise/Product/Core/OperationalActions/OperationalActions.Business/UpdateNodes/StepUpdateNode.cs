using System.Reflection;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("Property = {Info.Name}")]
	public abstract partial class StepUpdateNode : UpdateNode
	{
		internal StepUpdateNode(PropertyInfo info)
		{
			this.info = info;
		}

		public PropertyInfo Info
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return info; }
		}

		readonly PropertyInfo info;
	}
}

#region Test
#if DEBUG

#region DisplayProxy

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(StepUpdateNode.StepDisplayProxy<StepUpdateNode>))]
	partial class StepUpdateNode
	{
		protected class StepDisplayProxy<T> : UpdateNode.DisplayProxy<T>
			where T : StepUpdateNode
		{
			public StepDisplayProxy(T parent)
				: base(parent) { }

			public PropertyInfo Info
			{
				get { return parent.Info; }
			}
		}
	}
}

#endregion

#endif
#endregion
