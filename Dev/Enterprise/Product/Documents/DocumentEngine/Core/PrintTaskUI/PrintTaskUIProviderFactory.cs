using System;
using System.Collections;
using CargoWise.Application;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	public enum PrintTaskUIProviderTypes
	{
		None = 0,
		WinForms,
		Web,
		Unattended
	}

	public static partial class PrintTaskUIProviderFactory
	{
		internal static IPrintTaskUIProvider Create()
		{
#if DEBUG
			if (overriderForTesting != null)
			{
				return overriderForTesting.Value;
			}
#endif
			return Create(GetDefaultUIType());
		}

		// tested in PrintTask
		internal static IPrintTaskUIProvider CreateWithType(PrintTaskUIProviderTypes uiType)
		{
#if DEBUG
			if (overriderForTesting != null)
			{
				return overriderForTesting.Value;
			}
#endif
			var ui = (uiType != PrintTaskUIProviderTypes.None) ? uiType : GetDefaultUIType();

			return Create(ui);
		}

#if DEBUG

		[ThreadStatic]
		internal static OverriderForTesting overriderForTesting;
#endif

		static IPrintTaskUIProvider Create(PrintTaskUIProviderTypes uiType)
		{
			IPrintTaskUIProvider result = null;

			Hashtable printTaskUIProviders = ObjectFactory.Get<Hashtable>("PrintTaskUIProviderDictionary");
			string key = uiType.ToString();

			if (printTaskUIProviders.Contains(key))
			{
				ObjectHandle handle = (ObjectHandle)printTaskUIProviders[key];
				result = (IPrintTaskUIProvider)handle.GetObject();
			}

			return result;
		}

		static PrintTaskUIProviderTypes GetDefaultUIType()
		{
			PrintTaskUIProviderTypes result = PrintTaskUIProviderTypes.Unattended;

			if (Globals.IsUserInteractive && !Globals.IsWeb)
			{
				result = PrintTaskUIProviderTypes.WinForms;
			}
			else if (Globals.IsUserInteractive && Globals.IsWeb)
			{
				result = PrintTaskUIProviderTypes.Web;
			}

			return result;
		}

		#region For Testing
#if DEBUG
		internal static PrintTaskUIProviderTypes GetDefaultUITypeForTesting()
		{
			return GetDefaultUIType();
		}

		internal static IPrintTaskUIProvider CreateForTesting(PrintTaskUIProviderTypes uiType)
		{
			return CreateWithType(uiType);
		}

		public class OverriderForTesting : IDisposable
		{
			public OverriderForTesting(IPrintTaskUIProvider value)
			{
				this.Value = value;
				if (PrintTaskUIProviderFactory.overriderForTesting != null)
				{
					throw new InvalidOperationException("You cannot have more than one Overrider active at once.");
				}
				PrintTaskUIProviderFactory.overriderForTesting = this;
			}

			internal readonly IPrintTaskUIProvider Value;

			void IDisposable.Dispose()
			{
				PrintTaskUIProviderFactory.overriderForTesting = null;
			}
		}
#endif
		#endregion
	}
}
