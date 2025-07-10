using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	public class CustomsTestDataCreator : ICustomsTestDataCreator
	{
		public void CreateAttachedSeaCargo(ZGuid shipmentPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var cusSCAHouse = factory.New<BaseCusSCAHouse>();
			cusSCAHouse.CA_JS = shipmentPK;

			EDIMessage message1 = cusSCAHouse.Messages.AddNew();
			message1.EM_ReceiveTransmit = "RCV";
			message1.EM_ApplicationCode = "APP";
			message1.EM_MessageType = "TYP";
			message1.EM_MessageSubType = "SUB";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 12, 15, 15, 26, 17);
			message1.EM_ApplicationReference = "AUC";
			message1.EM_MessageNum = "123";

			factory.Save();
		}

		public void CreateAttachedAirCargo(ZGuid shipmentPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			CommonShipment shipment = factory.Load<CommonShipment>(shipmentPK);

			foreach (var consol in shipment.Consols)
			{
				CusMAWB cusMawb = factory.New<CusMAWB>();
				cusMawb.CM_JK = consol.PK;

				CusHAWB cusHawb = factory.New<CusHAWB>();
				cusHawb.CS_JS = shipmentPK;
				cusHawb.CS_CM = cusMawb.PK;
			}

			factory.Save();
		}

		public ZGuid CreateStandAloneDeclarationData()
			=> CreateDeclarationData(null, false);

		public ZGuid CreateAttachedDeclarationData(ZGuid shipmentPK, bool isCancelled)
			=> CreateDeclarationData(shipmentPK, isCancelled);

		ZGuid CreateDeclarationData(ZGuid? shipmentPKNullable, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();
			var jobDeclaration = factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_GB = Env.CurrentBranch.PK;
			var declarationPK = jobDeclaration.PK;

			if (isCancelled)
			{
				jobDeclaration.IsCancelled = true;
			}

			var orgAddress = factory.New<OrgAddress>();
			orgAddress.OA_OH = Env.CurrentCompany.OrganisationPK;
			orgAddress.OA_Address1 = "Eugene Leroy Street ";

			var jobOrderHeader = factory.New<Order>();
			jobOrderHeader.JD_JE = declarationPK;
			jobOrderHeader.JD_OA_BuyerAddress = orgAddress.PK;

			var jobOrderLine = factory.New<OrderLine>();
			jobOrderLine.JO_JD = jobOrderHeader.PK;

			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			cusEntryInstruction.CEI_JE = jobDeclaration.PK;

			var houseBill = jobDeclaration.Bills.AddNew();

			var jobComInvoiceHeader = factory.New<BaseJobComInvoiceHeader>();
			jobComInvoiceHeader.JZ_JE = jobDeclaration.PK;
			jobComInvoiceHeader.JZ_CU_RelatedHouseBill = houseBill.PK;

			var cusEntryHeader = factory.New<CusEntryHeader>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			cusEntryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			var cusEntrySnapshot = factory.New<CusEntrySnapshot>();
			cusEntrySnapshot.CES_CH_EntryHeader = cusEntryHeader.PK;

			var cusEntryHeaderCharges = factory.New<CusEntryHeaderCharges>();
			cusEntryHeaderCharges.C1_CH = cusEntryHeader.PK;

			var cusEntryPayInfo = factory.New<CusEntryPayInfo>();
			cusEntryPayInfo.C9_CH = cusEntryHeader.PK;

			var cusEntryHeaderChild1 = factory.New<CusEntryHeader>();
			cusEntryHeaderChild1.CH_JE = jobDeclaration.PK;
			cusEntryHeaderChild1.CH_CH_PrimeEntry = cusEntryHeader.PK;

			var cusEntryCPDec = factory.New<BaseCusEntryCPDec>();
			cusEntryCPDec.ON_CH = cusEntryHeader.PK;

			var cusContainer = factory.New<BaseCusContainer>();
			cusContainer.CO_JE = jobDeclaration.PK;

			var cusContainerEntryHeaderPivot = factory.New<CusContainerEntryHeaderPivot>();
			cusContainerEntryHeaderPivot.CCE_CH_EntryHeader = cusEntryHeader.PK;
			cusContainerEntryHeaderPivot.CCE_CO_Container = cusContainer.PK;

			var packingGroup = factory.New<BasePackingGroup>();
			packingGroup.CR_CU_HouseBill = houseBill.PK;

			var package = factory.New<BasePackage>();
			package.CW_CR_HouseContainer = packingGroup.PK;

			var cusHouseContPackInvoiceHeaderPivot = factory.New<InvoiceHeaderPackagePivot>();
			cusHouseContPackInvoiceHeaderPivot.CHZ_JE = declarationPK;
			cusHouseContPackInvoiceHeaderPivot.CHZ_JZ = jobComInvoiceHeader.PK;
			cusHouseContPackInvoiceHeaderPivot.CHZ_CW = package.PK;

			var interchange1 = factory.New<EDIInterchange>();
			interchange1.FillWithValidTestData();
			interchange1.EI_From = "CargoWise";
			interchange1.EI_To = "Customs";
			interchange1.EI_InterchangeNum = jobDeclaration.JE_DeclarationReference;

			var message1 = factory.New<EDIMessage>();
			message1.FillWithValidTestData();
			message1.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message1.EM_LinkUniqueID = jobDeclaration.PK;
			message1.EM_EI = interchange1.PK;
			message1.EM_ReceiveTransmit = "RCV";
			message1.EM_ApplicationCode = "APP";
			message1.EM_MessageType = "TYP";
			message1.EM_MessageSubType = "SUB";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 12, 15, 15, 26, 18);
			message1.EM_ApplicationReference = "AUC";
			message1.EM_MessageNum = "234";

			var message2 = factory.New<EDIMessage>();
			message2.FillWithValidTestData();
			message2.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message2.EM_LinkUniqueID = jobDeclaration.PK;
			message2.EM_EI = interchange1.PK;
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_ApplicationCode = "APP";
			message2.EM_MessageType = "TYP";
			message2.EM_MessageSubType = "SUB";
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 12, 15, 15, 26, 19);
			message2.EM_ApplicationReference = "AUC";
			message2.EM_MessageNum = "345";

			if (shipmentPKNullable != null)
			{
				var shipmentPK = shipmentPKNullable.Value;

				var shipment1 = factory.Load<CommonShipment>(shipmentPK);
				jobDeclaration.JE_JS = shipmentPK;
				jobOrderHeader.JD_JS = shipmentPK;
				interchange1.EI_InterchangeNum = shipment1.JS_UniqueConsignRef;

				foreach (var consol in shipment1.Consols)
				{
					var cusMawb = factory.New<CusMAWB>();
					cusMawb.CM_JK = consol.PK;

					var cusHawb = factory.New<CusHAWB>();
					cusHawb.CS_JE_CustomsFormalEntry = jobDeclaration.PK;
					cusHawb.CS_CM = cusMawb.PK;
				}

				Order[] jobOrderHeaders = factory.Load<Order>(new ZQuery(JobOrderHeaderSchema.JD_JS, shipmentPK));
				if (jobOrderHeaders != null && jobOrderHeaders.Length > 0)
				{
					foreach (var order in jobOrderHeaders)
					{
						order.JD_JE = jobDeclaration.PK;

						foreach (var orderLine in order.OrderLines)
						{
							var cusEntryLine = factory.New<CusEntryLine>();
							cusEntryLine.CL_CH = cusEntryHeader.PK;

							var invoiceLine = factory.New<BaseJobComInvoiceLine>();
							invoiceLine.JI_JZ = jobComInvoiceHeader.PK;
							invoiceLine.JI_JO = orderLine.PK;
							invoiceLine.JI_CEI = cusEntryInstruction.PK;
							invoiceLine.JI_CL = cusEntryLine.PK;

							var jobComInvoiceLineTax = factory.New<JobComInvoiceLineTax>();
							jobComInvoiceLineTax.JLT_JI = invoiceLine.PK;
							jobComInvoiceLineTax.JLT_Type = "A00";

							var cusHouseContPackInvoiceLinePivot = factory.New<InvoiceLinePackagePivot>();
							cusHouseContPackInvoiceLinePivot.CHC_JI = invoiceLine.PK;
							cusHouseContPackInvoiceLinePivot.CHC_JE = jobDeclaration.PK;
							cusHouseContPackInvoiceLinePivot.CHC_CW = package.PK;
						}
					}
				}
			}

			factory.Save();

			return declarationPK;
		}

		public void CreateOceanBillForConsol(ZGuid consolPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var bill = factory.New<BaseCusSCAOceanBill>();
			bill.CB_ParentId = consolPK;
			bill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			factory.Save();
		}
	}
}
