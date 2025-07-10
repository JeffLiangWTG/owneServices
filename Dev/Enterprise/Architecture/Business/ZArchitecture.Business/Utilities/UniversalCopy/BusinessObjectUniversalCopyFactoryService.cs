using System;
using System.Collections.Generic;

using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	public sealed class BusinessObjectUniversalCopyFactoryService : IService
	{
		internal BusinessObjectUniversalCopyFactoryService() { }

		public void AddOnCopyFinishedAction(Action action)
		{
			onCopyFinishedActions.Add(action);
		}

		readonly List<Action> onCopyFinishedActions = new List<Action>();

		internal void OnCopyFinished()
		{
			onCopyFinishedActions.ForEach(action => action());
			onCopyFinishedActions.Clear();
		}

		public static BusinessObjectUniversalCopyFactoryService GetExistingService(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>();
		}

		public static bool IsRunningUniversalCopy(BusinessObjectFactory factory)
		{
			return GetExistingService(factory) != null;
		}

		public static IDisposable EnsureServiceIsSetUp(BusinessObjectFactory factory)
		{
			return IsRunningUniversalCopy(factory) ? null : SetupServiceTemporarily(factory);
		}

		static IDisposable SetupServiceTemporarily(BusinessObjectFactory factory)
		{
			factory.ServiceContainer.AddService(new BusinessObjectUniversalCopyFactoryService());
			return new DisposableAction(() =>
			{
				factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>().OnCopyFinished();
				factory.ServiceContainer.RemoveService<BusinessObjectUniversalCopyFactoryService>();
			});
		}
	}
}
