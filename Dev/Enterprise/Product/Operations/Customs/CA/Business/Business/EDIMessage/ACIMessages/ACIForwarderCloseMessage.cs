namespace Enterprise.Customs.CA.Business
{
	using System.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using MasterFiles.Business.CustomValues;

	[SystemDefinedValues]
	public class ACIForwarderCloseMessage : ACIForwarderMessage
	{
		public ACIForwarderCloseMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : EDIMessage.Schema
		{
			public const string NeedAutoCloseReport = "NeedAutoCloseReport";
		}

		#endregion

		#region Overrides

		public override ZString CargoControlNumber
		{
			get { return BGMReference; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
		}

		#endregion

		#region NeedAutoCloseReport

		public ZBool NeedAutoCloseReport
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.NeedAutoCloseReport); }
			set { this.SetSystemDefinedValue(Schema.NeedAutoCloseReport, value); }
		}

		#endregion

	}
}
