using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStoragePackedItem : EU.Business.CusTempStorage.TemporaryStoragePackedItem
	{
		public TemporaryStoragePackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}

		#region Overrides

		[ReadOnly(true)]
		public override ZString API_GrossWeightUQ
		{
			get => base.API_GrossWeightUQ;
			set => base.API_GrossWeightUQ = value;
		}

		#endregion
	}
}
