using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SealContainer : NctsContainer
	{
		public SealContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("8338502D-F23B-42C1-8919-D9FF8BEB746C", Caption = "Seal Number")]
		public override ZString BC_Seal1 { get => base.BC_Seal1; set => base.BC_Seal1 = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BC_TypeOfService = ContainerTypeOfServiceList.Codes.Seal;
		}
	}
}

