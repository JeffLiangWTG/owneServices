using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class DynamicDataExtensions
	{
		#region TryCreate

		public static Try<IDynamicData> TryCreate(this IBusiness bizObj, string dataContext, IDocDataObjectParameters parameters = null)
		{
			if (string.IsNullOrWhiteSpace(dataContext))
			{
				return Try<IDynamicData>.Failure(Res.GetString("de77ee7a-10de-450d-983e-0a4bd21fe134", "Cannot create data because there is no data context."));
			}

			if (!(bizObj is BusinessObject businessObject))
			{
				return Try<IDynamicData>.Failure(Res.GetString("42f1eb2d-6ec7-4e2c-9121-ed4a27988c19", "Invalid data source."));
			}

			try
			{
				if (dataContext == DataContext.UXML)
				{
					return TryGetDocumentDataFromUXml(businessObject);
				}

				var res = bizObj.GetSupporter()?.GetDocDataObject(businessObject, dataContext, parameters);

				if (res.HasValue)
				{
					if (res.Value.IsLeft)
					{
						return Try<IDynamicData>.Failure(res.Value.Left);
					}

					var result = res.Value.Right is DocDataObject docDataObject
						? docDataObject.MakeDocDataDynamic()
						: res.Value.Right.MakeDynamic();

					return Try<IDynamicData>.Success(result);
				}

				return Try<IDynamicData>.Failure(Res.GetString("02e4fa4d-ef2d-4572-aa89-26686e654d60", "Data context is not valid."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ex.Data["DataContext"] = dataContext;
				ex.Data["BusinessObject.TableName"] = businessObject.TableName;
				ex.Data["BusinessObject.PK"] = businessObject.PK.ToString();

				var errorReporterKey = $"TryCreateDocumentData-{ex.Message}";
				ErrorReporter.ReportOnce(errorReporterKey, ex);

				return Try<IDynamicData>.Failure(ex);
			}
		}

		#endregion

		#region TryGetDocumentDataFromUXml

		static Try<IDynamicData> TryGetDocumentDataFromUXml(BusinessObject bizObj)
		{
			IDataObject dataObject = null;
			UXmlLinkManager linkManager = null;

			using (PerformanceStatistics.StartMonitoring(PerformanceMonitorArea.CreateUniversalXml))
			{
				var provider = ObjectFactory.Get<IUniversalShipmentDataObjectProvider>();

				linkManager = new UXmlLinkManager();

				dataObject = provider.GetDataObject(bizObj, linkManager);
			}

			if (dataObject == null)
			{
				return Try<IDynamicData>.Failure(Res.GetString("18c3704f-0c2f-4346-bfbc-6c5ab60e9936", "Universal XML data object has not been created."));
			}

			using (PerformanceStatistics.StartMonitoring(PerformanceMonitorArea.CreateDynamicData))
			{
				var metaDataProvider = new UXmlMetaDataProvider(linkManager);
				var data = dataObject.MakeDynamic(metaDataProvider, null, new UXmlTypeConverter());

				return Try<IDynamicData>.Success(data);
			}
		}

		#endregion

		#region ToCodeDescriptionPairList

		public static ICodeDescriptionPairList ToCodeDescriptionPairList(this object[] untypedList)
		{
			if (untypedList == null)
			{
				return null;
			}

			var result = new CodeDescriptionPairList();

			foreach (var element in untypedList)
			{
				var pair = element as object[];

				if (pair == null
					|| pair.Length < 2)
				{
					continue;
				}

				result.AddPairIfNotExist(
					Convert.ToString(pair[0], CultureInfo.InvariantCulture),
					Convert.ToString(pair[1], CultureInfo.InvariantCulture));
			}

			return result.Count > 0
				? result
				: null;
		}

		#endregion
	}
}
