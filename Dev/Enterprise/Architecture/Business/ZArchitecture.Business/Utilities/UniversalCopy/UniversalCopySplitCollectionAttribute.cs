using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	/// <summary>
	/// Defines filtered subset of collection that should be copied with separate configuration.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public sealed class UniversalCopySplitCollectionAttribute : Attribute
	{
		/// <summary>
		/// Defines filtered subset of collection that should be copied with separate configuration.
		/// </summary>
		/// <param name="name">Name of subset collection.</param>
		/// <param name="filter">Filter expression.</param>
		public UniversalCopySplitCollectionAttribute(string name, string filter)
		{
			Name = name;
			Filter = filter;
		}

		public string Name { get; private set; }
		public string Filter { get; private set; }
	}
}
