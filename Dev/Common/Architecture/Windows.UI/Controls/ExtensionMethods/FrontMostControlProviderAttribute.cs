using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class FrontMostControlProviderAttribute : Attribute
	{
		public FrontMostControlProviderAttribute(Type providerType)
		{
			if (providerType.IsAssignableFrom(typeof(DefaultFrontMostControlProvider)))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "providerType '{0}' not does not inherit from DefaultFrontMostControlProvider", providerType.Name));
			}

			this.ProviderType = providerType;
		}
		public Type ProviderType { get; private set; }

		DefaultFrontMostControlProvider Provider
		{
			get { return provider ?? (provider = (DefaultFrontMostControlProvider)Activator.CreateInstance(ProviderType)); }
		}
		DefaultFrontMostControlProvider provider;

		internal static DefaultFrontMostControlProvider GetFrontMostControlProvider(Control control)
		{
			Argument.NotNull(control, "control cannot be null");

			FrontMostControlProviderAttribute[] providers = (FrontMostControlProviderAttribute[])control.GetType().GetCustomAttributes(typeof(FrontMostControlProviderAttribute), false);
			if (providers.Length > 0)
			{
				return providers[0].Provider;
			}
			else
			{
				return new DefaultFrontMostControlProvider();
			}
		}
	}

	public class DefaultFrontMostControlProvider
	{
		public virtual Control GetFrontMostControl(Control control)
		{
			Control result = control;

			IContainerControl containerControl = control as IContainerControl;
			if (containerControl != null)
			{
				Control activeControl = containerControl.ActiveControl;
				if (activeControl != null)
				{
					DefaultFrontMostControlProvider provider = FrontMostControlProviderAttribute.GetFrontMostControlProvider(activeControl);
					result = provider.GetFrontMostControl(activeControl);
				}
			}

			return result;
		}
	}
}
