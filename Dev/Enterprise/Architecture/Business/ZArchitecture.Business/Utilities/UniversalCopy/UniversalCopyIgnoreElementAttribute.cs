using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class UniversalCopyIgnoreElementAttribute : Attribute
	{
		public UniversalCopyIgnoreElementAttribute(params string[] elementNames)
		{
			this.elementNames = new ReadOnlyCollection<string>(Argument.NotNull(elementNames, "elementNames"));
		}

		public ReadOnlyCollection<string> ElementNames
		{
			get { return elementNames; }
		}
		readonly ReadOnlyCollection<string> elementNames;
	}
}
