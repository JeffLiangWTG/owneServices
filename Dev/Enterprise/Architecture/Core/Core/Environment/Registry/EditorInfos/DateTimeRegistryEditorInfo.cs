using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class DateTimeRegistryEditorInfo : RegistryEditorInfo
	{
		public DateTimeRegistryEditorInfo(ZDateTimePickerFormat dateTimeFormat)
		{
			DateTimeFormat = dateTimeFormat;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(DateTime); }
		}

		public readonly ZDateTimePickerFormat DateTimeFormat;
	}
}
