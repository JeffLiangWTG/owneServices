using System;
using CargoWise.Common;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DocumentSettingsProviderService : IDocumentSettingsProviderService
	{
		public DocumentSettingsProviderService(Func<float> scaleProvider)
		{
			Argument.NotNull(scaleProvider, nameof(scaleProvider));

			this.scaleProvider = scaleProvider;
		}

		readonly Func<float> scaleProvider;

		public float Scale
		{
			get { return scaleProvider(); }
		}
	}
}