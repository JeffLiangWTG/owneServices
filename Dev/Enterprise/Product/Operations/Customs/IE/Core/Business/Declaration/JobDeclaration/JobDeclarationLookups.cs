using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public override IBusinessObjectCollection LocationOfGoodsCollection
		{
			get
			{
				var result = new RefUNLOCOCollection(Factory, new ZQuery(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JE_LocationOfGoods)); //This is a default for filter business object for which schema does not exist
				return result;
			}
		}

		public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PaymentMethodList>();

		public override ICodeDescriptionPairList DeclarantTypeList => Factory.GetCachedValue("IE.JobDeclarationLookups.DeclarantTypeList", () =>
		{
			var result = new EU.Business.RepresentationTypeList();
			result.RemoveCode(EU.Business.RepresentationTypeList.Codes._1Self);
			return result;
		});

		public CodeDescriptionPairList LocationTypeList =>
			RefCusCodeListTypes.GetCachedList(
				Parent.Factory,
				Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType,
				ZDateTime.Today,
				includeParentDataGrouping: false
			);

		public virtual CodeDescriptionPairList LocationQualifierList =>
			RefCusCodeListTypes.GetCachedList(
				Parent.Factory,
				Core.Constants.CountryCodes.Ireland,
				Constants.RefCusCodeListTypes.IrelandQualifierType,
				ZDateTime.Today,
				includeParentDataGrouping: false
			);

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;
	}
}
