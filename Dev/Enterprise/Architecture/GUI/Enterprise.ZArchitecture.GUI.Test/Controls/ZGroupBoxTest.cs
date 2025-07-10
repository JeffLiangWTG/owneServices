using System.Reflection;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGroupBoxTest : ZControlBaseTestCase<ZGroupBox>
	{
		protected override bool IsDragDropHandledByEDocs
		{
			get { return true; }
		}

		public void TestFlatStyle()
		{
			using (var groupBox = new ZGroupBox())
			{
				AssertEquals(
					"Default FlatStyle should be Standard, otherwise painting of the bold font without affecting child controls won't work",
					FlatStyle.Standard,
					groupBox.FlatStyle);
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZGroupBox.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZGroupBox)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZGroupBox).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}
}
