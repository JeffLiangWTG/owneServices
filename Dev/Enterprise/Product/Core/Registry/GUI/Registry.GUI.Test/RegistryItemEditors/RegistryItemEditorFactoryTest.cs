using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Environment.Registry.DataTypesAndValidators;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryItemEditorFactoryTest : TestCase
	{
		public void TestWebPrintNudgeEditorInfo()
		{
			var nudge = new WebPrintNudge();
			var registryItem = new WebPrintNudgeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, nudge);
			var editor = RegistryItemEditorFactory.NewEditor(registryItem, null, new WebPrintNudgeRegistryDataType(nudge), new WebPrintNudgeEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(WebPrintNudgeRegistryItemEditor), editor.GetType());
		}

		public void TestWebPrintNudgeSuspendingEditorInfo()
		{
			var nudgeSuspending = new WebPrintNudgeSuspending();
			var registryItem = new WebPrintNudgeSuspendingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, nudgeSuspending);
			var editor = RegistryItemEditorFactory.NewEditor(registryItem, null, new WebPrintNudgeSuspendingRegistryDataType(nudgeSuspending), new WebPrintNudgeSuspendingEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(WebPrintNudgeSuspendingRegistryItemEditor), editor.GetType());
		}

		public void TestDeleteExpiredRatesRegistryItemEditorInfo()
		{
			var deleteExpiredRates = new DeleteExpiredRates();
			var registryItem = new DeleteExpiredRatesRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, deleteExpiredRates);
			var editor = RegistryItemEditorFactory.NewEditor(registryItem, null, new DeleteExpiredRatesRegistryDataType(deleteExpiredRates), new DeleteExpiredRatesEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(DeleteExpiredRatesRegistryItemEditor), editor.GetType());
		}

		public void TestMs365OAuth2TokenRegistryEditorInfo()
		{
			var registryItem = new Ms365OAuth2TokenRegistryItem("", null, null, null, MailManager.Integration.EmailType.Incoming, null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(registryItem, null, null, new Ms365OAuth2TokenRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(ConsentGrantingAndOAuth2TokenUserRegistryItemEditor), editor.GetType());
		}

		public void TestTextRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.TextBox), null);
			AssertEquals("Editor.GetType()", typeof(TextBoxRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.Password), null);
			AssertEquals("Editor.GetType()", typeof(TextBoxRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText), null);
			AssertEquals("Editor.GetType()", typeof(TextBoxRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.AWBCustomisableMultilineText), null);
			AssertEquals("Editor.GetType()", typeof(TextBoxRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.Memo), null);
			AssertEquals("Editor.GetType()", typeof(TextBoxRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser), null);
			AssertEquals("Editor.GetType()", typeof(DirectoryBrowserRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.ScheduleControl), null);
			AssertEquals("Editor.GetType()", typeof(ScheduleControlRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.OrgHeaderCodeListEdit), null);
			AssertEquals("Editor.GetType()", typeof(OrgHeaderCodeListEditRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new FreightDataRegistry.AWBGridRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(AWBExtraTextControlRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.Guid), null);
			AssertEquals("Editor.GetType()", typeof(TextBoxRegistryItemEditor), editor.GetType());

			editor = RegistryItemEditorFactory.NewEditor(null, null, null, new TextRegistryEditorInfo(TextEditorType.HTML), null);
			AssertEquals("Editor.GetType()", typeof(TextBoxRegistryItemEditor), editor.GetType());
		}

		public void TestCodeFindBoxRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefUNLOCO, factory => new RefUNLOCOCollection(factory)), null);
			AssertEquals("Editor.GetType()", typeof(CodeFindBoxRegistryItemEditor), editor.GetType());
		}

		public void TestASPChargeCodeGuidFindBoxRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.AUCustomsQuarantineChargeCode), null);
			AssertEquals("Editor.GetType()", typeof(ASPChargeCodeGuidFindBoxRegistryItemEditor), editor.GetType());
		}

		public void TestChargeCodeGuidFindBoxRegistryEditorInfo()
		{
			var editor1 = RegistryItemEditorFactory.NewEditor(null, null, null, new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.DisbursementChargeCode), null);
			AssertEquals("Editor.GetType()", typeof(ChargeCodeGuidFindBoxRegistryItemEditor), editor1.GetType());
			var editor2 = RegistryItemEditorFactory.NewEditor(null, null, null, new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.CustomDeferredChargeCode), null);
			AssertEquals("Editor.GetType()", typeof(ChargeCodeGuidFindBoxRegistryItemEditor), editor2.GetType());
		}

		public void TestGuidFindBoxRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new GuidFindBoxRegistryEditorInfo(true, true), null);
			AssertEquals("Editor.GetType()", typeof(GuidFindBoxRegistryItemEditor), editor.GetType());
		}

		public void TestComboBoxRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new ComboBoxRegistryEditorInfo(new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList())), null);
			AssertEquals("Editor.GetType()", typeof(ComboBoxRegistryItemEditor), editor.GetType());
		}

		public void TestBooleanRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new BooleanRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(RadioButtonRegistryItemEditor), editor.GetType());
		}

		public void TestNumericRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new NumericRegistryEditorInfo(0), null);
			AssertEquals("Editor.GetType()", typeof(CalcEditRegistryItemEditor), editor.GetType());
		}

		public void TestOrgRequiredFieldsRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(new RegistryItemForTest("ABC"), null, null, new OrgRequiredFieldsRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(OrgRequiredFieldsRegistryItemEditor), editor.GetType());
		}

		public void TestAutoRatingRequiredFieldsRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new AutoRatingRequiredFieldsRegistryEditorInfo(false), null);
			AssertEquals("Editor.GetType()", typeof(AutoRatingRequiredFieldsRegistryItemEditor), editor.GetType());
		}

		public void TestOrderedListRegistryItemEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new OrderedListRegistryItemEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(OrderedListRegistryItemEditor), editor.GetType());
		}

		public void TestAuthenticationRegistryItemEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new AuthenticationRegistryItemEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(AuthenticationRegistryItemEditor), editor.GetType());
		}

		public void TestZAddressRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new ZAddressRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(ZAddressRegistryItemEditor), editor.GetType());
		}

		public void TestAutoRatingPriorityRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new AutoRatingPriorityRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(AutoRatingPriorityRegistryItemEditor), editor.GetType());
		}

		public void TestRateValidityRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new RateValidityRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(RateValidityRegistryItemEditor), editor.GetType());
		}

		public void TestQuoteValidityRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new QuoteValidityRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(QuoteValidityRegistryItemEditor), editor.GetType());
		}

		public void TestDepartmentMappingEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new DepartmentMappingEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(DepartmentMappingRegistryItemEditor), editor.GetType());
		}

		public void TestContactEditRegistryItemEditor()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new ContactRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(ContactEditRegistryItemEditor), editor.GetType());
		}

		public void TestFileUpLoaderX509CertificateRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new FileUpLoaderX509CertificateRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(DigitalCertificateRegistryItemEditor), editor.GetType());
		}

		public void TestHAWBDocumentPivotRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new HAWBDocumentPivotRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(HAWBDocumentPivotRegistryItemEditor), editor.GetType());
		}

		public void TestMAWBDocumentPivotRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new MAWBDocumentPivotRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(MAWBDocumentPivotRegistryItemEditor), editor.GetType());
		}

		public void TestMarkUpPercentagesRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new MarkUpPercentagesRegistryEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(MarkUpPercentagesRegistryItemEditor), editor.GetType());
		}

		public void TestCodeDescriptionPairListEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new CodeDescriptionPairListEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(CodeDescriptionListRegistryItemEditor), editor.GetType());
		}

		public void TestImageRegistryDataType()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, new ImageRegistryDataType(), null, null);
			AssertEquals("Editor.GetType()", typeof(ImageRegistryItemEditor), editor.GetType());
		}

		public void TestDateTimeRegistryDataType()
		{
			ZDateTimePickerFormat dateTimeFormat = ZDateTimePickerFormat.Short;
			DateTimeRegistryEditorInfo editorInfo = new DateTimeRegistryEditorInfo(dateTimeFormat);
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, editorInfo, null);
			AssertEquals("Editor.GetType()", typeof(DateEditRegistryItemEditor), editor.GetType());

			DateEditRegistryItemEditor dateEditor = (DateEditRegistryItemEditor)editor;
			AssertEquals("EditorInfo", editorInfo, dateEditor.EditorInfo);
		}

		public void TestCodeDescriptionBoolRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new CodeDescriptionBoolRegistryEditorInfo((NoResString)""), null);
			AssertEquals("Editor.GetType()", typeof(CodeDescriptionBoolRegistryItemEditor), editor.GetType());
		}

		public void TestParentAndChildCodeDescriptionBoolRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new ParentAndChildCodeDescriptionBoolRegistryEditorInfo((NoResString)"", (NoResString)"", null, null), null);
			AssertEquals("Editor.GetType()", typeof(ParentAndChildCodeDescriptionBoolRegistryItemEditor), editor.GetType());
		}

		public void TestAccChargeCodeListRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.None), null);
			AssertEquals("Editor.GetType()", typeof(AccChargeCodeListEditRegistryItemEditor), editor.GetType());
		}

		public void TestAccTaxRateListRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new AccTaxRateListRegistryEditorInfo(RegistryFindBoxFilter.None), null);
			AssertEquals("Editor.GetType()", typeof(AccTaxRateListEditRegistryItemEditor), editor.GetType());
		}

		public void TestComboBoxFilterLayoutRegistryEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new ComboBoxFilterLayoutRegistryEditorInfo("TrackingShipments"), null);
			AssertEquals("Editor.GetType()", typeof(ComboBoxFilterLayoutRegistryItemEditor), editor.GetType());
		}

		public void TestWebCustomImageEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(new RegistryItemForTest("WebTrackerCustomImages"), null, null, new WebCustomImagesEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(WebCustomImagesRegistryItemEditor), editor.GetType());
		}

		public void TestWebCustomCssEditorInfo()
		{
			var baseRegistryItem = new StringArrayRegistryItem("TESTREG", null, null, null, RegistryStorageFlags.System);
			var item = new WebCustomCssRegistryItem("WebTrackerCustomCss", baseRegistryItem, null, null, null, RegistryStorageFlags.System);

			var editor = RegistryItemEditorFactory.NewEditor(item, null, null, new WebCustomCssEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(WebCustomCssRegistryItemEditor), editor.GetType());
		}

		public void TestDecimalArrayRegistryDataType()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, new DecimalArrayRegistryDataType(), null, null);
			AssertEquals("editor.GetType()", typeof(DecimalArrayRegistryItemEditor), editor.GetType());
		}

		public void TestStringArraySimpleRegistryDataType()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, new DelimitedStringArrayRegistryDataType(), null, null);
			AssertEquals("editor.GetType()", typeof(StringArrayRegistryItemEditor), editor.GetType());
		}

		public void TestStringArrayJsonRegistryDataType()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, new DelimitedStringArrayRegistryDataType(), null, null);
			AssertEquals("editor.GetType()", typeof(StringArrayRegistryItemEditor), editor.GetType());
		}

		public void TestBinaryKeyRegistryDataType()
		{
			var editor = RegistryItemEditorFactory.NewEditor(null, null, new BinaryKeyRegistryDataType(0), null, null);
			AssertEquals("editor.GetType()", typeof(BinaryKeyRegistryItemEditor), editor.GetType());
		}

		#region TestNonPersistentBusinessObjectRegistryDataType

		public void TestNonPersistentBusinessObjectRegistryDataType()
		{
			DummyRegistryDataType dataType = new DummyRegistryDataType();
			FallbackLevel fallback = new FallbackLevel(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyRegistryItemEditor editor = (DummyRegistryItemEditor)RegistryItemEditorFactory.NewEditor(null, fallback, dataType, null, factory);

			AssertEquals("Editor.DataType", dataType, editor.DataType);
			AssertEquals("Editor.CurrentFallbackLevel", fallback, editor.FallbackLevel);
			AssertEquals("Editor.Factory", factory, editor.BusinessObjectFactory);
		}

		[RegistryEditor("Enterprise.Registry.GUI.Testing.RegistryItemEditorFactoryTest+DummyRegistryItemEditor, Enterprise.Registry.GUI.Test")]
		class DummyRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DummyRegistryBusinessObject>
		{
			public DummyRegistryDataType()
			{
			}
		}

		class DummyRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
		{
			public DummyRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				: base(dataType, fallbackLevel, factory)
			{
				this.fallbackLevel = fallbackLevel;
			}

			public new IRegistryDataType DataType
			{
				get { return base.DataType; }
			}

			public FallbackLevel FallbackLevel
			{
				get { return fallbackLevel; }
			}

			public BusinessObjectFactory BusinessObjectFactory
			{
				get { return Factory; }
			}

			protected override RegistryZUserControl NewBoundWinFormsEditorPane()
			{
				return null;
			}

			readonly FallbackLevel fallbackLevel;
		}

		#endregion

		#region TestDelegatedNewEditor

		public void TestDelegatedNewEditor()
		{
			var editor = RegistryItemEditorFactory.NewEditor(null, null, new AddressListRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new AddressListCollection()).DataType, null, null);
			AssertType<AddressListRegistryItemEditor>(editor);

			try
			{
				OverrideRegistryItemEditorFactory.RegisterThisSubTypeOverride();
				editor = RegistryItemEditorFactory.NewEditor(null, null, new AddressListRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new AddressListCollection()).DataType, null, null);
				AssertNull("Not recognised in the sub-class", editor);

				editor = RegistryItemEditorFactory.NewEditor(new RegistryItemForTest("MEH"), null, null, null, null);
				AssertType<TextBoxRegistryItemEditor>(editor);

				editor = RegistryItemEditorFactory.NewEditor(new RegistryItemForTest("HI THERE"), null, null, null, null);
				AssertType<DirectoryBrowserRegistryItemEditor>(editor);
			}
			finally
			{
				OverrideRegistryItemEditorFactory.UnregisterThisSubTypeOverride();
				editor = RegistryItemEditorFactory.NewEditor(null, null, new AddressListRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new AddressListCollection()).DataType, null, null);
				AssertType<AddressListRegistryItemEditor>(editor);
			}
		}

		class OverrideRegistryItemEditorFactory
		{
			static RegistryItemEditor NewEditor(IRegistryItem item, FallbackLevel fallback, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, BusinessObjectFactory factory)
			{
				RegistryItemEditor result = null;

				if (item != null)
				{
					if (item.Name == "MEH")
					{
						TextRegistryEditorInfo info = new TextRegistryEditorInfo(TextEditorType.Memo);
						result = new TextBoxRegistryItemEditor(dataType, info);
					}
					else if (item.Name == "HI THERE")
					{
						result = new DirectoryBrowserRegistryItemEditor(dataType);
					}
				}

				return result;
			}

			public static void RegisterThisSubTypeOverride()
			{
				RegistryItemEditorFactory.OverridenNewEditorDelegate = new RegistryItemEditorFactory.NewEditorDelegate(NewEditor);
			}

			public static void UnregisterThisSubTypeOverride()
			{
				RegistryItemEditorFactory.OverridenNewEditorDelegate = null;
			}
		}

		class RegistryItemForTest : RegistryItemImpl
		{
			public RegistryItemForTest(string name)
				: base(name, null, null, null, new StringRegistryDataType(), RegistryStorageFlags.System, RegistryOptions.Default)
			{
			}
		}

		#endregion

		public void TestParameterizedStringRegistryItem()
		{
			var item = new ParameterizedStringRegistryItem("test", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.All, RegistryOptions.Default,
				ResString.GetMultilingualString("k", "Test {0}", ResString.GetMultilingualString("y", "Value")), ResString.GetMultilingualString("p", "parameter"));
			var editor = RegistryItemEditorFactory.NewEditor(item, null, item.DataType, item.EditorInfo, null);
			AssertEquals("editor.GetType()", typeof(ParameterizedStringRegistryItemEditor), editor.GetType());
		}

		public void TestCodeDescriptionWithGroupRegistryEditorInfo()
		{
			var editor = RegistryItemEditorFactory.NewEditor(null, null, null, new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)""), null);
			AssertEquals("Editor.GetType()", typeof(CodeDescriptionWithGroupRegistryItemEditor), editor.GetType());
		}

		public void TestCodeDescriptionWithThreeGroupsGroupRegistryEditorInfo()
		{
			var editor = RegistryItemEditorFactory.NewEditor(null, null, null, new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"", (NoResString)"", (NoResString)"", (NoResString)"", (NoResString)"", null, true, false, false), null);
			AssertEquals("Editor.GetType()", typeof(CodeDescriptionWithThreeGroupsRegistryItemEditor), editor.GetType());
		}
	}
}
