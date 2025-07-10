using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.DocDataObjects.Testing;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DynamicContentEditorBuildServiceTest : TestCaseWithFactory
	{
		#region TestBuildFindBoxWithCustomLookup

		public void TestBuildFindBoxWithCustomLookup()
		{
			var customPopupWasShown = false;

			var popup = new DummyCustomFindBoxPopup();
			popup.ShownModalFormInvoker += (findBox, form) =>
			{
				Assert("VisualizerCodeFindBox is passed to custom popup", findBox is VisualizerCodeFindBox);
				customPopupWasShown = true;
			};

			using (ObjectFactory.Substitute<DummyCustomFindBoxPopup>(() => popup))
			{
				var content = new Mock<IDynamicContentLayoutElement>();
				var editorPresenter = new Mock<IEditorPresenter>();

				var property = new Mock<IMacroBusinessObjectProperty>();
				property.SetupGet(p => p.PropertyName).Returns("Code");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				var dummy = new DummyDocDataObject();
				var lookup = new DummyNonPersistentBusinessObjectCollection(Factory);
				dummy.CodeList = lookup;

				var dynamicData = dummy.MakeDocDataDynamic();
				var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));

				property.SetupGet(p => p.MetaData).Returns(code.GetMetaData());

				var editorService = new DynamicContentEditorBuildService();

				using (var editorView = editorService.Build(content.Object, editorPresenter.Object, new[] { property.Object }, 1f))
				{
					if (editorView is InPlaceDynamicEditorView inPlaceView
						&& inPlaceView.Control is InPlaceDynamicContentControl contentControl)
					{
						var codeBox = contentControl.Controls.OfType<VisualizerCodeFindBox>().SingleOrDefault();

						AssertNotNull("control contains VisualizerCodeFindBox", codeBox);

						codeBox.SelectFromPopupForm();

						Assert("Custom popup was shown", customPopupWasShown);
					}
					else
					{
						Fail($"Expected {nameof(InPlaceDynamicEditorView)} with {nameof(InPlaceDynamicContentControl)}");
					}
				}
			}
		}

		public void TestBuildFindBoxWithCustomLookup_ShowDescriptionAttribute()
		{
			var popup = new DummyCustomFindBoxPopup();

			using (ObjectFactory.Substitute<DummyCustomFindBoxPopup>(() => popup))
			{
				var content = new Mock<IDynamicContentLayoutElement>();
				var editorPresenter = new Mock<IEditorPresenter>();

				var property = new Mock<IMacroBusinessObjectProperty>();
				property.SetupGet(p => p.PropertyName).Returns("Code");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				var dummy = new DummyDocDataObjectWithFindBoxWithoutDescription();
				var lookup = new DummyNonPersistentBusinessObjectCollection(Factory);
				dummy.CodeList = lookup;

				var dynamicData = dummy.MakeDocDataDynamic();
				var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));

				property.SetupGet(p => p.MetaData).Returns(code.GetMetaData());

				var editorService = new DynamicContentEditorBuildService();

				using (var editorView = editorService.Build(content.Object, editorPresenter.Object, new[] { property.Object }, 1f))
				{
					if (editorView is InPlaceDynamicEditorView inPlaceView
						&& inPlaceView.Control is InPlaceDynamicContentControl contentControl)
					{
						var codeBox = contentControl.Controls.OfType<VisualizerCodeFindBox>().SingleOrDefault();

						AssertNotNull("control contains VisualizerCodeFindBox", codeBox);
						Assert("Custom popup was shown", !codeBox.ShowDescriptionBox);
						Assert("CodeBox is editable", !codeBox.ReadOnly);
					}
					else
					{
						Fail($"Expected {nameof(InPlaceDynamicEditorView)} with {nameof(InPlaceDynamicContentControl)}");
					}
				}
			}
		}

		public void TestBuildFindBoxWithCustomLookup_ReadOnly()
		{
			var popup = new DummyCustomFindBoxPopup();

			using (ObjectFactory.Substitute<DummyCustomFindBoxPopup>(() => popup))
			{
				var content = new Mock<IDynamicContentLayoutElement>();
				var editorPresenter = new Mock<IEditorPresenter>();

				var property = new Mock<IMacroBusinessObjectProperty>();
				property.SetupGet(p => p.PropertyName).Returns("Code");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				var dummy = new DummyDocDataObject();
				dummy.Code_DisableModifiable = true;
				var lookup = new DummyNonPersistentBusinessObjectCollection(Factory);
				dummy.CodeList = lookup;

				var dynamicData = dummy.MakeDocDataDynamic();
				var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));

				property.SetupGet(p => p.MetaData).Returns(code.GetMetaData());

				var editorService = new DynamicContentEditorBuildService();

				using (var editorView = editorService.Build(content.Object, editorPresenter.Object, new[] { property.Object }, 1f))
				{
					if (editorView is InPlaceDynamicEditorView inPlaceView && inPlaceView.Control is InPlaceDynamicContentControl contentControl)
					{
						var codeBox = contentControl.Controls.OfType<VisualizerCodeFindBox>().SingleOrDefault();
						AssertNotNull("control contains VisualizerCodeFindBox", codeBox);
						Assert("CodeBox is read-only", codeBox.ReadOnly);
					}
					else
					{
						Fail($"Expected {nameof(InPlaceDynamicEditorView)} with {nameof(InPlaceDynamicContentControl)}");
					}
				}
			}
		}

		#endregion
	}
}
