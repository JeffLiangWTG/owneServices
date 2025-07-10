using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public sealed class CachingTaskCardRenderer : IDisposable, ITaskCardRenderer
	{
		public CachingTaskCardRenderer(ITaskCardRenderer wrappedRenderer)
		{
			innerRenderer = wrappedRenderer;
		}

		readonly ITaskCardRenderer innerRenderer;
		readonly Dictionary<ZString, CardRenderTemplatePair> templates = new Dictionary<ZString, CardRenderTemplatePair>();

		public TaskCardControl ConstructTaskCardControl(KContextMenuStrip menuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache)
		{
#if WINZOR

			CardBitmaps bitmaps = null;

#else

			var bitmaps = viewModel.GetOrCreateCardBitmaps(cardContent, () =>
			{
				var renderTemplate = GetCardRenderTemplate(menuStrip, viewModel, cardContent);

				try
				{
					renderTemplate.Bindable.SuspendLayout();
					renderTemplate.Model.CardContent = cardContent;
					renderTemplate.Model.Cell = cell;
					renderTemplate.CustomisedTemplate.NoteTextLabel.Text = cardContent.NoteText;
					renderTemplate.CustomisedTemplate.SetBorderStyle(cardContent);
					var backColour = TagProvider.GetBackgroundColor(cardContent.Definitions, cardContent.ApplicableTagMagnitudes);
					renderTemplate.CustomisedTemplate.BackColor = backColour ?? renderTemplate.Model.Customisation.BackgroundColorValue;
					renderTemplate.Bindable.SetDataBinding(cardContent.Bindable, string.Empty);
				}
				finally
				{
					renderTemplate.Bindable.ResumeLayout();
				}

				return renderTemplate.CustomisedTemplate.CreateCardBitmaps(cardContent, isTemplateTaskCard: true);
			});

#endif

			return innerRenderer.ConstructTaskCardBasedOnTemplate(menuStrip, cardContent, viewModel, cell, shouldEnableDragDrop: true, canUseBitmapCache: true, renderAsBitmapOnLoad: false, model: null, bitmaps: bitmaps);
		}

#if !WINZOR
		CardRenderTemplate GetCardRenderTemplate(KContextMenuStrip menuStrip, BMBoardSectionViewModel viewModel, ICardContent cardContent)
		{
			var key = cardContent.WorkflowType;
			if (!templates.TryGetValue(key, out CardRenderTemplatePair templatePair))
			{
				templatePair = templates[key] = new CardRenderTemplatePair();
			}

			return templatePair.GetCardRenderTemplate(menuStrip, viewModel, cardContent, innerRenderer);
		}
#endif

		public void OnRenderingCardsCompleted()
		{
			new DisposableList(templates.Values).Dispose();
			templates.Clear();
		}

		public void Dispose()
		{
			OnRenderingCardsCompleted();
		}

		TaskCardControl ITaskCardRenderer.ConstructTaskCardBasedOnTemplate(KContextMenuStrip strip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
		{
			throw new InvalidOperationException("Caches shouldn't wrap around caches.");
		}
	}

	class CardRenderTemplatePair : IDisposable
	{
		internal CardRenderTemplate GetCardRenderTemplate(KContextMenuStrip menuStrip, BMBoardSectionViewModel viewModel, ICardContent content, ITaskCardRenderer innerRenderer)
		{
			// This is the dirty place where we differentiate between cards having different sizes because of their note text.
			// If other ways of differentiating size appear, this needs refactoring.
			if (content.NoteText.IsEmpty)
			{
				return WithoutNote ?? (WithoutNote = new CardRenderTemplate(menuStrip, viewModel, content, innerRenderer));
			}
			else
			{
				return WithNote ?? (WithNote = new CardRenderTemplate(menuStrip, viewModel, content, innerRenderer));
			}
		}

		CardRenderTemplate WithNote { get; set; }
		CardRenderTemplate WithoutNote { get; set; }

		public void Dispose()
		{
			if (WithNote != null)
			{
				WithNote.Dispose();
			}
			if (WithoutNote != null)
			{
				WithoutNote.Dispose();
			}
		}
	}

	class CardRenderTemplate : IDisposable
	{
		public CardRenderTemplate(KContextMenuStrip menuStrip, BMBoardSectionViewModel viewModel, ICardContent cardContent, ITaskCardRenderer innerRenderer)
		{
			var cellStub = new CellContent(-1, -1, CellContentType.Cards);

			Model = new ControlCustomisationViewModel(viewModel.GetSummaryCard(cardContent.WorkflowType), viewModel, cardContent, cellStub);
			CustomisedTemplate = innerRenderer.ConstructTaskCardBasedOnTemplate(menuStrip, cardContent, viewModel, cellStub, shouldEnableDragDrop: false, canUseBitmapCache: false, renderAsBitmapOnLoad: false, model: Model, bitmaps: null);
			Bindable = CustomisedTemplate.Controls.OfType<ZUserControl>().Single();
			CustomisedTemplate.ReverseControlOrder();
		}

		public ControlCustomisationViewModel Model { get; private set; }
		public TaskCardControl CustomisedTemplate { get; private set; }
		public ZUserControl Bindable { get; private set; }

		public void Dispose()
		{
			if (CustomisedTemplate != null)
			{
				CustomisedTemplate.Dispose();
			}
		}
	}
}
