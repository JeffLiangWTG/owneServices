using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Attribute to exclude property from being tested by TestRelatedBusinessObjects
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RelatedBusinessObjectTestExclude : Attribute
	{
		public RelatedBusinessObjectTestExclude(string comment)
		{
			Comment = comment;
		}

		public string Comment { get; private set; }
	}
}
