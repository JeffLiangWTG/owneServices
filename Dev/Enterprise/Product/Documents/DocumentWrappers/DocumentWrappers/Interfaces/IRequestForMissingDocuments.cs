using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IRequestForMissingDocuments
	{
		ZString Context { get; }
		ZString EmailSubjectNumber { get; }
		ZString MasterBillHeading { get; }
		ZString MasterBillAndIssueHeading { get; }
		ZString MasterBillNum { get; }
		ZString MasterBillAndIssueDate { get; }
		ZString HouseBillHeading { get; }
		ZString HouseBillAndIssueHeading { get; }
		ZString HouseBill { get; }
		ZString HouseBillAndIssueDate { get; }
		ZString DeclarationOrConsolNumber { get; }
		ZString GoodsDescription { get; }
		ZString OwnerRefAndOrderRefHeading { get; }
		ZString OwnerRefAndOrderRef { get; }
		ZString Packages { get; }
		ZString Weight { get; }
		ZString WeightUnit { get; }
		ZString Volume { get; }
		ZString VolumeUnit { get; }
		ZString TransportHeading { get; }
		ZString TransportInfo { get; }
		ZString MissingRequiredDocuments { get; }
		ZString RequestForMissingDocumentsInstruction { get; }
		ZString ContainerNumbers { get; }
		ZString ConsigneeOrgHeading { get; }
		ZString ConsignorOrgHeading { get; }

		DocOrganisation ConsigneeOrg { get; }
		DocOrganisation ConsignorOrg { get; }
	}
}
