using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForConsignmentReport : CcsukWrapper
	{
		public new static CcsukWrapperForConsignmentReport New(BusinessObject awb, BusinessObjectFactory factoryToWrap)
		{
			return new CcsukWrapperForConsignmentReport(awb, factoryToWrap);
		}

		CcsukWrapperForConsignmentReport(BusinessObject awb, BusinessObjectFactory factoryToWrap)
			: base(null, awb, factoryToWrap)
		{ }

		public CcsukWrapperForConsignmentReportSplitLineCollection Splits
		{
			get { return splits ?? (splits = new CcsukWrapperForConsignmentReportSplitLineCollection(iCcsukCusAwb, iCcsukCusAwb.Factory)); }
		}
		CcsukWrapperForConsignmentReportSplitLineCollection splits;

		public CcsukWrapperForConsignmentReportHouseLineCollection Houses
		{
			get { return houses ?? (houses = new CcsukWrapperForConsignmentReportHouseLineCollection(mawb, mawb.Factory)); }
		}
		CcsukWrapperForConsignmentReportHouseLineCollection houses;

		public ZString STATUSONEDATE
		{
			get { return iCcsukCusAwb.Status1Date.ToString("dd-MMM-yyyy HH:mm"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "the sniffer is stupid ")]
		public ZString FLIGHTANDDATE
		{
			get { return mawb != null ? mawb.CM_FlightNo + " " + mawb.CM_ArrivalDate.ToString("dd-MMM-yyyy") : ""; }
		}

		public ZString STATUS2
		{
			get { return iCcsukCusAwb.Status2Granted ? "Yes" : "No"; }
		}

		public ZString CREATEDDETAILS
		{
			get { return iCcsukCusAwb.LocalCreationDate.ToString("dd/MM/yy HH:mm", CultureInfo.CurrentCulture); }
		}

		public ZString EDITDETAILS
		{
			get { return iCcsukCusAwb.LastEditTime.ToString("dd/MM/yy HH:mm", CultureInfo.CurrentCulture); }
		}

		public ZString USER
		{
			get
			{
				var u = iCcsukCusAwb.UserInChargeOfJob;
				return u != null ? u.GS_LoginName : null;
			}
		}

		public ZString FREIGHTREFERENCE
		{
			get
			{
				var result = "";
				if (houseBill != null && houseBill.Shipment != null)
				{
					result = houseBill.Shipment.JS_UniqueConsignRef;
				}
				else if (mawb != null && mawb.Consol != null)
				{
					result = mawb.Consol.JK_UniqueConsignRef;
				}
				return result;
			}
		}

		public ZBool HASDECLARATIONUCR
		{
			get { return Declaration != null; }
		}

		public ZBool HASOUTTURNS
		{
			get { return iCcsukCusAwb.OutTurns.Count > 0; }
		}

		public ZBool HASSPLITS
		{
			get { return iCcsukCusAwb.HasSplits; }
		}

		public ZBool ISCONSOL
		{
			get { return (houseBill != null && houseBill.CS_IsMasterHouse) && !mawb.IsBasic; }
		}

		public JobDeclaration Declaration
		{
			get
			{
				JobDeclaration result = null;
				if (iCcsukCusAwb is SplitConsignment)
				{
					var split = iCcsukCusAwb as SplitConsignment;
					result = split.OwnDeclaration ?? GetDeclarationFromMUCR(split);
				}
				else if (houseBill != null)
				{
					result = houseBill.Declaration;  // houses or basics
				}
				return result;
			}
		}

		JobDeclaration GetDeclarationFromMUCR(SplitConsignment splitConsignment)
		{
			JobDeclaration jobDeclaration = null;

			var mUCR = splitConsignment.GetMasterUCRReference();

			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryNum, mUCR);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobDeclaration.Schema.TableName);
			query.OrderBy = CusEntryNumSchema.CE_IssueDate.Name + " DESC";
			CusEntryNumber cusEntryNum = Factory.LoadTop1<CusEntryNumber>(query);
			if (cusEntryNum != null)
			{
				var foundDeclaration = Factory.Load<JobDeclaration>(cusEntryNum.CE_ParentID);
				if (foundDeclaration?.ZG_Gateway.ToString() == GatewayList.Codes.CCSUKviaNTMsgGW)
				{
					jobDeclaration = foundDeclaration;
				}
			}

			return jobDeclaration;
		}
	}
}
