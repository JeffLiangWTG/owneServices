#if !WINZOR
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Startup.DeveloperFeatureControlOverrideForm;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(DeveloperFeatureControlOverrideForm))]
	public class DeveloperFeatureControlOverrideFormTest : ZFormBasherTest
	{
		Mock<IFeatureControlManager> mockFeatureControlManager;
		Mock<IFeatureControlRuleRepository> mockFeatureControlRuleRepository;
		Mock<IProgramRestarter> mockProgramRestarter;

		readonly int featureCodesCount = LicenceFeatureCodeList.GetLicenceFeatureCodePairs().Count();
		protected override Form GetFormToBashCore()
		{
			var form = new DeveloperFeatureControlOverrideForm();
			form.Show();
			return form;
		}

		[RequiresSTA]
		public void TestLoadForm()
		{
			// Setup
			SetupMocks(false);
			using var form = GetFormToBashCore();
			var grid = (ZGrid)form.Controls.Find("FeaturesGrid", true).Single();
			AssertNotNull("Grid should not be null", grid);
			AssertEquals("Columns Count", 4, grid.ColumnStyles.Count);
			mockFeatureControlManager.Verify(x => x.GetFeatureDataAsync(It.IsAny<string>(), CancellationToken.None), Times.AtLeast(featureCodesCount));    // Got Data
			mockFeatureControlRuleRepository.Verify(x => x.SaveFeatureControlRuleContentAsync(It.IsAny<byte[]>(), CancellationToken.None), Times.Never);   // Did not save
			form.Close();
		}

		[RequiresSTA]
		public void TestButtons_Cancel()
		{
			// Setup
			SetupMocks(false);
			using var form = GetFormToBashCore();

			// Act
			var newButton = (ZButton)form.Controls.Find("btnCancel", true).Single();
			newButton.PerformClick();

			// Assert
			mockFeatureControlManager.Verify(x => x.GetFeatureDataAsync(It.IsAny<string>(), CancellationToken.None), Times.AtLeast(featureCodesCount));    // Got Data
			mockFeatureControlRuleRepository.Verify(x => x.SaveFeatureControlRuleContentAsync(It.IsAny<byte[]>(), CancellationToken.None), Times.Never);    // Did not save
			Assert("Form is Closed", !form.IsHandleCreated);        // Closed
		}

		[RequiresSTA]
		public void TestButtons_Save()
		{
			// Setup
			SetupMocks(true);
			using var form = GetFormToBashCore();
			// Act
			var newButton = (ZButton)form.Controls.Find("btnSave", true).Single();
			newButton.PerformClick();

			// Assert
			mockFeatureControlManager.Verify(x => x.GetFeatureDataAsync(It.IsAny<string>(), CancellationToken.None), Times.AtLeast(featureCodesCount));    // Got Data
			mockFeatureControlRuleRepository.Verify(x => x.SaveFeatureControlRuleContentAsync(It.IsAny<byte[]>(), CancellationToken.None), Times.Once);    //  save
			Assert("Form is Closed", !form.IsHandleCreated);        // Closed
		}

		[RequiresSTA]
		public void TestButton_SaveAndRestart()
		{
			// Setup
			SetupMocks(true);
			using var form = GetFormToBashCore();
			// Act
			var newButton = (ZButton)form.Controls.Find("btnSave", true).Single();
			var grid = (ZGrid)form.Controls.Find("FeaturesGrid", true).Single();
			var featureData = (grid.DataSource as FeatureDataToBind).Data.Cast<FeatureData>().First();
			featureData.Enabled = !featureData.Enabled;
			featureData.HasChanges = true;
			newButton.PerformClick();

			// Assert
			mockProgramRestarter.Verify(x => x.ShutdownEnterpriseWithMessage(It.IsAny<string>()), Times.Once);
			Assert("Form is Closed", !form.IsHandleCreated);
		}

		[RequiresSTA]
		public void TestButtons_SelectAll()
		{
			// Setup
			SetupMocks(false);
			using var form = GetFormToBashCore();

			// Act
			var newButton = (ZButton)form.Controls.Find("btnSelectAll", true).Single();
			newButton.PerformClick();

			// Assert
			var grid = (ZGrid)form.Controls.Find("FeaturesGrid", true).Single();
			var allEnabled = (grid.DataSource as FeatureDataToBind).Data.Cast<FeatureData>().All(s => s.Enabled);
			Assert("All Rows Enabled", allEnabled);

			// Cleanup
			form.Close();
		}

		[RequiresSTA]
		public void TestButtons_SelectNone()
		{
			// Setup
			SetupMocks(true);
			using var form = GetFormToBashCore();
			// Act
			var newButton = (ZButton)form.Controls.Find("btnSelectNone", true).Single();
			newButton.PerformClick();

			// Assert
			var grid = (ZGrid)form.Controls.Find("FeaturesGrid", true).Single();
			var allDisabled = (grid.DataSource as FeatureDataToBind).Data.Cast<FeatureData>().All(s => !s.Enabled);
			Assert("All Rows Disabled", allDisabled);

			// Cleanup
			form.Close();
		}

		[RequiresSTA]
		public void TestButtons_Reset()
		{
			// Setup
			SetupMocks(true);
			using var form = GetFormToBashCore();
			// Act
			var newButton = (ZButton)form.Controls.Find("btnReset", true).Single();
			newButton.PerformClick();
			newButton = (ZButton)form.Controls.Find("btnSave", true).Single();
			newButton.PerformClick();
			Assert(string.IsNullOrEmpty(WebDataRegistry.Instance.FeatureControlRuleContent.Value));

			//Cleanup
			form.Close();
		}

		void SetupMocks(bool cwNextCodeEnabled)
		{
			EnablePartOfFeaturesForTest();
			mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlRuleRepository = new Mock<IFeatureControlRuleRepository>();
			if (cwNextCodeEnabled)
			{
				var cwNextCode = LicenceFeatureCodeList.Codes.CWNext;
				var featureData = new TestFeatureData() { Code = cwNextCode, Parameter = "{\"key1\":\"value1\"}" };
				mockFeatureControlManager.Setup(x => x.GetFeatureDataAsync(cwNextCode, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)featureData));
			}
			else
			{
				var testCode = LicenceFeatureCodeList.Codes.ControlTowerFeature;
				var featureData = new TestFeatureData() { Code = testCode, Parameter = "{\"key1\":\"value1\"}" };
				mockFeatureControlManager.Setup(x => x.GetFeatureDataAsync(testCode, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)featureData));
			}
			mockFeatureControlRuleRepository.Setup(x => x.SaveFeatureControlRuleContentAsync(It.IsAny<byte[]>(), CancellationToken.None)).Verifiable();

			mockProgramRestarter = new Mock<IProgramRestarter>();
			mockProgramRestarter.Setup(x => x.ShutdownEnterpriseWithMessage(It.IsAny<string>())).Verifiable();

			ObjectFactory.Substitute(mockProgramRestarter.Object);
			ObjectFactory.Substitute(mockFeatureControlManager.Object);
			ObjectFactory.Substitute(mockFeatureControlRuleRepository.Object);
		}

		class TestFeatureData : IFeatureData
		{
			public T DeserializeParameterAsJson<T>()
			{
				throw new System.NotImplementedException();
			}

			public bool TryDeserializeParameterAsJson<T>(out T result)
			{
				throw new System.NotImplementedException();
			}

			public string Code { get; init; }
			public string Parameter { get; init; }
		}
	}
}
#endif
