using CargoWise.Customs.CL.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class ParticipantsDocumentWrapper : IParticipantDocuments
	{
		internal ParticipantsDocumentWrapper()
		{
		}

		string IParticipantDocuments.IDValue => GlbCompany.CurrentCompany.GC_BusinessRegNo;
	}
}
