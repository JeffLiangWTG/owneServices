using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirReportHAWBExporter : CMRDataExporterCSV
	{
		public AirReportHAWBExporter(CusHAWB hAWB)
			: base(hAWB)
		{
		}

		public CusHAWB HAWB
		{
			get { return (CusHAWB)BizObj; }
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
			get { return HAWB; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return HAWB; }
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
				StringCollectionX result = new StringCollectionX();
				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				result.Add(HAWB.CS_FlightNo);
				result.Add(CMRDataExporterCSV.CMRDateString(HAWB.CS_ArrivalDate));
				result.Add(HAWB.CS_MasterBillNum);
				result.Add(HAWB.CS_HAWB);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(HAWB.CS_GoodsDescription);
				result.Add(HAWB.CS_ConsignorName);
				result.Add(HAWB.CS_ConsigneeName);
				result.Add(HAWB.ConsigneeAddressAsASingleLine);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(HAWB.MAWB.CM_RL_NKDischargePort);
				result.Add(HAWB.CS_RL_NKDestination);
				result.Add(HAWB.IsDocuments ? "Y" : "N");
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(HAWB.CS_IsSelfAssessedClearance ? "Y" : "N");
				result.Add(AdditionalData.OriginPremise);
				result.Add(HAWB.CS_PiecesManifested.ToString());
				result.Add(ZString.Empty);
				list.Add(result);

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}
	}
}
