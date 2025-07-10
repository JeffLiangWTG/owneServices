using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ChargeCodeGuidFindBoxRegistryItemEditor))]
	sealed class ChargeCodeGuidFindBoxRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestCustomValidation()
		{
			using (var editorPane = (ZGuidFindBox)Editor.NewWinFormsEditorPane())
			{
				var chargeCode = Factory.New<AccChargeCode>();
				chargeCode.AC_Code = "TST";

				var chargeCode2 = Factory.New<AccChargeCode>();
				chargeCode2.AC_Code = "TST2";

				var chargeCodeWithDate = new ChargeCodeWithDate();
				chargeCodeWithDate.ChargeCode = chargeCode.PK;
				chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Today;

				Business.RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
				Editor.SetValueFromEditorPane(editorPane, chargeCode.PK.ToGuid());
				AssertEquals("ErrorMessage", "The value should not be same to \"Default Quarantine Charge Code\".", Editor.GetCustomValidation(editorPane));

				Editor.SetValueFromEditorPane(editorPane, chargeCode2.PK.ToGuid());
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));

				Editor.SetValueFromEditorPane(editorPane, Guid.Empty);
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ChargeCodeGuidFindBoxRegistryItemEditor(null, new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.CustomDeferredChargeCode), new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new ChargeCodeGuidFindBoxRegistryItemEditorForTest(null, null, null).GetEditorPaneType();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var result = new GuidRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
			result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.CustomDeferredChargeCode);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { Guid.NewGuid(), Guid.Empty };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		class ChargeCodeGuidFindBoxRegistryItemEditorForTest : ChargeCodeGuidFindBoxRegistryItemEditor
		{
			public ChargeCodeGuidFindBoxRegistryItemEditorForTest(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallback)
				: base(dataType, editorInfo, fallback)
			{
			}

			public Type GetEditorPaneType()
			{
				return typeof(MyGuidFindBox);
			}
		}
	}
}
