using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class JobRequiredDocumentAddInfoFilterBusinessObject : MasterFiles.Module.JobRequiredDocumentAddInfoFilterBusinessObject
	{
		public new static class Constants
		{
			public const string URN = "URN";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = base.GetModuleFiltersCore();
			collection.AddTextFilter(Constants.URN, JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber).MultilingualDescription = ResString.GetMultilingualString("8254A38A-D41C-47D1-9FAD-238AA11539C4", "URN");
			return collection;
		}
	}
}
