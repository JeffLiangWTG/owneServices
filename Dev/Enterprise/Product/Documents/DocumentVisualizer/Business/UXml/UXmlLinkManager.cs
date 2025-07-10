using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class UXmlLinkManager : IDataWritingInformationCollector
	{
		readonly ConditionalWeakTable<IDataObject, object> dataObjectPKMap = new ConditionalWeakTable<IDataObject, object>();

		#region IDataWritingInformationCollector

		void IDataWritingInformationCollector.NotifyExported(IDataObject dataObject, BusinessObject businessObject)
		{
			if (dataObject != null && businessObject != null)
			{
				object pk;

				if (!dataObjectPKMap.TryGetValue(dataObject, out pk))
				{
					dataObjectPKMap.Add(dataObject, businessObject.PK);
				}
			}
		}

		#endregion

		public ZGuid GetPK(IDataObject dataObject)
		{
			if (dataObject != null)
			{
				object pk;

				if (dataObjectPKMap.TryGetValue(dataObject, out pk))
				{
					return (ZGuid)pk;
				}
			}

			return ZGuid.Invalid;
		}
	}
}
