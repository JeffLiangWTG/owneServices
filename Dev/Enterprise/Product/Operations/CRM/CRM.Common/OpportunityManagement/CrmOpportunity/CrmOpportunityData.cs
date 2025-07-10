using System;
using Enterprise.CRM.Common;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CrmOpportunityData),
	Enterprise.Core.Constants.DocManagerCodes.CrmOpportunity)]

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CrmOpportunity);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.ClientSupplierRelationship;
	}
}
