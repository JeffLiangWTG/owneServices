using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Customs.Forwarding.Business
{
	public abstract class BaseRelatedCancellableDataSupporter : IRelatedCancellableDataSupporter
	{
		public void SetIsCancelled(IBusiness parent, bool value)
		{
			foreach (var handler in Handlers)
			{
				var objs = handler?.Invoke(parent, value);
				objs?.ForEach(c => c.IsCancelled = value);
			}
		}

		public string CanCancel(IBusiness parent)
		{
			foreach (var handler in Handlers)
			{
				var objs = handler?.Invoke(parent, true);

				if (objs != null)
				{
					foreach (var cancellable in objs)
					{
						var canCancel = cancellable.CanCancel();

						if (!string.IsNullOrEmpty(canCancel))
						{
							var businessObject = cancellable as BusinessObject;
							var humanReadableName = businessObject != null ? (string)businessObject.HumanReadableName : cancellable.ToString();

							return string.Concat(Res.GetString(
									"52DF3DA2-EC22-4426-A49B-B2A9514BAC06",
									"This record cannot be deactivated as one of its related records cannot be deactivated due to the following reason.")
								, System.Environment.NewLine
								, humanReadableName
								, ": "
								, canCancel);
						}
					}
				}
			}

			return string.Empty;
		}

		protected IEnumerable<LoadDataHandler> Handlers
		{
			get
			{
				if (handlers == null)
				{
					handlers = new List<LoadDataHandler>();
					RegisterHandlersCore(handlers);
				}

				return handlers;
			}
		}
		List<LoadDataHandler> handlers;

		public delegate ICancellable[] LoadDataHandler(IBusiness parent, bool activeOnly);

		protected abstract void RegisterHandlersCore(List<LoadDataHandler> handlerList);
	}
}
