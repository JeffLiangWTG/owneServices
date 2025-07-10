using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PlaceOfUseOrProcessing : CusGoodsLocation
	{
		public PlaceOfUseOrProcessing(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusEntryInstruction Instruction => Factory.Load<CusEntryInstruction>(CGL_ParentID);

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new PlaceOfUseOrProcessingValidation(this);

		public new PlaceOfUseOrProcessingLookups Lookups => (PlaceOfUseOrProcessingLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups()
		{
			return new PlaceOfUseOrProcessingLookups(this);
		}

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.PlaceOfUseOrProcessing|DisplayText", Caption = "Goods Location")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.PlaceOfUseOrProcessing|IMPUCC6|DisplayText", Caption = "Goods Location", FullDescription = "[4/9] Place(s) of Use or Processing", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString DisplayText => base.DisplayText;

		public override ZString Unlocode
		{
			get
			{
				return CGL_CustomsOffice;
			}
			set
			{
				CGL_CustomsOffice = value;
			}
		}
		public new ZPropertyInfo UnlocodeInfo => GetWrappedZPropertyInfo(nameof(Unlocode), x => CGL_CustomsOfficeInfo);
	}
}
