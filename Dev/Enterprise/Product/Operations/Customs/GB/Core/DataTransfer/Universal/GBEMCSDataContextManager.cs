using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class GBEMCSDataContextManager : EventDataContextManager<EMCSJobDeclaration>
	{
		public override DataContextType DataContextType => DataContextType.EMCSJobDeclaration;

		public override ZString DataContextKey => ParentBO.JE_DeclarationReference;

		public override string DefaultOutputDirectory => string.Empty;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (!matchingValues.Key.IsEmpty)
			{
				result = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, matchingValues.Key);
				result.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, EMCSJobDeclaration.EMCSApplicationCode);
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, EMCSJobDeclaration.EMCSMessageTypeCode);
				result.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			}
			return result;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new GBEMCSEventParentFinder(factory, this, logger);
	}
}
