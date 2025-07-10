namespace Enterprise.DocumentEngine
{
	using System;

	interface IDocumentRendererLogger : IDisposable
	{
		void Log(string message);
	}

	class DummyDocumentRendererLogger : IDocumentRendererLogger
	{
		void IDocumentRendererLogger.Log(string message)
		{
		}

		void IDisposable.Dispose()
		{
		}
	}
}