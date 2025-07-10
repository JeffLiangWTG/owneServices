
using Enterprise.Customs.EU.Business.Declaration;
namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// UNIT IS RELEASED 
	class CnsChildProcessor_CmiSlr2 : CnsChildProcessor_CerRel2
	{
		protected override void DoFurtherProcessingForSuccessfullyFoundEntry(Business.Declaration.CusEntryHeader entryHeader)
		{
			base.DoFurtherProcessingForSuccessfullyFoundEntry(entryHeader);
			var containerNumber = GetFirstGroupMatchFromMessageText(@"Unit Number:\s+([A-Za-z0-9]{11})");
			CnsChildProcessor_CerRel2.UpdateContainerToStatus(entryHeader, containerNumber, ContainerStatusCodesList.Codes.Released);
		}
	}
}
