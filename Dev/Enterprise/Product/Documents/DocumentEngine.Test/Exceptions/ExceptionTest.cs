using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class ExceptionTest : TestCase
	{
		public void TestExceptionsShouldDescendFromExceptionNotApplicationExceptionAsPerMicrosoftDesignGuidelines()
		{
			ZStringBuilder errors = new ZStringBuilder();
			foreach (Type type in GetAllExceptionTypesInAssembly())
			{
				if (type.BaseType == typeof(ApplicationException))
				{
					errors.Append("[" + type.FullName + "] - Should be changed to descend from Exception, not ApplicationException.");
				}
			}
			Assert(@"Exceptions should descend from Exception not ApplicationException. (as per Microsoft Exception Design Guidelines)
" + errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		public void TestAllExceptionsAreInTheRightNamespace()
		{
			string exceptionsNamespace = "Enterprise.DocumentEngine.Exceptions";
			ZStringBuilder errors = new ZStringBuilder();
			foreach (Type type in GetAllExceptionTypesInAssembly())
			{
				if (type.Namespace != exceptionsNamespace && !type.IsNested)
				{
					errors.Append("[" + type.FullName + "] - Must be moved to [" + exceptionsNamespace + "] namespace.");
				}
			}
			Assert(@"All non nested exceptions defined in DocumentEngine must be in the right namespace so they are easy to find.
" + errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		public void TestAllExceptionsAreSerializable()
		{
#pragma warning disable SYSLIB0050 // 'Type.IsSerializable' is obsolete
			ZStringBuilder errors = new ZStringBuilder();
			foreach (Type type in GetAllExceptionTypesInAssembly())
			{
				if (!type.IsSerializable)
				{
					errors.Append("[" + type.FullName + "] - Must have [Serializable] attribute added to class.");
				}
				if (type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(System.Runtime.Serialization.SerializationInfo), typeof(System.Runtime.Serialization.StreamingContext) }, null) == null)
				{
					errors.Append("[" + type.FullName + "] - Must have a constructor with signature: protected " + type.Name + "(SerializationInfo info, StreamingContext context) : base(info, context) { }");
				}
				foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (!field.FieldType.IsInterface &&
						!field.FieldType.IsGenericParameter &&
						!field.FieldType.IsSerializable &&
						!field.FieldType.IsSubclassOf(typeof(MarshalByRefObject)) &&
						!field.IsDefined(typeof(NonSerializedAttribute), false))
					{
						errors.Append("[" + type.FullName + "] - Field '" + field.Name + "' is not serializable. Must either have [NonSerialized] attibute applied or changed to a Serializable type.");
					}
				}
			}
			Assert(@"All exceptions defined in DocumentEngine must be serializable so they can make it across AppDomain boundaries.
" + errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
#pragma warning restore SYSLIB0050 // 'Type.IsSerializable' is obsolete
		}

		List<Type> GetAllExceptionTypesInAssembly()
		{
			List<Type> result = new List<Type>();
			foreach (Type type in Assembly.Load("Enterprise.DocumentEngine").GetTypes())
			{
				if (type.IsSubclassOf(typeof(Exception)))
				{
					result.Add(type);
				}
			}
			Assert("Precondition: ExceptionsFound.Count > 0", result.Count > 0);
			return result;
		}
	}
}
