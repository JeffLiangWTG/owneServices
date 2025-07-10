using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		JobDeclaration declaration
		{
			get { return Bill.Declaration; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCU_fPartShipConsignmentReference();
		}

		public void ValidateCU_fPartShipConsignmentReference()
		{
			ValidateCalculatedProperty(Bill.CU_fPartShipConsignmentReferenceInfo);
		}

		protected virtual void CheckCU_fPartShipConsignmentReference()
		{
			if (!Bill.CU_fPartShipConsignmentReference.IsEmpty)
			{
				Bill.CU_fPartShipConsignmentReferenceInfo.AddWarning(ConsignReferenceEntered);
			}
		}

		internal const string ConsignReferenceEntered = "Please note that the entered reference number must match the value assigned to this house bill by the Air Courier (forwarder) who lodged the Air Cargo Report. If you enter a value, and this value does not match the value on the cargo report, or there is no value on the cargo report, then the cargo report will not match to the declaration and the cargo will not clear (even if the master and house bills match correctly).";

		#region House Bill Validation

		protected override void CheckCU_BillNumForHouseBillType()
		{
			base.CheckCU_BillNumForHouseBillType();

			CheckNotEmptyForEdifact(Bill.CU_BillNumInfo);
			if (declaration != null && !declaration.IsSACWithoutLines)
			{
				if (declaration.IsImport && declaration.IsPost)
				{
					if (Bill.CU_BillNum.IsEmpty)
					{
						Bill.CU_BillNumInfo.AddMessageError(ParcelPostNumberIsRequired);
					}
					else if (declaration.IsImportCMR)
					{
						ValidateParcelPostNumbersForCMR();
					}
					else
					{
						ValidateParcelPostNumbersForEdifice();
					}
				}
			}
		}

		void ValidateParcelPostNumbersForEdifice()
		{
			ZString errors = new ParcelPostNumberValidator().Validate(Bill.CU_BillNum);
			if (!errors.IsEmpty)
			{
				Bill.CU_BillNumInfo.AddMessageError(errors);
			}
		}
		void ValidateParcelPostNumbersForCMR()
		{
			if (!Bill.CU_BillNum.StartsWith("N")
				&& !Bill.CU_BillNum.StartsWith("V")
				&& !Bill.CU_BillNum.StartsWith("Q")
				&& !Bill.CU_BillNum.StartsWith("W")
				&& !Bill.CU_BillNum.StartsWith("S"))
			{
				Bill.CU_BillNumInfo.AddMessageError(InvalidParcelPostNumber);
			}
		}

		public const string InvalidParcelPostNumber = "Parcel Post number must start with 'N', 'V', 'Q', 'W' or 'S'";
		public const string ParcelPostNumberIsRequired = "A parcel post card number is required.";

		protected override bool NeedToValidateHouseBillAndPackages
		{
			get { return declaration.IsImportCMR && !declaration.IsSAC; }
		}

		public override INotificationType NotificationTypeForAirWayBillNumber
		{
			get
			{
				if (declaration.IsImport)
				{
					return CargoWise.EntityFramework.NotificationType.MessageError;
				}
				else
				{
					return CargoWise.EntityFramework.NotificationType.Warning;
				}
			}
		}

		#endregion

		public static void CheckNotEmptyForEdifact(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				ZString valueAfterStripping = new UNOBCharacterSet().FormatElement((ZString)propertyInfo.Value);

				if (valueAfterStripping.IsEmpty)
				{
					propertyInfo.AddMessageError("Please enter valid data. The current data is invalid for messaging.");
				}
			}
		}
	}
}
