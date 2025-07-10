using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class CustomisationLinePropertyList : PropertyNameList
	{
		public CustomisationLinePropertyList(string propertySource)
			: base(GetPropertySourceType(propertySource))
		{
		}

		static Type GetPropertySourceType(string propertySource)
		{
			switch (propertySource)
			{
				case PropertySourceList.Codes.ProcessTask:
					return typeof(ProcessTask);

				case PropertySourceList.Codes.Workflow:
					return typeof(ProcessHeader);

				default:
					return null;
			}
		}
	}
}
