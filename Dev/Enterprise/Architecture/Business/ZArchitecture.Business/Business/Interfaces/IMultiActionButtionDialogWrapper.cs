using System;

namespace Enterprise.ZArchitecture.Business
{
	public interface IMultiActionButtonDialogWrapper<T>
	 where T : struct, IConvertible
	{
		T ShowDialog(string message, string caption, params ButtonStripAction<T>[] buttonStripActions);
		T ShowDialog(string message, string caption, T defaultValue, params ButtonStripAction<T>[] buttonStripActions);
	}
}
