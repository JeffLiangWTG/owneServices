using System;
using CargoWise.ComponentModel;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	/// <summary>
	/// Provides DateTime format for ZDateTime and DateTime properties
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class DateTimeFormatAttribute : Attribute
	{
		public DateTimeFormatAttribute(KDateTimeFormat dateTimeFormat)
		{
			DateTimeFormat = dateTimeFormat;
		}

		public KDateTimeFormat DateTimeFormat { get; }
	}
}
