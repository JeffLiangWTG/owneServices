using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZToolStripDropDownButtonTest : TestCase
	{
		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZToolStripDropDownButton.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZToolStripDropDownButton)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZToolStripDropDownButton).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}
}
