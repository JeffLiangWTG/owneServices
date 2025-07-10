using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Export.Business
{
	//Here we had to duplicate CodeDescriptionPairLists from MasterFiles.Business.JobRequiredDocumentLookups
	//We do not have reference to MasterFiles.Business in Accounting.Export
	//The reason Accounting.Export has limited references is this export is used by Web exporter as well
	//Web exporter has limited functionality and additional references might break something with the web exporter.
	//These CodeDescriptionPairLists are protected by Unit test so that these are kept insync with MasterFiles.Business.JobRequiredDocumentLookups
	public static class JobRequiredDocumentLookups
	{
		public static CodeDescriptionPairList AllCategoryType_List
		{
			get
			{
				var allCategoryType_List = new CodeDescriptionPairList();

				allCategoryType_List.AddPair(Constants.ReferenceTypes.All, Constants.ReferenceTypeDescriptions.All);

				allCategoryType_List.AddPair(Constants.ReferenceTypes.Accounting, Constants.ReferenceTypeDescriptions.Accounting);
				allCategoryType_List.AddPair(Constants.ReferenceTypes.BusinessEntityProcessWorkflow, Constants.ReferenceTypeDescriptions.BusinessEntityProcessWorkflow);
				allCategoryType_List.AddPair(Constants.ReferenceTypes.GeneralReferenceTables, Constants.ReferenceTypeDescriptions.GeneralReferenceTables);
				allCategoryType_List.AddPair(Constants.ReferenceTypes.HumanResourcesStaffEmployment, Constants.ReferenceTypeDescriptions.HumanResourcesStaffEmployment);
				allCategoryType_List.AddPair(Constants.ReferenceTypes.ComplianceReport, Constants.ReferenceTypeDescriptions.ComplianceReport);
				allCategoryType_List.AddPair(Constants.ReferenceTypes.Unallocated, (NoResString)"Unallocated");

				allCategoryType_List.AddPair(Constants.ReferenceTypes.ClientSupplierRelationship, Constants.ReferenceTypeDescriptions.ClientSupplierRelationship);
				allCategoryType_List.AddPair(Constants.ReferenceTypes.SupplyChainLogistics, Constants.ReferenceTypeDescriptions.SupplyChainLogistics);

				return allCategoryType_List;
			}
		}

		public static CodeDescriptionPairList DocumentPeriod_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.JobRequiredDocumentPeriods);
			}
		}

		public static CodeDescriptionPairList DocUsage_List
		{
			get
			{
				var docUsage_List = new CodeDescriptionPairList();

				docUsage_List.AddPair("ALL", "ALL");

				docUsage_List.AddPair("BRK", (NoResString)"Broker");
				docUsage_List.AddPair("CRR", (NoResString)"Carrier");
				docUsage_List.AddPair("COM", (NoResString)"Competitor");
				docUsage_List.AddPair("CRT", (NoResString)"Creditor");
				docUsage_List.AddPair("DBT", (NoResString)"Debtor");
				docUsage_List.AddPair("FAG", (NoResString)"Forwarder / Agent");
				docUsage_List.AddPair("ICE", (NoResString)"Importer / Consignee");
				docUsage_List.AddPair("SVS", (NoResString)"Services");
				docUsage_List.AddPair("SCE", (NoResString)"Supplier / Consignor");
				docUsage_List.AddPair("TCT", (NoResString)"Transport Client");
				docUsage_List.AddPair("WAH", (NoResString)"Warehouse");
				docUsage_List.AddPair("ACP", (NoResString)"Attorney for Customs Procedures (ACP)");

				docUsage_List.AddPair("EXP", (NoResString)"Export");
				docUsage_List.AddPair("IMP", (NoResString)"Import");
				docUsage_List.AddPair("BTH", (NoResString)"Both Export and Import");
				docUsage_List.AddPair("DOM", (NoResString)"Domestic");

				return docUsage_List;
			}
		}
	}
}
