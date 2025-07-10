using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirUnderbondSelectorLine : UnderbondSelectorLine
	{
		public AirUnderbondSelectorLine(CusUnderbond cusUnderbond)
			: base(cusUnderbond)
		{
		}

		public UnderbondStatus UnderbondStatus { get; private set; }

		public override ZString UnderbondStatusText
		{
			get { return UnderbondStatus.ToString(); }
		}

		protected internal override void UpdateStatuses()
		{
			UnderbondStatus = Underbond.GetAccumulatedUnderbondStatus();
		}

		public override ZString OutturnStatusText
		{
			get { return ZString.Empty; }
		}
	}
}
