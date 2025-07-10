using System.Collections;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOHAWBExporter : CMRDataExporterCSV
	{
		public AirCTOHAWBExporter(CTOCusHAWB hAWB)
			: base(hAWB)
		{
		}

		public CTOCusHAWB HAWB
		{
			get { return (CTOCusHAWB)BizObj; }
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
				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number); //1
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress); //2
				result.Add(HAWB.CS_FlightNo); //3
				result.Add(CMRDataExporterCSV.CMRDateString(HAWB.CS_ArrivalDate)); //4
				result.Add(HAWB.CS_HAWB); //5
				result.Add(ZString.Empty); //6
				result.Add(ZString.Empty); //7
				result.Add(ZString.Empty); //8
				result.Add(StripLineBreakInString(HAWB.CS_GoodsDescription)); //9
				result.Add(HAWB.CS_ConsignorName); //10
				result.Add(HAWB.CS_ConsigneeName); //11
				result.Add(HAWB.ConsigneeAddressAsASingleLine); //12
				result.Add(ZString.Empty); //13
				result.Add(RoutingInformation); //14
				result.Add(ZString.Empty); //15
				result.Add(HAWB.CS_RL_NKDischargePort); //16
				result.Add(HAWB.CS_RL_NKDestination); //17
				result.Add(ZString.Empty); //18
				result.Add(ZString.Empty); //19
				result.Add(ZString.Empty); //20
				result.Add("N"); //21
				result.Add(AdditionalData.OriginPremise); //22
				result.Add(HAWB.CS_PiecesManifested.ToString()); //23
				result.Add(ZString.Empty); //24

				list.Add(result);

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}

		string RoutingInformation
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(GetPaddedString(HAWB.CS_RL_NKOrigin));
				builder.Append(GetPaddedString(HAWB.MAWB.CM_RL_NKRoutePort1));
				builder.Append(GetPaddedString(HAWB.MAWB.CM_RL_NKRoutePort2));
				builder.Append(GetPaddedString(HAWB.MAWB.CM_RL_NKRoutePort3));

				ZString result = builder.ToString();
				if (result.EndsWith(" "))
				{
					result = result.SubstringSafe(0, result.Length - 1);
				}

				return result;
			}
		}

		string GetPaddedString(string inputString)
		{
			return !string.IsNullOrEmpty(inputString) ? inputString + " " : inputString;
		}
	}
}
