using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Term
{
	[TestedType(typeof(TermsProgressForm))]
	public class TermsProgressFormTest : ZFormBasherTest
	{
		public void TestImage()
		{
			using (var form = new TermsProgressForm())
			{
				form.Show();
				Application.DoEvents();
				var loaderImg = Icons.GetImage(IconTypes.Loader);
				var pictureBox = form.Controls.Find("EnterpriseLogo", true).Single() as KPictureBox;
				AssertEquals(pictureBox.Image.GetHashCode(), loaderImg.GetHashCode());
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new TermsProgressForm();
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
