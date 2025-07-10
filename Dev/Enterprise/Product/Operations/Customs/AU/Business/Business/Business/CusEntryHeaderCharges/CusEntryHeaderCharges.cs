using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderCharges : Customs.Business.CusEntryHeaderCharges, Integration.Customs.AU.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IsPayableToCustomsForHeader

		/// <summary>
		/// Header Level charges that are payable. These charges will be aggregated later in the message processor to determine Total payable
		/// as Customs sends the additional or refundable amount for amendment message for Total Payable segment.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public bool IsPayableToCustomsForHeader
		{
			get
			{
				switch (C1_ChargeType)
				{
					case CusEntryChargeTypeList.Codes.TotalPayableAdmin:
					case CusEntryChargeTypeList.Codes.DeclarationProcessingCharge:
					case CusEntryChargeTypeList.Codes.AQISProcessingCharge:
					case CusEntryChargeTypeList.Codes.AQISContainerCharges:
					case CusEntryChargeTypeList.Codes.Woodlevy:
					case CusEntryChargeTypeList.Codes.OtherCharges:
					case CusEntryChargeTypeList.Codes.AQISServicePaymentAmount:
					case CusEntryChargeTypeList.Codes.EntryFee:
					case CusEntryChargeTypeList.Codes.MessageFee:
					case CusEntryChargeTypeList.Codes.ScreenFree:
					case CusEntryChargeTypeList.Codes.TradegateGST:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion

		#region ShouldResetDataOnMergingCore

		protected override bool ShouldResetDataOnMergingCore => !IsNotCalculated && !IsLodgedProcessingCharge;

		#region IsUserEntered

		bool IsNotCalculated
		{
			get
			{
				switch (C1_ChargeType)
				{
					case CusEntryChargeTypeList.Codes.AQISServicePaymentAmount:
					case CusEntryChargeTypeList.Codes.OtherCharges:
					case CusEntryChargeTypeList.Codes.TotalPayableAdmin:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion

		#region IsLodgedProcessingCharge

		bool IsLodgedProcessingCharge => IsProcessingCharge && HasBeenLodgedAtCustoms;

		bool HasBeenLodgedAtCustoms => EntryHeader?.HasBeenLodgedAtCustoms ?? false;

		bool IsProcessingCharge
		{
			get
			{
				switch (C1_ChargeType)
				{
					case CusEntryChargeTypeList.Codes.AQISContainerCharges:
					case CusEntryChargeTypeList.Codes.AQISProcessingCharge:
					case CusEntryChargeTypeList.Codes.DeclarationProcessingCharge:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion

		#region IsDeferrable

		public bool IsDeferrable => IsChargeTypeDeferrable(C1_ChargeType);

		public static bool IsChargeTypeDeferrable(string chargeType)
		{
			switch (chargeType)
			{
				case CusEntryChargeTypeList.Codes.AQISProcessingCharge:
				case CusEntryChargeTypeList.Codes.DeclarationProcessingCharge:
				case CusEntryChargeTypeList.Codes.Woodlevy:
					return true;
				default:
					return false;
			}
		}

		#endregion
		#endregion

		protected override void CheckChargeTypeUniqueness(ZString value)
		{
			CheckASPChargeCreated(value);
		}

		void CheckASPChargeCreated(ZString value)
		{
			if (!Globals.IsTest && value == CusEntryChargeTypeList.Codes.AQISServicePaymentAmount && GlbStaff.CurrentUser.GS_Code != User.ServiceUserCode)
			{
				ErrorReporter.ReportOnce("ASP charge only can be added by PAYRECMessageProcessor.");
			}
		}

		public new CusEntryHeaderChargesValidation Validation
		{
			get { return (CusEntryHeaderChargesValidation)base.Validation; }
		}

		protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation() => new CusEntryHeaderChargesValidation(this);

		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;
	}
}
