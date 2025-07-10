using System;
using System.Reflection;

namespace Enterprise.DocumentVisualizer.Integration
{
	public static class Extensions
	{
		public static bool IsSupportedByDocumentVisualizer(this object obj) => obj?.GetType().GetCustomAttribute<VisualizableDocumentsSupportableAttribute>() != null;

		public static IVisualizableDocumentSupporter GetSupporter(this object obj)
		{
			if (obj == null)
			{
				return null;
			}

			var objType = obj.GetType();

			var attrib = objType.GetCustomAttribute<VisualizableDocumentsSupportableAttribute>();

			if (attrib == null)
			{
				return null;
			}

			var ctor = attrib.SupporterType.GetConstructor(new[] { objType });

			var supporter = ctor != null
				? (IVisualizableDocumentSupporter)Activator.CreateInstance(attrib.SupporterType, obj)
				: (IVisualizableDocumentSupporter)Activator.CreateInstance(attrib.SupporterType);

			return supporter;
		}
	}
}
