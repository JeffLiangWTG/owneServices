using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(VerboseLoggingBusinessObject))]
	public class VerboseLoggingBusinessObjectTest : RegistryBusinessObjectTest
	{
		public void TestCorrectType()
		{
			// Arrange
			var codeDescriptionDateTime = new VerboseLoggingBusinessObject();

			// Act
			var result = codeDescriptionDateTime.ValueInfo.PropertyType;

			// Assert
			AssertEquals(nameof(ZDateTime), result.Name);
		}

		public void TestCodeHasListAttribute()
		{
			// Arrange
			var propertyInfo = typeof(VerboseLoggingBusinessObject)
				.GetProperty(nameof(VerboseLoggingBusinessObject.Code));

			// Act
			var result = propertyInfo
				?.GetCustomAttributes(typeof(ListAttribute), false)
				?.SingleOrDefault() as ListAttribute;

			// Assert
			AssertNotNull(result);
			AssertEquals(nameof(VerboseLoggingBusinessObject.Codes), result.ListDataSourceMember);
		}

		public void TestCodePopulatesDescription()
		{
			// Arrange
			var pairs = new[]
			{
				new HostedServiceCodeDescription("AAA", "AAA Description"),
				new HostedServiceCodeDescription("BBB", "BBB Description"),
				new HostedServiceCodeDescription("CCC", "CCC Description"),
			};
			var hostedServiceProviderMock = new Mock<IHostedServiceCodeDescriptionProvider>();
			hostedServiceProviderMock
				.Setup(provider => provider.GetHostedServices())
				.Returns(pairs);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var businessObject = new VerboseLoggingBusinessObject();

				foreach (var pair in pairs)
				{
					Test(pair);
				}

				void Test(HostedServiceCodeDescription pair)
				{
					// Act
					businessObject.Code = pair.Code;

					// Assert
					AssertEquals(pair.Description, businessObject.Description);
				}
			}
		}

		public void TestCodesReturnsValuesFromHostedServiceProvider()
		{
			Test(new []
			{
				new CodeDescriptionPair("AAA", "AAA Description"),
				new CodeDescriptionPair("BBB", "BBB Description"),
				new CodeDescriptionPair("CCC", "CCC Description"),
				new CodeDescriptionPair("DDD", "DDD Description"),
			});

			Test(new []
			{
				new CodeDescriptionPair("AAAA", "AAA Description"),
				new CodeDescriptionPair("BBBB", "BBB Description"),
				new CodeDescriptionPair("CCCC", "CCC Description"),
				new CodeDescriptionPair("DDDD", "DDD Description"),
			});

			void Test(IEnumerable<ICodeDescription> serviceTasks)
			{
				// Arrange
				var hostedServiceProviderMock = new Mock<IHostedServiceCodeDescriptionProvider>();
				hostedServiceProviderMock
					.Setup(provider => provider.GetHostedServices())
					.Returns(serviceTasks.Select(t => new HostedServiceCodeDescription(t.Code, t.Description)));

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var businessObject = new VerboseLoggingBusinessObject();

					// Act
					var result = businessObject.Codes
						.Cast<CodeDescriptionPair>();

					// Assert
					AssertContainsExactElementsInAnyOrder(
						input => $"[{input.Code}] [{input.Description}]",
						serviceTasks,
						result);
				}
			}
		}

		public void TestCodesReturnsSortedValues()
		{
			// Arrange
			var pairs = new []
			{
				new CodeDescriptionPair("CCC", "CCC Description"),
				new CodeDescriptionPair("BBB", "BBB Description"),
				new CodeDescriptionPair("DDD", "DDD Description"),
				new CodeDescriptionPair("AAA", "AAA Description"),
			};
			var hostedServiceProviderMock = new Mock<IHostedServiceCodeDescriptionProvider>();
			hostedServiceProviderMock
				.Setup(provider => provider.GetHostedServices())
				.Returns(pairs.Select(t => new HostedServiceCodeDescription(t.Code, t.Description)));

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var businessObject = new VerboseLoggingBusinessObject();
				var expected = pairs
					.OrderBy(pair => pair.Code);

				// Act
				var result = businessObject.Codes
					.Cast<CodeDescriptionPair>();

				// Assert
				AssertContainsExactElementsInExactOrder(
					input => $"[{input.Code}] [{input.Description}]",
					expected,
					result);
			}
		}

		public void TestCodesReadsValuesOnlyOnce()
		{
			// Arrange
			var pairs = Array.Empty<CodeDescriptionPair>();
			var hostedServiceProviderMock = new Mock<IHostedServiceCodeDescriptionProvider>();
			hostedServiceProviderMock
				.Setup(provider => provider.GetHostedServices())
				.Returns(pairs.Select(t => new HostedServiceCodeDescription(t.Code, t.Description)));

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var businessObject = new VerboseLoggingBusinessObject();

				// Act
				foreach (var i in Enumerable.Range(0, 10))
				{
					_ = businessObject.Codes
						.Cast<CodeDescriptionPair>();
				}
			}

			// Assert
			AssertNoExceptionThrown(() => hostedServiceProviderMock.Verify(provider => provider.GetHostedServices(), Times.Once));
		}

		public void TestDescriptionIsReadonly()
		{
			// Arrange
			var businessObject = new VerboseLoggingBusinessObject();

			// Act
			var result = businessObject.DescriptionInfo.ReadOnly;

			// Assert
			AssertEquals(true, result);
		}

		public void TestEnglishDescriptionIsReadonly()
		{
			// Arrange
			var businessObject = new VerboseLoggingBusinessObject();

			// Act
			var result = businessObject.EnglishDescriptionInfo.ReadOnly;

			// Assert
			AssertEquals(true, result);
		}

		[TestDate(2019, 12, 23, 20, 22, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestVerboseLogging()
		{
			Test(ZDateTime.UtcNow.AddSeconds(-1), false);
			Test(ZDateTime.UtcNow, false);
			Test(ZDateTime.UtcNow.AddSeconds(1), true);

			void Test(ZDateTime value, bool expected)
			{
				// Arrange
				var businessObject = new VerboseLoggingBusinessObject { Value = value };

				// Act
				var result = businessObject.VerboseLogging;

				// Assert
				AssertEquals(expected, result);
			}
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals(256, BizObj.DescriptionInfo.MaxLength);
		}

		protected override int ExpectedDefaultMaxCodeLength => 4;

		protected override bool RequiresFactory => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (VerboseLoggingBusinessObject)GetNewBusinessObject();

			result.Code = "TEST";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.CodeList = new CodeDescriptionPairList();

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var verboseLoggingBusinessObject = (VerboseLoggingBusinessObject)clone;
			AssertEquals("Code", "TEST", verboseLoggingBusinessObject.Code);
			AssertEquals("Description", "DescriptionA", verboseLoggingBusinessObject.Description);
			AssertEquals("CodeMaxLength", 4, verboseLoggingBusinessObject.CodeMaxLength);
			Assert("SystemDefined", verboseLoggingBusinessObject.SystemDefined);
			AssertNotNull("CodeList", verboseLoggingBusinessObject.CodeList);
		}

		public class ValidationTest : TestCase
		{
			public void TestCodeDoesNotMatch()
			{
				Test("TS1");
				Test("TS2");

				void Test(string code)
				{
					// Arrange
					var hostedServiceProviderMock = Mock.Of<IHostedServiceCodeDescriptionProvider>(
						provider => provider.GetHostedServices() == new[]
						{
							new HostedServiceCodeDescription("AAA", "AAA Description"),
							new HostedServiceCodeDescription("BBB", "BBB Description"),
							new HostedServiceCodeDescription("CCC", "CCC Description"),
						});

					using (ObjectFactory.Substitute(hostedServiceProviderMock))
					{
						var businessObject = new VerboseLoggingBusinessObject
						{
							Code = code,
						};

						// Act
						businessObject.RunPreSaveValidation();

						// Assert
						AssertEquals(true, businessObject.HasErrors);
						AssertEquals("Error - Code: Code does not match any service task.", businessObject.GetErrors().Single().Message);
					}
				}
			}

			[TestDate(2019, 12, 23, 20, 22, 0)]
			public void TestDateTimeValueHasErrorWhenTooFarInFuture()
			{
				Test(ZDateTime.UtcNow.AddDays(30));
				Test(ZDateTime.UtcNow.AddDays(30).AddMinutes(1));
				Test(ZDateTime.UtcNow.AddDays(31));
				Test(ZDateTime.UtcNow.AddYears(1));

				void Test(ZDateTime dateTime)
				{
					// Arrange
					var hostedServiceProviderMock = Mock.Of<IHostedServiceCodeDescriptionProvider>(
						provider => provider.GetHostedServices() == new[]
						{
							new HostedServiceCodeDescription("AAA", "AAA Description"),
							new HostedServiceCodeDescription("BBB", "BBB Description"),
							new HostedServiceCodeDescription("CCC", "CCC Description"),
						});
					using (ObjectFactory.Substitute(hostedServiceProviderMock))
					{
						var businessObject = new VerboseLoggingBusinessObject
						{
							Code = "AAA",
							Value = dateTime,
						};

						// Act
						businessObject.RunPreSaveValidation();

						// Assert
						AssertEquals(true, businessObject.HasErrors);
						AssertEquals("Error - Value: Verbose logging period must be less than 30 days.", businessObject.GetErrors().Single().Message);
					}
				}
			}

			[TestDate(2019, 12, 23, 20, 22, 0)]
			[TestUtcOffset(11, 0, 0)]
			public void TestDateTimeValueErrorPeriodIsUtc()
			{
				Test(ZDateTime.UtcNow.AddDays(30).AddSeconds(-1), false);
				Test(ZDateTime.UtcNow.AddDays(30), true);
				Test(ZDateTime.UtcNow.AddDays(30).AddSeconds(1), true);

				void Test(ZDateTime dateTime, bool expected)
				{
					// Arrange
					var hostedServiceProviderMock = Mock.Of<IHostedServiceCodeDescriptionProvider>(
						provider => provider.GetHostedServices() == new[]
						{
							new HostedServiceCodeDescription("AAA", "AAA Description"),
							new HostedServiceCodeDescription("BBB", "BBB Description"),
							new HostedServiceCodeDescription("CCC", "CCC Description"),
						});
					using (ObjectFactory.Substitute(hostedServiceProviderMock))
					{
						var businessObject = new VerboseLoggingBusinessObject
						{
							Code = "AAA",
							Value = dateTime,
						};

						// Act
						businessObject.RunPreSaveValidation();

						// Assert
						AssertEquals(expected, businessObject.HasErrors);
					}
				}
			}

			[TestDate(2019, 12, 23, 20, 22, 0)]
			public void TestDateTimeValueHasWarningWhenTooFarInFuture()
			{
				Test(ZDateTime.UtcNow.AddDays(7));
				Test(ZDateTime.UtcNow.AddDays(7).AddMinutes(1));
				Test(ZDateTime.UtcNow.AddDays(30).AddSeconds(-1));

				void Test(ZDateTime dateTime)
				{
					// Arrange
					var hostedServiceProviderMock = Mock.Of<IHostedServiceCodeDescriptionProvider>(
						provider => provider.GetHostedServices() == new[]
						{
							new HostedServiceCodeDescription("AAA", "AAA Description"),
							new HostedServiceCodeDescription("BBB", "BBB Description"),
							new HostedServiceCodeDescription("CCC", "CCC Description"),
						});
					using (ObjectFactory.Substitute(hostedServiceProviderMock))
					{
						var businessObject = new VerboseLoggingBusinessObject
						{
							Code = "AAA",
							Value = dateTime,
						};

						// Act
						businessObject.RunPreSaveValidation();

						// Assert
						AssertEquals(true, businessObject.HasWarnings);
						AssertEquals("Warning - Value: Verbose logging period is better to be less than 7 days.", businessObject.GetWarnings().Single().Message);
					}
				}
			}

			[TestDate(2019, 12, 23, 20, 22, 0)]
			[TestUtcOffset(11, 0, 0)]
			public void TestDateTimeValueWarningPeriodIsUtc()
			{
				Test(ZDateTime.UtcNow.AddDays(7).AddSeconds(-1), false);
				Test(ZDateTime.UtcNow.AddDays(7), true);
				Test(ZDateTime.UtcNow.AddDays(7).AddSeconds(1), true);

				void Test(ZDateTime dateTime, bool expected)
				{
					// Arrange
					var hostedServiceProviderMock = Mock.Of<IHostedServiceCodeDescriptionProvider>(
						provider => provider.GetHostedServices() == new[]
						{
							new HostedServiceCodeDescription("AAA", "AAA Description"),
							new HostedServiceCodeDescription("BBB", "BBB Description"),
							new HostedServiceCodeDescription("CCC", "CCC Description"),
						});
					using (ObjectFactory.Substitute(hostedServiceProviderMock))
					{
						var businessObject = new VerboseLoggingBusinessObject
						{
							Code = "AAA",
							Value = dateTime,
						};

						// Act
						businessObject.RunPreSaveValidation();

						// Assert
						AssertEquals(expected, businessObject.HasWarnings);
					}
				}
			}
		}
	}
}
