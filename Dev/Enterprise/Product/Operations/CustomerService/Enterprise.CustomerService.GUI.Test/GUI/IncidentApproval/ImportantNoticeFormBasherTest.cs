using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.GUI.Testing
{
	[TestedType(typeof(ImportantNoticeForm))]
	internal sealed class ImportantNoticeFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ImportantNoticeForm();
		}

		#endregion
	}
}
