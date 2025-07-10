using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.DeclarationH7;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class DeclarationH7HeaderWrapper(AsycudaBill bill) : IDeclarationH7Header
{
	public ZString SupervisingCustomsOffice => bill.Header.AMA_CustomsOffice;

	public ZDecimal GrossWeight =>
		Constants.Weight.Convert(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ, Constants.Weight.Kilograms);

	public IH7Representative Representative
	{
		get
		{
			if (bill.Header.Representative == null || bill.Header.AMA_AgentType == ESH7AgentTypes.Codes.SEL)
			{
				return null;
			}

			return new H7RepresentativeWrapper(bill);
		}
	}

	public IH7Declarant Declarant
	{
		get
		{
			if (bill.Header.Declarant == null)
			{
				return null;
			}

			return new H7DeclarantWrapper(bill);
		}
	}

	public IPartyProvider Exporter
	{
		get
		{
			if (bill.Shipper != null)
			{
				return PartyWrapper.New(bill.Shipper);
			}

			return new H7ExporterNonOrganizationWrapper(bill);
		}
	}

	public IH7Importer Importer => new H7ImporterWrapper(bill);

	public IReadOnlyCollection<IDocumentsCommon> SupportingDocuments
	{
		get
		{
			if (supportingDocuments == null)
			{
				var supDocsList = new List<DocumentCommonWrapper>();
				bill.SupportingDocuments.ForEach(doc => supDocsList.Add(new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber)));
				supportingDocuments = supDocsList.AsReadOnly();
			}

			return supportingDocuments;
		}
	}

	public IReadOnlyCollection<IDocumentsCommon> AdditionalReferences
	{
		get
		{
			if (additionalReferences == null)
			{
				var addRefList = new List<DocumentCommonWrapper>();

				var refDocs = bill.AdditionalDocuments.Where(doc => doc.IsAnAdditionalReference);
				refDocs.ForEach(doc => addRefList.Add(new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber)));

				additionalReferences = addRefList.AsReadOnly();
			}

			return additionalReferences;
		}
	}

	public IReadOnlyCollection<IDocumentsCommon> TransportDocuments
	{
		get
		{
			if (transportDocuments == null)
			{
				var addTransportList = new List<DocumentCommonWrapper>();

				var transportDocs = bill.AdditionalDocuments.Where(doc => doc.IsATransportDocument);
				transportDocs.ForEach(doc =>
				{
					addTransportList.Add(new DocumentCommonWrapper(TransportDocCode5025, bill.ABL_BillNumber));
					if (doc.CSI_Code == TransportDocCode5026)
					{
						addTransportList.Add(new DocumentCommonWrapper(TransportDocCode5026, doc.CSI_ReferenceNumber));
					}
				});

				if (addTransportList.IsNullOrEmpty())
				{
					addTransportList.Add(new DocumentCommonWrapper(TransportDocCode5025, bill.ABL_BillNumber));
				}

				transportDocuments = addTransportList.AsReadOnly();
			}

			return transportDocuments;
		}
	}

	public IReadOnlyCollection<IH7AdditionalInfo> AdditionalInformation
	{
		get
		{
			if (additionalInformation == null)
			{
				var addInfoList = new List<H7AdditionalInfoWrapper>();

				var infoDocs = bill.AdditionalDocuments.Where(doc => doc.IsAnAdditionalInformation);
				infoDocs.ForEach(doc => addInfoList.Add(new H7AdditionalInfoWrapper(doc)));

				additionalInformation = addInfoList.AsReadOnly();
			}

			return additionalInformation;
		}
	}

	public IH7Value TransportCostToDestination => new TransportCostWrapper(bill);

	public IReadOnlyCollection<IDocumentsCommon> PreviousDocuments
	{
		get
		{
			if (previousDocuments == null)
			{
				var preDocsList = new List<DocumentCommonWrapper>();
				bill.PreviousDocuments.ForEach(doc => preDocsList.Add(new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber)));
				previousDocuments = preDocsList.AsReadOnly();
			}

			return previousDocuments;
		}
	}

	public ZString ReferenceNumberUCR => bill.ABL_UCRNumber;

	public IReadOnlyCollection<IH7AdditionalFiscalRef> AdditionalFiscalRef
	{
		get
		{
			if (additionalFiscalRef == null)
			{
				var additionalFiscalRefList = new List<H7AdditionalFiscalRefWrapper> { new(bill) };
				additionalFiscalRef = additionalFiscalRefList.AsReadOnly();
			}

			return additionalFiscalRef;
		}
	}
	IReadOnlyCollection<IH7AdditionalFiscalRef> additionalFiscalRef;

	public IReadOnlyCollection<ZString> AdditionalProcedures
	{
		get
		{
			if (additionalProcedures == null)
			{
				var additionalProceduresList = bill.ABL_Procedure.Split("+").Where(p => !p.IsEmpty).ToList();
				additionalProcedures = additionalProceduresList.AsReadOnly();
			}

			return additionalProcedures;
		}
	}
	IReadOnlyCollection<ZString> additionalProcedures;

	public ZString GoodsLocation
	{
		get
		{
			if (!bill.G3MovementReferenceNumber.IsEmpty || bill.PreviousDocuments.Any(d => d.CSI_Code == PreviousDocumentTypeCode337))
			{
				if (bill.CusGoodsLocation.Address.AuthorisationNumber.IsEmpty)
				{
					return bill.Header.CusGoodsLocation.Address.AuthorisationNumber;
				}
				else
				{
					return bill.CusGoodsLocation.Address.AuthorisationNumber;
				}
			}

			return string.Empty;
		}
	}

	const string TransportDocCode5025 = "5025";
	const string TransportDocCode5026 = "5026";
	const string PreviousDocumentTypeCode337 = "337";

	IReadOnlyCollection<DocumentCommonWrapper> supportingDocuments;
	IReadOnlyCollection<DocumentCommonWrapper> previousDocuments;
	IReadOnlyCollection<DocumentCommonWrapper> additionalReferences;
	IReadOnlyCollection<DocumentCommonWrapper> transportDocuments;
	IReadOnlyCollection<H7AdditionalInfoWrapper> additionalInformation;
}
