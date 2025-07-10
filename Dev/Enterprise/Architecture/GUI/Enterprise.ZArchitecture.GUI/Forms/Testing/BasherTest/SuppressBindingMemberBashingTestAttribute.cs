using System;
using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class SuppressBindingMemberBashingTestAttribute : Attribute
	{
		[DefaultValue(true)]
		public bool IncludingChildren
		{
			get { return includingChildren; }
			set { includingChildren = value; }
		}
		bool includingChildren = true;
	}
}
