using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class UPECustomsQueueValidationHelperTestCase : UPEProcessQueueValidationHelperTestCase
	{
		protected override sealed ProcessQueueType.Enum ExpectedQueueType
		{
			get
			{
				return ProcessQueueType.Enum.Customs;
			}
		}
	}
}
