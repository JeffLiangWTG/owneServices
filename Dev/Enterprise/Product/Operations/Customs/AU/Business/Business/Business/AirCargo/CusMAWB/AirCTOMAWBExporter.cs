using System.Collections;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOMAWBExporter : CMRDataExporterCSV
	{
		public AirCTOMAWBExporter(CTOCusMAWB mAWB)
			: base(mAWB)
		{
		}

		public CTOCusMAWB MAWB
		{
			get { return (CTOCusMAWB)BizObj; }
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
					fAdditionalData.IsOriginPremiseReadOnly = true;
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

				foreach (CTOCusHAWB hAWB in MAWB.ChildBills)
				{
					StringCollectionX result = new StringCollectionX();

					result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number); //1
					result.Add(GlbStaff.CurrentUser.GS_EmailAddress); //2
					result.Add(hAWB.CS_FlightNo); //3
					result.Add(CMRDataExporterCSV.CMRDateString(hAWB.CS_ArrivalDate)); //4
					result.Add(hAWB.CS_HAWB); //5
					result.Add(ZString.Empty); //6
					result.Add(ZString.Empty); //7
					result.Add(ZString.Empty); //8
					result.Add(StripLineBreakInString(hAWB.CS_GoodsDescription)); //9
					result.Add(hAWB.CS_ConsignorName); //10
					result.Add(hAWB.CS_ConsigneeName); //11
					result.Add(hAWB.ConsigneeAddressAsASingleLine); //12
					result.Add(ZString.Empty); //13
					result.Add(GetRoutingInformation(hAWB)); //14
					result.Add(ZString.Empty); //15
					result.Add(hAWB.CS_RL_NKDischargePort); //16
					result.Add(hAWB.CS_RL_NKDestination); //17
					result.Add(ZString.Empty); //18
					result.Add(ZString.Empty); //19
					result.Add(ZString.Empty); //20
					result.Add("N"); //21
					result.Add(AdditionalData.OriginPremise); //22
					result.Add(hAWB.CS_PiecesManifested.ToString()); //23
					result.Add(ZString.Empty); //24

					list.Add(result);
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}

		string GetRoutingInformation(CTOCusHAWB hAWB)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(GetPaddedString(hAWB.CS_RL_NKOrigin));
			builder.Append(GetPaddedString(hAWB.MAWB.CM_RL_NKRoutePort1));
			builder.Append(GetPaddedString(hAWB.MAWB.CM_RL_NKRoutePort2));
			builder.Append(GetPaddedString(hAWB.MAWB.CM_RL_NKRoutePort3));

			ZString result = builder.ToString();
			if (result.EndsWith(" "))
			{
				result = result.SubstringSafe(0, result.Length - 1);
			}

			return result;
		}

		string GetPaddedString(string inputString)
		{
			return !string.IsNullOrEmpty(inputString) ? inputString + " " : inputString;
		}
	}
}
