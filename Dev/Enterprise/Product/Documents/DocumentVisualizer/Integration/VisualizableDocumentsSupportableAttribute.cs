using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;

namespace Enterprise.DocumentVisualizer.Integration
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class VisualizableDocumentsSupportableAttribute : Attribute
	{
		public VisualizableDocumentsSupportableAttribute(Type supporterType)
		{
			Argument.NotNull(supporterType, nameof(supporterType));
			SupporterType = ThrowIfInvalidSupporterType(supporterType, nameof(supporterType));
		}

		public VisualizableDocumentsSupportableAttribute(string supporterTypeName)
		{
			Argument.NotNullOrEmpty(supporterTypeName, nameof(supporterTypeName));

			var supporterType = ObjectFactory.GetType(supporterTypeName);
			SupporterType = ThrowIfInvalidSupporterType(supporterType, nameof(supporterTypeName));
		}

		Type ThrowIfInvalidSupporterType(Type type, string argumentName)
		{
			if (type == null
				|| type.GetInterfaces().All(i => i != typeof(IVisualizableDocumentSupporter)))
			{
				throw new ArgumentException($"The given argument is not {nameof(IVisualizableDocumentSupporter)}", argumentName);
			}

			return type;
		}

		public Type SupporterType { get; }
	}
}