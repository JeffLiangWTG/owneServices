using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.CustomerService.Business
{
	public class IncidentApprovalAttachment : NonPersistentBusinessObject, IObsoleteValidation
	{
		public IncidentApprovalAttachment() { }

		public IncidentApprovalAttachment(IeDoc eDoc)
		{
			this.eDoc = eDoc;
		}
		readonly IeDoc eDoc;

		public IeDoc EDoc { get { return eDoc; } }
		public ZString Description { get { return eDoc.Description; } }
		public ZPropertyInfo DescriptionInfo { get { return GetZPropertyInfo(nameof(Description)); } }
		public ZString FileName { get { return eDoc.FileName; } }
		public ZPropertyInfo FileNameInfo { get { return GetZPropertyInfo(nameof(FileName)); } }
	}
}
