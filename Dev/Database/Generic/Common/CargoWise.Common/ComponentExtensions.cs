using System.ComponentModel;
using System.ComponentModel.Design;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common
{
	public static class ComponentExtensions
	{
		/// <summary>
		/// Get whether the given component is in design mode.
		/// This will return false until the Site is assigned, in which case the value of this method
		/// cannot be relied upon when:<br/>
		/// <br/>
		/// In the constructor of the component.<br/>
		/// In the constructor of a parent component.<br/>
		/// </summary>
		[ValidExceptionFilterMember]
		public static bool IsDesignMode(this IComponent component)
		{
			Argument.NotNull(component, nameof(component));
			bool result =
#if DEBUG
				IsSwitchedToDesignMode ||
#endif
				(component.Site != null && (component.Site.DesignMode || component.Site.GetService(typeof(IDesignerHost)) != null));
			return result;
		}

#if DEBUG

		/// <summary>
		/// Get whether we are currently within a using (ComponentExtensions.SwitchToDesignMode()) region.
		/// </summary>
		public static bool IsSwitchedToDesignMode
		{
			get { return switchedToDesignMode > 0; }
		}

		/// <summary>
		/// Switch to design mode temporarily for a unit test.
		/// </summary>
		public static System.IDisposable SwitchToDesignMode()
		{
			switchedToDesignMode++;
			return new DisposableAction(delegate { switchedToDesignMode--; });
		}
		[System.ThreadStatic]
		static int switchedToDesignMode;
#endif
	}
}
