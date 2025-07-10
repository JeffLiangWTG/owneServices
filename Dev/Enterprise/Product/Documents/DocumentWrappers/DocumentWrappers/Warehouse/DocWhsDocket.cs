using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Barcode.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocWhsDocket : DocBaseWrapper, IDocServicesParent
	{
		#region Constructors

		public DocWhsDocket(WhsDocket whsDocket, BusinessObjectFactory factoryToWrap)
			: base(whsDocket, factoryToWrap)
		{
		}

		protected DocWhsDocket(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
			: base(docketLabel.Docket, factoryToWrap)
		{
			this.DocketLabel = docketLabel;
		}

		#endregion

		#region Related Business Objects

		#region Collections

		public DocWhsDocketLineCollection Lines
		{
			get { return GetDocketLines(); }
		}

		protected abstract DocWhsDocketLineCollection GetDocketLines();

		#endregion

		#region Implementation

		WhsDocket WhsDocket
		{
			get { return (WhsDocket)WrappedObject; }
		}

		public WhsPick Pick
		{
			get { return Factory.Load<WhsPick>(WhsDocket.WD_WP); }
		}

		public DocWhsDocketContainerCollection DocketContainers
		{
			get { return new DocWhsDocketContainerCollection(WhsDocket.Containers, Factory); }
		}

		#endregion

		#endregion

		#region Properties

		#region Customs Fields

		public ZBool IsCustomsTransaction
		{
			get { return WhsDocket.IsCustomsTransaction; }
		}

		public ZString CustomAttrib1
		{
			get { return WhsDocket.WD_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return WhsDocket.WD_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return WhsDocket.WD_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return WhsDocket.WD_CustomAttrib4; }
		}

		public ZString CustomAttrib5
		{
			get { return WhsDocket.WD_CustomAttrib5; }
		}

		public ZDateTime CustomDate1
		{
			get { return WhsDocket.WD_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return WhsDocket.WD_CustomDate2; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return WhsDocket.WD_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return WhsDocket.WD_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return WhsDocket.WD_CustomDecimal3; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return WhsDocket.WD_CustomDecimal4; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return WhsDocket.WD_CustomDecimal5; }
		}

		public ZBool CustomFlag1
		{
			get { return WhsDocket.WD_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return WhsDocket.WD_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return WhsDocket.WD_CustomFlag3; }
		}

		public ZBool CustomFlag4
		{
			get { return WhsDocket.WD_CustomFlag4; }
		}

		public ZBool CustomFlag5
		{
			get { return WhsDocket.WD_CustomFlag5; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTimeOffset BookingDate
		{
			get { return WhsDocket.WD_BookingDate; }
		}

		public ZDateTimeOffset ArrivalDate
		{
			get { return WhsDocket.WD_ArrivalDate; }
		}

		public ZDateTimeOffset RequiredDate
		{
			get { return WhsDocket.WD_RequiredDate; }
		}

		public ZDateTimeOffset FinalisedDate
		{
			get { return WhsDocket.WD_FinalisedDate; }
		}

		#endregion

		#region ZString Fields

		public ZString WhoCreated
		{
			get { return WhsDocket.Logs.CreatedByUserInitials; }
		}

		public ZString WhoFinalized
		{
			get { return WhsDocket.Logs.CreatedByUserInitials; }
		}

		public ZString ConsigneeName => ConsigneeNameCore;

		protected virtual ZString ConsigneeNameCore => ZString.Empty;

		public ZString CarrierName => CarrierNameCore;

		protected abstract ZString CarrierNameCore { get; }

		public ZString DestinationPort => DestinationPortCore;

		protected virtual ZString DestinationPortCore => ZString.Empty;

		public override string ToString()
		{
			return DocketID;
		}

		public ZString DGContact
		{
			get
			{
				ZString result = "";
				if (WhsDocket.Client != null)
				{
					result = WhsDocket.Client.MiscServ.UNDGContact != null ? WhsDocket.Client.MiscServ.UNDGContact.OC_ContactName : ZString.Empty;
				}
				return result;
			}
		}

		public ZString EmergencyNumber
		{
			get { return WhsDocket.Client != null ? WhsDocket.Client.MiscServ.DGPhoneNumber_Formatted : ZString.Empty; }
		}

		public ZString ClientName
		{
			get { return WhsDocket.Client != null ? WhsDocket.Client.OH_FullName : ZString.Empty; }
		}

		public ZString ClientNameAndAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (WhsDocket.Client != null)
				{
					foreach (DocAddress docAddress in new DocAddressCollection(WhsDocket.Client.Addresses, Factory))
					{
						if (docAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Receivables))
						{
							result = docAddress.PostalAddress;
							break;
						}
					}
					if (result.IsEmpty)
					{
						result = DocOrganisation.New(Factory, WhsDocket.Client.PK).PostalAddress;
					}
				}
				return result;
			}
		}

		public ZString ClientAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (WhsDocket.Client != null)
				{
					foreach (DocAddress docAddress in new DocAddressCollection(WhsDocket.Client.Addresses, Factory))
					{
						if (docAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Receivables))
						{
							result = docAddress.PostalAddressExcludeName;
							break;
						}
					}
					if (result.IsEmpty)
					{
						result = DocOrganisation.New(Factory, WhsDocket.Client.PK).PostalAddressExcludeName;
					}
				}
				return result;
			}
		}

		public ZString ClientCode
		{
			get { return WhsDocket.Client != null ? WhsDocket.Client.OH_Code : ZString.Empty; }
		}

		#region CustomerReference

		public ZString CustomerReference
		{
			get { return WhsDocket.WD_CustomerReference; }
		}

		public ZString CustomerReferenceBarcode
		{
			get
			{
				var barcode = new TextBarcode(WhsDocket.WD_CustomerReference);
				return barcode.TextAs128sFontString;
			}
		}

		#endregion

		public virtual ZString TransportReference
		{
			get { return WhsDocket.WD_TransportReference; }
		}

		public ZString DropOffName
		{
			get { return WhsDocket.DropOffDocAddress != null && WhsDocket.DropOffDocAddress.Organisation != null ? WhsDocket.DropOffDocAddress.Organisation.OH_FullName : ZString.Empty; }
		}

		public ZString SupplierAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (WhsDocket.SupplierDocAddress != null)
				{
					if (WhsDocket.SupplierDocAddress.Address != null && !WhsDocket.SupplierDocAddress.E2_AddressOverride)
					{
						result = DocAddress.New(WhsDocket.SupplierDocAddress.Address, Factory).PostalAddress;
					}
					else
					{
						result = WhsDocket.SupplierDocAddress.E2_CompanyName + "\n" + WhsDocket.SupplierDocAddress.AddressSummary.Replace("\r", String.Empty);
					}
				}
				return result;
			}
		}

		public MultilingualString WarehouseName
		{
			get { return WhsDocket.Warehouse != null ? WhsDocket.Warehouse.WW_WarehouseNameMultilingual : (NoResString)ZString.Empty; }
		}

		public MultilingualString WarehouseNameAndAddress
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (WhsDocket.Warehouse != null)
				{
					result = MultilingualString.Join(System.Environment.NewLine, WarehouseName, (NoResString)WarehouseAddress.ToString());
				}
				return result;
			}
		}

		public ZString WarehousePhoneAndFax
		{
			get
			{
				ZString result = ZString.Empty;
				if (WhsDocket.Warehouse != null)
				{
					if (!WhsDocket.Warehouse.WarehouseAddress.OA_Phone_Formatted.IsEmpty)
					{
						if (!WhsDocket.Warehouse.WarehouseAddress.OA_Fax_Formatted.IsEmpty)
						{
							result = Res.GetString("0b725185-2021-44d1-88a5-037703e876be", "Tel: {0}   Fax: {1}", WhsDocket.Warehouse.WarehouseAddress.OA_Phone_Formatted, WhsDocket.Warehouse.WarehouseAddress.OA_Fax_Formatted);
						}
						else
						{
							result = Res.GetString("9eb0cb5e-38a6-4daa-9648-2c3fbc7d977f", "Tel: {0}", WhsDocket.Warehouse.WarehouseAddress.OA_Phone_Formatted);
						}
					}
					else
					{
						if (!WhsDocket.Warehouse.WarehouseAddress.OA_Fax_Formatted.IsEmpty)
						{
							result = Res.GetString("fb225f55-a530-4d8e-b637-bf443c9e53c5", "Fax: {0}", WhsDocket.Warehouse.WarehouseAddress.OA_Fax_Formatted);
						}
					}
				}

				return result;
			}
		}

		public ZString DocketID
		{
			get { return WhsDocket.WD_DocketID; }
		}

		public ZString DocketStatus
		{
			get { return WhsDocket.WD_DocketStatus; }
		}

		public ZString DocketType
		{
			get { return WhsDocket.WD_DocketType; }
		}

		public ZString DocketSubType
		{
			get { return WhsDocket.WD_DocketSubType; }
		}

		public ZString ExternalReferenceBarcode
		{
			get
			{
				TextBarcode barcode = new TextBarcode(this.ExternalReference);
				return barcode.TextAs128sFontString;
			}
		}

		public ZString ExternalReference
		{
			get { return WhsDocket.WD_ExternalReference; }
		}

		public ZString ServiceLevel
		{
			get { return WhsDocket.CarrierServiceLevel != null ? WhsDocket.CarrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual : ZString.Empty; }
		}

		public ZString TotalWeightUnit
		{
			get { return WhsDocket.WD_TotalWeightUnit; }
		}

		public ZString TotalPackagesUnit
		{
			get { return WhsDocket.WD_F3_NKTotalPackType; }
		}

		public ZString TotalCubicUnit
		{
			get { return WhsDocket.WD_TotalCubicUnit; }
		}

		public virtual ZString PickNo
		{
			get { return ZString.Empty; }
		}

		public virtual ZString SubTypeDesc
		{
			get { return WhsDocket.SubTypeDesc; }
		}

		public ZString StatusDesc
		{
			get { return WhsDocket.WD_DocketStatusDescription; }
		}

		public ZString References
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (WhsDocketReference reference in WhsDocket.References)
				{
					result += reference.WX_RefType + ": " + reference.WX_Reference;
					result += "\n";
				}
				return result.TrimEndIncludingWhiteSpace('\n');
			}
		}

		public ZString ReferencesExtended
		{
			get
			{
				ZString result = ZString.Empty;
				ICodeDescriptionPairListWithDefaultCode referenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;
				int referencesPrinted = 0;

				foreach (WhsDocketReference reference in WhsDocket.References)
				{
					result += referenceTypes.GetDescriptionFromCode(reference.WX_RefType);
					result += ": " + reference.WX_Reference + System.Environment.NewLine;

					referencesPrinted++;

					if (referencesPrinted >= 4)
					{
						break;
					}
				}

				return result.TrimEndIncludingWhiteSpace('\n');
			}
		}

		public ZString BillOfLadingNo
		{
			get { return WhsDocket.WD_BOLNo; }
		}

		public ZString VehicleNo => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.VehicleNumber);

		public ZString INCOTerm
		{
			get { return WhsDocket.Lookups.INCOTerms.GetDescriptionFromCode(WhsDocket.WD_INCO); }
		}

		public ZString ContainerNumberAndTypeLine
		{
			get { return WhsDocketContainerSupport.ContainerNumberAndType(DocketContainers.ToIDocSimpleContainerCollection()); }
		}

		public ZString DocumentName
		{
			get { return MenuTitle; }
		}

		public ZString EmailSubjectNumber
		{
			get { return WhsDocket.WD_ExternalReference; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal ShipperCODAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (WhsDocket.WD_INCO == "FCD")
				{
					result = WhsDocket.WD_ShipperCODAmount;
				}

				return result;
			}
		}

		public ZDecimal TotalCubic
		{
			get { return WhsDocket.WD_TotalCubic; }
		}

		public ZDecimal TotalWeight
		{
			get { return WhsDocket.WD_TotalWeight; }
		}

		public ZDecimal TotalUnits
		{
			get { return WhsDocket.WD_TotalUnits; }
		}

		public ZDecimal UnitsSent
		{
			get { return WhsDocket.WD_UnitsSent; }
		}

		public ZDecimal CubicSent
		{
			get { return WhsDocket.WD_CubicSent; }
		}

		public ZDecimal WeightSent
		{
			get { return WhsDocket.WD_WeightSent; }
		}

		#endregion

		#region ZInt Fields

		public ZInt TotalNumberOfLabels
		{
			get { return (DocketLabel != null) ? DocketLabel.NumberOfLabelsToPrint : ZInt.Zero; }
		}

		public ZInt PackagesSent
		{
			get { return WhsDocket.WD_PackagesSent; }
		}

		#endregion

		#region ZShort Fields

		public ZShort TotalPallets
		{
			get { return WhsDocket.WD_TotalPallets; }
		}

		public ZShort PalletsSent
		{
			get { return WhsDocket.WD_PalletsSent; }
		}

		#endregion

		#region ZBool Fields

		public ZBool PrintPageWithContainerNumber
		{
			get
			{
				return (DocketContainers.Count > MaximumContainersWithTypeOnALine && AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.Value);
			}
		}

		#endregion

		#region Wrapper Fields

		public DocBranch Branch
		{
			get
			{
				GlbBranch branch = WhsDocket.Warehouse != null ? WhsDocket.Warehouse.RelatedCompanyBranch : null;
				return DocBranch.New(branch, Factory);
			}
		}

		public DocOrganisation Client
		{
			get { return DocOrganisation.New(WhsDocket.Client, Factory); }
		}

		public DocOrganisation Forwarder
		{
			get { return DocOrganisation.New(WhsDocket.Forwarder, Factory); }
		}

		public DocOrganisation Supplier
		{
			get { return DocOrganisation.New(WhsDocket.Supplier, Factory); }
		}

		public DocOrganisation TransportCo => TransportCoCore;

		protected abstract DocOrganisation TransportCoCore { get; }

		public DocDocAddress TransportCoAddress => TransportCoAddressCore;

		protected abstract DocDocAddress TransportCoAddressCore { get; }

		public DocDocAddress TransportCoNameAndAddress
		{
			get { return TransportCoAddress; }
		}

		public DocDocAddress SupplierDocAddress
		{
			get { return DocDocAddress.New(WhsDocket.SupplierDocAddress, Factory); }
		}

		public DocDocAddress GoodsBillToAddress
		{
			get
			{
				return DocDocAddress.New(WhsDocket.GoodsBillToDocAddress, Factory);
			}
		}

		public DocOrganisation Consignee => ConsigneeCore;

		protected virtual DocOrganisation ConsigneeCore => null;

		public DocDocAddress ConsigneeAddress => ConsigneeAddressCore;

		protected virtual DocDocAddress ConsigneeAddressCore => null;

		public DocDocAddress ConsigneeNameAndAddress => ConsigneeNameAndAddressCore;

		protected virtual DocDocAddress ConsigneeNameAndAddressCore => null;

		public DocDocAddress WarehouseAddress
		{
			get
			{
				DocDocAddress result = null;
				if (WhsDocket.Warehouse != null)
				{
					result = DocDocAddress.New(WhsDocket.Warehouse.WarehouseAddress, Factory);
				}
				return result;
			}
		}

		public Image WarehouseCompanyLogo
		{
			get
			{
				Image result = null;

				if (WhsDocket.Warehouse != null)
				{
					if (WhsDocket.Warehouse.WarehouseAddress != null && WhsDocket.Warehouse.WarehouseAddress.Header != null && WhsDocket.Warehouse.WarehouseAddress.Header.MiscServ.ClientDocumentLogo.Length > 0)
					{
						try
						{
							MemoryStream stream = new MemoryStream(WhsDocket.Warehouse.WarehouseAddress.Header.MiscServ.ClientDocumentLogo);
							result = Image.FromStream(stream);
						}
						catch (Exception exception)
						{
							if (exception.IsCriticalException()) { throw; }
							result = null;
						}
					}
					if (result == null && WhsDocket.Warehouse.RelatedCompanyBranch != null)
					{
						result = SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(CurrentCompany.PK.ToGuid(), WhsDocket.Warehouse.RelatedCompanyBranch.PK.ToGuid(), Guid.Empty);
					}
				}
				if (result == null)
				{
					result = CompanyLogo;
				}

				return result;
			}
		}

		public Image CustomCompanyLogo
		{
			get
			{
				Image result = null;

				if (Client != null && Client.MiscServ.ClientDocumentLogo.Length > 0)
				{
					try
					{
						MemoryStream stream = new MemoryStream(Client.MiscServ.ClientDocumentLogo);
						result = Image.FromStream(stream);
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException()) { throw; }
						result = CompanyLogo;
					}
				}
				else
				{
					result = CompanyLogo;
				}

				return result;
			}
		}

		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return DocketID; }
		}

		#endregion

		#endregion

		#region IDocServicesParent Members

		#region ConsolNumber

		ZString IDocServicesParent.ConsolNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region GoodsDescription

		ZString IDocServicesParent.GoodsDescription
		{
			get { return WhsDocket.WD_GoodsDescription; }
		}

		#endregion

		#region Packages

		ZString IDocServicesParent.Packages
		{
			get { return GetPackages(); }
		}

		protected virtual ZString GetPackages()
		{
			return WhsDocket.WD_PackagesSent.ToString();
		}

		#endregion

		#region Weight

		ZString IDocServicesParent.Weight
		{
			get { return GetWeight(); }
		}

		protected virtual ZString GetWeight()
		{
			return WhsDocket.WD_TotalWeight.ToString();
		}

		#endregion

		#region Volume

		ZString IDocServicesParent.Volume
		{
			get { return GetVolume(); }
		}

		protected virtual ZString GetVolume()
		{
			return WhsDocket.WD_TotalCubic.ToString();
		}

		#endregion

		#region WeightUnit

		ZString IDocServicesParent.WeightUnit
		{
			get { return WhsDocket.WD_TotalWeightUnit; }
		}

		#endregion

		#region VolumeUnit

		ZString IDocServicesParent.VolumeUnit
		{
			get { return WhsDocket.WD_TotalCubicUnit; }
		}

		#endregion

		#region MasterBillNum

		ZString IDocServicesParent.MasterBillNum
		{
			get { return GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.MasterBill); }
		}

		#endregion

		#region MasterBillHeading

		ZString IDocServicesParent.MasterBillHeading
		{
			get { return WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode(WarehouseAdditionalReferenceTypes.Codes.MasterBill); }
		}

		#endregion

		#region HouseBill

		ZString IDocServicesParent.HouseBill
		{
			get { return GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.HouseBill); }
		}

		#endregion

		#region HouseBillHeading

		ZString IDocServicesParent.HouseBillHeading
		{
			get { return WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode(WarehouseAdditionalReferenceTypes.Codes.HouseBill); }
		}

		#endregion

		#region TransportInfo

		ZString IDocServicesParent.TransportInfo
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ETD

		ZDateTime IDocServicesParent.ETD
		{
			get { return WhsDocket.WD_ETD.ToZDateTime(); }
		}

		#endregion

		#region ETA

		ZDateTime IDocServicesParent.ETA
		{
			get { return WhsDocket.WD_ETA.ToZDateTime(); }
		}

		#endregion

		#region ContainerNumbers

		ZString IDocServicesParent.ContainerNumbers
		{
			get { return WhsDocket.ContainerID; }
		}

		#endregion

		#region Context

		ZString IDocServicesParent.Context
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PortOfLoading

		DocUNLOCO IDocServicesParent.PortOfLoading
		{
			get { return null; }
		}

		#endregion

		#region PortOfDischarge

		DocUNLOCO IDocServicesParent.PortOfDischarge
		{
			get { return null; }
		}

		#endregion

		#region OwnerRefAndOrderRef

		ZString IDocServicesParent.OwnerRefAndOrderRef
		{
			get { return WhsDocket.WD_ExternalReference; }
		}

		#endregion

		#region OwnerRefAndOrderRefHeading

		ZString IDocServicesParent.OwnerRefAndOrderRefHeading
		{
			get { return ZString.Empty; }
		}

		#endregion

		ZString GetReferenceValue(ZString referenceCode)
		{
			ZString result = ZString.Empty;

			if (WhsDocket != null)
			{
				var additionalReference = WhsDocket.References.Cast<WhsDocketReference>().FirstOrDefault(r => r.WX_RefType == referenceCode);
				result = additionalReference != null ? additionalReference.WX_Reference : ZString.Empty;
			}

			return result;
		}

		#endregion

		#region Implementation

		protected DocContainerCollectionHelper WhsDocketContainerSupport
		{
			get { return whsDocketContainerSupport ?? (whsDocketContainerSupport = new DocContainerCollectionHelper(MaximumContainersWithTypeOnALine, false)); }
		}
		DocContainerCollectionHelper whsDocketContainerSupport;

		const int MaximumContainersWithTypeOnALine = 5;

		protected WhsDocketLabelControl DocketLabel;

		#endregion
	}
}
