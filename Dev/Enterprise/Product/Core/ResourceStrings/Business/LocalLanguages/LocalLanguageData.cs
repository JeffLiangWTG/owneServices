using System;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(LocalLanguageData),
	Enterprise.Core.Constants.DocManagerCodes.LocalLanguage)]

namespace Enterprise.ResourceStrings.Business
{
	class LocalLanguageData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefLocalLanguage); } }
		protected override Type CollectionType
		{
			get { return typeof(RefLocalLanguageCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefLocalLanguageCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.LocalLanguages; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("0c0967e3-7b9b-46e9-8a9d-b12b9b763a14", "Local Language"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new LocalLanguageEDocsViaUniversalXmlSupport();
	}
}
