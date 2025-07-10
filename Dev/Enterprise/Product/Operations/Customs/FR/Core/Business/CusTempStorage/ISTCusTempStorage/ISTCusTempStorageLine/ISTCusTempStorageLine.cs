using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(ISTCusTempStorageDec), "CusTempStorageLines")]

	public class ISTCusTempStorageLine : CusTempStorageLine
	{
		public ISTCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Dec

		public new ISTCusTempStorageDec Dec => Factory.Load<ISTCusTempStorageDec>(TSL_STH);

		public ZWeight EffectiveGrossWeight
		{
			get
			{
				ZString grossWeightUQ = Core.Constants.Weight.ContainsCode(TSL_GrossWeightUQ) ? TSL_GrossWeightUQ : (ZString)Core.Constants.Weight.Kilograms;
				return new ZWeight(TSL_GrossWeight, grossWeightUQ);
			}
		}

		#endregion

		public new ISTCusTempStorageLineValidation Validation => (ISTCusTempStorageLineValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation()
		{
			return new ISTCusTempStorageLineValidation(this);
		}
	}
}
