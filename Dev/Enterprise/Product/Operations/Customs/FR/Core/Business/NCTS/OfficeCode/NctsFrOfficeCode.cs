using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsFrOfficeCode : EU.NCTS.Business.NctsEuOfficeCode, Integration.Customs.FR.INctsEuOfficeCode
	{
		public NctsFrOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusCodeDataValidation GetNewPhase5Validation() => new NctsFrOfficeCodeValidation(this);

		protected override CusCodeDataValidation GetNewPhase4Validation() => new NctsFrOfficeCodeValidation(this);

		public new NctsHeader Header => Parent as NctsHeader;

		public new NctsDepartureMovementHeader MovementHeader => Parent as NctsDepartureMovementHeader;

		public new NctsArrivalMovementHeader ArrivalMovementHeader => Parent as NctsArrivalMovementHeader;

		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = base.CY_Code;
				base.CY_Code = value;
				if (!IsCopying && oldValue != value)
				{
					DefaultTHI();
				}
			}
		}

		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = base.CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != value)
				{
					DefaultTHI();
				}
			}
		}

		void DefaultTHI()
		{
			if (Header != null && !Header.IsPhase5)
			{
				var valueSetStrategy = new NctsHeaderPhase4ValueSetStrategy(Header);
				valueSetStrategy.DefaultTHI();
			}
		}
	}
}
