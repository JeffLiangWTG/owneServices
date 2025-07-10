using System;
using System.ComponentModel;
using System.Linq;

namespace CargoWise.EntityFramework
{
	interface IBindingTracked
	{
		bool IsBound { get; }
	}

	static class IBindingTracked_Extensions
	{
		public static bool IsBound(this ListChangedEventHandler listChanged, bool doRecursiveCheck)
		{
			var lambda = doRecursiveCheck ? new Func<Delegate, bool>(t => t.Target.IsCurrencyManager() || (t.Target is IBindingTracked bindo && bindo.IsBound))
				: d => d.Target.IsCurrencyManager();
			return listChanged?.GetInvocationList().Any(lambda) ?? false;
		}

		public static bool IsCurrencyManager(this object target) => IsCurrencyManager(target.GetType());

		static bool IsCurrencyManager(Type type) => type.FullName == "System.Windows.Forms.CurrencyManager" || (type.BaseType != null && IsCurrencyManager(type.BaseType));
	}
}
