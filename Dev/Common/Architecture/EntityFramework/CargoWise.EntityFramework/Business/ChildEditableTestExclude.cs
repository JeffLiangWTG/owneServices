using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ChildEditableTestExclude : Attribute
	{
		public ChildEditableTestExclude()
		{
		}
	}
}
