using System;

namespace Enterprise.DocumentScanning.Integration
{
	[Serializable]
	public class EmptyContentEDocsException : Exception
	{
		public override string Message => Res.GetString("44E64766-B7E3-40B3-9358-CB019F5285E3", "The eDoc content cannot be empty.");

		public EmptyContentEDocsException() : base()
		{
		}

#if NETFRAMEWORK
		protected EmptyContentEDocsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
