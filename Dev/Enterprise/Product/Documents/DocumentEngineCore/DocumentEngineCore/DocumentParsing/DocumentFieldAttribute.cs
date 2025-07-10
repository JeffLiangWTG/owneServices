using System;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method , AllowMultiple = false)]
	public class DocumentFieldAttribute : Attribute
	{
		public DocumentFieldAttribute(string userVisibleDescription)
		{
			this.userVisibleDescription = userVisibleDescription;
		}
		readonly string userVisibleDescription;

		public string UserVisibleDescription
		{
			get { return userVisibleDescription; }
		}
	}
}