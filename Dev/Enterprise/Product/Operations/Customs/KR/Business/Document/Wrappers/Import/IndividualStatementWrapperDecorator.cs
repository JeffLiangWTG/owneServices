using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public static class IndividualStatementWrapperDecorator
	{
		public static void Decorate(this IndividualStatementWrapper wrapper, CusEntryHeader entry)
		{
			foreach (var entryLine in entry.MergedLines)
			{
				wrapper.TotalGrossWeightInKG += entryLine.EffectiveGrossWeight.InKilograms;
			}
			wrapper.TotalPackQty = entry.EntryInstruction?.CEI_PackQty ?? ZInt.Zero;
			wrapper.DeclarationDate = entry.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			if (entry.Declaration.BrokerAddress != null)
			{
				wrapper.Declarant = new OrganizationDocWrapper(new Organisation(RoleType.Declarant)
				{
					CompanyName = entry.Declaration.BrokerAddress.CompanyName,
					RepresentativeName = entry.Declaration.BrokerAddress.Header.GetRepresentativeName(),
					PhoneNumber = entry.Declaration.BrokerAddress.OA_Phone,
					ExtensionNumber = entry.Declaration.BrokerAddress.Header.GetExtensionNumber(),
					Email = entry.Declaration.BrokerAddress.OA_Email
				});
			}
			if (entry.RelatedBill != null && entry.RelatedBill.IsHouseBill)
			{
				wrapper.HouseBillNumber = entry.RelatedBill.CU_BillNum;
			}
			wrapper.CustomsOfficeName = MessageFunctions.GetCustomsOffice(entry.Factory, entry.Declaration?.JE_CustomsOffice ?? ZString.Empty);
		}
	}
}
