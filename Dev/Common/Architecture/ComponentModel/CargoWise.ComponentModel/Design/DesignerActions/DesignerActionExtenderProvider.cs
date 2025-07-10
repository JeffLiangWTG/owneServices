using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
#if NET8_0_OR_GREATER
using System.Windows.Forms.Design;
#endif
using CargoWise.Common;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// When added as a field to a component that has a design surface, provides smart tags on all
	/// nested designable components.
	/// </summary>
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public class DesignerActionExtenderProvider : Component, IExtenderProvider
	{
		public bool CanExtend(object extendee)
		{
			if (!inCanExtend)
			{
				inCanExtend = true;
				try
				{
					var component = extendee as IComponent;
					if (component != null && component.Site != null && !components.ContainsKey(component))
					{
						var service = (DesignerActionService)component.Site.GetService(typeof(DesignerActionService));
						if (service != null &&
							!HasCargowiseDesignerActionList(service, component) &&
							!InGetComponentServiceActions()) // to prevent a 'enumeration modified' exception on controls with a .net 'dock in parent control' action
						{
							components[component] = null;
							var list = new KDesignerActionList(component);
							service.Add(component, list);
						}
					}
				}
				finally
				{
					inCanExtend = false;
				}
			}
			return false;
		}
		bool inCanExtend;

		static bool HasCargowiseDesignerActionList(DesignerActionService service, IComponent component)
		{
			Argument.NotNull(service, nameof(service)); // Suggested By ReviewBot 
			Argument.NotNull(component, nameof(component));
			var result = service.Contains(component);
			if (result)
			{
				result = false;
				var actionLists = service.GetComponentActions(component, ComponentActionsType.Service);
				foreach (var list in actionLists)
				{
					if (list is KDesignerActionList)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		static bool InGetComponentServiceActions()
		{
			var frames = new StackTrace().GetFrames();
			foreach (var frame in frames)
			{
				var method = frame.GetMethod();
				if (method != null && method.Name == "GetComponentServiceActions")
				{
					return true;
				}
			}
			return false;
		}

		readonly WeakReferencedKeyDictionary<IComponent, object> components = new WeakReferencedKeyDictionary<IComponent, object>();
	}
}
