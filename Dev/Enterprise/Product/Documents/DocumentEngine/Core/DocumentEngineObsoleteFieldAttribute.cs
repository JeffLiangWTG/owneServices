using System;

namespace Enterprise.DocumentEngine
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class DocumentEngineObsoleteField : Attribute
	{
		public DocumentEngineObsoleteField(string messageToShowToDeveloper)
		{
			this.MessageToShowToDeveloper = messageToShowToDeveloper;
		}

		public readonly string MessageToShowToDeveloper;
	}
}
