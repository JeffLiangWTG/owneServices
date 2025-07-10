using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace BusinessObjectSecurityCheckpointExtractor
{
	public class CheckpointExtractor
	{
		public readonly string OutputFilename = "BusinessObjectSecurityCheckpoints.json";
		public Dictionary<string, BusinessObjectCheckpoint> GenerateBusinessObjectSecurityCheckpoints()
		{
			var executingAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
			Directory.SetCurrentDirectory(executingAssemblyPath);

			var data = new Dictionary<string, BusinessObjectCheckpoint>();
			var derivedTypes = GetAllDerivedTypes(executingAssemblyPath);

			foreach (var type in derivedTypes)
			{
				var businessObjectType = ExtractBusinessObjectType(type.Controller);
				if (businessObjectType is null)
				{
					continue;
				}

				var checkpoints = GetControllerCheckpoints(type.Controller);

				var moduleCheckpoint = ExtractEnvSecurityFieldFromCheckpointMethod(type.Module, "SecurityCheckpoint", "ZFilterGridModule", out var _);
				if (moduleCheckpoint is not null && moduleCheckpoint.Name != "None")
				{
					checkpoints.Checkpoint = GetCheckpointFullNameFromPropertyName(moduleCheckpoint, string.Empty);
				}

				var key = $"{businessObjectType.FullName}, {businessObjectType.Scope.Name}";
				if (data.TryGetValue(key, out var value))
				{
					MergeCheckpoints(value, checkpoints);
				}
				else
				{
					checkpoints.Controller = $"{type.Controller.FullName}, {type.Controller.Module.Name.Replace(".dll", "")}";
					data[key] = checkpoints;
				}
			}

			return data;
		}

		static BusinessObjectCheckpoint GetControllerCheckpoints(TypeDefinition controller)
		{
			var checkpoints = new BusinessObjectCheckpoint();

			foreach (var name in new[] { "CheckPointForView", "CheckPointForEdit", "CheckPointForNew", "CheckPointForDelete" })
			{
				var field = ExtractEnvSecurityFieldFromCheckpointMethod(controller, name, "ZController", out var prefix);
				if (field is null || field.Name == "None" || field.Name.Contains("NoneCheckpoint") || field.Name == "get_NoneCheckpoint")
				{
					continue;
				}

				var tempController = controller;
				var methodFound = false;
				while (tempController != null && tempController.Name != "ZController")
				{
					if (tempController.Methods.Any(m => m.Name == $"Get{name}"))
					{
						methodFound = true;
						break;
					}

					tempController = GetBaseModuleType(tempController);
				}

				if (!methodFound)
				{
					var fullName = GetCheckpointFullNameFromPropertyName(field, prefix);
					switch (name)
					{
						case "CheckPointForView":
							checkpoints.View = fullName;
							break;
						case "CheckPointForEdit":
							checkpoints.Edit = fullName;
							break;
						case "CheckPointForNew":
							checkpoints.New = fullName;
							break;
						case "CheckPointForDelete":
							checkpoints.Delete = fullName;
							break;
					}
				}
			}

			return checkpoints;
		}

		public static void MergeCheckpoints(BusinessObjectCheckpoint existing, BusinessObjectCheckpoint incoming)
		{
			if (existing.Checkpoint != incoming.Checkpoint)
			{
				existing.Checkpoint = "";
			}

			if (existing.Delete != incoming.Delete)
			{
				existing.Delete = "";
			}

			if (existing.Edit != incoming.Edit)
			{
				existing.Edit = "";
			}

			if (existing.New != incoming.New)
			{
				existing.New = "";
			}

			if (existing.View != incoming.View)
			{
				existing.View = "";
			}
		}

		static string GetCheckpointFullNameFromPropertyName(MemberReference moduleCheckpoint, string prefix)
		{
			var checkpointName = moduleCheckpoint.Name;
			if (moduleCheckpoint is MethodReference)
			{
				checkpointName = checkpointName.Replace("get_", "").Replace(".ctor", "");
			}
			if (!string.IsNullOrEmpty(checkpointName))
			{
				checkpointName = $".{checkpointName}";
			}

			return $"m:{prefix}{moduleCheckpoint.DeclaringType.FullName}{checkpointName}";
		}

		static IEnumerable<(TypeDefinition Controller, TypeDefinition Module)> GetAllDerivedTypes(string directoryPath)
		{
			var assemblyFiles = Directory.GetFiles(directoryPath, "*.Module.dll", SearchOption.TopDirectoryOnly)
			 .Concat(Directory.GetFiles(directoryPath, "*.GUI.dll", SearchOption.TopDirectoryOnly))
			 .Concat(Directory.GetFiles(directoryPath, "*.GUI.UserControls.dll", SearchOption.TopDirectoryOnly))
			 .Concat(Directory.GetFiles(directoryPath, "ZClient*.dll", SearchOption.TopDirectoryOnly));

			var moduleListAssembly = LoadAssembly("Enterprise.ZArchitecture.Modules.dll");

			foreach (var assemblyPath in assemblyFiles)
			{
				var assembly = LoadAssembly(assemblyPath);
				if (assembly is null)
				{
					continue;
				}

				foreach (var module in assembly.Modules)
				{
					foreach (var type in module.Types)
					{
						if (!type.IsAbstract && type.IsClass && type.Name.EndsWith($"Controller") && IsDerivedFrom(type, "ZController"))
						{
							var moduleType = GetModuleType(type, moduleListAssembly, assembly);
							if (moduleType != null)
							{
								yield return (type, moduleType);
							}
						}
					}
				}
			}
		}

		static AssemblyDefinition LoadAssembly(string assemblyPath)
		{
			try
			{
				return AssemblyDefinition.ReadAssembly(assemblyPath);
			}
			catch (Exception)
			{
				return null;
			}
		}

		static bool IsDerivedFrom(TypeDefinition type, string baseTypeName)
		{
			var current = type.BaseType;
			while (current is not null)
			{
				if (current.Name == baseTypeName)
				{
					return true;
				}

				try
				{
					var resolved = current.Resolve();
					current = resolved?.BaseType;
				}
				catch
				{
					break;
				}
			}
			return false;
		}

		static TypeDefinition GetBaseModuleType(TypeDefinition type)
		{
			try
			{
				return type.BaseType.Resolve();
			}
			catch
			{
				return null;
			}
		}

		static TypeReference ExtractBusinessObjectType(TypeDefinition type)
		{
			var property = GetOutermostPropertyOverride(type, "TypeOfTopLevelBusinessObject", "ZController");
			if (property is null)
			{
				return null;
			}

			var getter = property.GetMethod;
			if (!getter.HasBody)
			{
				return null;
			}

			var instructions = getter.Body.Instructions;
			foreach (var instruction in instructions)
			{
				if (instruction.OpCode == OpCodes.Ldtoken)
				{
					if (instruction.Operand is not TypeReference typeRef)
					{
						continue;
					}
					if (typeRef.IsGenericParameter)
					{
						return null;
					}
					else
					{
						return typeRef;
					}
				}
			}

			return null;
		}

		static MemberReference ExtractEnvSecurityFieldFromCheckpointMethod(TypeDefinition type, string checkpointName, string baseTypeName, out string prefix)
		{
			prefix = "";
			var property = GetOutermostPropertyOverride(type, checkpointName, baseTypeName);
			if (property is null)
			{
				return null;
			}

			var getter = property.GetMethod;
			if (!getter.HasBody)
			{
				return null;
			}

			var instructions = getter.Body.Instructions;
			foreach (var instruction in instructions)
			{
				if (instruction.OpCode == OpCodes.Ldfld)
				{
					if (instruction.Operand is not FieldReference fieldRef)
					{
						continue;
					}
					if (fieldRef.FieldType.FullName is "Enterprise.Security.SecurityCheckpoint" or "Enterprise.Security.SecurityCheckpointNonOperationalAllowed" or "Enterprise.Security.ControllerOrExplicitAccessOnlyCheckpoint")
					{
						prefix = $"m:";
						return fieldRef;
					}
				}
				else if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
				{
					var method = (MethodReference)instruction.Operand;
					if (method.Name.StartsWith("get_") && (method.ReturnType.FullName == "Enterprise.Security.SecurityCheckpoint") && type != method.DeclaringType)
					{
						prefix = $"m:";
						return method;
					}
				}
				else if (instruction.OpCode == OpCodes.Newobj)
				{
					var method = (MethodReference)instruction.Operand;
					if (method.Name == ".ctor" && !method.HasParameters && method.DeclaringType.Name.Contains("SecurityCheckpoint"))
					{
						prefix = $"c:";
						return method;
					}
				}
			}

			return null;
		}

		static PropertyDefinition GetOutermostPropertyOverride(TypeDefinition type, string propertyName, string baseTypeName)
		{
			PropertyDefinition property = null;

			while (type is not null)
			{
				property = type.Properties.FirstOrDefault(p =>
					p.Name == propertyName &&
					p.GetMethod is not null &&
					(p.GetMethod.IsVirtual || p.GetMethod.IsReuseSlot || p.GetMethod.IsHideBySig)
				);

				if (property is not null)
				{
					break;
				}

				if (type.BaseType is null || type.BaseType.Name == baseTypeName)
				{
					break;
				}

				var resolvedBase = type.BaseType.Resolve();
				if (resolvedBase is null)
				{
					break;
				}

				type = resolvedBase;
			}

			return property;
		}

		static TypeDefinition GetModuleType(TypeDefinition type, AssemblyDefinition moduleListAssembly, AssemblyDefinition assembly)
		{
			PropertyDefinition moduleIDProp = null;
			while (type != null && moduleIDProp is null)
			{
				moduleIDProp = type.Properties.FirstOrDefault(p => p.Name == "ModuleID");
				if (moduleIDProp != null)
				{
					break;
				}

				type = GetBaseModuleType(type);
			}
			if (moduleIDProp == null)
			{
				return null;
			}

			var fieldRef = GetReturnedStaticField(moduleIDProp.GetMethod);
			if (fieldRef is null)
			{
				return null;
			}

			if (moduleListAssembly is null)
			{
				return null;
			}

			var moduleIDsType = moduleListAssembly.MainModule.Types.FirstOrDefault(t => t.Name == "ModuleIDs");
			if (moduleIDsType is null)
			{
				return null;
			}

			var parts = fieldRef.FullName.Split(new[] { "::" }, StringSplitOptions.None);

			var typePath = parts[0].Split('/');

			var myCurrentType = moduleIDsType;

			for (var i = 1; i < typePath.Length; i++)
			{
				myCurrentType = myCurrentType.NestedTypes.FirstOrDefault(t => t.Name == typePath[i]);
				if (myCurrentType == null)
				{
					return null;
				}
			}

			var field = myCurrentType.Fields.FirstOrDefault(f => f.Name == parts[1]);
			if (field is null)
			{
				return null;
			}

			var moduleInfo = GetModuleAssemblyClassFullNameFromFieldDefinition(moduleListAssembly, field);
			if (moduleInfo.ModuleClassFullName is null)
			{
				return null;
			}

			if (assembly.Name.Name != moduleInfo.AssemblyName.Trim())
			{
				assembly = LoadAssembly($"{moduleInfo.AssemblyName}.dll");
				if (assembly is null)
				{
					return null;
				}
			}
			return assembly.MainModule.Types.FirstOrDefault(t => t.FullName == moduleInfo.ModuleClassFullName);
		}

		static FieldReference GetReturnedStaticField(MethodDefinition getter)
		{
			if (!getter.HasBody)
			{
				return null;
			}

			foreach (var instruction in getter.Body.Instructions)
			{
				if (instruction.OpCode == OpCodes.Ldsfld && instruction.Operand is FieldReference field)
				{
					return field;
				}
			}
			return null;
		}

		static (string AssemblyName, string ModuleClassFullName) GetModuleAssemblyClassFullNameFromFieldDefinition(AssemblyDefinition assembly, FieldDefinition fieldDef)
		{
			var type = assembly.MainModule.Types.FirstOrDefault(t => t.FullName == "Enterprise.ZArchitecture.Modules.ModuleList");
			if (type is null || !type.IsClass || !type.Methods.Any(m => m.Name == ".ctor"))
			{
				return (null, null);
			}

			foreach (var method in type.Methods.Where(m => m.IsConstructor && m.HasBody))
			{
				var instructions = method.Body.Instructions;

				for (var i = 0; i < instructions.Count; i++)
				{
					if (instructions[i].OpCode == OpCodes.Ldsfld && instructions[i].Operand == fieldDef)
					{
						for (var j = i + 1; j < instructions.Count; j++)
						{
							if (instructions[j].OpCode == OpCodes.Newobj &&
								instructions[j].Operand is MethodReference ctor &&
								ctor.DeclaringType.Name == "ModuleInfo")
							{
								if (instructions[i + 2].OpCode == OpCodes.Ldstr && instructions[i + 2].Operand is string str)
								{
									return (instructions[i + 1].Operand.ToString(), instructions[i + 2].Operand.ToString());
								}
							}
						}
					}
				}
			}

			return (null, null);
		}
	}
}
