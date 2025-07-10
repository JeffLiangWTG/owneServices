using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Xml.Serializers;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeXmlSerializer : IBusinessSerializer, IBusinessObjectSerializer, IBusinessObjectWithDataContextInfoSerializer
	{
		public NativeXmlSerializer()
		{
			xmlGenerator = new EntitySetXmlSerializer();
		}
		readonly EntitySetXmlSerializer xmlGenerator;

		public BusinessObjectToEntityConverter Converter { get; set; }

		#region IBusinessSerializer Members

		public Stream Export(IEnumerable<IBusiness> businessObjects)
		{
			return Export(businessObjects, null);
		}

		public Stream Export(IEnumerable<IBusiness> businessObjects, BusinessObjectFactory factory)
		{
			return Export(businessObjects, factory, null);
		}

		public Stream Export(IEnumerable<IBusiness> businessObjects, BusinessObjectFactory factory, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			return SerializeToStream(null, businessObjects.OfType<BusinessObject>().Select<BusinessObject, RowID>(bo => bo), factory, filterOnMultiRowResult: filterOnMultiRowResult);
		}

		#endregion

		#region IBusinessObjectSerializer Members

		public SubStreamableStream SerializeToStream(BusinessObject businessObject)
		{
			return SerializeToStream(null, new RowID[] { businessObject }, null);
		}

		public SubStreamableStream SerializeToStream(IEnumerable<BusinessObject> businessObjects)
		{
			return SerializeToStream(null, businessObjects.Select<BusinessObject, RowID>(bo => bo), null);
		}

		#endregion

		#region IBusinessObjectWithDataContextInfoSerializer Members

		public SubStreamableStream SerializeToStream(BusinessObject businessObject, IDataContextDataObject dataContextInfo, List<IMessageNumber> messageNumberCollection)
		{
			return SerializeToStream(dataContextInfo, new RowID[] { businessObject }, null, new Guid(), null, null, messageNumberCollection);
		}

		#endregion

		#region Implementation

		internal SubStreamableStream SerializeToStream(IDataContextDataObject dataContextInfo, IEnumerable<RowID> rowIDs, BusinessObjectFactory factory, Guid orgHeaderPK = new Guid(), string entitySetOverride = null, Func<DataTable, DataRow> filterOnMultiRowResult = null, IEnumerable<IMessageNumber> messageNumberCollection = null)
		{
			var stream = (SubStreamableStream)new MemoryStream();

			var entityXml = ConvertToEntityXML(rowIDs, entitySetOverride, filterOnMultiRowResult);

			var request = new Request
			{
				Settings = HeaderData.New(EDICodeMapper.GetDefaultOrgCode(factory), DataContextWrapper.New(dataContextInfo), MessageNumberCollectionWrapper.New(messageNumberCollection).MessageNumberCollection),
				EntitySets = entityXml,
			};

			var serializer = new RequestSerializer();
			serializer.Serialize(stream, request, orgHeaderPK);

			stream.Position = 0;
			return stream;
		}

		IEnumerable<XElement> ConvertToEntityXML(IEnumerable<RowID> rowIDs, string entitySetOverride, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			foreach (var rowID in rowIDs)
			{
				progressChanged?.Invoke(this, null);
				var xElement = ConvertToEntityXML(rowID, entitySetOverride, filterOnMultiRowResult);
				if (xElement != null)
				{
					yield return xElement;
				}
			}
		}

		EventHandler progressChanged;
		public event EventHandler ProgressChanged
		{
			add => progressChanged += value;
			remove => progressChanged -= value;
		}

		XElement ConvertToEntityXML(RowID rowID, string entitySetOverride, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			var entity = Converter.GetEntity(rowID, entitySetOverride, filterOnMultiRowResult);

			if (entity == null)
			{
				return default(XElement);
			}
			return xmlGenerator.Serialize(entity);
		}

		#endregion
	}
}
