using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ES.NCTS.Business
{
	[SystemDefinedValues]
	public class Seal : EU.NCTS.Business.Seal
	{
		public Seal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsHeader));

		public new class Schema : CusCodeData.Schema
		{
			public const string IsBroken = "IsBroken";
		}
		[ResourceStringData("Enterprise.Customs.ES.Business.IsBroken", Caption = "Is Broken")]
		public ZBool IsBroken
		{
			get => this.GetSystemDefinedValue<ZBool>(GenAddOnHelper.IsBroken);
			set
			{
				var oldValue = IsBroken;
				this.SetSystemDefinedValue(GenAddOnHelper.IsBroken, value);
				IsBrokenInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo IsBrokenInfo => GetZPropertyInfo(Schema.IsBroken);
	}
}
