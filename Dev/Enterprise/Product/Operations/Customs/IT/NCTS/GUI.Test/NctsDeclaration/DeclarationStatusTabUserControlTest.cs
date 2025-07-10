using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class DeclarationStatusTabUserControlTest : TestCaseWithFactory
{
	public void TestMessageStatusDropEdit()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var messageStatusDropEdit = form.StatusTabPageExposed.FindSingleOrDefault<ZDropEdit>("MessageStatusDropEdit");
			AssertNotNull("MessageStatusDropEdit", messageStatusDropEdit);
			AssertDropEditReadOnlyAndVisible("MessageStatusDropEdit", messageStatusDropEdit);
		}
	}

	[RequiresSTA]
	public void TestDepartureStatusDropEdit()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var departureStatusDropEdit = form.StatusTabPageExposed.FindSingleOrDefault<ZDropEdit>("DepartureStatusDropEdit");
			AssertNotNull("DepartureStatusDropEdit", departureStatusDropEdit);
			AssertDropEditReadOnlyAndVisible("DepartureStatusDropEdit", departureStatusDropEdit);
		}
	}

	public void TestControlChannelTextBox()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var controlChannelDropEdit = form.StatusTabPageExposed.FindSingleOrDefault<ZDropEdit>("ControlChannelDropEdit");
			AssertNotNull("ControlChannelDropEdit", controlChannelDropEdit);
			AssertDropEditReadOnlyAndVisible("ControlChannelDropEdit", controlChannelDropEdit);
		}
	}

	public void TestArrivalDate()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var arrivalDateEdit = form.StatusTabPageExposed.FindSingleOrDefault<ZDateEdit>("ArrivalDateEdit");
			AssertNotNull("ArrivalDateEdit", arrivalDateEdit);
			Assert("ArrivalDateEdit", arrivalDateEdit.ReadOnly);
			Assert("ArrivalDateEdit", arrivalDateEdit.Visible);
		}
	}

	public void TestArrivalOffice()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var arrivalOfficeCode = form.StatusTabPageExposed.FindSingleOrDefault<ZTextBox>("ArrivalOfficeCodeTextBox");
			var arrivalOfficeDescription = form.StatusTabPageExposed.FindSingleOrDefault<ZTextBox>("ArrivalOfficeDescriptionTextBox");

			CombineAssertions("Arrival Office code and description not null", () =>
			{
				AssertNotNull("ArrivalOfficeCode", arrivalOfficeCode);
				AssertNotNull("ArrivalOfficeDescription", arrivalOfficeDescription);
			});

			CombineAssertions("Arrival Office code and description readonly and visible", () =>
			{
				Assert("ArrivalOfficeCode", arrivalOfficeCode.ReadOnly);
				Assert("ArrivalOfficeCode", arrivalOfficeCode.Visible);
				Assert("ArrivalOfficeDescription", arrivalOfficeDescription.ReadOnly);
				Assert("ArrivalOfficeDescription", arrivalOfficeDescription.Visible);
			});
		}
	}

	public void TestArrivalStatus()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var arrivalStatusCode = form.StatusTabPageExposed.FindSingleOrDefault<ZTextBox>("ArrivalStatusCodeTextBox");
			var arrivalStatusDescription = form.StatusTabPageExposed.FindSingleOrDefault<ZTextBox>("ArrivalStatusDescriptionTextBox");

			CombineAssertions("Arrival Status code and description not null", () =>
			{
				AssertNotNull("ArrivalStatusCode", arrivalStatusCode);
				AssertNotNull("ArrivalStatusDescription", arrivalStatusDescription);
			});

			CombineAssertions("Arrival Status code and description readonly and visible", () =>
			{
				Assert("ArrivalStatusCode", arrivalStatusCode.ReadOnly);
				Assert("ArrivalStatusCode", arrivalStatusCode.Visible);
				Assert("ArrivalStatusDescription", arrivalStatusDescription.ReadOnly);
				Assert("ArrivalStatusDescription", arrivalStatusDescription.Visible);
			});
		}
	}

	public void TestCustomsGroupBoxVisibleForDepartureJobs()
	{
		AssertCustomsGroupBoxVisibility(departureNctsHeader, expectedVisible: true);
	}

	public void TestCustomsGroupBoxNotVisibleForArrivalJobs()
	{
		var arrivalNctsHeader = Factory.New<NctsHeader>();
		arrivalNctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		AssertCustomsGroupBoxVisibility(arrivalNctsHeader, expectedVisible: false);
	}

	[RequiresSTA]
	public void TestCustomsGroupBox_AcceptanceDateEdit()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var acceptanceDateEdit = form.StatusTabPageExposed.FindSingle<ZDateEdit>("AcceptanceDateEdit");
			AssertDateEditReadOnlyAndVisibleAndBoundTo(acceptanceDateEdit.Name, acceptanceDateEdit, "MovementHeader.BM_EntryDate");
		}
	}

	public void TestCustomsGroupBox_RegistrationNumberTextBox()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var registrationNumberTextBox = form.StatusTabPageExposed.FindSingle<ZTextBox>("RegistrationNumberTextBox");
			AssertTextBoxReadOnlyAndVisibleAndBoundTo(registrationNumberTextBox.Name, registrationNumberTextBox, "EntryNumbersProvider.RegistrationInfo.CE_EntryNum");
		}
	}

	public void TestCustomsGroupBox_RegistrationOfficeTextBox()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var registrationOfficeTextBox = form.StatusTabPageExposed.FindSingle<ZTextBox>("RegistrationOfficeTextBox");
			AssertTextBoxReadOnlyAndVisibleAndBoundTo(registrationOfficeTextBox.Name, registrationOfficeTextBox, "EntryNumbersProvider.RegistrationInfo.CE_EntryLineReference");
		}
	}

	[RequiresSTA]
	public void TestCustomsGroupBox_RegistrationDateEdit()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var registrationDateEdit = form.StatusTabPageExposed.FindSingle<ZDateEdit>("RegistrationDateEdit");
			AssertDateEditReadOnlyAndVisibleAndBoundTo(registrationDateEdit.Name, registrationDateEdit, "EntryNumbersProvider.RegistrationInfo.CE_IssueDate");
		}
	}

	public void TestCustomsGroupBox_MrnTextBox()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var mrnTextBox = form.StatusTabPageExposed.FindSingle<ZTextBox>("MrnTextBox");
			AssertTextBoxReadOnlyAndVisibleAndBoundTo(mrnTextBox.Name, mrnTextBox, "MovementReferenceNumber");
		}
	}

	public void TestCustomsGroupBox_ReleaseCodeTextBox()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var releaseCodeTextBox = form.StatusTabPageExposed.FindSingle<ZTextBox>("ReleaseCodeTextBox");
			AssertTextBoxReadOnlyAndVisibleAndBoundTo(releaseCodeTextBox.Name, releaseCodeTextBox, "EntryNumbersProvider.ReleaseInfo.CE_EntryNum");
		}
	}

	public void TestCustomsGroupBox_ReleaseDateEdit()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var releaseDateEdit = form.StatusTabPageExposed.FindSingle<ZDateEdit>("ReleaseDateEdit");
			AssertDateEditReadOnlyAndVisibleAndBoundTo(releaseDateEdit.Name, releaseDateEdit, "EntryNumbersProvider.ReleaseInfo.CE_IssueDate");
		}
	}

	public void TestA93Grid()
	{
		using (var form = new NctsMovementFormForTesting(departureNctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var a93Grid = form.StatusTabPageExposed.FindSingle<ZGrid>("A93Grid");

			AssertNotNull("A93Grid", a93Grid);
			AssertEquals("A93Grid.Visibile", true, a93Grid.Visible);

			var columnStyles = a93Grid.ColumnStyles.Cast<ZGridColumnInfo>();

			CombineAssertions(() =>
			{
				AssertColumn<ZTextBoxColumnStyleInfo>(CusInBondPayInfo.Schema.BPI_TransactionType, 0);
				AssertColumn<ZTextBoxColumnStyleInfo>(CusInBondPayInfo.Schema.BPI_IncomingPayResponseNo, 1);
				AssertColumn<ZTextBoxColumnStyleInfo>(CusInBondPayInfo.Schema.BPI_MethodOfPayment, 2);
				AssertColumn<ZCalcEditColumnStyleInfo>(CusInBondPayInfo.Schema.BPI_PaymentAmount, 3);
				AssertColumn<ZDateEditColumnStyleInfo>(CusInBondPayInfo.Schema.BPI_PaymentDate, 4);
			});

			void AssertColumn<T>(string columnName, int expectedPosition)
				where T : ZGridColumnInfo
			{
				var columnStyle = columnStyles.SingleOrDefault(x => x.ColumnName == columnName);
				AssertNotNull($"{columnName} should be present", columnStyle);
				AssertType<T>($"{columnName} type", columnStyle);
				AssertEquals($"{columnStyle} IsVisible", true, columnStyle.IsVisible);
				AssertEquals($"{columnStyle} Column Position", expectedPosition, a93Grid.ColumnStyles.IndexOf(columnStyle));
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		departureNctsHeader = Factory.NewDepartureNctsHeader();
	}
	NctsHeader departureNctsHeader;

	#region Implementation

	void AssertDropEditReadOnlyAndVisible(string assertionMessage, ZDropEdit dropEdit)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals(nameof(dropEdit.ReadOnly), true, dropEdit.ReadOnly);
			AssertEquals(nameof(dropEdit.Visible), true, dropEdit.Visible);
		});
	}

	void AssertDateEditReadOnlyAndVisibleAndBoundTo(string assertionMessage, ZDateEdit dateEdit, ZString expectedBindTo)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals(nameof(dateEdit.Visible), true, dateEdit.Visible);
			AssertEquals(nameof(dateEdit.ReadOnly), true, dateEdit.ReadOnly);
			AssertEquals(nameof(dateEdit.BindTo), expectedBindTo, dateEdit.BindTo);
		});
	}

	void AssertTextBoxReadOnlyAndVisibleAndBoundTo(string assertionMessage, ZTextBox textBox, ZString expectedBindTo)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals(nameof(textBox.Visible), true, textBox.Visible);
			AssertEquals(nameof(textBox.ReadOnly), true, textBox.ReadOnly);
			AssertEquals(nameof(textBox.BindTo), expectedBindTo, textBox.BindTo);
		});
	}

	void AssertCustomsGroupBoxVisibility(NctsHeader nctsHeader, bool expectedVisible)
	{
		using (var form = new NctsMovementFormForTesting(nctsHeader))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.StatusTabPageExposed);

			var customsGroupBox = form.StatusTabPageExposed.FindSingle<ZGroupBox>("CustomsGroupBox");
			AssertEquals("CustomsGroupBox.Visible", expectedVisible, customsGroupBox.Visible);
		}
	}

	#endregion
}
