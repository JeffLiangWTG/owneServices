using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Web;
#endif
using CargoWise.BuildTools;
using CargoWise.Common;
using Mono.Cecil;

namespace Enterprise.ReflectionTest
{
	class DeadCodeAnalyzer
	{
		/// <summary>
		/// binaryPath will be set to CargoWise.Common.AssemblyLoader.GetBinPath()
		/// </summary>
		public DeadCodeAnalyzer()
			: this(AssemblyLoader.GetBinPath())
		{
		}

		public DeadCodeAnalyzer(string binaryPath)
		{
			if (string.IsNullOrEmpty(binaryPath))
			{
				throw new ArgumentException("Cannot be null or blank", nameof(binaryPath));
			}

			this.binaryPath = binaryPath;
		}

		readonly string binaryPath;

		public HashSet<string> DefinedTypes
		{
			get { return definedTypes; }
		}

		public HashSet<string> UsedTypes
		{
			get { return usedTypes; }
		}

		public void AnalyzeAssembly(string assemblyName, BuildXml buildXml)
		{
			if (assemblyName.Contains("XmlSerializers") || buildXml.IsDuplicatedCodeAssembly(assemblyName) || ReflectionTestHelper.IsNotReflectionTestable(assemblyName))
			{
				return;
			}

			if (AssemblyChecker.IsNotTargetPrefix(assemblyName))
			{
				return;
			}

			var assemblyPath = Path.Combine(binaryPath, assemblyName);
			if (Path.GetExtension(assemblyPath).Equals(".exe", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(assemblyPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
			{
				using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
				{
					if (!assemblyDefinition.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(WTG.StaticAnalysis.Annotation.CodeAliveAttribute).FullName))
					{
						foreach (var module in assemblyDefinition.Modules)
						{
							AnalyzeModule(module);
						}
						AnalyzeAttributes(assemblyDefinition, null);
					}
				}
			}
		}

		void AnalyzeModule(ModuleDefinition module)
		{
			var binPath = AssemblyLoader.GetBinPath();
			var assemblyResolver = (DefaultAssemblyResolver)module.AssemblyResolver;
			assemblyResolver.RemoveSearchDirectory(".");
			assemblyResolver.AddSearchDirectory(binPath);
			assemblyResolver.AddSearchDirectory(Path.Combine(binPath, "net40"));

			AnalyzeAttributes(module, null);
			if (module.EntryPoint != null)
			{
				LogUsage(module.EntryPoint.DeclaringType, null);
			}
			foreach (var type in module.Types)
			{
				AnalyzeTypeDefinition(type);
			}
		}

		void AnalyzeTypeDefinition(TypeDefinition type)
		{
			if (type.Name != "<Module>" && !IsRoot(type) && !HasConstantsOnly(type) && !IsCodeContracts(type) && !type.FullName.Contains("/<>c") && !IsCompilerGeneratedType(type))
			{
				lock (definedTypes)
				{
					definedTypes.Add(GetKey(type));
				}
			}
			AnalyzeAttributes(type, type);
			LogUsage(type.BaseType, type);
			foreach (var itf in type.Interfaces.Select(i => i.InterfaceType))
			{
				LogUsage(itf, type);
			}
			foreach (var method in type.Methods)
			{
				AnalyzeMethodDefinition(method);
			}
			foreach (var property in type.Properties)
			{
				AnalyzePropertyDefinition(property);
			}
			foreach (var field in type.Fields)
			{
				AnalyzeFieldDefinition(field);
			}
			foreach (var nestedType in type.NestedTypes)
			{
				AnalyzeTypeDefinition(nestedType);
			}
		}

		bool HasConstantsOnly(TypeDefinition type)
		{
			return type != null &&
					(!type.HasMethods || (type.Methods.Count == 1 && type.Methods[0].IsConstructor && !type.Methods[0].HasParameters)) &&
					!type.HasProperties &&
					((type.HasFields && type.Fields.All(field => field.IsLiteral)) || type.HasNestedTypes || type.Name == "NamespacePlaceHolder" || HasConstantsOnly(ResolveSafe(type.BaseType)));
		}

		TypeDefinition ResolveSafe(TypeReference typeRef)
		{
			TypeDefinition resolved = null;
			if (typeRef != null)
			{
				try
				{
					resolved = typeRef.Resolve();
				}
				catch (NotSupportedException)
				{ }
			}
			return resolved;
		}

		static bool IsCodeContracts(TypeDefinition type)
		{
			return type.FullName.StartsWith("System.Diagnostics.Contracts");
		}

		static bool IsCompilerGeneratedType(TypeDefinition type)
		{
			return type.CustomAttributes.Any(x =>
				x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"
				|| x.AttributeType.FullName == "Microsoft.CodeAnalysis.EmbeddedAttribute"
				|| x.AttributeType.FullName == "System.CodeDom.Compiler.GeneratedCodeAttribute");
		}

		void AnalyzeMethodDefinition(MethodDefinition method)
		{
			AnalyzeAttributes(method, method.DeclaringType);
			AnalyzeTypeFactoryAnnotationMethod(method);
			LogUsage(method.ReturnType, method.DeclaringType);
			foreach (var parameter in method.Parameters)
			{
				LogUsage(parameter.ParameterType, method.DeclaringType);
			}
			if (method.HasBody)
			{
				foreach (var instruction in method.Body.Instructions)
				{
					AnalyzeMethodReference(instruction.Operand as MethodReference, method.DeclaringType);
					AnalyzeFieldReference(instruction.Operand as FieldReference, method.DeclaringType);
					AnalyzeGenericInstance(instruction.Operand as IGenericInstance, method.DeclaringType);
					LogUsage(instruction.Operand as TypeReference, method.DeclaringType);
					AnalyzeString(instruction.Operand as string);
				}
			}
			if (IsKnownRoot(method))
			{
				LogUsage(method.DeclaringType, null);
			}
		}

		void AnalyzeFieldDefinition(FieldDefinition field)
		{
			AnalyzeAttributes(field, field.DeclaringType);
			LogUsage(field.FieldType, field.DeclaringType);
		}

		void AnalyzePropertyDefinition(PropertyDefinition property)
		{
			AnalyzeAttributes(property, property.DeclaringType);
			LogUsage(property.PropertyType, property.DeclaringType);
		}

		void AnalyzeMethodReference(MethodReference callMethod, TypeDefinition context)
		{
			if (callMethod != null)
			{
				LogUsage(callMethod.DeclaringType, context);
			}
		}

		void AnalyzeFieldReference(FieldReference fieldReference, TypeDefinition context)
		{
			if (fieldReference != null)
			{
				LogUsage(fieldReference.DeclaringType, context);
			}
		}

		void AnalyzeAttributes(ICustomAttributeProvider attributeProvider, TypeDefinition context)
		{
			if (attributeProvider.HasCustomAttributes)
			{
				foreach (var attribute in attributeProvider.CustomAttributes)
				{
					AnalyzeAttribute(attribute, context);
				}
			}
		}

		void AnalyzeAttribute(CustomAttribute attribute, TypeDefinition context)
		{
			try
			{
				LogUsage(attribute.AttributeType, context);
				foreach (var argument in attribute.ConstructorArguments)
				{
					AnalyzeAttributerArgument(argument, context);
				}
				foreach (var property in attribute.Properties)
				{
					AnalyzeAttributerArgument(property.Argument, context);
				}
				foreach (var field in attribute.Fields)
				{
					AnalyzeAttributerArgument(field.Argument, context);
				}
			}
			catch (NotSupportedException)
			{ }
		}

		void AnalyzeAttributerArgument(CustomAttributeArgument argument, TypeDefinition context)
		{
			LogUsage(argument.Type, context);
			LogUsage(argument.Value as TypeReference, context);
			AnalyzeString(argument.Value as string);
		}

		void AnalyzeGenericInstance(IGenericInstance generic, TypeDefinition context)
		{
			if (generic != null && generic.HasGenericArguments)
			{
				foreach (var genericTypeRef in generic.GenericArguments)
				{
					LogUsage(genericTypeRef, context);
				}
			}
		}

		void AnalyzeString(string stringValue)
		{
			if (!string.IsNullOrEmpty(stringValue) && stringValue.Contains('.') && stringValue.Contains(','))
			{
				lock (usedTypes)
				{
					usedTypes.Add(GetKey(stringValue));
				}
			}
		}

		void AnalyzeTypeFactoryAnnotationMethod(MethodDefinition method)
		{
			if (method.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(WTG.StaticAnalysis.Annotation.TypeFactoryAnnotationMethodAttribute).FullName))
			{
				var type = Type.GetType(method.DeclaringType.FullName + "," + method.DeclaringType.Module.Assembly.Name.Name, true, true);
				var methodInfo = type.GetMethod(method.Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				if (methodInfo == null || !typeof(IEnumerable<string>).IsAssignableFrom(methodInfo.ReturnType))
				{
					throw new InvalidOperationException("Expected " + method.FullName + " to have the following signature: static void IEnumerable<string> " + method.Name + "()");
				}
				foreach (var typeInfo in (IEnumerable<string>)methodInfo.Invoke(null, null))
				{
					lock (usedTypes)
					{
						usedTypes.Add(GetKey(typeInfo));
					}
				}
				LogUsage(method.DeclaringType, null);
			}
		}

		void LogUsage(TypeReference type, TypeDefinition context)
		{
			if (type != null && !InContext(type, context) && !IsTheTest(type, context) && !type.FullName.StartsWith("System.") && !type.FullName.StartsWith("Microsoft."))
			{
				LogUsed(type);

				// if a nested subclass is used, any parent class should automatically be considered used.
				var parentType = type.DeclaringType;
				while (parentType != null)
				{
					LogUsed(parentType);
					parentType = parentType.DeclaringType;
				}
			}

			void LogUsed(TypeReference typeToLog)
			{
				var resolved = ResolveSafe(typeToLog);
				if (resolved != null)
				{
					lock (usedTypes)
					{
						usedTypes.Add(GetKey(resolved));
					}
				}
			}

			AnalyzeGenericInstance(type as IGenericInstance, context);
		}

		bool InContext(TypeReference type, TypeDefinition context)
		{
			return context != null && (type == context || InContext(type, context.DeclaringType));
		}

		bool IsTheTest(TypeReference type, TypeDefinition context)
		{
			return context != null && (context.Name == type.Name + "Test" || IsTheTest(type, context.DeclaringType));
		}

		bool IsRoot(TypeDefinition type)
		{
			return type != null && (
					rootTypes.Contains(type.FullName) ||
					type.CustomAttributes.Any(a => rootTypes.Contains(a.AttributeType.FullName)) ||
					type.Module.Assembly.Name.Name == "PresentationFramework" || // until we can analyze XAML files
					type.Module.Assembly.Name.Name == "FxCopSdk" ||
					type.Interfaces.Select(i => i.InterfaceType).Any(i => IsRoot(i)) ||
					IsRoot(type.BaseType));
		}

		bool IsRoot(TypeReference type)
		{
			TypeDefinition resolved;
			return type != null && (resolved = ResolveSafe(type)) != null && IsRoot(resolved);
		}

#if NETFRAMEWORK
		readonly string[] rootTypes = new string[]
		{
			typeof(WTG.StaticAnalysis.Annotation.CodeAliveAttribute).FullName,
			typeof(System.Web.UI.Control).FullName,
			typeof(IHttpHandler).FullName,
			typeof(System.Web.Services.WebServiceAttribute).FullName,
			typeof(System.Web.Services.WebService).FullName,
			typeof(System.ServiceModel.IClientChannel).FullName,
			typeof(System.ServiceModel.ServiceContractAttribute).FullName,
			typeof(System.Configuration.Install.Installer).FullName,
			"Microsoft.Build.Framework.ITask",
			typeof(Microsoft.SqlServer.Server.SqlUserDefinedAggregateAttribute).FullName,
			typeof(System.Xml.Serialization.XmlRootAttribute).FullName,
			typeof(IHttpModule).FullName,
			"System.Web.Http.ApiController"
		};

		bool IsKnownRoot(MethodDefinition method)
		{
			return method.CustomAttributes.Any(attr =>
				attr.AttributeType.FullName == typeof(Microsoft.SqlServer.Server.SqlFunctionAttribute).FullName ||
				attr.AttributeType.FullName == typeof(Microsoft.SqlServer.Server.SqlMethodAttribute).FullName ||
				attr.AttributeType.FullName == typeof(Microsoft.SqlServer.Server.SqlProcedureAttribute).FullName ||
				attr.AttributeType.FullName.StartsWith("NUnit.Framework", StringComparison.Ordinal));
		}
#elif NET
		// Since there is no known answer to how to replace multiple types in actual running states,
		// we will briefly deal with net8.0 here for the time being, and then carry out the transformation
		// after the net8.0 upgrade and transformation has been promoted to this point and there is a clear answer.
		readonly string[] rootTypes =
		[
			typeof(WTG.StaticAnalysis.Annotation.CodeAliveAttribute).FullName,
			typeof(System.ServiceModel.IClientChannel).FullName,
			typeof(System.ServiceModel.ServiceContractAttribute).FullName,
			"Microsoft.Build.Framework.ITask",
			typeof(System.Xml.Serialization.XmlRootAttribute).FullName,
			"System.Web.Http.ApiController"
		];

		bool IsKnownRoot(MethodDefinition method)
		{
			return method.CustomAttributes.Any(attr =>
				attr.AttributeType.FullName.StartsWith("NUnit.Framework", StringComparison.Ordinal));
		}
#else
#error Unexpected target platform
#endif

		string GetKey(TypeDefinition type)
		{
			return Path.GetFileNameWithoutExtension(type.Module.Name) + "," + type.FullName;
		}

		string GetKey(string typePath)
		{
			var parts = typePath.Split(',');
			return parts[1].Trim() + "," + parts[0].Trim();
		}

		readonly HashSet<string> usedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		readonly HashSet<string> definedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}
}
