using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirReportMAWBExporter : CMRDataExporterCSV
	{
		public AirReportMAWBExporter(CusMAWB mAWB)
			: base(mAWB)
		{
		}

		public CusMAWB MAWB
		{
			get { return (CusMAWB)BizObj; }
		}

		public override string PartFileName
		{
			get
			{
				return "Cargo";
			}
		}

		public override string MailSubject
		{
			get
			{
				return "Contingency Cargo Report";
			}
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return MAWB; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return MAWB; }
		}

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
					fAdditionalData.OriginPremise = GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID;
				}
				return fAdditionalData;
			}
		}
		AdditionalContingencyData fAdditionalData;

		protected override StringCollectionX[] Values
		{
			get
			{
				ArrayList list = new ArrayList();

				foreach (CusHAWB hAWB in MAWB.ChildBills)
				{
					StringCollectionX result = new StringCollectionX();
					result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
					result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
					result.Add(hAWB.CS_FlightNo);
					result.Add(CMRDataExporterCSV.CMRDateString(hAWB.CS_ArrivalDate));
					result.Add(hAWB.CS_MasterBillNum);
					result.Add(hAWB.CS_HAWB);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(StripLineBreakInString(hAWB.CS_GoodsDescription));
					result.Add(hAWB.CS_ConsignorName);
					result.Add(hAWB.CS_ConsigneeName);
					result.Add(hAWB.ConsigneeAddressAsASingleLine);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(MAWB.CM_RL_NKDischargePort);
					result.Add(hAWB.CS_RL_NKDestination);
					result.Add(hAWB.IsDocuments ? "Y" : "N");
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(hAWB.CS_IsSelfAssessedClearance ? "Y" : "N");
					result.Add(AdditionalData.OriginPremise);
					result.Add(hAWB.CS_PiecesManifested.ToString());
					result.Add(ZString.Empty);
					list.Add(result);
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}
	}
}
