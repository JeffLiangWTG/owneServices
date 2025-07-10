namespace Enterprise.Customs.GB.Business
{
	public abstract class GbInventoryManagementMessageFunction : GbDes242MessageFunction
	{
		public override string FunctionCode { get { return SubFunctionCode; } }
		public override string FunctionHuman { get { return string.Format("{0} ({1} - {2})", HumanNameVerbCore, SubFunctionCode, Level); } }
		protected abstract string HumanNameVerbCore { get; }
		public virtual MasterOrDeclaration Level { get; private set; }
		public enum MasterOrDeclaration
		{
			Declaration,
			Master
		}

		public class ArrivalActual : GbInventoryManagementMessageFunction
		{
			public ArrivalActual(MasterOrDeclaration level) { Level = level; }
			public override string SubFunctionCode { get { return SubFunctionCodeCore; } }
			protected override string HumanNameVerbCore { get { return "Arrive at location"; } }
			public const string SubFunctionCodeCore = "EAL";
		}

		public class ArrivalAnticipated : GbInventoryManagementMessageFunction
		{
			public ArrivalAnticipated(MasterOrDeclaration level) { Level = level; }
			public override string SubFunctionCode { get { return SubFunctionCodeCore; } }
			protected override string HumanNameVerbCore { get { return "Anticipate arrival at location"; } }
			public const string SubFunctionCodeCore = "EAA";
		}

		public class Departure : GbInventoryManagementMessageFunction
		{
			public Departure(MasterOrDeclaration level) { Level = level; }
			public override string SubFunctionCode { get { return SubFunctionCodeCore; } }
			protected override string HumanNameVerbCore { get { return "Depart from location"; } }
			public const string SubFunctionCodeCore = "EDL";
		}
	}
}
