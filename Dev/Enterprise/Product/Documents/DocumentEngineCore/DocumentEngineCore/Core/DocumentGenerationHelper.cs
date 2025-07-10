using System;
using CargoWise.Common;

namespace Enterprise.DocumentEngineCore
{
	public static class DocumentGenerationHelper
	{
		[ThreadStatic]
		static bool isGeneratingDocument;
		public static bool IsGeneratingDocument
		{
			get { return isGeneratingDocument; }
			private set { isGeneratingDocument = value; }
		}

		public static IDisposable SetIsGeneratingDocument()
		{
			IsGeneratingDocument = true;
			DisposableAction disposableAction = new DisposableAction(() => IsGeneratingDocument = false);

			return disposableAction;
		}
	}
}
