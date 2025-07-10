using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZLinkLabelTest : TestCase
	{
		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZLinkLabel.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZLinkLabel)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZLinkLabel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}
}
