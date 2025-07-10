using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument, IDataGroupingProvider
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string DocumentDescription = nameof(PreviousDocument.DocumentDescription);
		}

		public ZString DataGrouping
		{
			get
			{
				if (!dataGroupingCached.HasValue)
				{
					dataGroupingCached = Parent is IDataGroupingProvider provider ? provider.DataGrouping : ZString.Empty;
				}
				return dataGroupingCached.Value;
			}
		}
		ZString? dataGroupingCached;

		public override ZGuid CSI_ParentID
		{
			get => base.CSI_ParentID;
			set
			{
				var oldValue = CSI_ParentID;
				base.CSI_ParentID = value;
				if (!IsCopying && oldValue != CSI_ParentID)
				{
					dataGroupingCached = null;
				}
			}
		}

		public ValidationConfiguration ValidationConfiguration => validationConfiguration ??= GetNewValidationConfiguration();
		ValidationConfiguration validationConfiguration;

		protected virtual ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CodeList))]
		[ResourceStringData("EUH7.PreviousDocuments.CSI_Code", Caption = "Type", MediumCaption = "Type", ShortCaption = "Type", FullDescription = "Previous document type.")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(70)]
		[ResourceStringData("EUH7.PreviousDocuments.CSI_ReferenceNumber", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Ref. No.", FullDescription = "Previous document reference number.")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[ResourceStringData("EUH7.PreviousDocuments.DocumentDescription", Caption = "Description", MediumCaption = "Description", ShortCaption = "Desc.", FullDescription = "Previous document type description.")]
		public ZString DocumentDescription => Factory.GetValue(ref documentDescriptionCache, GetPreviousDocumentDescription);
		CachedProperty<ZString> documentDescriptionCache;

		public ZPropertyInfo DocumentDescriptionInfo => GetZPropertyInfo(Schema.DocumentDescription);

		protected virtual ZString GetPreviousDocumentDescription()
		{
			var result = ZString.Empty;
			var code = CSI_Code;
			if (!code.IsEmpty)
			{
				var query = ((BusinessObjectCollection)Lookups.CodeList).CompleteFilter;
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
				query.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_StartDate + OrderByClause.Descending + ","
					+ ZZRefCusCodeListCombinedSchema.Constants.ZZD_EndDate + OrderByClause.Descending;
				result = Factory.LoadTop1<ZZRefCusCodeListCombined>(query)?.ZZD_Description ?? ZString.Empty;
			}

			return result;
		}
	}
}
