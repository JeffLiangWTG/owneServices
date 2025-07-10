using System;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class UnsupportedImagePageCompressionException : UnsupportedDocumentException
	{
		public UnsupportedImagePageCompressionException(Exception ex, string fileName)
			: base(ResString.GetMultilingualString("5c5149bd-ba86-4417-8d6f-8fa01e81ea55", "using an unsupported compression on some pages"), String.Format(CultureInfo.InvariantCulture, (NoResString)"Some pages in the image file ({0}) have an unsupported compression type. Please make sure that all pages of the image have the same compression.", fileName), ex)
		{ }

#if NETFRAMEWORK
		protected UnsupportedImagePageCompressionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
