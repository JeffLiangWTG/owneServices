using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Common.Import;
using Enterprise.DataTransfer.Native.Business;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Update;
using Enterprise.DataTransfer.Native.Business.Xml.Deserializers;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.Stat;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Adapter.ImportServices
{
	public class ImportHandler : DataImportService<XElement>
	{
		#region Dependencies

		public readonly EntitySetXmlDeserializer Parser;
		public readonly IConverter<HeaderData, UpdateContext> RequestConverter;
		readonly AncillaryImportServices sessionServices;
		#endregion

		public ImportHandler(AncillaryImportServices sessionServices)
		{
			this.sessionServices = sessionServices ?? throw new ArgumentNullException(nameof(sessionServices));
			Parser = new EntitySetXmlDeserializer
			{
				DefinitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() }
			};
			RequestConverter = new UpdateContextConverter(sessionServices, new FactoryProvider());
		}

		public void SetDefinitionFinder(string[] assemblyNames)
		{
			var cache = EntitySetDefinitionCache.GetInstance();
			cache.SetAssemblies(assemblyNames);
			Parser.DefinitionFinder = new DefinitionFinder { Cache = cache };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Math.Round is fine")]
		public sealed override void Import(Stream stream)
		{
			try
			{
				var stopwatch = new System.Diagnostics.Stopwatch();
				stopwatch.Start();

				Initial();
				var requestDeserializer = RequestDeserializerBuilder.GetDeserializer(stream);
				var request = DeserializeRequest(requestDeserializer, stream);
				ValidateRequest(request);

				var context = ConvertContext(request.Settings);
				context.AlwaysUseInternalPK = AlwaysUseProvidedPKs;
#if DEBUG
				LastRowFactoryForTest = context.RowFactory;
				using (LastRowFactoryForTest.EnableTableHitQueryCollection(TablesForHitQueryCollection))
#endif
				using (context)
				using (NativeHandler.SetUserContext(context.ObjectFactory, request.Settings?.DataContext, sessionServices.Logger))
				{
					ImportCore(request, context);
				}
				stopwatch.Stop();
				if (!ZArchitecture.Environment.Globals.IsTest)
				{
					var elapsedRounded = TimeSpan.FromSeconds(Math.Round(stopwatch.Elapsed.TotalSeconds, 1)); // Math.Round is fine
					sessionServices.Logger.Log(LogType.Information, "Import completed in time " + elapsedRounded.ToString("g", CultureInfo.CurrentCulture));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorOccur(null, ex);
			}
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] TablesForHitQueryCollection { get; set; }
		public CargoWise.EntityFramework.RowFactory LastRowFactoryForTest { get; private set; }
		public CargoWise.EntityFramework.RowFactory BehaviourRowFactoryForTest { get; private set; }
		public IReadOnlyDictionary<string, int> BatchMergeEntityCountForTest { get; private set; }
#endif

		#region Implementation
#if DEBUG
		protected virtual
#endif
		void ImportCore(ExportImportRequest request, UpdateContext context)
		{
			using (context)
			{
				BeforeProcess?.Invoke();

				foreach (var entitySetElement in request.EntitySets)
				{
					if (IsCancelled())
					{
						break;
					}

					BeforeUnitProcess?.Invoke(entitySetElement);

					try
					{
						using (sessionServices.ImportingEntitySet())
						{
							var entitySet = Parser.Deserialize(entitySetElement, sessionServices);

							context.Import(entitySet);
							context.Save();
						}
						UnitProcessSuccess?.Invoke(entitySetElement);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorOccur?.Invoke(entitySetElement, ex);

						sessionServices.Logger.Log(LogType.Error,
							"Error occurred trying to import file. Please fix the error and try importing the file again.");
						break;
					}

					AfterUnitProcess?.Invoke(entitySetElement);
				}

				AfterProcess?.Invoke();

				var statistics = context.Statistics;
				sessionServices.Logger.Informations(statistics.Summary());

				if (!context.Statistics.EntityAffected.Any() && !Parser.FoundAtLeastOneAction)
				{
					sessionServices.Logger.Log(LogType.Information, "There were no ‘Action’ attributes included in the XML provided. Without ‘Action’ attributes, no data changes will be made." +
						" Please include 'Action' attributes (UPDATE/DELETE/INSERT/MERGE) in the XML to let the system know how to apply any updates required.");
				}

#if DEBUG
				BatchMergeEntityCountForTest = statistics.BatchMergeEntityCountForTest;
				BehaviourRowFactoryForTest = context.BehaviourRowFactorySavedFirst;
#endif
			}
		}

		protected ExportImportRequest DeserializeRequest(BaseRequestDeserializer requestDeserializer, Stream stream)
		{
			var request = requestDeserializer.Deserialize(stream);
			return new ExportImportRequest(request);
		}

		protected void ValidateRequest(ExportImportRequest request)
		{
			var validateResult = request.Validate();
			if (!validateResult.IsSuccess)
			{
				throw new NativeXMLUserVisibleException(validateResult.Message);
			}
		}

		protected UpdateContext ConvertContext(HeaderData request)
		{
			return RequestConverter.Convert(request);
		}

#endregion
	}
}
