using System;
using System.IO;
using System.Text;
using CargoWise.Definitions;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceTaskCsvExporterTest : TestCase
	{
		public void TestWriteTo_WithServiceAttributes_ExportsHeader()
		{
			// Arrange
			var serviceAttributes = new[]
			{
				new HostedServiceAttribute
				{
					Code = "Code1",
					Description = "Description1",
					Category = "ProductArea1",
				},
			};

			serviceAttributeProviderMock
				.Setup(o => o.GetHostedServiceAttributes())
				.Returns(serviceAttributes);

			// Act
			var result = Export();

			// Assert
			var lines = result.Split(new [] { System.Environment.NewLine }, StringSplitOptions.None);
			AssertEquals(2, lines.Length);

			const string expectedContent = @"""Product Area"",""Product Area Description"",""Code"",""Mutually Exclusive Group"",""Allow Multiple"",""Schedule Readonly"",""Description""";
			AssertEquals(expectedContent, lines[0]);
		}

		public void TestWriteTo_WithServiceAttribute_ExportsAttribute()
		{
			// Arrange
			var serviceAttributes = new[]
			{
				new HostedServiceAttribute
				{
					Code = "Code1",
					Description = "Description1",
					Category = "ProductArea1",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup,
					AllowsMultipleInstances = true,
					IsScheduleReadOnly = false,
				},
			};

			serviceAttributeProviderMock
				.Setup(o => o.GetHostedServiceAttributes())
				.Returns(serviceAttributes);

			// Act
			var result = Export();

			// Assert
			const string expectedContent = @"""ProductArea1"","""",""Code1"","""",""Yes"",""No"",""Description1""";
			AssertEndsWith("Export should contain the service attribute", expectedContent, result);
		}

		public void TestWriteTo_WithMatchingCategory_CategoryIsPopulated()
		{
			// Arrange
			var serviceAttributes = new[]
			{
				new HostedServiceAttribute
				{
					Code = "Code1",
					Description = "Description1",
					Category = "Acc",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup,
					AllowsMultipleInstances = true,
					IsScheduleReadOnly = false,
				},
			};

			serviceAttributeProviderMock
				.Setup(o => o.GetHostedServiceAttributes())
				.Returns(serviceAttributes);

			// Act
			var result = Export();

			// Assert
			const string expectedContent = @"""Acc"",""Accounting"",""Code1"","""",""Yes"",""No"",""Description1""";
			AssertEndsWith("Export should contain the category", expectedContent, result);
		}

		public void TestWriteTo_WithReservedCharacters_CharactersAreEscaped()
		{
			// Arrange
			var serviceAttributes = new[]
			{
				new HostedServiceAttribute
				{
					Code = "Code1",
					Description = "[\"][,][\"\"]",
					Category = "ProductArea1",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup,
					AllowsMultipleInstances = true,
					IsScheduleReadOnly = false,
				},
			};

			serviceAttributeProviderMock
				.Setup(o => o.GetHostedServiceAttributes())
				.Returns(serviceAttributes);

			// Act
			var result = Export();

			// Assert
			const string expectedContent = @"""ProductArea1"","""",""Code1"","""",""Yes"",""No"",""[""""][,][""""""""]""";
			AssertEndsWith("Export should contain the reserved characters", expectedContent, result);
		}

		public void TestWriteTo_WithMultipleClientSpecificAttributes_ExportsAllAttributes()
		{
			// Arrange
			var serviceAttributes = new[]
			{
				new HostedServiceAttribute
				{
					Code = "Code1",
					Description = "Description1",
					Category = "ProductArea1",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup,
					AllowsMultipleInstances = true,
					IsScheduleReadOnly = false,
					ClientSpecificCode = Clients.AAP,
				},
				new HostedServiceAttribute
				{
					Code = "Code2",
					Description = "Description2",
					Category = "ProductArea1",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiAudit,
					AllowsMultipleInstances = false,
					IsScheduleReadOnly = true,
					ClientSpecificCode = Clients.ACS,
				},
				new HostedServiceAttribute
				{
					Code = "Code3",
					Description = "Description3",
					Category = "ProductArea1",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.Upgrade,
					AllowsMultipleInstances = true,
					IsScheduleReadOnly = false,
					ClientSpecificCode = Clients.None,
				},
			};

			serviceAttributeProviderMock
				.Setup(o => o.GetHostedServiceAttributes())
				.Returns(serviceAttributes);

			// Act
			var result = Export();

			// Assert
			const string expectedContent =
@"""ProductArea1"","""",""Code1"","""",""Yes"",""No"",""Description1""
""ProductArea1"","""",""Code2"",""BiAudit"",""No"",""Yes"",""Description2""
""ProductArea1"","""",""Code3"",""Upgrade"",""Yes"",""No"",""Description3""";

			AssertEndsWith("Export should contain all client attributes", expectedContent, result);
		}

		public void TestWriteTo_WithUnorderedServiceAttributes_ExportIsOrdered()
		{
			// Arrange
			var serviceAttributes = new[]
			{
				new HostedServiceAttribute
				{
					Code = "Code3",
					Description = "Description3",
					Category = "ProductArea1",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup,
					AllowsMultipleInstances = true,
					IsScheduleReadOnly = false,
					ClientSpecificCode = Clients.AAP,
				},
				new HostedServiceAttribute
				{
					Code = "Code2",
					Description = "Description2",
					Category = "ProductArea2",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiAudit,
					AllowsMultipleInstances = false,
					IsScheduleReadOnly = true,
					ClientSpecificCode = Clients.ACS,
				},
				new HostedServiceAttribute
				{
					Code = "Code1",
					Description = "Description1",
					Category = "ProductArea1",
					MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup,
					AllowsMultipleInstances = true,
					IsScheduleReadOnly = false,
					ClientSpecificCode = Clients.AAP,
				},
			};

			serviceAttributeProviderMock
				.Setup(o => o.GetHostedServiceAttributes())
				.Returns(serviceAttributes);

			// Act
			var result = Export();

			// Assert
			const string expectedContent =
@"""Product Area"",""Product Area Description"",""Code"",""Mutually Exclusive Group"",""Allow Multiple"",""Schedule Readonly"",""Description""
""ProductArea1"","""",""Code1"","""",""Yes"",""No"",""Description1""
""ProductArea1"","""",""Code3"","""",""Yes"",""No"",""Description3""
""ProductArea2"","""",""Code2"",""BiAudit"",""No"",""Yes"",""Description2""";

			AssertEquals("Export should be ordered", expectedContent, result);
		}

		string Export()
		{
			using var stream = new MemoryStream();
			using var writer = new StreamWriter(stream, Encoding.UTF8);

			exporter.WriteTo(stream);

			writer.Flush();
			stream.Position = 0;

			using var reader = new StreamReader(stream, Encoding.UTF8);

			const char bom = '\uFEFF';

			return reader
				.ReadToEnd()
				.TrimEnd(bom)
				.TrimEnd(System.Environment.NewLine.ToCharArray());
		}

		protected override void SetUp()
		{
			serviceAttributeProviderMock = new Mock<IHostedServiceAttributeProvider>();

			exporter = new ServiceTaskCsvExporter(serviceAttributeProviderMock.Object);
		}

		ServiceTaskCsvExporter exporter;
		Mock<IHostedServiceAttributeProvider> serviceAttributeProviderMock;
	}
}
