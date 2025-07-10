using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[RequiresSoftware(RequiredSoftware.VisualStudio)]
	public abstract class ComponentToolboxVisibilityBaseTestCase : TestCase
	{
#if !WINZOR
		public void TestToolboxVisibleComponents()
		{
			var message = new StringBuilder();
			if (UnexpectedToolboxVisibleTypes.Length > 0)
			{
				message.AppendLine("Unexpected toolbox visible components:");
				message.AppendLine(TypeListToString(UnexpectedToolboxVisibleTypes));
				message.AppendLine("Either:");
				message.AppendLine("- If you wish this control or component to show up in the toolbox, add the type to ExpectedToolboxVisibleComponentTypes to this test.");
				message.AppendLine("- Change the type visibility from public to internal.");
				message.AppendLine("- Apply [ToolboxItem(false)] to the type.");
			}
			if (MissingToolboxVisibleTypes.Length > 0)
			{
				message.AppendLine("Components expected to be toolbox visible, but are not:");
				message.AppendLine(TypeListToString(MissingToolboxVisibleTypes));
			}

			HtmlAssert("\n" + message.Replace("\n", "<BR>"), message.Length == 0);
		}
#endif
		protected abstract Type[] GetExpectedToolboxVisibleComponentTypes();

		#region Implementation

#if !WINZOR
		IList<Type> ExpectedToolboxVisibleComponentTypes
		{
			get { return GetExpectedToolboxVisibleComponentTypes(); }
		}

		Type[] UnexpectedToolboxVisibleTypes
		{
			get
			{
				List<Type> result = new List<Type>();
				foreach (Type type in ActualToolboxVisibleComponentTypes)
				{
					if (!ExpectedToolboxVisibleComponentTypes.Contains(type))
					{
						result.Add(type);
					}
				}
				return result.ToArray();
			}
		}

		Type[] MissingToolboxVisibleTypes
		{
			get
			{
				List<Type> result = new List<Type>();
				foreach (Type type in ExpectedToolboxVisibleComponentTypes)
				{
					if (!ActualToolboxVisibleComponentTypes.Contains(type))
					{
						result.Add(type);
					}
				}
				return result.ToArray();
			}
		}

		IList<Type> ActualToolboxVisibleComponentTypes
		{
			get
			{
				if (toolboxVisibleComponentTypes == null)
				{
					toolboxVisibleComponentTypes = new List<Type>();
					foreach (Type type in TargetAssembly.GetTypes())
					{
						if (IsToolboxVisible(type))
						{
							toolboxVisibleComponentTypes.Add(type);
						}
					}
				}
				return toolboxVisibleComponentTypes.ToArray();
			}
		}
		List<Type> toolboxVisibleComponentTypes;

		bool IsToolboxVisible(Type type)
		{
			ToolboxItemAttribute toolboxItem = (ToolboxItemAttribute)TypeDescriptor.GetAttributes(type)[typeof(ToolboxItemAttribute)];
			return
				type.IsVisible &&
				!type.IsInterface &&
				!type.IsAbstract &&
				typeof(IComponent).IsAssignableFrom(type) &&
				(toolboxItem == null || !string.IsNullOrEmpty(toolboxItem.ToolboxItemTypeName));
		}

		static string TypeListToString(Type[] typeList)
		{
			StringBuilder result = new StringBuilder();
			foreach (Type type in typeList)
			{
				result.AppendLine(type.FullName);
			}
			return result.ToString();
		}
 #endif
		protected virtual Assembly TargetAssembly
		{
			get
			{
				return GetType().Assembly;
			}
		}

		#endregion
	}
}
