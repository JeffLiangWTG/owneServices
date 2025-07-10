using System;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	interface IValueChangedActionSupporter
	{
		/// <summary>
		///	Adds an action to perform when property with propertyName changes. This action will run during merging from overrides and reset (as opposed to ZPropertyInfo.ValueChanged).
		/// </summary>
		/// <param name="propertyName">name of hte property for which the value changes</param>
		/// <param name="action">action to perform when the property value changes</param>
		void AddOnValueChangedAction(string propertyName, Action action);
	}
}
