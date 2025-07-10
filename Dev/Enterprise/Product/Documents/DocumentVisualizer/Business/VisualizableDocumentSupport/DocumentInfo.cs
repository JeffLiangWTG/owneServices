using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class DocumentInfo : IDocumentInfo
	{
		public DocumentInfo(Lazy<ITemplate> templateProvider,
			Lazy<IVisualizerDocumentData> documentDataProvider,
			Lazy<IServiceContainer> servicesProvider,
			Lazy<IDocumentDescriptor> documentDescriptorProvider,
			Lazy<IDocument> documentProvider)
		{
			this.templateProvider = templateProvider;
			this.documentDataProvider = documentDataProvider;
			this.servicesProvider = servicesProvider;
			this.documentDescriptorProvider = documentDescriptorProvider;
			this.documentProvider = documentProvider;
		}

		readonly Lazy<ITemplate> templateProvider;
		readonly Lazy<IVisualizerDocumentData> documentDataProvider;
		readonly Lazy<IServiceContainer> servicesProvider;
		readonly Lazy<IDocumentDescriptor> documentDescriptorProvider;
		readonly Lazy<IDocument> documentProvider;

		public IDocument Document => documentProvider.Value;
		public IDocumentDescriptor Descriptor => documentDescriptorProvider.Value;
		public IServiceContainer Services => servicesProvider.Value;
		public IVisualizerDocumentData DocumentData => documentDataProvider.Value;
		public ITemplate Template => templateProvider.Value;
	}
}