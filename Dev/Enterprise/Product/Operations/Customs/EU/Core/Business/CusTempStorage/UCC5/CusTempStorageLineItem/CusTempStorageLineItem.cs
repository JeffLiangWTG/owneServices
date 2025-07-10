using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageLine), "CusTempStorageLineItems")]
	public class CusTempStorageLineItem : AutoCusTempStorageLineItem
		, Integration.Customs.EU.ICusTempStorageLineItem
	{
		public CusTempStorageLineItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly CusTempStorageLineItemTypeDecider TypeDecider = new CusTempStorageLineItemTypeDecider();

		#endregion

		[RelatedBusinessObject("Line")]
		public override ZGuid TSI_TSL
		{
			get => base.TSI_TSL;
			set => base.TSI_TSL = value;
		}

		public virtual CusTempStorageLine Line => Factory.Load<CusTempStorageLine>(TSI_TSL);
	}
}
