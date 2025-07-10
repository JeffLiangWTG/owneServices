using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class ShipmentTypeUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestBorderTransportMeansDropEdit()
		{
			var borderTransportMeansDropEdit = control.BorderTransportMeansDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(borderTransportMeansDropEdit);
				AssertEquals("BindTo", nameof(JobDeclaration.ZG_BorderTransportMeans), borderTransportMeansDropEdit.BindTo);
			});
		}

		public void TestMethodOfPaymentDropEdit()
		{
			var methodOfPaymentDropEdit = control.MethodOfPaymentDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", methodOfPaymentDropEdit);
				AssertEquals("BindTo", nameof(JobDeclaration.ZG_MethodOfPayment), methodOfPaymentDropEdit.BindTo);
			});
		}

		ShipmentTypeUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentTypeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
