#define CODE_ANALYSIS

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.Business
{
	/// <summary>
	/// Data Binder class to get and set property of an object.
	/// </summary>
	public static class ZPropertyAccessor
	{
		public static object Get(object container, string propertyName)
		{
			ContainerPropertyDescriptorPair pair = new ContainerPropertyDescriptorPair(container, propertyName);

			TestRequiresSuppression(container, propertyName, pair);

			object result = null;

			if (!((pair.Container is IZType) && ((IZType)pair.Container).IsEmpty))
			{
				result = pair.Descriptor.GetValue(pair.Container);
			}

			IAccessControlled bizO = container as IAccessControlled;
			if (bizO != null && result is IZType && bizO.SuppressionItem.TextNeedsToBeSuppressed(bizO, propertyName))
			{
				return SuppressUtil.GetSuppressedValue((IZType)result);
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public static bool TryGet(object container, string propertyName, out object result)
		{
			try
			{
				result = Get(container, propertyName);
			}
			catch (ArgumentException)
			{
				result = null;
			}

			return result != null;
		}

		public static object GetWithEncode(object container, string propertyName)
		{
			object result = Get(container, propertyName);
			return (result is ZString) ? new ZString(WebUtility.HtmlEncode((ZString)result)) : result;
		}

		public static void Set(object container, string propertyName, object propertyValue)
		{
			ContainerPropertyDescriptorPair pair = new ContainerPropertyDescriptorPair(container, propertyName);
			if (pair.Descriptor.IsReadOnly)
			{
				throw new ArgumentException("Property is read-only", propertyName);
			}
			pair.Descriptor.SetValue(pair.Container, propertyValue);
		}

		public static PropertyDescriptor GetPropertyDescriptor(object container, string propertyName)
		{
			return new ContainerPropertyDescriptorPair(container, propertyName).Descriptor;
		}

		public static Type GetPropertyType(object container, string propertyName)
		{
			return GetPropertyDescriptor(container, propertyName).PropertyType;
		}

		class ContainerPropertyDescriptorPair
		{
			public readonly object Container;
			public readonly PropertyDescriptor Descriptor;

			public ContainerPropertyDescriptorPair(object container, string propertyName)
			{
				if (string.IsNullOrEmpty(propertyName))
				{
					throw new ArgumentNullException(nameof(propertyName));
				}

				string[] expressionParts = propertyName.Trim().Split(new[] { '.' });

				Container = container;
				Descriptor = GetDescriptor(Container, expressionParts[0]);

				for (int i = 1; i < expressionParts.Length; i++)
				{
					Container = Descriptor.GetValue(Container);
					Descriptor = GetDescriptor(Container, expressionParts[i]);
				}
			}

			PropertyDescriptor GetDescriptor(object container, string propertyName)
			{
				if (container == null)
				{
					throw new ArgumentException("Container for the Property cannot be null: ", propertyName);
				}

				return TypeDescriptor.GetProperties(container).Find(propertyName, true)
					?? throw new ArgumentException("Unabled to find '" + propertyName + "' property on '" + container + "' object.");
			}
		}

		[DebuggerStepThrough]
		static void TestRequiresSuppression(object container, string propertyName, ContainerPropertyDescriptorPair pair)
		{
#if DEBUG
			// TODO: Add page load test - this won't ever fail at the moment (but no point running outside of a test because the developer exception won't get thrown)

			if (Globals.IsTest)
			{
				bool containsAttribute = false;

				foreach (Attribute attribute in pair.Descriptor.Attributes)
				{
					if (attribute is RequiresSuppressionAttribute)
					{
						containsAttribute = true;
						break;
					}
				}

				if (containsAttribute)
				{
					ErrorReporter.ReportOnce("BindWithoutSuppression_" + propertyName + "_" + container.GetType(),
											 "The property " + propertyName + " cannot be bound to because it requires suppression. Bind to the suppressed equivalent instead (" +
											 "(the property with the same name minus the table prefix and with the suffix WithSuppression).");
				}
			}
#endif
		}
	}
}
