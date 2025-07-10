using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	static class EventExtensions
	{
		public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, IDocument document) where T : IDocumentAwareEvent
		{
			return source
				.Subscribe(t =>
				{
					if (t?.Document == document)
					{
						onNext(t);
					}
				});
		}
	}
}
