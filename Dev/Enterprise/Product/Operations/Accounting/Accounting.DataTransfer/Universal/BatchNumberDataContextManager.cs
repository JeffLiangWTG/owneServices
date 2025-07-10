using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class BatchNumberDataContextManager : EventDataContextManager<GenExportBatchSequence>
	{
		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => new ZQuery { IsNoResultQuery = true };
		public override DataContextType DataContextType => DataContextType.BatchNumber;
		public override ZString DataContextKey => ParentBO.XB_BatchNumber.ToString();
		public override string DefaultOutputDirectory => null;
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;
		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;
	}
}
