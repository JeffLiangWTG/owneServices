using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.Definitions;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Xml;
using Mono.Cecil;
using Mono.Cecil.Cil;

[assembly: AssemblyTitle("Assembly Meta Data Extractor")]

namespace AssemblyMetaDataExtractor
{
	class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property name")]
		static void Main(string[] args)
		{
			var subModule = string.Empty; //To Do - populate from args
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			string targetFile = Path.Combine(binPath, "AssemblyMetaData.xml");
			if (File.Exists(targetFile))
			{
				File.Delete(targetFile);
			}
			using (var writer = XmlWriter.Create(targetFile, new XmlWriterSettings() { Indent = true }))
			{
				writer.WriteStartElement("AssemblyMetaData");

				var allData = new ConcurrentDictionary<ushort, ConcurrentDictionary<string, ResourceStringData>>();
				Parallel.ForEach(Directory.GetFiles(binPath), fileName =>
				{
					string extension = Path.GetExtension(fileName);
					if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							using (var assembly = AssemblyDefinition.ReadAssembly(fileName))
							{
								if (assembly.HasCustomAttributes)
								{
									var metaData = new ResourceStringMetaData(assembly.FullName);
									foreach (var customAttribute in assembly.CustomAttributes)
									{
										if (IsAssemblyMetaDataAttribute(customAttribute.AttributeType))
										{
											var attribute = ReadAssemblyMetaDataAttribute(assembly, customAttribute);
											lock (writer)
											{
												ZXmlSerializer.New(attribute.GetType()).Serialize(writer, attribute);
											}
										}

										ICustomizableDataCaptionSource captionDataSource = null;

										if (IsCustomizableDataCaptionSourceAttribute(customAttribute.AttributeType) && !Path.GetFileNameWithoutExtension(fileName).StartsWith("ZClient", StringComparison.OrdinalIgnoreCase))
										{
											captionDataSource = GetCustomizableDataCaptionSource(customAttribute);
										}
										else if (IsResourceStringDataCaptionSourceAttribute(customAttribute.AttributeType))
										{
											var attributeInstance = (ResourceStringDataCaptionSourceAttribute)GetCustomAttributeInstance(customAttribute);
											foreach (var constructorArgument in customAttribute.ConstructorArguments)
											{
												if (constructorArgument.Value is TypeDefinition)
												{
													SetTypeProperties(attributeInstance, "Type", (TypeDefinition)constructorArgument.Value);
												}
											}

											var captionDataSourceConstructor = attributeInstance.CaptionSourceType.GetConstructor(Type.EmptyTypes);
											var captionDataSourceInstance = captionDataSourceConstructor?.Invoke(null);
											if (captionDataSourceInstance is ICustomizableDataCaptionSource source)
											{
												captionDataSource = source;
												metaData = new ResourceStringMetaData(attributeInstance.CaptionSourceType.FullName);
											}
										}

										if (captionDataSource != null)
										{
											var resourceStrings = allData.GetOrAdd(captionDataSource.Asmid, _ => new ConcurrentDictionary<string, ResourceStringData>());

											foreach (var caption in captionDataSource.GetCompileTimeSystemCaptions().ToArray())
											{
												resourceStrings.TryAdd(caption.ResourceKey, new ResourceStringData(caption.ResourceKey, caption.EnglishText, metaData));
											}
										}
									}
								}
							}
						}
						catch (NotSupportedException) { }
						catch (BadImageFormatException) { }
						catch (AssemblyResolutionException) { }
						catch (Exception e)
						{
							Console.Error.Write(e.ToString());
							Environment.Exit(-1);
						}
					}
				});
				writer.WriteEndDocument();

