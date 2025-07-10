using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	internal sealed class DeliveryAuthorizingDocumentOwnerAssemblyData : IAssemblyData
	{
		public Type BusinessObjectType => typeof(DeliveryAuthorizingDocumentOwner);
		public string DocManagerCode => DeliveryAuthorizingDocumentOwner.DocManagerCode;

		public string HumanReadableName { get; set; }
		public bool IsAllowedForUnallocatedeDocs { get; set; }
		public ZArchitecture.Modules.ModuleIdentifier ModuleID { get; set; }
		public string ReferenceType { get; set; }
		public bool AllowLookupOfBizOFromPk { get; set; }
		public IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => null;
		public IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams) => null;
		public IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => null;
		public ZQuery GetQuery(AssemblyDataParams assemblyDataParams) => null;
		public ZString GetFriendlyName(BusinessObject businessObject) => null;
	}
}
