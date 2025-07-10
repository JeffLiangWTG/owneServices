using System;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DisableModifiableMemberAttribute : MetaDataMemberAttribute
	{
		public DisableModifiableMemberAttribute(string propertyName)
			: base(MetaDataTypes.DisableModifiable, propertyName)
		{
			Argument.NotNull(propertyName, nameof(propertyName));
		}
	}
}
