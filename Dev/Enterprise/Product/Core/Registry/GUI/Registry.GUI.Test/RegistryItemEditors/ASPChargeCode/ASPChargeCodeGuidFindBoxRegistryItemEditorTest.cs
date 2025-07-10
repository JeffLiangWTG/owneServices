using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ASPChargeCodeGuidFindBoxRegistryItemEditor))]
	sealed class ASPChargeCodeGuidFindBoxRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestCustomValidation()
		{
			using (var editorPane = (ZGuidFindBox)Editor.NewWinFormsEditorPane())
			{
				Editor.SetValueFromEditorPane(editorPane, Guid.Empty);
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				Editor.SetValueFromEditorPane(editorPane, Guid.NewGuid());
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				AssertEquals("ErrorMessage", "Please select a valid selection.", Editor.GetCustomValidation(new Button()));

				var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				var chargeCode = Factory.New<AccChargeCode>();
				chargeCode.AC_Code = "TST";
				var entryChargeTypeSetting = collection.AddNew();
				entryChargeTypeSetting.AC_ChargeCode = chargeCode.PK;
				entryChargeTypeSetting.ChargeType = Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISContainerCharges;

				var chargeCodeWithDate = new ChargeCodeWithDate();
				chargeCodeWithDate.ChargeCode = chargeCode.PK;
				chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Today;

				using (Business.RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
				{
					Editor.SetValueFromEditorPane(editorPane, chargeCodeWithDate);
					AssertEquals("ErrorMessage", "The value should not be same to \"Default Disbursement Charge Code\", \"Deferred Charge Code\" and \"Disbursement Charge Code Override\".", Editor.GetCustomValidation(editorPane));

					Editor.SetValueFromEditorPane(editorPane, new ChargeCodeWithDate());
					AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				}

				using (Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid()))
				{
					Editor.SetValueFromEditorPane(editorPane, chargeCodeWithDate);
					AssertEquals("ErrorMessage", "The value should not be same to \"Default Disbursement Charge Code\", \"Deferred Charge Code\" and \"Disbursement Charge Code Override\".", Editor.GetCustomValidation(editorPane));

					Editor.SetValueFromEditorPane(editorPane, new ChargeCodeWithDate());
					AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				}

				using (Business.RatingDataRegistry.Instance.CustomDeferredChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid()))
				{
					Editor.SetValueFromEditorPane(editorPane, chargeCodeWithDate);
					AssertEquals("ErrorMessage", "The value should not be same to \"Default Disbursement Charge Code\", \"Deferred Charge Code\" and \"Disbursement Charge Code Override\".", Editor.GetCustomValidation(editorPane));

					Editor.SetValueFromEditorPane(editorPane, new ChargeCodeWithDate());
					AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				}
			}
		}

		public void TestGetValueFromEditorPane()
		{
			using (var editorPane = (ZGuidFindBox)Editor.NewWinFormsEditorPane())
			{
				var result = Editor.GetValueFromEditorPane(editorPane);
				AssertNotNull(result);
				AssertEquals(Guid.Empty, ((ChargeCodeWithDate)result).ChargeCode);

				var chargeCode1 = Guid.NewGuid();
				var input = new ChargeCodeWithDate();
				input.ChargeCode = chargeCode1;
				input.ActiveTimeUtc = new CargoWise.Types.ZDateTime(2020, 11, 10, 12, 10, 0);

				Editor.SetValueFromEditorPane(editorPane, input);
				result = Editor.GetValueFromEditorPane(editorPane);
				AssertNotNull(result);
				AssertEquals(chargeCode1, ((ChargeCodeWithDate)result).ChargeCode);
			}
		}

		public void TestSetValueFromEditorPane()
		{
			var input = new ChargeCodeWithDate();
			input.ChargeCode = Guid.NewGuid();
			input.ActiveTimeUtc = new CargoWise.Types.ZDateTime(2020, 11, 10, 12, 10, 0);

			using (var editorPane = (ZGuidFindBox)Editor.NewWinFormsEditorPane())
			{
				Editor.SetValueFromEditorPane(editorPane, input);
				AssertEquals(input.ChargeCode, ((ChargeCodeWithDate)(editorPane.Tag)).ChargeCode);
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ASPChargeCodeGuidFindBoxRegistryItemEditor(null, new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.AUCustomsQuarantineChargeCode), new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new ASPChargeCodeGuidFindBoxRegistryItemEditorForTest(null, null, null).GetEditorPaneType();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var result = new ChargeCodeWithDateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
			result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.AUCustomsQuarantineChargeCode);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			dispose = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
		}

		protected override void TearDown()
		{
			dispose?.Dispose();
			base.TearDown();
		}
		IDisposable dispose;

		protected override object[] GetValidRegistryValues()
		{
			return new object[]
			{
				new ChargeCodeWithDate() { ChargeCode = Guid.NewGuid(), ActiveTimeUtc = ZDateTime.Today },
				new ChargeCodeWithDate() { ChargeCode = Guid.Empty, ActiveTimeUtc = ZDateTime.Empty },
			};
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		class ASPChargeCodeGuidFindBoxRegistryItemEditorForTest : ASPChargeCodeGuidFindBoxRegistryItemEditor
		{
			public ASPChargeCodeGuidFindBoxRegistryItemEditorForTest(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallback)
				: base(dataType, editorInfo, fallback, new BusinessObjectFactory())
			{
			}

			public Type GetEditorPaneType()
			{
				return typeof(MyGuidFindBox);
			}
		}
	}
}
