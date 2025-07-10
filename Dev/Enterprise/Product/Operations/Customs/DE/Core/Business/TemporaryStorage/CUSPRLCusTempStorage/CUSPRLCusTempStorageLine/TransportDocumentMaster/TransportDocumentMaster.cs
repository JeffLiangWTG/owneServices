using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class TransportDocumentMaster : CusSupportingInfo
	{
		public TransportDocumentMaster(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("CBA8A350-088E-4A12-A2E1-57998BBE0A87", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(TransportDocumentMasterLookups.TransportNumberTypeList))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		internal static TransportDocumentMaster LoadOrCreate(BusinessObject parent) => Load(parent) ?? New(parent);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
			CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new TransportDocumentMasterValidation(this);

		public new TransportDocumentMasterLookups Lookups => (TransportDocumentMasterLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new TransportDocumentMasterLookups(this);

		protected override ZString HumanReadableNameCore => Res.GetString("C7917F23-C918-4F6E-9FF2-EC5BACD937BB", "Transport Document Master");

		static TransportDocumentMaster Load(BusinessObject parent)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, parent.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, parent.TablePrefix);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.AdditionalInfo);
			query.AddToFilter(CusSupportingInfoSchema.CSI_SubType, AdditionalDocTypeList.Codes.TransportDocuments);
			query.OrderBy = CusSupportingInfoSchema.CSI_SystemCreateTimeUtc.Name;
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			return parent.Factory.LoadTop1<TransportDocumentMaster>(query);
		}

		static TransportDocumentMaster New(BusinessObject parent)
		{
			var result = parent.Factory.New<TransportDocumentMaster>();
			using (result.SuspendSettingHasChanges())
			{
				result.CSI_ParentTableCode = parent.TablePrefix;
				result.CSI_ParentID = parent.PK;
			}
			return result;
		}
	}
}
