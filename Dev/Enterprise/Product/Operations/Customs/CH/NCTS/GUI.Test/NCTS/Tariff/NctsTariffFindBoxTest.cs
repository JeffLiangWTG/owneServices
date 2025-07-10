using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(NctsTariffFindBox))]
class NctsTariffFindBoxTest : TestCaseWithFactory
{
	public void TestGetDescription()
	{
		new RefDataTestHelper(Factory).CreateTariffsForTransit();

		using (var form = new ZForm())
		using (var tariffFindBox = new NctsTariffFindBox())
		{
			form.Controls.Add(tariffFindBox);
			form.Show();

			tariffFindBox.GetShouldShowExactDescription = () => false;
			CombineAssertions("Show Nearest Match Desciption", () =>
			{
				tariffFindBox.CurrentCode = "710121";
				UserIdleWorker.Flush();
				AssertEquals("WCO Tariff code = 710121", "natural pearls unworked", tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "04069099";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 04069099, get description from 04069099001", "cheese", tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "04069099001";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 04069099001", "cheese", tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "84061000";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 84061000", "steam turbines", tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "84061000001";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 84061000001", "steam turbines extra", tariffFindBox.DescriptionBox.Text);
			});

			tariffFindBox.GetShouldShowExactDescription = () => true;
			CombineAssertions("Show Exact Desciption", () =>
			{
				tariffFindBox.CurrentCode = "710121";
				UserIdleWorker.Flush();
				AssertEquals("WCO Tariff code = 710121", ZString.Empty, tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "04069099";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 04069099, NOT get description from 04069099001", ZString.Empty, tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "04069099001";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 04069099001", "cheese", tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "84061000";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 84061000", "steam turbines", tariffFindBox.DescriptionBox.Text);

				tariffFindBox.CurrentCode = "84061000001";
				UserIdleWorker.Flush();
				AssertEquals("CH Export Tariff code = 84061000001", "steam turbines extra", tariffFindBox.DescriptionBox.Text);
			});
		}
	}
}
