using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EUExitControl;
using JobDeclaration = Enterprise.Customs.EU.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitReport : EU.ExitControl.Business.CusExitReport
		, Integration.Customs.DEExitControl.ICusExitReport
		, ISupportAutoSendExitReportTransferMessage
	{
		public CusExitReport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusExitHeader Header => (CusExitHeader)base.Header;

		public new CusExitConsignment Consignment => (CusExitConsignment)base.Consignment;

		public new CusExitReportValidation Validation => (CusExitReportValidation)base.Validation;
		protected override ExitControlBase.Business.CusExitReportValidation GetNewValidation() => new CusExitReportValidation(this);

		public new CusExitReportLookups Lookups => (CusExitReportLookups)base.Lookups;
		protected override ExitControlBase.Business.CusExitReportLookups GetNewLookups() => new CusExitReportLookups(this);

		public IProcessor GetSendExitReportTransferMessageProcessor(BusinessObject parent) => new SendExitReportTransferMessageProcessor(parent);

		[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.OfficeOfExportList))]
		[ResourceStringData("DB879B28-4F1A-4C10-ACDA-FB3E932EB2F6", Caption = "Intended Office of Exit", MediumCaption = "Int. Office of Exit", ShortCaption = "Int. Office")]
		public override ZString CER_OfficeOfExport { get => base.CER_OfficeOfExport; set => base.CER_OfficeOfExport = value; }

		[ResourceStringData("F6A93B7B-C8C0-4C11-AD6B-60D55FEF03A6", Caption = "Exit Date & Time", MediumCaption = "Exit Date", ShortCaption = "Date")]
		public override ZDateTimeOffset CER_DateTime { get => base.CER_DateTime; set => base.CER_DateTime = value; }

		protected override void SetReportBehaviorFromConsignmentItemsCore()
		{
		}

		protected override void ValidateRepresentative(JobDocAddressValidation validation)
		{
			base.ValidateRepresentative(validation);

			var representativeDocAddress = validation.Parent;
			if (!representativeDocAddress.OrganisationPK.IsEmpty && !representativeDocAddress.E2_OA_Address.IsEmpty)
			{
				var representativeAddress = representativeDocAddress.Address;
				var eoriCodeIsMissing = representativeAddress.Header.GetEUEoriDetails().IsEmpty;
				var eoriBranchIsMissing = representativeAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix).IsEmpty;
				if (eoriCodeIsMissing || eoriBranchIsMissing)
				{
					representativeDocAddress.OrganisationPKInfo.AddMessageError(Res.GetString("129337CC-04AE-41AD-A754-0A13AC3342E9", "Representative is missing {0}", EORIHelper.GetMissingEoriMessage(eoriCodeIsMissing, eoriBranchIsMissing)));
				}
			}
		}

		protected override void OnChangedRepresentativeJobDocAddressRequirement() => Representative.Validation.ValidateOrganisationPK();

		protected override void ValidateDeclarant(JobDocAddressValidation validation)
		{
			base.ValidateDeclarant(validation);

			var declarantDocAddress = validation.Parent;
			if (!declarantDocAddress.OrganisationPK.IsEmpty && !declarantDocAddress.E2_OA_Address.IsEmpty && Representative.OrganisationPK == ZGuid.Empty)
			{
				var declarantAddress = declarantDocAddress.Address;
				var eoriCodeIsMissing = declarantAddress.Header.GetEUEoriDetails().IsEmpty;
				var eoriBranchIsMissing = declarantAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix).IsEmpty;
				if (eoriCodeIsMissing || eoriBranchIsMissing)
				{
					declarantDocAddress.OrganisationPKInfo.AddMessageError(Res.GetString("69195356-0BE6-46F4-9201-E3C8F6012684", "Declarant is missing {0}", EORIHelper.GetMissingEoriMessage(eoriCodeIsMissing, eoriBranchIsMissing)));
				}
			}
		}

		protected override void DefaultDataFromDeclaration(JobDeclaration declaration)
		{
			base.DefaultDataFromDeclaration(declaration);

			var declarationOfficeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>()
				.FirstOrDefault(office => office.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);

			if (declarationOfficeOfExit != null && CER_OfficeOfExit.IsEmpty)
			{
				CER_OfficeOfExit = declarationOfficeOfExit.CY_Data.Left(CusExitReport.Schema.CER_OfficeOfExitMaxLength);
			}
		}

		protected override void DefaultDataFromShipment(ForwardingShipment shipment)
		{
			base.DefaultDataFromShipment(shipment);

			DefaultFlightDataFromShipment(shipment);
		}

		void DefaultFlightDataFromShipment(ForwardingShipment shipment)
		{
			var (transport, airlineCountryCode) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
			if (transport != null)
			{
				CER_TransportMode = transport.JW_TransportMode;
				CER_TransportID = transport.JW_VoyageFlightForBinding;
				if (CER_RN_NKTransportNationality.IsEmpty)
				{
					CER_RN_NKTransportNationality = airlineCountryCode;
				}
				CER_DateTime = transport.JW_ETDForBinding.ToLocalBranchTimeOffset();
				CER_TransportType = CusExitReportTransportTypeList.Codes._40;
			}
		}

		public bool IsAutomatedValidationEnabledTRA => Header?.IsAutomatedValidationEnabledTRA ?? false;

		public Guid RegistryBranchPK
		{
			get
			{
				GlbBranch branch = Header.Branch;
				return (branch == null) ? GlbBranch.CurrentBranch.PK.ToGuid() : branch.PK.ToGuid();
			}
		}

		public IProcessor CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode)
			=> new CustomsStmProcessQueueCreatorProcessor(parent, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CER_IsFinalized = true;
		}

		protected override void OnChangedDeclarantJobDocAddressRequirement() => Declarant.Validation.ValidateOrganisationPK();

		[ResourceStringData("2E1B05A8-91B1-43F0-A67A-B2F00A9DC733", Caption = "Location")]
		[MaxLength(Schema.CER_LocationMaxLength)]
		public ZString Location
		{
			get => CER_Location;
			set
			{
				var oldValue = location;
				location = value;
				if (!IsCopying && oldValue != location)
				{
					var orgQuery = new ZQuery();
					orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, location);
					var orgHeader = Factory.LoadTop1<OrgHeader>(orgQuery);
					if (orgHeader != null)
					{
						UpdateCER_LocationAndExitOffice(orgHeader);
					}
					else
					{
						CER_Location = value;
					}
					LocationInfo.RefreshBinding();
				}
			}
		}
		ZString location;

		public ZPropertyInfo LocationInfo => GetZPropertyInfo(nameof(Location));

		void UpdateCER_LocationAndExitOffice(OrgHeader orgHeader)
		{
			var orgAddress = orgHeader.Addresses.OfType<OrgAddress>().FirstOrDefault(x => x.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Pickup));
			if (orgAddress != null)
			{
				var address1 = orgAddress.Address1;
				var postCode = orgAddress.Postcode.SubstringSafe(0, 9);
				var city = orgAddress.City.SubstringSafe(0, 35);
				var maxLengthForCompanyName = Schema.CER_LocationMaxLength - address1.Length - postCode.Length - city.Length - 3;
				var result = orgHeader.OH_FullName.SubstringSafe(0, maxLengthForCompanyName) + "," + address1 + "," + postCode + "," + city;
				CER_Location = result;
				if (CER_OfficeOfExit.IsEmpty)
				{
					CER_OfficeOfExit = orgHeader.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit).SubstringSafe(0, Schema.CER_OfficeOfExitMaxLength);
				}
			}
			else
			{
				CER_Location = ZString.Empty;
			}
		}
	}
}
