using System;
using System.Drawing;

namespace Enterprise.DocumentEngine.Imaging
{
	/*
	 * What is the purpose of this interface + impls? I fail to see the benefit it provides, and it is an awkward abstraction for what its trying to achieve
	 */
	public interface IImagePageSelector : IImagePageSelectorProvider, IDisposable
	{
		int TotalPages { get; }
		/// <summary>
		/// Returns the current page index. 
		/// -1 implies no image.
		/// Valid range is 0 to TotalPages - 1.
		/// </summary>
		int CurrentPageIndex { get; set; }
		Image CurrentImage { get; }
	}
}
