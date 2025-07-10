using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Common.GUI.Testing
{
	public abstract class TariffFindBoxTest : TestCaseWithFactory
	{
		public void TestThisExplodesIfBoundBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise()
		{
			ErrorReporter.Clear();
			using (var form = GetDummyForm())
			{
				form.Show();
				Application.DoEvents();
			}

			try
			{
				AssertEquals("ErrorReporter.LastMessageReported", "Enterprise.ZArchitecture.Business.Testing.DummyEnterpriseBusinessObject does not implement IHaveAdditionalDataForBorderWise. This Control cannot bind to a BusinessObject that does not implement this interface.", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestBadlyBoundListGetsDeveloperNotification()
		{
			using (var form = GetFormWithInvalidListBounded())
			{
				form.Show();
			}

			try
			{
				AssertContains("ErrorReporter.LastMessageReported", TariffFindBox.DeveloperErrorBindToListNotSetProperly, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestWrapperGetsAdditionalDataFromBusinessObject()
		{
			using (var form = GetFormWithValidListBounded())
			{
				var haveAdditionalData = form.DataSource as IHaveAdditionalDataForBorderWise;
				AssertNotNull("Precondition: InvoiceLine is IHaveAdditionalDataForBorderWise", haveAdditionalData);
				form.Show();
				var popup = form.FindBox.GetNewPopupFormForTest();
				var wrapper = popup as FindBoxWrapperForBorderWise;
				var expectedAdditionalData = haveAdditionalData.GetAdditionalDataForBorderWise("");
				AssertNotNull("Popup windows is of expected type", wrapper);
				AssertEquals("wrapper.AdditionalData.ParameterForBorderWise", expectedAdditionalData.ParameterForBorderWise, wrapper.AdditionalData.ParameterForBorderWise);
				AssertEquals("wrapper.AdditionalData.DateForDutyRate", expectedAdditionalData.DateForDutyRate, wrapper.AdditionalData.DateForDutyRate);
			}
		}

		public void TestFormForWhenBorderWiseIsNotEnabled()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			using (var findBox = GetConcreteFindBox())
			{
				using (var popupForm = (ZForm)findBox.GetNewPopupFormForTest())
				{
					AssertEquals("FindBox.GetNewPopupForm().GetType()", ExpectedFormTypeWhenBorderWiseNotEnabled, popupForm.GetType());
				}
			}
		}
		protected abstract TariffFindBox GetConcreteFindBox();
		protected abstract TestFormWithFindBox GetFormWithValidListBounded();
		protected abstract TestFormWithFindBox GetFormWithInvalidListBounded();
		protected abstract TestFormWithFindBox GetDummyForm();
		protected abstract Type ExpectedFormTypeWhenBorderWiseNotEnabled
		{
			get;
		}

		protected abstract class TestFormWithFindBox : ZForm
		{
			public TestFormWithFindBox(BusinessObject businessObject) : base(businessObject)
			{
			}

			public TariffFindBox FindBox
			{
				get;
				protected set;
			}
		}
	}
}
