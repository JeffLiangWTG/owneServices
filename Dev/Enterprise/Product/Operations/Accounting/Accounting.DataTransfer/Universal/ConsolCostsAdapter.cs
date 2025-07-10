using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class ConsolCostsAdapter : IConsolCostsAdapter
	{
		#region Generate

		public ConsolCosts Generate(IGenericJobCostPlugInBase consolCostParentCore, IDataObjectWriterStrategy writerStrategy)
		{
			ConsolCosts consolCosts = null;
			var consolCostParent = consolCostParentCore as IGenericJobCostPlugIn;
			if (consolCostParent != null)
			{
				ApportionmentListing apportionmentListing = new ApportionmentListing(consolCostParent.Factory, consolCostParent);
				try
				{
					consolCosts = ConsolCostsExporter.Generate(apportionmentListing, writerStrategy);
				}
				finally
				{
					apportionmentListing.ReleaseMutexes();
				}
			}

			return consolCosts;
		}

		#endregion

		#region ImportCharges

		public void ImportConsolCosts(BusinessObjectFactory factory, IXmlImportLogger logger, IConsolCostsData consol, ZGuid parentPK, ZString parentTablePrefix)
		{
			new ConsolCostImporter().ImportConsolCosts(factory, logger, consol, parentPK, parentTablePrefix);
		}

		#endregion
	}
}
