using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class UniversalDataContextAttribute : Attribute
	{
		public UniversalDataContextAttribute(DataContextType dataContextType)
		{
			this.DataContextType = dataContextType;
		}

		public DataContextType DataContextType { get; private set; }
	}
}
