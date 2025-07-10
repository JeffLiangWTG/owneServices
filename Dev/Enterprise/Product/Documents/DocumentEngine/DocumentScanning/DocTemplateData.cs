using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocumentScanning;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(DocTemplateData), Enterprise.Core.Constants.DocManagerCodes.DocTemplateRecord)]
namespace Enterprise.DocumentEngine.DocumentScanning
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using ResString = Enterprise.DocumentEngine.ResString;
	class DocTemplateData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(StmTemplateBase);
		protected override Type CollectionType => typeof(StmTemplateBaseCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new StmTemplateBaseCollection(factory);
		}
		public override string ReferenceType => Core.Constants.ReferenceTypes.GeneralReferenceTables;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("beaccb9d-0874-4dd4-ad34-c4cb498c2a07", "Document/Report Template Records");
	}
}