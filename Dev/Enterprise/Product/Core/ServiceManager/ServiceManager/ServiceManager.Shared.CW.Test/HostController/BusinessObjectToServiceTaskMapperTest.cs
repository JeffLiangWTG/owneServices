using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared.Interfaces;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.NudgingClient;
using ServiceManager.Integration.NudgingClient.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.HostsController.Testing
{
	public class BusinessObjectBindingsAttributesTest : TestCaseWithFactory
	{
		public void TestValidateAssertionRules()
		{
			var serviceTasksAttributes = new List<HostedServiceAttribute>
			{
				new HostedServiceAttribute("ST1", "AAA", "BBB", typeof(object)),
				new HostedServiceAttribute("ST2", "BBB", "CCC", typeof(object))
			};

			var bindingsForTest = new List<HostedServiceBusinessObjectBindingAttribute>
			{
				new HostedServiceBusinessObjectBindingAttribute("$%^", "StmServiceHost", Array.Empty<string>(), null), // unknown 3 letter task code

				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHoZt", Array.Empty<string>(), null), // unknown table

				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IZActive=Y" }, null), // equals - unknown column
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IsActive=OK" }, null), // equals - unknown value
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IsActive =Y" }, null), // equals - unsupported, with spaces, will not truncate
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IsActive=" }, null), // equals - empty value
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IsActive=Y" }, null), // equals - all good

				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE %BC" }, null), // like - start
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE AB%" }, null), // like - end
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE %B%" }, null), // like - contains
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE ABC" }, null), // like - no wildcard - not supported
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE A%C" }, null), // like - wildcard in the middle - not supported
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE A%%" }, null), // like - double wildcard - only one wildcard will be evaluated
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE %%C" }, null), // like - double wildcard - only one wildcard will be evaluated
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName  LIKE %BC" }, null), // like - even more spaces
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName= %LIKE " }, null), // like and equals mix - #1
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE =%" }, null), // like and equals mix - #1
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE %" }, null), // like - wildcard only #1
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName LIKE %%" }, null), // like - wildcard only #2
				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostNameLIKE ABC" }, null), // like - no space before like - should fail

				new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IsActive&Y" }, null) // all cases - no known operator
			};

			var notLikeTest = new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName NOT like %ABC%" }, null);
			var notLikeExclamationTest = new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName !like %ABC%" }, null);
			var notStartsTest = new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName Not like ABC%" }, null);
			var notEndsTest = new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_HostName not like %ABC" }, null);
			var notEqualsTest = new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IsActive!=Y" }, null);
			var notEqualsNotOperatorTest = new HostedServiceBusinessObjectBindingAttribute("ST1", "StmServiceHost", new string[] { "SH_IsActive not =Y" }, null);

			bindingsForTest.Add(notLikeTest);
			bindingsForTest.Add(notLikeExclamationTest);
			bindingsForTest.Add(notStartsTest);
			bindingsForTest.Add(notEndsTest);
			bindingsForTest.Add(notEqualsTest);
			bindingsForTest.Add(notEqualsNotOperatorTest);

			var result = CheckBizOBindingsCorrectness(bindingsForTest, serviceTasksAttributes);
			Assert("Validation check should not be empty", result.Any());
			var comparison = string.Join(System.Environment.NewLine, result);

#if NET
			var badParameter = """
				 (Parameter 'column')
				""";
#elif NETFRAMEWORK
			var badParameter = """

				Parameter name: column
				""";
#endif
			var expectedMessage = $"""
				Binding with code $%^ table StmServiceHost - associated code is unknown: '$%^'
				Binding with code ST1 table StmServiceHoZt - associated table is unknown: 'StmServiceHoZt'
				Binding with code ST1 table StmServiceHost - associated predicate is invalid: 'SH_IZActive=Y' . Exception was: Failed to get column 'SH_IZActive' for table 'StmServiceHost'{badParameter}
				Binding with code ST1 table StmServiceHost - associated predicate is invalid: 'SH_IsActive=OK' . Exception was: OK is not a valid value for Boolean.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_IsActive =Y'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_IsActive='.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_IsActive=Y'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE %BC'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE AB%'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE %B%'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE ABC'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE A%C'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE A%%'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE %%C'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName  LIKE %BC'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName= %LIKE '.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE =%'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE %'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName LIKE %%'.
				Binding with code ST1 table StmServiceHost - associated predicate is invalid: 'SH_HostNameLIKE ABC' . Exception was: Unsupported predicateValue [SH_HostNameLIKE ABC] for table [StmServiceHost].
				Binding with code ST1 table StmServiceHost - associated predicate is invalid: 'SH_IsActive&Y' . Exception was: Unsupported predicateValue [SH_IsActive&Y] for table [StmServiceHost].
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName NOT like %ABC%'.
				Binding with code ST1 table StmServiceHost - associated predicate is invalid: 'SH_HostName !like %ABC%' . Exception was: Unsupported predicateValue [SH_HostName !like %ABC%] for table [StmServiceHost].
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName Not like ABC%'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_HostName not like %ABC'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_IsActive!=Y'.
				Binding with code ST1 table StmServiceHost - predicate was successfully parsed: 'SH_IsActive not =Y'.
				""";

			AssertMultilineASCIIEquals("Correct message is shown", expectedMessage, comparison);
		}

		public void TestAllBindingAttributesHaveValidAssociatedServiceTasks()
		{
			var bizOTaskBindings = AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>();
			var serviceTasksAttributes = AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>().ToList();
			var result = CheckBizOBindingsCorrectness(bizOTaskBindings, serviceTasksAttributes);
			var failures = result.Where(line => !line.Contains("was successfully parsed")).ToList();
			if (failures.Any())
			{
				Assert(string.Join(System.Environment.NewLine, failures), false);
			}
			else
			{
				Assert(true);
			}
		}

		ICollection<string> CheckBizOBindingsCorrectness(IEnumerable<HostedServiceBusinessObjectBindingAttribute> bizOTaskBindings, IEnumerable<HostedServiceAttribute> serviceTasksAttributes)
		{
			var schemaResolver = new EnterpriseSchemaResolver();
			var result = new List<string>();
			foreach (var bindingAttribute in bizOTaskBindings)
			{
				if (serviceTasksAttributes.All(taskAttribute => taskAttribute.Code != bindingAttribute.ServiceTaskCode))
				{
					result.Add(string.Format("Binding with code {0} table {1} - associated code is unknown: '{0}'",
						bindingAttribute.ServiceTaskCode,
						bindingAttribute.Table));
				}

				var tableSchema = schemaResolver.GetTableSchema(bindingAttribute.Table);
				if (tableSchema != null)
				{
					foreach (var predicate in bindingAttribute.Predicates)
					{
						Validate(predicate, bindingAttribute).ForEach(line => result.Add(line));
					}
				}
				else
				{
					result.Add(
						string.Format("Binding with code {0} table {1} - associated table is unknown: '{1}'",
							bindingAttribute.ServiceTaskCode,
							bindingAttribute.Table));
				}
			}
			return result;
		}

		static IEnumerable<string> Validate(string predicateString, HostedServiceBusinessObjectBindingAttribute bindingAttribute)
		{
			var result = new List<string>();

			try
			{
				var nudgingSchemaResolver = new NudgingSchemaResolver(new EnterpriseSchemaResolver());
				var predicateFactory = new PredicateFactory(nudgingSchemaResolver);
				var predicate = predicateFactory.GeneratePredicates(new[] { predicateString }, bindingAttribute.Table).SingleOrDefault();
				if (predicate != null)
				{
					result.Add(string.Format(
						"Binding with code {0} table {1} - predicate was successfully parsed: '{2}'.",
						bindingAttribute.ServiceTaskCode, bindingAttribute.Table, predicateString));
				}
				else
				{
					result.Add(string.Format(
						"Binding with code {0} table {1} - associated predicate is invalid: '{2}'.",
						bindingAttribute.ServiceTaskCode, bindingAttribute.Table, predicateString));
				}
			}
			catch (Exception ex)
			{
				result.Add(string.Format(
					"Binding with code {0} table {1} - associated predicate is invalid: '{2}' . Exception was: {3}",
					bindingAttribute.ServiceTaskCode, bindingAttribute.Table, predicateString, ex.Message));
			}

			return result;
		}
	}

	class BusinessObjectToServiceTaskMapperTest : TestCaseWithFactory
	{
		public void TestNoPredicatesMatch()
		{
			// Arrange

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", Array.Empty<string>(), null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);

			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(12, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestSingleAttributesSingleMatch()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool=Y" }, null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool=N" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte=2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code=Code1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code=Code2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date=" + datetime1.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date=" + datetime2.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_AnotherDecimal=1.1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_AnotherDecimal=2.2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid=" + guid1 }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid=" + guid2 }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.1", "DummyBizo", new[] { "Z0_Code like %de1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.2", "DummyBizo", new[] { "Z0_Code LIKE %e2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("8.1", "DummyBizo", new[] { "Z0_Code like Code1%" }, null),
					new HostedServiceBusinessObjectBindingAttribute("8.2", "DummyBizo", new[] { "Z0_Code LIKE %Code2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("9.1", "DummyBizo", new[] { "Z0_Code like %Code1%" }, null),
					new HostedServiceBusinessObjectBindingAttribute("9.2", "DummyBizo", new[] { "Z0_Code LIKE %Code2%" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(18, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestNegationAttributesSingleMatch()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool!=N" }, null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool!=Y" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte!=2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte!=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code!=Code2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code!=Code1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date!=" + datetime2.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date!=" + datetime1.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_Decimal!=2.1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_Decimal!=1.2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid not =" + guid2 }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid not =" + guid1 }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.1", "DummyBizo", new[] { "Z0_Code NOT like %de2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.2", "DummyBizo", new[] { "Z0_Code not LIKE %e1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("8.1", "DummyBizo", new[] { "Z0_Code NOT like Code2%" }, null),
					new HostedServiceBusinessObjectBindingAttribute("8.2", "DummyBizo", new[] { "Z0_Code not LIKE %Code1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("9.1", "DummyBizo", new[] { "Z0_Code NOT like %Code2%" }, null),
					new HostedServiceBusinessObjectBindingAttribute("9.2", "DummyBizo", new[] { "Z0_Code not LIKE %Code1%" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_Decimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(18, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestMultipleAttributesSingleMatch()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool=Y", "Z0_Byte=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool=N", "Z0_Byte=2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte=1", "Z0_Code=Code1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte=2", "Z0_Code=Code2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code=Code1", "Z0_Date=" + datetime1.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code=Code2", "Z0_Date=" + datetime2.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date=" + datetime1.ToLongTimeString(), "Z0_AnotherDecimal=1.1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date=" + datetime2.ToLongTimeString(), "Z0_AnotherDecimal=2.2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_AnotherDecimal=1.1", "Z0_Guid=" + guid1 }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_AnotherDecimal=2.2", "Z0_Guid=" + guid2 }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid=" + guid1, "Z0_Bool=Y" }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid=" + guid2, "Z0_Bool=N" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.1", "DummyBizo", new[] { "Z0_Bool=Y", "Z0_Byte=1", "Z0_Code like %1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.2", "DummyBizo", new[] { "Z0_Bool=N", "Z0_Byte=2", "Z0_Code like %2" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(14, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestSingleAttributesMultipleBizOsAllMatch()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool=Y" }, null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool=N" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte=2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code=Code1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code=Code2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date=" + datetime1.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date=" + datetime2.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_AnotherDecimal=1.1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_AnotherDecimal=2.2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid=" + guid1 }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid=" + guid2 }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.1", "DummyBizo", new[] { "Z0_Code like %de1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.2", "DummyBizo", new[] { "Z0_Code LIKE %e2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("8.1", "DummyBizo", new[] { "Z0_Code like Code1%" }, null),
					new HostedServiceBusinessObjectBindingAttribute("8.2", "DummyBizo", new[] { "Z0_Code LIKE %Code2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("9.1", "DummyBizo", new[] { "Z0_Code like %Code1%" }, null),
					new HostedServiceBusinessObjectBindingAttribute("9.2", "DummyBizo", new[] { "Z0_Code LIKE %Code2%" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var bizO1B = Factory.New<DummyBusinessObject>();
			bizO1B.Z0_Bool = new ZBool(true);
			bizO1B.Z0_Byte = new ZByte(1);
			bizO1B.Z0_Code = "Code1";
			bizO1B.Z0_Date = datetime1;
			bizO1B.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1B.Z0_Guid = guid1;
			bizOs.Add(bizO1B);

			var bizO2B = Factory.New<DummyBusinessObject>();
			bizO2B.Z0_Bool = new ZBool(false);
			bizO2B.Z0_Byte = new ZByte(2);
			bizO2B.Z0_Code = "Code2";
			bizO2B.Z0_Date = datetime2;
			bizO2B.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2B.Z0_Guid = guid2;
			bizOs.Add(bizO2B);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(18, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestMultipleAttributesMultipleBizOsAllMatch()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool=Y", "Z0_Byte=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool=N", "Z0_Byte=2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte=1", "Z0_Code=Code1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte=2", "Z0_Code=Code2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code=Code1", "Z0_Date=" + datetime1.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code=Code2", "Z0_Date=" + datetime2.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date=" + datetime1.ToLongTimeString(), "Z0_AnotherDecimal=1.1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date=" + datetime2.ToLongTimeString(), "Z0_AnotherDecimal=2.2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_AnotherDecimal=1.1", "Z0_Guid=" + guid1 }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_AnotherDecimal=2.2", "Z0_Guid=" + guid2 }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid=" + guid1, "Z0_Bool=Y" }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid=" + guid2, "Z0_Bool=N" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.1", "DummyBizo", new[] { "Z0_Bool=Y", "Z0_Byte=1", "Z0_Code like %1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.2", "DummyBizo", new[] { "Z0_Bool=N", "Z0_Byte=2", "Z0_Code like %2" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var bizO1B = Factory.New<DummyBusinessObject>();
			bizO1B.Z0_Bool = new ZBool(true);
			bizO1B.Z0_Byte = new ZByte(1);
			bizO1B.Z0_Code = "Code1";
			bizO1B.Z0_Date = datetime1;
			bizO1B.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1B.Z0_Guid = guid1;
			bizOs.Add(bizO1B);

			var bizO2B = Factory.New<DummyBusinessObject>();
			bizO2B.Z0_Bool = new ZBool(false);
			bizO2B.Z0_Byte = new ZByte(2);
			bizO2B.Z0_Code = "Code2";
			bizO2B.Z0_Date = datetime2;
			bizO2B.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2B.Z0_Guid = guid2;
			bizOs.Add(bizO2B);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(14, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestMultipleAttributesPartialMatch()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));
			var guid2modified = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 9));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool=Y", "Z0_Byte=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool=N", "Z0_Byte=5" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte=1", "Z0_Code=Code1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte=2", "Z0_Code=OTHERCode2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code=Code1", "Z0_Date=" + datetime1.ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code=Code2", "Z0_Date=" + datetime2.AddDays(1).ToLongTimeString() }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date=" + datetime1.ToLongTimeString(), "Z0_AnotherDecimal=1.1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date=" + datetime2.ToLongTimeString(), "Z0_AnotherDecimal=3.2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_AnotherDecimal=1.1", "Z0_Guid=" + guid1 }, null),
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_AnotherDecimal=2.2", "Z0_Guid=" + guid2modified }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid=" + guid1, "Z0_Bool=Y" }, null),
					new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid=" + guid2, "Z0_Bool=Y" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.1", "DummyBizo", new[] { "Z0_Guid=" + guid1, "Z0_Bool=Y", "Z0_Code like %de1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("7.2", "DummyBizo", new[] { "Z0_Guid=" + guid2, "Z0_Bool=N", "Z0_Code like %OTHER" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var bizO1B = Factory.New<DummyBusinessObject>();
			bizO1B.Z0_Bool = new ZBool(true);
			bizO1B.Z0_Byte = new ZByte(1);
			bizO1B.Z0_Code = "Code1";
			bizO1B.Z0_Date = datetime1;
			bizO1B.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1B.Z0_Guid = guid1;
			bizOs.Add(bizO1B);

			var bizO2B = Factory.New<DummyBusinessObject>();
			bizO2B.Z0_Bool = new ZBool(false);
			bizO2B.Z0_Byte = new ZByte(2);
			bizO2B.Z0_Code = "Code2";
			bizO2B.Z0_Date = datetime2;
			bizO2B.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2B.Z0_Guid = guid2;
			bizOs.Add(bizO2B);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(7, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestDifferentBusinessObjectTable()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyDependentBizo", new[] { "ZD1_Code=Z", "ZD1_Number=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool=Y", "Z0_Byte=1" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyDependentBizo", new[] { "ZD1_Code=B", "ZD1_Number=2" }, null),
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Code=Code1", "Z0_AnotherDecimal=1.1" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyDependantBusinessObject>();
			bizO2.ZD1_Code = "Z";
			bizO2.ZD1_Number = 1;
			bizOs.Add(bizO2);

			var bizO3 = Factory.New<DummyDependantBusinessObject>();
			bizO3.ZD1_Code = "B";
			bizO3.ZD1_Number = 2;
			bizOs.Add(bizO3);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			schemaResolver.Setup(x => x.GetTableSchema("DummyDependentBizo")).Returns(DummyDependentBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(4, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
			}
			AssertEquals("DummyDependentBizo", result["1.1"].FirstOrDefault());
			AssertEquals("DummyBizo", result["1.2"].FirstOrDefault());
			AssertEquals("DummyDependentBizo", result["2.1"].FirstOrDefault());
			AssertEquals("DummyBizo", result["2.2"].FirstOrDefault());
		}

		public void TestSingleAttributesEmptyPredicates()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));

			var mapper = new BusinessObjectToServiceTaskMapperForTest();
			mapper.BizOTaskBindings = ImmutableList.Create(new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "" }, null)
			});

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_Decimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings)
				.Returns(Enumerable.Empty<IServiceTaskBinding>())
				.Callback(() => {
					var bindings = mapper.GenerateCompiledBindings(schemaResolver.Object);
					nudgingController.Setup(x => x.ServiceTaskBindings).Returns(bindings);
				});

			// Act
			// Assert
			var result = AssertExceptionThrown<NotSupportedException>(() => mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
			AssertEquals("Unsupported predicateValue [] for table [DummyBizo].", result.Message);
		}

		public void TestSingleAttributesNoPredicates()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest();
			mapper.BizOTaskBindings = ImmutableList.Create(new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", Array.Empty<string>(), null)
			});

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_Decimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(12, result.Count);
		}

		public void TestSingleAttributesContainsMatchingOnDifferentTypes()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest();
			mapper.BizOTaskBindings = ImmutableList.Create(new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool like %Y" }, null),
				new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool LIKE N%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte like %1" }, null),
				new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte LIKE 2%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code like %1%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code LIKE %2%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date like %" + datetime1.ToString() + "%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date LIKE %" + datetime2.ToString() + "%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_AnotherDecimal like %.1" }, null),
				new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_AnotherDecimal LIKE 2.%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid like %060708090001%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid LIKE %050607080900%" }, null)
			});

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(12, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestSingleAttributesContainsCoveringMatchingOnDifferentTypes()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;
			var datetime2 = datetime1.AddYears(1);

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));
			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest();
			mapper.BizOTaskBindings = ImmutableList.Create(new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool like %" }, null),
				new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool LIKE %%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("2.1", "DummyBizo", new[] { "Z0_Byte like %" }, null),
				new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte LIKE %%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code like %ode%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Code LIKE Cod%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "Z0_Date like %" }, null),
				new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date LIKE %%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_AnotherDecimal like %.%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_AnotherDecimal LIKE %" }, null),
				new HostedServiceBusinessObjectBindingAttribute("6.1", "DummyBizo", new[] { "Z0_Guid like %06070809%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("6.2", "DummyBizo", new[] { "Z0_Guid LIKE %070809%" }, null),
				new HostedServiceBusinessObjectBindingAttribute("7.1", "DummyBizo", new[] { "Z0_Code like %" }, null),
				new HostedServiceBusinessObjectBindingAttribute("7.2", "DummyBizo", new[] { "Z0_Code like %%" }, null)
			});

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_AnotherDecimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_AnotherDecimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(14, result.Count);
			foreach (var task in result)
			{
				AssertEquals(1, task.Value.Count);
				AssertEquals("DummyBizo", task.Value.FirstOrDefault());
			}
		}

		public void TestPredicateCase_Equals_Pre_Space()
		{
			// Arrange
			var datetime1 = ZDateTime.BrettsBirthday;

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.1", "DummyBizo", new[] { "Z0_Bool =Y" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_Decimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act

			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert

			AssertEquals(1, result.Count);
		}

		public void TestPredicateCase_Equals_Post_Space()
		{
			// Arrange
			var datetime2 = ZDateTime.BrettsBirthday.AddYears(1);

			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("1.2", "DummyBizo", new[] { "Z0_Bool= N" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act
			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert
			AssertEquals(1, result.Count);
		}

		public void TestIncorrectPredicateCase_Equals_BothSpaces()
		{
			// Arrange
			var datetime2 = ZDateTime.BrettsBirthday.AddYears(1);

			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("2.2", "DummyBizo", new[] { "Z0_Byte = 2" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act
			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert
			AssertEquals(1, result.Count);
		}

		public void TestIncorrectPredicateCase_Like_EmptyValue()
		{
			// Arrange
			var datetime1 = ZDateTime.BrettsBirthday;

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("3.1", "DummyBizo", new[] { "Z0_Code LIKE " }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_Decimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings)
				.Returns(Enumerable.Empty<IServiceTaskBinding>())
				.Callback(() => {
					var bindings = mapper.GenerateCompiledBindings(schemaResolver.Object);
					nudgingController.Setup(x => x.ServiceTaskBindings).Returns(bindings);
				});

			// Act
			// Assert
			var result = AssertExceptionThrown<InvalidOperationException>(() => mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
			AssertEquals("Failed to calculate predicate for for [DummyBizo].[Z0_Code]: [Z0_Code LIKE ].", result.Message);
		}

		public void TestIncorrectPredicateCase_Like_NoColumnName()
		{
			// Arrange
			var datetime2 = ZDateTime.BrettsBirthday.AddYears(1);

			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest();
			mapper.BizOTaskBindings = ImmutableList.Create(new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { " like %de2" }, null)
			});

			var bizOs = new List<BusinessObject>();

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings)
				.Returns(Enumerable.Empty<IServiceTaskBinding>())
				.Callback(() => {
					var bindings = mapper.GenerateCompiledBindings(schemaResolver.Object);
					nudgingController.Setup(x => x.ServiceTaskBindings).Returns(bindings);
				});
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentException>(() => mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
			AssertStartsWith("Unexcepted error message.", "Failed to get column", result.Message);
			AssertEquals("column", result.ParamName);
		}

		public void TestIncorrectPredicateCase_Like_NoSpaceBeforeLike()
		{
			// Arrange
			var datetime2 = ZDateTime.BrettsBirthday.AddYears(1);

			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "Z0_Codelike %de2" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings)
				.Returns(Enumerable.Empty<IServiceTaskBinding>())
				.Callback(() => {
					var bindings = mapper.GenerateCompiledBindings(schemaResolver.Object);
					nudgingController.Setup(x => x.ServiceTaskBindings).Returns(bindings);
				});
			// Act
			// Assert
			var result = AssertExceptionThrown<NotSupportedException>(() => mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
			AssertEquals("Unsupported predicateValue [Z0_Codelike %de2] for table [DummyBizo].", result.Message);
		}

		public void TestIncorrectPredicateCase_Like_NoColumnName2()
		{
			// Arrange
			var datetime2 = ZDateTime.BrettsBirthday.AddYears(1);

			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("3.2", "DummyBizo", new[] { "like %de2" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings)
				.Returns(Enumerable.Empty<IServiceTaskBinding>())
				.Callback(() => {
					var bindings = mapper.GenerateCompiledBindings(schemaResolver.Object);
					nudgingController.Setup(x => x.ServiceTaskBindings).Returns(bindings);
				});
			// Act
			// Assert
			var result = AssertExceptionThrown<NotSupportedException>(() => mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
			AssertEquals("Unsupported predicateValue [like %de2] for table [DummyBizo].", result.Message);
		}

		public void TestIncorrectPredicateCase_Equals_NoColumnName()
		{
			// Arrange
			var datetime1 = ZDateTime.BrettsBirthday;

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("4.1", "DummyBizo", new[] { "=" + datetime1.ToLongTimeString() }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_Decimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings)
				.Returns(Enumerable.Empty<IServiceTaskBinding>())
				.Callback(() => {
					var bindings = mapper.GenerateCompiledBindings(schemaResolver.Object);
					nudgingController.Setup(x => x.ServiceTaskBindings).Returns(bindings);
				});
			// Act
			// Assert
			var result = AssertExceptionThrown<NotSupportedException>(() => mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
			AssertEquals("Unsupported predicateValue [=18-Sep-71 00:00] for table [DummyBizo].", result.Message);
		}

		public void TestIncorrectPredicateCase_NoOperand()
		{
			// Arrange
			var datetime2 = ZDateTime.BrettsBirthday.AddYears(1);

			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("4.2", "DummyBizo", new[] { "Z0_Date " + datetime2.ToLongTimeString() }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings)
				.Returns(Enumerable.Empty<IServiceTaskBinding>())
				.Callback(() => {
					var bindings = mapper.GenerateCompiledBindings(schemaResolver.Object);
					nudgingController.Setup(x => x.ServiceTaskBindings).Returns(bindings);
				});
			// Act
			// Assert
			var result = AssertExceptionThrown<NotSupportedException>(() => mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
			AssertEquals("Unsupported predicateValue [Z0_Date 18-Sep-72 00:00] for table [DummyBizo].", result.Message);
		}

		public void TestIncorrectPredicateCase_Like_MatchingMask_1()
		{
			// Arrange

			var datetime1 = ZDateTime.BrettsBirthday;

			var guid1 = new ZGuid(new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1));

			var mapper = new BusinessObjectToServiceTaskMapperForTest();
			mapper.BizOTaskBindings = ImmutableList.Create(new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", new[] { "Z0_Decimal like %%%%%" }, null)
			});

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Bool = new ZBool(true);
			bizO1.Z0_Byte = new ZByte(1);
			bizO1.Z0_Code = "Code1";
			bizO1.Z0_Date = datetime1;
			bizO1.Z0_Decimal = new ZDecimal(1.1d);
			bizO1.Z0_Guid = guid1;
			bizOs.Add(bizO1);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act
			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert
			nudgingController.Verify(nc => nc.ReportNudgeIgnored(null, "No records matched the predicates for task(s)='5.1'"), Times.Once);
			AssertEquals(0, result.Count);
		}

		public void TestIncorrectPredicateCase_Like_MatchingMask2()
		{
			// Arrange
			var datetime2 = ZDateTime.BrettsBirthday.AddYears(1);

			var guid2 = new ZGuid(new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0));

			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("5.2", "DummyBizo", new[] { "Z0_Decimal LIKE % % %" }, null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Bool = new ZBool(false);
			bizO2.Z0_Byte = new ZByte(2);
			bizO2.Z0_Code = "Code2";
			bizO2.Z0_Date = datetime2;
			bizO2.Z0_Decimal = new ZDecimal(2.2d);
			bizO2.Z0_Guid = guid2;
			bizOs.Add(bizO2);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act
			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert
			nudgingController.Verify(nc => nc.ReportNudgeIgnored(null, "No records matched the predicates for task(s)='5.2'"), Times.Once);
			AssertEquals(0, result.Count);
		}

		public void TestMapBindingsWhenNoSuitableBizOs()
		{
			// Arrange
			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo", Array.Empty<string>(), null)
				})
			};

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));
			var bizOs = new List<BusinessObject>();

			// Act
			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert
			nudgingController.Verify(nc => nc.ReportNudgeIgnored(null, "No business objects were added or modified"), Times.Once());
			AssertEquals(0, result.Count);

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizOs.Add(bizO1);
			bizO1.Delete();

			result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);
			nudgingController.Verify(nc => nc.ReportNudgeIgnored(null, "No business objects were added or modified"), Times.Exactly(2));
			AssertEquals(0, result.Count);
		}

		public void TestMapBindingsWhenTableNotBinded()
		{
			// Arrange
			var mapper = new BusinessObjectToServiceTaskMapperForTest
			{
				BizOTaskBindings = ImmutableList.Create(new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("5.1", "DummyBizo2", Array.Empty<string>(), null)
				})
			};

			var bizOs = new List<BusinessObject>();

			var bizO1 = Factory.New<DummyBusinessObject>();
			bizOs.Add(bizO1);

			var schemaResolver = new Mock<IApplicationSchemaResolver>();
			schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns(mapper.GenerateCompiledBindings(schemaResolver.Object));

			// Act
			var result = mapper.MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs);

			// Assert
			nudgingController.Verify(nc => nc.ReportNudgeIgnored(null, "No mapping found for table(s)='DummyBizo'"), Times.Once);
			AssertEquals(0, result.Count);
		}

		public void TestServiceBindingsAreNull_Throws()
		{
			// Arrange
			var bizOs = new List<BusinessObject>();
			var bizO2 = Factory.New<DummyBusinessObject>();
			bizOs.Add(bizO2);

			var nudgingController = new Mock<INudgingController>();
			nudgingController.Setup(x => x.ServiceTaskBindings).Returns((IEnumerable<ServiceTaskBinding>)null);

			// Act
			// Assert
			AssertExceptionThrown(typeof(ArgumentNullException), () => new BusinessObjectToServiceTaskMapperForTest().MapBusinessObjectsToServiceTasks(nudgingController.Object, bizOs));
		}

		class BusinessObjectToServiceTaskMapperForTest : IBusinessObjectToServiceTaskMapper
		{
			public IEnumerable<IServiceTaskBinding> GenerateCompiledBindings(IApplicationSchemaResolver schemaResolver)
			{
				return GenerateCompiledBindings(BizOTaskBindings, schemaResolver);
			}

			public IEnumerable<IHostedServiceBusinessObjectBinding> BizOTaskBindings = new HostedServiceBusinessObjectBindingsProvider().BusinessObjectBindings;

			static IEnumerable<IServiceTaskBinding> GenerateCompiledBindings(IEnumerable<IHostedServiceBusinessObjectBinding> businessObjectTaskBindings, IApplicationSchemaResolver schemaResolver)
			{
				var result = new List<IServiceTaskBinding>();
				var dbSchemaResolver = new NudgingSchemaResolver(schemaResolver);
				var predicateFactory = new PredicateFactory(dbSchemaResolver);

				foreach (var bindingAttribute in businessObjectTaskBindings)
				{
					var predicates = predicateFactory.GeneratePredicates(bindingAttribute.Predicates, bindingAttribute.Table);
					var binding = new ServiceTaskBinding(bindingAttribute.ServiceTaskCode, bindingAttribute.Table, predicates);
					result.Add(binding);
				}
				return result;
			}

			public IDictionary<string, ICollection<string>> MapBusinessObjectsToServiceTasks(INudgingController nudgingController, IEnumerable<BusinessObject> businessObjects)
			{
				return BusinessObjectToServiceTaskMapper.Instance.MapBusinessObjectsToServiceTasks(nudgingController, businessObjects);
			}
		}
	}
}
