using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.Testing
{
	public delegate void PerformAction(BusinessObjectFactory factory);
	public delegate void PerformActionWithParameter<TParam>(BusinessObjectFactory factory, TParam parameter);
	public delegate TResult PerformActionWithReturnResult<TResult>(BusinessObjectFactory factory);
	public delegate TResult PerformActionWithParametersAndReturnResult<TParam, TResult>(BusinessObjectFactory factory, TParam parameter);

	public class SessionForConcurrencyTesting
	{
		BusinessObjectFactory fNonUpdatingFactory;
		BusinessObjectFactory FactoryWithDataRefreshDisabled
		{
			get
			{
				if (fNonUpdatingFactory == null)
				{
					fNonUpdatingFactory = new BusinessObjectFactory();
					fNonUpdatingFactory.RefreshEnabled = false;
				}
				return fNonUpdatingFactory;
			}
		}

		public void PerformAction(PerformAction actionDelegate)
		{
			actionDelegate(FactoryWithDataRefreshDisabled);
		}

		public void PerformAction<TParameter>(PerformActionWithParameter<TParameter> actionDelegate, TParameter parameter)
		{
			actionDelegate(FactoryWithDataRefreshDisabled, parameter);
		}

		public TResult PerformActionWithReturnValue<TResult>(PerformActionWithReturnResult<TResult> actionDelegate)
		{
			return actionDelegate(FactoryWithDataRefreshDisabled);
		}

		public TResult PerformActionWithReturnValue<TParam, TResult>(PerformActionWithParametersAndReturnResult<TParam, TResult> actionDelegate, TParam parameter)
		{
			return actionDelegate(FactoryWithDataRefreshDisabled, parameter);
		}
	}
}
