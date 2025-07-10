using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class OwnerOfGoodsCollection : DependentBusinessObjectCollection<OwnerOfGoods, CusEntryInstruction>
	{
		const int MaxCountForValidation = 98;

		public OwnerOfGoodsCollection(CusEntryInstruction master) : base(master, new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.OwnerOfGoods))
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(OwnerOfGoods);

		protected override SchemaGuidColumn FKSchemaColumnInDependent => JobDocAddressSchema.E2_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var ownerOfGoods = (OwnerOfGoods)child;
			ownerOfGoods.E2_AddressType = DocAddressTypes.Codes.OwnerOfGoods;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			var ownerOfGoods = bizO as OwnerOfGoods;
			ownerOfGoods?.Instruction?.SequenceGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(ownerOfGoods);
		}
	}
}
