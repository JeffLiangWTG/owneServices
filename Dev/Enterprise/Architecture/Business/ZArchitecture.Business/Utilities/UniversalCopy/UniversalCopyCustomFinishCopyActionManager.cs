using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;

namespace Enterprise.ZArchitecture.Business.Utilities.UniversalCopy
{
	public class UniversalCopyCustomFinishCopyActionManager
	{
		readonly List<IUniversalCopyCustomFinishCopyAction> FinishCopyActions;

		public UniversalCopyCustomFinishCopyActionManager()
		{
			FinishCopyActions = new List<IUniversalCopyCustomFinishCopyAction>();
			foreach (var finishCopyAction in
				ObjectFactory.Get<IEnumerable>("UniversalCopyCustomFinishCopyActionsList"))
			{
				FinishCopyActions.Add((IUniversalCopyCustomFinishCopyAction)finishCopyAction);
			}

			FinishCopyActions = FinishCopyActions.OrderBy(action => action.Priority).ToList();
		}

		public void PerformUniversalCopyCustomFinishCopyActions(Dictionary<object, object> copiedEntities)
		{
			foreach (var finishCopyAction in FinishCopyActions)
			{
				finishCopyAction.FinishCopyAction(copiedEntities);
			}
		}
	}
}
