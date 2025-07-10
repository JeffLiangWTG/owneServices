using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusSeal = Enterprise.Customs.JP.Common.CusSeal;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPAsycudaContainerUserControl))]
	sealed class JPAsycudaContainerUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalSealsGridOrders()
		{
			using var control = new JPAsycudaContainerUserControl();
			var additionalSealsGrid = control.FindSingle<ZGrid>("AdditionalSealsGrid");
			var columnStyles = additionalSealsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			var expectedColumnsOrders = new[] { CusSeal.Schema.BK_SealNumber };
			AssertContainsExactElementsInExactOrder(expectedColumnsOrders, columnStyles);
		}

		public void TestAdditionalSealsGroupBoxCaption()
		{
			using var control = new JPAsycudaContainerUserControl();
			var additionalSealsGroupBox = control.FindSingle<ZGroupBox>("AdditionalSealsGroupBox");
			AssertEquals("Additional Seals", additionalSealsGroupBox.CaptionResourceString.Caption);
		}
	}
}
