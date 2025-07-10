using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	public class NctsHeaderVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetCustomCommands()
		{
			var commands = supporter.GetCustomCommands(DataContext.FRPortsRegularizationTransitDOA);
			AssertArrayEqualsByElements(new Type[] { typeof(DisabledCommand), typeof(SendNativePortMessageOriginal) }, commands.Select(x => x.GetType()).ToArray());

			commands = supporter.GetCustomCommands(DataContext.FRPortsCustomsCheckCAED);
			AssertArrayEqualsByElements(new Type[] { typeof(DisabledCommand), typeof(SendNativePortMessageOriginal) }, commands.Select(x => x.GetType()).ToArray());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			supporter = new NctsHeaderVisualizableDocumentSupporter(header);
		}

		NctsHeader header;
		NctsHeaderVisualizableDocumentSupporter supporter;
	}
}