				foreach (var entry in allData)
				{
					if (entry.Value.Any())
					{
#pragma warning disable CW1024 // Bad Concurrent Collection Access - not actually concurrent here
						ZrsFile.Save(ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, subModule, binPath), entry.Key, entry.Value.Values);
#pragma warning restore CW1024 // Bad Concurrent Collection Access
					}
				}
			}
		}

		static bool IsResourceStringDataCaptionSourceAttribute(TypeReference attributeType)
		{
			var attributeDefinition = attributeType.Resolve();
			return attributeDefinition != null && attributeDefinition.FullName == typeof(ResourceStringDataCaptionSourceAttribute).FullName;
		}

		static bool IsCustomizableDataCaptionSourceAttribute(TypeReference attributeType)
		{
			var attributeDefinition = attributeType.Resolve();
			return attributeDefinition != null && attributeDefinition.Interfaces.FirstOrDefault(t => t.InterfaceType.FullName == typeof(ICustomizableDataCaptionSource).FullName) != null;
		}

		static bool IsAssemblyMetaDataAttribute(TypeReference attributeType)
		{
			var attrbuteTypeDefinition = attributeType.Resolve();
			return attrbuteTypeDefinition != null &&
				   attrbuteTypeDefinition.BaseType != null &&
				   (attrbuteTypeDefinition.BaseType.FullName == typeof(AssemblyMetaDataAttribute).FullName ||
					IsAssemblyMetaDataAttribute(attrbuteTypeDefinition.BaseType));
		}

		static ICustomizableDataCaptionSource GetCustomizableDataCaptionSource(CustomAttribute attribute)
		{
			var attributeInstance = (ICustomizableDataCaptionSource)GetCustomAttributeInstance(attribute);
			return attributeInstance;
		}

		static object GetCustomAttributeInstance(CustomAttribute attribute)
		{
			var attributeType = Type.GetType(attribute.AttributeType.FullName + "," + attribute.AttributeType.Resolve().Module.Assembly.Name.Name);
			var attributeConstructor = attributeType.GetConstructor(Array.ConvertAll(attribute.ConstructorArguments.ToArray(), arg => GetType(arg.Value)));
			var attributeInstance = attributeConstructor.Invoke(Array.ConvertAll(attribute.ConstructorArguments.ToArray(), ConvertArgument));
			return attributeInstance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property name")]
		static AssemblyMetaDataAttribute ReadAssemblyMetaDataAttribute(AssemblyDefinition assembly, CustomAttribute attribute)
		{
			var attributeType = Type.GetType(attribute.AttributeType.FullName + "," + attribute.AttributeType.Resolve().Module.Assembly.Name.Name);
			var attributeConstructor = attributeType.GetConstructor(Array.ConvertAll(attribute.ConstructorArguments.ToArray(), arg => GetType(arg.Value)));
			var attributeInstance = (AssemblyMetaDataAttribute)attributeConstructor.Invoke(Array.ConvertAll(attribute.ConstructorArguments.ToArray(), ConvertArgument));
			foreach (var constructorArgument in attribute.ConstructorArguments)
			{
				if (constructorArgument.Value is TypeDefinition)
				{
					SetTypeProperties((AssemblyMetaDataAttributeWithType)attributeInstance, "Type", (TypeDefinition)constructorArgument.Value);
				}
			}

			if (attribute.HasProperties)
			{
				foreach (var property in attribute.Properties)
				{
					if (property.Argument.Value is TypeDefinition)
					{
						SetTypeProperties(attributeInstance, property.Name, (TypeDefinition)property.Argument.Value);
					}
					else if (property.Argument.Value is TypeReference typeReference)
					{
						var typeDefinition = typeReference.Resolve();
						SetTypeProperties(attributeInstance, property.Name, typeDefinition);
					}
					else if (property.Argument.Value is CustomAttributeArgument[])
					{
						var typeOfArray = Type.GetType(property.Argument.Type.FullName, throwOnError: true);
						var typeOfArrayElement = typeOfArray.GetElementType();
						var toArrayFunction = typeof(Program).GetMethod("ToArray", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(typeOfArrayElement);
						var array = (Array)toArrayFunction.Invoke(null, new object[] { property.Argument });

						attributeType.GetProperty(property.Name).SetValue(attributeInstance, array, Array.Empty<object>());
					}
					else
					{
						attributeType.GetProperty(property.Name).SetValue(attributeInstance, property.Argument.Value is TypeReference typeRef ? Type.GetType(typeRef.Resolve().FullName) ?? typeof(object) : property.Argument.Value, Array.Empty<object>());
					}
				}
			}

			var clientSpecificMatch = clientSpecificAssemblyRegex.Match(assembly.Name.Name);
			if (clientSpecificMatch.Success)
			{
				Clients clientCode;
				if (!Enum.TryParse(clientSpecificMatch.Groups[1].Value, true, out clientCode))
				{
					clientCode = GetClientSpecificCode(assembly);
				}
				attributeInstance.ClientSpecificCode = clientCode;
			}

			return attributeInstance;
		}

		static object ConvertArgument(CustomAttributeArgument input)
		{
			if (input.Type.IsArray)
			{
				return ToArray<string>(input);
			}
			return input.Value is TypeDefinition ? null : input.Value;
		}

		public static T[] ToArray<T>(CustomAttributeArgument attributeArgument)
		{
			var source = ((CustomAttributeArgument[])attributeArgument.Value);
			var length = source.Length;
			var destination = new T[length];
			for (var i = 0; i < length; i++)
			{
				destination[i] = (T)source[i].Value;
			}
			return destination;
		}

		static void SetTypeProperties(object attribute, string typeProperty, TypeDefinition typeDefinition)
		{
			attribute.GetType().GetProperty(typeProperty + "AssemblyName").SetValue(attribute, typeDefinition.Module.Assembly.Name.Name, Array.Empty<object>());
			attribute.GetType().GetProperty(typeProperty + "Name").SetValue(attribute, typeDefinition.FullName.Replace('/', '+'), Array.Empty<object>());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer message")]
		static Type GetType(object value)
		{
			if (value is TypeDefinition)
			{
				return typeof(Type);
			}
			else if (value is string)
			{
				return typeof(string);
			}
			else if (value is CustomAttributeArgument[] && ((CustomAttributeArgument[])value).All(element => element.Type.FullName == "System.String"))
			{
				return typeof(string[]);
			}
			else
			{
				string exceptionInfo;
				if (value is CustomAttributeArgument[])
				{
					exceptionInfo = string.Format("Only System.String typed arrays are supported. Types in array were: {0}", string.Join(", ", ((CustomAttributeArgument[])value).Select(element => element.Type.FullName)));
				}
				else
				{
					if (value != null)
					{
						exceptionInfo = value.ToString();
					}
					else
					{
						//hack: perhaps can be identified in a better way
						return typeof(string);
					}
				}
				throw new InvalidDataException("Unsupported attribute constructor/property type: " + exceptionInfo);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		static Clients GetClientSpecificCode(AssemblyDefinition assembly)
		{
			return (Clients)Convert.ToInt32((sbyte)assembly.Modules[0].Types.First(type => type.Name == "ClientOverride").Properties.First(property => property.Name == "Client").GetMethod.Body.Instructions.First(i => i.OpCode != OpCodes.Nop).Operand);
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Console.Error.Write(e.ExceptionObject.ToString());
			Environment.Exit(-1);
		}

		readonly static string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		readonly static Regex clientSpecificAssemblyRegex = new Regex(@"ZClient(\w\w\w)", RegexOptions.Compiled);
	}
}
