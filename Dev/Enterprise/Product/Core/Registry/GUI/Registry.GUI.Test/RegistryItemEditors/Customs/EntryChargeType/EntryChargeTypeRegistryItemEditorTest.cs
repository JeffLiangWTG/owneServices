using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Customs.Testing
{
	[TestedType(typeof(EntryChargeTypeRegistryItemEditor))]
	sealed class EntryChargeTypeRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		public void TestCustomValidation()
		{
			using (var form = new ZForm())
			{
				var editorPane = (EntryChargeTypeControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				var chargeCode = Factory.New<AccChargeCode>();
				chargeCode.AC_Code = "TST";

				var chargeCode2 = Factory.New<AccChargeCode>();
				chargeCode2.AC_Code = "TST2";

				var chargeCodeWithDate = new ChargeCodeWithDate();
				chargeCodeWithDate.ChargeCode = chargeCode.PK;
				chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Today;

				var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				var entryChargeTypeSetting = collection.AddNew();
				entryChargeTypeSetting.AC_ChargeCode = chargeCode.PK;
				entryChargeTypeSetting.ChargeType = Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISContainerCharges;

				Business.RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
				Editor.SetValueFromEditorPane(editorPane, collection);
				AssertEquals("ErrorMessage", "The value should not be same to \"Default Quarantine Charge Code\".", Editor.GetCustomValidation(editorPane));

				entryChargeTypeSetting.AC_ChargeCode = chargeCode2.PK;
				Editor.SetValueFromEditorPane(editorPane, collection);
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));

				Editor.SetValueFromEditorPane(editorPane, Guid.Empty);
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new EntryChargeTypeRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((EntryChargeTypeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EntryChargeTypeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EntryChargeTypeSettingCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			EntryChargeTypeSettingCollection collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

			EntryChargeTypeSetting chargeType1 = collection.AddNew();
			chargeType1.ChargeType = Business.Customs.Testing.EntryChargeTypeList_ForTest.Codes.GST;
			chargeType1.AC_ChargeCode = Factory.New(AccChargeCodeType).PK;

			EntryChargeTypeSetting chargeType2 = collection.AddNew();
			chargeType2.ChargeType = Business.Customs.Testing.EntryChargeTypeList_ForTest.Codes.Duty;
			chargeType2.AC_ChargeCode = Factory.New(AccChargeCodeType).PK;

			EntryChargeTypeSetting chargeType3 = collection.AddNew();
			chargeType3.ChargeType = Business.Customs.Testing.EntryChargeTypeList_ForTest.Codes.EntryFee;
			chargeType3.AC_ChargeCode = Factory.New(AccChargeCodeType).PK;

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			EntryChargeTypeSettingCollection collection1 = (EntryChargeTypeSettingCollection)setValue;
			EntryChargeTypeSettingCollection collection2 = (EntryChargeTypeSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].ChargeType", i.ToString()), collection1[i].ChargeType, collection2[i].ChargeType);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].AC_ChargeCode", i.ToString()), collection1[i].AC_ChargeCode, collection2[i].AC_ChargeCode);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#region AccChargeCodeType
		Type AccChargeCodeType
		{
			get
			{
				if (fAccChargeCodeType == null)
				{
					System.Reflection.Assembly masterFilesAssembly = System.Reflection.Assembly.Load("Enterprise.MasterFiles.Business");
					fAccChargeCodeType = masterFilesAssembly.GetType("Enterprise.MasterFiles.Business.AccChargeCode");
				}
				return fAccChargeCodeType;
			}
		}
		Type fAccChargeCodeType;
		#endregion

		#endregion
	}
}
