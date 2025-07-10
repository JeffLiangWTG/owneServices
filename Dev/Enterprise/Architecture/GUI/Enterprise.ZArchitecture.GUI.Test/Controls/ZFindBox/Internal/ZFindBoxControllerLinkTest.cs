using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZFindBoxControllerLinkTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestClose_AfterSuspendingListChanged()
		{
			using (var testForm = new ZForm())
			using (Dummy.FilteredCollection.SuspendListChanged())
			{
				// Create the popup elements
				var testAttacher = new ZRecordAttacher(Dummy.Collection, Dummy.FilteredCollection, DummyModuleIDs.Dummy);
				var popupController = new PopupControllerLink(testAttacher);

				// Show the popups attached to a dummy form
				testForm.Show();
				testAttacher.Show(testForm);

				// Add a new element to the collection (which wont update the underlying grid)
				var childBizObj = Dummy.FilteredCollection.AddNew();
				var controller = new ControllerForTest { LastSavedFormExposed = testForm, LastSavedPKExposed = childBizObj.PK };

				// Hook the popup to a dummy controller and save its factory
				popupController.HookController(controller);
				controller.Factory.Save();

				// Close the form
				try
				{
					testForm.Close();
				}
				finally
				{
					testAttacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		public void TestHookToSeveralFormInDifferentControllers()
		{
			using (var findBox = new ZCodeFindBox())
			using (var form1 = new ZForm(new BusinessObjectFactory().New<DummyBusinessObject>()))
			using (var form2 = new ZForm(new BusinessObjectFactory().New<DummyBusinessObject>()))
			using (var form3 = new ZForm(new BusinessObjectFactory().New<DummyBusinessObject>()))
			{
				form1.Show();
				form2.Show();
				form3.Show();

				var controller1 = new ControllerForTest { LastSavedFormExposed = form1, LastSavedPKExposed = ZGuid.NewZGuid() };
				var controller2 = new ControllerForTest { LastSavedFormExposed = form2, LastSavedPKExposed = ZGuid.NewZGuid() };
				var controller3 = new ControllerForTest { LastSavedFormExposed = form3, LastSavedPKExposed = ZGuid.NewZGuid() };

				var link = new ControllerLinkForTest(findBox);

				link.HookController(controller1);
				link.HookController(controller2);
				link.HookController(controller3);

				form1.BusinessEntity.Factory.Save();
				form3.BusinessEntity.Factory.Save();

				AssertEquals("Precondition", ZGuid.Empty, link.LastSavedPK);

				form2.Close();
				AssertEquals("Should not react if there were no saves in that factory", ZGuid.Empty, link.LastSavedPK);

				form3.Close();
				AssertEquals(controller3.LastSavedPK, link.LastSavedPK);

				form1.Close();
				AssertEquals(controller1.LastSavedPK, link.LastSavedPK);
			}
		}

		public void TestPopupControllerLinkNullFindBox()
		{
			using (var form = new ZForm(new BusinessObjectFactory().New<DummyBusinessObject>()))
			{
				form.Show();
				ZCodeFindBox findBox = null;

				var controller = new ControllerForTest { LastSavedFormExposed = form };

				var link = new PopupControllerLink(findBox);
				link.HookController(controller);
				form.BusinessEntity.Factory.Save();

				AssertNoExceptionThrown(() => form.Close());
			}
		}

		public void TestPerformCloseActionDoesNotRecreatePopupForm()
		{
			var testFindBox = new ZFindBoxForTest();
			var popupControllerLink = new PopupControllerLinkForTest(testFindBox);
			var popupForm = testFindBox.PopupForm;
			testFindBox.PopupForm = null;

			popupControllerLink.PerformCloseAction_Exposed(ZGuid.Empty, popupForm);
			var actualPopupForm = testFindBox.PeekPopupForm();

			AssertNull(actualPopupForm);
		}

		class PopupControllerLinkForTest : PopupControllerLink
		{
			public PopupControllerLinkForTest(IFindBox findBox)
				: base(findBox)
			{
			}

			public void PerformCloseAction_Exposed(ZGuid savedPK, IFindBoxPopup popupForm)
			{
				PerformCloseAction(savedPK, popupForm);
			}
		}

		class ZFindBoxForTest : IFindBox
		{
			public ZFindBoxForTest()
			{
				popupForm = new PopupFormForTest();
			}

			IFindBoxPopup popupForm;

			public string Code { get; set; }

			public string Description { get; set; }

			public IFindBoxListProvider ListProvider
			{
				get
				{
					return null;
				}
			}

			public IFindBoxPopup PopupForm
			{
				get
				{
					if (popupForm == null)
					{
						popupForm = new PopupFormForTest();
					}
					return popupForm;
				}
				set
				{
					popupForm = value;
				}
			}

			public IFindBoxPopup PeekPopupForm()
			{
				return popupForm;
			}
		}

		class PopupFormForTest : IFindBoxPopup
		{
			public void ShowModal(IFindBox findBox, Form parentForm)
			{
			}

			public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
			{
				return SilentSelectResult.None;
			}

			public void SelectRowByPK(ZGuid pK)
			{
			}

			public void Dispose()
			{
			}

			public event EventHandler Closed
			{
				add
				{
					throw new NotImplementedException();
				}
				remove
				{
					throw new NotImplementedException();
				}
			}
		}

		class ControllerLinkForTest : ZFindBoxControllerLink
		{
			public ControllerLinkForTest(IFindBox findBox) : base(findBox) { }

			protected override void PerformCloseAction(ZGuid savedPK, IFindBoxPopup popup)
			{
				LastSavedPK = savedPK;
			}

			public ZGuid LastSavedPK { get; private set; }
		}

		class ControllerForTest : ZController
		{
			public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

			public IZForm LastSavedFormExposed
			{
				set { LastShownForm = value; }
			}

			public ZGuid LastSavedPKExposed { private get; set; }

			public override ZGuid LastSavedPK
			{
				get { return LastSavedPKExposed; }
			}

			#region Overrides of ZController

			public override ControllerID ID
			{
				get { throw new NotImplementedException(); }
			}

			public override Type TypeOfTopLevelBusinessObject
			{
				get { throw new NotImplementedException(); }
			}

			protected override IZForm GetForm(IBusiness businessEntity)
			{
				throw new NotImplementedException();
			}

			protected override SecurityCheckpoint CheckPointForView
			{
				get { throw new NotImplementedException(); }
			}

			protected override SecurityCheckpoint CheckPointForNew
			{
				get { throw new NotImplementedException(); }
			}

			protected override SecurityCheckpoint CheckPointForEdit
			{
				get { throw new NotImplementedException(); }
			}

			protected override SecurityCheckpoint CheckPointForDelete
			{
				get { throw new NotImplementedException(); }
			}

			public override ModuleIdentifier ModuleID
			{
				get { return null; }
			}
			#endregion
		}
	}
}
