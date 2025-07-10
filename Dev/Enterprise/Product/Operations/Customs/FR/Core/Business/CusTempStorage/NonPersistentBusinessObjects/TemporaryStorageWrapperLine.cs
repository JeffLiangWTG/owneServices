using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageWrapperLine : AutoTemporaryStorageWrapperLine
	{
		[BusinessObjectTestExclude]
		public TemporaryStorageWrapperFurtherDetailCollection TemporaryStorageFurtherDetails { get; set; }
	}
}
