using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ColsCusStorageDocPivotValidation : Customs.Business.CusStorageDocPivotValidation
	{
		public ColsCusStorageDocPivotValidation(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		protected override void CheckCSD_StorageDocReference()
		{
			base.CheckCSD_StorageDocReference();
			ListValidation.ErrorIfInvalidPK(Parent.CSD_StorageDocReferenceInfo);
			CheckDuplicateReference();
		}

		void CheckDuplicateReference()
		{
			var colsHeader = Parent.Parent as QuarantineColsHeader;
			if (colsHeader != null && colsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Any(x => x.PK != Parent.PK && x.CSD_StorageDocReference == Parent.CSD_StorageDocReference))
			{
				Parent.CSD_StorageDocReferenceInfo.AddError(Res.GetString("6C328247-36BA-46BD-847C-505C6B2C54CC", "EDoc should be unique."));
			}
		}

		protected override void CheckCSD_DocType()
		{
			MandatoryValidation.CheckEntered(Parent.CSD_DocTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.CSD_DocTypeInfo);
		}

		protected override void CheckCSD_Description()
		{
		}
	}
}
