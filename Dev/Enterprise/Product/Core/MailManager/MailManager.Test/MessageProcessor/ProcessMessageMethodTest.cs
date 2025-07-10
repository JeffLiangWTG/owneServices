using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MailManager.Test
{
	class ProcessMessageMethodTest : TestCaseWithFactory
	{
		public void TestAllFilterConditionsCanBeEvaluated()
		{
			var modelMethod = GetType().GetMethod(nameof(TestAllFilterConditionsCanBeEvaluated));
			var cache = new Dictionary<string, BusinessObject>();
			foreach (var (table, condition) in GetAllConditions())
			{
				if (!cache.TryGetValue(table, out var dummy))
				{
					var bizoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(table));
					cache[table] = dummy = Factory.NewWithValidTestData(bizoType);
				}

				var methodInfo = CreateMethodWithAttr(condition, modelMethod);
				var processMethod = new ProcessMessageMethod<object>(methodInfo);

				AssertNoExceptionThrown("Error evaluating " + condition.ToString(), () => processMethod.IsMatch(context, dummy));
			}
		}

		MethodInfo CreateMethodWithAttr(MessageFilterConditionAttribute attribute, MethodInfo model)
			=> new MethodWithAttribute(model, attribute);

		IEnumerable<(string tableName, MessageFilterConditionAttribute condition)> GetAllConditions()
		{
			foreach (var filterConfig in AssemblyMetaDataReader.GetAttributes<MessageFilterAttribute>())
			{
				var type = Type.GetType(Assembly.CreateQualifiedName(filterConfig.TypeAssemblyName, filterConfig.TypeName), false);
				var filterConditions = type.GetMethods().SelectMany(m => m.GetCustomAttributes<MessageFilterConditionAttribute>());

				foreach (var condition in filterConditions)
				{
					yield return (filterConfig.TableName, condition);
				}
			}
		}

		public void TestIsMatch()
		{
			var method1 = GetMethod("FilterMethod1");

			Assert(!method1.IsMatch(context, null));

			var bizObj = Factory.New<DummyBusinessObject>();
			Assert(!method1.IsMatch(context, bizObj));

			bizObj.Z0_VarCharMax = "AAA";
			Assert(!method1.IsMatch(context, bizObj));

			bizObj.Z0_Description = "Test Description!";
			Assert(method1.IsMatch(context, bizObj));

			bizObj.Z0_VarCharMax = "aaa";
			Assert(!method1.IsMatch(context, bizObj));

			bizObj.Z0_VarCharMax = "B";
			Assert(method1.IsMatch(context, bizObj));

			var method2 = GetMethod("FilterMethod2");
			Assert(!method2.IsMatch(context, bizObj));
		}

		public void TestIsMatchWithException()
		{
			Assert(!GetMethod("FilterMethod6").IsMatch(context, Factory.New<DummyBusinessObject>()));
			var expected = "Error|An unhandled exception occured during processing of DummyBusinessObject";
			Assert(logger[0].StartsWith(expected));
			AssertEquals("ArgumentException", ErrorReporter.LastExceptionReported.GetType().Name);
			ErrorReporter.Clear();
		}

		public void TestProcess()
		{
			var method1 = GetMethod("FilterMethod1");

			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Bool = true;
			Assert(method1.Process(context, bizObj1));

			var bizObj2 = Factory.New<DummyBusinessObject>();
			bizObj2.Z0_Bool = false;
			Assert(!method1.Process(context, bizObj2));
		}

		public void TestProcessWithLogger()
		{
			var method2 = GetMethod("FilterMethod2");

			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_VarCharMax = "Test1";
			Assert(method2.Process(context, bizObj1));
			AssertEquals("Information|Test1", logger[0]);

			var bizObj2 = Factory.New<DummyBusinessObject>();
			bizObj2.Z0_VarCharMax = "Test2";
			Assert(method2.Process(context, bizObj2));
			AssertEquals("Information|Test2", logger[1]);
		}

		public void TestProcessWithInvalidParameterType()
		{
			Assert(!GetMethod("FilterMethod3").Process(context, Factory.New<DummyBusinessObject>()));
			var expected = "Error|An unhandled exception occured during processing of DummyBusinessObject";
			Assert(logger[0].StartsWith(expected));
			AssertEquals("NotSupportedException", ErrorReporter.LastExceptionReported.GetType().Name);
			ErrorReporter.Clear();
		}

		public void TestProcessWithGuids()
		{
			var method4 = GetMethod("FilterMethod4");

			var bizObj = Factory.New<DummyBusinessObject>();
			Assert(method4.Process(context, bizObj));
			AssertEquals("Debug|" + bizObj.PK, logger[0]);
			AssertEquals("Debug|" + bizObj.PK, logger[1]);
		}

		public void TestProcessWithException()
		{
			var method5 = GetMethod("FilterMethod5");

			var bizObj = Factory.New<DummyBusinessObject>();
			Assert(!method5.Process(context, bizObj));
			var expected = string.Format("Error|An unhandled exception occured during processing of DummyBusinessObject(PK={{{0}}}).", bizObj.PK);
			Assert(logger[0].StartsWith(expected));
			AssertEquals("FilterMethod5 failed!", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestIsMatchWithPropertyHops()
		{
			var filterMethod8 = GetMethod("FilterMethod8");

			var bizObj = Factory.New<DummyBusinessObject>();
			var related = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizObj.Z0_Guid = related.PK;

			AssertNotNull("PRE: RelatedDummy exists", bizObj.RelatedDummy);
			Assert("Related bizo's property doesn't match the filter", !filterMethod8.IsMatch(context, bizObj));

			related.Z0_VarCharMax = "BLAH";
			Assert("RelatedDummy exists and matches filter", filterMethod8.IsMatch(context, bizObj));
		}

		public void TestIsMatchWithPropertyHops_PropertyPartNull()
		{
			var filterMethod8 = GetMethod("FilterMethod8");

			var bizObj = Factory.New<DummyBusinessObject>();
			AssertNull("PRE: RelatedDummy is null", bizObj.RelatedDummy);
			Assert("Related doesnt exist, so it is not a match", !filterMethod8.IsMatch(context, bizObj));
		}

		class TestFilter
		{
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_VarCharMax, "[A-Z]{1,3}")]
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_Description, "^test", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
			public bool FilterMethod1(DummyBusinessObject bizObj)
			{
				return bizObj.Z0_Bool;
			}

			public bool FilterMethod2(BusinessObject bizObj, ILogger logger)
			{
				logger.Log(LogType.Information, ((DummyBusinessObject)bizObj).Z0_VarCharMax);
				return true;
			}

			public bool FilterMethod3(ILogger logger, int i)
			{
				return true;
			}

			public bool FilterMethod4(Guid pk1, ZGuid pk2, ILogger logger)
			{
				logger.Log(LogType.Debug, pk1.ToString());
				logger.Log(LogType.Debug, pk2.ToString());
				return true;
			}

			public bool FilterMethod5(DummyBusinessObject bizObj)
			{
				throw new Exception("FilterMethod5 failed!");
			}

			[MessageFilterCondition("#!$", "")]
			public bool FilterMethod6()
			{
				return false;
			}

			[MessageFilterCondition("Collection." + DummyBizoSchema.Constants.Z0_VarCharMax, "^ABC$")]
			public bool FilterMethod7()
			{
				return true;
			}

			[MessageFilterCondition("RelatedDummy.Z0_VarCharMax", "BLAH")]
			public bool FilterMethod8()
				=> true;
		}

		ProcessMessageMethod<DummyBusinessObject> GetMethod(string methodName)
		{
			return new ProcessMessageMethod<DummyBusinessObject>(typeof(TestFilter).GetMethod(methodName));
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new TestServiceLogger();
			context = new MessageProcessorContext(logger);
		}

		TestServiceLogger logger;
		MessageProcessorContext context;
	}

	class MethodWithAttribute : MethodInfo
	{
		readonly MethodInfo baseMethod;
		readonly Attribute attribute;

		public MethodWithAttribute(MethodInfo baseMethod, Attribute attribute)
		{
			this.baseMethod = baseMethod;
			this.attribute = attribute;
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			var attributes = GetCustomAttributes(inherit)
				.Where(attr => attr.GetType().IsAssignableFrom(attributeType))
				.ToArray();

			var result = Array.CreateInstance(attributeType, attributes.Length);
			Array.Copy(attributes, result, result.Length);
			return (object[])result;
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes => baseMethod.ReturnTypeCustomAttributes;
		public override RuntimeMethodHandle MethodHandle => baseMethod.MethodHandle;
		public override MethodAttributes Attributes => baseMethod.Attributes;
		public override string Name => baseMethod.Name;
		public override Type DeclaringType => baseMethod.DeclaringType;
		public override Type ReflectedType => baseMethod.ReflectedType;
		public override MethodInfo GetBaseDefinition() => baseMethod.GetBaseDefinition();
		public override object[] GetCustomAttributes(bool inherit) => new[] { attribute };
		public override MethodImplAttributes GetMethodImplementationFlags() => baseMethod.GetMethodImplementationFlags();
		public override ParameterInfo[] GetParameters() => baseMethod.GetParameters();
		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) => baseMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
		public override bool IsDefined(Type attributeType, bool inherit) => baseMethod.IsDefined(attributeType, inherit);
	}
}
