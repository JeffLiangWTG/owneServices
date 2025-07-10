using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CIQRequiredDocumentCollection : DependentCusAddInfoCollection<CIQRequiredDocument, BusinessObject>
	{
		public CIQRequiredDocumentCollection(BusinessObject master) : base(master, CusAddInfoTypeAttribute.Codes.CIQRequiredDocument)
		{
		}

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent => CusAddInfoSchema.B7_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var document = (CIQRequiredDocument)child;
			document.XC_NumberOfOriginals = 1;
			document.XC_NumberOfCopies = 2;
		}
	}
}
