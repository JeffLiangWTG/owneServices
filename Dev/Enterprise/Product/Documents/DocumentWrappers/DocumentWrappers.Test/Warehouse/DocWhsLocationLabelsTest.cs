using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsLocationLabels))]
	sealed class DocWhsLocationLabelsTest : DocumentWrapperTestCase
	{
		#region Properties

		#region Collections

		public void TestLabels()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "ROW", 2, 1);
			Factory.Save();

			var locationA1 = warehouse.FindLocation("ROW-1");
			var locationA2 = warehouse.FindLocation("ROW-2");

			AssertEquals(0, DocWrapper.Labels.Count);

			LocationLabelControl.Locations.Add(locationA1);
			LocationLabelControl.Locations.Add(locationA2);

			AssertEquals(2, DocWrapper.Labels.Count);
			AssertEquals(locationA1.ToLocationString(), DocWrapper.Labels[0].LocationString);
			AssertEquals(locationA2.ToLocationString(), DocWrapper.Labels[1].LocationString);
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			Helper = new WhsTestHelperFunctionsEnv(Factory);
			LocationLabelControl = new WhsLocationLabelList(Factory);
			DocWrapper = DocWhsLocationLabels.New(LocationLabelControl, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsLocationLabelList LocationLabelControl;
		DocWhsLocationLabels DocWrapper;
		WhsTestHelperFunctionsEnv Helper;

		#endregion
	}
}
