using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationPackage : MessageSendingAction, IObsoleteValidation
	{
		public ExitNotificationPackage(CusExitItemPackage exitDetailPackage) : base(exitDetailPackage, (x) => string.Empty)
		{
			this.exitDetailPackage = exitDetailPackage;
			PackQTY = exitDetailPackage.B5_UnitCount;
		}

		public new CusExitItemPackage MessagingObject => (CusExitItemPackage)base.MessagingObject;

		[ResourceStringData("F337FDAB-B8DA-48B9-861A-FD105367E013", Caption = "Select")]
		public override ZBool ShouldSend { get => base.ShouldSend; set => base.ShouldSend = value; }

		[ResourceStringData("51B6F638-02E4-487E-8C6F-1CE26C2CCEC9", Caption = "Item No.")]
		public ZShort ItemNo => exitDetailPackage.Parent.CXI_LineNumber;

		[ResourceStringData("521C05CF-0BE9-4FE3-8FD2-9B9F125063A2", Caption = "Line No.")]
		public ZString LineNo => exitDetailPackage.B5_PackageID;

		[ResourceStringData("044D0F9A-C713-48A9-A560-90C9E39B4119", Caption = "Pack QTY")]
		public ZLong PackQTY
		{
			get => packQTY;
			set
			{
				SetNonPersistentPropertyValue(PackQTYInfo, ref packQTY, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePackQTY();
				}
			}
		}
		ZLong packQTY;
		public ZPropertyInfo PackQTYInfo => GetZPropertyInfo(nameof(PackQTY));

		[ResourceStringData("37F1AE09-1E1D-4901-8224-B0705BDFB6D7", Caption = "Pack Type")]
		public ZString PackType => exitDetailPackage.B5_UnitType;

		[ResourceStringData("B7674D5F-F37C-48BB-ACC5-630052C9E933", Caption = "Marks & Numbers")]
		public ZString MarksNumbers => exitDetailPackage.B5_MarksAndNumbers;

		#region Lookups

		public ExitNotificationPackageLookups Lookups => fLookups ?? (fLookups = new ExitNotificationPackageLookups(this));
		ExitNotificationPackageLookups fLookups;

		#endregion

		#region Validation

		public ExitNotificationPackageValidation Validation => new ExitNotificationPackageValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
		readonly CusExitItemPackage exitDetailPackage;
	}
}
