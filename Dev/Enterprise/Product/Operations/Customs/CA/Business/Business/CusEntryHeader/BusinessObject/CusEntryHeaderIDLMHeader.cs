using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	partial class CusEntryHeader : IDLMHeader
	{
		#region IMessageAttachee Members

		ZString IMessageAttachee.MessageType
		{
			get { return CH_MessageType; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return CH_Status; }
			set { CH_Status = value; }
		}

		EDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return Factory; }
		}

		#endregion

		#region IDLMHeader Members

		public IDLMOrganisation DLMExporter
		{
			get { return OrgWrapper.New((Declaration == null) ? null : Declaration.Supplier); }
		}

		public IDLMOrganisation DLMConsignee
		{
			get { return OrgWrapper.New((Declaration == null) ? null : Declaration.Importer); }
		}

		public IDLMOrganisation DLMServiceProvider
		{
			get { return OrgWrapper.New((Declaration == null) ? null : Declaration.Forwarder); }
		}

		public IDLMOrganisation DLMCertifier
		{
			get { return OrgWrapper.New(CompanyOrgProxy); }
		}

		ZString IDLMHeader.CertifierName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		OrgHeader CompanyOrgProxy
		{
			get { return Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK); }
		}

		public ZString CertifierStatus
		{
			get { return (Declaration != null && CompanyOrgProxy == Declaration.Supplier) ? "1" : "2"; }
		}

		public ZBool IsG7MessageStatusClearOriginalOrNotCleared => CH_Status == MessageStatusList.Codes.ClearOriginal || (!IsG7MessageStatusClearDelete && !IsG7MessageStatusClearAmendmentOrChange);
		public ZBool IsG7MessageStatusClearDelete => CH_Status == MessageStatusList.Codes.ClearDelete;
		public ZBool IsG7MessageStatusClearAmendmentOrChange => CH_Status == MessageStatusList.Codes.ClearReplace || CH_Status == MessageStatusList.Codes.ClearChange;

		ZDecimal IDLMHeader.CommodityGrossWeight
		{
			get { return (Declaration == null) ? ZDecimal.Zero : Declaration.JE_TotalWeight; }
		}

		ZString IDLMHeader.CommodityGrossWeightUnitOfMeasure
		{
			get { return (Declaration == null) ? "" : CanadianUnitOfWeightList.GetUnitAndDescription(Declaration.JE_TotalWeightUnit) ?? ""; }
		}

		public ZDecimal FreightCharges
		{
			get
			{
				if (freightChargesCached == null)
				{
					freightChargesCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0.00m;
						foreach (CusEntryLine entryLine in MergedLines)
						{
							result += entryLine.OverseasFreightInLocalCurrency.Amount;
						}
						return result;
					});
				}
				return freightChargesCached.Value;
			}
		}
		CachedProperty<ZDecimal> freightChargesCached;

		ZString IDLMHeader.CommodityCurrencyOfDeclaredValue
		{
			get { return (Declaration == null || Declaration.DeclaredCurrency == null) ? ZString.Empty : Declaration.DeclaredCurrency.RX_DescMultilingual; } //TODO: don't look at randomheader if we introduce currency on declaration level.
		}

		public ZDecimal TotalValueFOBPointOfExit
		{
			get
			{
				ZDecimal value = ZDecimal.Zero;
				foreach (IDLMDetailLine line in MergedLines)
				{
					value += line.ValueFOBPointOfExit;
				}
				return value;
			}
		}

		ZString IDLMHeader.ModeOfTransport
		{
			get { return (Declaration == null) ? "" : Declaration.Lookups.DLMTransportTypeList.GetDescriptionFromCode(Declaration.JE_TransportMode) ?? ""; }
		}

		public ZString ReasonForExport
		{
			get { return (Declaration == null) ? "" : Declaration.AddInfoLookups.ReasonForExportCodes.GetDescriptionFromCode(Declaration.CA_ReasonForExportCode) ?? ""; }
		}

		public ZString VesselName
		{
			get { return (Declaration == null || !Declaration.IsSea) ? ZString.Empty : Declaration.JE_VesselName; }
		}

		public ZString CountryOfFinalDestination
		{
			get
			{
				return (Declaration == null ||
					Declaration.FinalDestination == null ||
					Declaration.FinalDestination.Country == null) ? ZString.Empty : Declaration.FinalDestination.Country.RN_DescMultilingual;
			}
		}

		ZDateTime IDLMHeader.DateOfExportation
		{
			get { return (Declaration == null) ? ZDateTime.Empty : Declaration.JE_ExportDate; }
		}

		ZString IDLMHeader.PortOfExit
		{
			get { return (Declaration == null) ? ZString.Empty : Declaration.GetDescriptionFromPortOfficeCode(Declaration.CA_PortOfExit); }
		}

		ZString IDLMHeader.PlaceOfReport
		{
			get { return (Declaration == null) ? ZString.Empty : Declaration.GetDescriptionFromPortOfficeCode(Declaration.CA_PlaceOfReport); }
		}

		ZInt IDLMHeader.NumberOfPackages
		{
			get { return (Declaration == null) ? ZInt.Zero : Declaration.JE_TotalNoOfPacks; }
		}

		public ZString KindOfPackages
		{
			get { return (Declaration == null) ? "" : Declaration.Lookups.JE_TotalNoOfPacksPackType_List.GetDescriptionFromCode(Declaration.JE_TotalNoOfPacksPackType) ?? ""; }
		}

		ZString IDLMHeader.NameOfExportingCompany
		{
			get { return (Declaration != null && Declaration.ExportingCarrier != null) ? Declaration.ExportingCarrier.OH_FullNameTruncated : ZString.Empty; }
		}

		ZString IDLMHeader.TransportationDocumentNumber
		{
			get { return (Declaration == null) ? ZString.Empty : Declaration.CA_TransportDocumentNumber; }
		}

		IEnumerable<IDLMDetailLine> IDLMHeader.Details
		{
			get
			{
				foreach (IDLMDetailLine line in MergedLines)
				{
					yield return line;
				}
			}
		}

		public ZString[] DLMPermits
		{
			get
			{
				if (permitsCached == null)
				{
					permitsCached = new CachedProperty<ZString[]>(Factory, delegate
					{
						List<ZString> result = new List<ZString>();
						if (Declaration != null)
						{
							foreach (DeclarationExportPermit permit in Declaration.Permits)
							{
								if (!permit.CY_Data.IsEmpty)
								{
									result.Add(permit.CY_Data);
								}
							}
						}
						return result.ToArray();
					});
				}
				return permitsCached.Value;
			}
		}
		CachedProperty<ZString[]> permitsCached;

		ZString[] IDLMHeader.DLMContainers
		{
			get
			{
				if (containersCached == null)
				{
					containersCached = new CachedProperty<ZString[]>(Factory, delegate
					{
						List<ZString> result = new List<ZString>();
						foreach (CusContainer container in Containers)
						{
							result.Add(container.CO_ContainerNumber);
						}
						return result.ToArray();
					});
				}
				return containersCached.Value;
			}
		}
		CachedProperty<ZString[]> containersCached;

		ZString[] IDLMHeader.DLMReferences
		{
			get
			{
				if (referencesCached == null)
				{
					referencesCached = new CachedProperty<ZString[]>(Factory, delegate
					{
						List<ZString> result = new List<ZString>();
						foreach (JobComInvoiceHeader invoice in InvoiceHeaders)
						{
							result.Add(invoice.JZ_InvoiceNumber);
						}
						return result.ToArray();
					});
				}
				return referencesCached.Value;
			}
		}
		CachedProperty<ZString[]> referencesCached;
		#endregion
	}
}
