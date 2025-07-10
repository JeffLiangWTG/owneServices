using System;
using System.Data;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Data;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public class AnalyticsAPI<T> : BIAPI where T : AnalyticsModelHelper, new()
	{
		[ThreadSafe]
		static readonly Lazy<AnalyticsAPI<T>> lazy = new Lazy<AnalyticsAPI<T>>(() => new AnalyticsAPI<T>());

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Singleton Instance.")]
		public static AnalyticsAPI<T> Instance => lazy.Value;

		AnalyticsAPI()
		{
		}

		public T ModelHelper => modelHelper ?? (modelHelper = new T());
		T modelHelper;

		public string AnalysisServer
		{
			get
			{
				if (analysisServer == null)
				{
					using (Db.DisposableActionForDbConnection())
					{
						analysisServer = BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection);
					}
				}
				return analysisServer;
			}
		}

		string analysisServer;

		[ThreadSafe]
		public override bool IsAPIEnabled
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return BiServers.LoadReportAPIUsingCacheIfPossible(Db.Connection);
				}
			}
		}
		public override void VerifyAPIIsEnabled()
		{
			if (!IsAPIEnabled)
			{
				throw new AnalyticsAPIException(Res.GetString("93b5b84d-0163-4ff7-8260-bc46224a4dc0", "The Analytics Web API has not been enabled on this system"));
			}
		}

		public DataTable GetShipmentProfileReportData(string shipmentNo, string companyCode, string countryCode)
		{
			VerifyAPIIsEnabled();
			var dax = $"DEFINE\r\n    MEASURE 'Consolidation Transport'[First Vessel] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'Consolidation Transport'[Vessel], 1 ) )\r\n    MEASURE 'Consolidation Transport'[First Voyage Flight] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            FIRSTNONBLANK (\r\n                'Consolidation Transport'[Voyage Flight],\r\n                1\r\n            )\r\n        )\r\n    MEASURE 'Consolidation'[ETA Discharge] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'Main Transport'[ETA], TRUE () ) )\r\n    MEASURE 'Consolidation'[ETD Load] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'Main Transport'[ETD], TRUE () ) )\r\n    MEASURE 'Consolidation'[ETA Last Discharge] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            CALCULATE (\r\n                FIRSTNONBLANK ( 'Consolidation Transport'[ETA], TRUE () ),\r\n                TOPN (\r\n                    1,\r\n                    'Consolidation Transport',\r\n                    'Consolidation Transport'[Leg Order], DESC\r\n                )\r\n            )\r\n        )\r\n    MEASURE 'Consolidation'[ETD First Load] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            CALCULATE (\r\n                FIRSTNONBLANK ( 'Consolidation Transport'[Load Port Code], TRUE () ),\r\n                TOPN ( 1, 'Consolidation Transport', 'Consolidation Transport'[Leg Order], ASC )\r\n            )\r\n        )\r\n    MEASURE 'Consolidation'[First Load] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            CALCULATE (\r\n                FIRSTNONBLANK ( 'Load Port'[Load Port Code], TRUE () ),\r\n                TOPN ( 1, 'Consolidation Transport', 'Consolidation Transport'[Leg Order], ASC )\r\n            )\r\n        )\r\n    MEASURE 'Consolidation'[Last Discharge] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            CALCULATE (\r\n                FIRSTNONBLANK ( 'Consolidation Transport'[Discharge Port Code], TRUE () ),\r\n                TOPN (\r\n                    1,\r\n                    'Consolidation Transport',\r\n                    'Consolidation Transport'[Leg Order], DESC\r\n                )\r\n            )\r\n        )\r\n    MEASURE 'Consolidation'[Console ATA] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'Main Transport'[ATA], TRUE () ) )\r\n    MEASURE 'Consolidation'[Console ATD] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'Main Transport'[ATD], TRUE () ) )\r\n    MEASURE 'Consolidation'[Contract Number] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( Consolidation[Contract No], TRUE () ) )\r\n    MEASURE 'Shipment'[Shipment Customs Info] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            CONCATENATEX (\r\n                DISTINCT ( 'Shipment Customs Info' ),\r\n                'Shipment Customs Info'[Customs Info],\r\n                \",\"\r\n            )\r\n        )\r\n    MEASURE 'Shipment'[GL Branch] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'GL Transaction'[Branch], TRUE () ) )\r\n    MEASURE 'Shipment'[GL Department] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'GL Transaction'[Department], TRUE () ) )\r\n    MEASURE 'Shipment'[GL Local Agent] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'GL Transaction'[Local Agent], TRUE () ) )\r\n    MEASURE 'Shipment'[GL Representative Operator] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            FIRSTNONBLANK (\r\n                'GL Transaction'[Representative Operator],\r\n                TRUE ()\r\n            )\r\n        )\r\n    MEASURE 'Shipment'[GL Overseas Agent Code] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            FIRSTNONBLANK (\r\n                'GL Transaction'[Overseas Agent Code],\r\n                TRUE ()\r\n            )\r\n        )\r\n    MEASURE 'Shipment'[GL Overseas Agent Name] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            FIRSTNONBLANK (\r\n                'GL Transaction'[Overseas Agent Name],\r\n                TRUE ()\r\n            )\r\n        )\r\n    MEASURE 'Shipment'[GL Representative Sales] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            FIRSTNONBLANK (\r\n                'GL Transaction'[Representative Sales],\r\n                TRUE ()\r\n            )\r\n        )\r\n    MEASURE 'Shipment'[GL Status] =\r\n        ( /* USER DAX BEGIN */\r\n        FIRSTNONBLANK ( 'GL Transaction'[Status], TRUE () ) )\r\n    MEASURE 'Shipment'[GL Create Date] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            FIRSTNONBLANK (\r\n                'GL Transaction'[Job Header Create Date Utc],\r\n                TRUE ()\r\n            )\r\n        )\r\n    MEASURE 'Shipment'[GL Revenue Recognition Date] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            IF (\r\n                COUNTROWS ( 'GL Transaction' ) > 0,\r\n                FIRSTNONBLANK ( 'Revenue Recognition'[Recognition Date], TRUE () )\r\n            )\r\n        )\r\n    MEASURE 'Local Agent AR Settlement'[Local Client AR Settlement Group] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            IF (\r\n                COUNTROWS ( 'GL Transaction' ) > 0,\r\n                FIRSTNONBLANK (\r\n                    'Local Agent AR Settlement'[Local Agent AR Settlement Related Party],\r\n                    TRUE ()\r\n                )\r\n            )\r\n        )\r\n    MEASURE 'Overseas Agent AR Settlement'[Overseas Agent AR Settlement Group] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            IF (\r\n                COUNTROWS ( 'GL Transaction' ) > 0,\r\n                FIRSTNONBLANK (\r\n                    'Overseas Agent AR Settlement'[Overseas Agent AR Settlement Related Party],\r\n                    TRUE ()\r\n                )\r\n            )\r\n        )\r\n    MEASURE 'Main Consol Shipment'[Freight Direction] =\r\n        (\r\n            /* USER DAX BEGIN */\r\n            FIRSTNONBLANK ( 'Main Consol Shipment'[Direction], TRUE () )\r\n        )\r\n    VAR minDate =\r\n        CALCULATE (\r\n            VALUES ( 'Shipment'[Shipment Create Date] ),\r\n            KEEPFILTERS (\r\n                FILTER ( ALL ( 'Shipment'[Job Number] ), 'Shipment'[Job Number] = \"{shipmentNo}\" )\r\n            )\r\n        )\r\n    VAR maxDate =\r\n        CALCULATE ( DATE ( YEAR ( minDate ) + 2, MONTH ( minDate ), DAY ( minDate ) ) )\r\n    VAR __DS0FilterTable =\r\n        CALCULATETABLE (\r\n            VALUES ( 'Shipment'[Shipment Create Date] ),\r\n            KEEPFILTERS (\r\n                FILTER (\r\n                    ALL ( 'Shipment'[Shipment Create Date] ),\r\n                    'Shipment'[Shipment Create Date] >= minDate\r\n                        && 'Shipment'[Shipment Create Date] <= maxDate\r\n                )\r\n            )\r\n        )\r\n    VAR __DS0FilterTable2 =\r\n        FILTER (\r\n            KEEPFILTERS ( VALUES ( 'Shipment Coload Type'[Coload Type] ) ),\r\n            'Shipment Coload Type'[Coload Type] = \"All Shipments\"\r\n        )\r\n    VAR __DS0FilterTable3 =\r\n        FILTER (\r\n            KEEPFILTERS ( VALUES ( 'Shipment'[Is Active] ) ),\r\n            'Shipment'[Is Active] = TRUE\r\n        )\r\n    VAR __DS0FilterTable4 =\r\n        FILTER (\r\n            KEEPFILTERS ( VALUES ( 'Company Branch'[Company Code] ) ),\r\n            'Company Branch'[Company Code] = \"{companyCode}\"\r\n        )\r\n    VAR __DS0FilterTable5 =\r\n        FILTER (\r\n            KEEPFILTERS ( VALUES ( 'Company Branch Helper'[Company Code] ) ),\r\n            'Company Branch Helper'[Company Code] = \"{companyCode}\"\r\n        )\r\n    VAR __DS0FilterTable6 =\r\n        FILTER (\r\n            KEEPFILTERS ( VALUES ( 'Shipment'[Job Number] ) ),\r\n            ISONORAFTER ( 'Shipment'[Job Number], \"{shipmentNo}\", ASC )\r\n        )\r\n    VAR __DS0FilterTable7 =\r\n        FILTER (\r\n            KEEPFILTERS ( VALUES ( 'Company Country'[Country Code] ) ),\r\n            'Company Country'[Country Code] = \"{countryCode}\"\r\n        )\r\nEVALUATE\r\nTOPN (\r\n    500,\r\n    SUMMARIZECOLUMNS (\r\n        'Shipment'[Job Number],\r\n        'Shipment'[Transport Mode],\r\n        'Shipment'[Container Mode],\r\n        'Port Of Origin'[Port Of Origin Code],\r\n        'Port Of Origin'[Port Of Origin Country Code],\r\n        'Port Of Destination'[Port Of Destination Code],\r\n        'Port Of Destination'[Port Of Destination Country Code],\r\n        'Shipment'[Way Bill Number],\r\n        'Shipment'[Shipment Inco Term],\r\n        'Shipment'[Additional Terms],\r\n        'Shipment'[Shipment PPD CCX Term],\r\n        'Shipment'[Goods Description],\r\n        'Shipment'[Weight Unit],\r\n        'Shipment'[Volume Unit],\r\n        'Shipment'[Total No Of Packs Package Type],\r\n        'Shipment'[Outer Packs Package Type],\r\n        'Shipment'[Shipment Create Date],\r\n        'Import Broker'[Import Broker],\r\n        'Export Broker'[Export Broker],\r\n        'Shipment'[Shipment Status],\r\n        'Consolidation'[Job No.],\r\n        'Consolidation'[Master Bill No.],\r\n        'Service Level'[Service Level],\r\n        'Shipment'[Interim Receipt Number],\r\n        'Shipment'[Booking Confirmation Reference],\r\n        'Shipment'[House Bill Type],\r\n        'Shipment'[Is Transport Job],\r\n        'Shipment'[Estimated Pickup],\r\n        'Shipment'[Pickup Required By],\r\n        'Shipment'[Delivery Cartage Completed],\r\n        'Shipment'[Delivery Required By],\r\n        'Shipment'[FCL Delivery Equipment Needed],\r\n        'Shipment'[Estimated Delivery],\r\n        'Shipment'[Pickup Cartage Completed],\r\n        'Shipment'[FCL Pickup Equipment Needed],\r\n        'Shipment'[Pickup Cartage Advised],\r\n        'Shipment'[Chargeable Unit],\r\n        'Shipment'[Is Master / Lead],\r\n        'Shipment'[Is Brokerage],\r\n        'Shipment'[Coload Master],\r\n        'Shipment'[Container 20F],\r\n        'Shipment'[Container 20H],\r\n        'Shipment'[Container 20R],\r\n        'Shipment'[Container 40F],\r\n        'Shipment'[Container 40H],\r\n        'Shipment'[Container 40R],\r\n        'Shipment'[Container 45F],\r\n        'Shipment'[Container GEN],\r\n        'Shipment'[Container Other],\r\n        'Discharge Port'[Discharge Port Code],\r\n        'Load Port'[Load Port Code],\r\n        'Shipment'[Is Active],\r\n        'Consolidation'[Coload],\r\n        'Consignor Address'[Consignor City],\r\n        'Consignor Address'[Consignor State],\r\n        'Consignor Address'[Consignor Post Code],\r\n        'Consignee Address'[Consignee City],\r\n        'Consignee Address'[Consignee State],\r\n        'Consignee Address'[Consignee Post Code],\r\n        'Shipment'[Received Date],\r\n        'Shipment'[Delivery Cartage Advised],\r\n        'Shipment'[Loading Meters],\r\n        'Consignor Address'[Consignor],\r\n        'Consignee Address'[Consignee],\r\n        'Controlling Customer Address'[Controlling Customer],\r\n        'Controlling Agent Address'[Controlling Agent],\r\n        'Shipment'[Arrival Date],\r\n        'Shipment'[Departure Date],\r\n        'Sending Agent Address'[Sending Agent],\r\n        'Receiving Agent Address'[Receiving Agent],\r\n        'Carrier Address'[Carrier],\r\n        'Shipment'[Container TEU],\r\n        'Shipment'[Container Count],\r\n        __DS0FilterTable,\r\n        __DS0FilterTable2,\r\n        __DS0FilterTable3,\r\n        __DS0FilterTable4,\r\n        __DS0FilterTable5,\r\n        __DS0FilterTable6,\r\n        __DS0FilterTable7,\r\n        \"Shipment_Weight\", 'Shipment'[Shipment Weight],\r\n        \"Shipment_Volume\", 'Shipment'[Shipment Volume],\r\n        \"Actual_Chargeable\", 'Shipment'[Actual Chargeable],\r\n        \"Total_No__Of_Packs\", 'Shipment'[Total No. Of Packs],\r\n        \"Shipment_Outer_Packs\", 'Shipment'[Shipment Outer Packs],\r\n        \"First Vessel\", 'Consolidation Transport'[First Vessel],\r\n        \"First Voyage Flight\", 'Consolidation Transport'[First Voyage Flight],\r\n        \"ETA Discharge\", 'Consolidation'[ETA Discharge],\r\n        \"ETD Load\", 'Consolidation'[ETD Load],\r\n        \"ETA Last Discharge\", 'Consolidation'[ETA Last Discharge],\r\n        \"ETD First Load\", 'Consolidation'[ETD First Load],\r\n        \"First Load\", 'Consolidation'[First Load],\r\n        \"Last Discharge\", 'Consolidation'[Last Discharge],\r\n        \"Console ATA\", 'Consolidation'[Console ATA],\r\n        \"Console ATD\", 'Consolidation'[Console ATD],\r\n        \"Shipment Customs Info\", 'Shipment'[Shipment Customs Info],\r\n        \"Local Client AR Settlement Group\", 'Local Agent AR Settlement'[Local Client AR Settlement Group],\r\n        \"Overseas Agent AR Settlement Group\", 'Overseas Agent AR Settlement'[Overseas Agent AR Settlement Group],\r\n        \"Contract Number\", 'Consolidation'[Contract Number],\r\n        \"Freight Direction\", 'Main Consol Shipment'[Freight Direction],\r\n        \"GL Branch\", 'Shipment'[GL Branch],\r\n        \"GL Department\", 'Shipment'[GL Department],\r\n        \"GL Local Agent\", 'Shipment'[GL Local Agent],\r\n        \"GL Representative Operator\", 'Shipment'[GL Representative Operator],\r\n        \"GL Overseas Agent Code\", 'Shipment'[GL Overseas Agent Code],\r\n        \"GL Overseas Agent Name\", 'Shipment'[GL Overseas Agent Name],\r\n        \"GL Representative Sales\", 'Shipment'[GL Representative Sales],\r\n        \"Sum_of_Revenue\", 'GL Transaction'[Sum of Revenue],\r\n        \"Sum_of_Cost\", 'GL Transaction'[Sum of Cost],\r\n        \"Sum_of_Job_Profit\", 'GL Transaction'[Sum of Job Profit],\r\n        \"Sum_of_Accrual\", 'GL Transaction'[Sum of Accrual],\r\n        \"Sum_of_Unrecognised_Cost\", 'GL Transaction'[Sum of Unrecognised Cost],\r\n        \"Sum_of_Unrecognised_Revenue\", 'GL Transaction'[Sum of Unrecognised Revenue],\r\n        \"Sum_Total_of_Cost\", 'GL Transaction'[Sum Total of Cost],\r\n        \"Sum_of_Recognised_Expense\", 'GL Transaction'[Sum of Recognised Expense],\r\n        \"Sum_of_Unrecognised_Income\", 'GL Transaction'[Sum of Unrecognised Income],\r\n        \"Sum_Total_of_Income\", 'GL Transaction'[Sum Total of Income],\r\n        \"Sum_Total_of_Expense\", 'GL Transaction'[Sum Total of Expense],\r\n        \"Sum_Total_of_Accrual\", 'GL Transaction'[Sum Total of Accrual],\r\n        \"Sum_of_Recognised_Income\", 'GL Transaction'[Sum of Recognised Income],\r\n        \"Sum_of_Unrecognised_Expense\", 'GL Transaction'[Sum of Unrecognised Expense],\r\n        \"Sum_Total_of_WIP\", 'GL Transaction'[Sum Total of WIP],\r\n        \"GL Status\", 'Shipment'[GL Status],\r\n        \"GL Create Date\", 'Shipment'[GL Create Date],\r\n        \"Sum_Total_of_Revenue\", 'GL Transaction'[Sum Total of Revenue],\r\n        \"GL Revenue Recognition Date\", 'Shipment'[GL Revenue Recognition Date],\r\n        \"Sum_of_Unrecognised_WIP\", 'GL Transaction'[Sum of Unrecognised WIP],\r\n        \"Sum_of_WIP\", 'GL Transaction'[Sum of WIP],\r\n        \"Sum_of_Unrecognised_Accrual\", 'GL Transaction'[Sum of Unrecognised Accrual]\r\n    ),\r\n    'Shipment'[Job Number], 1,\r\n    'Shipment'[Transport Mode], 1,\r\n    'Shipment'[Container Mode], 1,\r\n    'Port Of Origin'[Port Of Origin Code], 1,\r\n    'Port Of Origin'[Port Of Origin Country Code], 1,\r\n    'Port Of Destination'[Port Of Destination Code], 1,\r\n    'Port Of Destination'[Port Of Destination Country Code], 1,\r\n    'Shipment'[Way Bill Number], 1,\r\n    'Shipment'[Shipment Inco Term], 1,\r\n    'Shipment'[Additional Terms], 1,\r\n    'Shipment'[Shipment PPD CCX Term], 1,\r\n    'Shipment'[Goods Description], 1,\r\n    'Shipment'[Weight Unit], 1,\r\n    'Shipment'[Volume Unit], 1,\r\n    'Shipment'[Total No Of Packs Package Type], 1,\r\n    'Shipment'[Outer Packs Package Type], 1,\r\n    'Shipment'[Shipment Create Date], 1,\r\n    'Import Broker'[Import Broker], 1,\r\n    'Export Broker'[Export Broker], 1,\r\n    'Shipment'[Shipment Status], 1,\r\n    'Consolidation'[Job No.], 1,\r\n    'Consolidation'[Master Bill No.], 1,\r\n    'Service Level'[Service Level], 1,\r\n    'Shipment'[Interim Receipt Number], 1,\r\n    'Shipment'[Booking Confirmation Reference], 1,\r\n    'Shipment'[House Bill Type], 1,\r\n    'Shipment'[Is Transport Job], 1,\r\n    'Shipment'[Estimated Pickup], 1,\r\n    'Shipment'[Pickup Required By], 1,\r\n    'Shipment'[Delivery Cartage Completed], 1,\r\n    'Shipment'[Delivery Required By], 1,\r\n    'Shipment'[FCL Delivery Equipment Needed], 1,\r\n    'Shipment'[Estimated Delivery], 1,\r\n    'Shipment'[Pickup Cartage Completed], 1,\r\n    'Shipment'[FCL Pickup Equipment Needed], 1,\r\n    'Shipment'[Pickup Cartage Advised], 1,\r\n    'Shipment'[Chargeable Unit], 1,\r\n    'Shipment'[Is Master / Lead], 1,\r\n    'Shipment'[Is Brokerage], 1,\r\n    'Shipment'[Coload Master], 1,\r\n    'Shipment'[Container 20F], 1,\r\n    'Shipment'[Container 20H], 1,\r\n    'Shipment'[Container 20R], 1,\r\n    'Shipment'[Container 40F], 1,\r\n    'Shipment'[Container 40H], 1,\r\n    'Shipment'[Container 40R], 1,\r\n    'Shipment'[Container 45F], 1,\r\n    'Shipment'[Container GEN], 1,\r\n    'Shipment'[Container Other], 1,\r\n    'Discharge Port'[Discharge Port Code], 1,\r\n    'Load Port'[Load Port Code], 1,\r\n    'Shipment'[Is Active], 1,\r\n    'Consolidation'[Coload], 1,\r\n    'Consignor Address'[Consignor City], 1,\r\n    'Consignor Address'[Consignor State], 1,\r\n    'Consignor Address'[Consignor Post Code], 1,\r\n    'Consignee Address'[Consignee City], 1,\r\n    'Consignee Address'[Consignee State], 1,\r\n    'Consignee Address'[Consignee Post Code], 1,\r\n    'Shipment'[Received Date], 1,\r\n    'Shipment'[Delivery Cartage Advised], 1,\r\n    'Shipment'[Loading Meters], 1,\r\n    'Consignor Address'[Consignor], 1,\r\n    'Consignee Address'[Consignee], 1,\r\n    'Controlling Customer Address'[Controlling Customer], 1,\r\n    'Controlling Agent Address'[Controlling Agent], 1,\r\n    'Shipment'[Arrival Date], 1,\r\n    'Shipment'[Departure Date], 1,\r\n    'Sending Agent Address'[Sending Agent], 1,\r\n    'Receiving Agent Address'[Receiving Agent], 1,\r\n    'Carrier Address'[Carrier], 1,\r\n    'Shipment'[Container TEU], 1,\r\n    'Shipment'[Container Count], 1\r\n)\r\nORDER BY\r\n    'Shipment'[Job Number],\r\n    'Shipment'[Transport Mode],\r\n    'Shipment'[Container Mode],\r\n    'Port Of Origin'[Port Of Origin Code],\r\n    'Port Of Origin'[Port Of Origin Country Code],\r\n    'Port Of Destination'[Port Of Destination Code],\r\n    'Port Of Destination'[Port Of Destination Country Code],\r\n    'Shipment'[Way Bill Number],\r\n    'Shipment'[Shipment Inco Term],\r\n    'Shipment'[Additional Terms],\r\n    'Shipment'[Shipment PPD CCX Term],\r\n    'Shipment'[Goods Description],\r\n    'Shipment'[Weight Unit],\r\n    'Shipment'[Volume Unit],\r\n    'Shipment'[Total No Of Packs Package Type],\r\n    'Shipment'[Outer Packs Package Type],\r\n    'Shipment'[Shipment Create Date],\r\n    'Import Broker'[Import Broker],\r\n    'Export Broker'[Export Broker],\r\n    'Shipment'[Shipment Status],\r\n    'Consolidation'[Job No.],\r\n    'Consolidation'[Master Bill No.],\r\n    'Service Level'[Service Level],\r\n    'Shipment'[Interim Receipt Number],\r\n    'Shipment'[Booking Confirmation Reference],\r\n    'Shipment'[House Bill Type],\r\n    'Shipment'[Is Transport Job],\r\n    'Shipment'[Estimated Pickup],\r\n    'Shipment'[Pickup Required By],\r\n    'Shipment'[Delivery Cartage Completed],\r\n    'Shipment'[Delivery Required By],\r\n    'Shipment'[FCL Delivery Equipment Needed],\r\n    'Shipment'[Estimated Delivery],\r\n    'Shipment'[Pickup Cartage Completed],\r\n    'Shipment'[FCL Pickup Equipment Needed],\r\n    'Shipment'[Pickup Cartage Advised],\r\n    'Shipment'[Chargeable Unit],\r\n    'Shipment'[Is Master / Lead],\r\n    'Shipment'[Is Brokerage],\r\n    'Shipment'[Coload Master],\r\n    'Shipment'[Container 20F],\r\n    'Shipment'[Container 20H],\r\n    'Shipment'[Container 20R],\r\n    'Shipment'[Container 40F],\r\n    'Shipment'[Container 40H],\r\n    'Shipment'[Container 40R],\r\n    'Shipment'[Container 45F],\r\n    'Shipment'[Container GEN],\r\n    'Shipment'[Container Other],\r\n    'Discharge Port'[Discharge Port Code],\r\n    'Load Port'[Load Port Code],\r\n    'Shipment'[Is Active],\r\n    'Consolidation'[Coload],\r\n    'Consignor Address'[Consignor City],\r\n    'Consignor Address'[Consignor State],\r\n    'Consignor Address'[Consignor Post Code],\r\n    'Consignee Address'[Consignee City],\r\n    'Consignee Address'[Consignee State],\r\n    'Consignee Address'[Consignee Post Code],\r\n    'Shipment'[Received Date],\r\n    'Shipment'[Delivery Cartage Advised],\r\n    'Shipment'[Loading Meters],\r\n    'Consignor Address'[Consignor],\r\n    'Consignee Address'[Consignee],\r\n    'Controlling Customer Address'[Controlling Customer],\r\n    'Controlling Agent Address'[Controlling Agent],\r\n    'Shipment'[Arrival Date],\r\n    'Shipment'[Departure Date],\r\n    'Sending Agent Address'[Sending Agent],\r\n    'Receiving Agent Address'[Receiving Agent],\r\n    'Carrier Address'[Carrier],\r\n    'Shipment'[Container TEU],\r\n    'Shipment'[Container Count]"; // Tabular model name
			using (Db.DisposableActionForDbConnection())
			using (var server = SsasServer.New(AnalysisServer))
			{
				return server.GetDataTableFromQuery(ModelHelper.LogisticsModel, dax);
			}
		}

		public DataTable GetTrialBalanceReportData(string companyCode)
		{
			VerifyAPIIsEnabled();
			var dax = $@"DEFINE  MEASURE 'Report Measures'[AccountDescription] =     (/* USER DAX BEGIN */VAR des =  SWITCH(TRUE(), [IsLevelThreeInHierarchy], VALUES(GLAccount[Description]), [IsLevelTwoInHierarchy],var acc = FIRSTNONBLANK(GLAccount[Level 2],true()) return CALCULATE(FIRSTNONBLANK(GLAccount[Description],true()),GLAccount[Account No.]=acc),  var acc = FIRSTNONBLANK(GLAccount[Level 1],true()) return CALCULATE(FIRSTNONBLANK(GLAccount[Description],true()),GLAccount[Account No.]=acc))return SWITCH (TRUE(),[IsGrandTotal],BLANK(),[CreditAmount]>0,des,[CreditAmount]=0 && VALUES('Include Zero Balance'[Include Zero Balance])=""Yes"",des)/* USER DAX END */)  MEASURE 'Report Measures'[IsLevelThreeInHierarchy] =     (/* USER DAX BEGIN */ISFILTERED(GLAccount[Level 1]) && ISFILTERED(GLAccount[Level 2]) && ISFILTERED(GLAccount[Level 3])/* USER DAX END */)  MEASURE 'Report Measures'[IsLevelTwoInHierarchy] =     (/* USER DAX BEGIN */ISFILTERED(GLAccount[Level 1]) && ISFILTERED(GLAccount[Level 2])/* USER DAX END */)  MEASURE 'Report Measures'[IsGrandTotal] =     (/* USER DAX BEGIN */COUNTROWS(VALUES(GLAccount[Level 1]))>1/* USER DAX END */)  MEASURE 'Report Measures'[CreditAmount] =     (/* USER DAX BEGIN */var amount =IF (NOT ISFILTERED('Presentation Journal'[Code]), [Sum Of Credit],CALCULATE([Sum Of Credit],KEEPFILTERS(FILTER(     ALL (GLTransaction[Presentation Journal Status],GLTransaction[Transaction Category],GLTransaction[GLAmount Credit]),    [Presentation Journal Status] =2 && SEARCH([Transaction Category],CONCATENATEX(VALUES('Presentation Journal'[Code]),'Presentation Journal'[Code],"",""),,0)>0 || [Presentation Journal Status] =1    ))))return IF( amount = 0 , if (VALUES('Include Zero Balance'[Include Zero Balance])=""Yes"",0),amount)/* USER DAX END */)  MEASURE 'Report Measures'[AccountDescriptionTranslated] =     (/* USER DAX BEGIN */var translationKey = SWITCH(TRUE(), [IsLevelThreeInHierarchy], VALUES(GLAccount[Translation Key]), [IsLevelTwoInHierarchy],var acc = FIRSTNONBLANK(GLAccount[Level 2],true()) return CALCULATE(FIRSTNONBLANK(GLAccount[Translation Key],true()),GLAccount[Account No.]=acc),  var acc = FIRSTNONBLANK(GLAccount[Level 1],true()) return CALCULATE(FIRSTNONBLANK(GLAccount[Translation Key],true()),GLAccount[Account No.]=acc))VAR translation = CALCULATE(FIRSTNONBLANK('Translation Lookup'[Translation Text],TRUE()),'Translation Lookup'[Translation Key]=translationKey)return SWITCH (TRUE(),[IsGrandTotal],BLANK(),[CreditAmount]>0,translation,[CreditAmount]=0 && VALUES('Include Zero Balance'[Include Zero Balance])=""Yes"",translation)/* USER DAX END */)  MEASURE 'Report Measures'[AccountType] =     (/* USER DAX BEGIN */var Acctype = SWITCH(TRUE(), [IsLevelThreeInHierarchy], VALUES(GLAccount[Account Type]), [IsLevelTwoInHierarchy],var acc = FIRSTNONBLANK(GLAccount[Level 2],true()) return CALCULATE(FIRSTNONBLANK(GLAccount[Account Type],true()),GLAccount[Account No.]=acc),  var acc = FIRSTNONBLANK(GLAccount[Level 1],true()) return CALCULATE(FIRSTNONBLANK(GLAccount[Account Type],true()),GLAccount[Account No.]=acc))return SWITCH (TRUE(),[IsGrandTotal], BLANK(),[CreditAmount]>0,Acctype,[CreditAmount]=0 && VALUES('Include Zero Balance'[Include Zero Balance])=""Yes"",Acctype)/* USER DAX END */)  MEASURE 'Report Measures'[BudgetPeriodRangeCurrentCredit] =     (/* USER DAX BEGIN */[Budget Period Range Current Credit] /* USER DAX END */)  MEASURE 'Report Measures'[BudgetPeriodRangeCurrentDebit] =     (/* USER DAX BEGIN */[Budget Period Range Current Debit]/* USER DAX END */)  MEASURE 'Report Measures'[BudgetClosingCredit] =     (/* USER DAX BEGIN */[Budget Closing Credit]/* USER DAX END */)  MEASURE 'Report Measures'[BudgetClosingDebit] =     (/* USER DAX BEGIN */[Budget Closing Debit]/* USER DAX END */)  VAR __DS0FilterTable =     FILTER(      KEEPFILTERS(VALUES('GLAccount'[Level 1])),      NOT(ISBLANK('GLAccount'[Level 1]))    )  VAR __DS0FilterTable2 =     FILTER(KEEPFILTERS(VALUES('Period'[Period])), 'Period'[Period] = 201703)  VAR __DS0FilterTable3 =     FILTER(KEEPFILTERS(VALUES('Language'[Description])), 'Language'[Description] = ""Dutch"")  VAR __DS0FilterTable4 =     FILTER(      KEEPFILTERS(VALUES('Company Branch Helper'[Company Code])),      'Company Branch Helper'[Company Code] = ""{companyCode}""    )  VAR __DS0FilterTable5 =     FILTER(      KEEPFILTERS(VALUES('Company Branch'[Company Code])),      'Company Branch'[Company Code] = ""{companyCode}""    )  VAR __DS0FilterTable6 =     FILTER(      KEEPFILTERS(VALUES('Include Zero Balance'[Include Zero Balance])),      'Include Zero Balance'[Include Zero Balance] = ""Yes""    )EVALUATE  TOPN(    502,    FILTER(      KEEPFILTERS(        SUMMARIZECOLUMNS(          ROLLUPADDISSUBTOTAL(            'GLAccount'[Level 1], ""IsGrandTotalRowTotal"",            'GLAccount'[Level 2], ""IsDM1Total"",            'GLAccount'[Level 3], ""IsDM3Total""          ),          __DS0FilterTable,          __DS0FilterTable2,          __DS0FilterTable3,          __DS0FilterTable4,          __DS0FilterTable5,          __DS0FilterTable6,          ""AccountDescription"", 'Report Measures'[AccountDescription],          ""AccountDescriptionTranslated"", 'Report Measures'[AccountDescriptionTranslated],          ""AccountType"", 'Report Measures'[AccountType],          ""CreditAmount"", 'Report Measures'[CreditAmount],          ""BudgetPeriodRangeCurrentCredit"", 'Report Measures'[BudgetPeriodRangeCurrentCredit],          ""BudgetPeriodRangeCurrentDebit"", 'Report Measures'[BudgetPeriodRangeCurrentDebit],          ""BudgetClosingCredit"", 'Report Measures'[BudgetClosingCredit],          ""BudgetClosingDebit"", 'Report Measures'[BudgetClosingDebit]        )      ),      OR(        OR(          OR([IsGrandTotalRowTotal], AND(NOT([IsGrandTotalRowTotal]), [IsDM1Total])),          AND(            AND(NOT([IsDM1Total]), [IsDM3Total]),            OR(              OR(                OR('GLAccount'[Level 1] = ""2050.00.00"", 'GLAccount'[Level 1] = ""3430.00.00""),                'GLAccount'[Level 1] = ""3530.00.00""              ),              'GLAccount'[Level 1] = ""3550.00.00""            )          )        ),        AND(          AND(            NOT([IsDM3Total]),            OR(              OR(                OR('GLAccount'[Level 1] = ""2050.00.00"", 'GLAccount'[Level 1] = ""3430.00.00""),                'GLAccount'[Level 1] = ""3530.00.00""              ),              'GLAccount'[Level 1] = ""3550.00.00""            )          ),          AND('GLAccount'[Level 1] = ""2050.00.00"", 'GLAccount'[Level 2] = ""2050.10.00"")        )      )    ),    [IsGrandTotalRowTotal],    0,    'GLAccount'[Level 1],    1,    [IsDM1Total],    0,    'GLAccount'[Level 2],    1,    [IsDM3Total],    0,    'GLAccount'[Level 3],    1  )ORDER BY  [IsGrandTotalRowTotal] DESC,  'GLAccount'[Level 1],  [IsDM1Total] DESC,  'GLAccount'[Level 2],  [IsDM3Total] DESC,  'GLAccount'[Level 3]"; // Tabular model name
			using (Db.DisposableActionForDbConnection())
			using (var server = SsasServer.New(AnalysisServer))
			{
				return server.GetDataTableFromQuery(ModelHelper.FinanceModel, dax);
			}
		}

		public DataTable GetTrialBalanceReportFilteredData(string companyCode)
		{
			VerifyAPIIsEnabled();
			var dax = $@"
			DEFINE
  MEASURE 'Report Measures'[Budget Period Debit] = 
    (/* USER DAX BEGIN */
GLBudget[Budget Period Range Current Debit]
/* USER DAX END */)

  MEASURE 'Report Measures'[Budget Period Credit] = 
    (/* USER DAX BEGIN */
[Budget Period Range Current Credit]
/* USER DAX END */)

  MEASURE 'Report Measures'[Actual Closing Credit] = 
    (/* USER DAX BEGIN */

        VAR mn =    MAX ( 'Period Management'[Current Year Begin Period] )
        VAR mx =    MAX ( 'Period'[Period] )
        RETURN  
SWITCH (
        TRUE (),
        NOT ISFILTERED ( 'Presentation Journal'[Code] ),
        CALCULATE (
                    [Current Credit],
                    FILTER (
                        ALL (
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        IF( [__IsCFXAccountForCurrentCompany]= 0, GLTransaction[Mapped  GLAccount Key]<>-1, 1) ),
                    FILTER(ALL(Period),
                              Period[Period]>= mn && 
                              Period[Period] <= mx)
                     ),
        IF( [__IsCFXAccountForCurrentCompany]= 0,
        CALCULATE (
                    [Current Credit],
                    FILTER ( ALL ( Period[Period] ), Period[Period] >= mn && Period[Period] <= mx ),
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type],
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        GLTransaction[Mapped  GLAccount Key]<>-1 &&
                        (GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    )
                    ),
            CALCULATE (
                    [Current Credit],
                    FILTER ( ALL ( Period[Period] ), Period[Period] >= mn && Period[Period] <= mx ),
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type],
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        (GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    ))
                ))
/* USER DAX END */)

  MEASURE 'Report Measures'[__IsCFXAccountForCurrentCompany] = 
    (/* USER DAX BEGIN */

    COUNTROWS (
        FILTER (
            'ControllingAccount ReportSubCode Mapping',
            'ControllingAccount ReportSubCode Mapping'[Report SubCode] = ""*JC*JNL*CFX*""
                && 'ControllingAccount ReportSubCode Mapping'[Controlling Account Key] = [__Acckey]
                && (  ISBLANK('ControllingAccount ReportSubCode Mapping'[Company Key]) ||  'ControllingAccount ReportSubCode Mapping'[Company Key]
                    = FIRSTNONBLANK ( VALUES ( 'Company Branch'[Company Key] ), 1 ))
        )

    )

/* USER DAX END */)

  MEASURE 'Report Measures'[__Acckey] = 
    (/* USER DAX BEGIN */

    SWITCH (
        TRUE (),
        ISFILTERED ( GLAccount[Level 1] ) && ISFILTERED ( GLAccount[Level 2] )
            && ISFILTERED ( GLAccount[Level 3] ), VALUES ( GLAccount[Account Key] ),
        ISFILTERED ( GLAccount[Level 1] ) && ISFILTERED ( GLAccount[Level 2] ),
            CALCULATE (
                FIRSTNONBLANK ( GLAccount[Account Key], TRUE () ),
                FILTER ( GLAccount, GLAccount[Account No.] = GLAccount[Level 2] )
            ),
        CALCULATE (
            FIRSTNONBLANK ( GLAccount[Account Key], TRUE () ),
            FILTER ( GLAccount, GLAccount[Account No.] = GLAccount[Level 1] )
        )
    )

/* USER DAX END */)

  MEASURE 'Report Measures'[Actual Period Debit] = 
    (/* USER DAX BEGIN */
SWITCH (
        TRUE (),
        NOT ISFILTERED ( 'Presentation Journal'[Code] ), IF( [__IsCFXAccountForCurrentCompany]= 0, CALCULATE([Current Debit], GLTransaction[Mapped  GLAccount Key]<>-1), [Current Debit]),
            IF( [__IsCFXAccountForCurrentCompany]= 0,
                 CALCULATE (
                    [Current Debit],
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type],
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                         GLTransaction[Mapped  GLAccount Key]<>-1 &&
                        (
                        GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    )
                ),
                CALCULATE (
                    [Current Debit],
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type],
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        (
                        GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    )
))  )
/* USER DAX END */)

  MEASURE 'Report Measures'[Actual Period Credit] = 
    (/* USER DAX BEGIN */
SWITCH (
        TRUE (),
        NOT ISFILTERED ( 'Presentation Journal'[Code] ), IF( [__IsCFXAccountForCurrentCompany]= 0, CALCULATE([Current Credit], GLTransaction[Mapped  GLAccount Key]<>-1), [Current Credit]),
             IF( [__IsCFXAccountForCurrentCompany]= 0,
                    CALCULATE (
                    [Current Credit],
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type] ,
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                      GLTransaction[Mapped  GLAccount Key]<>-1 &&
                        (GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    )),
                    CALCULATE (
                    [Current Credit],
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type] ,
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        (GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    )))
    )
