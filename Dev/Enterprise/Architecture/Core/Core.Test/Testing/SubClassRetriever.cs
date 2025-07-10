using System;
using System.Linq;
using System.Reflection;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class SubClassRetriever : ClassRetriever
	{
		public SubClassRetriever(Assembly assembly, Type typeToFind)
		{
			Assembly = assembly;
			TypeToFind = typeToFind;
		}

		public SubClassRetriever(string[] assemblies, Type typeToFind)
		{
			Assemblies = assemblies;
			TypeToFind = typeToFind;
		}

		public bool IncludeClientDlls = true;

		protected override bool MatchesConditionsCore(Type type)
		{
			return PassesSubclassCondition(type)
				&& PassedClientDllInclusionCondition(type);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Baseline")]
		bool PassesSubclassCondition(Type type)
		{
			bool result = false;

			if (type.FullName != "<PrivateImplementationDetails>")
			{
				if (TypeToFind.IsGenericTypeDefinition)
				{
					if (type != TypeToFind)
					{
						Type currentType = type;
						while (currentType != null)
						{
							if (currentType.IsGenericType)
							{
								result = currentType.GetGenericTypeDefinition() == TypeToFind;
								if (result)
								{
									break;
								}
							}
							currentType = currentType.BaseType;
						}
					}
				}
				else
				{
					result = type != TypeToFind && TypeToFind.IsAssignableFrom(type);
				}
			}

			return result;
		}

		bool PassedClientDllInclusionCondition(Type type)
		{
			return IncludeClientDlls || !ClientHookLoader.Instance.IsAnyClientOverrideAssembly(type.Assembly.FullName);
		}

		Type TypeToFind
		{
			get { return fTypeToFind; }
			set
			{
				fTypeToFind = value;
				IncludeTestClasses = value.IsSubclassOf(typeof(TestCase)) || value == typeof(TestCase);
			}
		}

		Type fTypeToFind;

		#region Test

		class Test : TestCase
		{
			public void TestPassesSubclassCondition()
			{
				Assembly core = Assembly.GetAssembly(GetType());

				Type[] allTypes = new Type[]
				{
					typeof(BaseClass<>),
					typeof(BaseClass<object>),
					typeof(BaseClass<string>),
					typeof(Class),
					typeof(Class<>),
					typeof(Class<object>),
					typeof(Class<string>),
					typeof(SubClass),
					typeof(AnotherBaseClass),
					typeof(AnotherClass),
					typeof(AnotherClass<>),
					typeof(AnotherClass<object>),
					typeof(AnotherClass<string>),
					typeof(AnotherSubClass),
					typeof(AnotherSubClass<>),
					typeof(AnotherSubClass<object>),
					typeof(AnotherSubClass<string>)
				};

				TestPassesSubclassCondition(typeof(BaseClass<>), allTypes, core,
					typeof(BaseClass<object>), typeof(BaseClass<string>), typeof(Class), typeof(Class<>), typeof(Class<object>),
					typeof(Class<string>), typeof(SubClass));

				TestPassesSubclassCondition(typeof(BaseClass<object>), allTypes, core, typeof(Class), typeof(Class<object>), typeof(SubClass));
				TestPassesSubclassCondition(typeof(BaseClass<string>), allTypes, core, typeof(Class<string>));
				TestPassesSubclassCondition(typeof(Class<>), allTypes, core, typeof(Class<object>), typeof(Class<string>), typeof(SubClass));
				TestPassesSubclassCondition(typeof(Class<object>), allTypes, core, typeof(SubClass));
				TestPassesSubclassCondition(typeof(Class<string>), allTypes, core);
				TestPassesSubclassCondition(typeof(Class), allTypes, core);
				TestPassesSubclassCondition(typeof(SubClass), allTypes, core);

				TestPassesSubclassCondition(typeof(AnotherBaseClass), allTypes, core,
					typeof(AnotherClass), typeof(AnotherClass<>), typeof(AnotherClass<object>), typeof(AnotherClass<string>),
					typeof(AnotherSubClass), typeof(AnotherSubClass<>), typeof(AnotherSubClass<object>), typeof(AnotherSubClass<string>));

				TestPassesSubclassCondition(typeof(AnotherClass), allTypes, core);

				TestPassesSubclassCondition(typeof(AnotherClass<>), allTypes, core,
					typeof(AnotherClass<object>), typeof(AnotherClass<string>), typeof(AnotherSubClass), typeof(AnotherSubClass<>),
					typeof(AnotherSubClass<object>), typeof(AnotherSubClass<string>));

				TestPassesSubclassCondition(typeof(AnotherClass<object>), allTypes, core, typeof(AnotherSubClass), typeof(AnotherSubClass<object>));
				TestPassesSubclassCondition(typeof(AnotherClass<string>), allTypes, core, typeof(AnotherSubClass<string>));
				TestPassesSubclassCondition(typeof(AnotherSubClass), allTypes, core);
				TestPassesSubclassCondition(typeof(AnotherSubClass<>), allTypes, core, typeof(AnotherSubClass<object>), typeof(AnotherSubClass<string>));
				TestPassesSubclassCondition(typeof(AnotherSubClass<object>), allTypes, core);
				TestPassesSubclassCondition(typeof(AnotherSubClass<string>), allTypes, core);
			}

			void TestPassesSubclassCondition(Type typeToFind, Type[] allTypes, Assembly assembly, params Type[] passingTypes)
			{
				SubClassRetriever retriever = new SubClassRetriever(assembly, typeToFind);
				foreach (Type type in allTypes)
				{
					AssertEquals("PassesSubclassCondition(typeof(" + type.Name + "))", ContainsType(passingTypes, type), retriever.PassesSubclassCondition(type));
				}
			}

			public void TestPassesExclusionAttributesCondition()
			{
				var assembly = Assembly.GetAssembly(GetType());
				var retriever = new SubClassRetriever(assembly, typeof(Parent));
				retriever.IncludeTestClasses = true;
				retriever.ExcludedAttributes = new Type[] { typeof(Exclude1Attribute), typeof(Exclude2Attribute) };
				var retrievedClasses = retriever.Retrieve();

				Assert("Child1 should be excluded.", !retrievedClasses.Contains(typeof(Child1)));
				Assert("Child2 should be excluded.", !retrievedClasses.Contains(typeof(Child2)));
				Assert("Child3 should be inlcuded.", retrievedClasses.Contains(typeof(Child3)));
			}

			#region Help TestPassesExclusionAttributeCondition and TestPassesExclusionAttributesCondition

			class Parent { }
			[Exclude1]
			class Child1 : Parent { }
			[Exclude2]
			class Child2 : Parent { }
			class Child3 : Parent { }
			[AttributeUsage(AttributeTargets.Class)]
			sealed class Exclude1Attribute : Attribute { }
			[AttributeUsage(AttributeTargets.Class)]
			sealed class Exclude2Attribute : Attribute { }

			#endregion

			bool ContainsType(Type[] types, Type typeToFind)
			{
				foreach (Type type in types)
				{
					if (type == typeToFind)
					{
						return true;
					}
				}
				return false;
			}

			class BaseClass<T> { }
			class Class : BaseClass<object> { }
			class Class<T> : BaseClass<T> { }
			class SubClass : Class<object> { }
			class AnotherBaseClass { }
			class AnotherClass : AnotherBaseClass { }
			class AnotherClass<T> : AnotherBaseClass { }
			class AnotherSubClass : AnotherClass<object> { }
			class AnotherSubClass<T> : AnotherClass<T> { }
		}

		#endregion
	}
}
