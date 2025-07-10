using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZStmALogAddForm))]
	sealed class ZStmALogAddFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			StmALogCollection col = new StmALogCollection(Factory);
			return new ZStmALogAddForm(col.AddNew(typeof(StmALogAsAddedByUser)) as StmALogAsAddedByUser, false);
		}

		protected override void AddHasChangesIsTrueException(ZForm formWithChanges)
		{
			// Adding an event via this form should set the parent form's HasChanges to true
		}
	}
}
