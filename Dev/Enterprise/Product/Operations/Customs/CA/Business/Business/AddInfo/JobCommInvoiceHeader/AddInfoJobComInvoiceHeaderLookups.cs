using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobComInvoiceHeaderLookups : CAAddInfoLookups
	{
		public AddInfoJobComInvoiceHeaderLookups(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceHeader Parent
		{
			get { return (AddInfoJobComInvoiceHeader)base.Parent; }
		}

		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (invoiceHeader == null)
				{
					invoiceHeader = Parent.Parent;
				}
				return invoiceHeader;
			}
		}
		JobComInvoiceHeader invoiceHeader;

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public CodeDescriptionPairList StatesOfExport
		{
			get { return Factory.GetCachedValue<USStatesList>(); }
		}

		public CodeDescriptionPairList StatesOfOrigin
		{
			get { return StatesOfOriginBase(InvoiceHeader.IsExport, InvoiceHeader.JZ_RN_NKDefaultOrigin); }
		}

		public TimeLimitUnitCodes TimeLimitUnits
		{
			get { return Factory.GetCachedValue<TimeLimitUnitCodes>(); }
		}

		public new CodeDescriptionPairList TreatmentCodes
		{
			get
			{
				return LookupsHelper.TreatmentCodesByOriginAndExport(Factory, InvoiceHeader.JZ_RN_NKDefaultOrigin, InvoiceHeader.CA_RN_NKExport, InvoiceHeader.CA_TradeZone, InvoiceHeader.JobDeclaration?.EffectiveDutyDate);
			}
		}

		public ZZRefCarrierCombinedCollection CarrierCodes => ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(Factory, Parent.Parent.JobDeclaration?.JE_TransportMode ?? ZString.Empty);

		#region Testing
#if DEBUG
		public override RefUNLOCOCollection LastPorts
		{
			get
			{
				if (ZArchitecture.Environment.Globals.IsTest)
				{
					return new RefUNLOCOCollection(Factory, new ZQuery(ZArchitecture.Schema.RefUNLOCOSchema.RL_Code, "AUBNE"));
				}
				return base.LastPorts;
			}
		}
#endif
		#endregion
	}
}
