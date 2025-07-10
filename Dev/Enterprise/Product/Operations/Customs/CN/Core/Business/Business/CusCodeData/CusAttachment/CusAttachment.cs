using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CusAttachment : CusCodeData
	{
		public CusAttachment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = Constants.CusCodeDataTypes.Codes.CusAttachment;
		}

		protected override CusCodeDataValidation GetNewValidation() => new CusAttachmentValidation(this);

		static ZQuery GetZQuery(ZGuid parentPK, ZString attachmentType, ZString attachmentTypeNumber)
		{
			var query = new ZQuery(CusCodeDataSchema.CY_Type, Constants.CusCodeDataTypes.Codes.CusAttachment);
			query.AddToFilter(CusCodeDataSchema.CY_ParentID, parentPK);
			query.AddToFilter(CusCodeDataSchema.CY_Code, attachmentType);
			query.AddToFilter(CusCodeDataSchema.CY_Data, attachmentTypeNumber);
			return query;
		}

		public static bool Exists(CusEntryInstruction parent, ZString attachmentType, ZString attachmentTypeNumber)
		{
			Argument.NotNull(parent, nameof(parent));

			return parent.Factory.Exists(typeof(CusAttachment), GetZQuery(parent.PK, attachmentType, attachmentTypeNumber));
		}

		public static CusAttachment AddNew(CusEntryInstruction parent, ZString attachmentType, ZString attachmentTypeNumber)
		{
			Argument.NotNull(parent, nameof(parent));

			var newAttachment = parent.Factory.New<CusAttachment>();
			newAttachment.Parent = parent;
			newAttachment.CY_Code = attachmentType;
			newAttachment.CY_Data = attachmentTypeNumber;
			return newAttachment;
		}
	}
}
