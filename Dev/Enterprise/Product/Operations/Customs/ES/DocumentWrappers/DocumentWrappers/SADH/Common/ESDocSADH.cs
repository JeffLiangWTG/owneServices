using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using ESCusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public abstract class ESDocSADH : DocSADH
	{
		protected ESDocSADH(ESCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{
			InvoiceHeader = entryHeader.RandomHeader;
		}

		protected readonly JobComInvoiceHeader InvoiceHeader;

		public new ESCusEntryHeader EntryHeader => (ESCusEntryHeader)base.EntryHeader;

		public new JobDeclaration Declaration => EntryHeader.Declaration;

		protected const string NoDecimalsFormat = "N0";
		const string DateFormatSpain = "dd-MM-yyyy";

		protected override ZBool ShowBox2SupplierCountryCodeCore => false;

		protected override ZBool ShowBox8ImporterCountryCodeCore => false;

		protected override ZString Box6TotalNoOfPacksCore() => (EntryHeader.PackagesCount).ToString(NoDecimalsFormat);

		protected override ZString Box18IdentityOfTransportAtDepartureCore => Declaration.ZG_Box18TransportID;

		protected override ZString Box20ShipmentIncoTermCore => !InvoiceHeader.JZ_IncoTerm.IsEmpty ? InvoiceHeader.JZ_IncoTerm : Declaration.JE_ShipmentIncoTerm;
		protected override ZString Box20AgreedPlaceCore => !InvoiceHeader.JZ_IncoTermPlace.IsEmpty ? InvoiceHeader.JZ_IncoTermPlace : Declaration.JE_ShipmentIncoTermPlace;

		protected override ZString Box30LocationOfGoodsCore
		{
			get
			{
				var authorisationNumber =  EntryHeader.EntryInstruction?.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty;
				return authorisationNumber.IsEmpty ? base.Box30LocationOfGoodsCore : authorisationNumber;
			}
		}

		public ZBool IsEntryAcceptedByCustoms => !EntryHeader.MovementReferenceNumber.IsEmpty;

		protected override ZString BoxDSignatureCore => IsEntryAcceptedByCustoms ? (NoResString)"AUTENTICACION INFORMATICA, ART. 98 R/UE 952/2013" : string.Empty;

		protected override ZString Box54PlaceCore
		{
			get
			{
				var customsOffice = Declaration.JE_CustomsOffice;
				return Factory.GetCachedValue("Box54Place.CustomsOffice_" + customsOffice, () =>
				{
					var result = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Declaration.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, customsOffice, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.City);
					return result.Any() ? result.First() : ZString.Empty;
				});
			}
		}

		protected override ZString Box54NameOfDeclarantAndRepresentativeCore
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration.Declarant != null)
				{
					result = Declaration.Declarant.Header.OH_FullName;
				}
				else if (GetBox54OrgHeader != null)
				{
					result = GetBox54OrgHeader.OH_FullName;
				}
				return result;
			}
		}

		protected abstract OrgHeader GetBox54OrgHeader { get; }

		protected override GlbStaff GetStaffForBox54Details() => Declaration.CusAgent ?? base.GetStaffForBox54Details();

		protected override ZString Box54SignatoryContactDetailsCore
		{
			get
			{
				var cusAgentNif = staffForBox54Details?.Certificates.Find(c => c.XZ_Type == StaffCertificateType.NID
																			&& (c.XZ_ExpiryOrDueDate >= ZDateTime.Today || c.XZ_ExpiryOrDueDate.IsEmpty))
																	.FirstOrDefault()?.XZ_RefNumber ?? ZString.Empty;
				if (cusAgentNif.IsEmpty)
				{
					return base.Box54SignatoryContactDetailsCore;
				}
				else
				{
					return "NIF:" + cusAgentNif;
				}
			}
		}

		protected override ZString FormatDateDefault(ZDateTime date) => date.ToString(DateFormatSpain);

		protected override ZString BoxABarcodeCore
		{
			get
			{
				var boxAcode = !EntryHeader.EntryNumber.IsEmpty ? EntryHeader.EntryNumber : EntryHeader.MovementReferenceNumber;
				return boxAcode.Length > 0 ? "*" + boxAcode + "*" : "";
			}
		}
	}
}
