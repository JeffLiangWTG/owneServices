using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AWBExtraTextControl))]
	sealed class AWBExtraTextControlTest : RegistryZUserControlTestCase
	{
		[ExpectNoExceptions]
		public void TestCreateInstance()
		{
			var codeDescriptionListControl = new Mock<ICodeDescriptionListControl>();

			var dummyControl = new ZUserControl();
			var dummyGrid = new ZGrid();

			codeDescriptionListControl.Setup(m => m.ReadOnly).Returns(false);
			codeDescriptionListControl.Setup(m => m.Control).Returns(dummyControl);
			codeDescriptionListControl.Setup(m => m.Grid).Returns(dummyGrid);

			using (new AWBExtraTextControl(codeDescriptionListControl.Object))
			{
			}

			dummyControl.Dispose();
			dummyGrid.Dispose();

			codeDescriptionListControl.Verify(m => m.ReadOnly, Times.AtLeastOnce);
			codeDescriptionListControl.Verify(m => m.Control, Times.AtLeastOnce);
			codeDescriptionListControl.Verify(m => m.Grid, Times.AtLeastOnce);
		}

		public void TestControlReadOnly()
		{
			var codeDescriptionListControl = new Mock<ICodeDescriptionListControl>();

			var dummyControl = new ZUserControl();
			var dummyGrid = new ZGrid();
			dummyControl.Controls.Add(dummyGrid);

			codeDescriptionListControl.Setup(m => m.ReadOnly).Returns(false);
			codeDescriptionListControl.Setup(m => m.ReadOnly).Returns(true);
			codeDescriptionListControl.Setup(m => m.ReadOnly).Returns(false)
				.Callback(() => dummyGrid.ReadOnly = false);
			codeDescriptionListControl.Setup(m => m.Control).Returns(dummyControl);
			codeDescriptionListControl.Setup(m => m.Grid).Returns(dummyGrid);

			using (var dummyForm = new Form())
			using (var control = new AWBExtraTextControl(codeDescriptionListControl.Object))
			{
				dummyForm.Controls.Add(control);
				dummyForm.Show();

				ZButton viewButton = (ZButton)control.Controls["viewButton"];
				AssertEquals(false, viewButton.ReadOnly);

				control.ReadOnly = true;
				AssertEquals(true, control.ReadOnly);
				AssertEquals(true, viewButton.ReadOnly);

				control.ReadOnly = false;
				AssertEquals(false, control.ReadOnly);
				AssertEquals(false, viewButton.ReadOnly);

				codeDescriptionListControl.Verify(m => m.ReadOnly, Times.AtLeastOnce);
				codeDescriptionListControl.Verify(m => m.ReadOnly, Times.Once);
				codeDescriptionListControl.Verify(m => m.Control, Times.AtLeastOnce);
				codeDescriptionListControl.Verify(m => m.Grid, Times.AtLeastOnce());
			}

			dummyControl.Dispose();
			dummyGrid.Dispose();
		}

		public void TestViewButton()
		{
			var mapTreePresentationManager =
				new Mock<IMapTreePresentationManager>(MockBehavior.Loose) { CallBase = true };

			var registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);
			ICodeDescriptionListControl codeDescriptionListControl = new CodeDescriptionListEditControlForRegistry(registryItem, true, true, CharacterCasing.Normal, CharacterCasing.Normal,
				"Code", "Desc", 35);

			mapTreePresentationManager.Setup(m => m.ParentTypes).Returns((Type[])null);
			mapTreePresentationManager.Setup(m => m.GetUserSelectionMacro()).Returns("<dummy>");
			mapTreePresentationManager.Setup(m => m.Dispose());

			using (ObjectFactory.Substitute(mapTreePresentationManager.Object))
			using (Form dummyForm = new Form())
			using (var control = new AWBExtraTextControl(codeDescriptionListControl))
			{
				dummyForm.Controls.Add(control);
				dummyForm.Show();

				control.Data = new CodeDescriptionPairList();

				ZButton viewButton = (ZButton)control.Controls["viewButton"];
				viewButton.GotFocus += (s, e) => codeDescriptionListControl.Grid.Focus(); // PerformClick focuses the button but we need to ensure that entryPointIsKnown is set to true
				viewButton.PerformClick();

				AssertEquals("<dummy>", codeDescriptionListControl.Grid[0, 0]);

				codeDescriptionListControl.Grid[0, 1] = "description";

				ReadOnlyCodeDescriptionPairList data = control.Data;
				AssertEquals("<dummy> - description", data.ElementsAsString);
			}

			((Control)codeDescriptionListControl).Dispose();
			mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.AtLeastOnce);
			mapTreePresentationManager.Verify(m => m.Dispose(), Times.Once);
		}

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((AWBExtraTextControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new AWBExtraTextControl(new CodeDescriptionListEditControl());
		}
	}
}
