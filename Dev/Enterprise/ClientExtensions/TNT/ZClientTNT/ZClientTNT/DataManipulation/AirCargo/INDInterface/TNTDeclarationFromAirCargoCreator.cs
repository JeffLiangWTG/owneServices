
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT
{
	public class TNTDeclarationFromAirCargoCreator : DeclarationFromAirCargoCreator
	{
		public TNTDeclarationFromAirCargoCreator(CusHAWB houseAirCargo)
			: base(houseAirCargo)
		{
			OrganisationCreator = new TemporaryOrganisationCreator(Factory);
		}

		JobDocAddress GetJobDocAddressFromAirCargo(ZString docAddressType)
		{
			ZQuery filter = new ZQuery(JobDocAddressSchema.E2_ParentTableCode, CusHAWBSchema.Constants.Prefix);
			filter.AddToFilter(JobDocAddressSchema.E2_ParentID, HouseAirCargo.PK);
			filter.AddToFilter(JobDocAddressSchema.E2_AddressType, docAddressType);

			return Factory.LoadTop1<JobDocAddress>(filter);
		}

		protected override void CreateCore(Customs.Business.BaseJobDeclaration declaration, INotifications notify)
		{
			base.CreateCore(declaration, notify);
			UpdateDeliveryAndPickupAddress(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.AutoCreateChargesBasedOnIncoTerm = false;
			converter.SetPropertyInfoValue(declaration.JE_AgentsReferenceInfo, HouseAirCargo.CS_HAWB, notify);
			converter.SetPropertyInfoValue(declaration.JE_TotalNoOfPacksPackTypeInfo, ((HouseAirCargo.CS_ShipmentType == "DOC") ? Core.Constants.PkgUnit.Piece : Core.Constants.PkgUnit.Carton), notify);
			converter.SetPropertyInfoValue(declaration.JE_MergeByInfo, OrgConstants.MergeInvoiceLines.Tariff, notify);
			converter.SetPropertyInfoValue(declaration.JE_PaymentMethodInfo, JobDeclaration.PaymentMethods.Broker, notify);
			CreateCommercialInvoiceDetails(declaration);
			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.SupplierPickupAddress.E2_AddressOverride = true;
			HouseAirCargo.CS_JE_CustomsFormalEntry = declaration.PK;
		}

		void UpdateDeliveryAndPickupAddress(Customs.Business.BaseJobDeclaration declaration)
		{
			var pickupAddress = GetJobDocAddressFromAirCargo(DocAddressTypes.Codes.SupplierPickupDeliveryAddress);
			if (pickupAddress != null)
			{
				declaration.SupplierPickupAddress.SynchroniseWithParent(pickupAddress);
			}

			var deliveryAddress = GetJobDocAddressFromAirCargo(DocAddressTypes.Codes.ImporterPickupDeliveryAddress);
			if (deliveryAddress != null)
			{
				declaration.ImporterDeliveryAddress.SynchroniseWithParent(deliveryAddress);
			}
		}

		#region CreateCommercialInvoiceDetails

		void CreateCommercialInvoiceDetails(Customs.Business.BaseJobDeclaration declaration)
		{
			JobComInvoiceHeader invoice = (JobComInvoiceHeader)declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoice.JZ_InvoiceAmount = HouseAirCargo.CS_GoodsValue;
			RefCurrency goodsCurrency = HouseAirCargo.GoodsCurrency;
			invoice.JZ_RX_NKInvoice_Currency = (goodsCurrency == null) ? ZString.Empty : goodsCurrency.RX_Code;
			invoice.JZ_Weight = declaration.JE_TotalWeight;
			invoice.JZ_WeightUQ = declaration.JE_TotalWeightUnit;
			invoice.JZ_ValuationBasis = "TV";
			invoice.AddInfo.ZA_HeaderREL_Hidden = "N";
			RefUNLOCO origin = (RefUNLOCO)Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, declaration.JE_RL_NKOrigin);
			invoice.ZA_ORG = (origin == null) ? ZString.Empty : origin.RL_RN_NKCountryCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = new ZDecimal(declaration.JE_TotalNoOfPacks);
			invoiceLine.JI_InvoiceUQ = declaration.JE_TotalNoOfPacksPackType;
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			invoiceLine.JI_Weight = invoice.JZ_Weight;
			invoiceLine.JI_WeightUQ = invoice.JZ_WeightUQ;
			invoiceLine.AddInfo.ZA_ValuationBasis_Hidden = "";

			ZDecimal overseasInsuranceAmount = invoice.JZ_Calc_FOBAmount / 400m;
			JobComInvoiceGroupHeader groupHeader = (JobComInvoiceGroupHeader)declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, overseasInsuranceAmount, (invoice.Invoice_Currency != null) ? invoice.Invoice_Currency.RX_Code : ZString.Empty);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 0m, GlbCompany.CurrentCompany.LocalCurrency.RX_Code);
		}

		#endregion

		#region GetConsignorPK

		protected override ZGuid GetConsignorPK(INotifications notify, Customs.Business.BaseJobDeclaration declaration)
		{
			OrgHeader consignor = HouseAirCargo.Consignor ?? CreateTemporaryConsignor(notify, declaration);

			return consignor == null ? ZGuid.Empty : consignor.PK;
		}

		OrgHeader CreateTemporaryConsignor(INotifications notify, Customs.Business.BaseJobDeclaration declaration)
		{
			return OrganisationCreator.CreateConsignor(TNTOrganisation.Consignor(HouseAirCargo), notify);
		}

		#endregion

		#region GetConsigneePK

		protected override ZGuid GetConsigneePK(INotifications notify, Customs.Business.BaseJobDeclaration declaration)
		{
			OrgHeader consignee = HouseAirCargo.Consignee ?? CreateTemporaryConsignee(notify, declaration);

			return consignee == null ? ZGuid.Empty : consignee.PK;
		}

		OrgHeader CreateTemporaryConsignee(INotifications notify, Customs.Business.BaseJobDeclaration declaration)
		{
			return OrganisationCreator.CreateConsignee(TNTOrganisation.Consignee(HouseAirCargo), notify);
		}

		#endregion

		readonly TemporaryOrganisationCreator OrganisationCreator;
	}
}
