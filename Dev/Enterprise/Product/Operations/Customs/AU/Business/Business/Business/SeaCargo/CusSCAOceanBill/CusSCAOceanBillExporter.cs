using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillExporter : CMRDataExporterCSV
	{
		public CusSCAOceanBillExporter(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
		}

		public CusSCAOceanBill OceanBill
		{
			get { return (CusSCAOceanBill)BizObj; }
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
			get { return OceanBill; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return OceanBill; }
		}

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
					if (OceanBill.Consol != null)
					{
						if (OceanBill.Consol.UnpackDepotAddress != null)
						{
							fAdditionalData.OriginPremise = OceanBill.Consol.UnpackDepotAddress.LocalControlledPremisesID;
						}
						if (fAdditionalData.OriginPremise.IsEmpty && OceanBill.Consol.ArrivalCTOAddress != null)
						{
							fAdditionalData.OriginPremise = OceanBill.Consol.ArrivalCTOAddress.LocalControlledPremisesID;
						}
					}
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

				foreach (CusSCAHouse house in OceanBill.HouseBills)
				{
					foreach (CusSCAPivot pivot in house.Pivot)
					{
						StringCollectionX result = new StringCollectionX();
						result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number); //1
						result.Add(GlbStaff.CurrentUser.GS_EmailAddress); //2
						result.Add(OceanBill.CB_LloydsIMO); //3
						result.Add(OceanBill.CB_Voyage); //4
						result.Add(OceanBill.CB_OceanBill); //5
						result.Add(house.CA_HouseBill); //6
						result.Add(house.CA_MasterHouseBill); //7
						result.Add(pivot.Container.CN_ContainerNumber); //8
						result.Add(StripLineBreakInString(pivot.CV_GoodsDescription)); //9
						result.Add(house.CA_ConsignorName); //10
						result.Add(house.CA_ConsigneeName); //11
						result.Add(house.ConsigneeAddressAsASingleLine); //12
						result.Add(pivot.Container.CN_ContainerMode); //13
						result.Add(ZString.Empty); //14
						result.Add(OceanBill.CB_RL_NKPortOfLoading); //15
						result.Add(OceanBill.CB_RL_NKPortOfDischarge); //16
						result.Add(house.CA_RL_NK_PortOfDestination); //17
						result.Add(ZString.Empty); //18
						result.Add(pivot.CV_HazardousGoods ? "Y" : "N"); //19
						result.Add(house.CA_NotifyName); //20
						result.Add(pivot.CV_IsSAC ? "Y" : "N"); //21
						result.Add(AdditionalData.OriginPremise); //22
						result.Add(pivot.CV_PackageCount.ToString()); //23
						result.Add(house.CA_BGMReference); //24
						list.Add(result);
					}
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}
	}
}
