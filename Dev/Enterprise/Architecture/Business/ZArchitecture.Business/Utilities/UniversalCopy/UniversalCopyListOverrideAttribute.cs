using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyListOverrideAttribute : Attribute
	{
		public UniversalCopyListOverrideAttribute(string dataPath)
		{
			DataPath = dataPath;
		}

		public string DataPath { get; }
	}
}
