using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.Visualisation
{
	class AreaVisualisationManager
	{
		public AreaVisualisationManager(Area area, RendererGeneralGetter rendererGetter)
		{
			if (area == null)
			{
				throw new ArgumentNullException(nameof(area), "Argument [Area area] must not be null.");
			}
			if (rendererGetter == null)
			{
				throw new ArgumentNullException(nameof(rendererGetter), "Argument [RendererGeneralGetter rendererGetter] must not be null.");
			}
			this.Area = area;
			this.rendererGetter = rendererGetter;
		}
		public readonly Area Area;
		readonly RendererGeneralGetter rendererGetter;

		bool? shouldShowInVisualiser;
		public bool ShouldShowInVisualiser
		{
			get { return shouldShowInVisualiser ?? !Area.ShouldDelete; }
			set { shouldShowInVisualiser = value; }
		}

		public RendererGeneral VisualisationRenderer
		{
			get { return visualisationRenderer ?? (visualisationRenderer = GetRenderer()); }
		}
		RendererGeneral visualisationRenderer;

		RendererGeneral GetRenderer()
		{
			return rendererGetter() ?? throw new ArgumentNullException("rendererGetter()", "The delegate passed into the constructor of the AreaVisualisationManager must never return null.");
		}

		public List<VisualiserComponent> Components
		{
			get { return fComponents ?? (fComponents = new List<VisualiserComponent>()); }
		}
		List<VisualiserComponent> fComponents;
	}

	delegate RendererGeneral RendererGeneralGetter();
}
