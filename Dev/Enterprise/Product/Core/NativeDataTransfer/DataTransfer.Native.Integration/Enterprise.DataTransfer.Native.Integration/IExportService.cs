using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Native.Integration
{
	public interface IExportService
	{
		bool CanBeExported(Type type);
		void Export(IEnumerable<IBusiness> businessObject);
		void Export(IEnumerable<IBusiness> businessObjects, Stream targetStream);
		void Export(IEnumerable<IBusiness> businessObjects, Stream targetStream, BusinessObjectFactory factory);
		void ExportWithSave(IEnumerable<IBusiness> businessObject);
		void ExportWithSave(IEnumerable<IBusiness> businessObject, Func<DataTable, DataRow> filterOnMultiRowResult);
	}
}
