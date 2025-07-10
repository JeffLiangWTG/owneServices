using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CalculatedCusStatus : Customs.Business.CusStatus
	{
		public CalculatedCusStatus(ZPropertyInfo wrappedStatusInfo, ICalculatedCusStatusCalculator calculator)
			: this(wrappedStatusInfo, calculator, new CodeDescriptionPairList(), ZString.Empty)
		{
		}

		public CalculatedCusStatus(ZPropertyInfo wrappedStatusInfo, ICalculatedCusStatusCalculator calculator, CodeDescriptionPairList statusList)
			: this(wrappedStatusInfo, calculator, statusList, ZString.Empty)
		{
		}

		public CalculatedCusStatus(ZPropertyInfo wrappedStatusInfo, ICalculatedCusStatusCalculator calculator, CodeDescriptionPairList statusList, ZString defaultValue)
			: base(wrappedStatusInfo, statusList, defaultValue)
		{
			this.Calculator = calculator;
		}

		#region CusStatus Overrides

		#region Code

		public override ZString Code
		{
			get
			{
				Calculator.DeriveStatusIfEmptyWithMessages();
				return base.Code;
			}
		}

		#endregion

		public override ZString UserFriendlyStatuses
		{
			get { return Calculator.UserFriendlyStatusText; }
		}

		#endregion

		#region Implementation

		protected readonly ICalculatedCusStatusCalculator Calculator;

		#endregion
	}
}
