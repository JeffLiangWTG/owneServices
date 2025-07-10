using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSPackage : EU.EMCS.Business.EMCSPackage
	{
		public EMCSPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString B5_UnitType
		{
			get => base.B5_UnitType;
			set
			{
				var oldValue = B5_UnitType;
				base.B5_UnitType = value;
				if (!IsCopying && oldValue != value)
				{
					ClearB5_MarksAndNumbersIfNecessary();
					Validation.ValidateB5_MarksAndNumbers();
				}
			}
		}

		[ReadOnlyMember(nameof(B5_MarksAndNumbersReadOnly))]
		public override ZString B5_MarksAndNumbers { get => base.B5_MarksAndNumbers; set => base.B5_MarksAndNumbers = value; }

		bool B5_MarksAndNumbersReadOnly => !EMCSUniversalHelper.IsCountable(this);

		protected override CusInvPackValidation GetNewValidation() => new EMCSPackageValidation(this);

		void ClearB5_MarksAndNumbersIfNecessary()
		{
			if (B5_MarksAndNumbersInfo.ReadOnly && !B5_MarksAndNumbers.IsEmpty)
			{
				B5_MarksAndNumbers = ZString.Empty;
			}
		}
	}
}
