using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IE.Business
{
	[CodeAlive("Currently a work in progress")]
	public class CustomsAndExciseReportProvider : MessageProvider
	{
		readonly ZString reportType;
		readonly ZDate startDate;
		readonly BusinessObjectFactory factory;

		readonly ZString[] reportTypesRequiringDate = new ZString[] {
			CustomsAndExciseReportTypeList.Codes.PSR,
			CustomsAndExciseReportTypeList.Codes.PCT,
			CustomsAndExciseReportTypeList.Codes.PTT,
			CustomsAndExciseReportTypeList.Codes.PCI,
			CustomsAndExciseReportTypeList.Codes.DSR,
			CustomsAndExciseReportTypeList.Codes.DCT,
			CustomsAndExciseReportTypeList.Codes.DTT
		};

		public CustomsAndExciseReportProvider(BusinessObjectFactory factory, ZString reportType, ZDate startDate)
		{
			if (startDate.IsEmpty && reportType.In(reportTypesRequiringDate))
			{
				throw new ArgumentException("reportType " + this.reportType + " requires a Date.", nameof(startDate));
			}

			this.reportType = reportType;
			this.startDate = startDate;
			this.factory = factory;
		}

		public ZString ReportUrl
		{
			get
			{
				var serverAddress = GetWebServiceEndPoint(factory);
				var endPoint = ZString.Empty;
				switch (reportType)
				{
					case CustomsAndExciseReportTypeList.Codes.PSR:
						endPoint = Constants.CustomsExciseReport.PSR(startDate);
						break;
					case CustomsAndExciseReportTypeList.Codes.PCT:
						endPoint = Constants.CustomsExciseReport.PCT(startDate);
						break;
					case CustomsAndExciseReportTypeList.Codes.PTT:
						endPoint = Constants.CustomsExciseReport.PTT(startDate);
						break;
					case CustomsAndExciseReportTypeList.Codes.PCI:
						endPoint = Constants.CustomsExciseReport.PCI(startDate);
						break;
					case CustomsAndExciseReportTypeList.Codes.DSR:
						endPoint = Constants.CustomsExciseReport.DSR(startDate);
						break;
					case CustomsAndExciseReportTypeList.Codes.DCT:
						endPoint = Constants.CustomsExciseReport.DCT(startDate);
						break;
					case CustomsAndExciseReportTypeList.Codes.DTT:
						endPoint = Constants.CustomsExciseReport.DTT(startDate);
						break;
					case CustomsAndExciseReportTypeList.Codes.UDR:
						endPoint = Constants.CustomsExciseReport.UDR;
						break;
					case CustomsAndExciseReportTypeList.Codes.BAL:
						endPoint = Constants.CustomsExciseReport.BAL;
						break;
				}
				return $"{serverAddress}{endPoint}";
			}
		}

		ZString GetWebServiceEndPoint(BusinessObjectFactory factory)
		{
			return WebServiceEndPointProvider.GetCustomsAndExciseReportURL(factory);
		}
	}
}
