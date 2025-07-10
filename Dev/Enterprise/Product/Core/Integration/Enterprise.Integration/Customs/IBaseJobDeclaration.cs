using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		[GlowInterfaceReference("IJobDeclaration")]
		public interface IBaseJobDeclaration : IBusiness, ICancellable
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }
			ZInt JE_ClusterKey { get; set; }

			ZString JE_DeclarationReference { get; set; }
			ZGuid JE_GB { get; set; }
			ZGuid JE_GC { get; set; }
			ZString JE_HouseBill { get; set; }
			ZGuid JE_JS { get; set; }
			ZString JE_AddInfo { get; set; }
			ZString JE_ContainerMode { get; set; }
			ZString JE_MasterBill { get; set; }
			ZString JE_MessageType { get; set; }
			ZDateTime JE_SystemCreateTimeUtc { get; set; }
			ZBool JE_IsCancelled { get; set; }
			ZString JE_ApplicationCode { get; set; }
			ZString JE_OwnerRef { get; set; }
			ZString JE_EntryStatus { get; set; }
			ZDateTime JE_EntrySubmittedDate { get; set; }
			ZGuid JE_OH_Supplier { get; set; }
			ZGuid JE_OH_Importer { get; set; }
			ZString JE_ScreeningStatus { get; set; }
			ZString JE_RL_NKOrigin { get; set; }
			ZString JE_RL_NKFinalDestination { get; set; }
			ZString JE_AgentsReference { get; set; }
			ZInt JE_LandedPieces { get; set; }
			ZInt JE_TotalNoOfPacks { get; set; }
			ZBool JE_OverrideFreightDefaults { get; set; }
			bool IsDeclarationMatchSpecificCountry(ZString countryCode);
			ZBool IsExport { get; }
			ZGuid CompanyPK { get; }
			ZBool IsReciprocalRates { get; }
			ZString LocalCurrencyCode { get; }
			ZString GetContainerMode(ZString transportMode, ZString shipmentPackingMode);
			ZDate DateForDutyRate { get; }
			ZString GetCreditCheckMessage();
			bool IsDeclarationIntegrated { get; }
			IInvoiceHeaderActiveCollection Invoices { get; }
			IBusinessObjectCollection InvoiceLines { get; }
			bool IsInvoiceLinesLoaded { get; }
		}
	}
}
