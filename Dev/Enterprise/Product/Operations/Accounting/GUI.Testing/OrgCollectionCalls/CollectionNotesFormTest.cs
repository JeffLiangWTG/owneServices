using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.OrgCollectionCalls.Testing
{
	[TestedType(typeof(CollectionNotesForm))]
	public class CollectionNotesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrgHeader qC = Factory.New<OrgHeader>();
			return new CollectionNotesForm(qC);
		}
	}
}
