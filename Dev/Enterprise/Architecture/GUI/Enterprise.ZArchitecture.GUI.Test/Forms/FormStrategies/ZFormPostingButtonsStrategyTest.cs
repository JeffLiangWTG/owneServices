using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Core.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.FormStrategies
{
	[GuiTest]
	public class ZFormPostingButtonsStrategyTest : TestCaseWithFactory
	{
		public void TestDeleteButtonText()
		{
			using (var form = new ZFormTest.ZFormForTestBusinessEntityForValidation(Factory.New<ZFormTest.DummyCancellableWhichCanNotBeCancelled>()))
			using (var postingButtonsUserControl = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, postingButtonsUserControl);
				ZFormPostingButtonsStrategy.DoDisplayModeDelete(form);
				var buttonsProvider = form as IPostingButtonsProvider;
				AssertEquals("Activate", buttonsProvider.CommandButtonPost.Text);
				form.BusinessEntityForValidationForTest = Factory.New<DummyBusinessObject>();
				ZFormPostingButtonsStrategy.DoDisplayModeDelete(form);
				buttonsProvider = form;
				AssertEquals("&Delete", buttonsProvider.CommandButtonPost.Text);
			}
		}

		public void TestDeleteButtonText_TemplateRecord()
		{
			var dummyTemplateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			dummyTemplateRecordProvider.TemplateRecord = Factory.New<DummyTemplateRecord>();

			using (var form = new ZFormTest.ZFormForTestBusinessEntityForValidation(dummyTemplateRecordProvider))
			using (var postingButtonsUserControl = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, postingButtonsUserControl);
				ZFormPostingButtonsStrategy.DoDisplayModeDelete(form);

				var buttonsProvider = form as IPostingButtonsProvider;
				AssertEquals("Activate", buttonsProvider.CommandButtonPost.Text);

				(dummyTemplateRecordProvider.TemplateRecord as ICancellable).IsCancelled = true;
				ZFormPostingButtonsStrategy.DoDisplayModeDelete(form);
				buttonsProvider = form;

				AssertEquals("Deactivate", buttonsProvider.CommandButtonPost.Text);
			}
		}

		public void TestDoDisplayModeNew()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var postingButtonsUserControl = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, postingButtonsUserControl);
				ZFormPostingButtonsStrategy.DoDisplayModeNew(form);

				var buttonsProvider = form as IPostingButtonsProvider;
				Assert(buttonsProvider.CommandButtonApply.Visible);
				Assert(buttonsProvider.CommandButtonApply.Enabled);
				Assert(buttonsProvider.CommandButtonPost.Visible);
				Assert(buttonsProvider.CommandButtonPost.Enabled);
				Assert(buttonsProvider.CommandButtonCancel.Visible);
				Assert(buttonsProvider.CommandButtonCancel.Enabled);
				AssertEquals("&Cancel", buttonsProvider.CommandButtonCancel.Text);
			}
		}

		public void TestDoDisplayModeNewWithoutChanges()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var postingButtonsUserControl = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, postingButtonsUserControl);
				ZFormPostingButtonsStrategy.DoDisplayModeNewSaved(form);

				var buttonsProvider = form as IPostingButtonsProvider;
				Assert(buttonsProvider.CommandButtonApply.Visible);
				Assert(!buttonsProvider.CommandButtonApply.Enabled);
				Assert(buttonsProvider.CommandButtonPost.Visible);
				Assert(!buttonsProvider.CommandButtonPost.Enabled);
				Assert(buttonsProvider.CommandButtonCancel.Visible);
				Assert(buttonsProvider.CommandButtonCancel.Enabled);
				AssertEquals("&Close", buttonsProvider.CommandButtonCancel.Text);
			}
		}

		public void TestFactoryChanged()
		{
			var dummies = new DummyBusinessObjectCollection(Factory);
			using (var form = new TestZForm(dummies))
			using (var postingButtonsUserControl = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, postingButtonsUserControl);

				form.OriginalDisplayMode = ODisplayMode.NewSaved;
				form.DisplayMode = ODisplayMode.Edit;
				AssertEquals("Just set to Edit.", ODisplayMode.Edit, form.DisplayMode);

				Factory.Save();
				AssertEquals("Saving the Factory should reset the Forms Display Mode to original NewSaved.", ODisplayMode.NewSaved, form.DisplayMode);

				form.DisplayMode = ODisplayMode.Edit;
				var newFactory = new BusinessObjectFactory();
				dummies.SwapFactoryAndRemoveAll(newFactory);
				form.OnFactoryChangedForTest();
				Factory.Save();
				AssertEquals("Should still be Edit as we have saved the Old Factory which has been unhooked.", ODisplayMode.Edit, form.DisplayMode);

				newFactory.Save();
				AssertEquals("Ensure saving the new factory should reset the Display Mode to the original NewSaved.", ODisplayMode.NewSaved, form.DisplayMode);

				form.DisplayMode = ODisplayMode.Edit;
				var newerFactory = new BusinessObjectFactory();
				dummies.SwapFactoryAndRemoveAll(newerFactory);
				form.OnFactoryChangedForTest();
				newFactory.Save();
				AssertEquals("Should still be Edit as we have saved the new (now old) Factory which has been unhooked.", ODisplayMode.Edit, form.DisplayMode);

				newerFactory.Save();
				AssertEquals("Ensure saving the newer factory should reset the Display Mode to the original NewSaved.", ODisplayMode.NewSaved, form.DisplayMode);
			}
		}

		#region TestAllowNew

		public void TestAllowNew()
		{
			using (var form = new AllowNewForm())
			using (var postingButtonsUserControl = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, postingButtonsUserControl);

				form.AllowNewForTest = true;
				ZFormPostingButtonsStrategy.DoDisplayModeBrowse(form);

				IPostingButtonsProvider buttonsProvider = form;

				Assert(buttonsProvider.CommandButtonApply.Visible);
				Assert(buttonsProvider.CommandButtonApply.Enabled);
				AssertEquals("&New", buttonsProvider.CommandButtonApply.Text);

				form.AllowNewForTest = false;
				ZFormPostingButtonsStrategy.DoDisplayModeBrowse(form);

				Assert(buttonsProvider.CommandButtonApply.Visible);
				Assert(!buttonsProvider.CommandButtonApply.Enabled);
				AssertEquals("&Save", buttonsProvider.CommandButtonApply.Text);

				form.IsPostOnly = true;
				ZFormPostingButtonsStrategy.DoDisplayModeBrowse(form);

				Assert(!buttonsProvider.CommandButtonApply.Visible);
				Assert(!buttonsProvider.CommandButtonApply.Enabled);
			}
		}

		class AllowNewForm : ZForm
		{
			protected override bool AllowNew
			{
				get { return AllowNewForTest; }
			}

			public bool AllowNewForTest { get; set; }
		}

		#endregion

		public void TestDeferredUpdateSaveButtonsBasedOnHasChangesExcludingChildren()
		{
			var dummyBizo = Factory.New<DummyBusinessObject>();
			Factory.Save();

			using (var form = new ZForm(dummyBizo))
			{
				form.OriginalDisplayMode = ODisplayMode.NewSaved;
				form.DisplayMode = ODisplayMode.NewSaved;
				form.LastSaveCount = 0;

				using (ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(form, true))
				{
					var childBizo = Factory.New<DummyBusinessObject>();
					dummyBizo.RegisterEditableChildObject(childBizo);
					childBizo.Z0_AnotherNumber = 1;
					ZFormPostingButtonsStrategy.UpdateSaveButtonsBasedOnHasChanges(form);
				}

				CombineAssertions("Should not not include HasChanges from children.", () =>
				{
					AssertEquals(ODisplayMode.NewSaved, form.DisplayMode);
					AssertEquals(ODisplayMode.NewSaved, form.OriginalDisplayMode);
					AssertEquals(0, form.LastSaveCount);
				});

				using (ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(form))
				{
					var childBizo = Factory.New<DummyBusinessObject>();
					dummyBizo.RegisterEditableChildObject(childBizo);
					childBizo.Z0_AnotherNumber = 1;
					ZFormPostingButtonsStrategy.UpdateSaveButtonsBasedOnHasChanges(form);
				}

				CombineAssertions("Should include HasChanges from children.", () =>
				{
					AssertEquals(ODisplayMode.Edit, form.DisplayMode);
					AssertEquals(ODisplayMode.NewSaved, form.OriginalDisplayMode);
					AssertEquals(BusinessObjectFactory.GlobalSaveCount, form.LastSaveCount);
				});
			}
		}
	}
}
