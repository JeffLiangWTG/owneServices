using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class PageViewPresenterTest : TestCaseWithFactory
	{
		#region TestShowingPopupMenuDoesNotCreateAdditionalControls

		public void TestShowingPopupMenuDoesNotCreateAdditionalControls()
		{
			var document = new DummyDocument();
			var data = new ZString().MakeDynamic();

			var cell = new DummyCell();
			cell.EditableData.Add("data", data);
			cell.Scope = new MacroScope(data);

			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IEditorBuildService>(new DynamicContentEditorBuildService());
			services.Register<IDocumentSettingsProviderService>(new DocumentSettingsProviderService(() => 20f));

			var size = new SizeF(100f, 100f);
			var presenter = new PageViewPresenter(document, services);

			using (var pageView = new PageView(size, presenter))
			{
				AssertEquals(0, pageView.Controls.Count);

				var element = new DynamicContent(cell, new PointF(3f, 3f), new SizeF(10f, 10f));
				var content = new DynamicContentLayoutElement(element);

				IEnumerable<string> GetChildControlTypeNames()
				{
					foreach (Control control in pageView.Controls)
					{
						yield return control.GetType().Name;
					}
				}

				presenter.BeginEdit(content, EditTrigger.User);

				AssertContainsExactElementsInAnyOrder("control has been created",
					new[]
					{
						nameof(InPlaceDynamicContentControl)
					},
					GetChildControlTypeNames());

				presenter.NotifyShowingContextMenu();

				presenter.BeginEdit(content, EditTrigger.User);

				AssertContainsExactElementsInAnyOrder("no new controls have been created",
					new[]
					{
						nameof(InPlaceDynamicContentControl)
					},
					GetChildControlTypeNames());
			}
		}

		#endregion

		#region TestCreateControlForPropertyOfTypeCodeDescription

		public void TestCreateControlForPropertyOfTypeCodeDescription()
		{
			var document = new DummyDocument();

			var codeDescription = Factory.New<DummyBusinessObject>().MakeDynamic();
			codeDescription.SetMetaData(MetaDataType.BindTo, nameof(DummyBusinessObject.Z0_Code));

			var cell = new DummyCell();
			cell.EditableData.Add("data", codeDescription);
			cell.Scope = new MacroScope(codeDescription);

			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IEditorBuildService>(new DynamicContentEditorBuildService());
			services.Register<IDocumentSettingsProviderService>(new DocumentSettingsProviderService(() => 20f));

			var size = new SizeF(100f, 100f);
			var presenter = new PageViewPresenter(document, services);

			using (var pageView = new PageView(size, presenter))
			{
				AssertEquals(0, pageView.Controls.Count);

				var element = new DynamicContent(cell, new PointF(3f, 3f), new SizeF(10f, 10f));
				var content = new DynamicContentLayoutElement(element);

				presenter.BeginEdit(content, EditTrigger.User);

				IEnumerable<string> GetChildControlTypeNames()
				{
					foreach (Control control in pageView.Controls)
					{
						yield return control.GetType().Name;
					}
				}

				AssertContainsExactElementsInAnyOrder("control has been created",
					new[]
					{
						nameof(InPlaceDynamicContentControl)
					},
					GetChildControlTypeNames());
			}
		}

		#endregion

		#region TestBeginEditEvent

		public void TestBeginEditEvent_CellHasEditableData() => AssertBeginEditEvent(true);
		public void TestBeginEditEvent_CellDoesNotHaveEditableData() => AssertBeginEditEvent(false);

		void AssertBeginEditEvent(bool cellHasEditableData)
		{
			var document = new DummyDocument();
			var data = new object().MakeDynamic();

			var cell = new DummyCell();
			if (cellHasEditableData)
			{
				cell.EditableData.Add("data", data);
			}
			cell.Scope = new MacroScope(data);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IEditorBuildService>(new DynamicContentEditorBuildService());
			services.Register<IDocumentSettingsProviderService>(new DocumentSettingsProviderService(() => 20f));

			var size = new SizeF(100f, 100f);
			var presenter = new PageViewPresenter(document, services);

			using (var pageView = new PageView(size, presenter))
			{
				AssertEquals(0, pageView.Controls.Count);

				var element = new DynamicContent(cell, new PointF(3f, 3f), new SizeF(10f, 10f));
				var content = new DynamicContentLayoutElement(element);

				var eventPublished = false;
				broker.GetEvent<BeginEditEvent>().Subscribe(e => { eventPublished = true; });

				presenter.BeginEdit(content, EditTrigger.User);
				AssertEquals("BeginEdit publishing", cellHasEditableData, eventPublished);
			}
		}

		#endregion

		#region TestControlTypeIsZDateEdit

		public void TestControlTypeIsZDateEdit()
		{
			var document = new DummyDocument();

			var datetime = new UXmlDateTime(ZDateTime.Now).MakeDynamic();
			datetime.SetMetaData(MetaDataType.BindTo, nameof(datetime));

			var cell = new DummyCell();
			cell.EditableData.Add("data", datetime);
			cell.Scope = new MacroScope(datetime);

			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IEditorBuildService>(new DynamicContentEditorBuildService());
			services.Register<IDocumentSettingsProviderService>(new DocumentSettingsProviderService(() => 20f));

			var size = new SizeF(100f, 100f);
			var presenter = new PageViewPresenter(document, services);

			using (var pageView = new PageView(size, presenter))
			{
				AssertEquals(0, pageView.Controls.Count);

				var element = new DynamicContent(cell, new PointF(3f, 3f), new SizeF(10f, 10f));
				var content = new DynamicContentLayoutElement(element);

				presenter.BeginEdit(content, EditTrigger.User);

				var inPlaceDynamicContentControl = pageView.Controls.OfType<InPlaceDynamicContentControl>().SingleOrDefault();
				AssertNotNull("control contains InPlaceDynamicContentControl", inPlaceDynamicContentControl);

				var dateEditControl = inPlaceDynamicContentControl.Controls.OfType<ZDateEdit>().SingleOrDefault();
				AssertNotNull("control contains ZDateEdit", dateEditControl);
			}
		}

		#endregion

		#region TestControlTypeIsZDateTimeOffsetEdit

		public void TestControlTypeIsZDateTimeOffsetEdit()
		{
			var document = new DummyDocument();

			var datetime = new UXmlDateTime(ZDateTimeOffset.Now).MakeDynamic();
			datetime.SetMetaData(MetaDataType.BindTo, nameof(datetime));

			var cell = new DummyCell();
			cell.EditableData.Add("data", datetime);
			cell.Scope = new MacroScope(datetime);

			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IEditorBuildService>(new DynamicContentEditorBuildService());
			services.Register<IDocumentSettingsProviderService>(new DocumentSettingsProviderService(() => 20f));

			var size = new SizeF(100f, 100f);
			var presenter = new PageViewPresenter(document, services);

			using (var pageView = new PageView(size, presenter))
			{
				AssertEquals(0, pageView.Controls.Count);

				var element = new DynamicContent(cell, new PointF(3f, 3f), new SizeF(10f, 10f));
				var content = new DynamicContentLayoutElement(element);

				presenter.BeginEdit(content, EditTrigger.User);

				var inPlaceDynamicContentControl = pageView.Controls.OfType<InPlaceDynamicContentControl>().SingleOrDefault();
				AssertNotNull("control contains InPlaceDynamicContentControl", inPlaceDynamicContentControl);

				var dateEditControl = inPlaceDynamicContentControl.Controls.OfType<ZDateTimeOffsetEdit>().SingleOrDefault();
				AssertNotNull("control contains ZDateTimeOffsetEdit", dateEditControl);
			}
		}

		#endregion
	}
}
