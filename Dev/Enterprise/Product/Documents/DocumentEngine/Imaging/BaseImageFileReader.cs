using CargoWise.Common;

namespace Enterprise.DocumentEngine.Imaging
{
	/// <summary>
	/// Base class for ImageFileReaders. Abstract.
	/// </summary>
	public abstract class BaseImageFileReader : Disposable, IImagePageSelectorProvider
	{
		protected BaseImageFileReader()
		{
		}

		public abstract IImagePageSelector PageSelector { get; }
	}
}
