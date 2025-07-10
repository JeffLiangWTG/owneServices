using System;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Application
{
	[Immutable]
	public class ObjectHandle
	{
		protected ObjectHandle()
		{
			objectName = string.Empty;
		}

		internal ObjectHandle(string objectName)
		{
			Argument.NotNull(objectName, nameof(objectName));
			this.objectName = objectName;
		}

		public virtual Type GetObjectType()
		{
			return ObjectFactory.GetTypeWithoutSecurityCheck(typeof(ObjectFactory.EmptyType), objectName);
		}

		public virtual object GetObject()
		{
			return ObjectFactory.GetWithoutSecurityCheck(objectName);
		}

		public virtual object GetObject(params object[] arguments)
		{
			return ObjectFactory.GetWithoutSecurityCheck(objectName, arguments);
		}

		readonly string objectName;
	}
}
