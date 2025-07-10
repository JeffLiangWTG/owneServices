using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationItem : MessageSendingAction, IObsoleteValidation
	{
		public ExitNotificationItem(CusExitItem exitItem) : base(exitItem, (x) => string.Empty)
		{
			this.exitItem = exitItem;
		}

		public new CusExitItem MessagingObject => (CusExitItem)base.MessagingObject;

		[ResourceStringData("5F37CB7E-572C-4618-883E-CF01D2B82F20", Caption = "Select")]
		public override ZBool ShouldSend { get => base.ShouldSend; set => base.ShouldSend = value; }

		[ResourceStringData("e2d6433b-c353-41fe-874f-cc3808ff48d3", Caption = "Line No.")]
		public ZInt LineNo => exitItem.CXI_LineNumber;

		[ResourceStringData("5d4651e4-795c-41c9-bb00-90b40ba3a34c", Caption = "Gross Mass")]
		public ZDecimal GrossMass => exitItem.CXI_GrossMass;

		[ResourceStringData("8d596083-b481-44f9-9858-586a65cf6600", Caption = "Gross")]
		public ZString Gross => exitItem.CXI_GrossMassUQ;

		[ResourceStringData("b10cb2db-86bf-4b9e-941f-94b437be94a9", Caption = "Net Mass")]
		public ZDecimal NetMass => exitItem.CXI_NetMass;

		[ResourceStringData("ba6e526a-12a9-472f-b341-bcbf65df5b00", Caption = "Net")]
		public ZString Net => exitItem.CXI_NetMassUQ;

		[ResourceStringData("1e7d265c-47c9-4435-90bf-42401b9a0c6a", Caption = "Status")]
		public ZString Status => exitItem.CXI_Status;

		[ResourceStringData("54096cc9-5ab6-4e9c-902d-973c2183f37f", Caption = "Status Description")]
		public ZString StatusDescription => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, Status, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDate.Today)?.ZZD_Description ?? ZString.Empty;

		#region Lookups

		public ExitNotificationItemLookups Lookups => fLookups ?? (fLookups = new ExitNotificationItemLookups(this));
		ExitNotificationItemLookups fLookups;

		#endregion

		#region Validation

		public ExitNotificationItemValidation Validation => new ExitNotificationItemValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
		readonly CusExitItem exitItem;
	}
}
