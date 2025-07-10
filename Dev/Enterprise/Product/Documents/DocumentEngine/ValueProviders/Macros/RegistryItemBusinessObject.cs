using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class RegistryItemBusinessObject : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<RegistryItemBusinessObject({registryobjectidentifier}).{Property Name or Function}>",
				ResString.GetMultilingualString("11c7d610-8fa7-4536-aa1d-b50652c9562b",
				@"Will allow registry item business objects to be accessed as though it were through a property on a document wrapper. 
Please note that this will NOT work on old style registry items as they are not based on Business Objects."),
				new List<(string example, object expectedResult)> {
					(
					(NoResString)"<RegistryItemBusinessObject(Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PrincipalDocumentBrand,Enterprise.DocumentEngineCore).Format(\"{Code}\", Comma)>", (NoResString)"WTG, XYZ"
					) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			GroupCollection parameters = Regex.Match(macro).Groups;
			return GetReplacementCore(parameters["registry"].Value, parameters["assembly"].Value, parameters["path"].Value, report);
		}

		object GetReplacementCore(string registryName, string assemblyName, string path, Report report)
		{
			IRegistryItem item = null;
			string[] parameters = registryName.Split(new string[] { ".Instance." }, StringSplitOptions.RemoveEmptyEntries);

			if (parameters.Length == 2)
			{
				Type registryItemType;
				string property;

				if (!string.IsNullOrEmpty(assemblyName))
				{
					property = parameters[1];
					registryItemType = Type.GetType(parameters[0] + ", " + assemblyName);
				}
				else
				{
					registryItemType = typeof(DocumentsDataRegistry).Assembly.GetType(typeof(DocumentsDataRegistry).Namespace + "." + parameters[0]);
					property = parameters[1];
				}

				if (registryItemType != null)
				{
					object currentObject = GetRegistryItemSet(registryItemType);
					item = GetRegistryItem(currentObject, property);
				}
			}

			if (item == null)
			{
				ReportMacroError(report, Res.GetString("611dbc90-0b6e-4625-b47e-97b326ca4052", "Unknown Registry Item: [{0}]", registryName));
				return null;
			}
			else if (string.IsNullOrEmpty(path))
			{
				var isPassword1 = (item as RegistryItemWrapper) != null && (item as RegistryItemWrapper).EditorInfo as TextRegistryEditorInfo != null && ((item as RegistryItemWrapper).EditorInfo as TextRegistryEditorInfo).EditorType == TextEditorType.Password;
				var isPassword2 = (item as RegistryItemImpl) != null && (item as RegistryItemImpl).EditorInfo as TextRegistryEditorInfo != null && ((item as RegistryItemImpl).EditorInfo as TextRegistryEditorInfo).EditorType == TextEditorType.Password;
				if (isPassword1 || isPassword2)
				{
					return "***";
				}

				return item.Value;
			}
			else
			{
				return FollowPath(item, path, report);
			}
		}

		object GetRegistryItemSet(Type registryType)
		{
			return registryType.InvokeMember("Instance", BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public, null, null, Array.Empty<object>());
		}

		IRegistryItem GetRegistryItem(object registryItemSet, string propertyName)
		{
			PropertyInfo propertyInfo;
			FieldInfo fieldInfo;

			if ((propertyInfo = registryItemSet.GetType().GetProperty(propertyName)) != null)
			{
				return (IRegistryItem)propertyInfo.GetValue(registryItemSet, Array.Empty<object>());
			}
			else if ((fieldInfo = registryItemSet.GetType().GetField(propertyName)) != null)
			{
				return (IRegistryItem)fieldInfo.GetValue(registryItemSet);
			}
			else
			{
				return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Value Property Name")]
		object FollowPath(IRegistryItem registryObject, string path, Report report)
		{
			const string Value = "Value";
			MethodInfoChainLink[] links = new BusinessObjectReflector().GetMethodInfoChain(registryObject.GetType(), registryObject, Value + path);

			if (links == null)
			{
				ReportMacroError(report, Res.GetString("75e9cf10-a8ef-4909-814e-17c5ff517913", "Unable To Resolve Path: [{0}]", path));
				return null;
			}
			else
			{
				object current = registryObject;

				foreach (MethodInfoChainLink link in links)
				{
					current = BODocDataProvider.GetObject(link.ReflectOutObject(current, registryObject));

					if (current == null)
					{
						return null;
					}
				}

				return current;
			}
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<\s*Registry\s*Item\s*Business\s*Object\s*\(\s*(?<registry>[a-z0-9_.]*)(\s*,\s*(?<assembly>[a-z0-9_.]*))?\s*\)(?<path>(\s*.\s*[a-z0-9_]+(\s*\(\s*(""([^""\\]|\\.)*""|[^"")])*\s*\))?)*)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
	}
}
