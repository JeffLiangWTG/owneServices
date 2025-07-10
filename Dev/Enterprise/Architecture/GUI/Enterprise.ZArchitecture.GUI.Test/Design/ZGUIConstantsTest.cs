using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZGUIConstantsTest : TestCase
	{
		public void TestTypesExist()
		{
			AssertNotNull("BindToPropertyAttributes", Type.GetType(ZGUIConstants.BindToPropertyAttributes));
			AssertNotNull("ColumnStyleEditor", Type.GetType(ZGUIConstants.ColumnStyleEditor));
			AssertNotNull("GridDesigner", Type.GetType(ZGUIConstants.GridDesigner));
			AssertNotNull("ButtonGridDesigner", Type.GetType(ZGUIConstants.ButtonGridDesigner));
		}
	}
}
