using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZMessageBoxWithFixedSizeTest : TransactionedTestCase
	{
		public void TestPerformanceWithHugeMessageContent()
		{
			var message = (NoResString)"Sometimes, we need to use this messagebox for showing a large number of validation errors.";
			var sb = new StringBuilder();
			for (var i = 0; i < 25_000 * 4; i++)
			{
				sb.AppendLine(message);
			}

			var messageContent = sb.ToString();
			var sw = new Stopwatch();
			sw.Start();
			using (var messageBox = new ZMessageBoxWithFixedSize(messageContent, "Performance Test", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
			{
				messageBox.Show();
			}

			sw.Stop();
			AssertLessThan("The total time should less than 10 seconds.", sw.Elapsed.TotalSeconds, 10);
		}
	}
}
