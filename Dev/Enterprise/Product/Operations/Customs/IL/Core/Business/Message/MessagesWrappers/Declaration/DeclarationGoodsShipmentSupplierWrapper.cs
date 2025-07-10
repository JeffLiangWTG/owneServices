using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business
{
	sealed class DeclarationGoodsShipmentSupplierWrapper : IDeclarationGoodsShipmentSupplier
	{
		DeclarationGoodsShipmentSupplierWrapper(OrgHeader supplier)
		{
			this.supplier = supplier;
		}
		readonly OrgHeader supplier;

		internal static DeclarationGoodsShipmentSupplierWrapper NewOrNull(OrgHeader supplier) => supplier == null ? null : new DeclarationGoodsShipmentSupplierWrapper(supplier);

		#region IDeclarationGoodsShipmentSupplier

		IIDType IDeclarationGoodsShipmentSupplier.ID => (supplier.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.SupplierCode, CountryCodes.Israel) is OrgCusCode orgCusCode && !orgCusCode.OK_CustomsRegNo.IsEmpty) ? IDTypeWrapper.NewOrNull(orgCusCode.OK_CustomsRegNo) : null;

		#endregion

	}
}
