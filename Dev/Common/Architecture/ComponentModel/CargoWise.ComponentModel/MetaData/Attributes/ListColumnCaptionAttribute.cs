using System;

namespace CargoWise.ComponentModel
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class ListColumnCaptionAttribute : Attribute
	{
		public ListColumnCaptionAttribute(Type delegateType, string delegateName)
		{
			fullNameCaption = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), delegateType, delegateName);
		}

		readonly Func<string> fullNameCaption;
		public string Caption => fullNameCaption();
	}
}
