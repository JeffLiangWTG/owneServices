using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupportingDocument : CusSupportingInfo
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string DocumentDescription = "DocumentDescription";
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);
		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CodeList))]
		[MaxLength(4)]
		[ResourceStringData("EUICS2.SupportingDocuments.CSI_Code", Caption = "Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code && !IsCopying)
				{
					documentDescriptionCache = null;
					DocumentDescriptionInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("EUICS2.SupportingDocuments.DocumentDescription", Caption = "Description")]
		public ZString DocumentDescription => CachedValueHelper.GetValue(ref documentDescriptionCache, GetSupportingDocumentDescription);

		CachedValue<ZString> documentDescriptionCache;

		public ZPropertyInfo DocumentDescriptionInfo => GetZPropertyInfo(Schema.DocumentDescription);
		ZString GetSupportingDocumentDescription()
		{
			var descrip = ZString.Empty;
			var code = CSI_Code;
			if (!CSI_Code.IsEmpty)
			{
				var query = ((BusinessObjectCollection)Lookups.CodeList).CompleteFilter;
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
				descrip = Factory.LoadTop1<ZZRefCusCodeListCombined>(query)?.ZZD_Description ?? ZString.Empty;
			}
			return descrip;
		}

		[MaxLength(70)]
		[ResourceStringData("EUICS2.SupportingDocuments.CSI_ReferenceNumber", Caption = "Number")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }
	}
}
