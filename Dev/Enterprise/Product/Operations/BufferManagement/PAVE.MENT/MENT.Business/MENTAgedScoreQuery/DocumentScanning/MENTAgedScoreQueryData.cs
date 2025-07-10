using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(MENTAgedScoreQueryData),
	Constants.DocManagerCodes.MENTAgedScoreQuery)]
namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreQueryData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(MENTAgedScoreQuery); }
		}

		protected override Type CollectionType
		{
			get { return typeof(MENTAgedScoreQueryCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new MENTAgedScoreQueryCollection(factory, new ZQuery());
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.MENTAgedScoreQuery; }
		}

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.BusinessEntityProcessWorkflow; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("11518381-452a-46bb-9ada-b2fea0b7b58c", "MENT Aged Score Query"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
