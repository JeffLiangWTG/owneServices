using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobComInvoiceHeader ParentAsInvoiceHeader => Parent as JobComInvoiceHeader;

		public JobComInvoiceLine ParentAsInvoiceLine => Parent as JobComInvoiceLine;

		[ResourceStringData("F96D6C79-D3BD-43DB-9A8F-872606273910", Caption = "CCI Validate Country", MediumCaption = "CCI Valid. Country", ShortCaption = "CCI Country", FullDescription = "Only used in CCI. Identify the country who will validate the data")]
		public override ZString CSI_RN_NKCountryCode { get => base.CSI_RN_NKCountryCode; set => base.CSI_RN_NKCountryCode = value; }

		[ResourceStringData("ESAdditionalInfo|CSI_NctsExportFromEC", Caption = "Is EU Code", MediumCaption = "Is EU", ShortCaption = "Is EU")]
		public override ZBool CSI_NctsExportFromEC { get => base.CSI_NctsExportFromEC; set => base.CSI_NctsExportFromEC = value; }

		[ResourceStringData("ESAddInfoAdditionalInfo|CSI_SubType", Caption = "Kind", MediumCaption = "Kind", ShortCaption = "Kind")]
		[ResourceStringData("ESAddInfoAdditionalInfo|CSI_SubType|INF", Caption = "Kind", MediumCaption = "Kind", ShortCaption = "Kind", FullDescription = "[12 02 000 000] Additional Information", IsApplicableMember = nameof(IsAnAdditionalInformation))]
		[ResourceStringData("ESAddInfoAdditionalInfo|CSI_SubType|REF", Caption = "Kind", MediumCaption = "Kind", ShortCaption = "Kind", FullDescription = "[12 04 000 000] Additional Reference", IsApplicableMember = nameof(IsAnAdditionalReference))]
		[ResourceStringData("ESAddInfoAdditionalInfo|CSI_SubType|TRA", Caption = "Kind", MediumCaption = "Kind", ShortCaption = "Kind", FullDescription = "[12 05 000 000] Transport Document", IsApplicableMember = nameof(IsATransportDocument))]

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.KindList))]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => GetNewLookupsInternal();

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

		protected override ZString DefaultStatus => ZString.Empty;

		public static AdditionalInfo CopyFrom(ReadOnlyAdditionalInfo readOnlyAddInf)
		{
			Argument.NotNull(readOnlyAddInf, nameof(readOnlyAddInf));
			var document = readOnlyAddInf.Factory.New<AdditionalInfo>();
			document.CSI_Code = readOnlyAddInf.CSI_Code;
			document.CSI_Description = readOnlyAddInf.CSI_Description;
			document.CSI_SubType = readOnlyAddInf.CSI_SubType;
			document.CSI_ReferenceNumber = readOnlyAddInf.CSI_ReferenceNumber;
			document.CSI_ReferenceNumber2 = readOnlyAddInf.CSI_ReferenceNumber2;
			document.CSI_RX_NKCurrency = readOnlyAddInf.CSI_RX_NKCurrency;
			document.CSI_Value = readOnlyAddInf.CSI_Value;
			document.CSI_DataModel = readOnlyAddInf.CSI_DataModel;
			return document;
		}

		#region Implementation

		CusSupportingInfoLookups GetNewLookupsInternal()
		{
			return IsUcc6 || IsImport
				? new Ucc6AdditionalInfoLookup(this)
				: new AdditionalInfoLookups(this);
		}

		bool IsUcc6 => (Parent as IUcc6ValueProvider)?.IsUCC6 ?? false;

		bool IsImport => (Parent as IUcc6ValueProvider)?.IsImport ?? false;

		#endregion

	}
}
