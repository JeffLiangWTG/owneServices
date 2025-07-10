using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public interface IBusinessSerializer
	{
		Stream Export(IEnumerable<IBusiness> businessObjects);
		Stream Export(IEnumerable<IBusiness> businessObjects, BusinessObjectFactory factory);
		Stream Export(IEnumerable<IBusiness> businessObjects, BusinessObjectFactory factory, Func<DataTable, DataRow> filterOnMultiRowResult);
		event EventHandler ProgressChanged;
	}
}
