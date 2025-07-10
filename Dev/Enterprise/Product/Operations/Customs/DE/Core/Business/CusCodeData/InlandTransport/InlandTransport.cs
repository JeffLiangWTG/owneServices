using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class InlandTransport : EU.Business.Declaration.InlandTransport
	{
		public InlandTransport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("09819E33-0D85-4CC7-B9BB-7CE6A647D1CC", Caption = "Wagon Number")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override ZString HumanReadableNameCore => Res.GetString("9D46199A-9553-4D77-95B4-DAD14BB575C3", "Wagon Number");
	}
}
