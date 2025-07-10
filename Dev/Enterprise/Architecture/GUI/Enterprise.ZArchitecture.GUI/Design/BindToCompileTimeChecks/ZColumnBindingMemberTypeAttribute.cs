using System;

namespace Enterprise.ZArchitecture
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ZColumnBindingMemberTypeAttribute : Attribute
	{
		public ZColumnBindingMemberTypeAttribute(Type type)
		{
			this.Type = type;
		}

		public Type Type
		{
			get { return type; }
			set { type = value; }
		}
		Type type;
	}
}
