using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(BulkConsolidationProgressForm))]
	sealed class BulkConsolidationProgressFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var processor = new LVXBulkConsolidateProcessor(new List<ZGuid> { });
			return new BulkConsolidationProgressForm(processor);
		}
	}
}
