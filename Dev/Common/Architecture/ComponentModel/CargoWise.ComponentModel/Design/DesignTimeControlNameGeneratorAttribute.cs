using System;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Apply this attribute to a control class to control automatic the control name prefix prepending.
	/// For example, you might apply [DesignTimeControlNameGenerator("txt")] to a text box.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DesignTimeControlNameGeneratorAttribute : Attribute
	{
		public DesignTimeControlNameGeneratorAttribute(string prefix)
		{
			this.prefix = prefix;
		}

		public string GenerateNameFromBindingMember(string bindingMember)
		{
			Argument.NotNull(bindingMember, nameof(bindingMember)); // Suggested By ReviewBot 
			return Prefix + bindingMember.Replace(".", "_").Replace("+", "_");
		}

		public string Prefix
		{
			get { return prefix; }
		}
		readonly string prefix;
	}
}
