using System;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Operations;

namespace Enterprise.DataTransfer.Native.Common.Interceptors
{
	public class InterceptedUpdateOperation : IUpdateOperation
	{
		public InterceptedUpdateOperation(IEntityContext context)
		{
			this.context = context;
		}

		public void Update(IEntitySet entitySet)
		{
			Action<IEntitySet> updateFunction = e => UpdateOperation.Update(e);
			var settings = context.InterceptorSettings;
			foreach (var setting in settings)
			{
				if (!setting.Enable)
				{
					continue;
				}

				var disableEntitySets = setting.DisableList;
				if (disableEntitySets.Any())
				{
					if (disableEntitySets.Contains(entitySet.Name))
					{
						continue;
					}
				}

				var enableEntitySets = setting.EnableList;
				if (enableEntitySets.Any())
				{
					if (!enableEntitySets.Contains(entitySet.Name))
					{
						continue;
					}
				}

				var interceptor = setting.Interceptor;
				interceptor.Function = updateFunction;
				updateFunction = interceptor.Invoke;
			}

			updateFunction(entitySet);
		}

		#region Dependency

		public IUpdateOperation UpdateOperation { get; set; }

		#endregion

		readonly IEntityContext context;
	}
}