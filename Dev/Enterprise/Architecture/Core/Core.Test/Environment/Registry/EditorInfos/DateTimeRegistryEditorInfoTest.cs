using System;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DateTimeRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			DateTimeRegistryEditorInfo editorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Time);
			AssertNotNull("EditorInfo", editorInfo);
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Time, editorInfo.DateTimeFormat);
		}

		public void TestBaseDataTypeToBeEdited()
		{
			DateTimeRegistryEditorInfo editorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short);
			AssertEquals("BaseDataTypeToBeEdited", typeof(DateTime), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
