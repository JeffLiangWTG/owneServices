using System.Collections.Generic;
using System.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Numerics.LinearAlgebra;

namespace Enterprise.Client.EDI.Test
{
	public class IncidentSimilarityExclusionTest : TestCaseWithFactory
	{
		public class IncidentMainWrapper : AutoIncidentMain
		{
			public IncidentMainWrapper(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
		public class IncidentAssociationNewIncidentsRunnerWrapper : IncidentAssociationNewIncidentsRunner
		{
			public IncidentAssociationNewIncidentsRunnerWrapper(ILogger logger) : base(logger)
			{
			}

			public IncidentAssociationNewIncidentsRunnerWrapper(IIncidentTokenizer incidentTokenizer, ISimilarIncidentRepository similarIncidentRepository, ILinearAlgebra linearAlgebra, IMetaSupportIncidentProvider metaSupportIncidentProvider, ISimilarityMatrixBootstrapperQueryExecutor similarityMatrixBootstrapperQueryExecutor, ILogger logger) : base(incidentTokenizer, similarIncidentRepository, linearAlgebra, metaSupportIncidentProvider, similarityMatrixBootstrapperQueryExecutor, logger)
			{
			}

			public bool IncidentInTheExclusion(ZGuid item, BusinessObjectFactory factory)
			{
				if (factory == null)
				{
					return false;
				}
				var filter = new ZQuery();
				filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, item);
				var result = factory.LoadTop1<IncidentSimilarityExclusion>(filter);
				return result != null;
			}

			public IncidentSimilarityExclusion GetIncidentInTheExclusion(ZGuid item, BusinessObjectFactory factory)
			{
				if (factory == null)
				{
					return null;
				}
				var filter = new ZQuery();
				filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, item);
				var result = factory.LoadTop1<IncidentSimilarityExclusion>(filter);
				return result;
			}

			public void AddToExclusionWrapper(ZGuid item, BusinessObjectFactory factory, ZDateTime systemLastEditTimeUtc)
			{
				AddToExclusion(item, factory, systemLastEditTimeUtc);
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAddToExclusionSingleRecord()
		{
			//Arrange
			var incidentMainObj = Factory.NewWithValidTestData<IncidentMainWrapper>();
			Factory.Save();

			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, incidentMainObj.PK);

			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});
			//Act
			var wrapper = new IncidentAssociationNewIncidentsRunnerWrapper(mockLogger.Object);
			wrapper.AddToExclusionWrapper(incidentMainObj.PK, Factory, ZDateTime.UtcNow);
			Factory.Save();
			var record = Factory.LoadTop1<IncidentSimilarityExclusion>(filter);
			//Assert
			AssertNotNull(record);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAddToExclusionBusinessObjectFactoryIsNullWarning()
		{
			//Arrange
			var incidentMainObj = Factory.NewWithValidTestData<IncidentMainWrapper>();
			Factory.Save();
			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, incidentMainObj.PK);

			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});
			//Act
			var wrapper = new IncidentAssociationNewIncidentsRunnerWrapper(mockLogger.Object);
			AssertEquals(0, warningLog.Count);
			wrapper.AddToExclusionWrapper(incidentMainObj.PK, null, ZDateTime.UtcNow);
			Factory.Save();
			var record = Factory.LoadTop1<IncidentSimilarityExclusion>(filter);
			//Assert
			AssertNull(record);
			AssertEquals(1, warningLog.Count);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestIncidentInTheExclusionObjectFactoryIsNullWarning()
		{
			//Arrange
			var incidentMainObj = Factory.NewWithValidTestData<IncidentMainWrapper>();
			Factory.Save();
			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, incidentMainObj.PK);
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});
			//Act
			var wrapper = new IncidentAssociationNewIncidentsRunnerWrapper(mockLogger.Object);
			var res = wrapper.IncidentInTheExclusion(incidentMainObj.PK, null);
			//Assert
			AssertEquals(0, warningLog.Count);
			AssertEquals(false, res);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestIncidentInTheExclusionFalse()
		{
			//Arrange
			var incidentMainObj = Factory.NewWithValidTestData<IncidentMainWrapper>();
			Factory.Save();
			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, incidentMainObj.PK);
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});
			//Act
			var wrapper = new IncidentAssociationNewIncidentsRunnerWrapper(mockLogger.Object);
			var res = wrapper.IncidentInTheExclusion(incidentMainObj.PK, Factory);
			//Assert
			AssertEquals(false, res);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestIncidentInTheExclusionTrue()
		{
			//Arrange
			var incidentMainObj = Factory.NewWithValidTestData<IncidentMainWrapper>();
			Factory.Save();
			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, incidentMainObj.PK);
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});
			//Act
			var wrapper = new IncidentAssociationNewIncidentsRunnerWrapper(mockLogger.Object);
			wrapper.AddToExclusionWrapper(incidentMainObj.PK, Factory, ZDateTime.UtcNow);
			Factory.Save();
			var res = wrapper.IncidentInTheExclusion(incidentMainObj.PK, Factory);
			//Assert
			AssertEquals(true, res);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAddToExclusionBusinessObjectFactoryIsCheckSystemLastEditTimeUtc()
		{
			//Arrange
			var incidentMainObj = Factory.NewWithValidTestData<IncidentMainWrapper>();
			Factory.Save();
			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, incidentMainObj.PK);
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});

			var expectedSystemLastEditTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			var wrapper = new IncidentAssociationNewIncidentsRunnerWrapper(mockLogger.Object);
			//Act
			wrapper.AddToExclusionWrapper(incidentMainObj.PK, Factory, expectedSystemLastEditTimeUtc);
			Factory.Save();
			var record = Factory.LoadTop1<IncidentSimilarityExclusion>(filter);
			//Assert
			AssertEquals(0, warningLog.Count);
			AssertEquals(expectedSystemLastEditTimeUtc, record.ISE_LastUpdatedUtc);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAddToExclusionUpdatedISE_LastUpdatedUtcFromSmallDateTime()
		{
			//Arrange
			var incidentMainObj = Factory.NewWithValidTestData<IncidentMainWrapper>();
			Factory.Save();
			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, incidentMainObj.PK);
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});

			var expectedSystemLastEditTimeUtc = new ZDateTime(2023, 11, 15, 14, 34, 00);
			var wrapper = new IncidentAssociationNewIncidentsRunnerWrapper(mockLogger.Object);
			//Act
			wrapper.AddToExclusionWrapper(incidentMainObj.PK, Factory, expectedSystemLastEditTimeUtc);
			Factory.Save();
			var record = Factory.LoadTop1<IncidentSimilarityExclusion>(filter);
			//Assert
			AssertEquals(0, warningLog.Count);
			AssertEquals(expectedSystemLastEditTimeUtc, record.ISE_LastUpdatedUtc);

			//Arrange
			expectedSystemLastEditTimeUtc = new ZDateTime(2023, 11, 15, 14, 34, 56);

			//Act
			wrapper.AddToExclusionWrapper(incidentMainObj.PK, Factory, expectedSystemLastEditTimeUtc);
			Factory.Save();
			record = Factory.LoadTop1<IncidentSimilarityExclusion>(filter);
			//Assert
			AssertEquals(0, warningLog.Count);
			AssertEquals(expectedSystemLastEditTimeUtc, record.ISE_LastUpdatedUtc);
		}
	}
}
