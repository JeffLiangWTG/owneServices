using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocumentMetaData : CusCodeData
	{
		public SupportingDocumentMetaData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public new SupportingDocumentMetaDataLookups Lookups => (SupportingDocumentMetaDataLookups)base.Lookups;
		protected override CusCodeDataLookups GetNewLookups() => new SupportingDocumentMetaDataLookups(this);

		public new SupportingDocumentMetaDataValidation Validation => (SupportingDocumentMetaDataValidation)base.Validation;
		protected override CusCodeDataValidation GetNewValidation() => new SupportingDocumentMetaDataValidation(this);

		[ResourceStringData("Enterprise.Customs.IL.Business.SupportingDocumentMetaData.CY_Code", Caption = "Code")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				base.CY_Code = value;
				MandatoryInfo.RefreshBinding();
			}
		}

		public ZBool Mandatory
		{
			get
			{
				var manandatoryMetaDatas = Factory.GetCachedLoadTop1ByCountryAndAttributes(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILDocumentType, Parent.CSI_Code)?
					.GetAttributesValues(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MetadataMandatory) ?? Array.Empty<ZString>();
				return manandatoryMetaDatas.Contains(CY_Code);
			}
		}

		public virtual ZPropertyInfo MandatoryInfo
		{
			[DebuggerStepThrough]
			get
			{
				return GetZPropertyInfo(nameof(Mandatory));
			}
		}

		[ResourceStringData("Enterprise.Customs.IL.Business.SupportingDocumentMetaData.CY_Data", Caption = "Value")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(SupportingDocument));
	}
}