/* USER DAX END */)

  MEASURE 'Report Measures'[Actual Closing Debit] = 
    (/* USER DAX BEGIN */

        VAR mn =    MAX ( 'Period Management'[Current Year Begin Period] )
        VAR mx =    MAX ( 'Period'[Period] )
RETURN  
        SWITCH (
        TRUE (),
        NOT ISFILTERED ( 'Presentation Journal'[Code] ), 
        CALCULATE (
        [Current Debit],FILTER (
                        ALL (
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        IF( [__IsCFXAccountForCurrentCompany]= 0, GLTransaction[Mapped  GLAccount Key]<>-1, 1) ),
                        FILTER(ALL(Period), Period[Period]>= mn && Period[Period] <= mx)
                       ),
        IF( [__IsCFXAccountForCurrentCompany]= 0,
         CALCULATE (
                    [Current Debit],
                    FILTER ( ALL ( Period[Period] ), Period[Period] >= mn && Period[Period] <= mx ),
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type],
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        GLTransaction[Mapped  GLAccount Key]<>-1 &&
                        (GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    )),
        CALCULATE (
                    [Current Debit],
                    FILTER ( ALL ( Period[Period] ), Period[Period] >= mn && Period[Period] <= mx ),
                    FILTER (
                        ALL (
                            GLTransaction[Transaction Category],
                            GLTransaction[Transaction Type],
                            GLTransaction[Mapped  GLAccount Key]
                        ),
                        (GLTransaction[Transaction Category] <> """" &&
                         (  GLTransaction[Transaction Type]=""AJL""|| 
                            GLTransaction[Transaction Type]=""RJL""||
                            GLTransaction[Transaction Type]=""GJL"" )
                        && SEARCH ( GLTransaction[Transaction Category], CONCATENATEX (VALUES ( 'Presentation Journal'[Code] ), 'Presentation Journal'[Code], "","" ),,0) > 0
                        || 
                        (GLTransaction[Transaction Type]<>""AJL""&& 
                        GLTransaction[Transaction Type]<>""RJL""&& 
                        GLTransaction[Transaction Type]<>""GJL""))
                    ))
            )
    )
/* USER DAX END */)

  MEASURE 'Report Measures'[Budget Closing Debit ] = 
    (/* USER DAX BEGIN */
GLBudget[Budget Closing Debit]
/* USER DAX END */)

  MEASURE 'Report Measures'[Budget Closing Credit ] = 
    (/* USER DAX BEGIN */
GLBudget[Budget Closing Credit]
/* USER DAX END */)

  MEASURE 'Report Measures'[Name] = 
    (/* USER DAX BEGIN */
//Account name
var translationKey = 
SWITCH(
TRUE(),
 ISFILTERED(GLAccount[Level 1]) && ISFILTERED(GLAccount[Level 2]) && ISFILTERED(GLAccount[Level 3]), VALUES(GLAccount[Translation Key]),
 ISFILTERED(GLAccount[Level 1]) && ISFILTERED(GLAccount[Level 2]),CALCULATE(FIRSTNONBLANK(GLAccount[Translation Key],TRUE()),FILTER(GLAccount,GLAccount[Account No.]=GLAccount[Level 2])),
 CALCULATE(FIRSTNONBLANK(GLAccount[Translation Key],TRUE()),FILTER(GLAccount,GLAccount[Account No.]=GLAccount[Level 1]))
)
VAR translation = CALCULATE(
FIRSTNONBLANK('Translation Lookup'[Translation Text],TRUE()),
FILTER('Translation Lookup','Translation Lookup'[Translation Key]=translationKey))
return 
SWITCH (
TRUE(),
COUNTROWS(VALUES(GLAccount[Level 1]))>1 , BLANK(),
[Actual Closing Credit]+[Actual Closing Debit]+[Actual Period Credit]+[Actual Period Debit]+[Budget Closing Credit]+[Budget Closing Debit]+[Budget Period Credit]+[Budget Period Debit]<>0,translation,
OR([Closing Credit]=0,[Closing Debit]=0) && VALUES('Include Zero Balance'[Include Zero Balance])=""Yes"",translation)
/* USER DAX END */)

  MEASURE 'Report Measures'[Type ] = 
    (/* USER DAX BEGIN */
//Account Type 
var __accType = SWITCH(
TRUE(),
 ISFILTERED(GLAccount[Level 1]) && ISFILTERED(GLAccount[Level 2]) && ISFILTERED(GLAccount[Level 3]), VALUES(GLAccount[Account Type]),
 ISFILTERED(GLAccount[Level 1]) && ISFILTERED(GLAccount[Level 2]),CALCULATE(FIRSTNONBLANK(GLAccount[Account Type],TRUE()),FILTER(GLAccount,GLAccount[Account No.]=GLAccount[Level 2])),
 CALCULATE(FIRSTNONBLANK(GLAccount[Account Type],TRUE()),FILTER(GLAccount,GLAccount[Account No.]=GLAccount[Level 1]))
)
return
SWITCH (
TRUE(),
COUNTROWS(VALUES(GLAccount[Level 1]))>1 , BLANK(),
[Actual Closing Credit]+[Actual Closing Debit]+[Actual Period Credit]+[Actual Period Debit]+[Budget Closing Credit]+[Budget Closing Debit]+[Budget Period Credit]+[Budget Period Debit]<>0,__accType,
OR([Closing Credit]=0,[Closing Debit]=0) && VALUES('Include Zero Balance'[Include Zero Balance])=""Yes"",__accType)
/* USER DAX END */)

  VAR __DS1FilterTable = 
    FILTER(
      KEEPFILTERS(VALUES('GLAccount'[Level 1])),
      NOT(ISBLANK('GLAccount'[Level 1]))
    )

  VAR __DS1FilterTable2 = 
    FILTER(KEEPFILTERS(VALUES('Language'[Description])), 'Language'[Description] = ""English"")

  VAR __DS1FilterTable3 = 
    FILTER(
      KEEPFILTERS(VALUES('Include Zero Balance'[Include Zero Balance])),
      'Include Zero Balance'[Include Zero Balance] = ""No""
    )

  VAR __DS1FilterTable4 = 
    FILTER(
      KEEPFILTERS(VALUES('Company Branch'[Company])),
      'Company Branch'[Company] = ""[{companyCode}] US Demo Company""
    )

EVALUATE
  TOPN(
    502,
    SUMMARIZECOLUMNS(
      ROLLUPADDISSUBTOTAL(
        'GLAccount'[Level 1], ""IsGrandTotalRowTotal"",
        'GLAccount'[Level 2], ""IsDM1Total"",
        'GLAccount'[Level 3], ""IsDM3Total""
      ),
      __DS1FilterTable,
      __DS1FilterTable2,
      __DS1FilterTable3,
      __DS1FilterTable4,
      ""Budget Period Debit"", 'Report Measures'[Budget Period Debit],
      ""Budget Period Credit"", 'Report Measures'[Budget Period Credit],
      ""Actual Closing Credit"", 'Report Measures'[Actual Closing Credit],
      ""Actual Period Debit"", 'Report Measures'[Actual Period Debit],
      ""Actual Period Credit"", 'Report Measures'[Actual Period Credit],
      ""Actual Closing Debit"", 'Report Measures'[Actual Closing Debit],
      ""Budget Closing Debit "", 'Report Measures'[Budget Closing Debit ],
      ""Budget Closing Credit "", 'Report Measures'[Budget Closing Credit ],
      ""Name"", 'Report Measures'[Name],
      ""Type "", 'Report Measures'[Type ]
    ),
    [IsGrandTotalRowTotal],
    0,
    'GLAccount'[Level 1],
    1,
    [IsDM1Total],
    0,
    'GLAccount'[Level 2],
    1,
    [IsDM3Total],
    0,
    'GLAccount'[Level 3],
    1
  )

ORDER BY
  [IsGrandTotalRowTotal] DESC,
  'GLAccount'[Level 1],
  [IsDM1Total] DESC,
  'GLAccount'[Level 2],
  [IsDM3Total] DESC,
  'GLAccount'[Level 3]"; // DAX Query

			using (Db.DisposableActionForDbConnection())
			using (var server = SsasServer.New(AnalysisServer))
			{
				var ssasDatabaseName = ModelHelper.FinanceModel;
#if DEBUG
				ssasDatabaseName = $"{Db.DatabaseName}_FinanceModel_FilteredWebServiceTest"; // Database Name
#endif
				return server.GetDataTableFromQuery(ssasDatabaseName, dax);
			}
		}
	}

	public class AnalyticsModelHelper
	{
		public AnalyticsModelHelper()
		{
		}
		public virtual string LogisticsModel => logisticsModel ?? (logisticsModel = $"{Db.DatabaseName}_LogisticsModel"); // Tabular model name
		string logisticsModel;

		public virtual string FinanceModel => financeModel ?? (financeModel = $"{Db.DatabaseName}_FinanceModel"); // Tabular model name
		string financeModel;
	}
}
