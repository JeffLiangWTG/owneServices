using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureHeaderContainer : EU.NCTS.Business.NctsDepartureHeaderContainer, Integration.Customs.ES.INctsDepartureHeaderContainer
	{
		public NctsDepartureHeaderContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString BC_Mode
		{
			get => base.BC_Mode;
			set
			{
				var oldValue = base.BC_Mode;
				base.BC_Mode = value;
				if (!IsCopying && oldValue != value)
				{
					Header?.HeaderContainersLineNumberGenerator.ReCalculateAll();
				}
			}
		}

		protected override ShortSequenceNumberGenerator AdditionalSealsLineNumberGeneratorCore => new CusSealSequenceNumberGenerator(() => AdditionalSeals.Cast<EU.NCTS.Business.CusSeal>());

		protected override CusInBondContainerValidation GetNewPhase4Validation() => new NctsDepartureHeaderContainerPhase4Validation(this);
	}
}
