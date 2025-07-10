using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(Exit2ImportForm))]
	public class Exit2ImportFormTest : ZFormBasherTest
	{
		TNTTestUtils TestUtils;

		protected override Form GetFormToBashCore()
		{
			Exit2ImportManager exit2ImportManager = new Exit2ImportManager(new BusinessObjectFactory(), TestUtils.CopyResourceToFile("SYD.X2.20040817.180100.ok"));
			return new Exit2ImportForm(exit2ImportManager);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestUtils = new TNTTestUtils();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestUtils.Dispose();
		}
	}
}
