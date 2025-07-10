using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.GUI;

namespace Enterprise.Client.YAS.GUI.Testing
{
	public class YASMenuForTest : YASMenu
	{
		public new static EDIMenu New()
		{
			return new YASMenuForTest();
		}

		public MenuItem DataMenuItemForTest
		{
			get
			{
				return this.dataMenuItem;
			}
		}

		public void SetTopLevelMenuForTest()
		{
			this.SetupTopLevelMenu();
		}
	}
}
