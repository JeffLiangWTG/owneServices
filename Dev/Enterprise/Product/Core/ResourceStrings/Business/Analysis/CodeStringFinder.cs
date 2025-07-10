using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.StaticAnalysis;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Enterprise.ResourceStrings.Business.Analysis
{
	/// <summary>
	/// Provides functionality to find usage of resource strings within a compiled .NET assembly.
	/// It takes the name of the assembly, reads its symbols if the .pdb file exists, and creates an AssemblyDefinition object.
	/// 
	/// The FindStringUsages method searches for a type with the given name in the assembly,
	///	then recursively searches through its hierarchy of base types for a method with the given name.
	///	If a matching method is found, it searches for usage of resource strings within the method's body
	///	and returns an array of ResourceStringReference objects representing each usage found.
	/// </summary>
	public class CodeStringFinder : IDisposable
	{
		public CodeStringFinder(string assemblyName)
		{
			bool readSymbols = File.Exists(Path.Combine(BinPath, assemblyName + ".pdb"));
			assembly = AssemblyDefinition.ReadAssembly(Path.Combine(BinPath, assemblyName + ".dll"), new ReaderParameters() { ReadSymbols = readSymbols });
			if (assembly.Modules.Count != 1)
			{
				throw new NotImplementedException("Expected an assembly containing exactly 1 module");
			}
			((DefaultAssemblyResolver)assembly.Modules[0].AssemblyResolver).RemoveSearchDirectory(".");
			((DefaultAssemblyResolver)assembly.Modules[0].AssemblyResolver).AddSearchDirectory(BinPath);
		}

		/// <summary>
		/// Searches for usages of resource strings within the specified member of the given type.
		/// </summary>
		public ResourceStringReference[] FindStringUsages(string typeName, string memberName, bool ignoreReturnType = false)
		{
			var type = assembly.Modules[0].GetType(typeName.Replace('+', '/')) ?? throw new ArgumentException(string.Format("No type named '{0}' found in assembly '{1}'", typeName, assembly.FullName));

			do
			{
				foreach (var method in type.Methods)
				{
					if (method.Name == memberName || method.Name == "get_" + memberName)
					{
						if (ignoreReturnType || IsSupportedReturnType(method.ReturnType) || HasReturnsResourceStringAttribute(method))
						{
							return FindStringUsages(method, new ExecutionState());
						}
						else
						{
							return Array.Empty<ResourceStringReference>();
						}
					}
				}

				type = type.BaseType != null ? type.BaseType.Resolve() : null;
			}
			while (type != null);

			throw new ArgumentException(string.Format("No member named '{0}' found in type '{1}'", memberName, typeName));
		}

		/// <summary>
		/// Finds usages of resource strings in the specified method.
		/// </summary>
		ResourceStringReference[] FindStringUsages(MethodDefinition method, ExecutionState state)
		{
			if (method == null || method.DeclaringType.Namespace.StartsWith("System") || method.DeclaringType.Namespace.StartsWith("Microsoft"))
			{
				return Array.Empty<ResourceStringReference>();
			}
			else if (cache.ContainsKey(method.FullName))
			{
				return cache[method.FullName];
			}
			else
			{
				HashSet<ResourceStringReference> findResults = new HashSet<ResourceStringReference>();
				Array.ForEach(FindStringUsagesInDerived(method, state), item => findResults.Add(item));
				Array.ForEach(FindStringUsagesInRelatedAnonymousDelegates(method, state), item => findResults.Add(item));
				Array.ForEach(FindStringUsagesInFromHints(method, state), item => findResults.Add(item));
				if (method.HasBody && !state.CallStack.Contains(method))
				{
					state.CallStack.Push(method);
					var analyzer = new MethodCallAnalyzer() { AnlayzeReadonlyFields = true };
					analyzer.MethodCall += new MethodCallAnalyzer.MethodCallDelegate((MethodReference callMethod, object instance, object[] parameters, Context context) =>
					{
						if (IsResourceStringGeneratedClass(callMethod, context))
						{
							return null;
						}

						if (IsResStringTarget(callMethod))
						{
							int paramidx = 0;
							if (parameters[0] is IntConstant)
							{
								paramidx++;
							}
							string key = parameters.Length > paramidx && parameters[paramidx] is StringConstant ? ((StringConstant)parameters[paramidx]).Value : null;
							paramidx++;
							string value = parameters.Length > paramidx && parameters[paramidx] is StringConstant ? ((StringConstant)(parameters[paramidx])).Value : null;
							findResults.Add(new ResourceStringReference(key, value, context.SequencePoint, context.Method));
						}
						else if (IsRegistryItem(callMethod.ReturnType))
						{
							if (GetRegistryItem(callMethod) is IRegistryItemCaptionSource registryItemCaptionSource)
							{
								foreach (var resString in registryItemCaptionSource.DefaultStrings)
								{
									findResults.Add(new ResourceStringReference(resString.ResourceKey, resString.EnglishText, context.SequencePoint, context.Method));
								}
							}
						}
						else if (callMethod.Name == "GetCachedValue" && callMethod.Parameters.Count <= 1 /* :( */ && callMethod is GenericInstanceMethod && ((GenericInstanceMethod)callMethod).GenericArguments.Count == 1)
						{
							var cachedType = ((GenericInstanceMethod)callMethod).GenericArguments[0].Resolve();
							foreach (var cachedTypeMethod in cachedType.Methods)
							{
								if (cachedTypeMethod.IsConstructor && cachedTypeMethod.Parameters.Count == 0)
								{
									Array.ForEach(FindStringUsages(cachedTypeMethod.Resolve(), context.State.CloneForCall()), item => findResults.Add(item));
									break;
								}
							}
						}
						else if (callMethod.Name == "GetCachedCodeDescriptionPairList")
						{
							foreach (ICodeDescription pair in new CodeDescriptionPairList((OLookUpEditType)((IntConstant)parameters[1]).Value))
							{
								if (pair is IMultilingualDescription && ((IMultilingualDescription)pair).MultilingualDescription != null && ((IMultilingualDescription)pair).MultilingualDescription is ResourceString)
								{
									findResults.Add(new ResourceStringReference(((ResourceString)((IMultilingualDescription)pair).MultilingualDescription).ResourceKey, ((ResourceString)((IMultilingualDescription)pair).MultilingualDescription).GetUnresolvedString(), context.SequencePoint, method));
								}
							}
						}
						else if (callMethod.Name == "GetMultilingual" && method.Name.StartsWith("get_") && method.Name.EndsWith("Multilingual"))
						{
							var property = method.DeclaringType.Properties.FirstOrDefault(p => p.Name == method.Name.Substring(4, method.Name.Length - 16));
							if (property != null)
							{
								FindStringsForTranslatableField(property, findResults, context.SequencePoint);
							}
						}
						else if (callMethod.Name == ".ctor")
						{
							OnNewObj(findResults, callMethod, parameters, context);
						}
						else if ((IsSupportedReturnType(callMethod.ReturnType) && !IsInternalResrouceStringFunction(callMethod)) || HasReturnsResourceStringAttribute(callMethod) || (method.IsConstructor && !callMethod.Resolve().IsConstructor))
						{
							Array.ForEach(FindStringUsages(callMethod.Resolve(), context.State.CloneForCall()), item => findResults.Add(item));
						}

						return null;
					});

					analyzer.NewObj += new MethodCallAnalyzer.NewObjDelegate((MethodReference constructorMethod, object[] parameters, Context context) =>
						{
							OnNewObj(findResults, constructorMethod, parameters, context);
							return null;
						});

					analyzer.ReadonlyField += new MethodCallAnalyzer.ReadonlyFieldDelegate((FieldReference field, MethodReference constructorMethod, Context context) =>
						{
							if (!IsCodeDescriptionPairList(constructorMethod.DeclaringType))
							{
								Array.ForEach(FindStringUsages(constructorMethod.Resolve(), context.State.CloneForCall()), item => findResults.Add(item));
							}
						});

					analyzer.Analyze(method, state);
				}
				return cache[method.FullName] = findResults.ToArray();
			}
		}

		void OnNewObj(HashSet<ResourceStringReference> findResults, MethodReference constructorMethod, object[] parameters, Context context)
		{
			if (constructorMethod.DeclaringType.FullName == typeof(CodeDescriptionPairList).FullName && constructorMethod.Parameters.Count == 1 && constructorMethod.Parameters[0].ParameterType.FullName == typeof(OLookUpEditType).FullName)
			{
				if (parameters[0] is IntConstant)
				{
					foreach (ICodeDescription pair in new CodeDescriptionPairList((OLookUpEditType)((IntConstant)parameters[0]).Value))
					{
						if (pair is IMultilingualDescription && ((IMultilingualDescription)pair).MultilingualDescription != null && ((IMultilingualDescription)pair).MultilingualDescription is ResourceString)
						{
							findResults.Add(new ResourceStringReference(((ResourceString)((IMultilingualDescription)pair).MultilingualDescription).ResourceKey, ((ResourceString)((IMultilingualDescription)pair).MultilingualDescription).GetUnresolvedString(), context.SequencePoint, context.Method));
						}
					}
				}
			}
			else if (IsCodeDescriptionPairList(constructorMethod.DeclaringType) && constructorMethod.DeclaringType.FullName != typeof(CodeDescriptionPairList).FullName && constructorMethod.DeclaringType.FullName != typeof(ReadOnlyCodeDescriptionPairList).FullName)
			{
				Array.ForEach(FindStringUsages(constructorMethod.Resolve(), context.State.CloneForCall()), item => findResults.Add(item));
			}
		}

		object analyzer_NewObj(MethodReference constructorMethod, object[] parameters, Context context)
		{
			throw new NotImplementedException();
		}

		object analyzer_MethodCall(MethodReference callMethod, object instance, object[] parameters, Context context)
		{
			throw new NotImplementedException();
		}

		ResourceStringReference[] FindStringUsagesInDerived(MethodDefinition method, ExecutionState state)
		{
			HashSet<ResourceStringReference> findResults = new HashSet<ResourceStringReference>();
			foreach (var type in FindDerivedTypes(method.DeclaringType))
			{
				foreach (var typeMethod in type.Methods)
				{
					if (typeMethod.Name == method.Name)
					{
						Array.ForEach(FindStringUsages(typeMethod, state.CloneForBranch()), item => findResults.Add(item));
					}
				}
			}
			return findResults.ToArray();
		}

		IEnumerable<TypeDefinition> FindDerivedTypes(TypeDefinition declaringType)
		{
			foreach (var type in declaringType.Module.Types)
			{
				foreach (var derived in FindDerivedTypes(declaringType, type))
				{
					yield return derived;
				}
			}
		}

		IEnumerable<TypeDefinition> FindDerivedTypes(TypeDefinition declaringType, TypeDefinition type)
		{
			if (type.BaseType == declaringType || type.Interfaces.Select(i => i.InterfaceType).Contains(declaringType) || (type.BaseType != null && type.BaseType.IsGenericInstance && type.BaseType.Resolve() == declaringType))
			{
				foreach (var derived in FindDerivedTypes(type))
				{
					yield return derived;
				}
				yield return type;
			}
			foreach (var nestedType in type.NestedTypes)
			{
				foreach (var derived in FindDerivedTypes(declaringType, nestedType))
				{
					yield return derived;
				}
			}
		}

		ResourceStringReference[] FindStringUsagesInRelatedAnonymousDelegates(MethodDefinition method, ExecutionState state)
		{
			HashSet<ResourceStringReference> findResults = new HashSet<ResourceStringReference>();
			FindStringUsagesInRelatedAnonymousDelegates(method, method.DeclaringType, findResults, state);
			foreach (var nestedType in method.DeclaringType.NestedTypes)
			{
				FindStringUsagesInRelatedAnonymousDelegates(method, nestedType, findResults, state);
			}
			return findResults.ToArray();
		}

		void FindStringUsagesInRelatedAnonymousDelegates(MethodDefinition method, TypeDefinition type, HashSet<ResourceStringReference> findResults, ExecutionState state)
		{
			foreach (var typeMethod in type.Methods)
			{
				if (typeMethod.Name.StartsWith("<" + method.Name + ">b__"))
				{
					Array.ForEach(FindStringUsages(typeMethod, state.CloneForBranch()), item => findResults.Add(item));
				}
			}
		}

		void FindStringsForTranslatableField(PropertyDefinition property, HashSet<ResourceStringReference> findResults, SequencePoint sequencePoint)
		{
			var attribute = property.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "TranslatableDataFieldAttribute");
			TranslatableDataFieldAttribute attributeInstance = null;

			if (attribute != null)
			{
				var args = attribute.ConstructorArguments.Select(arg => arg.Value).ToArray();
				attributeInstance = (TranslatableDataFieldAttribute)typeof(TranslatableDataFieldAttribute).GetConstructor(Array.ConvertAll(args, arg => arg.GetType())).Invoke(args);
				foreach (var attrProperty in attribute.Properties)
				{
					object value = attrProperty.Argument.Value;
					if (value is ValueType)
					{
						attributeInstance.GetType().GetProperty(attrProperty.Name).SetValue(attributeInstance, attrProperty.Argument.Value, null);
					}
				}
			}
			else
			{
				var linkedTransAttribute = property.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "LinkedTranslatableDataFieldAttribute");
				if (linkedTransAttribute != null)
				{
					var args = linkedTransAttribute.ConstructorArguments.Select(arg => arg.Value).ToArray();
					TypeDefinition argType = ((TypeDefinition)args[0]);
					var linkedTransAttributeInstance = (LinkedTranslatableDataFieldAttribute)typeof(LinkedTranslatableDataFieldAttribute).GetConstructor(new Type[] { typeof(Type), typeof(string) })
						.Invoke(new object[] { Assembly.Load(argType.Namespace).GetType(argType.FullName), args[1] });

					attributeInstance = (TranslatableDataFieldAttribute)linkedTransAttributeInstance.GetType().GetField("attribute", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(linkedTransAttributeInstance);
				}
			}

			if (attributeInstance != null)
			{
				try
				{
					foreach (var caption in attributeInstance.GetCompileTimeSystemCaptions())
					{
						findResults.Add(new ResourceStringReference(caption.ResourceKey, caption.ToString(), sequencePoint, property.GetMethod));
					}
				}
				catch (DirectoryNotFoundException)
				{
					var eng = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
					foreach (var key in eng.AllKeys.Where(key => key.StartsWith(attributeInstance.RootKeyPrefixWithSeperator)))
					{
						findResults.Add(new ResourceStringReference(key, eng.Get(key).Caption, sequencePoint, property.GetMethod));
					}
				}
			}
		}

		static string BinPath
		{
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		bool IsSupportedReturnType(TypeReference type)
		{
			TypeDefinition resolved;
			IGenericInstance generic;
			return Array.IndexOf(SupportedReturnTypes, type.FullName) > -1 ||
				((resolved = type.Resolve()) != null && resolved.BaseType != null && IsSupportedReturnType(resolved.BaseType)) ||
				((generic = type as IGenericInstance) != null && generic.GenericArguments.Any(t => t.Resolve()?.BaseType != type && IsSupportedReturnType(t))) ||
				(resolved != null && FindAttribute(resolved, typeof(CodeStringFinderSupportedReturnTypeAttribute)).Any());
		}

		public static bool IsSupportedReturnType(Type type)
		{
			return Array.IndexOf(SupportedReturnTypes, type.FullName) > -1 ||
				(type.BaseType != null && IsSupportedReturnType(type.BaseType)) ||
				type.GetCustomAttributes(typeof(CodeStringFinderSupportedReturnTypeAttribute), true).Length > 0;
		}

		static readonly string[] SupportedReturnTypes = new string[]
		{
			typeof(string).FullName,
			typeof(ZString).FullName,
			typeof(MultilingualString).FullName,
			typeof(ResourceString).FullName,
			typeof(ZMultilingual).FullName,
			typeof(ReadOnlyCodeDescriptionPairList).FullName,
			typeof(IZType).FullName,
			"Enterprise.DocumentWrappers.GenericWrappers.CodeAndDescriptionWrapper",
			"Enterprise.DocumentWrappers.GenericWrappers.LabelValuePairWrapper",
			typeof(void).FullName,
		};

		static bool IsInternalResrouceStringFunction(MethodReference method)
		{
			return method.DeclaringType.FullName == typeof(MultilingualString).FullName || method.DeclaringType.FullName == typeof(ResourceString).FullName;
		}

		bool HasReturnsResourceStringAttribute(MethodReference method)
		{
			var methodDefintion = method.Resolve();
			return methodDefintion == null || HasReturnsResourceStringAttribute(methodDefintion);
		}

		bool HasReturnsResourceStringAttribute(MethodDefinition method)
		{
			return FindAttribute(method, typeof(ReturnsResourceStringAttribute)).Any();
		}

		ResourceStringReference[] FindStringUsagesInFromHints(MethodDefinition method, ExecutionState state)
		{
			HashSet<ResourceStringReference> results = null;
			MethodHint methodHint = new MethodHint(method);
			if (!state.CallStack.Contains(methodHint))
			{
				foreach (var attribute in FindAttribute(method, typeof(CodeStringFinderHintAttribute)))
				{
					TypeDefinition hintType = ((TypeReference)attribute.ConstructorArguments[0].Value).Resolve();
					string hintMethodName = (string)attribute.ConstructorArguments[1].Value;
					foreach (var typeMethod in hintType.Methods)
					{
						if (typeMethod.Name == hintMethodName || typeMethod.Name == "get_" + hintMethodName)
						{
							if (results == null)
							{
								results = new HashSet<ResourceStringReference>();
							}
							state = state.CloneForCall();
							state.CallStack.Push(methodHint);
							Array.ForEach(FindStringUsages(typeMethod, state), item => results.Add(item));
						}
					}
				}
			}
			return results != null ? results.ToArray() : Array.Empty<ResourceStringReference>();
		}

		class MethodHint
		{
			public MethodHint(MethodDefinition method)
			{
				this.method = method;
			}

			readonly MethodDefinition method;

			public override bool Equals(object obj)
			{
				return obj is MethodHint && ((MethodHint)obj).method.Equals(method);
			}

			public override int GetHashCode()
			{
				return method.GetHashCode();
			}
		}

		IEnumerable<CustomAttribute> FindAttribute(MethodDefinition method, Type attributeType)
		{
			foreach (var property in method.DeclaringType.Properties)
			{
				if (property.GetMethod == method)
				{
					foreach (var attribute in property.CustomAttributes)
					{
						if (attribute.AttributeType.FullName == attributeType.FullName)
						{
							yield return attribute;
						}
					}
				}
			}
			foreach (var attribute in method.CustomAttributes)
			{
				if (attribute.AttributeType.FullName == attributeType.FullName)
				{
					yield return attribute;
				}
			}
		}

		IEnumerable<CustomAttribute> FindAttribute(Mono.Cecil.ICustomAttributeProvider type, Type attributeType)
		{
			foreach (var attribute in type.CustomAttributes)
			{
				if (attribute.AttributeType.FullName == attributeType.FullName)
				{
					yield return attribute;
				}
			}
		}

		bool IsCodeDescriptionPairList(TypeReference type)
		{
			TypeDefinition resolved;
			return type.FullName == typeof(ReadOnlyCodeDescriptionPairList).FullName || ((resolved = type.Resolve()) != null && resolved.BaseType != null && IsCodeDescriptionPairList(resolved.BaseType));
		}

		bool IsResourceStringGeneratedClass(MethodReference callMethod, Context context)
		{
			var documentUrl = context?.SequencePoint?.Document?.Url;
			if (documentUrl == null)
			{
				return callMethod.DeclaringType.Name == nameof(ResStringBridge)
					&& callMethod.Name == nameof(ResStringBridge._GetMultilingualString);
			}

			return documentUrl.EndsWith("ReplacedMethods.g.cs");
		}

		static bool IsResStringTarget(MethodReference method)
		{
			var excludedTypeList = new string[]
			{
				"Enterprise.Mobile.Shared.ResourceString.Cache.Res",
			};
			if (excludedTypeList.Contains(method.DeclaringType.FullName))
			{
				return false;
			}

			if (method.DeclaringType.FullName.EndsWith(".Res", StringComparison.Ordinal))
			{
				if (method.Name == "GetString"
					|| (method.Name == "_GetString"))
				{
					return method.ReturnType.Name == nameof(String);
				}
				else if (method.Name == "GetData"
					|| (method.Name == "_GetData"))
				{
					return method.ReturnType.Name == nameof(ResourceStringData);
				}
			}
			else if (method.DeclaringType.FullName.EndsWith(".ResString", StringComparison.Ordinal)
				 && (method.Name == "GetMultilingualString"
				  || (method.Name == "_GetMultilingualString")))
			{
				return method.ReturnType.Name == nameof(ResourceString);
			}
			return false;
		}

		bool IsRegistryItem(TypeReference type)
		{
			return type.Name.Contains("Registry") && IsRegistryItem(type.Resolve());
		}

		bool IsRegistryItem(TypeDefinition type)
		{
			return type != null && (type.FullName == typeof(IRegistryItem).FullName || (type.BaseType != null && IsRegistryItem(type.BaseType)) || type.Interfaces.Select(i => i.InterfaceType).FirstOrDefault(item => IsRegistryItem(item)) != null);
		}

		static IRegistryItem GetRegistryItem(MethodReference method)
		{
			var assembly = Assembly.Load(method.Resolve().Module.Assembly.FullName);
			var registrySetType = assembly.GetType(method.DeclaringType.FullName);
			var instance = registrySetType.GetProperty("Instance").GetValue(null, Array.Empty<object>());
			return (IRegistryItem)instance.GetType().GetMethod(method.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, Array.Empty<object>());
		}

		public void Dispose()
		{
			assembly.Dispose();
		}

		public string AssemblyName
		{
			get { return assembly.Name.Name; }
		}

		readonly AssemblyDefinition assembly;
		readonly Dictionary<string, ResourceStringReference[]> cache = new Dictionary<string, ResourceStringReference[]>();

		public class ResourceStringReference
		{
			public ResourceStringReference(string key, string value, SequencePoint sequencePoint, MethodDefinition context)
			{
				this.Key = key;
				this.Value = value;
				if (sequencePoint != null)
				{
					this.FileName = sequencePoint.Document.Url;
					this.Line = sequencePoint.StartLine;
					this.Column = sequencePoint.StartColumn;
				}
				if (context != null)
				{
					this.Context = context.FullName;
				}
			}

			public readonly string Key;
			public readonly string Value;
			public readonly string FileName;
			public readonly int Line;
			public readonly int Column;
			public readonly string Context;

			public override bool Equals(object obj)
			{
				return obj is ResourceStringReference && this.Key != null ? this.Key == ((ResourceStringReference)obj).Key :
					this.FileName == ((ResourceStringReference)obj).FileName && this.Line == ((ResourceStringReference)obj).Line;
			}

			public override int GetHashCode()
			{
				return this.Key != null ? this.Key.GetHashCode() : (this.FileName ?? "").GetHashCode() ^ this.Line.GetHashCode();
			}
		}
	}
}
