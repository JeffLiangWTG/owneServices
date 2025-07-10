using System;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this to a property (or validate method) to indicate the property is calculated from another
	/// property on the same instance.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
	public sealed class CalculatedFromAttribute : CalculatedFromAttributeBase
	{
		public CalculatedFromAttribute(string propertyName1)
			: this(new string[] { propertyName1 })
		{
		}

		public CalculatedFromAttribute(string propertyName1, string propertyName2)
			: this(new string[] { propertyName1, propertyName2 })
		{
		}

		public CalculatedFromAttribute(string propertyName1, string propertyName2, string propertyName3)
			: this(new string[] { propertyName1, propertyName2, propertyName3 })
		{
		}

		public CalculatedFromAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4)
			: this(new string[] { propertyName1, propertyName2, propertyName3, propertyName4 })
		{
		}

		public CalculatedFromAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4, string propertyName5)
			: this(new string[] { propertyName1, propertyName2, propertyName3, propertyName4, propertyName5 })
		{
		}

		public CalculatedFromAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4, string propertyName5, string propertyName6)
			: this(new string[] { propertyName1, propertyName2, propertyName3, propertyName4, propertyName5, propertyName6 })
		{
		}

		public CalculatedFromAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4, string propertyName5, string propertyName6, string propertyName7)
			: this(new string[] { propertyName1, propertyName2, propertyName3, propertyName4, propertyName5, propertyName6, propertyName7 })
		{
		}

		public CalculatedFromAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4, string propertyName5, string propertyName6, string propertyName7, string propertyName8)
			: this(new string[] { propertyName1, propertyName2, propertyName3, propertyName4, propertyName5, propertyName6, propertyName7, propertyName8 })
		{
		}

		public CalculatedFromAttribute(params string[] propertyNames)
			: base(propertyNames)
		{
			Argument.NotNull(propertyNames, nameof(propertyNames));
		}
	}
}
