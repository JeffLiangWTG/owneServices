using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(CMRSACDialogBox))]
	sealed class CMRSACDialogBoxTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			CusHAWB hAWB = Factory.New<CusHAWB>();
			hAWB.CS_GoodsDescription = "Cigar";
			hAWB.CS_GoodsValue = 250;
			using (CMRSACDialogBox dialog = new CMRSACDialogBox(new SACDialogBizo(hAWB)))
			{
				AssertEquals("Form Header", "Self Assessed Clearance", dialog.FormHeading);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CusHAWB hAWB = Factory.New<CusHAWB>();
			hAWB.CS_GoodsDescription = "Cigar";
			hAWB.CS_GoodsValue = 250;
			return new CMRSACDialogBox(new SACDialogBizo(hAWB));
		}
	}
}
