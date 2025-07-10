using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.ZArchitecture.Business
{
	public static class AdvOrmFeatureHelper
	{
		public static bool IsEnabled
		{
			get
			{
#if DEBUG
				if(MockedIsEnabled.HasValue)
				{
					return MockedIsEnabled.Value;
				}
#endif
				if (null != ObjectFactory.Get<IFeatureControlManager>()
								.GetFeatureData(LicenceFeatureCodeList.Codes.AdvancedOrderManagerFeatureControl))
				{
					return true;
				}

				return false;
			}
		}
#if DEBUG

		[ThreadStatic]
		static bool? MockedIsEnabled;

		public static void RunTestWith(bool isEnabled, Action action)
		{
			using var advOrm = GetMockedDisposable(isEnabled);
			action();
		}

		public static DisposableAction GetMockedDisposable(bool isEnabled)
		{
			return new DisposableAction(() => MockedIsEnabled = isEnabled, () => MockedIsEnabled = null);
		}
#endif
	}
}
