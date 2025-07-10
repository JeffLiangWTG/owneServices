using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public interface IAdornmentLayout
	{
		Type ControlType  { get; }
		IEnumerable<Control> GetBackroundAdornmentTargets(Control source);
		IEnumerable<IIconLayout> GetIconAdornmentTargets(Control source);
	}

	public class AdornmentLayout<T> : IAdornmentLayout where T : Control
	{
		public Type ControlType
		{
			get { return typeof(T); }
		}

		public virtual IEnumerable<Control> GetBackroundAdornmentTargets(T source)
		{
			yield return source;
		}

		public virtual IEnumerable<IIconLayout> GetIconAdornmentTargets(T source)
		{
			yield return new IconLayout(source, IconAlignment.Default);
		}

		#region IAdornmentLayout Members

		IEnumerable<Control> IAdornmentLayout.GetBackroundAdornmentTargets(Control source)
		{
			return GetBackroundAdornmentTargets((T)source);
		}

		IEnumerable<IIconLayout> IAdornmentLayout.GetIconAdornmentTargets(Control source)
		{
			return GetIconAdornmentTargets((T)source);
		}

		#endregion
	}

	class DefaultAdornmentLayout : AdornmentLayout<Control>
	{ }
}
