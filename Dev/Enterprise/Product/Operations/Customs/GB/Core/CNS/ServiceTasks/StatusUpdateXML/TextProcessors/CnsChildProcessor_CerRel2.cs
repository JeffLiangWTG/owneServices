using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// NOTIFICATION OF UNIT RELEASE       
	// NOTIFICATION CHANGE OF RELEASEE 
	class CnsChildProcessor_CerRel2 : CnsChildProcessor
	{
		protected override ZString RegexPatternForUcn
		{
			// Some have "UCN No.:", others have "UCN:", pfffff.  Can't add a new optional unit for " No.:" because this would make a new gorup, and we're looking for the 1st group. 
			get
			{
				return receivedEdiMessage.EM_MessageText.Contains("CHANGE OF RELEASEE") ?
					@"UCN:\s+([A-Za-z0-9]+?)\s"
				: @"UCN No\.:?\s+([A-Za-z0-9]+?)\s";
			}
		}

		protected override void DoFurtherProcessingForSuccessfullyFoundEntry(Business.Declaration.CusEntryHeader entryHeader)
		{
			base.DoFurtherProcessingForSuccessfullyFoundEntry(entryHeader);
			if (receivedEdiMessage.EM_MessageText.Contains("NOTIFICATION OF UNIT RELEASE"))
			{
				var containerNumber = GetFirstGroupMatchFromMessageText(@"Unit Number:\s+([A-Za-z0-9]{11})");
				UpdateContainerToStatus(entryHeader, containerNumber, ContainerStatusCodesList.Codes.Released);
			}
		}

		internal static void UpdateContainerToStatus(Business.Declaration.CusEntryHeader entryHeader, ZString containerNumber, string newStatus)
		{
			if (!containerNumber.IsEmpty)
			{
				var cusContainer = (from CusContainer c in entryHeader.Declaration.CusContainers where c.CO_ContainerNumber == containerNumber select c).FirstOrDefault();
				if (cusContainer != null)
				{
					cusContainer.CO_MessageStatus = newStatus;
				}
			}
		}
	}
}
