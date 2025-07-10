using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class RegistryItem : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RegistryItem({registryobjectidentifier})>"
				, ResString.GetMultilingualString("d6cad01b-a397-4a52-8192-d7ab93b71780",
				@"Will return registry item values from the object in the Old Style registry (i.e: {0}.property name) or the New Style registry (i.e: {1}.property name) based on the registry object identifier that you pass in.",
				"Env.Registry", "DocumentsDataRegistry.Instance"),
				new List<(string example, object expectedResult)> { ((NoResString)"<RegistryItem(Env.Registry.AWBSecurityDeclaration.OpeningText)>", (NoResString)"Some Opening") });
		}

		#region IsRegistryValueOfBoolType

		internal static bool IsRegistryValueOfBoolType(string macro)
		{
			var registryValue = GetRegistryValueFromMacro(macro);
			return registryValue is bool;
		}

		#endregion

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = GetRegistryValueFromMacro(macro);
			if (result == null)
			{
				ReportMacroError(report, Res.GetString("f9f132f4-021f-4eee-a28b-6a5f63726041", "Could not find Registry Item indicated by {0}.", macro));
				result = "";
			}

			return result;
		}

		static object GetRegistryValueFromMacro(string macro)
		{
			int indexOfOpeningBracket = macro.IndexOf("(", StringComparison.OrdinalIgnoreCase) + 1;
			int indexOfClosingBracket = macro.IndexOf(")", StringComparison.OrdinalIgnoreCase);
			object result = null;

			if (indexOfClosingBracket > indexOfOpeningBracket)
			{
				result = GetRegistryValueFromObjectIdentifier(macro.Substring(indexOfOpeningBracket, indexOfClosingBracket - indexOfOpeningBracket));
			}

			return result;
		}

		static object GetRegistryValueFromObjectIdentifier(string objectIdentifier)
		{
			object result = null;
			try
			{
				if (objectIdentifier.StartsWith("Env.Registry."))
				{
					result = ReflectOut(objectIdentifier.Substring(13), Env.Registry);
				}
				else
				{
					string[] parameters = objectIdentifier.Split(new string[] { ".Instance." }, StringSplitOptions.RemoveEmptyEntries);
					if (parameters.Length == 2)
					{
						if (parameters[1].Contains(","))
						{
							var propertyPlusAssembly = parameters[1].Split(',');
							string property = propertyPlusAssembly[0];
							string assembly = propertyPlusAssembly[1];
							result = GetNewStyleRegistryItem(parameters[0] + ", " + assembly, property);
						}
						else
						{
							result = GetNewStyleRegistryItem(parameters[0], parameters[1]);
						}
					}
				}
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}
			}

			return result;
		}

		static object GetNewStyleRegistryItem(string objectTypeName, string fullPropertyName)
		{
			Type objectType;
			if (objectTypeName.Contains(","))
			{
				objectType = Type.GetType(objectTypeName);
			}
			else
			{
				var registryItemSetLocator = new RegistryItemSetLocator();
				objectType = registryItemSetLocator.GetRegistryItemSet(objectTypeName).GetType();
			}

			PropertyInfo propertyInfo = objectType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
			object currentObject = propertyInfo.GetValue(null, null);

			if (currentObject != null)
			{
				currentObject = ReflectOut(fullPropertyName, currentObject);

				if (currentObject != null)
				{
					var isPassword1 = (currentObject as RegistryItemWrapper) != null && (currentObject as RegistryItemWrapper).EditorInfo as TextRegistryEditorInfo != null && ((currentObject as RegistryItemWrapper).EditorInfo as TextRegistryEditorInfo).EditorType == TextEditorType.Password;
					var isPassword2 = (currentObject as RegistryItemImpl) != null && (currentObject as RegistryItemImpl).EditorInfo as TextRegistryEditorInfo != null && ((currentObject as RegistryItemImpl).EditorInfo as TextRegistryEditorInfo).EditorType == TextEditorType.Password;
					if (isPassword1 || isPassword2)
					{
						currentObject = "***";
					}
					else
					{
						currentObject = ReflectOut((NoResString)"Value", currentObject);
					}
				}
			}

			return currentObject;
		}

		static object ReflectOut(string fullPropertyName, object currentObject)
		{
			string[] propertyNames = fullPropertyName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			for (int index = 0; index < propertyNames.Length; index++)
			{
				PropertyInfo propertyInfo = currentObject.GetType().GetProperty(propertyNames[index]);
				FieldInfo fieldInfo = currentObject.GetType().GetField(propertyNames[index]);
				if (propertyInfo != null)
				{
					currentObject = propertyInfo.GetValue(currentObject, null);
				}
				else if (fieldInfo != null)
				{
					currentObject = fieldInfo.GetValue(currentObject);
				}
				else
				{
					currentObject = null;
					break;
				}
			}
			return currentObject;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Registry\s*Item\s*\(.*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
