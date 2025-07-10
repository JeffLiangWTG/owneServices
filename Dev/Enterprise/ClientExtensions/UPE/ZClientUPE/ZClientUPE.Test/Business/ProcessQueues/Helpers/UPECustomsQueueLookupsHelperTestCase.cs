using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class UPECustomsQueueLookupsHelperTestCase : UPEProcessQueueLookupsHelperTestCase
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
