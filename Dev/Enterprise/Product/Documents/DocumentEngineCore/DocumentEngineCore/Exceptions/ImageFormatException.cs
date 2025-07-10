using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Exceptions
{
	[Serializable]
	public class ImageFormatException : Exception
	{
		public ImageFormatException()
			: base((NoResString)"The image data stream supplied does not contain a valid image format.")
		{
		}

#if NETFRAMEWORK
		protected ImageFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
