using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocMAFCoverSheet : DocBaseWrapper
	{
		public DocMAFCoverSheet(BusinessObjectFactory factoryToWrap, NZDocsMAFCoverSheet coverSheet)
			: base(null, factoryToWrap)
		{
			this.CoverSheet = coverSheet;
		}
		readonly NZDocsMAFCoverSheet CoverSheet;

		public DocMAFCoverSheetPageCollection Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = new DocMAFCoverSheetPageCollection(Factory, CoverSheet);
				}
				return fPage;
			}
		}
		DocMAFCoverSheetPageCollection fPage;

		#region Fields exposed directly from NZDocsMAFCoverSheet
		public ZString AccountHolder
		{
			get { return CoverSheet.D0_AccountHolder; }
		}
		public ZString AgentCompanyName
		{
			get { return CoverSheet.D0_AgentCompanyName; }
		}
		public ZString AgentContactName
		{
			get { return CoverSheet.D0_AgentContactName; }
		}
		public ZString AgentEmail
		{
			get { return CoverSheet.D0_AgentEmail; }
		}
		public ZString AgentFaxNumber
		{
			get { return CoverSheet.D0_AgentFaxNumber_Formatted; }
		}
		public ZString AgentPhoneNumber
		{
			get { return CoverSheet.D0_AgentPhoneNumber_Formatted; }
		}
		public ZString ClientReference
		{
			get { return CoverSheet.D0_ClientReference; }
		}
		public ZInt TotalPages
		{
			get { return CoverSheet.D0_TotalPages; }
		}
		public ZDateTime DateOfArrival
		{
			get { return CoverSheet.D0_DateOfArrival; }
		}
		public ZDateTime DateSigned
		{
			get { return CoverSheet.D0_DateSigned; }
		}
		public ZString EDITariffCodes
		{
			get { return CoverSheet.D0_EDITariffCodes; }
		}
		public ZString EntryNumber
		{
			get { return CoverSheet.D0_EntryNumber; }
		}
		public ZString ExporterName
		{
			get { return CoverSheet.D0_ExporterName; }
		}
		public ZString HouseBill
		{
			get { return CoverSheet.D0_HouseBill; }
		}
		public ZString ImporterName
		{
			get { return CoverSheet.D0_ImporterName; }
		}
		public ZString MasterBill
		{
			get { return CoverSheet.D0_MasterBill; }
		}
		public ZString PayByCash
		{
			get { return CoverSheet.D0_PayBeCash ? "X" : " "; }
		}
		public ZString PayByCheque
		{
			get { return CoverSheet.D0_PayBeCheque ? "X" : " "; }
		}
		public ZString PayByAccount
		{
			get { return CoverSheet.D0_PayByAccount ? "X" : " "; }
		}
		public ZString QENumber
		{
			get { return CoverSheet.D0_QENumber; }
		}
		public ZString Destination
		{
			get
			{
				RefUNLOCO destination = CoverSheet.Destination;
				return CoverSheet.D0_RL_NKDestination + (destination == null ? "" : " - " + destination.RL_PortName);
			}
		}
		public ZString PortOfDischarge
		{
			get
			{
				RefUNLOCO portOfDischarge = CoverSheet.PortOfDischarge;
				return CoverSheet.D0_RL_NKPortOfDischarge + (portOfDischarge == null ? "" : " - " + portOfDischarge.RL_PortName);
			}
		}
		public ZString CountryOfOrigin
		{
			get
			{
				RefCountry countryOfOrigin = CoverSheet.CountryOfOrigin;
				return CoverSheet.D0_RN_NKCountryOfOrigin + (countryOfOrigin == null ? "" : " - " + countryOfOrigin.RN_DescMultilingual);
			}
		}
		public ZString ShippingOrAirLine
		{
			get { return CoverSheet.D0_ShippingOrAirLine; }
		}
		public ZString SignatoryCompanyName
		{
			get { return CoverSheet.D0_SignatoryCompanyName; }
		}
		public ZString SignatoryFullName
		{
			get { return CoverSheet.D0_SignatoryName; }
		}
		public ZString SignatoryPhoneNumber
		{
			get { return CoverSheet.D0_SignatoryPhoneNumber; }
		}
		public ZString DocsSuppliedBillOfLading
		{
			get { return CoverSheet.D0_SuppliedBillOfLading ? "X" : " "; }
		}
		public ZString DocsSuppliedCertificates
		{
			get { return CoverSheet.D0_SuppliedCertificates ? "X" : " "; }
		}
		public ZString DocsSuppliedComplianceAgreement
		{
			get { return CoverSheet.D0_SuppliedComplianceAgreement ? "X" : " "; }
		}
		public ZString DocsSuppliedComplianceCheckCompleted
		{
			get { return CoverSheet.D0_SuppliedComplianceCheckCompleted ? "X" : " "; }
		}
		public ZString DocsSuppliedIHS
		{
			get { return CoverSheet.D0_SuppliedIHS ? "X" : " "; }
		}
		public ZString DocsSuppliedImportPermit
		{
			get { return CoverSheet.D0_SuppliedImportPermit ? "X" : " "; }
		}
		public ZString DocsSuppliedOtherDocumentation
		{
			get { return CoverSheet.D0_SuppliedOtherDocumentation; }
		}
		public ZString DocsSuppliedQuarantineDeclaration
		{
			get { return CoverSheet.D0_SuppliedQuarantineDeclaration ? "X" : " "; }
		}
		public ZString DocsSuppliedRelevantInvoices
		{
			get { return CoverSheet.D0_SuppliedRelevantInvoices ? "X" : " "; }
		}
		public ZString ToMAFQuarantineService
		{
			get { return CoverSheet.D0_ToMAFQuarantineService; }
		}
		public ZString TransitionalFacility
		{
			get { return CoverSheet.D0_TransitionalFacility; }
		}
		public ZString TreatmentSupplier
		{
			get { return CoverSheet.D0_TreatmentSupplier; }
		}
		public ZString Vessel
		{
			get { return CoverSheet.D0_Vessel; }
		}
		public ZString VoyageOrFlight
		{
			get { return CoverSheet.D0_VoyageOrFlight; }
		}
		#endregion

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return CoverSheet != null ? CoverSheet.Parent : base.BusinessObjectToLogAgainst; }
		}
	}
}
