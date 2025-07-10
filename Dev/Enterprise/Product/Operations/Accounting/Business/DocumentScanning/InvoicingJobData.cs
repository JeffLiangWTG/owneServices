using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(InvoicingJobData),
	Enterprise.Core.Constants.DocManagerCodes.InvoicingJob)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class InvoicingJobData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Job); } }
		protected override Type CollectionType
		{
			get { return typeof(JobCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new JobCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.JobHeader; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("966bb09d-9880-4170-bf61-ab0d346dfaa0", "Invoicing Job"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
