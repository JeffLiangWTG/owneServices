using System;

namespace Enterprise.Customs.IE.Business
{
	public class ResponseDetail
	{
		public Type XmlObjectType { get; }

		public Type ProcessorType { get; }

		public ResponseDetail(Type xmlObjectType, Type processorType)
		{
			XmlObjectType = xmlObjectType;
			ProcessorType = processorType;
		}

		public static ResponseDetail Empty => empty ?? (empty = new ResponseDetail(null, null));
		[ThreadStatic]
		static ResponseDetail empty;
	}
}
