using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class SupportingDocumentFieldsControlTest : TestCaseWithFactory
	{
		public void TestQuantityCalcDropEdit()
		{
			TestControl<ZCalcDropEdit>(nameof(SupportingDocumentFieldsControl.QuantityCalcDropEdit), (c) =>
			{
				AssertEquals("BindToAmount", nameof(SupportingDocument.Schema.CSI_Quantity), c.BindToAmount);
				AssertEquals("BindToUnit", nameof(SupportingDocument.Schema.CSI_UnitOfQuantity), c.BindToUnit);
			});
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				TestControlAndCharacterCasing<ZTextBox>("ReferenceNumberTextBox", CharacterCasing.Normal);
				TestControlAndCharacterCasing<ZCodeFindBox>("CodeCodeFindBox");
				TestControlAndCharacterCasing<ZCalcEdit>("QuantityCalcEdit", CharacterCasing.Upper);
				TestControlAndCharacterCasing<ZDropEdit>("StatusDropEdit", CharacterCasing.Upper);
				TestControlAndCharacterCasing<ZCalcEdit>("Quantity2CalcEdit", CharacterCasing.Upper);
				TestControlAndCharacterCasing<ZTextBox>("UnitOfQuantity2TextBox", CharacterCasing.Normal);
				TestControlAndCharacterCasing<ZCalcEdit>("ValueCalcEdit", CharacterCasing.Upper);
				TestControlAndCharacterCasing<ZCodeFindBox>("CurrencyCodeFindBox");
				TestControlAndCharacterCasing<ZDateEdit>("DateOfExpiryDateEdit");
				TestControlAndCharacterCasing<ZCodeFindBox>("ReferenceNumberCodeFindBox");
				TestControlAndCharacterCasing<ZTextBox>("UnitOfQuantityTextBox", CharacterCasing.Normal);
				TestControlAndCharacterCasing<ZDateEdit>("DateOfIssueDateEdit");
				TestControlAndCharacterCasing<ZTextBox>("AdditionalDescriptionTextBox", CharacterCasing.Upper);
				TestControlAndCharacterCasing<ZDropEdit>("UnitOfQuantityDropEdit", CharacterCasing.Upper);
				TestControlAndCharacterCasing<ZCalcEdit>("DocumentLineNoCalcEdit", CharacterCasing.Upper);
			});
		}

		void TestControlAndCharacterCasing<T>(string controlName, CharacterCasing? characterCasing = null) where T : Control
		{
			T field = null;
			AssertNoExceptionThrown($"{controlName} should be found", () => { field = control.FindSingle<T>(controlName); });
			AssertNotNull($"{controlName} should not be bull", field);
			if (characterCasing != null)
			{
				AssertEquals($"{controlName} CharacterCasing", characterCasing, typeof(T).GetProperty("CharacterCasing")?.GetValue(field));
			}
		}

		void TestControl<T>(string controlName, Action<T> assertControl) where T : Control
		{
			using (var control = new SupportingDocumentFieldsControl())
			{
				CombineAssertions(() =>
				{
					T field = null;
					AssertNoExceptionThrown($"{controlName} should be found", () => { field = control.FindSingle<T>(controlName); });
					assertControl(field);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SupportingDocumentFieldsControl();
		}
		SupportingDocumentFieldsControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
		}
	}
}
