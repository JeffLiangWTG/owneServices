using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class VehiclesGridColumnBag
	{
		public static VehiclesGridColumnBag Instance => instance ??= new VehiclesGridColumnBag();

		[ThreadStatic]
		static VehiclesGridColumnBag instance;

		public VehiclesGridColumnBag()
		{
			VinTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusVehicle.Schema.CVH_VehicleIdentificationNumber, 60);
			BrandTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusVehicle.Schema.CVH_BrandName, 60);
			ModelTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusVehicle.Schema.CVH_ModelName, 60);
		}

		public IGridColumnReference VinTextBoxColumn { get; }
		public IGridColumnReference BrandTextBoxColumn { get; }
		public IGridColumnReference ModelTextBoxColumn { get; }
	}
}
