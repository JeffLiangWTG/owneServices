using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZTemplateForm))]
	public class ZTemplateFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestGetBusinessEntityForHasChanges()
		{
			var bizObj = Factory.New<DummyBusinessObject>();

			using (var form = new DummyZTemplateForm(bizObj))
			{
				CombineAssertions("Should just return base entity for has changes", () =>
				{
					AssertEquals(bizObj, form.BusinessEntityForHasChanges);
					Assert(bizObj.IsTopLevel);
				});
			}
		}

		[RequiresSTA]
		public void TestGetBusinessEntityForHasChanges_TemplateRecord()
		{
			var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			var templateRecord = Factory.New<DummyTemplateRecord>();
			templateRecordProvider.TemplateRecord = templateRecord;

			using (var form = new DummyZTemplateForm(templateRecordProvider))
			{
				CombineAssertions("Template record exists on parent, should be the top level object for form", () =>
				{
					AssertEquals(templateRecord, form.BusinessEntityForHasChanges);
					Assert(!templateRecordProvider.IsTopLevel);
					Assert(templateRecord.IsTopLevel);
				});
			}

			templateRecordProvider.TemplateRecord = null;

			using (var form = new DummyZTemplateForm(templateRecordProvider))
			{
				CombineAssertions("Is a top level template provider but record does not exist, so provider should be top", () =>
				{
					AssertEquals(templateRecordProvider, form.BusinessEntityForHasChanges);
					Assert(templateRecordProvider.IsTopLevel);
				});
			}
		}

		public void TestGetBusinessEntityForValidation()
		{
			var bizObj = Factory.New<DummyBusinessObject>();

			using (var form = new DummyZTemplateForm(bizObj))
			{
				CombineAssertions("Should just return base entity for has changes", () =>
				{
					AssertEquals(bizObj, form.BusinessEntityForValidation_Exposed);
					Assert(bizObj.IsTopLevel);
				});
			}
		}

		public void TestGetBusinessEntityForValidation_TemplateRecord()
		{
			var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			var templateRecord = Factory.New<DummyTemplateRecord>();
			templateRecordProvider.TemplateRecord = templateRecord;

			using (var form = new DummyZTemplateForm(templateRecordProvider))
			{
				CombineAssertions("Template record exists on parent, should be the top level object for form", () =>
				{
					AssertEquals(templateRecord, form.BusinessEntityForValidation_Exposed);
					Assert(!templateRecordProvider.IsTopLevel);
					Assert(templateRecord.IsTopLevel);
				});
			}

			templateRecordProvider.TemplateRecord = null;

			using (var form = new DummyZTemplateForm(templateRecordProvider))
			{
				CombineAssertions("Is a top level template provider but record does not exist, so provider should be top", () =>
				{
					AssertEquals(templateRecordProvider, form.BusinessEntityForValidation_Exposed);
					Assert(templateRecordProvider.IsTopLevel);
				});
			}
		}

		public void TestGetBusinessObjectForValidation_NotTemplate_ButHasTemplateRecord_ShouldReturnBusinessObject()
		{
			var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			var templateRecord = Factory.New<DummyTemplateRecord>();
			templateRecordProvider.TemplateRecord = templateRecord;

			Assert(templateRecordProvider.IsTemplateRecord);

			using (var form = new DummyZTemplateForm(templateRecordProvider))
			{
				CombineAssertions("Should just return template for validation", () =>
				{
					AssertEquals(templateRecord, form.BusinessEntityForValidation_Exposed);
					Assert(templateRecord.IsTopLevel);
				});
			}

			templateRecordProvider.IsTemplateRecord = false;

			using (var form = new DummyZTemplateForm(templateRecordProvider))
			{
				CombineAssertions("Should just return template record provider/business object for validation", () =>
				{
					AssertEquals(templateRecordProvider, form.BusinessEntityForValidation_Exposed);
					Assert(templateRecordProvider.IsTopLevel);
				});
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var bO = Factory.New<DummyEnterpriseBusinessObject>();
			Factory.Save();
			return new DummyZTemplateForm(bO);
		}

		public class DummyZTemplateForm : ZTemplateForm
		{
			public DummyZTemplateForm(BusinessObject bO) : base(bO) { }

			protected override void InitializeComponent()
			{
				var button = new ZButton();
				button.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("x", "Button");
				button.Click += button_Click;
				MainTabPage.Controls.Add(button);
				CaptionRenderingEnabled = true;
				CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("y", "Title");
			}

			void button_Click(object sender, EventArgs e)
			{
			}

			protected override bool SupportsEDocs
			{
				get { return false; } // EDocs interface is in MasterFiles so can't be easily used here
			}

			public IBusiness BusinessEntityForValidation_Exposed => base.BusinessEntityForValidation;
		}

		#endregion
	}
}
