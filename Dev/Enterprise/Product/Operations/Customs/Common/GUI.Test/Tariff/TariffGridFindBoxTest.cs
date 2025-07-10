using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.GUI.Testing
{
	public abstract class TariffGridFindBoxTest : TestCaseWithFactory
	{
		public void TestTariffClassFindBoxListProvider()
		{
			using (TariffGridFindBox findBox = GetNewTariffGridFindBox())
			{
				var listProvider = typeof(TariffGridFindBox)
					.GetProperty("ListProvider", BindingFlags.Instance | BindingFlags.NonPublic)
					.GetValue(findBox, null);

				AssertType(ExpectedListProviderType, listProvider);
			}
		}

		#region TestThisExplodesIfBoundBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise
		public void TestThisExplodesIfBoundBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise()
		{
			ErrorReporter.Clear();
			DummyBusinessObject dummyBO = DummyBusinessObject.New(Factory);
			try
			{
				using (TestFormTakingDummyBO form = new TestFormTakingDummyBO(dummyBO))
				{
					form.Show();
					DummyChildBusinessObject dummyChild1 = dummyBO.Collection.AddNew();
					TariffColumnStyle columnStyle = (TariffColumnStyle)form.Grid.Columns[0].ColumnStyle;
					form.Grid.CurrentCell = new DataGridCell(1, 0);
					form.Grid.CurrentCell = new DataGridCell(0, 0);
					typeof(TariffGridFindBox).GetMethod("GetNewPopupForm", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(columnStyle.FindBox, null);
				}

				AssertContains("ErrorReporter.LastMessageReported", "DummyChildBusinessObject does not implement IHaveAdditionalDataForBorderWise. This Control cannot bind to a BusinessObject that does not implement this interface.", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion
		#region TestWrapperGetsGivenAnApproriateAdditionalInfoObject
		public void TestWrapperGetsGivenAnApproriateAdditionalInfoObject()
		{
			ErrorReporter.Clear();
			DummyEnterpriseBusinessObject dummyBO = DummyEnterpriseBusinessObject.New(Factory);
			DummyChildWithInterface childDummyBO1 = Factory.New<DummyChildWithInterface>();
			childDummyBO1.Z0_Bool = true;
			childDummyBO1.Z0_Date = new ZDateTime(2005, 12, 23);
			dummyBO.Collection.Add(childDummyBO1);
			DummyChildWithInterface childDummyBO2 = Factory.New<DummyChildWithInterface>();
			childDummyBO2.Z0_Bool = true;
			childDummyBO2.Z0_Date = new ZDateTime(2005, 12, 23);
			dummyBO.Collection.Add(childDummyBO2);
			try
			{
				using (TestFormTakingDummyWithInterface form = new TestFormTakingDummyWithInterface(dummyBO))
				{
					form.Show();
					TariffColumnStyle columnStyle = (TariffColumnStyle)form.Grid.Columns[0].ColumnStyle;
					form.Grid.CurrentCell = new DataGridCell(1, 0);
					form.Grid.CurrentCell = new DataGridCell(0, 0);
					IFindBoxPopup popup = (IFindBoxPopup)typeof(TariffGridFindBox).GetMethod("GetNewPopupForm", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(columnStyle.FindBox, null);
					AssertEquals(typeof(FindBoxWrapperForBorderWise), popup.GetType());
					FindBoxWrapperForBorderWise wrapper = popup as FindBoxWrapperForBorderWise;
					AssertEquals("ErrorReporter.LastMessageReported", "", ErrorReporter.LastMessageReported);
					AssertEquals("Wrapper.AdditionalData.ParameterForBorderWise", "I", wrapper.AdditionalData.ParameterForBorderWise);
					AssertEquals("Wrapper.AdditionalData.DateForDutyRate", new ZDateTime(2005, 12, 23), wrapper.AdditionalData.DateForDutyRate);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion
		#region Implementation
		protected abstract TariffColumnStyleInfo GetNewTariffColumnStyleInfo();
		protected abstract Type ExpectedFormTypeWhenBorderWiseNotEnabled
		{
			get;
		}

		protected abstract Type ExpectedListProviderType
		{
			get;
		}

		protected abstract TariffGridFindBox GetNewTariffGridFindBox();
		#region CurrentInstance
		static TariffGridFindBoxTest currentInstance;
		protected override void SetUp()
		{
			currentInstance = this;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			currentInstance = null;
		}

		#endregion
		#region class TestFormTakingDummyWithInterface
		class TestFormTakingDummyWithInterface : ZArchitecture.GUI.Testing.ZTestForm
		{
			public TestFormTakingDummyWithInterface(DummyEnterpriseBusinessObject dummyBO) : base(dummyBO)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid.ColumnStyles.Clear();
				TariffColumnStyleInfo columnStyleInfo = currentInstance.GetNewTariffColumnStyleInfo();
				columnStyleInfo.ColumnName = DummyBizoSchema.Z0_VarCharMax.Name;
				columnStyleInfo.BindToList = "Lookups.DummyList";
				Grid.ColumnStyles.Add(columnStyleInfo);
			}
		}

		#endregion
		#region class TestFormTakingDummyBO
		class TestFormTakingDummyBO : ZArchitecture.GUI.Testing.ZTestForm
		{
			public TestFormTakingDummyBO(DummyBusinessObject dummy) : base(dummy)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid.ColumnStyles.Clear();
				TariffColumnStyleInfo columnStyleInfo = currentInstance.GetNewTariffColumnStyleInfo();
				columnStyleInfo.ColumnName = DummyBizoSchema.Z0_VarCharMax.Name;
				columnStyleInfo.BindToList = "Lookups.DummyList";
				Grid.ColumnStyles.Add(columnStyleInfo);
			}
		}
		#endregion
		#endregion
	}
}
