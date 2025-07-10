using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[RemovalType(RemovalCode)]
	public class InterShedRemoval : CusUnderbond, INewShedProvider, Integration.Customs.GB.CCSUK.ICusUnderbond_InterShedRemoval
	{
		public InterShedRemoval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override void HandleSettingOfParent()
		{
			base.HandleSettingOfParent();
			this.C4_RL_NKDischargePort = WholeAwb != null ? WholeAwb.CargoTerminalOperatorAirport : ZString.Empty;
		}

		protected override Customs.Business.CusUnderbondValidation GetNewValidation()
		{
			return new InterShedRemovalValidation(this);
		}

		public const string RemovalCode = "ISR";

		public override ZString RemovalTypeHuman
		{
			get { return "Inter-Shed Removal"; }
		}

		[List(nameof(Lookups) + "." + nameof(CusUnderbondLookups.ShedsList))]
		public ZString NewShedId
		{
			get { return NewShedIdCore; }
			set { NewShedIdCore = value; }
		}
		public ZPropertyInfo NewShedIdInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(NewShedId), x => C4_DischargePremiseIDInfo); }
		}

		public override string ToString()
		{
			var table = new HtmlTableCreator();
			WriteCorePropertiesForInterpretation(table);
			table.WriteRow("New Shed", NewShedId);

			return table.ToHtml();
		}

		public override ZString Description
		{
			get { return NewShedId.IsEmpty ? "Inter-shed removal" : "Inter-shed removal to " + NewShedId; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C4_ApplicationCode = Enterprise.Customs.Business.CusUnderbondApplicationCodeList.Codes.GBInterShedRemoval;
		}
	}
}
