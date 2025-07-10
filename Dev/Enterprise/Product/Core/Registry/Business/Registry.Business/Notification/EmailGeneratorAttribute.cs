using System;

namespace Enterprise.Registry.Business
{
	public class EmailGeneratorAttribute : Attribute
	{
		public readonly Type Type;

		public EmailGeneratorAttribute(Type type)
		{
			Type = type;
		}
	}
}
