using System;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZFormGlobalStrategies : IDisposable
	{
		#region Constructors
		internal ZFormGlobalStrategies()
		{
			if (!isInitialized)
			{
				Initialize();
			}

			creatingStrategy = new ZFormCreatingStrategy();
			disposingStrategy = new ZFormDisposingStrategy();
			captionStrategy = new ZFormCaptionStrategy();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "important that this is constructed, even though it is never referenced")]
		static readonly ZFormGlobalStrategies instance = new();

		readonly ZFormCreatingStrategy creatingStrategy;
		readonly ZFormDisposingStrategy disposingStrategy;
		readonly ZFormCaptionStrategy captionStrategy;

		/// <summary>
		/// For load type, run static constructor, and initialize form strategies.
		/// </summary>
		public static void Initialize()
		{
			isInitialized = true;
		}

		[ThreadStatic]
		static bool isInitialized;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				creatingStrategy.Dispose();
				disposingStrategy.Dispose();
				captionStrategy.Dispose();
			}
		}

		#endregion
	}
}
