using System;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public readonly struct EMCSResponseDetail
	{
		public Type XmlObjectType { get; }

		public Type ProcessorType { get; }

		public EMCSResponseDetail(Type xmlObjectType, Type processorType)
		{
			XmlObjectType = xmlObjectType;
			ProcessorType = processorType;
		}
	}
}
