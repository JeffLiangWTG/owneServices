using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class AccCollectionBatchFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMoveGridSplitter()
		{
			var batchCollection = new AccCollectionBatchCollection(Factory);
			var filterBO = new AccCollectionBatchFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				AccCollectionBatchFilterControl filterControl = new AccCollectionBatchFilterControl(batchCollection, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
				filterControl.GridSplitter_SplitterMoved(filterControl.GridSplitter, null);
			}
		}
	}
}
