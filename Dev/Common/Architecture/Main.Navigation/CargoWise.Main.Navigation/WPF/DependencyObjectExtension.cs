using System.Windows;
using CargoWise.Common;

namespace CargoWise.Main.Navigation.WPF;
public static class DependencyObjectExtension
{
	public static T GetValue<T>(this DependencyObject dobj, DependencyProperty dp)
	{
		Argument.NotNull(dobj, nameof(dobj));
		Argument.NotNull(dp, nameof(dp));
		var result = dobj.GetValue(dp);
		return (T)result;
	}
}
