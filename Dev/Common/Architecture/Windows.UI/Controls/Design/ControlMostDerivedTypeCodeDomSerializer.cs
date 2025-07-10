using System;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI.Design
{
	public class ControlMostDerivedTypeCodeDomSerializer : ControlDpiScalingCodeDomSerializer
	{
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			if (manager.GetService(typeof(IDesignerHost)) is IDesignerHost host)
			{
				var rootComponentClassName = host.RootComponentClassName;
				if (!string.IsNullOrEmpty(rootComponentClassName))
				{
					// Get the ITypeResolutionService to resolve the class name to a Type
					if (host.GetService(typeof(ITypeResolutionService)) is ITypeResolutionService typeResolutionService)
					{
						// Resolve to the most-derived Type.  Due to Windows Forms Designer's performance optimizations, the root component
						// is not always the form being designed, but rather a base class.  We need to find the most-derived type to ensure correct serialization.
						if (typeResolutionService.GetType(rootComponentClassName) is Type mostDerivedType)
						{
							var rootNamespace = mostDerivedType.Assembly.GetName().Name;
							CargoWiseOne.ResourceStrings.ResourceStringDataTypeConverter.TargetFormAssembly = mostDerivedType.Assembly;

							try
							{
								return base.Serialize(manager, value);
							}
							finally
							{
								// Restore the variable to null to avoid side effects on other components
								CargoWiseOne.ResourceStrings.ResourceStringDataTypeConverter.TargetFormAssembly = null;
							}
						}
					}
				}
			}
			return base.Serialize(manager, value);
		}
	}
}
