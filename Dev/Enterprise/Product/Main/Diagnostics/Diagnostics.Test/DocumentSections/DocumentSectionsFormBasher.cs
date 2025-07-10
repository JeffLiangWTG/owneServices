using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(DocumentSectionsForm))]
	public class DocumentSectionsFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DocumentSectionsForm();
		}
	}
}
