using System;
using System.Linq;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Business.Binding;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class PageViewPresenter : IPageViewPresenter
	{
		public PageViewPresenter(IDocument document, IServiceContainer services)
		{
			this.document = document ?? throw new ArgumentNullException(nameof(document));
			this.services = services ?? throw new ArgumentNullException(nameof(services));
		}

		readonly IDocument document;
		readonly IServiceContainer services;

		IPageView view;

		public bool IsDocumentTranslatable => document.IsTranslatable;

		public void Init(IPageView pageView)
		{
			this.view = pageView;
		}

		#region Events

		IEventBroker EventBroker => broker ?? (broker = services.Resolve<IEventBroker>());
		IEventBroker broker;

		#endregion

		#region Editing

		public void NotifyShowingContextMenu()
		{
			EventBroker.Publish(new EndEditEvent(document));
		}

		public void BeginEdit(IDynamicContentLayoutElement content, EditTrigger trigger)
		{
			if (content == null)
			{
				return;
			}

			var beforeEditEvent = new BeforeEditEvent(document, content);
			EventBroker.Publish(beforeEditEvent);

			var bindingSources = content.
				EditableData
				.Select(data => CreateMacroBusinessObjectProperty(data.Key, data.Value))
				.ToArray();

			if (!bindingSources.Any())
			{
				return;
			}

			var editorBuilder = services.Resolve<IEditorBuildService>();

			var presenter = new EditorPresenter(document, content, services.Resolve<IEventBroker>());

			var editor = editorBuilder.Build(content,
				presenter,
				bindingSources,
				services.Resolve<IDocumentSettingsProviderService>().Scale);

			if (editor == null)
			{
				return;
			}

			var editEvent = new BeginEditEvent(document, content);
			EventBroker.Publish(editEvent);

			view.ShowEditor(editor);

			editor.BeginEdit();

			if (!editor.WaitForUserInput)
			{
				switch (trigger)
				{
					case EditTrigger.User:
						editor.EndEdit();
						EventBroker.Publish(new EndEditEvent(document, content));
						break;

					case EditTrigger.TabForward:
						EventBroker.Publish(new MoveToNextEditableElementEvent(document, content, MoveToNextDirection.Forward));
						break;

					case EditTrigger.TabBackwards:
						EventBroker.Publish(new MoveToNextEditableElementEvent(document, content, MoveToNextDirection.Backward));
						break;
				}
			}
		}

		IMacroBusinessObjectProperty CreateMacroBusinessObjectProperty(string name, IDynamicData dynamicData)
		{
			var bindTo = dynamicData.GetMetaData<string>(MetaDataType.BindTo);
			IMacroBusinessObjectProperty macroBusinessObjectProperty = null;

			if (!string.IsNullOrWhiteSpace(bindTo)
				&& dynamicData.GetDynamicProperty(bindTo) is IDynamicData bindToProperty)
			{
				macroBusinessObjectProperty = new MacroBusinessObjectProperty(bindTo, bindToProperty);
			}
			else
			{
				macroBusinessObjectProperty = new MacroBusinessObjectProperty(name, dynamicData);
			}

			if (macroBusinessObjectProperty.PropertyType == typeof(UXmlDateTime))
			{
				macroBusinessObjectProperty = new UXmlDateTimeMacroBusinessObjectProperty(macroBusinessObjectProperty);
			}

			return macroBusinessObjectProperty;
		}

		public void CancelOverride(IDynamicContentLayoutElement element)
		{
			if (element != null)
			{
				element.CancelOverride();
				EventBroker.Publish(new ValueChangedEvent(document));
			}
		}

		#endregion

		#region IDisposable members

		public void Dispose()
		{
		}

		#endregion
	}
}
